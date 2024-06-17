using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NSS_3330S
{
    public class IFSeqBase
    {
        #region VAR
        private int m_nSeqNo = 0;
        private bool m_bSeqStart = false;
        private bool m_bSeqReply = false;
        private bool m_bSeqFault = false;
        private string[] m_strDataArr = null;
        public Stopwatch swTimeStamp = new Stopwatch();

        private int m_nMode = 0;
        private string m_strPpid = "";
        private long m_lEqmsgid = 0;
        private string m_strCpName = "";
        private string m_strReturn = "";
        private List<string> m_listPPID = new List<string>();
        #endregion

        #region TOR
        public IFSeqBase()
        {
            ResetCmd();
            swTimeStamp.Stop();
            swTimeStamp.Reset();
        }

        #endregion

        public void ResetCmd()
        {
            m_nMode = 0;
            m_strPpid = "";
            m_lEqmsgid = 0;
            m_strCpName = "";
            m_strReturn = "";

            m_nSeqNo = 0;
            m_bSeqStart = false;
            m_bSeqFault = false;
            m_bSeqReply = false;
            m_strDataArr = null;
            swTimeStamp.Stop();
            swTimeStamp.Reset();
        }

        public void SemiResetCmd()
        {
            m_nMode = 0;
            m_strPpid = "";
            m_lEqmsgid = 0;
            m_strCpName = "";

            m_nSeqNo = 0;
            m_bSeqStart = false;
            m_bSeqFault = false;
            swTimeStamp.Stop();
            swTimeStamp.Reset();
        }


        public int GetSeqNo
        {
            get { return m_nSeqNo; }
        }

        #region SEQ VAR

        public int SEQNO
        {
            get { return m_nSeqNo; }
            set { m_nSeqNo = value; }
        }

        public bool SEQSTART
        {
            get { return m_bSeqStart; }
            set { m_bSeqStart = value; }
        }

        public bool SEQREPLY
        {
            get { return m_bSeqReply; }
            set { m_bSeqReply = value; }
        }

        public bool SEQFAULT
        {
            get { return m_bSeqFault; }
            set { m_bSeqFault = value; }
        }

        public string[] SEQDATA
        {
            get { return m_strDataArr; }
            set { m_strDataArr = value; }
        }

        #endregion

        #region 전용변수

        public int MODE
        {
            get { return m_nMode; }
            set { m_nMode = value; }
        }
        public string PPID
        {
            get { return m_strPpid; }
            set { m_strPpid = value; }
        }
        public long EQMSGID
        {
            get { return m_lEqmsgid; }
            set { m_lEqmsgid = value; }
        }

        public string CPNAME
        {
            get { return m_strCpName; }
            set { m_strCpName = value; }
        }

        public List<string> PPIDLIST
        {
            get { return m_listPPID; }
            set { m_listPPID = value; }
        }

        public string RETURN
        {
            get { return m_strReturn; }
            set { m_strReturn = value; }
        }
        #endregion
    }
}
