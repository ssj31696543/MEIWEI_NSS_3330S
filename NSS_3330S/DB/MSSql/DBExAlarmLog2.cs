﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSS_3330S
{
    public class DBExAlarmLog2 : DBExBase
    {
        Thread m_thread;
        bool m_bFlagThreadAlive = false;

        ConcurrentQueue<string> m_queue = new ConcurrentQueue<string>();

        public string strTableName = "";
        bool _isOpen;
        DataTable m_dtResult = null;

        DateTime m_dtDelete;

        public DBExAlarmLog2()
        {
            m_dtDelete = DateTime.Now.AddDays(-1);

            // 테이블 명
            strTableName = "LOG_ALARM";
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Idx INT IDENTITY(1,1) NOT NULL,
                                Time BIGINT NOT NULL DEFAULT 0, 
                                No VARCHAR(50) NOT NULL DEFAULT '', 
                                Name VARCHAR(50) NOT NULL DEFAULT '', 
                                Cause VARCHAR(200) NOT NULL DEFAULT '')";

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
                m_thread.Name = "ALARM LOG THREAD";
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
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ALARM_LOG_EXP_PERIOD].nValue > 0)
                        {
                            DateTime dtDel = DateTime.Now.AddDays(-ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ALARM_LOG_EXP_PERIOD].nValue);
                            query = string.Format("DELETE FROM {0} WHERE Time < {1}", TABLE_NAME, dtDel.ToString(DATE_FORMAT));

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

        public void Enqueue_InsertDB(AlarmProperties colInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0} VALUES (", TABLE_NAME);
            sb.AppendFormat("'{0}',", colInfo.Time);
            sb.AppendFormat("'{0}',", colInfo.No == null ? "" : colInfo.No);
            sb.AppendFormat("'{0}',", colInfo.Name == null ? "" : colInfo.Name);
            sb.AppendFormat("'{0}')", colInfo.Cause == null ? "" : colInfo.Cause);
            m_queue.Enqueue(sb.ToString());
        }

        /// <summary>
        /// 원하는 내용의 DB를 검색합니다.
        /// </summary>
        /// <param name="tablename"> 테이블 이름 </param>
        /// <param name="col_name"> 열 이름 </param>
        /// <param name="item"> 아이템 </param>
        /// <param name="dt"> 데이터 테이블 </param> dt.Rows[0][0], dt.Rows[0][1]... 값을 사용하면 됨
        public AlarmProperties[] SelectDataInRange(DateTime dtStart, DateTime dtEnd, string col_name = "Time", int nMaxCount = 10000)
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

            AlarmProperties[] colInfo = null;
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTableToQuery(dt, query);

                colInfo = new AlarmProperties[dt.Rows.Count];

                if (dt.Rows.Count > 0)
                {
                    DataRow dr;

                    for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                    {
                        dr = dt.Rows[nCnt];
                        colInfo[nCnt] = new AlarmProperties();
                        colInfo[nCnt].Time = dr.ItemArray[1].ToString();
                        colInfo[nCnt].No = dr.ItemArray[2].ToString();
                        colInfo[nCnt].Name = dr.ItemArray[3].ToString();
                        colInfo[nCnt].Cause = dr.ItemArray[4].ToString();
                    }
                }

                dt.Dispose();
            }
            return colInfo;
        }

        public DataTable DataTableSelectDataInRange(DateTime dtStart, DateTime dtEnd, string col_name = "Time", int nMaxCount = 10000)
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
