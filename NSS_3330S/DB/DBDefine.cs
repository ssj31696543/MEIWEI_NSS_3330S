using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S
{
    public class DBDF
    {
        #region LOG_INFO
        public static string[] LogDbName = new string[] {
            "LOG_SEQ_LD_MZ_LD_CONV",

            "LOG_SEQ_LD_MZ_ELV_TRANSFER",

            "LOG_SEQ_STRIP_TRANSFER",
            "LOG_SEQ_UNIT_TRANSFER",

            "LOG_SEQ_DRY_TABLE",

            "LOG_SEQ_MAP_TRANSFER",
            "LOG_SEQ_MAP_VISION_TABLE_1",
            "LOG_SEQ_MAP_VISION_TABLE_2",

            "LOG_SEQ_PKG_PICKnPLACE1",
            "LOG_SEQ_PKG_PICKnPLACE2",

            "LOG_SEQ_ULD_GD1_TRAY_TABLE",
            "LOG_SEQ_ULD_GD2_TRAY_TABLE", 
            "LOG_SEQ_ULD_RW_TRAY_TABLE", //UNLOAD_TRAY_TABLE_2_1,

            "LOG_SEQ_TRAY_TRANSFER",

            "LOG_SEQ_ULD_ELV_GOOD_1",
            "LOG_SEQ_ULD_ELV_GOOD_2",
            "LOG_SEQ_ULD_ELV_REWORK",
            "LOG_SEQ_ULD_ELV_EMPTY_1",
            "LOG_SEQ_ULD_ELV_EMPTY_2",

            "LOG_SEQ_MANUAL_RUN",
            "LOG_SEQ_ALWAYS_WORK",
            "LOG_SEQ_AUTO_LOT_END",
            
            "LOG_SEQ_CIM_MANAGER",
            "LOG_SEQ_CIM_READ_DATA",

            "LOG_SEQ_TEN_KEY_MON",
            "LOG_SEQ_VISION_MOVE",

            "LOG_IF_SAW", 
            "LOG_IF_VISION",
            "LOG_IF_MES"
        };

        public enum LOG_TYPE
        {
            SEQ_LD_MZ_LD_CONV = 0,
            // LOADER
            SEQ_LD_MZ_ELV_TRANSFER,

            SEQ_STRIP_TRANSFER,
            SEQ_UNIT_TRANSFER,

            SEQ_DRY_TABLE,
            SEQ_MAP_TRANSFER,
            SEQ_MAP_VISION_TABLE_1,
            SEQ_MAP_VISION_TABLE_2,

            SEQ_PICK_N_PLACE_1,
            SEQ_PICK_N_PLACE_2,

            SEQ_UNLOAD_GD1_TRAY_TABLE,
            SEQ_UNLOAD_GD2_TRAY_TABLE,
            SEQ_UNLOAD_RW_TRAY_TABLE, //UNLOAD_TRAY_TABLE_2_1,

            SEQ_TRAY_TRANSFER,

            SEQ_ULD_ELV_GOOD_1,
            SEQ_ULD_ELV_GOOD_2,
            SEQ_ULD_ELV_REWORK,
            SEQ_ULD_ELV_EMPTY_1,
            SEQ_ULD_ELV_EMPTY_2,


            SEQ_MANUAL_RUN,
            SEQ_ALWAYS_WORK,
            SEQ_AUTO_LOT_END,

            //CIM 
            SEQ_CIM_MANAGER,
            SEQ_CIM_READ_DATA,

            SEQ_TEN_KEY_MON,
            SEQ_VISION_MOVE,

            IF_SAW, 
            IF_VISION,
            IF_MES,
            MAX
        }
        #endregion

        // [2022.04.06. kmlee] 추가
        public enum E_TO_VISION
        {
            NO = 0,
            ITEM,
            VALUE,
        }

        // [2022.04.06. kmlee] 추가
        public enum E_TO_VISION_ALIGN
        {
            NO = 0,
            INSP_MODE,
        }

        // [2022.04.06. kmlee] 추가
        public enum E_TO_HANDLER_PICKER_CAL
        {
            NO = 0,
            VALUE,
        }

        // [2022.04.06. kmlee] 추가
        public enum E_TO_HANDLER_ALIGN
        {
            NO = 0,
            RESULT_1,
            RESULT_2,
            CODE,
        }

        // [2022.04.06. kmlee] 추가
        public enum E_TO_HANDLER_TOP_ALIGN
        {
            NO = 0,
            RESULT_1,
            RESULT_2,
        }

        public enum ePRE_ITEMS
        {
            NO = 0,
            RESULT_1,
            RESULT_2,
            ORIENTATION,
            CODE,
        }

        public enum MAIN_ITEMS
        {
            NO = 0,
            ID,
            START_INDEX,
            INSP_DIRECTION,
        }

        public enum RCP_EQ_ITEM
        {
            NO,
            NAME,
            MODIFIED,
            USE_OPTION,
            CONFIRM,
            BEFORE,
            AFTER
        }

        public enum eBALL_MAIN_ROW
        {
            HOST_CODE = 1,
            MAP_NG_CODE,
        }
    }

    public class LogProperties
    {
        public string Time
        {
            get;
            set;
        }
        public string Model
        {
            get;
            set;
        }

        #region 삭제 예정 20220106 pjh
        public string Level
        {
            get;
            set;
        }
        public string User_Id
        {
            get;
            set;
        }
        public string Cell_Id
        {
            get;
            set;
        }
        public string Log
        {
            get;
            set;
        }
        #endregion

        public string Command
        {
            get;
            set;
        }

        public string Part
        {
            get;
            set;
        }

        public string Status
        {
            get;
            set;
        }
    }

    public class AlarmProperties
    {
        public string Time
        {
            get;
            set;
        }
        public string No
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public string Cause
        {
            get;
            set;
        }
    }


    public class EventProperties
    {
        public string Time
        {
            get;
            set;
        }
        public string Section
        {
            get;
            set;
        }
        public string Detail
        {
            get;
            set;
        }
    }

    public class TactTimeProperties
    {
        public string Process
        {
            get;
            set;
        }
        public string StartTime
        {
            get;
            set;
        }
        public string EndTime
        {
            get;
            set;
        }
        public string TactTime
        {
            get;
            set;
        }
    }

    public class SeqProperties
    {
        public enum Col
        {
            NO,
            seqLdMzLoading,
            seqLdMzLotStart,
            seqStripRail,
            seqStripTransfer,
            seqSawCuttingTable1,
            seqSawCuttingTable2,
            seqUnitTransfer,
            seqUnitCleaning,
            seqUnitDry,
            seqMapTransfer,
            seqMapVisionTable1,
            seqMapVisionTable2,
            seqPkgPickNPlace1,
            seqPkgPickNPlace2,
            seqUldGoodTrayTable1,
            seqUldGoodTrayTable2,
            seqUldReworkTrayTable,
            seqUldRejectBoxTable,
            seqUldTrayTransfer,
            seqUldTrayElvGood1,
            seqUldTrayElvGood2,
            seqUldTrayElvRework,
            seqUldTrayElvEmpty1,
            seqUldTrayElvEmpty2,
            Max,
        }
    }

    public class LotProperties
    {
        public enum ColName
        {
            None = -1,
            Move_in_time,
            Move_out_time,
            Lot_id,
            Cycle_time,
            Carrier_id,
            Pkg_prod_count,
            Strip_count,
            Eqp_sq_chip_count,
            Eqp_nq_chip_count,
            Eqp_rj_chip_count,
            Eqp_sq_tray_count,
            Eqp_nq_tray_count,
            Sat_count,
            Total_chip_count,
            Mark_vision_err_count,
            Ball_vision_err_count,
            X_out_count,
            Host_sq_chip_count,
            Host_nq_chip_count,
            Host_rj_chip_count,
            Process_err_count,
            Strip_cutting_count,
            Unit_picker_place_count,
            Max,
        }
    }

    public class LotInfoPro
    {
        public enum ColName
        {
            None = -1, 
            Lot_id,
            Its_id,
            Lot_type,
            Sub_qty,
            prod_type,
            tool_no,
            its_usage,
            itslotid_in,
            itslotid_ct,
            unitsize_x,
            unitsize_y,
            unitupper,
            unitlower,
            thick,
            thick_upper,
            thick_lower,
            abf,
            landpkg_x,
            landpkgx_upper,
            landpkgx_lower,
            landpkg_y,
            landpkgy_upper,
            landpkgy_lower,
            Max,
        }
    }

    public class MagazineProperties
    {
        public string Time
        {
            get;
            set;
        }
        public string MagazineId
        {
            get;
            set;
        }
        public string StripId
        {
            get;
            set;
        }
    }

    public class StripProperties
    {
        public enum ColName
        {
            Time,
            InTime,
            OutTime,
            LotId,
            ItsId,
            StripId,
            MagazineSlotNo,
            CuttingTableNo,
            MapStageNo,
            RcpMat,
            HostCode,
            TopCoodi,
            BtmCoodi,
            TopNgCode,
            BtmNgCode,
            TopInspResult,
            BtmInspResult,
            TopPkgSize,
            BtmPkgSize,
            BtmSawOffset,
            OutPort,
            IsUnit,
        }

    }

    /// <summary>
    /// SeqId와 
    /// </summary>
    public class MccInfo
    {
        public enum ColName
        {
            None = -1,
            StripId,
            CycleTime,
            RailLoadStart,
            RailUnloadEnd,
            StripLoadStart,
            StripUnloadEnd,
            CuttingTable1LoadStart,
            CuttingTable1UnloadEnd,
            CuttingTable2LoadStart,
            CuttingTable2UnloadEnd,
            UnitLoadStart,
            UnitLoadEnd,
            FlipperLoadStart,
            FlipperLoadEnd,
            DryblockLoadStart,
            DryblockLoadEnd,
            MapPickerLoadStart,
            MapPickerLoadEnd,
            MapTable1LoadStart,
            MapTable1LoadEnd,
            MapTable2LoadStart,
            MapTable2LoadEnd,
        }

        public string StripId
        {
            get;
            set;
        }
        public string RailLoadStart
        {
            get;
            set;
        }
        public string RailUnloadEnd
        {
            get;
            set;
        }
        public string StripLoadStart
        {
            get;
            set;
        }
        public string StripUnloadEnd
        {
            get;
            set;
        }
        public string CuttingTable1LoadStart
        {
            get;
            set;
        }
        public string CuttingTable1UnloadEnd
        {
            get;
            set;
        }
        public string CuttingTable2LoadStart
        {
            get;
            set;
        }
        public string CuttingTable2UnloadEnd
        {
            get;
            set;
        }
        public string UnitLoadStart
        {
            get;
            set;
        }
        public string UnitUnloadEnd
        {
            get;
            set;
        }
        public string FlipperLoadStart
        {
            get;
            set;
        }
        public string FlipperUnloadEnd
        {
            get;
            set;
        }
        public string DryblockLoadStart
        {
            get;
            set;
        }
        public string DryblockUnloadEnd
        {
            get;
            set;
        }
        public string MapPickerLoadStart
        {
            get;
            set;
        }
        public string MapPickerUnloadEnd
        {
            get;
            set;
        }
        public string MapTable1LoadStart
        {
            get;
            set;
        }
        public string MapTable1UnloadEnd
        {
            get;
            set;
        }
        public string MapTable2LoadStart
        {
            get;
            set;
        }
        public string MapTable2UnloadEnd
        {
            get;
            set;
        }
    }

    public class ExLogProperties
    {
        public string Time
        {
            get;
            set;
        }
        public string FuncName
        {
            get;
            set;
        }
        public string Detail
        {
            get;
            set;
        }
    }

    public class LotBga
    {
        public enum ColName
        {
            NO,
            TOTAL_CNT,
            GOOD_CNT,
            REWORK_CNT,
            REJECT_CNT,
            LIGHT,
            ABSENCE,
            PKG_SIZE,
            SAW_OFFSET,
            MIS_REF,
            MIS_BALL,
            EXTRA_BALL,
            BALL_SIZE,
            BALL_POS,
            BALL_PITCH,
            BALL_DAMAGE,
            SCRATCH,
            FM,
            HOST_MAP_RJ,
            MAX,
        }


    }

    public class LotMap
    {
        public enum ColName
        {
            NO,
            TOTAL_CNT,
            GOOD_CNT,
            REWORK_CNT,
            REJECT_CNT,
            LIGHT,
            ABSENCE,
            PKG_SIZE,
            BURR,
            NO_MARK,
            MARK_CNT,
            MARK_POS,
            WRONG_CHAR,
            BROCKEN_CHAR,
            SCRATCH,
            FM,
            HOST_MAP_RJ,
            MAX,
        }

    }

    public class LotBin
    {
        public enum ColName
        {
            NO,
            TOTAL_CNT,
            GOOD_CNT,
            REWORK_CNT,
            REJECT_CNT,

            MAP_LIGHT,//
            MAP_PKG_NONE,//
            MAP_MISS_REF,//
            MAP_PKG_SIZE,//
            MAP_PKG_BURR,//
            MAP_NO_MARK,//
            MAP_MARK_COUNT,//
            MAP_MARK_POS,//
            MAP_WRONG_CHAR,//
            MAP_BROCKEN_CHAR,//
            MAP_SCRATCH,//
            MAP_FM,//

            BGA_LIGHT,//
            BGA_PKG_NONE,//
            BGA_MISS_REF,//
            BGA_PKG_SIZE,//
            BGA_SAW_OFFSET,
            BGA_MISS_BALL,
            BGA_EXTRA_BALL,
            BGA_BALL_SIZE,
            BGA_BALL_POS,
            BGA_BALL_PITCH,
            BGA_BALL_DAMAGE,
            BGA_SCRATCH,
            BGA_FM,
            HOST_MAP_RJ,
            MAX,
        }

    }

    public class LotVision
    {

        public enum ColName
        {
            NO,
            MODE,
            LOT_ID,
            OP_ID,
            PKG_NAME,
        }
    }

    public class ITS
    {
        public enum ColName
        {
            None = -1,
            LotNumber,
            SHIPTO,
            StripID,
            StripX,
            StripY,
            XOUT_X,
            XOUT_Y,
            LAngle,
            Max,
        }
    }

    public class ITS_LOG
    {
        public enum ColName
        {
            None = -1,
            InTime,
            LotNumber,
            SHIPTO,
            StripID,
            StripX,
            StripY,
            XOUT_X,
            XOUT_Y,
            LAngle,
            Lot_Id,
            Max,
        }
    }

    public struct tagRetAlignData
    {
        public bool bJudge;
        public double dX;
        public double dY;
        public double dT;

        public bool SetData(string strArray)
        {
            string[] arr = strArray.Split(',');

            if(arr.Length < 3)
                return false;

            bJudge = false;
            dX = 0.0;
            dY = 0.0;
            dT = 0.0;

            if (arr.Length > 0)
            {
                bJudge = arr[0] == "1";
            }

            if (arr.Length > 1)
            {
                if (!double.TryParse(arr[1], out dX))
                    return false;
            }

            if (arr.Length > 2)
            {
                if (!double.TryParse(arr[2], out dY))
                    return false;

            }

            if (arr.Length > 3)
            {
                if (!double.TryParse(arr[3], out dT))
                    return false;
            }

            return true;
        }
    }

	//220611 pjh
    public class ProcQtyLog
    {
        public enum ColName
        {
            None = -1,
            Idx,
            LotId,
            ItsId,
            Mgz_Count,
            Strip_input_count,
            Strip_output_count,
            Strip_cutting_count,
            Multi_picker_1_pickup_count,
            Multi_picker_2_pickup_count,
            Multi_picker_1_place_count,
            Multi_picker_2_place_count,
            Good_table_1_tray_work_count,
            Good_table_2_tray_work_count,
            Rework_table_tray_work_count,
            Mark_vision_ok_count,
            Mark_vision_rw_count,
            Ball_vision_ok_count,
            Ball_vision_ng_count,
            Total_chip_prod_count,
            Total_ok_count,
            Total_rw_count,
            Total_ng_count,
            Its_count,
            Eqp_running_time,
            Eqp_stop_time,
            Eqp_in_time,
            Eqp_out_time,
            Max,
        }
    }


    public class ProcQtyInfo
    {
        public enum ColName
        {
            None = -1,
            LotId,
            ItsId,
            Mgz_Count,
            Strip_input_count,
            Strip_output_count,
            Strip_cutting_count,
            Multi_picker_1_pickup_count,
            Multi_picker_2_pickup_count,
            Multi_picker_1_place_count,
            Multi_picker_2_place_count,
            Good_table_1_tray_work_count,
            Good_table_2_tray_work_count,
            Rework_table_tray_work_count,
            Mark_vision_ok_count,
            Mark_vision_rw_count,
            Ball_vision_ok_count,
            Ball_vision_ng_count,
            Total_chip_prod_count,
            Total_ok_count,
            Total_rw_count,
            Total_ng_count,
            Its_count,
            Eqp_running_time,
            Eqp_stop_time,
            Strip_loss_count,
            Max,
        }
    }

    public class StripProcInfo
    {
        public enum ColName
        {
            Strip_id,
            Panel_Idx,
            Line_In,
            Mdl_In,
            Mdl_Out,
            Line_Out,
            Max,
        }
    }
}
