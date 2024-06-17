using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnCfgPosMove : SeqBase
    {
        public int[] m_nAxisArray = null;
        public double[] m_dPosArray = null;
        public uint[] m_nOrderArray = null;
        public double[] m_dSpeedArray = null;
        public double[] m_dAccArray = null;
        public double[] m_dDecArray = null;

        uint m_nCurrentOrder = 0;
        uint m_nMaxOrder = 10;

        bool m_bManualSpeed = false;

        bool m_bInterlock = true;

        public void SetParam(int[] nAxisArr, double[] dPosArr, uint[] nOrderArr, bool bInterlock, bool bManualSpeed, double[] dSpeed, double[] dAcc, double[] dDec)
        {
            m_nAxisArray = new int[nAxisArr.Length];
            Array.Copy(nAxisArr, m_nAxisArray, nAxisArr.Length);

            m_dPosArray = new double[dPosArr.Length];
            Array.Copy(dPosArr, m_dPosArray, dPosArr.Length);

            m_dAccArray = new double[dAcc.Length];
            Array.Copy(dAcc, m_dAccArray, dAcc.Length);

            m_dDecArray = new double[dDec.Length];
            Array.Copy(dDec, m_dDecArray, dDec.Length);

            m_nOrderArray = new uint[nOrderArr.Length];
            Array.Copy(nOrderArr, m_nOrderArray, nOrderArr.Length);

            m_bInterlock = bInterlock;

            if (dSpeed == null) bManualSpeed = false;
            if (bManualSpeed == true)
            {
                m_dSpeedArray = new double[dSpeed.Length];
                Array.Copy(dSpeed, m_dSpeedArray, dSpeed.Length);

                m_dAccArray = new double[dAcc.Length];
                Array.Copy(dAcc, m_dAccArray, dAcc.Length);

                m_dDecArray = new double[dDec.Length];
                Array.Copy(dDec, m_dDecArray, dDec.Length);
            }

            m_bManualSpeed = bManualSpeed;
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
                    m_nCurrentOrder = 0;

                    m_nMaxOrder = 0;
                    for (int nCnt = 0; nCnt < m_nOrderArray.Length; nCnt++)
                    {
                        if (m_nMaxOrder < m_nOrderArray[nCnt])
                            m_nMaxOrder = m_nOrderArray[nCnt];
                    }

                    swTimeStamp.Restart();
                    break;

                case 2:
                    nFuncResult = MoveCurrentOrder(m_nCurrentOrder, 0, m_bInterlock);

                    if (FNC.IsErr(nFuncResult))
                    {
                        SetError(nFuncResult);
                        return;
                    }
                    else if (FNC.IsBusy(nFuncResult)) return;
                    {
                        int nCount = 0;
                        for (int nCnt = 0; nCnt < m_nOrderArray.Length; nCnt++)
                        {
                            if (m_nOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                //System.Diagnostics.Debug.WriteLine(string.Format("Order {0} => {1} Axis : {2:0.000}, Current {3:0.000}",
                                //                                                 m_nCurrentOrder,
                                //                                                 m_nAxisArray[nCnt], m_dPosArray[nCnt],
                                //                                                 MotionMgr.Inst[m_nAxisArray[nCnt]].GetRealPos()));
                                nCount++;
                            }
                        }
                    }
                    break;
                case 4:
                    m_nCurrentOrder++;
                    if (m_nCurrentOrder < 10)
                    {
                        NextSeq(2);
                        return;
                    }
                    break;

                case 6:
                    swTimeStamp.Stop();
                    System.Diagnostics.Debug.WriteLine(swTimeStamp.Elapsed.TotalMilliseconds);
                    FINISH = true;
                    return;
                default:
                    m_bIgnoreLog = true;
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

        int MoveCurrentOrder(uint nOrder, long lDelay = 0, bool bCheckInterlock = true)
        {
            int nCountAxis = 0;

            for (int nCnt = 0; nCnt < m_nOrderArray.Length; nCnt++)
            {
                if (m_nOrderArray[nCnt] == nOrder)
                    nCountAxis++;
            }

            if (nCountAxis <= 0)
                return FNC.SUCCESS;

            int[] nAxisArray = new int[nCountAxis];
            double[] dPosArray = new double[nCountAxis];
            double[] dSpeedArray = new double[nCountAxis];
            double[] dAccArray = new double[nCountAxis];
            double[] dDecArray = new double[nCountAxis];

            int nCount = 0;
            for (int nCnt = 0; nCnt < m_nOrderArray.Length; nCnt++)
            {
                if (m_nOrderArray[nCnt] == nOrder)
                {
                    nAxisArray[nCount] = m_nAxisArray[nCnt];
                    dPosArray[nCount] = m_dPosArray[nCnt];
                    if (m_bManualSpeed)
                    {
                        dSpeedArray[nCount] = m_dSpeedArray[nCnt];
                        dAccArray[nCount] = m_dAccArray[nCnt];
                        dDecArray[nCount] = m_dDecArray[nCnt];
                    }
                    nCount++;
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray.Where(p => p == nAxisArray[nCnt]).Count() > 1)
                {
                    // 같은 축이 같은 순서(order)에 있다...
                    // 이러면 잘못된 것
                    System.Diagnostics.Debugger.Break();
                    return (int)ERDF.E_SV_MOVE + nAxisArray[nCnt];
                }
            }

            if (m_bManualSpeed)
            {
                return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, bCheckInterlock);
            }
            else
            {
                return AxisMultiMovePos(nAxisArray, dPosArray, lDelay, bCheckInterlock);
            }
        }
    }
}
