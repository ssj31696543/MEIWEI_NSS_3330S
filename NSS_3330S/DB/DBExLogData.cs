using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteDB;

namespace NSK_8000S
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
            strTableName = string.Format("{0}", DBDF.LogType[(int)nType]);
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Time BIGINT NOT NULL DEFAULT 0, 
                                Model VARCHAR(100) NOT NULL DEFAULT '', 
                                Level VARCHAR(100) NOT NULL DEFAULT '',
                                User_Id VARCHAR(100) NOT NULL DEFAULT '',
                                Cell_Id VARCHAR(100) NOT NULL DEFAULT '', 
                                Log VARCHAR(400) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            CreateDB();
        }
      }
}

    /*
     * 
            using (DataTable dt = new DataTable())
            {
                string query = string.Format(@"
SELECT TOP {2}
		[LOG_TIME],
        [LOG_DATA]
		FROM [MCDB].[dbo].[{3}]
		WHERE LOG_TIME BETWEEN CONVERT(BIGINT, '{0}') AND CONVERT(BIGINT, '{1}') ORDER BY LOG_TIME ASC;",
                                   dtStart.ToString("yyyyMMddHHmmssfff"), dtEnd.ToString("yyyyMMddHHmmssfff"), nMaxCount, strTableName);

                helper.SelectToDataTable(query, dt);

                if (dt.Rows.Count == 0) return null;

                LogInfo = new DB_LOG_INFO[dt.Rows.Count];

                DataRow dr = null;
                if (dt.Rows.Count > 0)
                {
                    for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                    {
                        dr = dt.Rows[nCnt];
                        LogInfo[nCnt] = new DB_LOG_INFO();

                        LogInfo[nCnt].LOG_TIME = dr.ItemArray[0].ToString();
                        LogInfo[nCnt].LOG_DATA = dr.ItemArray[1].ToString();
                    }
                }
            }

            return LogInfo;
     */

