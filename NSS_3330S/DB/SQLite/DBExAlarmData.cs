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
    public class DBExAlarmData : SQLiteBase
    {
        public string strTableName = "";

        public DBExAlarmData()
        {
            // 테이블 명
            strTableName = "LOG_ALARM";
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Time BIGINT NOT NULL DEFAULT 0, 
                                No VARCHAR(50) NOT NULL DEFAULT '', 
                                Detail VARCHAR(200) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();

            if (!CheckIfColumnExists(TABLE_NAME, "Cause"))
            {
                AddColumn(TABLE_NAME, "Cause VARCHAR(255) NOT NULL DEFAULT ''");
            }
        }

        public void Enqueue_InsertDB(AlarmProperties colInfo)
        {
            DBQueue.Enqueue(string.Format("INSERT INTO {0} VALUES ('{1}', '{2}', '{3}', '{4}')", TABLE_NAME, colInfo.Time, colInfo.No, colInfo.Name, colInfo.Cause));
            //DBQueue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("
            //                + "'" + colInfo.Time + "', "
            //                 + "'" + colInfo.No + "', "
            //                     + "'" + colInfo.Name + "')");
        }

        public AlarmProperties[] SelectAlarmDataInRange(DateTime dtStart, DateTime dtEnd, string col_name = "Time", int nMaxCount = 10000)
        {
            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                AlarmProperties[] colInfo = null;
                using (var transaction = SQConn.BeginTransaction())
                {
                    //select 할 command객체 생성
                    using (SQLiteCommand sqCommend = new SQLiteCommand(SQConn))
                    {
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
                        colInfo = new AlarmProperties[dt.Rows.Count];

                        DataRow dr = null;
                        if (dt.Rows.Count > 0)
                        {
                            for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                            {
                                dr = dt.Rows[nCnt];
                                colInfo[nCnt] = new AlarmProperties();
                                colInfo[nCnt].Time = dr.ItemArray[0].ToString();
                                colInfo[nCnt].No = dr.ItemArray[1].ToString();
                                colInfo[nCnt].Name = dr.ItemArray[2].ToString();
                                colInfo[nCnt].Cause = dr.ItemArray[3].ToString();
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

        //public void SetDeletePeriod(int nPeriod)
        //{
        //    DeleteDB_TrigReset();
        //    DeleteDB_TrigSet(nPeriod, "Time");
        //}
    }
}
