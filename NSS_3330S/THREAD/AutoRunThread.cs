using NSS_3330S;
using NSS_3330S.SEQ.AUTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSS_3330S.THREAD
{
    public class AutoRunThread : IDisposable
    {
        public enum MODE_THREAD_TYPE
        {
            OHT_INTF = 0,

            //LOADER LOAD, UNLOAD CONV 추가
            LD_MGZ_CONVEYOR,
            ULD_MGZ_CONVEYOR,

            LD_MGZ_TRANSFER,

            STRIP_HANDLER_STRIP,
            STRIP_HANDLER_UNIT,
            STRIP_HANDLER_DRY,
            MAP_TRANSFER,

            SORTER_PNP_1,
            SORTER_PNP_2,

            ULD_TRAY_TRANSFER,

            MONITORING,

            CIM,
            MAX
        }

        public const int MODE_THREAD_CNT = (int)MODE_THREAD_TYPE.MAX;
        public const int SEQ_ID_CNT = (int)SEQ_ID.MAX;

        Thread[] threadArray = new Thread[MODE_THREAD_CNT];
        bool[] flagThreadAliveArray = new bool[MODE_THREAD_CNT];
        SeqBase[] seqArray = new SeqBase[SEQ_ID_CNT];

        public delegate void manCfgStopEvent(int nErrSafty);
        public event manCfgStopEvent errSaftyStopEvent;

        public delegate void procAddMsgEvent(int nProcNo, int nSeqNo, int nSeqSubNo, string strAddMsg, bool bLogView);
        public event procAddMsgEvent procMsgEvent;

        public SeqBase this[SEQ_ID index]
        {
            get
            {
                return seqArray[(int)index];
            }
            set
            {
                seqArray[(int)index] = value;
            }
        }

        public AutoRunThread()
        {
            for (int nCnt = 0; nCnt < MODE_THREAD_CNT; nCnt++)
            {
                threadArray[nCnt] = null;
                flagThreadAliveArray[nCnt] = false;

                if ((MODE_THREAD_TYPE)nCnt < MODE_THREAD_TYPE.MONITORING)
                {
                    StartStopAutoRunThread((MODE_THREAD_TYPE)nCnt, true, ThreadPriority.AboveNormal);
                }
                else
                {
                    StartStopAutoRunThread((MODE_THREAD_TYPE)nCnt, true, ThreadPriority.Normal);
                }
            }

            for (int nCnt = 0; nCnt < SEQ_ID_CNT; nCnt++)
            {
                seqArray[nCnt] = null;
            }

            //기존 코드
            this[SEQ_ID.LD_MZ_ELV_TRANSFER] = new procLoader_MzTransfer((int)SEQ_ID.LD_MZ_ELV_TRANSFER);
            //this[SEQ_ID.LD_MZ_LOT_START] = new procLdMzLotStart((int)SEQ_ID.LD_MZ_LOT_START);

            //신규 추가
            this[SEQ_ID.LD_MZ_LD_CONV] = new procLoader_LdConv((int)SEQ_ID.LD_MZ_LD_CONV);
            this[SEQ_ID.LD_MZ_ULD_CONV] = new procLoader_UldConv((int)SEQ_ID.LD_MZ_ULD_CONV);
            //this[SEQ_ID.LD_MZ_ELV_TRANSFER] = new procLoader_MzTransfer((int)SEQ_ID.LD_MZ_ELV_TRANSFER);
            this[SEQ_ID.STRIP_TRANSFER] = new procSawStrpTransfer((int)SEQ_ID.STRIP_TRANSFER);
            this[SEQ_ID.UNIT_TRANSFER] = new procSawUnitTransfer((int)SEQ_ID.UNIT_TRANSFER);

            this[SEQ_ID.DRY_UNIT] = new procUnitDry((int)SEQ_ID.DRY_UNIT);

            this[SEQ_ID.MAP_TRANSFER] = new procStMapTransfer((int)SEQ_ID.MAP_TRANSFER);
            this[SEQ_ID.MAP_VISION_TABLE_1] = new procStMapVisionTable((int)SEQ_ID.MAP_VISION_TABLE_1);
            this[SEQ_ID.MAP_VISION_TABLE_2] = new procStMapVisionTable((int)SEQ_ID.MAP_VISION_TABLE_2);

            this[SEQ_ID.PICK_N_PLACE_1] = new procStUnitPnP1((int)SEQ_ID.PICK_N_PLACE_1);
            this[SEQ_ID.PICK_N_PLACE_2] = new procStUnitPnP2((int)SEQ_ID.PICK_N_PLACE_2);

            this[SEQ_ID.UNLOAD_GD1_TRAY_TABLE] = new procStUldGDTrayTable1((int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE);
            this[SEQ_ID.UNLOAD_GD2_TRAY_TABLE] = new procStUldGDTrayTable2((int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE);
            this[SEQ_ID.UNLOAD_RW_TRAY_TABLE] = new procStUldRWTrayTable((int)SEQ_ID.UNLOAD_RW_TRAY_TABLE);

            this[SEQ_ID.TRAY_TRANSFER] = new procStUldTrayTransfer((int)SEQ_ID.TRAY_TRANSFER);

            this[SEQ_ID.ULD_ELV_GOOD_1] = new procStUldElvGood((int)SEQ_ID.ULD_ELV_GOOD_1);
            this[SEQ_ID.ULD_ELV_GOOD_2] = new procStUldElvGood((int)SEQ_ID.ULD_ELV_GOOD_2);
            this[SEQ_ID.ULD_ELV_REWORK] = new procStUldElvRework((int)SEQ_ID.ULD_ELV_REWORK);
            this[SEQ_ID.ULD_ELV_EMPTY_1] = new procStUldElvEmpty((int)SEQ_ID.ULD_ELV_EMPTY_1);
            this[SEQ_ID.ULD_ELV_EMPTY_2] = new procStUldElvEmpty((int)SEQ_ID.ULD_ELV_EMPTY_2);

            this[SEQ_ID.ALWAYS_WORK] = new procAlwaysWork((int)SEQ_ID.ALWAYS_WORK);
            this[SEQ_ID.AUTO_LOT_END] = new procAutoLotEnd((int)SEQ_ID.AUTO_LOT_END);
            this[SEQ_ID.TEN_KEY_MON] = new procTenkey((int)SEQ_ID.TEN_KEY_MON);
            this[SEQ_ID.VISION_MOVE] = new procVisionMove((int)SEQ_ID.VISION_MOVE);

            foreach (SeqBase item in seqArray)
            {
                if (item != null)
                {
                    item.SetErrorFunc(SetError);
                    item.SetAddMsgFunc(SetMsgEvent);
                    item.SetCycleMode(false);
                }
            }
        }

        private void SetError(int nErrNo)
        {
            if (errSaftyStopEvent != null)
                errSaftyStopEvent(nErrNo);
        }

        private void SetMsgEvent(int nProcNo, int nSeqNo, int nSeqSubNo, string strAddMsg, bool bLogView)
        {
            if (procMsgEvent != null)
                procMsgEvent(nProcNo, nSeqNo, nSeqSubNo, strAddMsg, bLogView);
        }

        public void ResetCmd()
        {
            for (int nCnt = 0; nCnt < SEQ_ID_CNT; nCnt++)
            {
                if (seqArray[nCnt] != null) seqArray[nCnt].ResetCmd();
            }
        }

        public void AllInitSeq()
        {
            for (int nCnt = 0; nCnt < SEQ_ID_CNT; nCnt++)
            {
                if (seqArray[nCnt] != null) seqArray[nCnt].InitSeq();
            }
        }

        public void SelectInitSeq(int nSeqID)
        {
            if (seqArray[nSeqID] != null) seqArray[nSeqID].InitSeq();
        }

        public void Dispose()
        {
            for (int nCnt = 0; nCnt < MODE_THREAD_CNT; nCnt++)
            {
                StartStopAutoRunThread((MODE_THREAD_TYPE)nCnt, false);
            }
        }

        void StartStopAutoRunThread(MODE_THREAD_TYPE mode, bool bStart, ThreadPriority priority = ThreadPriority.Normal)
        {
            if (mode >= MODE_THREAD_TYPE.MAX) return;

            if (threadArray[(int)mode] != null)
            {
                flagThreadAliveArray[(int)mode] = false;
                threadArray[(int)mode].Join(2000);
                threadArray[(int)mode].Abort();
                threadArray[(int)mode] = null;
            }

            if (bStart)
            {
                flagThreadAliveArray[(int)mode] = true;
                threadArray[(int)mode] = new Thread(new ParameterizedThreadStart(ThreadRun));
                threadArray[(int)mode].Name = string.Format("Auto process thread {0}", mode.ToString());
                threadArray[(int)mode].Priority = priority;
                if (threadArray[(int)mode].IsAlive == false)
                    threadArray[(int)mode].Start(mode);
            }
        }

        void ThreadRun(object obj)
        {
            MODE_THREAD_TYPE mode = (MODE_THREAD_TYPE)obj;

            switch (mode)
            {
                case MODE_THREAD_TYPE.LD_MGZ_CONVEYOR:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(20);

                            ////사용시 해제하고 사용하면 됨
                            #region LOAD CONV
                            if (seqArray[(int)SEQ_ID.LD_MZ_LD_CONV] != null)
                                seqArray[(int)SEQ_ID.LD_MZ_LD_CONV].Run();
                            #endregion
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.ULD_MGZ_CONVEYOR:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(20);

                            ////사용시 해제하고 사용하면 됨
                            #region UNLOAD CONV
                            if (seqArray[(int)SEQ_ID.LD_MZ_ULD_CONV] != null)
                                seqArray[(int)SEQ_ID.LD_MZ_ULD_CONV].Run();
                            #endregion
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.LD_MGZ_TRANSFER:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(20);

                            if (seqArray[(int)SEQ_ID.LD_MZ_ELV_TRANSFER] != null)
                                seqArray[(int)SEQ_ID.LD_MZ_ELV_TRANSFER].Run();
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.STRIP_HANDLER_STRIP:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(10);
                            //트랜스퍼 사용 시 해제해서 쓰면 됨
                            if (seqArray[(int)SEQ_ID.STRIP_TRANSFER] != null)
                                seqArray[(int)SEQ_ID.STRIP_TRANSFER].Run();
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.STRIP_HANDLER_UNIT:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(10);

                            if (seqArray[(int)SEQ_ID.UNIT_TRANSFER] != null)
                                seqArray[(int)SEQ_ID.UNIT_TRANSFER].Run();
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.STRIP_HANDLER_DRY:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(10);
  
                            if (seqArray[(int)SEQ_ID.DRY_UNIT] != null)
                                seqArray[(int)SEQ_ID.DRY_UNIT].Run();
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.MAP_TRANSFER:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(10);

                            if (seqArray[(int)SEQ_ID.MAP_TRANSFER] != null)
                                seqArray[(int)SEQ_ID.MAP_TRANSFER].Run();

                            if (seqArray[(int)SEQ_ID.MAP_VISION_TABLE_1] != null)
                                seqArray[(int)SEQ_ID.MAP_VISION_TABLE_1].Run();

                            if (seqArray[(int)SEQ_ID.MAP_VISION_TABLE_2] != null)
                                seqArray[(int)SEQ_ID.MAP_VISION_TABLE_2].Run();
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.SORTER_PNP_1:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(1);

                            if (seqArray[(int)SEQ_ID.PICK_N_PLACE_1] != null)
                                seqArray[(int)SEQ_ID.PICK_N_PLACE_1].Run();

                            if (seqArray[(int)SEQ_ID.PICK_N_PLACE_2] != null)
                                seqArray[(int)SEQ_ID.PICK_N_PLACE_2].Run();
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.SORTER_PNP_2:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(5);

                            //if (seqArray[(int)SEQ_ID.PICK_N_PLACE_2] != null)
                            //    seqArray[(int)SEQ_ID.PICK_N_PLACE_2].Run();
                        }
                    }
                    break;

                case MODE_THREAD_TYPE.ULD_TRAY_TRANSFER:
                    {
                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(20);

                            if (seqArray[(int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE] != null)
                                seqArray[(int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE].Run();

                            if (seqArray[(int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE] != null)
                                seqArray[(int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE].Run();

                            if (seqArray[(int)SEQ_ID.UNLOAD_RW_TRAY_TABLE] != null)
                                seqArray[(int)SEQ_ID.UNLOAD_RW_TRAY_TABLE].Run();


                            if (seqArray[(int)SEQ_ID.TRAY_TRANSFER] != null)
                                seqArray[(int)SEQ_ID.TRAY_TRANSFER].Run();

                            if (seqArray[(int)SEQ_ID.ULD_ELV_GOOD_1] != null)
                                seqArray[(int)SEQ_ID.ULD_ELV_GOOD_1].Run();

                            if (seqArray[(int)SEQ_ID.ULD_ELV_GOOD_2] != null)
                                seqArray[(int)SEQ_ID.ULD_ELV_GOOD_2].Run();

                            if (seqArray[(int)SEQ_ID.ULD_ELV_REWORK] != null)
                                seqArray[(int)SEQ_ID.ULD_ELV_REWORK].Run();

                            if (seqArray[(int)SEQ_ID.ULD_ELV_EMPTY_1] != null)
                                seqArray[(int)SEQ_ID.ULD_ELV_EMPTY_1].Run();

                            if (seqArray[(int)SEQ_ID.ULD_ELV_EMPTY_2] != null)
                                seqArray[(int)SEQ_ID.ULD_ELV_EMPTY_2].Run();

                        }
                    }
                    break;

                case MODE_THREAD_TYPE.MONITORING:
                    {
                        // Event 연결 후 알람을 받게끔
                        Thread.Sleep(3000);

                        while (flagThreadAliveArray[(int)mode])
                        {
                            Thread.Sleep(100);

                            if (seqArray[(int)SEQ_ID.ALWAYS_WORK] != null)
                                seqArray[(int)SEQ_ID.ALWAYS_WORK].Run();

                            if (seqArray[(int)SEQ_ID.AUTO_LOT_END] != null)
                                seqArray[(int)SEQ_ID.AUTO_LOT_END].Run();

                            if (seqArray[(int)SEQ_ID.TEN_KEY_MON] != null)
                                seqArray[(int)SEQ_ID.TEN_KEY_MON].Run();

                            if (seqArray[(int)SEQ_ID.VISION_MOVE] != null)
                                seqArray[(int)SEQ_ID.VISION_MOVE].Run();
                        }
                    }
                    break;
            }
        }
    }
}
