using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Drawing;

namespace NSS_3330S
{
    public class DBExITSLog2 : DBExBase
    {
        Thread m_thread;
        bool m_bFlagThreadAlive = false;

        ConcurrentQueue<string> m_queue = new ConcurrentQueue<string>();

        public string strTableName = "";
        bool _isOpen;
        DataTable m_dtResult = null;

        DateTime m_dtDelete;

        public DBExITSLog2()
        {
            m_dtDelete = DateTime.Now.AddDays(-1);

            // 테이블 명
            strTableName = "TRK_ITS_LOG";
            TABLE_NAME = strTableName;
            
            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Idx INT IDENTITY(1,1) NOT NULL,
                                InTime BIGINT NOT NULL DEFAULT 0,
                                LotNumber VARCHAR(100) NOT NULL DEFAULT '',
                                SHIPTO VARCHAR(100) NOT NULL DEFAULT '',
                                StripID VARCHAR(100) NOT NULL DEFAULT '',
                                StripX VARCHAR(100) NOT NULL DEFAULT '',
                                StripY VARCHAR(100) NOT NULL DEFAULT '',
                                XOUT_X VARCHAR(100) NOT NULL DEFAULT '',
                                XOUT_Y VARCHAR(100) NOT NULL DEFAULT '', 
                                LAngle VARCHAR(100) NOT NULL DEFAULT '',
                                Lot_Id VARCHAR(100) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            _isOpen = Open(dbInfo);
            if (_isOpen == false) return;
#if DBServer
            Create();
            //CreateData();
#endif

            StartStopManualThread(true);
        }

        public override void Close()
        {
            StartStopManualThread(false);

            base.Close();
        }

        public void StartStopManualThread(bool bStart)
        {
            if (m_thread != null)
            {
                // Join의 시간은 Thread에서 처리되는 최대 시간으로 지정
                m_bFlagThreadAlive = false;
                m_thread.Join(600);
                m_thread.Abort();
                m_thread = null;
            }

            if (bStart)
            {
                m_bFlagThreadAlive = true;
                m_thread = new Thread(new ParameterizedThreadStart(Run));
                m_thread.Name = "ITS LOG THREAD";
                if (m_thread.IsAlive == false)
                    m_thread.Start(this);
            }
        }

