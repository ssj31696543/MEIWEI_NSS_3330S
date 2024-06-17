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
    public class DBExProcQtyInfo : SQLiteBase
    {
        public string strTableName = "PROC_QTY_INFO";

        public DBExProcQtyInfo()
        {
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(";
            for (int i = 0; i < (int)ProcQtyInfo.ColName.Max; i++)
            {
                strQueryCreate += ((ProcQtyInfo.ColName)i).ToString() + " VARCHAR(100) NOT NULL DEFAULT '',";
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
        private void Enqueue_InsertDB(EqpProcInfo Info)
        {
            //그리고 맨 앞에 추가
            DBQueue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("
                            + "'" + Info.LOT_ID + "', "
                            + "'" + Info.LOT_ID + "', "                           
                            + "'" + Info.MGZ_COUNT + "', "
                            + "'" + Info.STRIP_INPUT_COUNT + "', " 
                            + "'" + Info.STRIP_OUTPUT_COUNT + "', "
                            + "'" + Info.STRIP_CUTTING_COUNT + "', " 
                            + "'" + Info.MULTI_PICKER_1_PICKUP_COUNT + "', "
                            + "'" + Info.MULTI_PICKER_2_PICKUP_COUNT + "', " 
                            + "'" + Info.MULTI_PICKER_1_PLACE_COUNT + "', "
                            + "'" + Info.MULTI_PICKER_2_PLACE_COUNT + "', " 
                            + "'" + Info.GOOD_TABLE_1_TRAY_WORK_COUNT + "', "
                            + "'" + Info.GOOD_TABLE_2_TRAY_WORK_COUNT + "', "
                            + "'" + Info.REWORK_TABLE_TRAY_WORK_COUNT + "', "
                            + "'" + Info.MARK_VISION_OK_COUNT + "', "
                            + "'" + Info.MARK_VISION_RW_COUNT + "', "
                            + "'" + Info.BALL_VISION_OK_COUNT + "', "
                            + "'" + Info.BALL_VISION_NG_COUNT + "', "
                            + "'" + Info.TOTAL_CHIP_PROD_COUNT + "', "
                            + "'" + Info.TOTAL_OK_COUNT + "', "
                            + "'" + Info.TOTAL_RW_COUNT + "', "
                            + "'" + Info.TOTAL_NG_COUNT + "', "
                            + "'" + Info.ITS_XMARK_COUNT + "', "
                            + "'" + Info.EQP_RUNNING_TIME + "', "
                            + "'" + Info.EQP_STOP_TIME + "', "
                            + "'" + Info.LOSS_UNIT_COUNT + "')");
        }
        #endregion

        #region UPDATE
        private void Enqueue_UpdateDB(EqpProcInfo Info)
        {
            DBQueue.Enqueue("UPDATE " + TABLE_NAME + " SET " +
                    ProcQtyInfo.ColName.LotId.ToString() + "='" + Info.LOT_ID + "', "                      +
                    ProcQtyInfo.ColName.LotId.ToString() + "='" + Info.LOT_ID + "', "                      +
                    ProcQtyInfo.ColName.Mgz_Count.ToString() + "='" + Info.MGZ_COUNT + "', "+
                    ProcQtyInfo.ColName.Strip_input_count.ToString() + "='" + Info.STRIP_INPUT_COUNT + "', " +
                    ProcQtyInfo.ColName.Strip_output_count.ToString() + "='" + Info.STRIP_OUTPUT_COUNT + "', "+
                    ProcQtyInfo.ColName.Strip_cutting_count.ToString() + "='" + Info.STRIP_CUTTING_COUNT + "', " +
                    ProcQtyInfo.ColName.Multi_picker_1_pickup_count.ToString() + "='" + Info.MULTI_PICKER_1_PICKUP_COUNT + "', "+
                    ProcQtyInfo.ColName.Multi_picker_2_pickup_count.ToString() + "='" + Info.MULTI_PICKER_2_PICKUP_COUNT + "', " +
                    ProcQtyInfo.ColName.Multi_picker_1_place_count.ToString() + "='" + Info.MULTI_PICKER_1_PLACE_COUNT + "', "+
                    ProcQtyInfo.ColName.Multi_picker_2_place_count.ToString() + "='" + Info.MULTI_PICKER_2_PLACE_COUNT + "', " +
                    ProcQtyInfo.ColName.Good_table_1_tray_work_count.ToString() + "='" + Info.GOOD_TABLE_1_TRAY_WORK_COUNT + "', "+
                    ProcQtyInfo.ColName.Good_table_2_tray_work_count.ToString() + "='" + Info.GOOD_TABLE_2_TRAY_WORK_COUNT + "', "+
                    ProcQtyInfo.ColName.Rework_table_tray_work_count.ToString() + "='" + Info.REWORK_TABLE_TRAY_WORK_COUNT + "', "+
                    ProcQtyInfo.ColName.Mark_vision_ok_count.ToString() + "='" + Info.MARK_VISION_OK_COUNT + "', "+
                    ProcQtyInfo.ColName.Mark_vision_rw_count.ToString() + "='" + Info.MARK_VISION_RW_COUNT + "', "+
                    ProcQtyInfo.ColName.Ball_vision_ok_count.ToString() + "='" + Info.BALL_VISION_OK_COUNT + "', "+
                    ProcQtyInfo.ColName.Ball_vision_ng_count.ToString() + "='" + Info.BALL_VISION_NG_COUNT + "', "+
                    ProcQtyInfo.ColName.Total_chip_prod_count.ToString() + "='" + Info.TOTAL_CHIP_PROD_COUNT + "', "+
                    ProcQtyInfo.ColName.Total_ok_count.ToString() + "='" + Info.TOTAL_OK_COUNT + "', "+
                    ProcQtyInfo.ColName.Total_rw_count.ToString() + "='" + Info.TOTAL_RW_COUNT + "', "+
                    ProcQtyInfo.ColName.Total_ng_count.ToString() + "='" + Info.TOTAL_NG_COUNT + "', "+
                    ProcQtyInfo.ColName.Its_count.ToString() + "='" + Info.ITS_XMARK_COUNT + "', "+
                    ProcQtyInfo.ColName.Eqp_running_time.ToString() + "='" + Info.EQP_RUNNING_TIME + "', " +
                    ProcQtyInfo.ColName.Eqp_stop_time.ToString() + "='" + Info.EQP_STOP_TIME + "', " +
                    ProcQtyInfo.ColName.Strip_loss_count.ToString() + "='" + Info.LOSS_UNIT_COUNT+ "', " +
                    "WHERE " + ProcQtyInfo.ColName.LotId.ToString() + "='" + Info.LOT_ID + "'");
        }
        #endregion

        #region SELECT
        private BindingList<EqpProcInfo> Enqueue_SelectInfo()
        {
            BindingList<EqpProcInfo> Info = new BindingList<EqpProcInfo>();

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
                                    int nValue = 0;
                                    dr = dt.Rows[nCnt];
                                    Info.Add(new EqpProcInfo());
                                    Info[nCnt].LOT_ID = dr.ItemArray[(int)ProcQtyInfo.ColName.LotId].ToString();

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Mgz_Count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].MGZ_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Strip_input_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].STRIP_INPUT_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Strip_output_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].STRIP_OUTPUT_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Strip_cutting_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].STRIP_CUTTING_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Multi_picker_1_pickup_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].MULTI_PICKER_1_PICKUP_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Multi_picker_2_pickup_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].MULTI_PICKER_2_PICKUP_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Multi_picker_1_place_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].MULTI_PICKER_1_PLACE_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Multi_picker_2_place_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].MULTI_PICKER_2_PLACE_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Good_table_1_tray_work_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].GOOD_TABLE_1_TRAY_WORK_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Good_table_2_tray_work_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].GOOD_TABLE_2_TRAY_WORK_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Rework_table_tray_work_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].REWORK_TABLE_TRAY_WORK_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Mark_vision_ok_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].MARK_VISION_OK_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Mark_vision_rw_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].MARK_VISION_RW_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Ball_vision_ok_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].BALL_VISION_OK_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Ball_vision_ng_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].BALL_VISION_NG_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Total_chip_prod_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].TOTAL_CHIP_PROD_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Total_ok_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].TOTAL_OK_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Total_rw_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].TOTAL_RW_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Total_ng_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].TOTAL_NG_COUNT = nValue;

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Its_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].ITS_XMARK_COUNT = nValue;

                                    Info[nCnt].EQP_RUNNING_TIME = dr.ItemArray[(int)ProcQtyInfo.ColName.Eqp_running_time].ToString();
                                    Info[nCnt].EQP_STOP_TIME = dr.ItemArray[(int)ProcQtyInfo.ColName.Eqp_stop_time].ToString();

                                    if (!int.TryParse(dr.ItemArray[(int)ProcQtyInfo.ColName.Strip_loss_count].ToString(), out nValue)) nValue = 0;
                                    Info[nCnt].LOSS_UNIT_COUNT = nValue;
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
                Info = new BindingList<EqpProcInfo>();
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
            DBQueue.Enqueue("DELETE FROM " + TABLE_NAME + " WHERE " + ProcQtyInfo.ColName.LotId.ToString() + "='" + strLotId + "'");
        }
        #endregion

        #endregion

        #region 외부함수
        /// <summary>
        /// 새로운 Lot이 투입되었을 시 추가한다.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public bool InsertInfo(EqpProcInfo Info)
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
        public bool UpdateInfo(BindingList<EqpProcInfo> Info)
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
        public bool UpdateAllInfo(BindingList<EqpProcInfo> Info)
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
        public BindingList<EqpProcInfo> DownloadInfo()
        {
            BindingList<EqpProcInfo> lstRet = new BindingList<EqpProcInfo>();

            try
            {
                //모든 정보를 가져와 EqpProcInfo리스트에 넣는다.
                lstRet = Enqueue_SelectInfo();
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
