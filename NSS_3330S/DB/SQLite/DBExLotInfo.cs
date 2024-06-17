using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using SQLiteDB;

namespace NSS_3330S
{
    public class DBExLotInfo : SQLiteBase
    {
        public string strTableName = "PROC_INFO_LOT";

        //HostLotInfo
        public DBExLotInfo()
        {
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(";
            for (int i = 0; i < (int)LotInfoPro.ColName.Max; i++)
            {
                strQueryCreate += ((LotInfoPro.ColName)i).ToString() + " VARCHAR(100) NOT NULL DEFAULT '',";
            }
            strQueryCreate = strQueryCreate.TrimEnd(',');
            strQueryCreate += ")";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();
        }

        #region 내부 함수

        #region INSERT
        /// <summary>
        /// 새로운 레코드를 추가합니다.
        /// </summary>
        /// <param name="Info">스트립 정보</param>
        /// <param name="Intime">시간</param>
        private void Enqueue_InsertDB(HostLotInfo Info)
        {
            //그리고 맨 앞에 추가
            DBQueue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("
                            + "'" + Info.LOT_ID + "', "
                            + "'" + Info.LOT_ID + "', "
                            + "'" + Info.LOT_TYPE+ "', "
                            + "'" + Info.SUB_QTY + "', "
                            + "'" + Info.PROD_TYPE + "', "
                            + "'" + Info.TOOL_NO + "', "
                            + "'" + Info.ITS_USAGE + "', "
                            + "'" + Info.ITSLOTID_IN + "', "
                            + "'" + Info.ITSLOTID_CT + "', "
                            + "'" + Info.UNITSIZE_X + "', "
                            + "'" + Info.UNITSIZE_Y + "', "
                            + "'" + Info.UNITUPPER + "', "
                            + "'" + Info.UNITLOWER + "', "
                            + "'" + Info.THICK + "', "
                            + "'" + Info.THICK_UPPER + "', "
                            + "'" + Info.THICK_LOWER + "', "
                            + "'" + Info.ABF + "', "
                            + "'" + Info.LANDPKG_X+ "', "
                            + "'" + Info.LANDPKGX_UPPER + "', "
                            + "'" + Info.LANDPKGX_LOWER + "', "
                            + "'" + Info.LANDPKG_Y + "', "
                            + "'" + Info.LANDPKGX_UPPER + "', "
                            + "'" + Info.LANDPKGY_LOWER + "')");
        }
        #endregion

        #region UPDATE
        private void Enqueue_UpdateDB(HostLotInfo Info)
        {
            DBQueue.Enqueue("UPDATE " + TABLE_NAME + " SET " +
                    LotInfoPro.ColName.Lot_id.ToString() + "='" + Info.LOT_ID + "', " +
                    LotInfoPro.ColName.Its_id.ToString() + "='" + Info.LOT_ID + "', " +
                    LotInfoPro.ColName.Lot_type.ToString() + "='" + Info.LOT_TYPE+ "', " +
                    LotInfoPro.ColName.Sub_qty.ToString() + "='" + Info.SUB_QTY + "', " +
                    LotInfoPro.ColName.prod_type.ToString() + "='" + Info.PROD_TYPE + "', " +
                    LotInfoPro.ColName.tool_no.ToString() + "='" + Info.TOOL_NO + "', " +
                    LotInfoPro.ColName.its_usage.ToString() + "='" + Info.ITS_USAGE+ "', " +
                    LotInfoPro.ColName.itslotid_in.ToString() + "='" + Info.ITSLOTID_IN + "', " +
                    LotInfoPro.ColName.itslotid_ct.ToString() + "='" + Info.ITSLOTID_CT + "', " +
                    LotInfoPro.ColName.unitsize_x.ToString() + "='" + Info.UNITSIZE_X + "', " +
                    LotInfoPro.ColName.unitsize_y.ToString() + "='" + Info.UNITSIZE_Y + "', " +
                    LotInfoPro.ColName.unitupper.ToString() + "='" + Info.UNITUPPER + "', " +
                    LotInfoPro.ColName.unitlower.ToString() + "='" + Info.UNITLOWER + "', " +
                    LotInfoPro.ColName.thick.ToString() + "='" + Info.THICK + "', " +
                    LotInfoPro.ColName.thick_upper.ToString() + "='" + Info.THICK_UPPER+ "', " +
                    LotInfoPro.ColName.thick_lower.ToString() + "='" + Info.THICK_LOWER + "'" +

                    LotInfoPro.ColName.abf.ToString() + "='" + Info.ABF + "'" +
                    LotInfoPro.ColName.landpkg_x.ToString() + "='" + Info.LANDPKG_X + "'" +
                    LotInfoPro.ColName.landpkgx_upper.ToString() + "='" + Info.LANDPKGX_UPPER + "'" +
                    LotInfoPro.ColName.landpkgx_lower.ToString() + "='" + Info.LANDPKGX_LOWER + "'" +
                    LotInfoPro.ColName.landpkg_y.ToString() + "='" + Info.LANDPKG_Y + "'" +
                    LotInfoPro.ColName.landpkgy_upper.ToString() + "='" + Info.LANDPKGY_UPPER + "'" +
                    LotInfoPro.ColName.landpkgy_lower.ToString() + "='" + Info.LANDPKGY_LOWER + "'" +
                    "WHERE " + LotInfoPro.ColName.Lot_id.ToString() + "='" + Info.LOT_ID + "'");
        }
        #endregion

        #region SELECT
        private BindingList<HostLotInfo> Enqueue_SelectStrip()
        {
            BindingList<HostLotInfo> Info = new BindingList<HostLotInfo>();

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
                            sqCommend.CommandText = string.Format("SELECT * FROM {0}", TABLE_NAME);

                            //데이터를 받아올 adapter 생성
                            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                            //datatable 생성하고 그 테이블에 데이터를 받아온다.
                            adapter.Fill(dt);

                            DataRow dr = null;
                            if (dt.Rows.Count > 0)
                            {
                                for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                                {
                                    dr = dt.Rows[nCnt];
                                    Info.Add(new HostLotInfo());

                                    Info[nCnt].LOT_ID = dr.ItemArray[0].ToString();
                                    Info[nCnt].LOT_ID = dr.ItemArray[1].ToString();
                                    Info[nCnt].LOT_TYPE = dr.ItemArray[2].ToString();
                                    Info[nCnt].SUB_QTY = dr.ItemArray[3].ToString();
                                    Info[nCnt].PROD_TYPE = dr.ItemArray[4].ToString();
                                    Info[nCnt].TOOL_NO = dr.ItemArray[5].ToString();
                                    Info[nCnt].ITS_USAGE = dr.ItemArray[6].ToString();
                                    Info[nCnt].ITSLOTID_IN = dr.ItemArray[7].ToString();
                                    Info[nCnt].ITSLOTID_CT = dr.ItemArray[8].ToString();
                                    Info[nCnt].UNITSIZE_X = dr.ItemArray[9].ToString();
                                    Info[nCnt].UNITSIZE_Y = dr.ItemArray[10].ToString();
                                    Info[nCnt].UNITUPPER = dr.ItemArray[11].ToString();
                                    Info[nCnt].UNITLOWER = dr.ItemArray[12].ToString();
                                    Info[nCnt].THICK = dr.ItemArray[13].ToString();
                                    Info[nCnt].THICK_UPPER = dr.ItemArray[14].ToString();
                                    Info[nCnt].THICK_LOWER = dr.ItemArray[15].ToString();

                                    Info[nCnt].ABF = dr.ItemArray[16].ToString();
                                    Info[nCnt].LANDPKG_X= dr.ItemArray[17].ToString();
                                    Info[nCnt].LANDPKGX_UPPER = dr.ItemArray[18].ToString();
                                    Info[nCnt].LANDPKGX_LOWER= dr.ItemArray[19].ToString();
                                    Info[nCnt].LANDPKG_Y = dr.ItemArray[20].ToString();
                                    Info[nCnt].LANDPKGY_UPPER = dr.ItemArray[21].ToString();
                                    Info[nCnt].LANDPKGY_LOWER = dr.ItemArray[22].ToString();
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
                }
            }
            catch (Exception)
            {
                Info = new BindingList<HostLotInfo>();
            }

            return Info;
        }
        #endregion

        #region DELETE
        private void Enqueue_DeleteAllDB()
        {
            DBQueue.Enqueue("DELETE FROM " + TABLE_NAME + "");
        }

        private void Enqueue_DeleteDB(string strLotId)
        {
            DBQueue.Enqueue("DELETE FROM " + TABLE_NAME + " WHERE " + LotInfoPro.ColName.Lot_id.ToString() + "='" + strLotId + "'");
        }
        #endregion
     
        #endregion

        #region 외부함수
        /// <summary>
        /// 새로운 Lot이 투입되었을 시 추가한다.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public bool InsertInfo(HostLotInfo Info)
        {
            bool bRet = true;

            try
            {
                Enqueue_InsertDB(Info);
            }
            catch (Exception)
            {

            }

            return bRet;
        }

        /// <summary>
        /// Lot의 정보가 변경되었을 시 정보를 갱신한다.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public bool UpdateInfo(BindingList<HostLotInfo> Info)
        {
            bool bRet = true;

            try
            {
                for (int i = 0; i < Info.Count; i++)
                {
                    Enqueue_UpdateDB(Info[i]);
                }
            }
            catch (Exception)
            {

            }

            return bRet;
        }


        /// <summary>
        /// Lot의 정보가 변경되었을 시 정보를 갱신한다.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public bool UpdateAllInfo(BindingList<HostLotInfo> Info)
        {
            bool bRet = true;

            try
            {
                //1. 모두 삭제
                //2. 모두 추가

                Enqueue_DeleteAllDB();

                for (int i = 0; i < Info.Count; i++)
                {
                    Enqueue_InsertDB(Info[i]);
                }
            }
            catch (Exception)
            {

            }

            return bRet;
        }

        /// <summary>
        /// DB에 있는 Lot의 정볼를 가져온다.
        /// </summary>
        /// <returns></returns>
        public BindingList<HostLotInfo> DownloadInfo()
        {
            BindingList<HostLotInfo> lstRet = new BindingList<HostLotInfo>();

            try
            {
                //모든 정보를 가져와 Lot에 넣는다.
                lstRet = Enqueue_SelectStrip();
            }
            catch (Exception)
            {

            }

            return lstRet;
        }

        /// <summary>
        /// Lot이 배출되었을 시 삭제한다.
        /// </summary>
        /// <param name="strLotId"></param>
        /// <returns></returns>
        public bool DeleteInfo(string strLotId)
        {
            bool bRet = true;

            try
            {
                Enqueue_DeleteDB(strLotId);
            }
            catch (Exception)
            {

            }

            return bRet;
        }
        #endregion
    }
}
