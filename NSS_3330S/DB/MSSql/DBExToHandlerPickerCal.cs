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
    /// Picker Cal 시 사용. 추 후 협의
    /// </summary>
    public class DBExToHandlerPickerCal : DBExBase
    {
        bool _isOpen;

        public DBExToHandlerPickerCal()
        {
            TABLE_NAME = "ToHandlerPickerCal";

            strQueryCreate = "CREATE TABLE {0}" +
                string.Format("({0} INT NOT NULL UNIQUE CLUSTERED, {1} VARCHAR(100) NOT NULL DEFAULT '')",
                                DBDF.E_TO_HANDLER_PICKER_CAL.NO, DBDF.E_TO_HANDLER_PICKER_CAL.VALUE);

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
                Insert(1, "");
            }
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
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
                query = string.Format("INSERT INTO {0} ({1}, {2}) VALUES(?, ?)", TABLE_NAME, DBDF.E_TO_HANDLER_PICKER_CAL.NO, DBDF.E_TO_HANDLER_PICKER_CAL.VALUE);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_PICKER_CAL.NO), nNo);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_PICKER_CAL.VALUE), strValue);
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
        public bool Update(string strValue)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=?", TABLE_NAME, DBDF.E_TO_HANDLER_PICKER_CAL.VALUE);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_PICKER_CAL.VALUE), strValue);

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
        /// <returns></returns>
        public tagRetAlignData Select()
        {
            tagRetAlignData stRetAlignData = new tagRetAlignData();

            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = string.Format("SELECT {1} FROM {0}", TABLE_NAME, DBDF.E_TO_HANDLER_PICKER_CAL.VALUE);
            string strValue = "";
            DataTable dt = new DataTable();

            if (helper.CONNECTION.State == ConnectionState.Open)
            {
                SelectToDataTable(query, dt);

                for (int nRow = 0; nRow < dt.Rows.Count; nRow++)
                {
                    DataRow dr = dt.Rows[nRow];

                    strValue = (string)dr.ItemArray[1];

                    stRetAlignData.SetData(strValue);
                }

                dt.Dispose();
            }

            return stRetAlignData;
        }

        public bool ClearData()
        {
            return DeleteAll();
        }
    }
}
