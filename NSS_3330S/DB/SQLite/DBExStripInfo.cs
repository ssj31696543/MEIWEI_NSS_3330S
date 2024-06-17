using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using SQLiteDB;

namespace NSS_3330S
{
    public class DBExStripInfo : SQLiteBase
    {
        public string strTableName = "PROC_INFO_STRIP";
        public int nRowCnt = 0;

        public DBExStripInfo(int nType)
        {
            if ((int)STRIP_MDL.NONE == nType || (int)STRIP_MDL.NONE == nType) return;

            TABLE_NAME = string.Format("{0}_{1}", strTableName, ((STRIP_MDL)nType).ToString());

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Time BIGINT NOT NULL DEFAULT 0,
                                InTime VARCHAR(5000) NOT NULL DEFAULT '',
                                OutTime VARCHAR(5000) NOT NULL DEFAULT '',
                                LotId VARCHAR(50) NOT NULL DEFAULT '',
                                MgzId VARCHAR(50) NOT NULL DEFAULT '',
                                StripId VARCHAR(100) NOT NULL DEFAULT '', 
                                MagazineSlotNo INT NOT NULL DEFAULT 0,                                
                                CuttingTableNo INT NOT NULL DEFAULT 0,
                                MapStageNo INT NOT NULL DEFAULT 0,
                                RcpMat VARCHAR(100) NOT NULL DEFAULT '', 
                                HostCode VARCHAR(500) NOT NULL DEFAULT '',
                                TopCoodi VARCHAR(5000) NOT NULL DEFAULT '',
                                BtmCoodi VARCHAR(5000) NOT NULL DEFAULT '',
                                TopNgCode VARCHAR(500) NOT NULL DEFAULT '',
                                BtmNgCode VARCHAR(500) NOT NULL DEFAULT '',
                                TopInspResult VARCHAR(500) NOT NULL DEFAULT '',
                                BtmInspResult VARCHAR(500) NOT NULL DEFAULT '', 
                                TopPkgSize VARCHAR(5000) NOT NULL DEFAULT '',
                                BtmPkgSize VARCHAR(5000) NOT NULL DEFAULT '', 
                                BtmSawOffset VARCHAR(5000) NOT NULL DEFAULT '',
                                OutPort VARCHAR(500) NOT NULL DEFAULT '',
                                IsUnit VARCHAR(500) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();
        }

        #region 내부 함수
        #region INSERT
        /// <summary>
        /// 새로운 레코드를 추가합니다.
        /// pjh 220728수정
        /// </summary>
        /// <param name="Info">스트립 정보</param>
        /// <param name="Intime">시간</param>
        private void Enqueue_InsertDB(StripInfo Info, DateTime dt, string strInTime, string strOutTime)
        {
            //그리고 맨 앞에 추가
            DBQueue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("
                            + "'" + dt.ToString("yyyyMMddHHmmssff") + "', "
                            + "'" + strInTime + "', "
                            + "'" + strOutTime + "', "
                            + "'" + Info.LOT_ID + "', "
                            + "'" + Info.ITS_ID + "', "
                            + "'" + Info.STRIP_ID + "', "
                            + "'" + Info.MAGAZINE_SLOT_NO + "', "
                            + "'" + Info.CUTTING_TABLE_NO + "', "
                            + "'" + Info.MAP_TABLE_NO + "', "
                            + "'" + string.Format("{0},{1}", 
                                                    RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY, 
                                                    RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX) + "', "
                            + "'" + Info.HOST_CODE() + "', "
                            + "'" + Info.TOP_COORDI() + "', "
                            + "'" + Info.BTM_COORDI() + "', "
                            + "'" + Info.TOP_NG_CODE() + "', "
                            + "'" + Info.BTM_NG_CODE() + "', "
                            + "'" + Info.TOP_RESULT() + "', "
                            + "'" + Info.BTM_RESULT() + "', "
                            + "'" + Info.MAP_PKG_SIZE() + "', "
                            + "'" + Info.BALL_PKG_SIZE() + "', "
                            + "'" + Info.BALL_SAW_OFFSET() + "', "
                            + "'" + Info.OUT_PORT() + "', "
                            + "'" + Info.ISUNIT() + "')");
        }
        #endregion

