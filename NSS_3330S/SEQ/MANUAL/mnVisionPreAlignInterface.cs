using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnVisionPreAlignInterface : SeqBase
    {
        int _nAlignMode = 0;

        cycStripTransfer _cycStripTransfer = null;

        public mnVisionPreAlignInterface()
        {
            _cycStripTransfer = new cycStripTransfer((int)SEQ_ID.STRIP_TRANSFER, 0);

            _cycStripTransfer.SetAutoManualMode(false);

            _cycStripTransfer.SetAddMsgFunc(SetProcMsgEvent);
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            _cycStripTransfer.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            _cycStripTransfer.InitSeq();
        }

        public void SetParam(int nAlignMode)
        {
            _nAlignMode = nAlignMode;
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
                    nFuncResult = _cycStripTransfer.ManualPreAlignInterface(_nAlignMode);
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
