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
    public class DBExMgzInfo : SQLiteBase
    {
        public string strTableName = "TRK_MGZ_INFO";

        public DBExMgzInfo()
        {
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                MagazineId VARCHAR(50) NOT NULL DEFAULT '', 
                                StripId VARCHAR(100) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();
        }

        public void Enqueue_InsertDB(MagazineProperties colInfo)
        {
            DBQueue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("
                            + "'" + colInfo.Time + "', "
                            + "'" + colInfo.MagazineId + "', "
                            + "'" + colInfo.StripId + "')");
        }

    }
}