        #region SELECT

        private StripInfo[] Enqueue_SelectStrip(string strStripId)
        {

            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                StripInfo[] Info = null;
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
                        sqCommend.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", TABLE_NAME, StripProperties.ColName.StripId.ToString(), strStripId);

                        //데이터를 받아올 adapter 생성
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqCommend);
                        //datatable 생성하고 그 테이블에 데이터를 받아온다.
                        adapter.Fill(dt);

                        DataRow dr = null;
                        if (dt.Rows.Count > 0)
                        {
                            Info = new StripInfo[dt.Rows.Count];
                            for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                            {
                                dr = dt.Rows[nCnt];
                                string[] strMat = dr.ItemArray[9].ToString().Split(',');
                                if (strMat.Length == 2)
                                {
                                    int nRow = 0;
                                    int nCol = 0;
                                    if (!int.TryParse(strMat[0], out nRow)) nRow = 0;
                                    if (!int.TryParse(strMat[1], out nCol)) nCol = 0;

                                    Info[nCnt] = new StripInfo();
                                    Info[nCnt].Init(nRow);
                                    Info[nCnt].Set(nCol);
                                    string[] InTimes = dr.ItemArray[1].ToString().Split(',');
                                    string[] OutTimes = dr.ItemArray[2].ToString().Split(',');

                                    for (int i = 0; i < InTimes.Length; i++)
                                    {
                                        if (DateTime.TryParseExact(InTimes[i], "yyyyMMddHHmmssff", null, System.Globalization.DateTimeStyles.None, out Info[nCnt].dtLastCycleIn[i])) Info[nCnt].dtLastCycleIn[i] = DateTime.Now;
                                        if (DateTime.TryParseExact(OutTimes[i], "yyyyMMddHHmmssff", null, System.Globalization.DateTimeStyles.None, out Info[nCnt].dtLastCycleOut[i])) Info[nCnt].dtLastCycleOut[i] = DateTime.Now;
                                    }

                                    Info[nCnt].LOT_ID = dr.ItemArray[3].ToString();
                                    Info[nCnt].ITS_ID = dr.ItemArray[4].ToString();
                                    Info[nCnt].STRIP_ID = dr.ItemArray[5].ToString();
                                    try
                                    {
                                        Info[nCnt].MAGAZINE_SLOT_NO = int.Parse(dr.ItemArray[6].ToString());
                                        Info[nCnt].CUTTING_TABLE_NO = int.Parse(dr.ItemArray[7].ToString());
                                        Info[nCnt].MAP_TABLE_NO = int.Parse(dr.ItemArray[8].ToString());
                                    }
                                    catch { }
                                    Info[nCnt].SetHostCode(dr.ItemArray[10].ToString());
                                    Info[nCnt].SetTopCoodi(dr.ItemArray[11].ToString());
                                    Info[nCnt].SetBtmCoodi(dr.ItemArray[12].ToString());
                                    Info[nCnt].SetTopNgCode(dr.ItemArray[13].ToString());
                                    Info[nCnt].SetBtmNgCode(dr.ItemArray[14].ToString());
                                    Info[nCnt].SetTopResult(dr.ItemArray[15].ToString());
                                    Info[nCnt].SetBtmResult(dr.ItemArray[16].ToString());
                                    Info[nCnt].SetTopPkgSize(dr.ItemArray[17].ToString());
                                    Info[nCnt].SetBtmPkgSize(dr.ItemArray[18].ToString());
                                    Info[nCnt].SetBtmSawOffsetLt(dr.ItemArray[19].ToString());
                                    Info[nCnt].SetOutPort(dr.ItemArray[20].ToString());
                                    Info[nCnt].SetIsUnit(dr.ItemArray[21].ToString());
                                }
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
                return Info;
            }
        }

