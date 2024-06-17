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
    /// Vision에서 핸들러로 넘겨주는 값
    /// Sorter 부의 하부 Inspection에 사용
    /// </summary>
    public class DBExToHandlerBtmResult : DBExBase
    {
        bool _isOpen;

        /// <summary>
        /// Picker Head 번호
        /// 1 또는 2
        /// </summary>
        /// <param name="nHead_stx1"></param>
        public DBExToHandlerBtmResult(int nHead_stx1)
        {
            TABLE_NAME = string.Format("ToHandlerBtmResult_{0}", nHead_stx1);

            StringBuilder sb = new StringBuilder();

            sb.Append("CREATE TABLE {0} (NO INT NOT NULL UNIQUE CLUSTERED");
            for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
            {
                sb.AppendFormat(", PAD{0} VARCHAR(100) NOT NULL DEFAULT ''", nCnt + 1);
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
            Insert(1);
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
                for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
                {
                    sb.AppendFormat("{0} PAD{1}=?", nCnt == 0 ? "" : ",", nCnt + 1);
                }

                query = sb.ToString();
                cmd.CommandText = query;

                for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
                {
                    cmd.Parameters.AddWithValue(string.Format("@PAD{0}", nCnt + 1), strData[nCnt]);
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
        public bool Update(int nChipNo, string strValue)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=?", TABLE_NAME, DBDF.E_TO_VISION_ALIGN.INSP_MODE);
                cmd.CommandText = query;

                cmd.Parameters.AddWithValue(string.Format("@PAD{0}", nChipNo + 1), strValue);

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
        public string[] SelectAllChip()
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            StringBuilder sbColumn = new StringBuilder();
            //sbColumn.Append("NO");
            for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
            {
                sbColumn.AppendFormat("{0} PAD{1}", nCnt == 0 ? "" : ",", nCnt + 1);
            }

            string query = string.Format("SELECT {1} FROM {0}", TABLE_NAME, sbColumn.ToString());
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                //SelectToDataTable(query, dt);
                SelectToDataTableToQuery(dt, query);

                string[] values = new string[CFG_DF.MAX_PICKER_PAD_CNT];

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];

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
     
        public bool InitData()
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("UPDATE {0} SET", TABLE_NAME);
                for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
                {
                    sb.AppendFormat("{0} PAD{1}=?", nCnt == 0 ? "" : ",", nCnt + 1);
                }

                query = sb.ToString();
                cmd.CommandText = query;

                for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
                {
                    cmd.Parameters.AddWithValue(string.Format("@PAD{0}", nCnt + 1), "");
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
