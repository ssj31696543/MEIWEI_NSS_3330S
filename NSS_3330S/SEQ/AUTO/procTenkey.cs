using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NSS_3330S.MOTION;

namespace NSS_3330S.SEQ.AUTO
{
    class procTenkey : SeqBase
    {
        const int TEN_OFFSET = 4;
        int m_Number = 0;

        //INPUT TEN_KEY_1, // 1, 2, 3
        //INPUT TEN_KEY_2, // 4, 5, 6
        //INPUT TEN_KEY_3, // 7, 8, 9
        //INPUT TEN_KEY_4, // Clr,0,Set
        //INPUT TEN_KEY_5, // 1, 4, 7, Clr
        //INPUT TEN_KEY_6, // 2, 5, 8, 0
        //INPUT TEN_KEY_7, // 3, 6, 9, SET

        //TEN_KEY_1, // (일단위 1)
        //TEN_KEY_2, // (일단위 2)
        //TEN_KEY_3, // (일단위 4)
        //TEN_KEY_4, // (일단위 8)
        //TEN_KEY_5, // (십단위 1)
        //TEN_KEY_6, // (십단위 2)
        //TEN_KEY_7, // (십단위 4)
        //TEN_KEY_8, // (십단위 8)
        //TEN_KEY_9, // (백단위 1)
        
        //조합 정정
        //0	1,2,3,4
        //1	2,3,4
        //2	1,3,4
        //3	3,4
        //4	1,2,4
        //5	2,4
        //6	1,4
        //7	4
        //8	1,2,3
        //9	2,3

       
        void TenKeySetOutput(int nNumber)
        {
            ClearTenKey();

            int nHundred = nNumber / 100;
            int nTen = (nNumber / 10) % 10;
            int nOne = nNumber % 10;

            if (nHundred == 1)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_9, true);

            }
            else
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_9, false);
            }

            switch (nTen)
            {
                case 0:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4 + TEN_OFFSET, true);
                    }
                    break;
                case 1:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4 + TEN_OFFSET, true);
                    }
                    break;
                case 2:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4 + TEN_OFFSET, true);
                    }
                    break;
                case 3:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4 + TEN_OFFSET, true);
                    }
                    break;
                case 4:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4 + TEN_OFFSET, true);
                    }
                    break;
                case 5:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4 + TEN_OFFSET, true);
                    }
                    break;
                case 6:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4 + TEN_OFFSET, true);
                    }
                    break;
                case 7:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4 + TEN_OFFSET, true);
                    }
                    break;
                case 8:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3 + TEN_OFFSET, true);
                    }
                    break;
                case 9:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2 + TEN_OFFSET, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3 + TEN_OFFSET, true);
                    }
                    break;
                default:
                    break;
            }
            switch (nOne)
            {
                case 0:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4, true);
                    }
                    break;
                case 1:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4, true);
                    }
                    break;
                case 2:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4, true);
                    }
                    break;
                case 3:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4, true);
                    }
                    break;
                case 4:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4, true);
                    }
                    break;
                case 5:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4, true);
                    }
                    break;
                case 6:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4, true);
                    }
                    break;
                case 7:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_4, true);
                    }
                    break;
                case 8:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3, true);
                    }
                    break;
                case 9:
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_2, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_3, true);
                    }
                    break;
                default:
                    break;
            }
           

        }
        void ClearOneUnit()
        {
            for (int nIdx = 0; nIdx < 4; nIdx++)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1 + nIdx, false);
            }
        }
        void ClearTenUnit()
        {
            for (int nIdx = 0; nIdx < 4; nIdx++)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_5 + nIdx, false);
            }
        }
        void ClearHundredUnit()
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_9, false);
        }

        void ClearTenKey()
        {
            for (int nIdx = 0; nIdx < 9; nIdx++)
            {
                if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.TEN_KEY_1 + nIdx] == 1)
                {
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TEN_KEY_1 + nIdx, false);
                }
            }
        }
        public procTenkey(int nSeqID)
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

            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_4] == 1 &&
                GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
            {
                InitSeq();
                ClearTenKey();
            }
            
            switch (m_nSeqNo)
            {
                case 0:
                    {

                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_1] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = 1;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = 2;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = 3;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_2] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = 4;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = 5;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = 6;
                                TenKeySetOutput(m_Number);

                                break;
                            }
                        }
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_3] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = 7;
                                TenKeySetOutput(m_Number);

                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = 8;
                                TenKeySetOutput(m_Number);

                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = 9;
                                TenKeySetOutput(m_Number);

                                break;
                            }
                        }
                    }
                    return;
                case 1:
                    {
                        for (int nIdx = 0; nIdx < 7; nIdx++)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_1 + nIdx] == 1) return;
                        }
                    }
                    break;
                case 2:
                    {
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_1] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = m_Number * 10 + 1;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = m_Number * 10 + 2;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = m_Number * 10 + 3;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_2] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = m_Number * 10 + 4;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = m_Number * 10 + 5;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = m_Number * 10 + 6;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_3] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = m_Number * 10 + 7;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = m_Number * 10 + 8;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = m_Number * 10 + 9;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_4] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = m_Number * 10;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                    }
                    return;
                case 3:
                    {
                        for (int nIdx = 0; nIdx < 7; nIdx++)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_1 + nIdx] == 1) return;
                        }
                    }
                    break;
                case 4:
                    {
                        if (m_Number >= 20) return;
                        //텐키에 1보다 큰 값이 들어가면 안된다.
                       
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_1] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = m_Number * 10 + 1;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = m_Number * 10 + 2;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = m_Number * 10 + 3;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_2] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = m_Number * 10 + 4;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = m_Number * 10 + 5;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = m_Number * 10 + 6;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_3] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_5] == 1)
                            {
                                m_Number = m_Number * 10 + 7;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = m_Number * 10 + 8;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_7] == 1)
                            {
                                m_Number = m_Number * 10 + 9;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_4] == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_6] == 1)
                            {
                                m_Number = m_Number * 10;
                                TenKeySetOutput(m_Number);
                                break;
                            }
                        }
                    }
                    return;
                case 5:
                    {
                        for (int nIdx = 0; nIdx < 7; nIdx++)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.TEN_KEY_1 + nIdx] == 1) return;
                        }
                    }
                    break;
                default:
                    {
                        NextSeq(0);
                        return;
                    }
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
