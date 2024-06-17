using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace NSS_3330S
{
    public class DBExProcQtyLog2 : DBExBase
    {
        Thread m_thread;
        bool m_bFlagThreadAlive = false;

        ConcurrentQueue<string> m_queue = new ConcurrentQueue<string>();

        public string strTableName = "";
        bool _isOpen;
        DataTable m_dtResult = null;

        DateTime m_dtDelete;

        public DBExProcQtyLog2()
        {
            m_dtDelete = DateTime.Now.AddDays(-1);

            // 테이블 명
            strTableName = "PROC_QTY_LOG";
            TABLE_NAME = strTableName;
            DATE_FORMAT = "yyyyMMddHHmmss";

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(";
            strQueryCreate += "Idx INT IDENTITY(1, 1) NOT NULL,";
            for (int i = 1; i < (int)ProcQtyLog.ColName.Max; i++)
            {
                strQueryCreate += ((ProcQtyLog.ColName)i).ToString() + " VARCHAR(100) NOT NULL DEFAULT '',";
            }
            strQueryCreate = strQueryCreate.TrimEnd(',');
            strQueryCreate += ")";

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
                m_thread.Name = "EVENT LOG THREAD";
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
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD].nValue > 0)
                        {
                            DateTime dtDel = DateTime.Now.AddDays(-ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD].nValue);
                            query = string.Format("DELETE FROM {0} WHERE Eqp_in_time < {1}", TABLE_NAME, dtDel.ToString(DATE_FORMAT));

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

        private void Enqueue_InsertDB(EqpProcInfo Info)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0} VALUES (", TABLE_NAME);

            sb.AppendFormat("'{0}', ", Info.LOT_ID);
            sb.AppendFormat("'{0}', ", Info.LOT_ID);
            sb.AppendFormat("'{0}', ", Info.MGZ_COUNT);
            sb.AppendFormat("'{0}', ", Info.STRIP_INPUT_COUNT);
            sb.AppendFormat("'{0}', ", Info.STRIP_OUTPUT_COUNT);
            sb.AppendFormat("'{0}', ", Info.STRIP_CUTTING_COUNT);
            sb.AppendFormat("'{0}', ", Info.MULTI_PICKER_1_PICKUP_COUNT);
            sb.AppendFormat("'{0}', ", Info.MULTI_PICKER_2_PICKUP_COUNT);
            sb.AppendFormat("'{0}', ", Info.MULTI_PICKER_1_PLACE_COUNT);
            sb.AppendFormat("'{0}', ", Info.MULTI_PICKER_2_PLACE_COUNT);
            sb.AppendFormat("'{0}', ", Info.GOOD_TABLE_1_TRAY_WORK_COUNT);
            sb.AppendFormat("'{0}', ", Info.GOOD_TABLE_2_TRAY_WORK_COUNT);
            sb.AppendFormat("'{0}', ", Info.REWORK_TABLE_TRAY_WORK_COUNT);
            sb.AppendFormat("'{0}', ", Info.MARK_VISION_OK_COUNT);
            sb.AppendFormat("'{0}', ", Info.MARK_VISION_RW_COUNT);
            sb.AppendFormat("'{0}', ", Info.BALL_VISION_OK_COUNT);
            sb.AppendFormat("'{0}', ", Info.BALL_VISION_NG_COUNT);
            sb.AppendFormat("'{0}', ", Info.TOTAL_CHIP_PROD_COUNT);
            sb.AppendFormat("'{0}', ", Info.TOTAL_OK_COUNT);
            sb.AppendFormat("'{0}', ", Info.TOTAL_RW_COUNT);
            sb.AppendFormat("'{0}', ", Info.TOTAL_NG_COUNT);
            sb.AppendFormat("'{0}', ", Info.ITS_XMARK_COUNT);
            sb.AppendFormat("'{0}', ", Info.EQP_RUNNING_TIME);
            sb.AppendFormat("'{0}', ", Info.EQP_STOP_TIME);
            sb.AppendFormat("'{0}', ", Info.EQP_IN_TIME);
            sb.AppendFormat("'{0}'  ", Info.EQP_OUT_TIME);
            sb.Append(")");

            m_queue.Enqueue(sb.ToString());

            sb.Clear();
        }

        private void Enqueue_UpdateDB(EqpProcInfo Info)
        {
            m_queue.Enqueue("UPDATE " + TABLE_NAME + " SET " +
                    ProcQtyLog.ColName.LotId.ToString() + "='" + Info.LOT_ID + "', " +
                    ProcQtyLog.ColName.ItsId.ToString() + "='" + Info.LOT_ID + "', " +
                    ProcQtyLog.ColName.Mgz_Count.ToString() + "='" + Info.MGZ_COUNT + "', " +
                    ProcQtyLog.ColName.Strip_input_count.ToString() + "='" + Info.STRIP_INPUT_COUNT + "', " +
                    ProcQtyLog.ColName.Strip_output_count.ToString() + "='" + Info.STRIP_OUTPUT_COUNT + "', " +
                    ProcQtyLog.ColName.Strip_cutting_count.ToString() + "='" + Info.STRIP_CUTTING_COUNT + "', " +
                    ProcQtyLog.ColName.Multi_picker_1_pickup_count.ToString() + "='" + Info.MULTI_PICKER_1_PICKUP_COUNT + "', " +
                    ProcQtyLog.ColName.Multi_picker_2_pickup_count.ToString() + "='" + Info.MULTI_PICKER_2_PICKUP_COUNT + "', " +
                    ProcQtyLog.ColName.Multi_picker_1_place_count.ToString() + "='" + Info.MULTI_PICKER_1_PLACE_COUNT + "', " +
                    ProcQtyLog.ColName.Multi_picker_2_place_count.ToString() + "='" + Info.MULTI_PICKER_2_PLACE_COUNT + "', " +
                    ProcQtyLog.ColName.Good_table_1_tray_work_count.ToString() + "='" + Info.GOOD_TABLE_1_TRAY_WORK_COUNT + "', " +
                    ProcQtyLog.ColName.Good_table_2_tray_work_count.ToString() + "='" + Info.GOOD_TABLE_2_TRAY_WORK_COUNT + "', " +
                    ProcQtyLog.ColName.Rework_table_tray_work_count.ToString() + "='" + Info.REWORK_TABLE_TRAY_WORK_COUNT + "', " +
                    ProcQtyLog.ColName.Mark_vision_ok_count.ToString() + "='" + Info.MARK_VISION_OK_COUNT + "', " +
                    ProcQtyLog.ColName.Mark_vision_rw_count.ToString() + "='" + Info.MARK_VISION_RW_COUNT + "', " +
                    ProcQtyLog.ColName.Ball_vision_ok_count.ToString() + "='" + Info.BALL_VISION_OK_COUNT + "', " +
                    ProcQtyLog.ColName.Ball_vision_ng_count.ToString() + "='" + Info.BALL_VISION_NG_COUNT + "', " +
                    ProcQtyLog.ColName.Total_chip_prod_count.ToString() + "='" + Info.TOTAL_CHIP_PROD_COUNT + "', " +
                    ProcQtyLog.ColName.Total_ok_count.ToString() + "='" + Info.TOTAL_OK_COUNT + "', " +
                    ProcQtyLog.ColName.Total_rw_count.ToString() + "='" + Info.TOTAL_RW_COUNT + "', " +
                    ProcQtyLog.ColName.Total_ng_count.ToString() + "='" + Info.TOTAL_NG_COUNT + "', " +
                    ProcQtyLog.ColName.Its_count.ToString() + "='" + Info.ITS_XMARK_COUNT + "', " +
                    ProcQtyLog.ColName.Eqp_running_time.ToString() + "='" + Info.EQP_RUNNING_TIME + "', " +
                    ProcQtyLog.ColName.Eqp_stop_time.ToString() + "='" + Info.EQP_STOP_TIME + "', " +
                    ProcQtyLog.ColName.Eqp_in_time.ToString() + "='" + Info.EQP_IN_TIME + "', " +
                    ProcQtyLog.ColName.Eqp_out_time.ToString() + "='" + Info.EQP_OUT_TIME + "', " +
                    "WHERE " + ProcQtyLog.ColName.LotId.ToString() + "='" + Info.LOT_ID + "'");
        }

        private void Enqueue_DeleteAllDB()
        {
            m_queue.Enqueue("DELETE FROM " + TABLE_NAME + "");
        }

        private void Enqueue_DeleteDB(string strLotId)
        {
            m_queue.Enqueue("DELETE FROM " + TABLE_NAME + " WHERE " + ProcQtyLog.ColName.LotId.ToString() + "='" + strLotId + "'");
        }

        private BindingList<EqpProcInfo> Enqueue_SelectInfo()
        {
            BindingList<EqpProcInfo> Info = new BindingList<EqpProcInfo>();

            try
            {
                string query = "";

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
                        for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                        {
                            int nValue = 0;
                            dr = dt.Rows[nCnt];
                            Info.Add(new EqpProcInfo());
                            Info[nCnt].LOT_ID = dr.ItemArray[(int)ProcQtyLog.ColName.LotId].ToString();
                            Info[nCnt].LOT_ID = dr.ItemArray[(int)ProcQtyLog.ColName.LotId].ToString();

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Mgz_Count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].MGZ_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Strip_input_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].STRIP_INPUT_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Strip_output_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].STRIP_OUTPUT_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Strip_cutting_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].STRIP_CUTTING_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Multi_picker_1_pickup_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].MULTI_PICKER_1_PICKUP_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Multi_picker_2_pickup_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].MULTI_PICKER_2_PICKUP_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Multi_picker_1_place_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].MULTI_PICKER_1_PLACE_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Multi_picker_2_place_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].MULTI_PICKER_2_PLACE_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Good_table_1_tray_work_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].GOOD_TABLE_1_TRAY_WORK_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Good_table_2_tray_work_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].GOOD_TABLE_2_TRAY_WORK_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Rework_table_tray_work_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].REWORK_TABLE_TRAY_WORK_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Mark_vision_ok_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].MARK_VISION_OK_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Mark_vision_rw_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].MARK_VISION_RW_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Ball_vision_ok_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].BALL_VISION_OK_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Ball_vision_ng_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].BALL_VISION_NG_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Total_chip_prod_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].TOTAL_CHIP_PROD_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Total_ok_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].TOTAL_OK_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Total_rw_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].TOTAL_RW_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Total_ng_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].TOTAL_NG_COUNT = nValue;

                            if (!int.TryParse(dr.ItemArray[(int)ProcQtyLog.ColName.Its_count].ToString(), out nValue)) nValue = 0;
                            Info[nCnt].ITS_XMARK_COUNT = nValue;

                            Info[nCnt].EQP_RUNNING_TIME = dr.ItemArray[(int)ProcQtyLog.ColName.Eqp_running_time].ToString();
                            Info[nCnt].EQP_STOP_TIME = dr.ItemArray[(int)ProcQtyLog.ColName.Eqp_stop_time].ToString();

                            Info[nCnt].EQP_IN_TIME = dr.ItemArray[(int)ProcQtyLog.ColName.Eqp_in_time].ToString();
                            Info[nCnt].EQP_OUT_TIME = dr.ItemArray[(int)ProcQtyLog.ColName.Eqp_out_time].ToString();
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
