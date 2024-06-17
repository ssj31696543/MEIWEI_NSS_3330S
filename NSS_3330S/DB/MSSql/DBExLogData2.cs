using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSS_3330S
{
    public class DBExLogData2 : DBExBase
    {
        Thread m_thread;
        bool m_bFlagThreadAlive = false;
        bool m_isForcedDelete = false;

        ConcurrentQueue<string> m_queue = new ConcurrentQueue<string>();

        public string strTableName = "";
        bool _isOpen;
        DataTable m_dtResult = null;

        DBDF.LOG_TYPE m_Type = DBDF.LOG_TYPE.SEQ_MANUAL_RUN;
        Stopwatch swDeleteTime = new Stopwatch();

        DateTime m_dtDelete;

        public DBExLogData2(int nType)
        {
            // nType 범위 확인
            if (nType < 0 || nType >= (int)DBDF.LOG_TYPE.MAX)
            {
                return;
            }

            m_Type = (DBDF.LOG_TYPE)nType;
            m_dtDelete = DateTime.Now.AddDays(-1);

            // 테이블 명
            strTableName = string.Format("{0}", DBDF.LogDbName[(int)nType]);
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Idx INT IDENTITY(1,1) NOT NULL,
                                Time BIGINT NOT NULL DEFAULT 0, 
                                Model VARCHAR(100) NOT NULL DEFAULT '', 
                                Level VARCHAR(100) NOT NULL DEFAULT '',
                                User_Id VARCHAR(100) NOT NULL DEFAULT '',
                                Cell_Id VARCHAR(100) NOT NULL DEFAULT '', 
                                Log VARCHAR(200) NOT NULL DEFAULT '',
                                Command VARCHAR(200) NOT NULL DEFAULT '',
                                Part VARCHAR(200) NOT NULL DEFAULT '', 
                                Status VARCHAR(200) NOT NULL DEFAULT '')";

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
                m_thread.Name = string.Format("SEQUENCE LOG THREAD {0}", m_Type);
                if (m_thread.IsAlive == false)
                    m_thread.Start(this);

                swDeleteTime.Start();
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
                    Thread.Sleep(100);

                    if (DateTime.Now.Day != m_dtDelete.Day || m_isForcedDelete == true || 
                        (swDeleteTime.IsRunning == true && swDeleteTime.ElapsedMilliseconds > 1800000))// 30분에 한번 씩
                    {
                        swDeleteTime.Stop();
                        m_isForcedDelete = false;

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SEQ_LOG_EXP_PERIOD].nValue > 0)
                        {
                            DateTime dtDel = DateTime.Now.AddDays(-ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SEQ_LOG_EXP_PERIOD].nValue);
                            query = string.Format("DELETE TOP(1000) FROM {0} WHERE Time < {1}", TABLE_NAME, dtDel.ToString(DATE_FORMAT));

                            try
                            {
                                DateTime dtStart = DateTime.Now;
                                helper.Execute(query);
                                TimeSpan ts = DateTime.Now - dtStart;
                                System.Diagnostics.Debug.WriteLine(string.Format("Del Time : {0}", ts.ToString()));
                            }
                            catch (Exception)
                            {
                                swDeleteTime.Restart();
                            }

                            m_dtDelete = DateTime.Now;
                        }

                        swDeleteTime.Restart();
                    }
                }
            }
        }

        public void ForcedDeleteOldFile()
        {
            m_isForcedDelete = true;
        }

        public void Enqueue_InsertDB(string tablename, LogProperties colInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0} VALUES (", tablename);
            sb.AppendFormat("'{0}',", colInfo.Time);
            sb.AppendFormat("'{0}',", colInfo.Model == null ? "" : colInfo.Model);
            sb.AppendFormat("'{0}',", colInfo.Level == null ? "" : colInfo.Level);
            sb.AppendFormat("'{0}',", colInfo.User_Id == null ? "" : colInfo.User_Id);
            sb.AppendFormat("'{0}',", colInfo.Cell_Id == null ? "" : colInfo.Cell_Id);
            sb.AppendFormat("'{0}',", colInfo.Log == null ? "" : colInfo.Log);
            sb.AppendFormat("'{0}',", colInfo.Command == null ? "" : colInfo.Command);
            sb.AppendFormat("'{0}',", colInfo.Part == null ? "" : colInfo.Part);
            sb.AppendFormat("'{0}')", colInfo.Status == null ? "" : colInfo.Status);

            m_queue.Enqueue(sb.ToString());
        }

        /// <summary>
        /// 원하는 내용의 DB를 검색합니다.
        /// </summary>
        /// <param name="tablename"> 테이블 이름 </param>
        /// <param name="col_name"> 열 이름 </param>
        /// <param name="item"> 아이템 </param>
        /// <param name="dt"> 데이터 테이블 </param> dt.Rows[0][0], dt.Rows[0][1]... 값을 사용하면 됨
        public LogProperties[] SelectDataInRange(DateTime dtStart, DateTime dtEnd, string col_name = "Time", int nMaxCount = 10000)
        {

            string query = string.Format("SELECT * FROM {0} WHERE {1} BETWEEN '{2}' AND '{3}' ORDER BY {4}",
                                                TABLE_NAME, col_name, 
                                                dtStart.ToString(DATE_FORMAT),
                                                dtEnd.ToString(DATE_FORMAT),
                                                col_name);

            if (helper.CONNECTION.State != ConnectionState.Open)
            {
                Open(dbInfo);
            }

            LogProperties[] colInfo = null;
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTableToQuery(dt, query);

                colInfo = new LogProperties[dt.Rows.Count];

                if (dt.Rows.Count > 0)
                {
                    DataRow dr;

                    for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                    {
                        dr = dt.Rows[nCnt];
                        colInfo[nCnt] = new LogProperties();
                        colInfo[nCnt].Time = dr.ItemArray[1].ToString();
                        colInfo[nCnt].Model = dr.ItemArray[2].ToString();
                        colInfo[nCnt].Level = dr.ItemArray[3].ToString();
                        colInfo[nCnt].User_Id = dr.ItemArray[4].ToString();
                        colInfo[nCnt].Cell_Id = dr.ItemArray[5].ToString();
                        colInfo[nCnt].Log = dr.ItemArray[6].ToString();
                    }
                }

                dt.Dispose();

                return colInfo;
            }

            return null;
        }

        public int GetCountDataInRange(DateTime dtStart, DateTime dtEnd, string col_name = "Time")
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
            {
                Open(dbInfo);
            }

            return GetCount("WHERE {0} BETWEEN '{1}' AND '{2}'", 
                                                col_name,
                                                dtStart.ToString(DATE_FORMAT),
                                                dtEnd.ToString(DATE_FORMAT));
        }

        public DataTable DataTableSelectDataInRange(DateTime dtStart, DateTime dtEnd, string col_name = "Time", int nMaxCount = 10000)
        {
            string query = string.Format("SELECT TOP 1000 * FROM {0} WHERE {1} BETWEEN '{2}' AND '{3}' ORDER BY {4}",
                                                TABLE_NAME, col_name,
                                                dtStart.ToString(DATE_FORMAT),
                                                dtEnd.ToString(DATE_FORMAT),
                                                col_name);

            if (helper.CONNECTION.State != ConnectionState.Open)
            {
                Open(dbInfo);
            }

            if (m_dtResult != null)
            {
                m_dtResult.Dispose();
                m_dtResult = null;
            }
            m_dtResult = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTableToQuery(m_dtResult, query);

                return m_dtResult;
            }

            return null;
        }

        public DataTable DataTableSelectDataInRangePage(DateTime dtStart, DateTime dtEnd, int nPageNo, int nPageSize, string col_name = "Time")
        {
            string query = string.Format("SELECT * FROM {0} WHERE {0}.{1} BETWEEN '{2}' AND '{3}' ORDER BY {0}.{1} OFFSET ({4}-1)*{5} ROW FETCH NEXT {5} ROW ONLY",
                                                TABLE_NAME, col_name,
                                                dtStart.ToString(DATE_FORMAT),
                                                dtEnd.ToString(DATE_FORMAT),
                                                nPageNo, nPageSize);

            if (helper.CONNECTION.State != ConnectionState.Open)
            {
                Open(dbInfo);
            }

            if (m_dtResult != null)
            {
                m_dtResult.Dispose();
                m_dtResult = null;
            }
            m_dtResult = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTableToQuery(m_dtResult, query);

                return m_dtResult;
            }

            return null;
        }
    }
}
