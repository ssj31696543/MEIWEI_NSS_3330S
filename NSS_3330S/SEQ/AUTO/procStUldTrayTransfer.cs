using NSS_3330S.MOTION;
using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStUldTrayTransfer : SeqBase
    {
        cycTrayTransfer cycFunc = null;

        Stopwatch m_swMoveReadyPos = new Stopwatch();
        bool m_bMoveReadyPos = false;

        public procStUldTrayTransfer(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sUldTrayTransfer;

            cycFunc = new cycTrayTransfer(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }
        void NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER seqNo)
        {
            NextSeq((int)seqNo);
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            cycFunc.InitSeq();

            m_swMoveReadyPos.Reset();
            m_bMoveReadyPos = false;
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
                if (m_seqInfo != GbVar.Seq.sUldTrayTransfer)
                {
                    m_seqInfo = GbVar.Seq.sUldTrayTransfer;
                    m_nSeqNo = GbVar.Seq.sUldTrayTransfer.nCurrentSeqNo;
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

            switch ((seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER)m_nSeqNo)
            {
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.INIT:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("-----------", "Soter Tray Picker", "Start !!");

                        m_bFirstSeqStep = false;

                        m_swMoveReadyPos.Restart();
                        m_bMoveReadyPos = false;
                    }
                    break;

                // Z축 준비 위치 이동
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.READY_POS_MOVE_Z_AXIS:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", "Tray Picker Z Axis", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Ready Move", "Tray Picker Z Axis", "Done");
                    }
                    break;

                // Y축 준비 위치 이동
                // 2022 08 25 HEP
                // 대기위치는 Y먼저 보내는게 안전
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.READY_POS_MOVE_Y_AXIS:
                    // [2022.05.19.kmlee] 바로 안가고 대기하기 위해서
                    if (!m_bMoveReadyPos)
                        break;

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", "Tray Picker Y Axis", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosTrayPkY(POSDF.TRAY_PICKER_READY);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Ready Move", "Tray Picker Y Axis", "Done");
                    }
                    break;
                // X축 준비 위치 이동
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.READY_POS_MOVE_X_AXIS:
                    // [2022.05.19.kmlee] 바로 안가고 대기하기 위해서
                    if (!m_bMoveReadyPos)
                        break;

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", "Tray Picker X Axis", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosTrayPkX(POSDF.TRAY_PICKER_READY);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Ready Move", "Tray Picker X Axis", "Done");

                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = false;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.START:
                    {
                        // 일정 시간 후에 요청이 없으면 READY 위치로 가게끔
                        if (m_swMoveReadyPos.IsRunning)
                        {
                            if (m_swMoveReadyPos.Elapsed.TotalSeconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_MOVE_READY_WAIT_TIME].dValue)
                            {
                                m_swMoveReadyPos.Stop();
                                m_bMoveReadyPos = true;

                                NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.READY_POS_MOVE_Y_AXIS);
                                return;
                            }
                        }
                    }
                    break;

                // Good 1 트레이 테이블과 트레이 엘레베이터 로딩 요청 확인
                // 엘레베이터에 있는 트레이를 픽업 후 트레이 테이블 1에 안착
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_TABLE_1_LOAD_REQ:

                    if (GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        SeqHistory("Tray Loading Ready Req", "Good Tray Table 1", "On");

                        // Empty Elv 준비되어 있는 
                        if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_IN] == 1 &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 0;
                            GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD1_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 1 &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 0;
                            GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD1_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() >= MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 0;
                            GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD1_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() < MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 0;
                            GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD1_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 0;
                            GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD1_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 0;
                            GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD1_TABLE);
                            return;
                        }
                    }

                    break;

                // Good 2 트레이 테이블과 트레이 엘레베이터 로딩 요청 확인
                // 엘레베이터에 있는 트레이를 픽업 후 트레이 테이블 2에 안착
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_TABLE_2_LOAD_REQ:
                    if (GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        SeqHistory("Tray Loading Ready Req", "Good Tray Table 2", "On");
                        // Empty Elv 준비되어 있는 
                        if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 1 &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 1;
                            GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD2_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 1 &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 1;
                            GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD2_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() >= MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 1;
                            GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD2_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() < MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 1;
                            GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD2_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 1;
                            GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD2_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 1;
                            GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD2_TABLE);
                            return;
                        }
                        //20211130 choh : cycle stop 추가하기
                    }

                    break;

                // REWORK 트레이 테이블과 REWORK 엘레베이터 요청 확인
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_TABLE_3_LOAD_REQ:
                    if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_LOADING_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        SeqHistory("Tray Loading Ready Req", "Rework Tray Table", "On");
                        if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 2;
                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_RW_TABLE);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadTableNo = 2;
                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_LOADING_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_RW_TABLE);
                            return;
                        }
                    }
                    break;

                // Good 1 트레이 테이블 언로딩 요청 확인
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_1_UNLOAD_REQ:
                    if (GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                            GbVar.Seq.sUldTrayElvGood[0].nTrayInCount == GbVar.Seq.sUldTrayElvGood[1].nTrayInCount &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 1, Good Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                GbVar.Seq.sUldTrayElvGood[0].nTrayInCount == GbVar.Seq.sUldTrayElvGood[1].nTrayInCount &&
                                GbVar.GB_INPUT[(int)IODF.INPUT.GD2_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 1, Good Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                GbVar.Seq.sUldTrayElvGood[0].nTrayInCount >= GbVar.Seq.sUldTrayElvGood[1].nTrayInCount)
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 1, Good Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                 GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                 GbVar.Seq.sUldTrayElvGood[0].nTrayInCount < GbVar.Seq.sUldTrayElvGood[1].nTrayInCount)
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 1, Good Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY])
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 1, Good Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY])
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 1, Good Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                        }
                        else
                        {
                            // 내려놓을데가 없다
                            break;
                        }

                        GbVar.Seq.sUldTrayElvGood[GbVar.Seq.sUldTrayTransfer.nUnloadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PLACE_PROCESS] = true;
                        GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_READY] = false;

                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_TO_TABLE_GD_1);
                        return;

                    }
                    break;

                // Good 2 트레이 테이블 언로딩 요청 확인
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_2_UNLOAD_REQ:
                    if (GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                            GbVar.Seq.sUldTrayElvGood[0].nTrayInCount == GbVar.Seq.sUldTrayElvGood[1].nTrayInCount &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 2, Good Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                GbVar.Seq.sUldTrayElvGood[0].nTrayInCount == GbVar.Seq.sUldTrayElvGood[1].nTrayInCount &&
                                GbVar.GB_INPUT[(int)IODF.INPUT.GD2_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 2, Good Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                            GbVar.Seq.sUldTrayElvGood[0].nTrayInCount >= GbVar.Seq.sUldTrayElvGood[1].nTrayInCount)
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 2, Good Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                 GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] &&
                                 GbVar.Seq.sUldTrayElvGood[0].nTrayInCount < GbVar.Seq.sUldTrayElvGood[1].nTrayInCount)
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 2, Good Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY])
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 2, Good Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                        }
                        else if (GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY])
                        {
                            SeqHistory("Tray Unloading Ready Req", "Good Tray Table 2, Good Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                        }
                        else
                        {
                            // 내려놓을데가 없다
                            break;
                        }

                        GbVar.Seq.sUldTrayElvGood[GbVar.Seq.sUldTrayTransfer.nUnloadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PLACE_PROCESS] = true;
                        GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_READY] = false;

                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_TO_TABLE_GD_2);
                        return;

                    }
                    break;

                // Rework 트레이 테이블 언로딩 요청 확인
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_3_UNLOAD_REQ:
                    if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNLOADING_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        if (GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY])
                        {
                            SeqHistory("Tray Unloading Ready Req", "Rework Tray Table", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadTableNo = 2;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 2;

                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_TRANSFER_PLACE_PROCESS] = true;
                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNLOADING_READY] = false;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_TO_TABLE_RW);
                            return;
                        }
                    }

                    break;

                #region EMPTY TRAY COVER

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_1_COVER_LOAD_REQ:

                    if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        SeqHistory("Tray Cover Ready Req", "Good Elevator 1", "On");

                        // Empty Elv 준비되어 있는 
                        if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 1 &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                            GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD1_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 1 &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                            GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD1_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() >= MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                            GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD1_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() < MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                            GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD1_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                            GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD1_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 0;
                            GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD1_ELV_COVER);
                            return;
                        }
                    }

                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_2_COVER_LOAD_REQ:

                    if (GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        SeqHistory("Tray Cover Ready Req", "Good Elevator 2", "On");

                        // Empty Elv 준비되어 있는 
                        if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 1 &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD2_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 1 &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD2_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() >= MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD2_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() < MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD2_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD2_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 1;
                            GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD2_ELV_COVER);
                            return;
                        }
                    }

                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.IS_TRAY_3_COVER_LOAD_REQ:

                    if (GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_READY] == true) //20211022 choh: sUldGDTrayTable[0] 또는 sUldGDTrayTable[0] 처리방법확인필요!!
                    {
                        SeqHistory("Tray Cover Ready Req", "Rework Elevator", "On");

                        // Empty Elv 준비되어 있는 
                        if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 1 &&
                            GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 2;
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_REWORK_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY2_CONV_TRAY_CHECK_OUT] == 1 &&
                                 GbVar.GB_INPUT[(int)IODF.INPUT.EMPTY1_CONV_TRAY_CHECK_OUT] == 0)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 2;
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_REWORK_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                            MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() >= MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 2;
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_REWORK_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true &&
                                 MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos() < MotionMgr.Inst[SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos())
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 2;
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_REWORK_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 1", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 0;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 2;
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_REWORK_ELV_COVER);
                            return;
                        }
                        else if (GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] == true)
                        {
                            SeqHistory("Tray Ready", "Empty Tray Elevator 2", "On");

                            GbVar.Seq.sUldTrayTransfer.nLoadElvNo = 1;
                            GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = 2;
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_READY] = false;
                            GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                            GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = true;

                            NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_REWORK_ELV_COVER);
                            return;
                        }
                    }

                    NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.START);
                    return;


                #endregion

                #region Table Tray Load Cycle
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD1_TABLE:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PICK_UP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Loading For Good Table 1", "Soter Tray Picker", "Start");
                    }

                    //인터락 추가 2022.07.25 HEP
                    if (GbVar.Seq.sUldTrayTransfer.nLoadElvNo == 0)
                    {
                        if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] == true)
                        {
                            return;
                        }
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = true;
                    }
                    else
                    {
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = false;
                    }

                    nFuncResult = cycFunc.TrayLoadingFromElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Tray Loading For Good Table 1", "Soter Tray Picker", "Done");

                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].nTrayCount--;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_OUT_COMP] = true;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = false;
                    }
                    break;
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_UNLOAD_TO_TABLE_GD_1:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PLACE_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Unloading To Good Table 1", "Soter Tray Picker", "Start");
                    }

                    //인터락 추가 2022.07.25 HEP
                    if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] == true)
                    {
                        return;
                    }
                    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = true;

                    if (GbVar.Seq.sUldTrayTransfer.bTrayPopErrSelectNewTray)
                    {
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sUldTrayTransfer.nUnloadTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = true;
                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.INIT);
                        return;
                    }
                    nFuncResult = cycFunc.TrayUnloadingToTable();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Tray Unloading To Good Table 1", "Soter Tray Picker", "Done");
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sUldTrayTransfer.nUnloadTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_COMPLETE] = true;

                        GbVar.Seq.sUldGDTrayTable[0].bTrayExist = true;
                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                        return;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_GD2_TABLE:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PICK_UP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Loading For Good Table 2", "Soter Tray Picker", "Start");
                    }
                    //인터락 추가 2022.07.25 HEP
                    if (GbVar.Seq.sUldTrayTransfer.nLoadElvNo == 0)
                    {
                        if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] == true)
                        {
                            return;
                        }
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = true;
                    }
                    else
                    {
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = false;
                    }
                    nFuncResult = cycFunc.TrayLoadingFromElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].nTrayCount--;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_OUT_COMP] = true;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = false;

                        SeqHistory("Tray Loading For Good Table 2", "Soter Tray Picker", "Done");
                    }
                    break;
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_UNLOAD_TO_TABLE_GD_2:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PLACE_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Unloading To Good Table 2", "Soter Tray Picker", "Start");
                    }
                    //인터락 추가 2022.07.25 HEP
                    if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] == true)
                    {
                        return;
                    }
                    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = true;

                    if (GbVar.Seq.sUldTrayTransfer.bTrayPopErrSelectNewTray)
                    {
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sUldTrayTransfer.nUnloadTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = true;
                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.INIT);
                        return;
                    }
                    nFuncResult = cycFunc.TrayUnloadingToTable();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Tray Unloading To Good Table 2", "Soter Tray Picker", "Done");

                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sUldTrayTransfer.nUnloadTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_COMPLETE] = true;
                        
                        GbVar.Seq.sUldGDTrayTable[1].bTrayExist = true;

                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                        return;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_EMPTY_TRAY_LOAD_FOR_RW_TABLE:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PICK_UP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Loading For Rework Table", "Soter Tray Picker", "Start");
                    }

                    nFuncResult = cycFunc.TrayLoadingFromElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].nTrayCount--;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_OUT_COMP] = true;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = false;

                        SeqHistory("Tray Loading For Rework Table", "Soter Tray Picker", "Done");
                    }
                    break;
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_UNLOAD_TO_TABLE_RW:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PLACE_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Unloading To Rework Table", "Soter Tray Picker", "Start");
                    }
                    nFuncResult = cycFunc.TrayUnloadingToTable();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Tray Unloading To Rework Table", "Soter Tray Picker", "Done");

                        GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_LOADING_COMPLETE] = true;

                        GbVar.Seq.sUldRWTrayTable.bTrayExist = true;

                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                        return;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD1_ELV_COVER:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PICK_UP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Cover Loading For Good Elevator 1", "Soter Tray Picker", "Start");
                    }

                    //인터락 추가 2022.07.25 HEP
                    if (GbVar.Seq.sUldTrayTransfer.nLoadElvNo == 0)
                    {
                        if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] == true)
                        {
                            return;
                        }
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = true;
                    }
                    else
                    {
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = false;
                    }
                    nFuncResult = cycFunc.TrayLoadingFromElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Tray Cover Loading For Good Elevator 1", "Soter Tray Picker", "Done");

                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].nTrayCount--;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_OUT_COMP] = true;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = false;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_UNLOAD_TO_GD1_ELV_COVER:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Cover Unloading To Elevator", "Soter Tray Picker", "Start");

                        m_bFirstSeqStep = false;
                    }

                    //인터락 추가 2022.07.25 HEP
                    if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] == true)
                    {
                        return;
                    }
                    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = true;

                    if (GbVar.Seq.sUldTrayTransfer.bTrayPopErrSelectNewTray)
                    {
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sUldTrayTransfer.nUnloadTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = true;
                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.INIT);
                        return;
                    }
                    nFuncResult = cycFunc.TrayUnloadingToElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_COMP] = true;

                        SeqHistory("Tray Cover Unloading To Elevator", "Soter Tray Picker", "Done");
                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                        return;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_GD2_ELV_COVER:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PICK_UP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Cover Loading For Good Elevator 2", "Soter Tray Picker", "Start");
                    }

                    //인터락 추가 2022.07.25 HEP
                    if (GbVar.Seq.sUldTrayTransfer.nLoadElvNo == 0)
                    {
                        if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] == true)
                        {
                            return;
                        }
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = true;
                    }
                    else
                    {
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = false;
                    }
                    nFuncResult = cycFunc.TrayLoadingFromElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Tray Cover Loading For Good Elevator 2", "Soter Tray Picker", "Done");

                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].nTrayCount--;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_OUT_COMP] = true;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = false;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_UNLOAD_TO_GD2_ELV_COVER:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Cover Unloading  To Elevator", "Soter Tray Picker", "Start");

                        m_bFirstSeqStep = false;
                    }
                    //인터락 추가 2022.07.25 HEP
                    if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] == true)
                    {
                        return;
                    }
                    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START] = true;

                    if (GbVar.Seq.sUldTrayTransfer.bTrayPopErrSelectNewTray)
                    {
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sUldTrayTransfer.nUnloadTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = true;
                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.INIT);
                        return;
                    }
                    nFuncResult = cycFunc.TrayUnloadingToElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.TRAY_COVER_COMP] = true;

                        SeqHistory("Tray Cover Unloading  To Elevator", "Soter Tray Picker", "Done");
                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                        return;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_FOR_REWORK_ELV_COVER:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PICK_UP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Tray Cover Loading For Rework Elevator", "Sorter Tray Picker", "Start");
                    }
                    nFuncResult = cycFunc.TrayLoadingFromElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Tray Cover Loading For Rework Elevator", "Soter Tray Picker", "Done");

                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].nTrayCount--;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvEmpty.TRAY_OUT_COMP] = true;
                        GbVar.Seq.sUldTrayElvEmpty[GbVar.Seq.sUldTrayTransfer.nLoadElvNo].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS] = false;
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_UNLOAD_TO_REWORK_ELV_COVER:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Cover Unloading To Elevator", "Soter Tray Picker", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.TrayUnloadingToElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_COMP] = true;

                        SeqHistory("Tray Cover Unloading To Elevator", "Soter Tray Picker", "Done");
                        NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                        return;
                    }
                    break;

                #endregion

                #region Table Tray Unload Cycle
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_TO_TABLE_GD_1:
                    break;
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_TO_TABLE_GD_2:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.TRAY_PICKER_PICK_UP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        SeqHistory(String.Format("Tray Load To Good Table {0}", GbVar.Seq.sUldTrayTransfer.nLoadTableNo), "Sorter Tray Picker", "Start");
                    }

                    nFuncResult = cycFunc.TrayLoadingFromTable();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sUldTrayTransfer.nLoadTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_COMPLETE] = true;

                        SeqHistory(String.Format("Tray Load To Good Table {0}", GbVar.Seq.sUldTrayTransfer.nLoadTableNo), "Sorter Tray Picker", "Done");
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.WAIT_GD_ELV_READY:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Tray In Ready", "Soter Tray Picker", "Wait");

                        m_bFirstSeqStep = false;
                    }


                    //220503 LOAD, UNLOAD 변수 잘못 연결되어있어서 수정
                    //if (GbVar.Seq.sUldTrayElvGood[cycFunc.LOAD_ELV_NO].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] == false) return;
                    //GbVar.Seq.sUldTrayElvGood[cycFunc.LOAD_ELV_NO].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] = false;


                    if (GbVar.Seq.sUldTrayElvGood[MCDF.ELV_TRAY_GOOD_1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] == false && 
                        GbVar.Seq.sUldTrayElvGood[MCDF.ELV_TRAY_GOOD_2].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] == false) return;

                    //220617
                    //if (GbVar.Seq.sUldTrayElvGood[MCDF.ELV_TRAY_GOOD_1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] == true)
                    //{
                    //    GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = MCDF.ELV_TRAY_GOOD_1;
                    //    GbVar.Seq.sUldTrayElvGood[MCDF.ELV_TRAY_GOOD_1].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] = false;
                    //}
                    //else
                    //{
                    //    GbVar.Seq.sUldTrayTransfer.nUnloadElvNo = MCDF.ELV_TRAY_GOOD_2;
                    //    GbVar.Seq.sUldTrayElvGood[MCDF.ELV_TRAY_GOOD_2].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] = false;
                    //}


                    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.TRAY_UNLOAD_RUN] = true;
                    SeqHistory("Check Tray In Ready", "Soter Tray Picker", "Complete");
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_UNLOAD_TO_GOOD_ELV:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Unloading To Elevator", "Soter Tray Picker", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.TrayUnloadingToElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //GbVar.Seq.sUldTrayElvGood.nTrayInCount++;

                        GbVar.Seq.sUldTrayElvGood[cycFunc.UNLOAD_ELV_NO].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_COMP] = true;
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sUldTrayTransfer.nLoadTableNo].bTrayExist = false;

                        SeqHistory("Tray Unloading To Elevator", "Soter Tray Picker", "Done");
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.TRAY_UNLOAD_GOOD_ELV_TRAY_IN_CHECKED:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Tray In Complete", "Good Tray Elevator", "Wait");

                        m_bFirstSeqStep = false;
                    }
                    
                    if (GbVar.Seq.sUldTrayElvGood[cycFunc.UNLOAD_ELV_NO].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_COMP] != false) return;

                    SeqHistory("Tray In Complete", "Soter Tray Picker", "Complete");
                    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.TRAY_UNLOAD_RUN] = false;
                    GbVar.Seq.sUldTrayElvGood[cycFunc.UNLOAD_ELV_NO].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PLACE_PROCESS] = false;

                    NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                    return;


                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_LOAD_TO_TABLE_RW:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Load From Rework Table", "Soter Tray Picker", "Start");

                        m_bFirstSeqStep = false;
                    }
                    nFuncResult = cycFunc.TrayLoadingFromTable();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNLOADING_COMPLETE] = true;
                        //NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                        //return;

                        SeqHistory("Tray Load From Rework Table", "Soter Tray Picker", "Done");
                    }
                    break;
                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.WAIT_RW_ELV_READY:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Tray In Ready", "Rework Tray Elevator", "Wait");

                        m_bFirstSeqStep = false;
                    }
                    if (!GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY]) return;

                    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY] = false;
                    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.TRAY_UNLOAD_RUN] = true;
                    SeqHistory("Check Tray In Ready", "Rework Tray Elevator", "Complete");
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.CYC_TRAY_UNLOAD_TO_REWORK_ELV:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Unload To Rework Elevator", "Soter Tray Picker", "Start");

                        m_bFirstSeqStep = false;
                    }
                    nFuncResult = cycFunc.TrayUnloadingToElv();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //GbVar.Seq.sUldTrayElvRework.nTrayInCount++;

                        GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_COMP] = true;
                    	GbVar.Seq.sUldRWTrayTable.bTrayExist = false;

                        SeqHistory("Tray Unload To Rework Elevator", "Soter Tray Picker", "Complete");
                    }
                    break;

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.TRAY_UNLOAD_REWORK_ELV_TRAY_IN_CHECKED:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Tray In Comp", "Rework Tray Elevator", "Wait");

                        m_bFirstSeqStep = false;
                    }

                    if (GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_COMP] != false) return;

                    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.TRAY_UNLOAD_RUN] = false;
                    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_TRANSFER_PLACE_PROCESS] = false;

                    SeqHistory("Tray In Comp", "Rework Tray Elevator", "Complete");

                    NextSeq(seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH);
                    return;
                #endregion

                case seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.FINISH:
                    //if (GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.LOT_END_TRAY] == true &&
                    //    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.GOOD_TRAY_OUT_COMPLETE] == true &&
                    //    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.REWORK_TRAY_OUT_COMPLETE] == true)
                    //{
                    //    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.LOT_END_TRAY] = false;
                    //    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.GOOD_TRAY_OUT_COMPLETE] = false;
                    //    GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.REWORK_TRAY_OUT_COMPLETE] = false;

                        
                    //}
                    SeqHistory("-----------", "Soter Tray Picker", "Finish !!");

                    NextSeq((int)seqUldTrayTransfer.SEQ_SOTER_ULD_TRAY_TRANSFER.INIT);
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
