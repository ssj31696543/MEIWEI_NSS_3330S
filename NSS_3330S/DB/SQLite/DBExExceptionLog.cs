using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteDB;

namespace NSS_3330S
{
    public class DBExExceptionLog : SQLiteBase
    {
        public string strTableName = "EXCEPTION_LOG";

        public DBExExceptionLog()
        {
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Time VARCHAR(50) NOT NULL DEFAULT '', 
                                FuncName VARCHAR(100) NOT NULL DEFAULT '', 
                                Detail VARCHAR(10000) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();
        }
    
        public void Enqueue_InsertDb(string funcName, string ex)
        {
            DBQueue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("
                            + "'" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + "', "
                            + "'" + funcName + "', "
                            + "'" + ex + "')");
        }

    }
}
