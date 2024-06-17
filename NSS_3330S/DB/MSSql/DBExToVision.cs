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
    /// 핸들러에서 비전에게 공유할 내용들을 한 테이블로 관리
    /// </summary>
    public class DBExToVision : DBExBase
    {
        bool _isOpen;

        public DBExToVision()
        {
            TABLE_NAME = "ToVision";

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE {0}(");
            sb.AppendFormat("  {0} INT NOT NULL UNIQUE CLUSTERED", DBDF.E_TO_VISION.NO);
            sb.AppendFormat(", {0} VARCHAR(100) NOT NULL DEFAULT ''", DBDF.E_TO_VISION.ITEM);
            sb.AppendFormat(", {0} VARCHAR(100) NOT NULL DEFAULT ''", DBDF.E_TO_VISION.VALUE);
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
            Insert(1, "MapBlock1", "");
            Insert(2, "MapBlock2", "");
            Insert(3, "TrainName", "");
            Insert(4, "Barcode", "");
            Insert(5, "LOT_ID", "");
            Insert(6, "UnitSize", "");
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="nNo"></param>
        /// <param name="strItem"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool Insert(int nNo, string strItem, string strValue)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);
            int nCount = GetCount(" WHERE {0}='{1}'", DBDF.E_TO_VISION.ITEM, strItem);
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
                    query = string.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES(?, ?, ?)", TABLE_NAME, DBDF.E_TO_VISION.NO, DBDF.E_TO_VISION.ITEM, DBDF.E_TO_VISION.VALUE);
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_VISION.NO), nNo);
                    cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_VISION.ITEM), strItem);
                    cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_VISION.VALUE), strValue);
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
        /// 값 업데이트
        /// </summary>
        /// <param name="strItem"></param>
        /// <param name="strValue"></param>
        /// <param name="bUpdateExistsIndex"></param>
        /// <returns></returns>
        public bool Update(string strItem, string strValue)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=? WHERE {2}='{3}'", TABLE_NAME, DBDF.E_TO_VISION.VALUE, DBDF.E_TO_VISION.ITEM, strItem);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_VISION.VALUE), strValue);

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

        public bool UpdateMapBlock1(string strValue)
        {
            return Update("MapBlock1", strValue);
        }

        public bool UpdateMapBlock2(string strValue)
        {
            return Update("MapBlock2", strValue);
        }

        public bool UpdateTrainName(string strValue)
        {
            return Update("TrainName", strValue);
        }

        public bool UpdateBarcode(string strValue)
        {
            return Update("Barcode", strValue);
        }

        public bool UpdateLotID(string strValue)
        {
            return Update("LOT_ID", strValue);
        }

        public bool UpdateUnitSize(string strValue)
        {
            return Update("UnitSize", strValue);
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

            string query = string.Format("SELECT {3} FROM {0} WHERE {1}='{2}'", TABLE_NAME, DBDF.E_TO_VISION.ITEM, strItem, DBDF.E_TO_VISION.VALUE);
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
