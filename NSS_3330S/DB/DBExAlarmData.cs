using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteDB;

namespace NSK_8000S
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

            CreateDB();
        }
      }
}
