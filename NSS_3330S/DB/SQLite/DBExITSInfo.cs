using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using SQLiteDB;

namespace NSS_3330S
{
    public class DBExITSInfo : SQLiteBase
    {
        public string strTableName = "PROC_INFO_ITS";
        public int nRowCnt = 0;

        public DBExITSInfo()
        {
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                LotNumber VARCHAR(100) NOT NULL DEFAULT '',
                                SHIPTO VARCHAR(100) NOT NULL DEFAULT '',
                                StripID VARCHAR(100) NOT NULL DEFAULT '', 
                                StripX VARCHAR(100) NOT NULL DEFAULT '',
                                StripY VARCHAR(100) NOT NULL DEFAULT '', 
                                XOUT_X VARCHAR(100) NOT NULL DEFAULT '',
                                XOUT_Y VARCHAR(100) NOT NULL DEFAULT '',
                                LANGLE VARCHAR(100) NOT NULL DEFAULT '',
                                Lot_Id VARCHAR(100) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();


            if (!CheckIfColumnExists(TABLE_NAME, "Lot_Id"))
            {
                AddColumn(TABLE_NAME, "Lot_Id VARCHAR(100) NOT NULL DEFAULT ''");
            }
        }

        #region 내부 함수
        #region INSERT
        /// <summary>
        /// 새로운 레코드를 추가합니다.
        /// </summary>
        /// <param name="Info">스트립 정보</param>
        /// <param name="Intime">시간</param>
        private bool Enqueue_InsertDB(string sLotId, DataTable dt)
        {
            bool bRet = true;  
            try
            {
                if (dt == null) return false;

                string strQuery = "";
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("INSERT INTO {0} VALUES", TABLE_NAME);

                DataRow dr;

                for (int nRow = 0; nRow < dt.Rows.Count; nRow++)
                {
                    dr = dt.Rows[nRow];

                    sb.Append("(");
                    for (int nCol = 0; nCol < dr.ItemArray.Length; nCol++)
                    {
                        sb.AppendFormat("'{0}', ", dr.ItemArray[nCol]);
                    }

                    sb.AppendFormat("'{0}'), ", sLotId);
                }

                int nSubLen = sb.ToString().Length - 2;
                if (nSubLen < 0)
                    nSubLen = 0;

                strQuery = sb.ToString().Substring(0, nSubLen);
                DBQueue.Enqueue(strQuery);

            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }
        #endregion

        #region SELECT
        private Point[] Enqueue_SelectXOUT(string strStripId)
        {

            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                Point[] XOutLoc = new Point[0];
                int nX, nY;
                using (var transaction = SQConn.BeginTransaction())
                {
                    //select 할 command객체 생성
                    using (SQLiteCommand sqCommend = new SQLiteCommand(SQConn))
                    {
                        if (sqliteLock.IsReadLockHeld)
                            sqliteLock.ExitReadLock();

                        sqliteLock.EnterReadLock();
                        sqCommend.CommandType = CommandType.Text;
                        //select query
                        sqCommend.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", TABLE_NAME, ITS.ColName.StripID.ToString(), strStripId);

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);

                        DataRow dr = null;
                        if (dt.Rows.Count > 0)
                        {
                            XOutLoc = new Point[dt.Rows.Count];
                            for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                            {
                                dr = dt.Rows[nCnt];

                                if (!int.TryParse(dr.ItemArray[(int)ITS.ColName.XOUT_X].ToString(), out nX)) nX = -1;
                                if (!int.TryParse(dr.ItemArray[(int)ITS.ColName.XOUT_Y].ToString(), out nY)) nY = -1;

                                XOutLoc[nCnt].X = nX;
                                XOutLoc[nCnt].Y = nY;
                            }
                        }
                        sqliteLock.ExitReadLock();

                        //사용헀던 객체 삭제
                        adapter.Dispose();
                        sqCommend.Dispose();
                        transaction.Commit();
                        SQConn.Close();
                        SQConn.Dispose();
                    }
                }
                return XOutLoc;
            }
        }

