using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using SQLiteDB;

namespace NSS_3330S
{
    public class DBExTactTime : SQLiteBase
    {
        public string strTableName = "";

        public DBExTactTime()
        {
            // 테이블 명
            strTableName = "TACTTIME";
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Process VARCHAR(50) NOT NULL DEFAULT '', 
                                StartTime VARCHAR(50) NOT NULL DEFAULT '', 
                                EndTime VARCHAR(50) NOT NULL DEFAULT '', 
                                ElapsedTime VARCHAR(50) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();
        }

        public void Enqueue_InsertDB(TactTimeProperties colInfo)
        {
            DBQueue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("
                            + "'" + colInfo.Process + "', "
                            + "'" + colInfo.StartTime + "', "
                            + "'" + colInfo.EndTime + "', "
                            + "'" + colInfo.TactTime + "')");
        }

        public void Enqueue_UpdateDB(TactTimeProperties colInfo)
        {
            DBQueue.Enqueue("UPDATE " + TABLE_NAME + " SET " +
                            "StartTime = '" + colInfo.StartTime + "', " +
                            "EndTime = '" + colInfo.EndTime + "', " +
                            "ElapsedTime = '" + colInfo.TactTime + "' " +
                            "WHERE Process = '" + colInfo.Process + "'");
        }

        public void Enqueue_DeleteAllDB()
        {
            DBQueue.Enqueue("DELETE FROM " + TABLE_NAME);
        }

        public TactTimeProperties[] SelectTactTimeAll()
        {
            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                TactTimeProperties[] colInfo = null;
                using (var transaction = SQConn.BeginTransaction())
                {
                    //select 할 command객체 생성
                    using (SQLiteCommand sqCommend = new SQLiteCommand(SQConn))
                    {
                        sqliteLock.EnterReadLock();
                        sqCommend.CommandType = CommandType.Text;
                        //select query
                        sqCommend.CommandText = "SELECT * FROM " + TABLE_NAME;

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);
                        colInfo = new TactTimeProperties[dt.Rows.Count];

                        DataRow dr = null;
                        if (dt.Rows.Count > 0)
                        {
                            for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                            {
                                dr = dt.Rows[nCnt];
                                colInfo[nCnt] = new TactTimeProperties();
                                colInfo[nCnt].Process = dr.ItemArray[0].ToString();
                                colInfo[nCnt].StartTime = dr.ItemArray[1].ToString();
                                colInfo[nCnt].EndTime = dr.ItemArray[2].ToString();
                                colInfo[nCnt].TactTime = dr.ItemArray[3].ToString();
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

        public TactTimeProperties[] SelectTactTimeInRange(DateTime dtStart, DateTime dtEnd, string col_name = "StartTime", int nMaxCount = 10000)
        {
            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                TactTimeProperties[] colInfo = null;
                using (var transaction = SQConn.BeginTransaction())
                {
                    //select 할 command객체 생성
                    using (SQLiteCommand sqCommend = new SQLiteCommand(SQConn))
                    {
                        sqliteLock.EnterReadLock();
                        sqCommend.CommandType = CommandType.Text;
                        //select query
                        sqCommend.CommandText = "SELECT * FROM " + TABLE_NAME + " WHERE " + col_name +
                                                " BETWEEN '" + dtStart.ToString("yyyyMMddHHmmssfff") + "'" +
                                                " AND '" + dtEnd.ToString("yyyyMMddHHmmssfff") + "' ORDER BY " + col_name;

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);
                        colInfo = new TactTimeProperties[dt.Rows.Count];

                        DataRow dr = null;
                        if (dt.Rows.Count > 0)
                        {
                            for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                            {
                                dr = dt.Rows[nCnt];
                                colInfo[nCnt] = new TactTimeProperties();
                                colInfo[nCnt].Process = dr.ItemArray[0].ToString();
                                colInfo[nCnt].StartTime = dr.ItemArray[1].ToString();
                                colInfo[nCnt].EndTime = dr.ItemArray[2].ToString();
                                colInfo[nCnt].TactTime = dr.ItemArray[3].ToString();
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
    }
}