        /// <summary>
        /// 테이블의 모든 스트립 정보를 내림차순 시간순으로 가져옵니다.
        /// </summary>
        /// <returns>스트립 정보 배열</returns>
        private StripInfo[] Enqueue_SelectDB()
        {
            using (SQLiteConnection SQConn = new SQLiteConnection())
            {
                SQConn.ConnectionString = connecString;
                SQConn.Open();
                DataTable dt = new DataTable();
                StripInfo[] Info = new StripInfo[5];
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
                        sqCommend.CommandText = "SELECT * FROM " + TABLE_NAME + " ORDER BY " + StripProperties.ColName.Time.ToString() + " DESC";

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
                                string[] strMat = dr.ItemArray[9].ToString().Split(',');
                                if (strMat.Length == 2)
                                {
                                    int nRow = 0;
                                    int nCol = 0;
                                    if (!int.TryParse(strMat[0], out nRow)) nRow = 0;
                                    if (!int.TryParse(strMat[1], out nCol)) nCol = 0;

                                    Info[nCnt] = new StripInfo();
                                    Info[nCnt].Init(nRow);
                                    Info[nCnt].Set(nCol);
                                    string[] InTimes = dr.ItemArray[1].ToString().Split(',');
                                    string[] OutTimes = dr.ItemArray[2].ToString().Split(',');

                                    for (int i = 0; i < InTimes.Length; i++)
                                    {
                                        if (!DateTime.TryParseExact(InTimes[i], "yyyyMMddHHmmssff", null, System.Globalization.DateTimeStyles.None, out Info[nCnt].dtLastCycleIn[i])) Info[nCnt].dtLastCycleIn[i] = DateTime.Now;
                                        if (!DateTime.TryParseExact(OutTimes[i], "yyyyMMddHHmmssff", null, System.Globalization.DateTimeStyles.None, out Info[nCnt].dtLastCycleOut[i])) Info[nCnt].dtLastCycleOut[i] = DateTime.Now;
                                    }

                                    Info[nCnt].LOT_ID = dr.ItemArray[3].ToString();
                                    Info[nCnt].ITS_ID = dr.ItemArray[4].ToString();
                                    Info[nCnt].STRIP_ID = dr.ItemArray[5].ToString();
                                    try
                                    {
                                        Info[nCnt].MAGAZINE_SLOT_NO = int.Parse(dr.ItemArray[6].ToString());
                                        Info[nCnt].CUTTING_TABLE_NO = int.Parse(dr.ItemArray[7].ToString());
                                        Info[nCnt].MAP_TABLE_NO = int.Parse(dr.ItemArray[8].ToString());
                                    }
                                    catch { }
                                    Info[nCnt].SetHostCode(dr.ItemArray[10].ToString());
                                    Info[nCnt].SetTopCoodi(dr.ItemArray[11].ToString());
                                    Info[nCnt].SetBtmCoodi(dr.ItemArray[12].ToString());
                                    Info[nCnt].SetTopNgCode(dr.ItemArray[13].ToString());
                                    Info[nCnt].SetBtmNgCode(dr.ItemArray[14].ToString());
                                    Info[nCnt].SetTopResult(dr.ItemArray[15].ToString());
                                    Info[nCnt].SetBtmResult(dr.ItemArray[16].ToString());
                                    Info[nCnt].SetTopPkgSize(dr.ItemArray[17].ToString());
                                    Info[nCnt].SetBtmPkgSize(dr.ItemArray[18].ToString());
                                    Info[nCnt].SetBtmSawOffsetLt(dr.ItemArray[19].ToString());
                                    Info[nCnt].SetOutPort(dr.ItemArray[20].ToString());
                                    Info[nCnt].SetIsUnit(dr.ItemArray[21].ToString());
                                }
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
                return Info;
            }
        }

        #endregion

