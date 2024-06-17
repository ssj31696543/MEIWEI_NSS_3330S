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
    public class DBExSeqInfo : SQLiteBase
    {
        public string strTableName = "PROC_INFO_SEQ";
        public const int nCreateRowCount = 1;

        public DBExSeqInfo()
        {
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(";

            for (int i = 0; i < (int)SeqProperties.Col.Max; i++)
			{
			    strQueryCreate += string.Format("{0} VARCHAR(10000) NOT NULL DEFAULT '', ",((SeqProperties.Col)i).ToString());
			}

            strQueryCreate = strQueryCreate.Substring(0,strQueryCreate.Length - 2) + ")";
            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();

            int nExistRow = GetTableRowsCount(TABLE_NAME);

            if (nExistRow < nCreateRowCount)
            {
                InsertEmptyRow(nExistRow);
            }
        }

        #region 내부함수
        /// <summary>
        /// 공백으로 구성된 열을 지정한 개수만큼 추가합니다.
        /// </summary>
        /// <param name="nExistRow">존재하는 열의 수</param>
        private void InsertEmptyRow(int nExistRow)
        {
            string strQuery = "";

            for (int nRow = nExistRow; nRow < nCreateRowCount; nRow++)
            {
                strQuery = "";
                strQuery = "INSERT INTO " + TABLE_NAME + " VALUES (";

                strQuery += string.Format("'{0}',", nRow);

                for (int i = 1; i < (int)SeqProperties.Col.Max; i++)
                {
                    strQuery += "'',";
                }

                strQuery = strQuery.TrimEnd(',') + ")";

                DBQueue.Enqueue(strQuery);
            }
        }


        /// <summary>
        /// 각 시퀀스의 값을 업데이트합니다.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="data"></param>
        private void Enqueue_UpdateSeq(string strSeqName, string strContent, int nRow = 0)
        {
            if (nRow >= nCreateRowCount) return;

            string strQuery = "";
            strQuery = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}={4}", TABLE_NAME, strSeqName, strContent, SeqProperties.Col.NO.ToString(), nRow);

            DBQueue.Enqueue(strQuery);
        }


        /// <summary>
        /// 각 시퀀스의 값을 가져옵니다.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="data"></param>
        private string Enqueue_SelectSeq(string strSeqName, int nRow = 0)
        {
            string strContents = "";
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
        
                        //sqCommend.CommandText = "SELECT * FROM " + TABLE_NAME + " WHERE " + strSeqName;
                        sqCommend.CommandText = string.Format("SELECT {0} FROM {1}",strSeqName, TABLE_NAME);       

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);
                        colInfo = new LogProperties[dt.Rows.Count];

                        DataRow dr = null;
                        if (dt.Rows.Count > 0)
                        {
                            strContents = dt.Rows[0].ItemArray[0].ToString();
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
            }

            return strContents;
        }
        #endregion

        #region 외부함수
        public bool UpLoadSeq(string strName, string strValue)
        {
            bool bRet = true;
            try
            {
                Enqueue_UpdateSeq(strName, strValue);
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        public string LoadSeq(string strName)
        {
            string strSeqValue = "";
            try
            {
                strSeqValue = Enqueue_SelectSeq(strName);
            }
            catch (Exception)
            {
                strSeqValue = "";
            }

            return strSeqValue;
        }
        #endregion
    }
}
