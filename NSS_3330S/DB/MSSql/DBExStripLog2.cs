using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Drawing;

namespace NSS_3330S
{
    public class DBExStripLog2 : DBExBase
    {
        Thread m_thread;
        bool m_bFlagThreadAlive = false;

        ConcurrentQueue<string> m_queue = new ConcurrentQueue<string>();

        public string strTableName = "";
        bool _isOpen;
        DataTable m_dtResult = null;

        DateTime m_dtDelete;

        public DBExStripLog2()
        {
            m_dtDelete = DateTime.Now.AddDays(-1);

            // 테이블 명

            DATE_FORMAT = "yyyyMMddHHmmssff";
            strTableName = "TRK_STRIP_LOG";
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Idx INT IDENTITY(1,1) NOT NULL,
                                Time VARCHAR(50) NOT NULL DEFAULT '',
                                InTime VARCHAR(5000) NOT NULL DEFAULT '',
                                OutTime VARCHAR(5000) NOT NULL DEFAULT '',
                                LotId VARCHAR(50) NOT NULL DEFAULT '',
                                ItsId VARCHAR(50) NOT NULL DEFAULT '',
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
                                IsUnit VARCHAR(500) NOT NULL DEFAULT '', 
                                IsLineIn VARCHAR(500) NOT NULL DEFAULT '', 
                                IsMdlIn VARCHAR(500) NOT NULL DEFAULT '', 
                                IsMdlOut VARCHAR(500) NOT NULL DEFAULT '', 
                                IsLineOut VARCHAR(500) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            _isOpen = Open(dbInfo);
            if (_isOpen == false) return;
#if DBServer
            Create();
            //CreateData();
#endif

            StartStopManualThread(true);
        }

        public override void Close()
        {
            StartStopManualThread(false);

            base.Close();
        }

        public void StartStopManualThread(bool bStart)
        {
            if (m_thread != null)
            {
                // Join의 시간은 Thread에서 처리되는 최대 시간으로 지정
                m_bFlagThreadAlive = false;
                m_thread.Join(600);
                m_thread.Abort();
                m_thread = null;
            }

            if (bStart)
            {
                m_bFlagThreadAlive = true;
                m_thread = new Thread(new ParameterizedThreadStart(Run));
                m_thread.Name = "STRIP LOG THREAD";
                if (m_thread.IsAlive == false)
                    m_thread.Start(this);
            }
        }