        /// <summary>
        /// 검색한 스트립의 첫번째 Col, Row정보를 반환합니다.
        /// </summary>
        /// <param name="strStripId"></param>
        /// <returns>220528 bool형에서 int형으로 수정</returns>
        private int Enqueue_SelectCol(string strStripId, ref int nX, ref int nY )
        {
            int nRet = FNC.SUCCESS;

            try
            {
                using (SQLiteConnection SQConn = new SQLiteConnection())
                {
                    SQConn.ConnectionString = connecString;
                    SQConn.Open();
                    DataTable dt = new DataTable();

                    using (var transaction = SQConn.BeginTransaction())
                    {
                        //select 할 command객체 생성
                        using (SQLiteCommand sqCommend = new SQLiteCommand(SQConn))
                        {
                            if (sqliteLock.IsReadLockHeld)
                                sqliteLock.ExitReadLock();

                            sqliteLock.EnterReadLock();
                            sqCommend.CommandType = CommandType.Text;
                            //select query
                            sqCommend.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", TABLE_NAME, ITS.ColName.StripID.ToString(), strStripId);

                            //데이터를 받아올 adapter 생성
                            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                            //datatable 생성하고 그 테이블에 데이터를 받아온다.
                            adapter.Fill(dt);

                            DataRow dr = null;
                            if (dt.Rows.Count > 0)
                            {
                                dr = dt.Rows[0];

                                if (!int.TryParse(dr.ItemArray[(int)ITS.ColName.StripX].ToString(), out nX)) nX = -1;
                                if (!int.TryParse(dr.ItemArray[(int)ITS.ColName.StripY].ToString(), out nY)) nY = -1;

                                if (nX > 0 && nY > 0)
                                    nRet = FNC.SUCCESS;
                                else
                                    nRet = (int)ERDF.E_ITS_ROW_COL_CASTING_ERROR;
                            }
                            else
                            {//검색됨 정보가 없음

                                nX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;
                                nY = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;

                                //하부 비전 사용 안할 시 알람
                                //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_USE].bOptionUse == false)
                                //    nRet = (int)ERDF.E_THERE_IS_NO_ITS_INFORMATION;
                            }

                            
                            sqliteLock.ExitReadLock();

                            //사용헀던 객체 삭제
                            adapter.Dispose();
                            sqCommend.Dispose();
                            transaction.Commit();
                            SQConn.Close();
                            SQConn.Dispose();
                        }
                    }
                    
                }
            }
            catch (Exception)
            {
                nRet = (int)ERDF.E_THERE_IS_NO_ITS_INFORMATION;
            }

            return nRet;
            
        }

