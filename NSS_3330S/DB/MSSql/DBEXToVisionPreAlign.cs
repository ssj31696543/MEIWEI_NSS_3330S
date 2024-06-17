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
    /// 핸들러는 Align 검사 진행 시 해당 테이블에 검사 모드를 업데이트 후 PIO로 인터페이스를 진행한다
    /// Dicing 부의 Pre Align에 사용
    /// </summary>
    public class DBEXToVisionPreAlign : DBExBase
    {
        bool _isOpen;

        public DBEXToVisionPreAlign()
        {
            TABLE_NAME = "ToVisionAlign";

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE {0}(");
            sb.AppendFormat("  {0} INT NOT NULL UNIQUE CLUSTERED", DBDF.E_TO_VISION_ALIGN.NO);
            sb.AppendFormat(", {0} VARCHAR(10) NOT NULL DEFAULT '1'", DBDF.E_TO_VISION_ALIGN.INSP_MODE);
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
                Insert(1, "1");
            }
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="nNo"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool Insert(int nNo, string strValue)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";

            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //insert
                query = string.Format("INSERT INTO {0} ({1}, {2}) VALUES(?, ?)", TABLE_NAME, DBDF.E_TO_VISION_ALIGN.NO, DBDF.E_TO_VISION_ALIGN.INSP_MODE);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_VISION_ALIGN.NO), nNo);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_VISION_ALIGN.INSP_MODE), strValue);
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
        /// 값 업데이트
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool Update(int nMode, int nRowNo = 1)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=? WHERE {2}='{3}'", TABLE_NAME, DBDF.E_TO_VISION_ALIGN.INSP_MODE, DBDF.E_TO_VISION_ALIGN.NO, nRowNo);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_VISION_ALIGN.INSP_MODE), nMode.ToString());

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
        public string Select(string strItem)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = string.Format("SELECT {1} FROM {0}", TABLE_NAME, DBDF.E_TO_VISION_ALIGN.INSP_MODE);
            string strValue = "";
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTable(query, dt);

                for (int nRow = 0; nRow < dt.Rows.Count; nRow++)
                {
                    DataRow dr = dt.Rows[nRow];

                    strValue = (string)dr.ItemArray[0];
                }

                dt.Dispose();
            }

            return strValue;
        }

        public bool ClearData()
        {
            return DeleteAll();
        }
    }
}