        void Run(object obj)
        {
            StringBuilder sb = new StringBuilder();
            int nCount = 0;

            while (m_bFlagThreadAlive)
            {
                Thread.Sleep(10);

                string query = "";
                if (m_queue.Count > 0)
                {
                    nCount = 0;
                    sb.Clear();

                    while (m_queue.Count > 0 && m_queue.TryDequeue(out query))
                    {
                        sb.AppendFormat("{0};", query);

                        nCount++;
                        if (nCount > 20)
                            break;
                    }

                    if (nCount == 0)
                        continue;

                    try
                    {
                        helper.Execute(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        GbFunc.WriteExeptionLog(ex.ToString());
                    }
                }
                else
                {
                    Thread.Sleep(500);

                    if (DateTime.Now.Day != m_dtDelete.Day)
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_LOG_EXP_PERIOD].nValue > 0)
                        {
                            DateTime dtDel = DateTime.Now.AddDays(-ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_LOG_EXP_PERIOD].nValue);
                            query = string.Format("DELETE FROM {0} WHERE InTime < {1}", TABLE_NAME, dtDel.ToString(DATE_FORMAT));

                            try
                            {
                                DateTime dtStart = DateTime.Now;
                                helper.Execute(query);
                                TimeSpan ts = DateTime.Now - dtStart;
                                System.Diagnostics.Debug.WriteLine(string.Format("Del Time : {0}", ts.ToString()));
                            }
                            catch (Exception)
                            {
                            }

                            m_dtDelete = DateTime.Now;
                        }
                    }
                }
            }
        }

        #region 내부 함수
        #region INSERT
        public bool Enqueue_InsertDB(DateTime InTime, string strLotId, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            bool bRet = false;

            try
            {
                if (dt == null) return bRet;

                string strQuery = "";
                sb.AppendFormat("INSERT INTO {0} VALUES", TABLE_NAME);

                DataRow dr;

                for (int nRow = 0; nRow < dt.Rows.Count; nRow++)
                {
                    dr = dt.Rows[nRow];

                    sb.Append("(");

                    sb.AppendFormat("'{0}', ", InTime.ToString(DATE_FORMAT));

                    for (int nCol = 0; nCol < dr.ItemArray.Length; nCol++)
                    {
                        sb.AppendFormat("'{0}', ", dr.ItemArray[nCol]);
                    }

                    sb.AppendFormat("'{0}'), ", strLotId);
                }

                int nSubLen = sb.ToString().Length - 2;
                if (nSubLen < 0)
                    nSubLen = 0;

                strQuery = sb.ToString().Substring(0, nSubLen);
                 m_queue.Enqueue(strQuery);

                bRet = true;
                return bRet;
            }
            catch (Exception)
            {
                return bRet;
            }
        }

        private Point[] Enqueue_SelectXOUT(string strStripId)
        {
            Point[] XOutLoc = new Point[100];
            try
            {
                int nX, nY;

                string query = string.Format("WHERE {0} = '{1}'", ITS_LOG.ColName.StripID.ToString(), strStripId);

                DataTable dt = new DataTable();

                if (helper.CONNECTION.State == ConnectionState.Open)
                {
                    SelectToDataTable(query, dt);

                    DataRow dr = null;
                    if (dt.Rows.Count > 0)
                    {
                        XOutLoc = new Point[dt.Rows.Count];
                        for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                        {
                            dr = dt.Rows[nCnt];

                            if (!int.TryParse(dr.ItemArray[(int)ITS_LOG.ColName.XOUT_X].ToString(), out nX)) nX = -1;
                            if (!int.TryParse(dr.ItemArray[(int)ITS_LOG.ColName.XOUT_Y].ToString(), out nY)) nY = -1;

                            XOutLoc[nCnt].X = nX;
                            XOutLoc[nCnt].Y = nY;
                        }
                    }
                }
                return XOutLoc;
            }
            catch (Exception)
            {
                return XOutLoc;
            }
        }

        public DataTable SelectItsLog(string strStripId)
        {
            string query = string.Format("WHERE {0} = '{1}'", ITS_LOG.ColName.StripID.ToString(), strStripId);
            if (strStripId == "") query = "";//공백 일 경우 전부 가져 옴
            
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTable(query, dt);
            }
            return dt;
        }
        #endregion

        #region UPDATE
        #endregion

        #region DELETE
        /// <summary>
        /// 지정 개수 만큼만 남겨두고 나머지 레코드는 삭제합니다.
        /// </summary>
        /// <param name="nLimitCount">남길 레코드의 개수</param>
        /// <param name="eSortProp">내림차순 정렬 시 기준이 되는 ColName</param>
        private void Enqueue_DeleteTopN(int nLimitCount, ITS_LOG.ColName eSortProp = ITS_LOG.ColName.InTime)
        {
            string strQuery = "";
            strQuery = string.Format("DELETE FROM {0} WHERE {1} NOT IN ( SELECT * FROM ( SELECT {1} FROM {0} ORDER BY {1} DESC) LIMIT {2})", TABLE_NAME, eSortProp.ToString(), nLimitCount);
            m_queue.Enqueue(strQuery);
        }

        /// <summary>
        /// 일치하는 스트립 아이디가 존재 할 시 해당 레코드를 삭제합니다.
        /// </summary>
        /// <param name="strStripId">스트립 아이디</param>
        private void Enqueue_DeleteDB(string strStripId)
        {
            m_queue.Enqueue("DELETE FROM " + TABLE_NAME + " WHERE " + ITS_LOG.ColName.StripID.ToString() + "='" + strStripId + "'");
        }

        /// <summary>
        /// 일치하는 스트립 아이디가 존재 할 시 해당 레코드를 삭제합니다.
        /// </summary>
        /// <param name="strStripId">스트립 아이디</param>
        private void Enqueue_DeleteAll()
        {
            m_queue.Enqueue("DELETE FROM " + TABLE_NAME);
        }
        #endregion
        #endregion

        #region 외부 함수
        public bool DeleteXOutInfo()
        {
            bool bRet = true;
            try
            {
                Enqueue_DeleteAll();
            }
            catch (Exception)
            {

            }

            return bRet;
        }

        public Point[] SelectXOutInfo(string StripId)
        {
            Point[] XOutLoc = null;

            try
            {
                XOutLoc = Enqueue_SelectXOUT(StripId);
            }
            catch (Exception)
            {
                XOutLoc = null;
            }

            return XOutLoc;
        }

        public DataTable SelectLog(string strStripId)
        {
            DataTable dt = new DataTable();
            try
            {
                dt = SelectItsLog(strStripId);
            }
            catch (Exception)
            {
            }

            return dt;
        }

        public bool isExsitStrip(string strStripId)
        {
            bool bRet = true;
            try
            {
                Point[] XOutLoc = null;

                XOutLoc = Enqueue_SelectXOUT(strStripId);

                if (XOutLoc == null)
                {
                    bRet = false;
                }
                else
                {
                    if (XOutLoc.Length == 0)
                    {
                        bRet = false;
                    }
                }
            }
            catch (Exception)
            {

            }

            return bRet;
        }

        /// <summary>
        /// SQL 서버에서 가져온 ITS정보를 로그에 기록합니다.
        /// 220604 lot id추가
        /// </summary>
        /// <param name="dt">ITS Location Info</param>
        /// <returns>성공 유무</returns>
        public bool InsertXOutInfo(string strLotId, DataTable dt)
        {
            bool bRet = true;
            try
            {
                bRet = Enqueue_InsertDB(DateTime.Now, strLotId, dt);
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }
        #endregion

    }
}