        #region UPDATE
        /// <summary>
        /// 스트립의 모든 정보를 시간과 함께 업데이트 합니다.
        /// pjh 220728 수정
        /// </summary>
        /// <param name="Info">스트립 정보</param>
        /// <param name="Intime">시간</param>
        private void Enqueue_UpdateStripInfo(StripInfo Info, string strInTimes, string strOutTimes)
        {
            DBQueue.Enqueue("UPDATE " + TABLE_NAME + " SET " +
                    StripProperties.ColName.InTime.ToString() + "='" + strInTimes + "', " +
                    StripProperties.ColName.OutTime.ToString() + "='" + strOutTimes + "', " +
                    StripProperties.ColName.LotId.ToString() + "='" + Info.LOT_ID + "', " +
                    StripProperties.ColName.ItsId.ToString() + "='" + Info.ITS_ID + "', " +
                    StripProperties.ColName.StripId.ToString() + "='" + Info.STRIP_ID + "', " +
                    StripProperties.ColName.MagazineSlotNo.ToString() + "='" + Info.MAGAZINE_SLOT_NO + "', " +
                    StripProperties.ColName.CuttingTableNo.ToString() + "='" + Info.CUTTING_TABLE_NO + "', " +
                    StripProperties.ColName.MapStageNo.ToString() + "='" + Info.MAP_TABLE_NO + "', " +
                    StripProperties.ColName.RcpMat.ToString() + "='" + string.Format("{0},{1}", RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY, 
                                                                                        RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX) + "', " +
                    StripProperties.ColName.HostCode.ToString() + "='" + Info.HOST_CODE() + "', " +
                    StripProperties.ColName.TopCoodi.ToString() + "='" + Info.TOP_COORDI() + "', " +
                    StripProperties.ColName.BtmCoodi.ToString() + "='" + Info.BTM_COORDI() + "', " +
                    StripProperties.ColName.TopNgCode.ToString() + "='" + Info.TOP_NG_CODE() + "', " +
                    StripProperties.ColName.BtmNgCode.ToString() + "='" + Info.BTM_NG_CODE() + "', " +
                    StripProperties.ColName.TopInspResult.ToString() + "='" + Info.TOP_RESULT() + "', " +
                    StripProperties.ColName.BtmInspResult.ToString() + "='" + Info.BTM_RESULT() + "', " +
                    StripProperties.ColName.TopPkgSize.ToString() + "='" + Info.MAP_PKG_SIZE() + "', " +
                    StripProperties.ColName.BtmPkgSize.ToString() + "='" + Info.BALL_PKG_SIZE() + "', " +
                    StripProperties.ColName.BtmSawOffset.ToString() + "='" + Info.BALL_SAW_OFFSET() + "', " +
                    StripProperties.ColName.OutPort.ToString() + "='" + Info.BALL_SAW_OFFSET() + "', " +
                    StripProperties.ColName.IsUnit.ToString() + "='" + Info.ISUNIT() + "'");
        }

        /// <summary>
        /// 스트립의 모든 정보를 업데이트 합니다.(시간은 업데이트x)
        /// pjh 220728수정
        /// </summary>
        /// <param name="Info">스트립 정보</param>
        private void Enqueue_UpdateStripInfo(StripInfo Info, string strOutTimes)
        {
            DBQueue.Enqueue("UPDATE " + TABLE_NAME + " SET " +
                    StripProperties.ColName.OutTime.ToString() + "='" + strOutTimes + "', " +
                    StripProperties.ColName.LotId.ToString() + "='" + Info.LOT_ID + "', " +
                    StripProperties.ColName.ItsId.ToString() + "='" + Info.ITS_ID + "', " +
                    StripProperties.ColName.StripId.ToString() + "='" + Info.STRIP_ID + "', " +
                    StripProperties.ColName.MagazineSlotNo.ToString() + "='" + Info.MAGAZINE_SLOT_NO + "', " +
                    StripProperties.ColName.CuttingTableNo.ToString() + "='" + Info.CUTTING_TABLE_NO + "', " +
                    StripProperties.ColName.MapStageNo.ToString() + "='" + Info.MAP_TABLE_NO + "', " +
                    StripProperties.ColName.RcpMat.ToString() + "='" + string.Format("{0},{1}", RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY,
                                                                                        RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX) + "', " +
                    StripProperties.ColName.HostCode.ToString() + "='" + Info.HOST_CODE() + "', " +
                    StripProperties.ColName.TopCoodi.ToString() + "='" + Info.TOP_COORDI() + "', " +
                    StripProperties.ColName.BtmCoodi.ToString() + "='" + Info.BTM_COORDI() + "', " +
                    StripProperties.ColName.TopNgCode.ToString() + "='" + Info.TOP_NG_CODE() + "', " +
                    StripProperties.ColName.BtmNgCode.ToString() + "='" + Info.BTM_NG_CODE() + "', " +
                    StripProperties.ColName.TopInspResult.ToString() + "='" + Info.TOP_RESULT() + "', " +
                    StripProperties.ColName.BtmInspResult.ToString() + "='" + Info.BTM_RESULT() + "', " +
                    StripProperties.ColName.TopPkgSize.ToString() + "='" + Info.MAP_PKG_SIZE() + "', " +
                    StripProperties.ColName.BtmPkgSize.ToString() + "='" + Info.BALL_PKG_SIZE() + "', " +
                    StripProperties.ColName.BtmSawOffset.ToString() + "='" + Info.BALL_SAW_OFFSET() + "', " +
                    StripProperties.ColName.OutPort.ToString() + "='" + Info.OUT_PORT() + "', " +
                    StripProperties.ColName.IsUnit.ToString() + "='" + Info.ISUNIT() + "'" +
                    " WHERE " + StripProperties.ColName.StripId.ToString() + "='" + Info.STRIP_ID + "'");
        }