        void Run(object obj)
        {
            StringBuilder sb = new StringBuilder();
            int nCount = 0;

            while (m_bFlagThreadAlive)
            {
                Thread.Sleep(10);

                string query = "";
                if (m_queue.Count > 0)
                {
                    nCount = 0;
                    sb.Clear();

                    while (m_queue.Count > 0 && m_queue.TryDequeue(out query))
                    {
                        sb.AppendFormat("{0};", query);

                        nCount++;
                        if (nCount > 20)
                            break;
                    }

                    if (nCount == 0)
                        continue;

                    try
                    {
                        helper.Execute(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        GbFunc.WriteExeptionLog(ex.ToString());
                    }
                }
                else
                {
                    Thread.Sleep(500);

                    if (DateTime.Now.Day != m_dtDelete.Day)
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_LOG_EXP_PERIOD].nValue > 0)
                        {
                            DateTime dtDel = DateTime.Now.AddDays(-ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_LOG_EXP_PERIOD].nValue);
                            query = string.Format("DELETE FROM {0} WHERE InTime < {1}", TABLE_NAME, dtDel.ToString(DATE_FORMAT));

                            try
                            {
                                DateTime dtStart = DateTime.Now;
                                helper.Execute(query);
                                TimeSpan ts = DateTime.Now - dtStart;
                                System.Diagnostics.Debug.WriteLine(string.Format("Del Time : {0}", ts.ToString()));
                            }
                            catch (Exception)
                            {
                            }

                            m_dtDelete = DateTime.Now;
                        }
                    }
                }
            }
        }

        #region 내부 함수
        #region INSERT
        private void Enqueue_InsertDB(StripInfo Info, string ts, string strInTime, string strOutTime)
        {
            //string sMdlIn = "";
            //string sMdlOut = "";

            //for (int i = 0; i < Info.isMdlIn.Length; i++)
            //{
            //    sMdlIn += Info.isMdlIn[i] + ",";
            //}
            //sMdlIn = sMdlIn.TrimEnd(',');

            //for (int i = 0; i < Info.isMdlOut.Length; i++)
            //{
            //    sMdlOut += Info.isMdlOut[i] + ",";
            //}
            //sMdlOut = sMdlOut.TrimEnd(',');

            //그리고 맨 앞에 추가
            m_queue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("
                            + "'" + ts + "', "
                            + "'" + strInTime + "', "
                            + "'" + strOutTime + "', "
                            + "'" + Info.LOT_ID + "', "
                            + "'" + Info.ITS_ID + "', "
                            + "'" + Info.STRIP_ID + "', "
                            + "'" + Info.MAGAZINE_SLOT_NO + "', "
                            + "'" + Info.CUTTING_TABLE_NO + "', "
                            + "'" + Info.MAP_TABLE_NO + "', "
                            + "'" + string.Format("{0},{1}", RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX) + "', "
                            + "'" + Info.HOST_CODE() + "', "
                            + "'" + Info.TOP_COORDI() + "', "
                            + "'" + Info.BTM_COORDI() + "', "
                            + "'" + Info.TOP_NG_CODE() + "', "
                            + "'" + Info.BTM_NG_CODE() + "', "
                            + "'" + Info.TOP_RESULT() + "', "
                            + "'" + Info.BTM_RESULT() + "', "
                            + "'" + Info.ISUNIT() + "', "
                            + "'" + Info.isLineIn + "', "
                            + "'" + sMdlIn + "', "
                            + "'" + sMdlOut + "', "
                            + "'" + Info.isLineOut + "')");
        }
        #endregion

        #region SELECT
        private StripInfo[] Enqueue_SelectStrip(string strStripId)
        {
            string query = string.Format("WHERE {0} = '{1}'", StripProperties.ColName.StripId.ToString(), strStripId);
            StripInfo[] Info = null;

            try
            {
                if (helper.CONNECTION.State != ConnectionState.Open)
                {
                    Open(dbInfo);
                }

                DataTable dt = new DataTable();

                if (helper.CONNECTION.State == ConnectionState.Open)
                {
                    SelectToDataTable(query, dt);

                    DataRow dr = null;
                    if (dt.Rows.Count > 0)
                    {
                        Info = new StripInfo[dt.Rows.Count];
                        for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                        {
                            dr = dt.Rows[nCnt];
                            string[] strMat = dr.ItemArray[10].ToString().Split(',');
                            if (strMat.Length == 2)
                            {
                                int nRow = 0;
                                int nCol = 0;
                                if (!int.TryParse(strMat[0], out nRow)) nRow = 0;
                                if (!int.TryParse(strMat[1], out nCol)) nCol = 0;


                                Info[nCnt] = new StripInfo();
                                Info[nCnt].Init(nRow);
                                Info[nCnt].Set(nCol);
                                string[] InTimes = dr.ItemArray[2].ToString().Split(',');
                                string[] OutTimes = dr.ItemArray[3].ToString().Split(',');

                                for (int i = 0; i < InTimes.Length; i++)
                                {
                                    if (DateTime.TryParseExact(InTimes[i], DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out Info[nCnt].dtLastCycleIn[i])) Info[nCnt].dtLastCycleIn[i] = DateTime.Now;
                                    if (DateTime.TryParseExact(OutTimes[i], DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out Info[nCnt].dtLastCycleOut[i])) Info[nCnt].dtLastCycleOut[i] = DateTime.Now;
                                }

                                Info[nCnt].LOT_ID = dr.ItemArray[4].ToString();
                                Info[nCnt].ITS_ID = dr.ItemArray[5].ToString();
                                Info[nCnt].STRIP_ID = dr.ItemArray[6].ToString();

                                Info[nCnt].MAGAZINE_SLOT_NO = int.Parse(dr.ItemArray[7].ToString());
                                Info[nCnt].CUTTING_TABLE_NO = int.Parse(dr.ItemArray[8].ToString());
                                Info[nCnt].MAP_TABLE_NO = int.Parse(dr.ItemArray[9].ToString());
                                Info[nCnt].SetHostCode(dr.ItemArray[11].ToString());
                                Info[nCnt].SetTopCoodi(dr.ItemArray[12].ToString());
                                Info[nCnt].SetBtmCoodi(dr.ItemArray[13].ToString());
                                Info[nCnt].SetTopNgCode(dr.ItemArray[14].ToString());
                                Info[nCnt].SetBtmNgCode(dr.ItemArray[15].ToString());
                                Info[nCnt].SetTopResult(dr.ItemArray[16].ToString());
                                Info[nCnt].SetBtmResult(dr.ItemArray[17].ToString());
                                Info[nCnt].SetIsUnit(dr.ItemArray[18].ToString());

                                bool value = false;
                                if (!bool.TryParse(dr.ItemArray[19].ToString(), out value)) value = false;
                                Info[nCnt].isLineIn = value;

                                string[] arrvalue;
                                arrvalue = dr.ItemArray[20].ToString().Split(',');
                                for (int i = 0; i < arrvalue.Length; i++)
                                {
                                    if (Info[nCnt].isMdlIn.Length <= i) continue;

                                    if (!bool.TryParse(arrvalue[i], out value)) value = false;
                                    Info[nCnt].isMdlIn[i] = value;
                                }

                                arrvalue = dr.ItemArray[21].ToString().Split(',');
                                for (int i = 0; i < arrvalue.Length; i++)
                                {
                                    if (Info[nCnt].isMdlOut.Length <= i) continue;

                                    if (!bool.TryParse(arrvalue[i], out value)) value = false;
                                    Info[nCnt].isMdlOut[i] = value;
                                }

                                if (!bool.TryParse(dr.ItemArray[22].ToString(), out value)) value = false;
                                Info[nCnt].isLineOut = value;

                            }
                        }
                    }
                }
                return Info;
            }
            catch (Exception)
            {
                return Info;
            }
        }

        private DataTable Enqueue_SelectLotStrip(string strLotId)
        {
            DataTable dt = new DataTable();
            string query = string.Format("SELECT Time, LotId, StripId  FROM {0} WHERE {1} = '{2}'", TABLE_NAME, StripProperties.ColName.LotId.ToString(), strLotId);
            try
            {
                if (helper.CONNECTION.State != ConnectionState.Open)
                {
                    Open(dbInfo);
                }

                if (helper.CONNECTION.State == ConnectionState.Open)
                {
                    SelectToDataTableToQuery(dt, query);
                }

                return dt;
            }
            catch
            {
                return dt;
            }
        }

        /// <summary>
        /// 테이블의 모든 스트립 정보를 내림차순 시간순으로 가져옵니다.
        /// </summary>
        /// <returns>스트립 정보 배열</returns>
        private StripInfo[] Enqueue_SelectDB()
        {
            string query = "";
            StripInfo[] Info = new StripInfo[1];

            try
            {
                if (helper.CONNECTION.State != ConnectionState.Open)
                {
                    Open(dbInfo);
                }

                DataTable dt = new DataTable();

                if (helper.CONNECTION.State == ConnectionState.Open)
                {
                    SelectToDataTable(query, dt);

                    DataRow dr = null;
                    if (dt.Rows.Count > 0)
                    {
                        Info = new StripInfo[dt.Rows.Count];
                        for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                        {
                            dr = dt.Rows[nCnt];
                            string[] strMat = dr.ItemArray[10].ToString().Split(',');
                            if (strMat.Length == 2)
                            {
                                int nRow = 0;
                                int nCol = 0;
                                if (!int.TryParse(strMat[0], out nRow)) nRow = 0;
                                if (!int.TryParse(strMat[1], out nCol)) nCol = 0;


                                Info[nCnt] = new StripInfo();
                                Info[nCnt].Init(nRow);
                                Info[nCnt].Set(nCol);
                                string[] InTimes = dr.ItemArray[2].ToString().Split(',');
                                string[] OutTimes = dr.ItemArray[3].ToString().Split(',');

                                for (int i = 0; i < InTimes.Length; i++)
                                {
                                    if (DateTime.TryParseExact(InTimes[i], DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out Info[nCnt].dtLastCycleIn[i])) Info[nCnt].dtLastCycleIn[i] = DateTime.Now;
                                    if (DateTime.TryParseExact(OutTimes[i], DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out Info[nCnt].dtLastCycleOut[i])) Info[nCnt].dtLastCycleOut[i] = DateTime.Now;
                                }

                                Info[nCnt].LOT_ID = dr.ItemArray[4].ToString();
                                Info[nCnt].ITS_ID = dr.ItemArray[5].ToString();
                                Info[nCnt].STRIP_ID = dr.ItemArray[6].ToString();

                                Info[nCnt].MAGAZINE_SLOT_NO = int.Parse(dr.ItemArray[7].ToString());
                                Info[nCnt].CUTTING_TABLE_NO = int.Parse(dr.ItemArray[8].ToString());
                                Info[nCnt].MAP_TABLE_NO = int.Parse(dr.ItemArray[9].ToString());
                                Info[nCnt].SetHostCode(dr.ItemArray[11].ToString());
                                Info[nCnt].SetTopCoodi(dr.ItemArray[12].ToString());
                                Info[nCnt].SetBtmCoodi(dr.ItemArray[13].ToString());
                                Info[nCnt].SetTopNgCode(dr.ItemArray[14].ToString());
                                Info[nCnt].SetBtmNgCode(dr.ItemArray[15].ToString());
                                Info[nCnt].SetTopResult(dr.ItemArray[16].ToString());
                                Info[nCnt].SetBtmResult(dr.ItemArray[17].ToString());
                                Info[nCnt].SetIsUnit(dr.ItemArray[18].ToString());

                                bool value = false;
                                if (!bool.TryParse(dr.ItemArray[19].ToString(), out value)) value = false;
                                Info[nCnt].isLineIn = value;

                                string[] arrvalue;
                                arrvalue = dr.ItemArray[20].ToString().Split(',');
                                for (int i = 0; i < arrvalue.Length; i++)
                                {
                                    if (Info[nCnt].isMdlIn.Length <= i) continue;

                                    if (!bool.TryParse(arrvalue[i], out value)) value = false;
                                    Info[nCnt].isMdlIn[i] = value;
                                }

                                arrvalue = dr.ItemArray[21].ToString().Split(',');
                                for (int i = 0; i < arrvalue.Length; i++)
                                {
                                    if (Info[nCnt].isMdlOut.Length <= i) continue;

                                    if (!bool.TryParse(arrvalue[i], out value)) value = false;
                                    Info[nCnt].isMdlOut[i] = value;
                                }

                                if (!bool.TryParse(dr.ItemArray[22].ToString(), out value)) value = false;
                                Info[nCnt].isLineOut = value;

                            }
                        }
                    }
                }
                return Info;
            }
            catch (Exception)
            {
                return Info;
            }
        }

        #endregion

        #region UPDATE
        /// <summary>
        /// 스트립의 모든 정보를 시간과 함께 업데이트 합니다.
        /// </summary>
        /// <param name="Info">스트립 정보</param>
        /// <param name="Intime">시간</param>
        private void Enqueue_UpdateStripInfo(StripInfo Info, string strInTimes, string strOutTimes)
        {
            string sMdlIn = "";
            string sMdlOut = "";

            for (int i = 0; i < Info.isMdlIn.Length; i++)
            {
                sMdlIn += Info.isMdlIn[i] + ",";
            }
            sMdlIn = sMdlIn.TrimEnd(',');

            for (int i = 0; i < Info.isMdlOut.Length; i++)
            {
                sMdlOut += Info.isMdlOut[i] + ",";
            }
            sMdlOut = sMdlOut.TrimEnd(',');

            m_queue.Enqueue("UPDATE " + TABLE_NAME + " SET " +
                    StripProperties.ColName.InTime.ToString() + "='" + strInTimes + "', " +
                    StripProperties.ColName.OutTime.ToString() + "='" + strOutTimes + "', " +
                    StripProperties.ColName.LotId.ToString() + "='" + Info.LOT_ID + "', " +
                    StripProperties.ColName.ItsId.ToString() + "='" + Info.ITS_ID + "', " +
                    StripProperties.ColName.StripId.ToString() + "='" + Info.STRIP_ID + "', " +
                    StripProperties.ColName.MagazineSlotNo.ToString() + "='" + Info.MAGAZINE_SLOT_NO + "', " +
                    StripProperties.ColName.CuttingTableNo.ToString() + "='" + Info.CUTTING_TABLE_NO + "', " +
                    StripProperties.ColName.MapStageNo.ToString() + "='" + Info.MAP_TABLE_NO + "', " +
                    StripProperties.ColName.RcpMat.ToString() + "='" + string.Format("{0},{1}", RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX) + "', " +
                    StripProperties.ColName.HostCode.ToString() + "='" + Info.HOST_CODE() + "', " +
                    StripProperties.ColName.TopCoodi.ToString() + "='" + Info.TOP_COORDI() + "', " +
                    StripProperties.ColName.BtmCoodi.ToString() + "='" + Info.BTM_COORDI() + "', " +
                    StripProperties.ColName.TopNgCode.ToString() + "='" + Info.TOP_NG_CODE() + "', " +
                    StripProperties.ColName.BtmNgCode.ToString() + "='" + Info.BTM_NG_CODE() + "', " +
                    StripProperties.ColName.TopInspResult.ToString() + "='" + Info.TOP_RESULT() + "', " +
                    StripProperties.ColName.BtmInspResult.ToString() + "='" + Info.BTM_RESULT() + "', " +
                    StripProperties.ColName.IsUnit.ToString() + "='" + Info.ISUNIT() + "', " +
                    StripProperties.ColName.IsLineIn.ToString() + "='" + Info.isLineIn + "', " +
                    StripProperties.ColName.IsMdlIn.ToString() + "='" + sMdlIn + "', " +
                    StripProperties.ColName.IsMdlOut.ToString() + "='" + sMdlOut + "', " +
                    StripProperties.ColName.IsLineOut.ToString() + "='" + Info.isLineOut + "'");
        }

        /// <summary>
        /// 스트립의 모든 정보를 업데이트 합니다.(시간은 업데이트x)
        /// </summary>
        /// <param name="Info">스트립 정보</param>
        private void Enqueue_UpdateStripInfo(StripInfo Info, string strOutTimes)
        {
            string sMdlIn = "";
            string sMdlOut = "";

            for (int i = 0; i < Info.isMdlIn.Length; i++)
            {
                sMdlIn += Info.isMdlIn[i] + ",";
            }
            sMdlIn = sMdlIn.TrimEnd(',');

            for (int i = 0; i < Info.isMdlOut.Length; i++)
            {
                sMdlOut += Info.isMdlOut[i] + ",";
            }
            sMdlOut = sMdlOut.TrimEnd(',');

            m_queue.Enqueue("UPDATE " + TABLE_NAME + " SET " +
                    StripProperties.ColName.OutTime.ToString() + "='" + strOutTimes + "', " +
                    StripProperties.ColName.LotId.ToString() + "='" + Info.LOT_ID + "', " +
                    StripProperties.ColName.ItsId.ToString() + "='" + Info.ITS_ID + "', " +
                    StripProperties.ColName.StripId.ToString() + "='" + Info.STRIP_ID + "', " +
                    StripProperties.ColName.MagazineSlotNo.ToString() + "='" + Info.MAGAZINE_SLOT_NO + "', " +
                    StripProperties.ColName.CuttingTableNo.ToString() + "='" + Info.CUTTING_TABLE_NO + "', " +
                    StripProperties.ColName.MapStageNo.ToString() + "='" + Info.MAP_TABLE_NO + "', " +
                    StripProperties.ColName.RcpMat.ToString() + "='" + string.Format("{0},{1}", RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX) + "', " +
                    StripProperties.ColName.HostCode.ToString() + "='" + Info.HOST_CODE() + "', " +
                    StripProperties.ColName.TopCoodi.ToString() + "='" + Info.TOP_COORDI() + "', " +
                    StripProperties.ColName.BtmCoodi.ToString() + "='" + Info.BTM_COORDI() + "', " +
                    StripProperties.ColName.TopNgCode.ToString() + "='" + Info.TOP_NG_CODE() + "', " +
                    StripProperties.ColName.BtmNgCode.ToString() + "='" + Info.BTM_NG_CODE() + "', " +
                    StripProperties.ColName.TopInspResult.ToString() + "='" + Info.TOP_RESULT() + "', " +
                    StripProperties.ColName.BtmInspResult.ToString() + "='" + Info.BTM_RESULT() + "', " +
                    StripProperties.ColName.IsUnit.ToString() + "='" + Info.ISUNIT() + "', " +
                    StripProperties.ColName.IsLineIn.ToString() + "='" + Info.isLineIn + "', " +
                    StripProperties.ColName.IsMdlIn.ToString() + "='" + sMdlIn + "', " +
                    StripProperties.ColName.IsMdlOut.ToString() + "='" + sMdlOut + "', " +
                    StripProperties.ColName.IsLineOut.ToString() + "='" + Info.isLineOut + "'" +
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
            m_queue.Enqueue("UPDATE " + TABLE_NAME + " SET " +

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

            m_queue.Enqueue(strQry);
        }
        #endregion

        #region DELETE
        /// <summary>
        /// 지정 개수 만큼만 남겨두고 나머지 레코드는 삭제합니다.
        /// </summary>
        /// <param name="nLimitCount">남길 레코드의 개수</param>
        /// <param name="eSortProp">내림차순 정렬 시 기준이 되는 ColName</param>
        private void Enqueue_DeleteTopN(int nLimitCount, ITS_LOG.ColName eSortProp = ITS_LOG.ColName.InTime)
        {
            string strQuery = "";
            strQuery = string.Format("DELETE FROM {0} WHERE {1} NOT IN ( SELECT * FROM ( SELECT {1} FROM {0} ORDER BY {1} DESC) LIMIT {2})", TABLE_NAME, eSortProp.ToString(), nLimitCount);
            m_queue.Enqueue(strQuery);
        }

        /// <summary>
        /// 일치하는 스트립 아이디가 존재 할 시 해당 레코드를 삭제합니다.
        /// </summary>
        /// <param name="strStripId">스트립 아이디</param>
        private void Enqueue_DeleteDB(string strStripId)
        {
            m_queue.Enqueue("DELETE FROM " + TABLE_NAME + " WHERE " + ITS_LOG.ColName.StripID.ToString() + "='" + strStripId + "'");
        }

        /// <summary>
        /// 일치하는 스트립 아이디가 존재 할 시 해당 레코드를 삭제합니다.
        /// </summary>
        /// <param name="strStripId">스트립 아이디</param>
        private void Enqueue_DeleteAll()
        {
            m_queue.Enqueue("DELETE FROM " + TABLE_NAME);
        }
        #endregion
        #endregion

        #region 외부 함수
        public void InsertStripInfo(StripInfo info, string strTimespan, string strInTimes, string strOutTimes)
        {
            if (isExsitStrip(info.STRIP_ID)) return;
            Enqueue_InsertDB(info, strTimespan, strInTimes, strOutTimes);
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


        public DataTable SelectLotStrip(string strLotId)
        {
            DataTable dt = new DataTable();
            try
            {
                dt = Enqueue_SelectLotStrip(strLotId);
            }
            catch (Exception)
            {
            }
            return dt;
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
