using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace NSS_3330S
{
    public class DBExFromSaw : DBExBase
    {
        bool isOpen;

        public DBExFromSaw()
        {
            TABLE_NAME = "FROM_SAW";

            strQueryCreate = "CREATE TABLE {0} (NO INT NOT NULL UNIQUE CLUSTERED, "
                                             + "X VARCHAR(100), "
                                             + "Y VARCHAR(100), "
                                             + "T VARCHAR(100))";

            isOpen = Open(dbInfo);
            if (isOpen == false) return;
#if DBServer
            Create();
            CreateData();
#endif
        }

        /// <summary>
        /// 테이블에 Row가 하나도 for문 Count만큼 생성한다.
        /// </summary>
        void CreateData()
        {
            if (GetCount(string.Format(" WHERE NO={0}", 1)) == 0)
            {
                Insert(1, "0.000", "0.000", "0.000", true);             
            }
        }


        public bool Insert(int nRow, string strValueX, string strValueY, string strValueT, bool bUpdateExistsIndex = false)
        {
            if (!isOpen) return false;

            int nCount = GetCount(string.Format(" WHERE NO='{0}'", nRow));
            string query = "";

            try
            {
                if (nCount > 0)
                {
                    return false;
                }
                else
                {
                    OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                    //insert
                    query = string.Format("INSERT INTO {0} (NO, X, Y, T) VALUES(?, ?, ?, ?)", TABLE_NAME);
                          
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("NO", nRow);
                    cmd.Parameters.AddWithValue("X", strValueX);
                    cmd.Parameters.AddWithValue("Y", strValueY);
                    cmd.Parameters.AddWithValue("T", strValueT);


                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    cmd = null;
                }
            }
            catch (OleDbException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 모든 아이템 업데이트
        /// </summary>
        /// <param name="strID"></param>
        /// <param name="strStartIdx"></param>
        /// <param name="strInspDir"></param>
        /// <param name="nHead"></param>
        /// <param name="strValue"></param>
        /// <param name="bUpdateExistsIndex"></param>
        /// <returns></returns>
        public bool UpdateAllData(string strValueX, string strValueY, string strValueT, bool bUpdateExistsIndex = false)
        {
             if (!isOpen) return false;

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                if (!bUpdateExistsIndex) return false;

                //update MCGLOBAL set VALUE = 0 where ID >= 500 AND ID < 600;

                //update
                query = string.Format("UPDATE {0} SET X='{1}', Y='{2}', T='{3}' WHERE NO={4}", TABLE_NAME, strValueX, strValueY, strValueT, 1);
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
            }
            catch (OleDbException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 개별 아이템 업데이트
        /// </summary>
        /// <param name="strItem"></param>
        /// <param name="nRow"></param>
        /// <param name="strValue"></param>
        /// <param name="bUpdateExistsIndex"></param>
        /// <returns></returns>
        public bool Update(DBDF.MAIN_ITEMS Item, int nRow, string strValue, bool bUpdateExistsIndex = false)
        {
            if (!isOpen) return false;

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                if (!bUpdateExistsIndex) return false;

                //update MCGLOBAL set VALUE = 0 where ID >= 500 AND ID < 600;

                //update
                query = string.Format("UPDATE {0} SET {1}=? WHERE {2}={3}", TABLE_NAME, Item.ToString(), DBDF.MAIN_ITEMS.NO.ToString(), nRow);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", Item.ToString()), strValue);

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
            }
            catch (OleDbException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public string GetSqlUpdateData(DBDF.MAIN_ITEMS Item, int nRow, string value)
        {
            string query = "";

            query = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}';", TABLE_NAME, Item.ToString(), value, DBDF.MAIN_ITEMS.NO.ToString(), nRow);

            return query;
        }

        /// <summary>
        /// 검사 결과 값을 가져온다.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="nRow"></param>
        /// <param name="strAddOption"></param>
        /// <returns></returns>
        public string[] SelectResult(string strAddOption = "")
        {
            if (!isOpen) return null;

            string query = string.Format("SELECT * FROM {0} {1}", TABLE_NAME, strAddOption);
            string[] values = new string[3];
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTable(query, dt);
                DataRow dr = dt.Rows[0];

                if (dr == null) return values;

                values[0] = dr.ItemArray[1].ToString();
                values[1] = dr.ItemArray[2].ToString();
                values[2] = dr.ItemArray[3].ToString();

                dt.Dispose();
            }
            return values;
        }

        /// <summary>
        /// Item의 nRow번째를 가져온다.
        /// </summary>
        /// <param name="nRow"></param>
        /// <param name="strAddOption"></param>
        /// <returns></returns>
        public string Select(int nCol, int nRow)
        {
            if (!isOpen) return null;

            string query = string.Format("SELECT * FROM {0}", TABLE_NAME);

            DataTable dt = new DataTable();
            if (!SelectToDataTable(query, dt))
                return null;

            if (dt.Rows.Count == 0)
                return null;

            string[] values = new string[1];
            DataRow dr = dt.Rows[nRow];
            if (dr.ItemArray[nCol] != System.DBNull.Value) values[0] = (string)dr.ItemArray[nCol].ToString();
            else values[0] = null;
            dt.Dispose();
            return values[0];
        }

        public bool ClearData()
        {
            return DeleteAll();
        }

    }
}
