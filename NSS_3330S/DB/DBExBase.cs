using DionesTool.DATA;
using DionesTool.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace NSK_8000S
{
    public abstract class DBExBase
    {
        public delegate void DeleException(string strErr);
        public event DeleException deleException = null;

        protected OleDbHelper helper = new OleDbHelper();
        protected DbInfo dbInfo = null;

        protected const string strQueryExists = @"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'";
        protected const string strQueryCount = @"SELECT COUNT(*) FROM {0}";

        protected string TABLE_NAME = "TABLE_NAME";

        protected string strQueryCreate = @"CREATE TABLE {0}(
                                            ID INT PRIMARY KEY,
                                            NAME VARCHAR(40) NOT NULL,
                                            ECDEF VARCHAR(10) NOT NULL,
                                            ECMAX VARCHAR(10) NOT NULL,
                                            ECMIN VARCHAR(10) NOT NULL,
                                            LAYER VARCHAR(10) NOT NULL
                                            )";

        protected string lastErrorString = "";

        public OleDbHelper HELPER
        {
            get { return helper; }
        }

        public DBExBase()
        {
            helper.deleException += ReturnException;

            string strFileName = PathMgr.Inst.FILE_PATH_DB_INFO;
            dbInfo = DbInfo.Deserialize(strFileName);
            if (!dbInfo.EXISTS_LOAD_FILE)
            {
                dbInfo.type = OleDbHelper.TYPE_OLE_DB.MSSQL;
                dbInfo.userID = "MCDB";
                dbInfo.passWD = "diones";
                dbInfo.serverPath = "127.0.0.1\\MCDB,1433";
                dbInfo.dbName = "MCDB";
                dbInfo.Serialize(strFileName);
            }

#if LOCAL_DB
            dbInfo.serverPath = "localhost\\MCDB,1433";
#endif
        }

        ~DBExBase()
        {
            helper.deleException -= ReturnException;
        }

        /// <summary>
        /// DB OPEN
        /// </summary>
        /// <param name="dbInfo"></param>
        /// <returns></returns>
        public bool Open(DbInfo dbInfo)
        {
#if _DB_NOT_CONN
            return true;
#endif
            return helper.Open(dbInfo);
        }

        /// <summary>
        /// DB CLOSE
        /// </summary>
        public void Close()
        {
            helper.Close();
        }
        public bool IsOpen()
        {
            return helper.STATE == ConnectionState.Open;
        }


        protected virtual void ReturnException(string strErr)
        {
            lastErrorString = strErr;

            if (deleException != null)
                deleException(strErr);
        }

        public string GetLastError()
        {
            return lastErrorString;
        }

        /// <summary>
        /// 테이블이 없다면 생성합니다
        /// </summary>
        public void Create()
        {
            if (helper.STATE != ConnectionState.Open)
                return;

            string sql = string.Format(strQueryExists, TABLE_NAME);

            DataTable dt = new DataTable();
            helper.SelectToDataTable(sql, dt);

            if (dt.Rows.Count <= 0)
            {
                helper.Execute(string.Format(strQueryCreate, TABLE_NAME));
            }
        }

        /// <summary>
        /// 레코드 개수를 반환힙니다
        /// </summary>
        /// <param name="queryWhere">조건절이 들어간다면 추가 (WHERE ID='123')</param>
        /// <returns>레코드 개수</returns>
        public int GetCount(string queryWhere = "", params object[] args)
        {
            if (helper.STATE == ConnectionState.Closed)
                Open(dbInfo);

            string where = string.Format(queryWhere, args);
            string sql = string.Format(strQueryCount, TABLE_NAME) + " " + where;
            int cnt = 0;

            using (DataTable dt = new DataTable())
            {
                helper.SelectToDataTable(sql, dt);

                if (dt.Rows.Count <= 0)
                    return 0;

                if (dt.Rows[0].ItemArray.Count() < 0)
                    return 0;

                cnt = (int)dt.Rows[0].ItemArray[0];
            }

            return cnt;
        }

        /// <summary>
        /// Select한 값을 DataTable에 담습니다
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public bool SelectToDataTableToQuery(DataTable dt, string query)
        {
            if (helper.STATE == ConnectionState.Closed)
                Open(dbInfo);

            return helper.SelectToDataTable(query, dt);
        }

        /// <summary>
        /// Select한 값을 DataTable에 담습니다
        /// </summary>
        /// <param name="dt">담을 DataTable</param>
        /// <param name="queryWhere">조건절 (WHERE ID='123')</param>
        /// <returns>성공 여부</returns>
        public bool SelectToDataTable(DataTable dt, string queryWhere = "")
        {
            if (helper.STATE == ConnectionState.Closed)
                Open(dbInfo);

            string query = string.Format("SELECT * FROM {0} {1}", TABLE_NAME, queryWhere);

            return helper.SelectToDataTable(query, dt);
        }

        /// <summary>
        /// Select한 값을 ListView에 넣어줍니다
        /// </summary>
        /// <param name="listView">ListView</param>
        /// <param name="queryWhere">조건절 (WHERE ID='123')</param>
        /// <returns>성공 여부</returns>
        public bool SelectToListView(ListView listView, string queryWhere = "")
        {
            if (helper.STATE == ConnectionState.Closed)
                Open(dbInfo);

            string query = string.Format("SELECT * FROM {0} {1}", TABLE_NAME, queryWhere);

            return helper.SelectToListView(query, listView);
        }

        public bool DeleteAll()
        {
            if (helper.STATE == ConnectionState.Closed)
                Open(dbInfo);

            string query = string.Format("DELETE FROM {0}", TABLE_NAME);

            return helper.Execute(query);
        }

        public bool Execute(string query)
        {
            if (helper.STATE == ConnectionState.Closed)
                Open(dbInfo);

            return helper.Execute(query);
        }

        protected bool ExistsColumn(string columnName)
        {
#if _DB_NOT_CONN
            return true;
#endif
            using (DataTable dt = new DataTable())
            {
                // select * from INFORMATION_SCHEMA.COLUMNS where table_name=[테이블 명] and column_name=[필드 명]
                string query = string.Format(@"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name='{0}' AND column_name='{1}'", TABLE_NAME, columnName);
                helper.SelectToDataTable(query, dt);

                if (dt.Rows.Count == 0) return false;
            }

            return true;
        }

        protected bool AddColumn(string strAddColumn)
        {
            string query = string.Format("ALTER TABLE {0} ADD {1}", TABLE_NAME, strAddColumn);

            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                ReturnException(ex.ToString());
                return false;
            }

            return true;
        }
    }
}
