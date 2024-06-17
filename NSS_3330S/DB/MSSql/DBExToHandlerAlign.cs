using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S
{
    /// <summary>
    /// [2022.04.06. kmlee] 추가
    /// PreAlign 시 Vision에서 핸들러로 넘겨주는 값
    /// Dicing 부의 Pre Align에 사용
    /// </summary>
    public class DBExToHandlerPreAlign : DBExBase
    {
        bool _isOpen;

        public DBExToHandlerPreAlign()
        {
            TABLE_NAME = "ToHandlerAlign";

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE {0}(");
            sb.AppendFormat("  {0} INT NOT NULL UNIQUE CLUSTERED", DBDF.E_TO_HANDLER_ALIGN.NO);
            sb.AppendFormat(", {0} VARCHAR(100) NOT NULL DEFAULT ''", DBDF.E_TO_HANDLER_ALIGN.RESULT_1);
            sb.AppendFormat(", {0} VARCHAR(100) NOT NULL DEFAULT ''", DBDF.E_TO_HANDLER_ALIGN.RESULT_2);
            sb.AppendFormat(", {0} VARCHAR(100) NOT NULL DEFAULT ''", DBDF.E_TO_HANDLER_ALIGN.CODE);
            sb.Append(")");

            strQueryCreate = sb.ToString();

            _isOpen = Open(dbInfo);
            if (_isOpen == false) return;
#if DBServer
            Create();
            CreateData();
#endif
        }

        /// <summary>
        /// 아이템 생성
        /// </summary>
        void CreateData()
        {
            if (GetCount() == 0)
            {
                Insert(1);
            }
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="nNo"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool Insert(int nNo, string strResult1 = "", string strResult2 = "", string strCode = "")
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            int nCount = GetCount(" WHERE {0}='{1}'", DBDF.E_TO_HANDLER_ALIGN.NO, nNo);
            string query = "";

            if (nCount > 0)
            {
                return false;
            }

            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //insert
                query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}) VALUES(?, ?, ?, ?)", TABLE_NAME, 
                    DBDF.E_TO_HANDLER_ALIGN.NO,
                    DBDF.E_TO_HANDLER_ALIGN.RESULT_1,
                    DBDF.E_TO_HANDLER_ALIGN.RESULT_2,
                    DBDF.E_TO_HANDLER_ALIGN.CODE
                    );
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.NO), nNo);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.RESULT_1), strResult1);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.RESULT_2), strResult2);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.CODE), strCode);
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

        [Obsolete("Vision에서 업데이트", true)]
        /// <summary>
        /// 값 업데이트
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool Update(int nSearchNo = 1, string strResult1 = "", string strResult2 = "", string strCode = "")
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=?, {2}=?, {3}=? WHERE {4}={5}", TABLE_NAME, 
                    DBDF.E_TO_HANDLER_ALIGN.RESULT_1,
                    DBDF.E_TO_HANDLER_ALIGN.RESULT_2,
                    DBDF.E_TO_HANDLER_ALIGN.CODE,
                    DBDF.E_TO_HANDLER_ALIGN.NO,
                    nSearchNo);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.RESULT_1), strResult1);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.RESULT_2), strResult2);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.CODE), strCode);

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
        /// 값 가져오기
        /// </summary>
        /// <param name="strAddOption"></param>
        /// <returns></returns>
        public string[] Select(int nSearchNo = 1)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = string.Format("SELECT {1}, {2}, {3} FROM {0} WHERE {4}={5}", TABLE_NAME,
                DBDF.E_TO_HANDLER_ALIGN.RESULT_1,
                DBDF.E_TO_HANDLER_ALIGN.RESULT_2,
                DBDF.E_TO_HANDLER_ALIGN.CODE,
                DBDF.E_TO_HANDLER_ALIGN.NO,
                nSearchNo);

            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTable(query, dt);

                string[] values = null;

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];

                    values = new string[dr.ItemArray.Length];
                    for (int nCol = 0; nCol < dr.ItemArray.Length; nCol++)
                    {
                        values[nCol] = (string)dr.ItemArray[nCol];
                    }
                }

                dt.Dispose();

                return values;
            }

            return null;
        }

        public string[] SelectResult(int nItem, int nSearchNo = 1)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            //string query = string.Format("SELECT {1}, {2}, {3} FROM {0} WHERE {4}={5}", TABLE_NAME,
            //    DBDF.E_TO_HANDLER_ALIGN.RESULT_1,
            //    DBDF.E_TO_HANDLER_ALIGN.RESULT_2,
            //    DBDF.E_TO_HANDLER_ALIGN.CODE,
            //    DBDF.E_TO_HANDLER_ALIGN.NO,
            //    nSearchNo);

            string queryWhere = string.Format(" WHERE {0}={1}", DBDF.E_TO_HANDLER_ALIGN.NO,
                nSearchNo);

            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTable(queryWhere, dt);

                string[] values = null;

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];

                    values = dr.ItemArray[nItem].ToString().Split(',');
                }

                dt.Dispose();

                return values;
            }

            return null;
        }
        public bool InitDataPreAlign(int nSearchNo = 1, string strResult1 = "", string strResult2 = "")
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=?, {2}=? WHERE {3}={4}", TABLE_NAME,
                    DBDF.E_TO_HANDLER_ALIGN.RESULT_1,
                    DBDF.E_TO_HANDLER_ALIGN.RESULT_2,
                    DBDF.E_TO_HANDLER_ALIGN.NO,
                    nSearchNo);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.RESULT_1), strResult1);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.RESULT_2), strResult2);

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

        public bool InitDataBarcode(int nSearchNo = 1, string strCode = "")
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=? WHERE {2}={3}", TABLE_NAME,
                    DBDF.E_TO_HANDLER_ALIGN.CODE,
                    DBDF.E_TO_HANDLER_ALIGN.NO,
                    nSearchNo);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_ALIGN.CODE), strCode);

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
        public bool ClearData()
        {
            return DeleteAll();
        }
    }
}