        /// <summary>
        /// 스트립 아이디로 검색 후 지정한 Col의 값을 변경합니다.
        /// </summary>
        /// <param name="strStripId">검색할 스트립 아이디</param>
        /// <param name="name">변경할 열의 이름</param>
        /// <param name="data">변경할 데이터</param>
        private void Enqueue_UpdateSrtipItem(string strStripId, StripProperties.ColName name, object data)
        {
            DBQueue.Enqueue("UPDATE " + TABLE_NAME + " SET " +

                    name + "'" + data.ToString() + "' " +

                    " WHERE " + StripProperties.ColName.StripId.ToString() + "='" + strStripId.ToString() + "'");
        }

        /// <summary>
        /// 스트립 아이디로 검색 후 지정한 여러개의 열 값을 변경합니다.
        /// </summary>
        /// <param name="strStripId">검색할 스트립 아이디</param>
        /// <param name="name">변경할 열의 이름 배열</param>
        /// <param name="data">변경할 데이터 배열</param>
        private void Enqueue_UpdateSrtipItems(string strStripId, StripProperties.ColName[] name, object[] data)
        {
            string strQry = "";

            strQry = "UPDATE " + TABLE_NAME + " SET ";

            for (int i = 0; i < name.Length; i++)
            {
                strQry += name[i].ToString() + "= '" + data[i].ToString() + "', ";
            }
            strQry.Substring(0, strQry.Length - 2);//마지막 따옴표 삭제

            strQry += " WHERE " + StripProperties.ColName.StripId.ToString() + "='" + strStripId.ToString() + "'";

            DBQueue.Enqueue(strQry);
        }
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
        #endregion
        #endregion

        #region 외부 함수
        public void InsertStripInfo(StripInfo Info, string strInTimes, string strOutTimes)
        {
            if (isExsitStrip(Info.STRIP_ID)) return;

            Enqueue_DeleteTopN(4);
            Enqueue_InsertDB(Info, DateTime.Now, strInTimes, strOutTimes);
        }

        public void UpdateStripInfo(StripInfo Info, string strOutTimes)
        {
            Enqueue_UpdateStripInfo(Info, strOutTimes);
        }

        /*서브 맵 표시 용 존재하는 모든 서브맵 가져오기
         * 화면에는 현재의 서브만 보여지기
         * 선택한 서브로 업데이트 시 
         * 스트립이 쉬프트 되는 시점에는 화면 갱신하기..
         * */
        public StripInfo[] SelectStripsInfo()
        {
            StripInfo[] info = null;

            try
            {
                info = Enqueue_SelectDB();
            }
            catch (Exception)
            {
                info = null;
            }

            return info;
        }

        public StripInfo SelectRecentStripInfo()
        {
            StripInfo[] infos = null;
            StripInfo info = null;

            try
            {
                infos = Enqueue_SelectDB();

                if (infos.Length > 0)
                {
                    if (infos[0] != null)
                    {
                        info = infos[0];
                    }
                }
            }
            catch (Exception)
            {
                info = null;
            }

            return info;
        }

        public bool isExsitStrip(string strStripId)
        {
            bool bRet = true;
            try
            {
                StripInfo[] info = null;

                info = Enqueue_SelectStrip(strStripId);

                if (info == null)
                {
                    bRet = false;
                }
                else
                {
                    if (info.Length == 0)
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

        #endregion
    }
}
