using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using SQLiteDB;

namespace NSS_3330S
{
    public class DBExLogData : SQLiteBase
    {
        public string strTableName = "";

        public DBExLogData(int nType)
        {
            // nType 범위 확인
            if (nType < 0 || nType >= (int)DBDF.LOG_TYPE.MAX)
            {
                return;
            }

            // 테이블 명
            strTableName = string.Format("{0}", DBDF.LogDbName[(int)nType]);
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Time BIGINT NOT NULL DEFAULT 0, 
                                Model VARCHAR(100) NOT NULL DEFAULT '', 
                                Level VARCHAR(100) NOT NULL DEFAULT '',
                                User_Id VARCHAR(100) NOT NULL DEFAULT '',
                                Cell_Id VARCHAR(100) NOT NULL DEFAULT '', 
                                Log VARCHAR(100) NOT NULL DEFAULT '',
                                Command VARCHAR(200) NOT NULL DEFAULT '',
                                Part VARCHAR(200) NOT NULL DEFAULT '', 
                                Status VARCHAR(200) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();

            //DeleteDB_TrigSet(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_LOG_EXP_PERIOD].nValue, "Time");
        }

        public void Enqueue_InsertDB(string tablename, LogProperties colInfo)
        {
            DBQueue.Enqueue("INSERT INTO " + tablename + " VALUES ("
                            + "'" + colInfo.Time + "', "
                             + "'" + colInfo.Model + "', "
                              + "'" + colInfo.Level + "', "
                               + "'" + colInfo.User_Id + "', "
                                + "'" + colInfo.Cell_Id + "', "
                                  + "'" + colInfo.Log + "', "
                                   + "'" + colInfo.Command + "', "
                                    + "'" + colInfo.Part + "', "
                                     + "'" + colInfo.Status + "')");
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
            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                LogProperties[] colInfo = null;
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
                        sqCommend.CommandText = "SELECT * FROM " + TABLE_NAME + " WHERE " + col_name +
                                                " BETWEEN '" + dtStart.ToString(DATE_FORMAT) + "'" +
                                                " AND '" + dtEnd.ToString(DATE_FORMAT) + "' ORDER BY " + col_name;

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);
                        colInfo = new LogProperties[dt.Rows.Count];

                        DataRow dr = null;
                        if (dt.Rows.Count > 0)
                        {
                            for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                            {
                                dr = dt.Rows[nCnt];
                                colInfo[nCnt] = new LogProperties();
                                colInfo[nCnt].Time = dr.ItemArray[0].ToString();
                                colInfo[nCnt].Model = dr.ItemArray[1].ToString();
                                colInfo[nCnt].Level = dr.ItemArray[2].ToString();
                                colInfo[nCnt].User_Id = dr.ItemArray[3].ToString();
                                colInfo[nCnt].Cell_Id = dr.ItemArray[4].ToString();
                                colInfo[nCnt].Log = dr.ItemArray[5].ToString();
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
                return colInfo;
            }
        }

        public DataTable DataTableSelectDataInRange(DateTime dtStart, DateTime dtEnd, string col_name = "Time", int nMaxCount = 10000)
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
                        if (sqliteLock.IsReadLockHeld)
                            sqliteLock.ExitReadLock();

                        sqliteLock.EnterReadLock();
                        sqCommend.CommandType = CommandType.Text;
                        //select query
                        sqCommend.CommandText = "SELECT * FROM " + TABLE_NAME + " WHERE " + col_name +
                                                " BETWEEN '" + dtStart.ToString(DATE_FORMAT) + "'" +
                                                " AND '" + dtEnd.ToString(DATE_FORMAT) + "' ORDER BY " + col_name;

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);
                        //colInfo = new LogProperties[dt.Rows.Count];

                        //DataRow dr = null;
                        //if (dt.Rows.Count > 0)
                        //{
                        //    for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                        //    {
                        //        dr = dt.Rows[nCnt];
                        //        colInfo[nCnt] = new LogProperties();
                        //        colInfo[nCnt].Time = dr.ItemArray[0].ToString();
                        //        colInfo[nCnt].Model = dr.ItemArray[1].ToString();
                        //        colInfo[nCnt].Level = dr.ItemArray[2].ToString();
                        //        colInfo[nCnt].User_Id = dr.ItemArray[3].ToString();
                        //        colInfo[nCnt].Cell_Id = dr.ItemArray[4].ToString();
                        //        colInfo[nCnt].Log = dr.ItemArray[5].ToString();
                        //    }
                        //}
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

        //public void SetDeletePeriod(int nPeriod)
        //{
        //    DeleteDB_TrigReset();
        //    DeleteDB_TrigSet(nPeriod, "Time");
        //}
    }
}
