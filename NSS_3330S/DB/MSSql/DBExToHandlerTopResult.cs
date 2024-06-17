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
    /// Inspection 시 Vision에서 핸들러로 넘겨주는 값
    /// Sorter 부의 상부 Inspection에 사용
    /// </summary>
    public class DBExToHandlerTopResult : DBExBase
    {
        bool _isOpen;

        /// <summary>
        /// 스테이지 테이블 번호
        /// 1 또는 2
        /// </summary>
        /// <param name="nTableNo_stx1"></param>
        public DBExToHandlerTopResult(int nTableNo_stx1)
        {
            TABLE_NAME = string.Format("ToHandlerTopResult_{0}", nTableNo_stx1);

            StringBuilder sb = new StringBuilder();

            sb.Append("CREATE TABLE {0} (NO INT NOT NULL UNIQUE CLUSTERED");
            for (int nCnt = 0; nCnt < 100; nCnt++)
            {
                sb.AppendFormat(", COL{0} VARCHAR(100) NOT NULL DEFAULT ''", nCnt + 1);
            }
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
            int nCount = GetCount();
            bool bInsert = false;

            if (nCount == 0)
            {
                bInsert = true;
            }
            else if (nCount != 100)
            {
                ClearData();
                bInsert = true;
            }

            if (bInsert)
            {
                for (int nCnt = 0; nCnt < 100; nCnt++)
                {
                    Insert(nCnt + 1);
                }
            }
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="nNo"></param>
        /// <param name="strItem"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool Insert(int nNo)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            int nCount = GetCount(" WHERE NO='{0}'", nNo);
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
                    query = string.Format("INSERT INTO {0} (NO) VALUES(?)", TABLE_NAME);
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@NO", nNo);
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

        [Obsolete("Vision에서 업데이트", true)]
        /// <summary>
        /// 값 업데이트
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public bool Update(string[] strData)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("UPDATE {0} SET");
                for (int nCnt = 0; nCnt < 100; nCnt++)
                {
                    if (strData.Length >= nCnt)
                        break;

                    sb.AppendFormat("{0} COL{1}=?", nCnt == 0 ? "" : ",", nCnt + 1);
                }

                query = sb.ToString();
                cmd.CommandText = query;

                for (int nCnt = 0; nCnt < 100; nCnt++)
                {
                    if (strData.Length >= nCnt)
                        break;

                    cmd.Parameters.AddWithValue(string.Format("@COL{0}", nCnt + 1), strData[nCnt]);
                }

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
        /// <param name="strData"></param>
        /// <returns></returns>
        public bool Update(int nRowNo, int nColNo, string strValue)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string strColumnName = string.Format("COL{0}", nColNo + 1);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=? WHERE NO={2}", TABLE_NAME, strColumnName, nRowNo);
                cmd.CommandText = query;

                cmd.Parameters.AddWithValue(string.Format("@{0}", strColumnName), strValue);

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
        /// <param name="nMapColCnt"></param>
        /// <param name="nMapRowCnt"></param>
        /// <returns></returns>
        public string[,] SelectAllJudges(int nMapColCnt, int nMapRowCnt)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string[,] strValue = new string[nMapRowCnt, nMapColCnt];

            string query = "";
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GROUP1_USE].bOptionUse)
            {
                StringBuilder sbColumn = new StringBuilder();
                for (int nCnt = 0; nCnt < nMapColCnt; nCnt++)
                {
                    sbColumn.AppendFormat("{0} COL{1}", nCnt == 0 ? "" : ",", nCnt + 1);
                }

                query = string.Format("SELECT TOP {2} {1} FROM {0} ORDER BY NO ASC", TABLE_NAME, sbColumn.ToString(), nMapRowCnt);

            }
            else
            {
                //for (int nCnt = 0; nCnt < nMapColCnt + nMapRowCnt; nCnt++)
                //{
                //    sbColumn.AppendFormat("{0} COL{1}", nCnt == 0 ? "" : ",", nCnt + 1);
                //}

                //string query = string.Format("SELECT {1} FROM {0} WHERE NO = {2}", TABLE_NAME, sbColumn.ToString(), nMapColCnt + nMapRowCnt);

                 query = string.Format("SELECT TOP {2} {1} FROM {0} ORDER BY NO ASC", TABLE_NAME, "COL1", nMapRowCnt * nMapColCnt);
            }
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTableToQuery(dt, query);

                for (int nRow = 0; nRow < dt.Rows.Count; nRow++)
                {
                    DataRow dr = dt.Rows[nRow];

                    for (int nCol = 0; nCol < dr.ItemArray.Length; nCol++)
                    {
                        if (nCol >= nMapColCnt) break;

                        strValue[nRow, nCol] = (string)dr.ItemArray[nCol];
                    }
                }

                dt.Dispose();
            }

            return strValue;
        }

        public string SelectJudges(int nRowNo, int nColNo)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string strValue = "";

            StringBuilder sbColumn = new StringBuilder();
            sbColumn.AppendFormat("COL{0}", nColNo + 1);

            string query = string.Format("SELECT TOP {2} {1} FROM {0} WHERE NO={3} ORDER BY NO ASC", TABLE_NAME, sbColumn.ToString(), 1, nRowNo + 1);
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTable(query, dt);

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];

                    if (dr.ItemArray.Length > 0)
                    {
                        strValue = (string)dr.ItemArray[0];
                    }
                }

                dt.Dispose();
            }

            return strValue;
        }
        public bool InitData(int nMapColCnt, int nMapRowCnt)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            StringBuilder sbColumn = new StringBuilder();
            for (int nCnt = 0; nCnt < nMapColCnt; nCnt++)
            {
                sbColumn.AppendFormat("{0} COL{1}=?", nCnt == 0 ? "" : ",", nCnt + 1);
            }

            string query = "";
            try
            {

                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1} WHERE {2}<={3}", TABLE_NAME,
                    sbColumn,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.NO,
                    nMapRowCnt);
                cmd.CommandText = query;

                for (int nCnt = 0; nCnt < nMapColCnt; nCnt++)
                {
                    cmd.Parameters.AddWithValue(string.Format("@COL{0}", nCnt + 1), "");
                }

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
