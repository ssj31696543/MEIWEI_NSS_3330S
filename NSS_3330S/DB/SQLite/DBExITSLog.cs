using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using SQLiteDB;
using System.Drawing;

namespace NSS_3330S
{
    public  class DBExITSLog : SQLiteBase
    {
        public string strTableName = "TRK_ITS_LOG";

        public DBExITSLog()
        {
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
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

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();

            if (!CheckIfColumnExists(TABLE_NAME, "Lot_Id"))
            {
                AddColumn(TABLE_NAME, "Lot_Id VARCHAR(100) NOT NULL DEFAULT ''");
            }
        }

        #region 내부 함수
        #region INSERT
        /// <summary>
        /// 새로운 레코드를 추가합니다.
        /// 220604 lot id추가
        /// </summary>
        /// <param name="Info">스트립 정보</param>
        /// <param name="Intime">시간</param>
        private bool Enqueue_InsertDB(DateTime InTime, string strLotId, DataTable dt)
        {
            bool bRet = true;
            try
            {
                if (dt == null) return false;

                string strQuery = "";
                StringBuilder sb = new StringBuilder();
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
                DBQueue.Enqueue(strQuery);

            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }
        #endregion

        #region SELECT
        private Point[] Enqueue_SelectXOUT(string strStripId)
        {

            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                Point[] XOutLoc = new Point[100];
                int nX, nY;
                using (var transaction = SQConn.BeginTransaction())
                {
                    //select 할 command객체 생성
                    using (SQLiteCommand sqCommend = new SQLiteCommand(SQConn))
                    {
                        if (sqliteLock.IsReadLockHeld)
                            sqliteLock.ExitReadLock();

                        sqliteLock.EnterReadLock();
                        sqCommend.CommandType = CommandType.Text;
                        //select query
                        sqCommend.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", TABLE_NAME, ITS_LOG.ColName.StripID.ToString(), strStripId);

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);

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
                        sqliteLock.ExitReadLock();

                        //사용헀던 객체 삭제
                        adapter.Dispose();
                        sqCommend.Dispose();
                        transaction.Commit();
                        SQConn.Close();
                        SQConn.Dispose();
                    }
                }
                return XOutLoc;
            }
        }

        public DataTable SelectItsLog(string strStripId)
        {
            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                using (var transaction = SQConn.BeginTransaction())
                {
                    //select 할 command객체 생성
                    using (SQLiteCommand sqCommend = new SQLiteCommand(SQConn))
                    {
                        sqliteLock.EnterReadLock();
                        sqCommend.CommandType = CommandType.Text;
                        //select query
                        sqCommend.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", TABLE_NAME, ITS_LOG.ColName.StripID.ToString(), strStripId);

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);

                        sqliteLock.ExitReadLock();

                        //사용헀던 객체 삭제
                        adapter.Dispose();
                        sqCommend.Dispose();
                        transaction.Commit();
                        SQConn.Close();
                        SQConn.Dispose();
                    }
                }
                return dt;
            }
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
            DBQueue.Enqueue(strQuery);
        }

        /// <summary>
        /// 일치하는 스트립 아이디가 존재 할 시 해당 레코드를 삭제합니다.
        /// </summary>
        /// <param name="strStripId">스트립 아이디</param>
        private void Enqueue_DeleteDB(string strStripId)
        {
            DBQueue.Enqueue("DELETE FROM " + TABLE_NAME + " WHERE " + ITS_LOG.ColName.StripID.ToString() + "='" + strStripId + "'");
        }

        /// <summary>
        /// 일치하는 스트립 아이디가 존재 할 시 해당 레코드를 삭제합니다.
        /// </summary>
        /// <param name="strStripId">스트립 아이디</param>
        private void Enqueue_DeleteAll()
        {
            DBQueue.Enqueue("DELETE FROM " + TABLE_NAME);
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