        private DataTable Enqueue_SelectItsInfo(string strStripId)
        {
            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                using (var transaction = SQConn.BeginTransaction())
                {
                    //select 할 command객체 생성
                    using (SQLiteCommand sqCommend = new SQLiteCommand(SQConn))
                    {
                        sqliteLock.EnterReadLock();
                        sqCommend.CommandType = CommandType.Text;

                        if(strStripId == "")
                        {
                            //select query
                            sqCommend.CommandText = string.Format("SELECT * FROM {0}", TABLE_NAME);
                        }
                        else
                        {
                            //select query
                            sqCommend.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", TABLE_NAME, ITS_LOG.ColName.StripID.ToString(), strStripId);
                        }
                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);

                        sqliteLock.ExitReadLock();

                        //사용헀던 객체 삭제
                        adapter.Dispose();
                        sqCommend.Dispose();
                        transaction.Commit();
                        SQConn.Close();
                        SQConn.Dispose();
                    }
                }
                return dt;
            }
        }
        #endregion

        #region UPDATE
        #endregion

        #region DELETE
        /// <summary>
        /// 지정 개수 만큼만 남겨두고 나머지 레코드는 삭제합니다.
        /// </summary>
        /// <param name="nLimitCount">남길 레코드의 개수</param>
        /// <param name="eSortProp">내림차순 정렬 시 기준이 되는 ColName</param>
        private void Enqueue_DeleteTopN(int nLimitCount, StripProperties.ColName eSortProp = StripProperties.ColName.InTime)
        {
            string strQuery = "";
            strQuery = string.Format("DELETE FROM {0} WHERE {1} NOT IN ( SELECT * FROM ( SELECT {1} FROM {0} ORDER BY {1} DESC) LIMIT {2})", TABLE_NAME, eSortProp.ToString(), nLimitCount);
            DBQueue.Enqueue(strQuery);
        }

        /// <summary>
        /// 일치하는 스트립 아이디가 존재 할 시 해당 레코드를 삭제합니다.
        /// </summary>
        /// <param name="strStripId">스트립 아이디</param>
        private void Enqueue_DeleteDB(string strStripId)
        {
            DBQueue.Enqueue("DELETE FROM " + TABLE_NAME + " WHERE " + StripProperties.ColName.StripId.ToString() + "='" + strStripId + "'");
        }

        /// <summary>
        /// 일치하는 스트립 아이디가 존재 할 시 해당 레코드를 삭제합니다.
        /// </summary>
        /// <param name="strStripId">스트립 아이디</param>
        private void Enqueue_DeleteAll()
        {
            DBQueue.Enqueue("DELETE FROM " + TABLE_NAME);
        }
        #endregion
        #endregion

        #region 외부 함수
        /// <summary>
        /// 모든 XOUT정보를 삭제합니다.
        /// </summary>
        /// <returns></returns>
        public bool DeleteXOutInfo()
        {
            bool bRet = true;
            try
            {
                Enqueue_DeleteAll();
            }
            catch (Exception)
            {

            }

            return bRet;
        }
        public DataTable SelectInfo(string strStripId)
        {
            DataTable dt = new DataTable();
            try
            {
                dt = Enqueue_SelectItsInfo(strStripId);
            }
            catch (Exception)
            {
            }

            return dt;
        }

        public Point[] SelectXOutInfo(string StripId)
        {
            Point[] XOutLoc = null;

            try
            {
                XOutLoc = Enqueue_SelectXOUT(StripId);
            }
            catch (Exception)
            {
                XOutLoc = null;
            }

            return XOutLoc;
        }


        /// <summary>
        /// 220512 pjh
        /// ITS등록 정보와 현재 설비의 설정되어있는 Unit X,Y Count가 일치하는지 확인한다.
        /// </summary>
        /// <param name="StripId"></param>
        /// <returns>220528 bool형에서 int형으로 수정</returns>
        public int isItsInfoMatchRecipeInfo(string StripId)
        {
            int nRet = FNC.SUCCESS;

            try
            {
                int nX = 0;
                int nY = 0;

                //스트립 아이디로 검색하여 ITS에 등록되어있는 X, Y의 개수를 알아낸다.
                nRet = Enqueue_SelectCol(StripId, ref nX, ref nY);

                if (nRet == FNC.SUCCESS)
                {
                    //X, Y둘 중 하나라도 현재 레시피 값과 다르다면 알람
                    // [2022.05.26.kmlee] X와 Y가 반대
                    if (nY != RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX || 
                        nX != RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY)
                        nRet = (int)ERDF.E_ITS_ROW_COL_INFO_IS_NOT_MATCH_EQ_SETTING;
                }
            }
            catch (Exception)
            {
                nRet = (int)ERDF.E_ITS_ROW_COL_CASTING_ERROR;
            }

            return nRet;
        }

        public bool isExsitStrip(string strStripId)
        {
            bool bRet = true;
            try
            {
                Point[] XOutLoc = null;

                XOutLoc = Enqueue_SelectXOUT(strStripId);

                if (XOutLoc == null)
                {
                    bRet = false;
                }
                else
                {
                    if (XOutLoc.Length == 0)
                    {
                        bRet = false;
                    }
                }
            }
            catch (Exception)
            {

            }

            return bRet;
        }


        /// <summary>
        ///등록된 ITS정보가 있는지 확인 
		///220609 LOT정보 조회
        /// </summary>
        /// <returns></returns>
        public bool isExsitInfo(ref string strItsId, ref string strLotId, ref int nQty)
        {
            bool bRet = true;
            try
            {
                DataTable ItsInfo = null;

                ItsInfo = Enqueue_SelectItsInfo("");

                if (ItsInfo == null)
                {
                     return bRet = false;
                }
                else
                {
                    if (ItsInfo.Rows.Count == 0)
                    {
                        return bRet = false;
                    }
                }

                strItsId = ItsInfo.Rows[0].ItemArray[0].ToString();
                strLotId = ItsInfo.Rows[0].ItemArray[8].ToString();
                nQty = ItsInfo.Rows.Count;
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        public bool InsertXOutInfo(string sLotId, DataTable dt)
        {
            bool bRet = true;
            try
            {
                bRet = Enqueue_InsertDB(sLotId, dt);
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        #endregion
    }
}
