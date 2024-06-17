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
    public class DBExLotLog : SQLiteBase
    {
        public string strTableName = "TRK_LOT_LOG";

        //ProductLotInfo
        public DBExLotLog()
        {
            TABLE_NAME = strTableName;

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Move_in_time BIGINT NOT NULL DEFAULT 0,
                                Move_out_time BIGINT NOT NULL DEFAULT 0,
                                Lot_id VARCHAR(50) NOT NULL DEFAULT '', 
                                Cycle_time VARCHAR(50) NOT NULL DEFAULT '',
                                Carrier_rfid VARCHAR(50) NOT NULL DEFAULT '', 
                                Pkg_prod_count BIGINT NOT NULL DEFAULT 0,
                                Strip_count BIGINT NOT NULL DEFAULT 0,
                                Eqp_sq_chip_count BIGINT NOT NULL DEFAULT 0,
                                Eqp_nq_chip_count BIGINT NOT NULL DEFAULT 0,
                                Eqp_rj_chip_count BIGINT NOT NULL DEFAULT 0,
                                Eqp_sq_tray_count BIGINT NOT NULL DEFAULT 0,
                                Eqp_nq_tray_count BIGINT NOT NULL DEFAULT 0,
                                Sat_count BIGINT NOT NULL DEFAULT 0,
                                Total_chip_count BIGINT NOT NULL DEFAULT 0,
                                Mark_vision_err_count BIGINT NOT NULL DEFAULT 0,
                                Ball_vision_err_count BIGINT NOT NULL DEFAULT 0,
                                X_out_count BIGINT NOT NULL DEFAULT 0,
                                Host_sq_chip_count BIGINT NOT NULL DEFAULT 0,
                                Host_nq_chip_count BIGINT NOT NULL DEFAULT 0,
                                Host_rj_chip_count BIGINT NOT NULL DEFAULT 0,
                                Process_err_count BIGINT NOT NULL DEFAULT 0,
                                Strip_cutting_count BIGINT NOT NULL DEFAULT 0,
                                Unit_picker_place_count BIGINT NOT NULL DEFAULT 0)"; 

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();
        }

        #region 내부함수

        #region INSERT
        private void Enqueue_InsertDB(ProductLotInfo Info)
        {
            DBQueue.Enqueue("INSERT INTO " + TABLE_NAME + " VALUES ("             
                            + "'" + Info.MOVE_IN_TIME.ToString(DATE_FORMAT) + "', "
                            + "'" + Info.MOVE_OUT_TIME.ToString(DATE_FORMAT) + "', "
                            + "'" + Info.LOT_ID + "', "
                            + "'" + Info.CYCLE_TIME + "', "
                            + "'" + Info.CARRIER_ID + "', "
                            + "'" + Info.PKG_PROD_COUNT + "', "
                            + "'" + Info.STRIP_COUNT + "', "
                            + "'" + Info.EQP_SQ_CHIP_COUNT + "', "
                            + "'" + Info.EQP_NQ_CHIP_COUNT + "', "
                            + "'" + Info.EQP_RJ_CHIP_COUNT + "', "
                            + "'" + Info.EQP_SQ_TRAY_COUNT + "', "
                            + "'" + Info.EQP_NQ_TRAY_COUNT + "', "
                            + "'" + Info.SAT_COUNT + "', "
                            + "'" + Info.TOTAL_CHIP_COUNT + "', "
                            + "'" + Info.MARK_VISION_ERR_COUNT + "', "
                            + "'" + Info.BALL_VISION_ERR_COUNT + "', "
                            + "'" + Info.X_OUT_COUNT + "', "
                            + "'" + Info.HOST_SQ_CHIP_COUNT + "', "
                            + "'" + Info.HOST_NQ_CHIP_COUNT + "', "
                            + "'" + Info.HOST_RJ_CHIP_COUNT + "', "
                            + "'" + Info.PROCESS_ERR_COUNT + "', "
                            + "'" + Info.STRIP_CUTTING_COUNT + "', "
                            + "'" + Info.UNIT_PICKER_PLACE_COUNT + "')");
        }
        #endregion

        #region UPDATE
        private void Enqueue_UpdateDB(ProductLotInfo Info, TimeSpan ts)
        {
            DBQueue.Enqueue("UPDATE " + TABLE_NAME + " SET "
                            + "Move_out_time ='" + Info.MOVE_OUT_TIME.ToString(DATE_FORMAT) + "', "
                            + "Cycle_time ='" + ts.ToString() + "', "
                            + "Pkg_prod_count ='" + Info.PKG_PROD_COUNT + "', "
                            + "Strip_count ='" + Info.STRIP_COUNT + "', "
                            + "Eqp_sq_chip_count ='" + Info.EQP_SQ_CHIP_COUNT + "', "
                            + "Eqp_nq_chip_count ='" + Info.EQP_NQ_CHIP_COUNT + "', "
                            + "Eqp_rj_chip_count ='" + Info.EQP_RJ_CHIP_COUNT + "', "
                            + "Eqp_sq_tray_count ='" + Info.EQP_SQ_TRAY_COUNT + "', "
                            + "Eqp_nq_tray_count ='" + Info.EQP_NQ_TRAY_COUNT + "', "
                            + "Sat_count ='" + Info.SAT_COUNT + "', "
                            + "Total_chip_count ='" + Info.TOTAL_CHIP_COUNT + "', "
                            + "Mark_vision_err_count ='" + Info.MARK_VISION_ERR_COUNT + "', "
                            + "Ball_vision_err_count ='" + Info.BALL_VISION_ERR_COUNT + "', "
                            + "X_out_count ='" + Info.X_OUT_COUNT + "', "
                            + "Host_sq_chip_count ='" + Info.HOST_SQ_CHIP_COUNT + "', "
                            + "Host_nq_chip_count ='" + Info.HOST_NQ_CHIP_COUNT + "', "
                            + "Host_rj_chip_count ='" + Info.HOST_RJ_CHIP_COUNT + "', "
                            + "Process_err_count ='" + Info.PROCESS_ERR_COUNT + "', "
                            + "Strip_cutting_count ='" + Info.STRIP_CUTTING_COUNT + "', "
                            + "Unit_picker_place_count ='" + Info.UNIT_PICKER_PLACE_COUNT + "' "
                            + "WHERE Lot_id ='" + Info.LOT_ID + "'");
        }
        #endregion

        #region SELECT
 
        /// <summary>
        /// 입력한 조건에 부합하는 LOT 정보를 반환합니다.
        /// </summary>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="strLotId"></param>
        /// <param name="nMaxCount"></param>
        /// <returns></returns>
        public DataTable DataTableSelectDataInRange(DateTime dtStart, DateTime dtEnd, string strLotId, int nMaxCount = 10000)
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
                        //select query

                        if (strLotId == "")
                        {
                            sqCommend.CommandText = "SELECT * FROM " + TABLE_NAME + " WHERE Move_in_time" +
                                                    " BETWEEN '" + dtStart.ToString(DATE_FORMAT) + "'" +
                                                    " AND '" + dtEnd.ToString(DATE_FORMAT) + "' ORDER BY Move_in_time";
                        }
                        else
                        {
                            sqCommend.CommandText = "SELECT * FROM " + TABLE_NAME + " WHERE Lot_id='" + strLotId + "' AND (" +
                                                    "Move_in_time BETWEEN '" + dtStart.ToString(DATE_FORMAT) + "'" +
                                                    " AND '" + dtEnd.ToString(DATE_FORMAT) + "') ORDER BY Move_in_time";
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

        #region DELETE
        private void Enqueue_DeleteDB(string strLotId)
        {
            DBQueue.Enqueue("DELETE FROM" + TABLE_NAME + "WHERE Lot_id='" + strLotId);
        }
        #endregion

        #endregion

        #region 외부함수
        public void InsertLog(ProductLotInfo Info)
        {
            Enqueue_InsertDB(Info);
        }

        public void UpdateLog(ProductLotInfo info, TimeSpan ts)
        {
            Enqueue_UpdateDB(info, ts);
        }

        public DataTable SelectLotList(DateTime dtStart, DateTime dtEnd, string strLotId)
        {
            DataTable dt = new DataTable();

            try
            {
                dt = DataTableSelectDataInRange(dtStart, dtEnd, strLotId);
            }
            catch (Exception)
            {
            }

            return dt;
        }

        //public void SetDeletePeriod(int nPeriod)
        //{
        //    DeleteDB_TrigReset();
        //    DeleteDB_TrigSet(nPeriod, "Move_in_time");
        //}
        #endregion
    }
}
