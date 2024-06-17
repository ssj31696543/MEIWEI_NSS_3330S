using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnMultiAxisHome :SeqBase
    {
        int m_nIndex = 0;
        int[] m_nAxisNoArray;

        public int[] AXIS_NO_ARRAY
        {
            get { return m_nAxisNoArray; }
        }

        public void SetParam(int[] nAxisNoArray)
        {
            m_nAxisNoArray = nAxisNoArray;
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

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_nIndex = 0;
                    }
                    break;
                case 2:
                    {
                        if (m_nIndex >= m_nAxisNoArray.Length)
                        {
                            m_nIndex = 0;
                            NextSeq(6);
                            return;
                        }
                    }
                    break;
                case 3:
                    {
                        // Interlock Check
                        nFuncResult = SafetyMgr.Inst.GetAxisSafetyBeforeHome(m_nAxisNoArray[m_nIndex]);
                        if (FNC.IsErr(nFuncResult))
                        {
                            SetError(nFuncResult);
                            return;
                        }
                    }
                    break;
                case 4:
                    {
                        GbVar.mcState.isHomeComplete[m_nAxisNoArray[m_nIndex]] = false;
                    }
                    break;
                case 5:
                    {
                        MotionMgr.Inst[m_nAxisNoArray[m_nIndex]].HomeStart();
                    }

                    m_nIndex++;
                    NextSeq(2);
                    return;

                case 6:
                    if (m_nIndex >= m_nAxisNoArray.Length)
                    {
                        m_nIndex = 0;
                        NextSeq(10);
                        return;
                    }
                    break;
                case 7:
                    if (FNC.IsBusy(RunLib.msecDelay(500)))
                        return;

                    if (MotionMgr.Inst[m_nAxisNoArray[m_nIndex]].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Fail)
                    {
                        SetError((int)ERDF.E_SV_NOT_HOME + m_nAxisNoArray[m_nIndex]);
                        return;
                    }

                    if (MotionMgr.Inst[m_nAxisNoArray[m_nIndex]].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Process ||
                        MotionMgr.Inst[m_nAxisNoArray[m_nIndex]].IsBusy())
                    {
                        return;
                    }

                    GbVar.mcState.isHomeComplete[m_nAxisNoArray[m_nIndex]] = true;
                    break;

                case 8:
                    m_nIndex++;
                    NextSeq(6);
                    return;


                case 10:
                    if (m_nIndex >= m_nAxisNoArray.Length)
                    {
                        m_nIndex = 0;
                        NextSeq(15);

                        if (GbVar.isInfiniteHome == true)
                        {
                            for (int i = 0; i < m_nAxisNoArray.Length; i++)
                            {
                                if (m_nAxisNoArray[i] < (int)SVDF.AXES.CHIP_PK_1_Z_1 || m_nAxisNoArray[i] > (int)SVDF.AXES.CHIP_PK_2_T_6)
                                {
                                    m_nIndex = 0;
                                    NextSeq(15);
                                    return;
                                }
                            }

                            NextSeq(0);
                        }
                        return;
                    }
                    break;
                case 11:
                    {
                        // 211108 CHOH : 피커축 싱글홈 완료하면 안전위치로 이동하기
                        if (m_nAxisNoArray[m_nIndex] == (int)SVDF.AXES.UNIT_PK_X)
                        {
                            //nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_READY);

                            //if (FNC.IsErr(nFuncResult))
                            //{
                            //    return;
                            //}
                            //else if (FNC.IsBusy(nFuncResult)) return;
                        }
                        else if (m_nAxisNoArray[m_nIndex] == (int)SVDF.AXES.MAP_PK_X)
                        {
                            //nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1);

                            //if (FNC.IsErr(nFuncResult))
                            //{
                            //    return;
                            //}
                            //else if (FNC.IsBusy(nFuncResult)) return;
                        }
                    }
                    break;

                case 12:
                    m_nIndex++;
                    NextSeq(10);
                    return;

                case 15:
                    FINISH = true;
                    return;
                default:
                    break;

            
            }

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
