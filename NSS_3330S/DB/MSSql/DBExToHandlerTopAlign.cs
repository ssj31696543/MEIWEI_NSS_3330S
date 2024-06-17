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
    /// Align 시 Vision에서 핸들러로 넘겨주는 값
    /// Sorter 부의 Top Align에 사용
    /// </summary>
    public class DBExToHandlerTopAlign : DBExBase
    {
        bool _isOpen;

        public DBExToHandlerTopAlign()
        {
            TABLE_NAME = "ToHandlerTopAlign";

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE {0}(");
            sb.AppendFormat("  {0} INT NOT NULL UNIQUE CLUSTERED", DBDF.E_TO_HANDLER_TOP_ALIGN.NO);
            sb.AppendFormat(", {0} VARCHAR(100) NOT NULL DEFAULT ''", DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_1);
            sb.AppendFormat(", {0} VARCHAR(100) NOT NULL DEFAULT ''", DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_2);
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
        /// <param name="strResult1"></param>
        /// <param name="strResult2"></param>
        /// <returns></returns>
        public bool Insert(int nNo, string strResult1 = "", string strResult2 = "")
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            int nCount = GetCount(" WHERE {0}='{1}'", DBDF.E_TO_HANDLER_TOP_ALIGN.NO, nNo);
            string query = "";

            if (nCount > 0)
            {
                return false;
            }

            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //insert
                query = string.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES(?, ?, ?)", TABLE_NAME,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.NO,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_1,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_2
                    );
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_TOP_ALIGN.NO), nNo);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_1), strResult1);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_2), strResult2);
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
        /// <param name="nSearchNo"></param>
        /// <param name="strResult1"></param>
        /// <param name="strResult2"></param>
        /// <param name="strCode"></param>
        /// <returns></returns>
        [Obsolete("Vision에서 업데이트", true)]
        public bool Update(int nSearchNo = 1, string strResult1 = "", string strResult2 = "", string strCode = "")
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=?, {2}=? WHERE {3}={4}", TABLE_NAME,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_1,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_2,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.NO,
                    nSearchNo);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_1), strResult1);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_2), strResult2);

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
        /// <param name="nSearchNo"></param>
        /// <returns></returns>
        public tagRetAlignData[] Select(int nSearchNo = 1)
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            //string query = string.Format("SELECT {1}, {2} FROM {0} WHERE {3}={4}", TABLE_NAME,
            //    DBDF.E_TO_HANDLER_ALIGN.RESULT_1,
            //    DBDF.E_TO_HANDLER_ALIGN.RESULT_2,
            //    DBDF.E_TO_HANDLER_ALIGN.NO,
            //    nSearchNo);

            tagRetAlignData[] stRetAlignData = new tagRetAlignData[2];
            for (int nCnt = 0; nCnt < stRetAlignData.Length; nCnt++)
            {
                stRetAlignData[nCnt] = new tagRetAlignData();
            }

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

                    values = new string[dr.ItemArray.Length];
                    for (int nCol = 0; nCol < dr.ItemArray.Length; nCol++)
                    {
                        if (nCol == 0)
                        {
                            values[nCol] = ((int)dr.ItemArray[nCol]).ToString();
                        }
                        else
                        {
                            values[nCol] = (string)dr.ItemArray[nCol];
                        }
                    }

                    if (values.Length == 3)
                    {
                        bool bRet1 = stRetAlignData[0].SetData(values[1]);
                        bool bRet2 = stRetAlignData[1].SetData(values[2]);

                        if (bRet1 == false || 
                            bRet2 == false)
                        {
                            // 2022 08 25 홍은표
                            // Parsing Alarm이 발생하면
                            stRetAlignData = new tagRetAlignData[0];
                        }
                    }
					else
					{
						stRetAlignData = new tagRetAlignData[0];
					}
                }
				else
				{
					stRetAlignData = new tagRetAlignData[0];
				}
                dt.Dispose();

                return stRetAlignData;
            }

            // 2022 08 25 홍은표
            // null로 반환하면 문제 발생
            return new tagRetAlignData[0];
        }
        public bool InitData(int nSearchNo = 1, string strResult1 = "", string strResult2 = "", string strCode = "")
        {
            if (helper.CONNECTION.State != ConnectionState.Open)
                _isOpen = Open(dbInfo);

            string query = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(query, helper.CONNECTION);
                //update
                query = string.Format("UPDATE {0} SET {1}=?, {2}=? WHERE {3}={4}", TABLE_NAME,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_1,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_2,
                    DBDF.E_TO_HANDLER_TOP_ALIGN.NO,
                    nSearchNo);
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_1), strResult1);
                cmd.Parameters.AddWithValue(string.Format("@{0}", DBDF.E_TO_HANDLER_TOP_ALIGN.RESULT_2), strResult2);

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
