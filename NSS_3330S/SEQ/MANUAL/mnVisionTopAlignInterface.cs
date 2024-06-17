using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnVisionTopAlignInterface : SeqBase
    {
        int _nTableNo = 0;
        int _nMaxTriggerCount = 10;

        cycMapTransfer _cycMapTransfer = null;

        public mnVisionTopAlignInterface()
        {
            _cycMapTransfer = new cycMapTransfer((int)SEQ_ID.MAP_TRANSFER, 0);

            _cycMapTransfer.SetAutoManualMode(false);

            _cycMapTransfer.SetAddMsgFunc(SetProcMsgEvent);
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            _cycMapTransfer.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            _cycMapTransfer.InitSeq();
        }

        public void SetParam(int nTableNo)
        {
            _nTableNo = nTableNo;
            _nMaxTriggerCount = 2;
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
                    swTimeStamp.Restart();
                    break;

                case 2:
                    nFuncResult = _cycMapTransfer.ManualTopAlignInterface(_nTableNo, _nMaxTriggerCount);
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

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;

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
