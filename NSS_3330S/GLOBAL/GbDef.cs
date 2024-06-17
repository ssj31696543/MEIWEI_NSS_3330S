using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using DWORD = System.Int32;
using WORD = System.Int16;
using DionesTool.Objects;
using System.Runtime.CompilerServices;

namespace NSS_3330S
{
    public class CommonEvent
    {
        public delegate void FinishSeqEvent(int nResult);
    }

    public enum fn : int
    {
        //err = -1,
        //busy,
        //success
        busy = -1,
        success,
        //에러는 0을 제외한 양수
    };

    public enum MCC
    {
        LOAD_RAIL = 0,
        STRIP_PICKER,
        TURN_TABLE,
        UNIT_PICKER,
        CLEANER,
        DRY_BLOCK,
        MAP_PICKER,
        VISION_TABLE1,
        VISION_TABLE2,
        MAX,
    }

    public class FNC
    {
        //Function Result
        public const int SUCCESS = 0;
        public const int BUSY = -1;
        public const int CYCLE_CHECK = -2;
        //ERROR는 0을 제외한 양수

        public static bool IsErr(int nRetVal)
        {
            if (nRetVal > 0) return true;
            return false;
        }

        public static bool IsSuccess(int fRet)
        {
            if (fRet == 0) return true;
            return false;
        }

        public static bool IsBusy(int fRet)
        {
            if (fRet == -1) return true;
            return false;
        }

        public static bool IsCycleCheck(int fRet)
        {
            if (fRet == -2) return true;
            return false;
        }
    }

    public enum DOWNLOAD_MODE
    {
        RECIPE_DATA,
        CONFIG_DATA,
    }

    #region POSITION
    public static class POSDF
    {
        public const int MAX_POS_COUNT = 100;

        public enum TEACH_POS_MODE
        {
            NONE = 0,
            MGZ_LD_ELEV,
            LD_RAIL_T,
            LD_RAIL_Y,
            BARCODE_Y,
            LD_VISION_Y,
            STRIP_PICKER,
            UNIT_PICKER,
            DRY_BLOCK_STAGE,
            MAP_PICKER,
            MAP_STAGE,
            CHIP_PICKER,
            BALL_VISION,
            TRAY_STAGE,
            MAP_VISION,
            TRAY_PICKER,
            TRAY_ELEV,
            MAX
        }

        public enum CFG_POS_MODE
        {
            NONE = 0,
            PICKER_HEAD_OFFSET,
            VISION_TABLE_OSSET,
            ULD_TRAY_TABLE_OFFSET,
            PICKER_ZEROSET,
            TOP_CAM_SCALE,
            MAX
        }

        public enum FUNC_POS_MODE
        {
            NONE = 0,
            MAX
        }

        public static string[,] strRcpPosName = null;
        public static string[,] strCfgPosName = null;
        public static string[,] strFncPosName = null;

        #region TEACH_POS
        #region MGZ_LD_ELEV
        public const int MGZ_LD_ELEV_READY = 0;
        public const int MGZ_LD_ELEV_POSITION_LD_1F_1 = 1;
        public const int MGZ_LD_ELEV_POSITION_ULD_2F_1 = 2;
        public const int MGZ_LD_ELEV_STRIP_SUPPLY = 3;
        public const int MGZ_LD_ELEV_QUAD_SUPPLY = 4;
        #endregion

        #region MGZ_LD_PUSHER
        public const int MGZ_LD_PUSHER_READY = 0;
        public const int MGZ_LD_PUSHER_STRIP_PUSH_FWD = 1;
        public const int MGZ_LD_PUSHER_STRIP_PUSH_BWD = 2;
        #endregion

        /// <summary>
        /// 신규 추가 유닛
        /// </summary>
        #region RAIL GRIPPER X
        public const int LD_RAIL_GRIPPER_READY = 0;
        public const int LD_RAIL_GRIPPER_PRE_GRIP = 1;
        public const int LD_RAIL_GRIPPER_GRIP = 2; // 자재 로드
        public const int LD_RAIL_GRIPPER_BOT_BARCODE = 3;
        public const int LD_RAIL_GRIPPER_UNGRIP = 4; // 자재를 그립해서 가져온 뒤 그립을 푸는 위치
        public const int LD_RAIL_GRIPPER_INLET_LOAD = 5; // 언그립 상태로 자재를 밀어서 IN-LET 테이블에 놓는 위치

        #endregion

        /// <summary>
        /// 신규 추가 유닛
        /// </summary>
        #region BOTTOM BARCODE Y
        public const int BOT_BARCDOE_READY = 0;
        public const int BOT_BARCDOE_SCAN = 1;
        #endregion

        #region LD_RAIL
        public const int LD_RAIL_T_READY = 0;
        public const int LD_RAIL_T_STRIP_LOADING = 1;
        public const int LD_RAIL_T_STRIP_ALIGN = 2;
        public const int LD_RAIL_T_STRIP_UNLOADING = 3;
        public const int LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK = 4; // 매거진 공급 시 클립 해제 체크하는 위치
        public const int LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK_STRIP_MODE = 5;
        #endregion

        #region LD_VISION_X
        public const int LD_VISION_X_READY = 0;
        public const int LD_VISION_X_STRIP_GRIP = 1;
        public const int LD_VISION_X_STRIP_ALIGN_1 = 2;
        public const int LD_VISION_X_STRIP_ALIGN_2 = 3;
        public const int LD_VISION_X_STRIP_ORIENT_CHECK = 4;
        public const int LD_VISION_X_STRIP_BARCODE = 5;
        #endregion

        #region STRIP_PICKER
        public const int STRIP_PICKER_READY = 0;
        public const int STRIP_PICKER_STRIP_GRIP_SAFETY = 1;
        public const int STRIP_PICKER_STRIP_GRIP = 2;
        public const int STRIP_PICKER_STRIP_UNGRIP = 3;
        public const int STRIP_PICKER_STRIP_UNGRIP_SAFETY = 4;
        public const int STRIP_PICKER_STRIP_PREALIGN_1 = 5;
        public const int STRIP_PICKER_STRIP_PREALIGN_2 = 6;
        public const int STRIP_PICKER_STRIP_ORIENT_CHECK = 7;
        public const int STRIP_PICKER_STRIP_2D_CODE = 8;
        public const int STRIP_PICKER_STRIP_LOADING = 9;
        public const int STRIP_PICKER_UNLOADING_TABLE_1 = 10;

        public const int STRIP_PICKER_STRIP_BTM_BARCODE = 11;
        public const int STRIP_PICKER_STRIP_PUSH_DOWN = 12;
        public const int STRIP_PICKER_UNLOADING_TABLE_1_WITHOUT_PREALIGN = 13;
        #endregion

        #region UNIT_PICKER
        public const int UNIT_PICKER_READY = 0;
        public const int UNIT_PICKER_LOADING_TABLE_1 = 1;

        public const int UNIT_PICKER_SCRAP_1 = 2;
        public const int UNIT_PICKER_SCRAP_2 = 3;
        public const int UNIT_PICKER_SPONGE_START = 4;
        public const int UNIT_PICKER_SPONGE_END = 5;
        public const int UNIT_PICKER_WATER_AIR_SWING = 6;
        public const int UNIT_PICKER_STRIP_UNLOADING = 7;

        public const int UNIT_PICKER_AIR_KNIFE_START = 10;
        public const int UNIT_PICKER_AIR_KNIFE_END = 11;

        #endregion

        #region CLEANER_Z
        public const int CLEANER_Z_READY = 0;
        public const int CLEANER_Z_STRIP_LOADING = 1;
        public const int CLEANER_Z_STRIP_FLIP = 2;
        public const int CLEANER_Z_STRIP_CLEANING = 3;
        public const int CLEANER_Z_STRIP_DRY = 4;
        public const int CLEANER_Z_STRIP_UNLOADING = 5;

        public const int CLEANER_Z_TOOL_READY = 6;
        public const int CLEANER_Z_TOOL_LOADING = 7;
        public const int CLEANER_Z_TOOL_FLIP_SAFETY = 8;
        public const int CLEANER_Z_TOOL_UNLOADING = 9;
        #endregion

        #region CLEANER_FLIP
        public const int CLEANER_FLIP_READY = 0;
        public const int CLEANER_FLIP_STRIP_FLIP_FRONT = 1;
        public const int CLEANER_FLIP_STRIP_FLIP_REAR = 2;

        public const int CLEANER_FLIP_TOOL_READY = 3;
        public const int CLEANER_FLIP_TOOL_FRONT = 4;
        public const int CLEANER_FLIP_TOOL_REAR = 5;
        #endregion

        #region DRY_BLOCK_STAGE
        public const int DRY_BLOCK_STAGE_READY = 0;
        public const int DRY_BLOCK_STAGE_STRIP_BTM_DRY_START = 1;
        public const int DRY_BLOCK_STAGE_STRIP_BTM_DRY_END = 2;
        public const int DRY_BLOCK_STAGE_STRIP_LOADING = 3;
        public const int DRY_BLOCK_STAGE_STRIP_UNLOADING = 4;
        public const int DRY_BLOCK_STAGE_STRIP_TOP_DRY_START = 5;
        public const int DRY_BLOCK_STAGE_STRIP_TOP_DRY_END = 6;
        #endregion

        #region MAP_PICKER
        public const int MAP_PICKER_READY = 0;
        public const int MAP_PICKER_STRIP_LOADING = 1;
        public const int MAP_PICKER_MAP_VISION_START_T1 = 2; // 이 번호 바꾸려면 MAP STAGE 번호도 함께 변경해야 합니다.
        public const int MAP_PICKER_MAP_VISION_END_T1 = 3;//셀 맵 정보로 자동계산
        public const int MAP_PICKER_MAP_VISION_START_T2 = 4;
        public const int MAP_PICKER_MAP_VISION_END_T2 = 5;//셀 맵 정보로 자동계산
        public const int MAP_PICKER_STRIP_UNLOADING_STAGE_1 = 6;
        public const int MAP_PICKER_STRIP_UNLOADING_STAGE_2 = 7;
        public const int MAP_PICKER_MAP_VISION_ALIGN_T1 = 8;
        public const int MAP_PICKER_MAP_VISION_ALIGN_T2 = 9;
        public const int MAP_PICKER_AIR_DRY_START = 10;//220507 pjh 추가
        public const int MAP_PICKER_AIR_DRY_END = 11;//220507 pjh 추가
        public const int MAP_PICKER_T1_LEFT_TOP_UNIT_POS = 12;//220517
        public const int MAP_PICKER_T2_LEFT_TOP_UNIT_POS = 13;//220517
        #endregion

        #region MAP_STAGE
        public const int MAP_STAGE_READY = 0;
        public const int MAP_STAGE_STRIP_LOADING = 1;
        public const int MAP_STAGE_MAP_VISION_START = 2;
        public const int MAP_STAGE_MAP_VISION_END = 3;

        public const int MAP_STAGE_UNIT_UNLOADING_P1 = 4;
        public const int MAP_STAGE_UNIT_UNLOADING_P2 = 5;

        public const int MAP_STAGE_UNIT_CLEAN_START = 6;
        public const int MAP_STAGE_UNIT_CLEAN_END = 7;

        public const int MAP_STAGE_TOP_ALIGN_MARK1 = 8;
        public const int MAP_STAGE_TOP_ALIGN_MARK2 = 9;

        public const int MAP_STAGE_ROATE = 10;
        public const int MAP_STAGE_PICKUP_MOVE = 11;
        public const int MAP_STAGE_LEFT_TOP_POS = 12;

        public const int MAP_STAGE_UNIT_UNLOADING_VISION_P1 = 13;
        public const int MAP_STAGE_UNIT_UNLOADING_VISION_P2 = 14;

        public const int MAP_STAGE_AIR_KNIFE_START = 15;
        public const int MAP_STAGE_AIR_KNIFE_END = 16;
        #endregion

        #region CHIP_PICKER
        public const int CHIP_PICKER_READY = 0;
        public const int CHIP_PICKER_CHIP_LOADING_REF_T1 = 1;
        public const int CHIP_PICKER_CHIP_LOADING_REF_T2 = 2;

        public const int CHIP_PICKER_CHIP_BGA_VISION_REF = 3;

        public const int CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 = 4;
        public const int CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2 = 5;

        public const int CHIP_PICKER_CHIP_UNLOADING_REWORK_REF = 6;
        public const int CHIP_PICKER_CHIP_UNLOADING_BIN = 7;

        public const int CHIP_PICKER_CHIP_LOADING_VISION_T1 = 8;
        public const int CHIP_PICKER_CHIP_LOADING_VISION_T2 = 9;

        // 트레이 피치를 구하기 위한 포지션
        public const int CHIP_PICKER_TRAY_PITCH_LT = 11;
        public const int CHIP_PICKER_TRAY_PITCH_RB = 12;

        public const int CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_VISION_T1 = 14;
        public const int CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_VISION_T2 = 15;

        public const int CHIP_PICKER_CHIP_UNLOADING_REWORK_VISION_REF = 16;
        #endregion

        #region BALL_VISION
        public const int BALL_VISION_READY = 0;
        public const int BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 = 1;
        public const int BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2 = 2;
        #endregion

        #region TRAY_STAGE
        public const int TRAY_STAGE_READY = 0;
        //public const int TRAY_STAGE_TRAY_LOADING = 1;

        public const int TRAY_STAGE_TRAY_WORKING_P1 = 2;
        public const int TRAY_STAGE_TRAY_WORKING_P2 = 3;
        
        public const int TRAY_STAGE_TRAY_UNLOADING = 4;

        public const int TRAY_STAGE_TRAY_INSPECTION_START = 5;

        public const int TRAY_STAGE_TRAY_WORKING_VISION_P1 = 12;
        public const int TRAY_STAGE_TRAY_WORKING_VISION_P2 = 13;
        #endregion

        #region MAP_VISION
        public const int MAP_VISION_READY = 0;
        public const int MAP_VISION_FOCUS_T1 = 1;
        public const int MAP_VISION_FOCUS_T2 = 2;
        #endregion

        #region REJECT_BOX
        public const int REJECT_BOX_READY = 0;

        public const int REJECT_BOX_REJECT_BIN_1_P1 = 1;
        public const int REJECT_BOX_REJECT_BIN_1_P2 = 2;

        public const int REJECT_BOX_REJECT_BIN_2_P1 = 3;
        public const int REJECT_BOX_REJECT_BIN_2_P2 = 4;

        public const int REJECT_BOX_REJECT_BIN_3_P1 = 5;
        public const int REJECT_BOX_REJECT_BIN_3_P2 = 6;

        public const int REJECT_BOX_REJECT_BIN_4_P1 = 7;
        public const int REJECT_BOX_REJECT_BIN_4_P2 = 8;

        public const int REJECT_BOX_REJECT_BOX_EJECT = 9;
        #endregion

        #region TRAY_PICKER
        public const int TRAY_PICKER_READY = 0;
        public const int TRAY_PICKER_TRAY_ELV_EMPTY_1 = 1;
        public const int TRAY_PICKER_TRAY_ELV_EMPTY_2 = 2;

        public const int TRAY_PICKER_STAGE_GOOD_1 = 3;
        public const int TRAY_PICKER_STAGE_GOOD_2 = 4;
        public const int TRAY_PICKER_STAGE_REWORK = 5;

        public const int TRAY_PICKER_TRAY_ELV_GOOD_1 = 6;
        public const int TRAY_PICKER_TRAY_ELV_GOOD_2 = 7;
        public const int TRAY_PICKER_TRAY_ELV_REWORK = 8;
        public const int TRAY_PICKER_TRAY_GOOD_1_INSPECTION = 9;
        public const int TRAY_PICKER_TRAY_GOOD_2_INSPECTION = 10;
        public const int TRAY_PICKER_TRAY_REWORK_INSPECTION = 11;
        #endregion

        #region TRAY_ELEV
        public const int TRAY_ELEV_READY = 0;
        public const int TRAY_ELEV_TRAY_LOADING = 1;
        public const int TRAY_ELEV_TRAY_OHT_LIFT_IN = 2;
        public const int TRAY_ELEV_TRAY_OHT_LIFT_OUT = 3;
        public const int TRAY_ELEV_TRAY_UNLOADING = 4;
        #endregion

        #endregion

        #region CFG_POS
        public const int CFG_PICKER_HEAD_1_PITCH_OFFSET_READY = 0;
        public const int CFG_PICKER_HEAD_2_PITCH_OFFSET_READY = 1;

        public const int CFG_PICKER_HEAD_1_VISION_OFFSET_READY = 2;
        public const int CFG_PICKER_HEAD_2_VISION_OFFSET_READY = 3;


        public const int CFG_MAP_TABLE_1_OFFSET_START_P1 = 10;
        public const int CFG_MAP_TABLE_1_OFFSET_LAST_X_P1 = 11;
        public const int CFG_MAP_TABLE_1_OFFSET_LAST_Y_P1 = 12;
        public const int CFG_MAP_TABLE_2_OFFSET_START_P1 = 13;
        public const int CFG_MAP_TABLE_2_OFFSET_LAST_X_P1 = 14;
        public const int CFG_MAP_TABLE_2_OFFSET_LAST_Y_P1 = 15;

        public const int CFG_MAP_TABLE_1_OFFSET_START_P2 = 20;
        public const int CFG_MAP_TABLE_1_OFFSET_LAST_X_P2 = 21;
        public const int CFG_MAP_TABLE_1_OFFSET_LAST_Y_P2 = 22;
        public const int CFG_MAP_TABLE_2_OFFSET_START_P2 = 23;
        public const int CFG_MAP_TABLE_2_OFFSET_LAST_X_P2 = 24;
        public const int CFG_MAP_TABLE_2_OFFSET_LAST_Y_P2 = 25;

        public const int CFG_MAP_TABLE_1_OFFSET_START_MV = 30;
        public const int CFG_MAP_TABLE_1_OFFSET_LAST_X_MV = 31;
        public const int CFG_MAP_TABLE_1_OFFSET_LAST_Y_MV = 32;
        public const int CFG_MAP_TABLE_2_OFFSET_START_MV = 33;
        public const int CFG_MAP_TABLE_2_OFFSET_LAST_X_MV = 34;
        public const int CFG_MAP_TABLE_2_OFFSET_LAST_Y_MV = 35;

        public const int CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 = 40;
        public const int CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1 = 41;
        public const int CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1 = 42;
        public const int CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1 = 43;
        public const int CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P1 = 44;
        public const int CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P1 = 45;
        public const int CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1 = 46;
        public const int CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P1 = 47;
        public const int CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P1 = 48;

        public const int CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 = 50;
        public const int CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2 = 51;
        public const int CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2 = 52;
        public const int CFG_ULD_TRAY_TABLE_2_OFFSET_START_P2 = 53;
        public const int CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P2 = 54;
        public const int CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P2 = 55;
        public const int CFG_ULD_TRAY_TABLE_3_OFFSET_START_P2 = 56;
        public const int CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P2 = 57;
        public const int CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P2 = 58;

        public const int CFG_PICKER_HEAD_1_ZEROSET_POS_4_PICKUP = 60;
        public const int CFG_PICKER_HEAD_2_ZEROSET_POS_4_PICKUP = 61;
        public const int CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE = 62;
        public const int CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE = 63;

        public const int CFG_TOP_CAM_SCALE_MAP_TABLE_1_POS = 64;
        public const int CFG_TOP_CAM_SCALE_MAP_TABLE_2_POS = 65;

        public const int CFG_PICKER_HEAD_1_ZEROSET_POS_4_PICKUP_T2 = 66;
        public const int CFG_PICKER_HEAD_2_ZEROSET_POS_4_PICKUP_T2 = 67;
        public const int CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE_T2 = 68;
        public const int CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE_T2 = 69;
        public const int CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE_RW = 70;
        public const int CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE_RW = 71;
        #endregion


        static POSDF()
        {
            #region INIT
            strRcpPosName = new string[(int)TEACH_POS_MODE.MAX, MAX_POS_COUNT];

            for (int nCntMode = 0; nCntMode < strRcpPosName.GetLength(0); nCntMode++)
            {
                for (int nCntPos = 0; nCntPos < strRcpPosName.GetLength(1); nCntPos++)
                {
                    strRcpPosName[nCntMode, nCntPos] = "";
                }
            }

            strCfgPosName = new string[(int)CFG_POS_MODE.MAX, MAX_POS_COUNT];

            for (int nCntMode = 0; nCntMode < strCfgPosName.GetLength(0); nCntMode++)
            {
                for (int nCntPos = 0; nCntPos < strCfgPosName.GetLength(1); nCntPos++)
                {
                    strCfgPosName[nCntMode, nCntPos] = "";
                }
            }

            strFncPosName = new string[(int)FUNC_POS_MODE.MAX, MAX_POS_COUNT];

            for (int nCntMode = 0; nCntMode < strFncPosName.GetLength(0); nCntMode++)
            {
                for (int nCntPos = 0; nCntPos < strFncPosName.GetLength(1); nCntPos++)
                {
                    strFncPosName[nCntMode, nCntPos] = "";
                }
            }
            #endregion

            #region SET TEACH POSITION NAME
            #region MGZ_LD_ELEV
            int i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.MGZ_LD_ELEV, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.MGZ_LD_ELEV, i++] = "POSITION_1F";
            strRcpPosName[(int)TEACH_POS_MODE.MGZ_LD_ELEV, i++] = "POSITION_2F";
            strRcpPosName[(int)TEACH_POS_MODE.MGZ_LD_ELEV, i++] = "STRIP_SUPPLY";
            strRcpPosName[(int)TEACH_POS_MODE.MGZ_LD_ELEV, i++] = "QUAD_SUPPLY";
            #endregion

            #region LD_RAIL
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_T, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_T, i++] = "STRIP_LOADING";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_T, i++] = "STRIP_ALIGN";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_T, i++] = "STRIP_UNLOADING";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_T, i++] = "MAGAZINE_CLIP_OPEN_CHECK";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_T, i++] = "MAGAZINE_CLIP_OPEN_CHECK_STRIP_MODE";

            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_Y, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_Y, i++] = "STRIP_LOADING";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_Y, i++] = "STRIP_ALIGN";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_Y, i++] = "STRIP_UNLOADING";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_Y, i++] = "MAGAZINE_CLIP_OPEN_CHECK";
            strRcpPosName[(int)TEACH_POS_MODE.LD_RAIL_Y, i++] = "MAGAZINE_CLIP_OPEN_CHECK_STRIP_MODE";
            #endregion

            #region BARCODE_Y
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.BARCODE_Y, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.BARCODE_Y, i++] = "SCAN";
            #endregion

            #region LD_VISION_Y
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.LD_VISION_Y, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.LD_VISION_Y, i++] = "STRIP_GRIP";
            strRcpPosName[(int)TEACH_POS_MODE.LD_VISION_Y, i++] = "STRIP_ALIGN_1";
            strRcpPosName[(int)TEACH_POS_MODE.LD_VISION_Y, i++] = "STRIP_ALIGN_2";
            strRcpPosName[(int)TEACH_POS_MODE.LD_VISION_Y, i++] = "STRIP_ORIENTATION_CHECK";
            strRcpPosName[(int)TEACH_POS_MODE.LD_VISION_Y, i++] = "STRIP_BARCODE";
            #endregion
            
            #region STRIP_PICKER
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_GRIP_SAFETY";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_GRIP";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_UNGRIP";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_UNGRIP_SAFETY";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_PREALIGN_1";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_PREALIGN_2";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_ORIENTATION_CHECK";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_2D_CODE";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "LOADING_STRIP";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "UNLOADING_STRIP";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_BOTTOM_BARCODE";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_PUSH_DOWN";
            strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "UNLOADING_STRIP_WITHOUT_PREALIGN";

            //strRcpPosName[(int)TEACH_POS_MODE.STRIP_PICKER, i++] = "STRIP_PUSH_DOWN";
            #endregion

            #region UNIT_PICKER
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.UNIT_PICKER, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.UNIT_PICKER, i++] = "LOADING_UNIT";
            strRcpPosName[(int)TEACH_POS_MODE.UNIT_PICKER, i++] = "SCRAP_1";
            strRcpPosName[(int)TEACH_POS_MODE.UNIT_PICKER, i++] = "SCRAP_2";
            strRcpPosName[(int)TEACH_POS_MODE.UNIT_PICKER, i++] = "SPONGE_SWING_START";
            strRcpPosName[(int)TEACH_POS_MODE.UNIT_PICKER, i++] = "SPONGE_SWING_END";
            strRcpPosName[(int)TEACH_POS_MODE.UNIT_PICKER, i++] = "WATER_AIR_SWING";
            strRcpPosName[(int)TEACH_POS_MODE.UNIT_PICKER, i++] = "UNLOADING_UNIT";
            #endregion

            #region DRY_BLOCK_STAGE
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.DRY_BLOCK_STAGE, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.DRY_BLOCK_STAGE, i++] = "STRIP_BTM_DRY_START";
            strRcpPosName[(int)TEACH_POS_MODE.DRY_BLOCK_STAGE, i++] = "STRIP_BTM_DRY_END";
            strRcpPosName[(int)TEACH_POS_MODE.DRY_BLOCK_STAGE, i++] = "STRIP_LOADING";
            strRcpPosName[(int)TEACH_POS_MODE.DRY_BLOCK_STAGE, i++] = "STRIP_UNLOADING";
            strRcpPosName[(int)TEACH_POS_MODE.DRY_BLOCK_STAGE, i++] = "STRIP_TOP_DRY_START";
            strRcpPosName[(int)TEACH_POS_MODE.DRY_BLOCK_STAGE, i++] = "STRIP_TOP_DRY_END";
            #endregion

            #region MAP_PICKER
        i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "STRIP_LOADING";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "MAP_VISION_START_T1";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "MAP_VISION_END_T1";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "MAP_VISION_START_T2";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "MAP_VISION_END_T2";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "STRIP_UNLOADING_STAGE_1";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "STRIP_UNLOADING_STAGE_2";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "MAP_VISION_ALIGN_T1";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "MAP_VISION_ALIGN_T2";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "AIR_DRY_START";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "AIR_DRY_END";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "T1_LEFT_TOP_UNIT_VIEW";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_PICKER, i++] = "T2_LEFT_TOP_UNIT_VIEW";
            #endregion

            #region MAP_STAGE
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "STRIP_LOADING";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_VISION_START";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_VISION_END";

            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "UNIT_UNLOADING_P1";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "UNIT_UNLOADING_P2";

            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "UNIT_CLEAN_START";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "UNIT_CLEAN_END";

			// [2022.04.22.kmlee] Top Align 시에 Table을 움직여서 Mark를 찍는다
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_STAGE_ALIGN_MARK1";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_STAGE_ALIGN_MARK2";

            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_STAGE_ROTATE";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_STAGE_PICKUP_MOVE";

            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "LEFT_TOP_UNIT_VIEW";

            //strRcpPosName[(int)RCP_POS_MODE.MAP_STAGE, i++] = "APC_LOADING_REF";  //FUNCTION POS MODE로 추가해야함
            //strRcpPosName[(int)RCP_POS_MODE.MAP_STAGE, i++] = "APC_UNLOADING_REF";//FUNCTION POS MODE로 추가해야함

            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_STAGE_UNIT_UNLOADING_VISION_P1";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_STAGE_UNIT_UNLOADING_VISION_P2";

            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_STAGE_AIR_KNIFE_START";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_STAGE, i++] = "MAP_STAGE_AIR_KNIFE_END";

            #endregion

            #region CHIP_PICKER
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.CHIP_PICKER, i++] = "READY";

            strRcpPosName[(int)TEACH_POS_MODE.CHIP_PICKER, i++] = "CHIP_LOADING_REF_T1";
            strRcpPosName[(int)TEACH_POS_MODE.CHIP_PICKER, i++] = "CHIP_LOADING_REF_T2";

            strRcpPosName[(int)TEACH_POS_MODE.CHIP_PICKER, i++] = "CHIP_BGA_VISION_REF";

            strRcpPosName[(int)TEACH_POS_MODE.CHIP_PICKER, i++] = "CHIP_UNLOADING_GOOD_REF_T1";
            strRcpPosName[(int)TEACH_POS_MODE.CHIP_PICKER, i++] = "CHIP_UNLOADING_GOOD_REF_T2";

            strRcpPosName[(int)TEACH_POS_MODE.CHIP_PICKER, i++] = "CHIP_UNLOADING_REWORK_REF";
            strRcpPosName[(int)TEACH_POS_MODE.CHIP_PICKER, i++] = "CHIP_UNLOADING_BIN";
            #endregion
            
            #region BALL_VISION
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.BALL_VISION, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.BALL_VISION, i++] = "CHIP_BGA_INSPECTION_HEAD_1";
            strRcpPosName[(int)TEACH_POS_MODE.BALL_VISION, i++] = "CHIP_BGA_INSPECTION_HEAD_2";
            #endregion
            
            #region TRAY_STAGE
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_STAGE, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_STAGE, i++] = "TRAY_LOADING";

            strRcpPosName[(int)TEACH_POS_MODE.TRAY_STAGE, i++] = "TRAY_WORKING_P1";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_STAGE, i++] = "TRAY_WORKING_P2";

            strRcpPosName[(int)TEACH_POS_MODE.TRAY_STAGE, i++] = "TRAY_UNLOADING";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_STAGE, i++] = "TRAY_INSPECTION_START";
            #endregion

            #region MAP_VISION
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.MAP_VISION, i++] = "MAP_VISION_READY";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_VISION, i++] = "MAP_VISION_T1";
            strRcpPosName[(int)TEACH_POS_MODE.MAP_VISION, i++] = "MAP_VISION_T2";
            #endregion

            #region TRAY_PICKER
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "TRAY_ELV_EMPTY_1";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "TRAY_ELV_EMPTY_2";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "STAGE_GOOD_1";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "STAGE_GOOD_2";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "STAGE_REWORK";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "TRAY_ELV_GOOD_1"; 
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "TRAY_ELV_GOOD_2";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "TRAY_ELV_REWORK";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "TRAY_PICKER_TRAY_GOOD_1_INSPECTION";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "TRAY_PICKER_TRAY_GOOD_2_INSPECTION";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_PICKER, i++] = "TRAY_PICKER_TRAY_REWORK_INSPECTION";
            #endregion
            
            #region TRAY_ELEV
            i = 0;
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_ELEV, i++] = "READY";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_ELEV, i++] = "TRAY_LOADING";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_ELEV, i++] = "TRAY_LIFT_IN";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_ELEV, i++] = "TRAY_LIFT_OUT";
            strRcpPosName[(int)TEACH_POS_MODE.TRAY_ELEV, i++] = "TRAY_UNLOADING";
            #endregion
            #endregion

            #region SET CONFIG POSITION NAME
            strCfgPosName[(int)CFG_POS_MODE.PICKER_HEAD_OFFSET, 0] = "CFG_PICKER_HEAD_1_PITCH_OFFSET_READY";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_HEAD_OFFSET, 1] = "CFG_PICKER_HEAD_2_PITCH_OFFSET_READY";

            strCfgPosName[(int)CFG_POS_MODE.PICKER_HEAD_OFFSET, 2] = "CFG_PICKER_HEAD_1_VISION_OFFSET_READY";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_HEAD_OFFSET, 3] = "CFG_PICKER_HEAD_2_VISION_OFFSET_READY";

            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 10] = "CFG_MAP_TABLE_1_OFFSET_START_P1";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 11] = "CFG_MAP_TABLE_1_OFFSET_LAST_X_P1";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 12] = "CFG_MAP_TABLE_1_OFFSET_LAST_Y_P1";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 13] = "CFG_MAP_TABLE_2_OFFSET_START_P1";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 14] = "CFG_MAP_TABLE_2_OFFSET_LAST_X_P1";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 15] = "CFG_MAP_TABLE_2_OFFSET_LAST_Y_P1";

            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 20] = "CFG_MAP_TABLE_1_OFFSET_START_P2";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 21] = "CFG_MAP_TABLE_1_OFFSET_LAST_X_P2";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 22] = "CFG_MAP_TABLE_1_OFFSET_LAST_Y_P2";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 23] = "CFG_MAP_TABLE_2_OFFSET_START_P2";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 24] = "CFG_MAP_TABLE_2_OFFSET_LAST_X_P2";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 25] = "CFG_MAP_TABLE_2_OFFSET_LAST_Y_P2";

            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 30] = "CFG_MAP_TABLE_1_OFFSET_START_MV";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 31] = "CFG_MAP_TABLE_1_OFFSET_LAST_X_MV";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 32] = "CFG_MAP_TABLE_1_OFFSET_LAST_Y_MV";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 33] = "CFG_MAP_TABLE_2_OFFSET_START_MV";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 34] = "CFG_MAP_TABLE_2_OFFSET_LAST_X_MV";
            strCfgPosName[(int)CFG_POS_MODE.VISION_TABLE_OSSET, 35] = "CFG_MAP_TABLE_2_OFFSET_LAST_Y_MV";

            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 40] = "CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 41] = "CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 42] = "CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 43] = "CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 44] = "CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P1";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 45] = "CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P1";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 46] = "CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 47] = "CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P1";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 48] = "CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P1";

            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 50] = "CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 51] = "CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 52] = "CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 53] = "CFG_ULD_TRAY_TABLE_2_OFFSET_START_P2";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 54] = "CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P2";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 55] = "CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P2";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 56] = "CFG_ULD_TRAY_TABLE_3_OFFSET_START_P2";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 57] = "CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P2";
            strCfgPosName[(int)CFG_POS_MODE.ULD_TRAY_TABLE_OFFSET, 58] = "CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P2";

            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 60] = "CFG_PICKER_HEAD_1_ZEROSET_POS_4_PICKUP";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 61] = "CFG_PICKER_HEAD_2_ZEROSET_POS_4_PICKUP";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 62] = "CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 63] = "CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE";

            strCfgPosName[(int)CFG_POS_MODE.TOP_CAM_SCALE, 64] = "CFG_TOP_CAM_SCALE_MAP_TABLE_1_POS";
            strCfgPosName[(int)CFG_POS_MODE.TOP_CAM_SCALE, 65] = "CFG_TOP_CAM_SCALE_MAP_TABLE_2_POS";

            // 20230201 CHOH : 제로셋 할 때 X축 시작위치 (테이블 2번 및 리워크테이블)
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 66] = "CFG_PICKER_HEAD_1_ZEROSET_POS_4_PICKUP_T2";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 67] = "CFG_PICKER_HEAD_2_ZEROSET_POS_4_PICKUP_T2";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 68] = "CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE_T2";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 69] = "CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE_T2";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 70] = "CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE_RW";
            strCfgPosName[(int)CFG_POS_MODE.PICKER_ZEROSET, 71] = "CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE_RW";
            #endregion

        }

        #region GetPosName
        public static string GetPosName(TEACH_POS_MODE mode, int nPosNo)
        {
            if (nPosNo < 0 || nPosNo >= MAX_POS_COUNT)
            {
                System.Diagnostics.Debugger.Break();
                return null;
            }

            return strRcpPosName[(int)mode, nPosNo];
        }

        public static string GetPosName(CFG_POS_MODE mode, int nPosNo)
        {
            if (nPosNo < 0 || nPosNo >= MAX_POS_COUNT)
            {
                System.Diagnostics.Debugger.Break();
                return null;
            }

            return strCfgPosName[(int)mode, nPosNo];
        }

        public static string GetPosName(FUNC_POS_MODE mode, int nPosNo)
        {
            if (nPosNo < 0 || nPosNo >= MAX_POS_COUNT)
            {
                System.Diagnostics.Debugger.Break();
                return null;
            }

            return strFncPosName[(int)mode, nPosNo];
        }
        #endregion

        public static TEACH_POS_MODE GetTeachPosModeAxis(int nAxis)
        {
            return GetTeachPosModeAxis((SVDF.AXES)nAxis);
        }

        public static TEACH_POS_MODE GetTeachPosModeAxis(SVDF.AXES axis)
        {
            POSDF.TEACH_POS_MODE mode = TEACH_POS_MODE.NONE;
            switch (axis)
            {
                case SVDF.AXES.NONE:
                case SVDF.AXES.MAX:
                    break;
                case SVDF.AXES.MAGAZINE_ELV_Z:
                    mode = TEACH_POS_MODE.MGZ_LD_ELEV;
                    break;
                case SVDF.AXES.LD_RAIL_T:
                    mode = TEACH_POS_MODE.LD_RAIL_T;
                    break;
                case SVDF.AXES.LD_RAIL_Y_FRONT:
                case SVDF.AXES.LD_RAIL_Y_REAR:
                    mode = TEACH_POS_MODE.LD_RAIL_Y;
                    break;
                case SVDF.AXES.BARCODE_Y:
                    mode = TEACH_POS_MODE.BARCODE_Y;
                    break;
                case SVDF.AXES.LD_VISION_X:
                    mode = TEACH_POS_MODE.LD_VISION_Y;
                    break;
                case SVDF.AXES.STRIP_PK_X:
                case SVDF.AXES.STRIP_PK_Z:
                    mode = TEACH_POS_MODE.STRIP_PICKER;
                    break;
                case SVDF.AXES.UNIT_PK_X:
                case SVDF.AXES.UNIT_PK_Z:
                    mode = TEACH_POS_MODE.UNIT_PICKER;
                    break;
                case SVDF.AXES.DRY_BLOCK_STG_X:
                    mode = TEACH_POS_MODE.DRY_BLOCK_STAGE;
                    break;
                case SVDF.AXES.MAP_PK_X:
                case SVDF.AXES.MAP_PK_Z:
                    mode = TEACH_POS_MODE.MAP_PICKER;
                    break;
                case SVDF.AXES.MAP_STG_1_Y:
                case SVDF.AXES.MAP_STG_2_Y:
                case SVDF.AXES.MAP_STG_1_T:
                case SVDF.AXES.MAP_STG_2_T:
                    mode = TEACH_POS_MODE.MAP_STAGE;
                    break;
                case SVDF.AXES.CHIP_PK_1_X:
                case SVDF.AXES.CHIP_PK_1_Z_1:
                case SVDF.AXES.CHIP_PK_1_Z_2:
                case SVDF.AXES.CHIP_PK_1_Z_3:
                case SVDF.AXES.CHIP_PK_1_Z_4:
                case SVDF.AXES.CHIP_PK_1_Z_5:
                case SVDF.AXES.CHIP_PK_1_Z_6:
                case SVDF.AXES.CHIP_PK_1_Z_7:
                case SVDF.AXES.CHIP_PK_1_Z_8:
                case SVDF.AXES.CHIP_PK_1_T_1:
                case SVDF.AXES.CHIP_PK_1_T_2:
                case SVDF.AXES.CHIP_PK_1_T_3:
                case SVDF.AXES.CHIP_PK_1_T_4:
                case SVDF.AXES.CHIP_PK_1_T_5:
                case SVDF.AXES.CHIP_PK_1_T_6:
                case SVDF.AXES.CHIP_PK_1_T_7:
                case SVDF.AXES.CHIP_PK_1_T_8:
                case SVDF.AXES.CHIP_PK_2_X:
                case SVDF.AXES.CHIP_PK_2_Z_1:
                case SVDF.AXES.CHIP_PK_2_Z_2:
                case SVDF.AXES.CHIP_PK_2_Z_3:
                case SVDF.AXES.CHIP_PK_2_Z_4:
                case SVDF.AXES.CHIP_PK_2_Z_5:
                case SVDF.AXES.CHIP_PK_2_Z_6:
                case SVDF.AXES.CHIP_PK_2_Z_7:
                case SVDF.AXES.CHIP_PK_2_Z_8:
                case SVDF.AXES.CHIP_PK_2_T_1:
                case SVDF.AXES.CHIP_PK_2_T_2:
                case SVDF.AXES.CHIP_PK_2_T_3:
                case SVDF.AXES.CHIP_PK_2_T_4:
                case SVDF.AXES.CHIP_PK_2_T_5:
                case SVDF.AXES.CHIP_PK_2_T_6:
                case SVDF.AXES.CHIP_PK_2_T_7:
                case SVDF.AXES.CHIP_PK_2_T_8:
                    mode = TEACH_POS_MODE.CHIP_PICKER;
                    break;
                case SVDF.AXES.BALL_VISION_Y:
                case SVDF.AXES.BALL_VISION_Z:
                    mode = TEACH_POS_MODE.BALL_VISION;
                    break;
                case SVDF.AXES.TRAY_PK_X:
                case SVDF.AXES.TRAY_PK_Z:
                case SVDF.AXES.TRAY_PK_Y:
                    mode = TEACH_POS_MODE.TRAY_PICKER;
                    break;
                case SVDF.AXES.GD_TRAY_1_ELV_Z:
                case SVDF.AXES.GD_TRAY_2_ELV_Z:
                case SVDF.AXES.RW_TRAY_ELV_Z:
                case SVDF.AXES.EMTY_TRAY_1_ELV_Z:
                case SVDF.AXES.EMTY_TRAY_2_ELV_Z:
                //case SVDF.AXES.COVER_TRAY_ELV_Z:
                    mode = TEACH_POS_MODE.TRAY_ELEV;
                    break;
                case SVDF.AXES.GD_TRAY_STG_1_Y:
                case SVDF.AXES.GD_TRAY_STG_2_Y:
                case SVDF.AXES.RW_TRAY_STG_Y:
                    mode = TEACH_POS_MODE.TRAY_STAGE;
                    break;
                case SVDF.AXES.MAP_VISION_Z:
                    mode = TEACH_POS_MODE.MAP_VISION;
                    break;
                default:
                    break;
            }

            return mode;
        }
    }
    #endregion

    #region SERVO
    public static class SVDF
    {
        public const int SV_CNT = (int)AXES.MAX;
        public const int POS_CNT = 100;
        public static MOTINFO[] mInfo = new MOTINFO[SV_CNT];
        public static string[] axisNames;

        static SVDF()
        {
            for (int i = 0; i < SV_CNT; i++)
            {
                mInfo[i].nAxis = i;
                mInfo[i].lMoveTime = 10000;
                mInfo[i].nAmpErrNo = (int)ERDF.E_SV_AMP + i;
                mInfo[i].nMoveErrNo = (int)ERDF.E_SV_MOVE + i;
                mInfo[i].nHomeErrNo = (int)ERDF.E_SV_NOT_HOME + i;
                mInfo[i].isUseInPos = true;
                mInfo[i].dPos = 0.0;
                mInfo[i].dVel = 10;
                mInfo[i].dAcc = 10;
                mInfo[i].dDec = mInfo[i].dAcc;
                mInfo[i].dInposGap = 0.1;
            }

            #region AXIS NAMES
            axisNames = new string[] 
            {
                #region SORTER
                "VISION_TRIGGER_BALL_1",
                "VISION_TRIGGER_BALL_2",
                "VISION_TRIGGER_MAP_1",
                "VISION_TRIGGER_MAP_2",
                "DRY_BLOCK_STG_X",

                "MAP_PK_X",
                "MAP_PK_Z",
                "MAP_STG_1_Y",
                "MAP_STG_2_Y",
                "CHIP_PK_1_X",
                "CHIP_PK_2_X",
                "BALL_VISION_Y",
                "BALL_VISION_Z",

                "TRAY_PK_X",
                "TRAY_PK_Z",
                "TRAY_PK_Y",

                "GD_TRAY_1_ELV_Z",
                "GD_TRAY_2_ELV_Z",
                "RW_TRAY_ELV_Z",
                "EMTY_TRAY_1_ELV_Z",
                "EMTY_TRAY_2_ELV_Z",
                "GD_TRAY_STG_1_Y",
                "GD_TRAY_STG_2_Y",
                "RW_TRAY_STG_Y",
                "MAP_STG_1_T",
                "MAP_STG_2_T",
				"MAP_VISION_Z",
                #endregion

                #region HEAD
                "CHIP_PK_1_Z_1",
                "CHIP_PK_1_T_1",
                "CHIP_PK_1_Z_2",
                "CHIP_PK_1_T_2",
                "CHIP_PK_1_Z_3",
                "CHIP_PK_1_T_3",
                "CHIP_PK_1_Z_4",
                "CHIP_PK_1_T_4",
                "CHIP_PK_1_Z_5",
                "CHIP_PK_1_T_5",
                "CHIP_PK_1_Z_6",
                "CHIP_PK_1_T_6",
                "CHIP_PK_1_Z_7",
                "CHIP_PK_1_T_7",
                "CHIP_PK_1_Z_8",
                "CHIP_PK_1_T_8",

                "CHIP_PK_2_Z_1",
                "CHIP_PK_2_T_1",
                "CHIP_PK_2_Z_2",
                "CHIP_PK_2_T_2",
                "CHIP_PK_2_Z_3",
                "CHIP_PK_2_T_3",
                "CHIP_PK_2_Z_4",
                "CHIP_PK_2_T_4",
                "CHIP_PK_2_Z_5",
                "CHIP_PK_2_T_5",
                "CHIP_PK_2_Z_6",
                "CHIP_PK_2_T_6",
                "CHIP_PK_2_Z_7",
                "CHIP_PK_2_T_7",
                "CHIP_PK_2_Z_8",
                "CHIP_PK_2_T_8",

                #endregion

                #region LOADER
                "LD_RAIL_T",
                "LD_RAIL_Y_FRONT",
                "LD_RAIL_Y_REAR",
                "BARCODE_Y",
                "LD_VISION_X",
                "STRIP_PK_X",
                "STRIP_PK_Z",
                "UNIT_PK_X",
                "UNIT_PK_Z",
                "MAGAZINE_ELV_Z",
                #endregion
            };
            #endregion
        }

        public static void Init()
        {
            try
            {
                for (int i = 0; i < SV_CNT; i++)
                {
                    mInfo[i].nAxis = i;
                    mInfo[i].lMoveTime = ConfigMgr.Inst.Cfg.MotData[i].lMoveTime;
                    mInfo[i].nAmpErrNo = (int)ERDF.E_SV_AMP + i;
                    mInfo[i].nMoveErrNo = (int)ERDF.E_SV_MOVE + i;
                    mInfo[i].nHomeErrNo = (int)ERDF.E_SV_NOT_HOME + i;
                    mInfo[i].isUseInPos = true;
                    mInfo[i].dPos = 0.0;
                    //mInfo[i].dVel = ConfigMgr.Inst.Cfg.MotData[i].dVel > 1 ? ConfigMgr.Inst.Cfg.MotData[i].dVel : 10.0;
                    //mInfo[i].dAcc = ConfigMgr.Inst.Cfg.MotData[i].dAcc > 10 ? ConfigMgr.Inst.Cfg.MotData[i].dAcc : 10.0;
                    //mInfo[i].dDec = ConfigMgr.Inst.Cfg.MotData[i].dAcc > 10 ? ConfigMgr.Inst.Cfg.MotData[i].dAcc : 10.0;
                    mInfo[i].dInposGap = 0.1;
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
            }
        }

        private static void SetMoveInfo(int nAxis, double dPos, double dVel, double dAcc, double dDec)
        {
            if (nAxis < 0) return;

            mInfo[nAxis].dPos = dPos;
            mInfo[nAxis].dVel = dVel;
            mInfo[nAxis].dAcc = dAcc;
            mInfo[nAxis].dDec = dDec;
        }

        public static void SetMotionInfo(int nAxis, double dPos)
        {
            if (nAxis < 0) return;
            double dRunVel = ConfigMgr.Inst.Cfg.MotData[nAxis].dVel;
            double dRunAcc = ConfigMgr.Inst.Cfg.MotData[nAxis].dAcc;

            if (dRunVel <= 0) dRunVel = 1;
            if (dRunVel > 500) dRunVel = 10;
            if (dRunAcc <= 20) dRunAcc = 20;
            if (dRunAcc > 5000) dRunAcc = 1000;

            SVDF.SetMoveInfo(nAxis, dPos, dRunVel, dRunAcc, dRunAcc);
        }

        public static void SetMotionInfo(int nAxis, double dPos, double dVel)
        {
            double dRunVel = dVel;
            double dRunAcc = ConfigMgr.Inst.Cfg.MotData[nAxis].dAcc;

            if (dRunVel <= 0) dRunVel = 1;
            if (dRunVel > 2000) dRunVel = 10;
            if (dRunAcc <= 20) dRunAcc = 20;
            if (dRunAcc > 1000) dRunAcc = 1000;

            SVDF.SetMoveInfo(nAxis, dPos, dRunVel, dRunAcc, dRunAcc);
        }

        #region AXES LIST

        public enum AXES
        {
            NONE = -1,

            #region SORTER
            VISION_TRIGGER_BALL,                      //0
            VISION_TRIGGER_BALL_2,            //1
            VISION_TRIGGER_MAP,             //2
            VISION_TRIGGER_MAP_2,

            DRY_BLOCK_STG_X = 4,            //4
            MAP_PK_X,                       //5
            MAP_PK_Z,                       //6
            MAP_STG_1_Y,                    //7
            MAP_STG_2_Y,                    //8
            CHIP_PK_1_X,                    //9
            CHIP_PK_2_X,                    //10
            BALL_VISION_Y,                  //11
            BALL_VISION_Z,                  //12
            TRAY_PK_X,                      //13
            TRAY_PK_Z,                      //14
            TRAY_PK_Y,                      //15
            GD_TRAY_1_ELV_Z,                //16
            GD_TRAY_2_ELV_Z,                //17
            RW_TRAY_ELV_Z,                  //18
            EMTY_TRAY_1_ELV_Z,              //19
            EMTY_TRAY_2_ELV_Z,              //20
            GD_TRAY_STG_1_Y,                //21
            GD_TRAY_STG_2_Y,                //22
            RW_TRAY_STG_Y,                  //23
            MAP_STG_1_T,                    //24
            MAP_STG_2_T,                    //25
            MAP_VISION_Z,                   //26
            #endregion

            #region HEAD
            CHIP_PK_1_Z_1,                  //27
            CHIP_PK_1_T_1,                  //28
            CHIP_PK_1_Z_2,                  //29
            CHIP_PK_1_T_2,                  //30
            CHIP_PK_1_Z_3,                  //31
            CHIP_PK_1_T_3,                  //32
            CHIP_PK_1_Z_4,                  //33
            CHIP_PK_1_T_4,                  //34
            CHIP_PK_1_Z_5,                  //35
            CHIP_PK_1_T_5,                  //36
            CHIP_PK_1_Z_6,                  //37
            CHIP_PK_1_T_6,                  //38
            CHIP_PK_1_Z_7,                  //39
            CHIP_PK_1_T_7,                  //40
            CHIP_PK_1_Z_8,                  //41
            CHIP_PK_1_T_8,                  //42

            CHIP_PK_2_Z_1,                  //43
            CHIP_PK_2_T_1,                  //44
            CHIP_PK_2_Z_2,                  //45
            CHIP_PK_2_T_2,                  //46
            CHIP_PK_2_Z_3,                  //47
            CHIP_PK_2_T_3,                  //48
            CHIP_PK_2_Z_4,                  //49
            CHIP_PK_2_T_4,                  //50
            CHIP_PK_2_Z_5,                  //51
            CHIP_PK_2_T_5,                  //52
            CHIP_PK_2_Z_6,                  //53
            CHIP_PK_2_T_6,                  //54
            CHIP_PK_2_Z_7,                  //55
            CHIP_PK_2_T_7,                  //56
            CHIP_PK_2_Z_8,                  //57
            CHIP_PK_2_T_8,                  //58
            #endregion

            #region LOADER
            LD_RAIL_T,                      //59
            LD_RAIL_Y_FRONT,                //60
            LD_RAIL_Y_REAR,                 //61
            BARCODE_Y,                      //62
            LD_VISION_X,                    //63
            STRIP_PK_X,                     //64
            STRIP_PK_Z,                     //65
            UNIT_PK_X,                      //66
            UNIT_PK_Z,                      //67
            MAGAZINE_ELV_Z,                 //68
            #endregion
            MAX,
        }
        #endregion

        public static int Get(AXES axis)
        {
            return (int)axis;
        }

        public static string GetAxisName(AXES axis)
        {
            int nAxisNo = (int)axis;
            if (nAxisNo < 0 || nAxisNo >= axisNames.Length) return axis.ToString();
            return axisNames[nAxisNo];
        }

        public static string GetAxisName(int nAxisNo)
        {
            if (nAxisNo < 0 || nAxisNo >= axisNames.Length) return nAxisNo.ToString();
            return axisNames[nAxisNo];
        }

        public static AXES GetAxisByName(string strAxisName)
        {
            AXES axs = AXES.NONE;

            if (Enum.TryParse<AXES>(strAxisName, out axs) == false)
                axs = AXES.NONE;

            return axs;
        }
    } 
    #endregion

    public enum MANUAL_POS_GROUP
    {
        NONE = 0,
        RCP_LD_PICKER_MOVE = 10,
        RCP_CUT_TABLE_MOVE = 20,
        RCP_CUT_ALIGN_LEFT = 30,
        RCP_CUT_ALIGN_RIGHT = 40,
        RCP_TRSF_MOVE = 50,
        RCP_BREAKING_MOVE = 60,
    }

    #region VAC STATUS
    public class VACDF
    {
        public enum VAC
        { 
            LOADING_PICKER_VAC_1 = 0,
            LOADING_PICKER_VAC_2 = 1,
            WORKING_STAGE_1_VAC_1 = 2,
            WORKING_STAGE_1_VAC_2 = 3,
            LOADING_TRANSFER_VAC_1 = 4,
            LOADING_TRANSFER_VAC_2 = 5,
            LD_BUFFER_RAIL_VAC_1 = 6,
            LD_BUFFER_RAIL_VAC_2 = 7,
            WORKING_STAGE_2_VAC_1 = 8,
            WORKING_STAGE_2_VAC_2 = 9,
            UNLOADING_TRANSFER_VAC_1 = 10,
            UNLOADING_TRANSFER_VAC_2 = 11,
            ULD_BUFFER_RAIL_VAC_1 = 12,
            ULD_BUFFER_RAIL_VAC_2 = 13,
            VISION_STAGE_VAC_1 = 14,
            VISION_STAGE_VAC_2 = 15,
            UNLOADING_PICKER_VAC_1 = 16,
            UNLOADING_PICKER_VAC_2 = 17,
        }
    }
    #endregion

    //[StructLayout(LayoutKind.Explicit)]
    //public struct UNION_MT_DATA
    //{
    //    [FieldOffset(0)]
    //    public DWORD nData;

    //    [FieldOffset(0)]
    //    public WORD wData1;

    //    [FieldOffset(2)]
    //    public WORD wData2;
    //}

    #region IO
    public class IODF
    {
        public const int IO_SLOT_CNT = 32;

        public const int IN_TOTAL_CNT = (int)INPUT.MAX;
        public const int OUT_TOTAL_CNT = (int)OUTPUT.MAX;
        public const int AIN_TOTAL_CNT = (int)A_INPUT.MAX;

        public enum A_INPUT
        {
            NONE = -1,
            #region Sorter
            #region AD01~AD16
            DRIVE_AIR = 0,
            BLOW_AIR,
            CHIP_PK_AIR,
            SPARE_04,
            MAP_PK_AIR,
            MAP_PK_WORK_VAC,
            MAP_PK_CHUCK_VAC,
            DRY_BLOCK_WORK_VAC,
            SPARE_09,
            MAP_STG_AIR,
            MAP_STG_1_WORK_VAC,
            SPARE_12,
            SPARE_13,
            SPARE_14,
            MAP_STG_2_WORK_VAC,
            SPARE_16,
            #endregion

            #region AD17~AD32
            SPARE_17,
            DRY_STG_AIR,
            SPARE_19,
            SPARE_20,
            SPARE_21,
            SPARE_22,
            SPARE_23,
            SPARE_24,
            SPARE_25,
            SPARE_26,
            SPARE_27,
            SPARE_28,
            SPARE_29,
            SPARE_30,
            SPARE_31,
            SPARE_32,
            #endregion
            #endregion

            #region Loader
            #region AD50~AD65
            PUMP_AIR_1,
            STRIP_PK_VAC,
            SPARE_52,
            UNIT_PK_VAC,
            SPARE_54,
            SPARE_55,
            SPARE_56,
            LD_RAIL_VAC,
            PUMP_AIR_2,
            SCRAP_VAC_1,
            SCRAP_VAC_2,
            SPARE_61,
            SPARE_62,
            SPARE_63,
            SPARE_64,
            SPARE_65,
            #endregion
            #endregion

            #region CHIP PK
            X1_AXIS_1_PICKER_VACUUM,
            X1_AXIS_2_PICKER_VACUUM,
            X1_AXIS_3_PICKER_VACUUM,
            X1_AXIS_4_PICKER_VACUUM,
            X1_AXIS_5_PICKER_VACUUM,
            X1_AXIS_6_PICKER_VACUUM,
            X1_AXIS_7_PICKER_VACUUM,
            X1_AXIS_8_PICKER_VACUUM,

            X2_AXIS_1_PICKER_VACUUM,
            X2_AXIS_2_PICKER_VACUUM,
            X2_AXIS_3_PICKER_VACUUM,
            X2_AXIS_4_PICKER_VACUUM,
            X2_AXIS_5_PICKER_VACUUM,
            X2_AXIS_6_PICKER_VACUUM,
            X2_AXIS_7_PICKER_VACUUM,
            X2_AXIS_8_PICKER_VACUUM,
            #endregion
            MAX,
        }

        public enum A_OUTPUT
        {
            NONE = -1,

            MAX,
        }

        public enum INPUT
        {
            NONE = -1,
            //IO SHEET DI32P

            #region Sorter

            #region X000~X031                   
            TEN_KEY_1,                                  //IO SHEET -> SPARE  (용도?) 
            TEN_KEY_2,                                  //IO SHEET -> SPARE  (용도?) 
            TEN_KEY_3,                                  //IO SHEET -> SPARE  (용도?) 
            TEN_KEY_4,                                  //IO SHEET -> SPARE  (용도?) 
            TEN_KEY_5,                                  //IO SHEET -> SPARE  (용도?) 
            TEN_KEY_6,                                  //IO SHEET -> SPARE  (용도?) 
            TEN_KEY_7,                                  //IO SHEET -> SPARE  (용도?) 
            SPARE_07,
            SPARE_08,                                   //IO SHEET -> SPARE  (삭제?)
            EMO_SWITCH_1,
            EMO_SWITCH_2,                                   //IO SHEET -> SPARE  (삭제?)
            EMO_SWITCH_3,                               //IO SHEET -> EMO_SWITCH_3 (REAR)
            SPARE_012,
            SPARE_013,
            START_SWITCH,
            STOP_SWITCH,
            RESET_SWITCH,
            FORNT_MONITOR_DOOR,
            FORNT_MONITOR_RT_SID_DOOR,
            FORNT_TRAY_TOP_LF_DOOR,
            FORNT_TRAY_TOP_RT_DOOR,
            RT_SIDE_LF_DOOR,
            RT_SIDE_CENTER_DOOR,
            RT_SIDE_RT_DOOR,
            REAR_LF_DOOR,
            REAR_CENTER_DOOR,
            REAR_RT_DOOR,
            SPARE_027,
            SPARE_028,
            SPARE_029,
            SPARE_030,
            SPARE_031,
            #endregion

            #region X100~X131
            SPARE_100,
            SPARE_101,
            SPARE_102,
            SPARE_103,
            SPARE_104,
            SPARE_105,                                  //IO SHEET -> SPARE  (삭제?)
            SPARE_106,                                  //IO SHEET -> SPARE  (삭제?)
            SPARE_107,                                  //IO SHEET -> SPARE  (삭제?)
            SPARE_108,                                  //IO SHEET -> SPARE  (삭제?)
            ION_BAR_RUN_STOP_STATE_1,
            ION_BAR_HV_ABNOMAL_ALARM_1,
            SPARE_111,
            SPARE_112,
            SPARE_113,
            SPARE_114,
            SPARE_115,
            SPARE_116,
            SPARE_117,
            SPARE_118,
            SPARE_119,
            SPARE_120,
            SPARE_121,
            SPARE_122,
            SPARE_123,
            SPARE_124,
            SPARE_125,
            SPARE_126,
            SPARE_127,
            SPARE_128,
            SPARE_129,
            BALL_VISION_ALIGN_FWD,
            BALL_VISION_ALIGN_BWD,
            #endregion

            #region X200~X231
            GD_TRAY_1_STG_UP,
            GD_TRAY_1_STG_DOWN,
            GD_TRAY_1_STG_ALIGN_FWD,
            GD_TRAY_1_STG_ALIGN_BWD,
            GD_TRAY_1_STG_GRIP_OPEN,
            GD_TRAY_1_STG_GRIP_CLOSE,
            GD_TRAY_1_CHECK,
            GD_TRAY_2_STG_UP,
            GD_TRAY_2_STG_DOWN,
            GD_TRAY_2_STG_ALIGN_FWD,
            GD_TRAY_2_STG_ALIGN_BWD,
            GD_TRAY_2_STG_GRIP_OPEN,
            GD_TRAY_2_STG_GRIP_CLOSE,
            GD_TRAY_2_CHECK,
            RW_TRAY_STG_ALIGN_FWD,
            RW_TRAY_STG_ALIGN_BWD,
            RW_TRAY_STG_GRIP_OPEN,
            RW_TRAY_STG_GRIP_CLOSE,
            RW_TRAY_CHECK,
            TRAY_PK_FRONT_CLAMP,                        //IO SHEET -> CLAMP  (변경)
            TRAY_PK_FRONT_UNCLAMP,                      //IO SHEET -> UNCLAMP  (변경)
            TRAY_PK_REAR_CLAMP,                         //IO SHEET -> CLAMP  (변경)
            TRAY_PK_REAR_UNCLAMP,                       //IO SHEET -> UNCLAMP  (변경)
            SPARE_223,
            TRAY_PK_REAR_GRIP,
            TRAY_PK_FRONT_TRAY_CHECK,
            TRAY_PK_REAR_TRAY_CHECK,
            GD_1_TRAY_ELV_STACKER_LF_UP,
            GD_1_TRAY_ELV_STACKER_LF_DOWN,
            GD_1_TRAY_ELV_STACKER_RT_UP,
            GD_1_TRAY_ELV_STACKER_RT_DOWN,
            GD_1_TRAY_ELV_GUIDE_FWD,
            #endregion

            #region X300~X331
            GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK,
            GD_1_TRAY_ELV_POS_CHECK,
            GD_1_TRAY_ELV_CHECK,
            GD_2_TRAY_ELV_STACKER_LF_UP,
            GD_2_TRAY_ELV_STACKER_LF_DOWN,
            GD_2_TRAY_ELV_STACKER_RT_UP,
            GD_2_TRAY_ELV_STACKER_RT_DOWN,
            GD_2_TRAY_ELV_GUIDE_FWD,
            GD_2_TRAY_ELV_RESIDUAL_QTY_CHECK,
            GD_2_TRAY_ELV_POS_CHECK,
            GD_2_TRAY_ELV_CHECK,
            RW_TRAY_ELV_STACKER_LF_UP,
            RW_TRAY_ELV_STACKER_LF_DOWN,
            RW_TRAY_ELV_STACKER_RT_UP,
            RW_TRAY_ELV_STACKER_RT_DOWN,
            RW_TRAY_ELV_GUIDE_FWD,
            RW_TRAY_ELV_RESIDUAL_QTY_CHECK,
            RW_TRAY_ELV_POS_CHECK,
            RW_TRAY_ELV_CHECK,
            EMTY_1_TRAY_ELV_STACKER_LF_UP,
            EMTY_1_TRAY_ELV_STACKER_LF_DOWN,
            EMTY_1_TRAY_ELV_STACKER_RT_UP,
            EMTY_1_TRAY_ELV_STACKER_RT_DOWN,
            EMTY_1_TRAY_ELV_GUIDE_FWD,
            EMTY_1_TRAY_ELV_RESIDUAL_QTY_CHECK,
            EMTY_1_TRAY_ELV_POS_CHECK,
            EMTY_1_TRAY_ELV_CHECK,
            EMTY_2_TRAY_ELV_STACKER_LF_UP,
            EMTY_2_TRAY_ELV_STACKER_LF_DOWN,
            EMTY_2_TRAY_ELV_STACKER_RT_UP,
            EMTY_2_TRAY_ELV_STACKER_RT_DOWN,
            EMTY_2_TRAY_ELV_GUIDE_FWD,
            #endregion

            #region X400~X431
            EMTY_2_TRAY_ELV_RESIDUAL_QTY_CHECK,
            EMTY_2_TRAY_ELV_POS_CHECK,
            EMTY_2_TRAY_ELV_CHECK,
            GD1_CONV_TRAY_CHECK_IN,
            GD1_CONV_TRAY_CHECK_OUT,
            SPARE_405,
            GD2_CONV_TRAY_CHECK_IN,
            GD2_CONV_TRAY_CHECK_OUT,
            SPARE_408,
            RW_CONV_TRAY_CHECK_IN,
            RW_CONV_TRAY_CHECK_OUT,
            SPARE_411,
            EMPTY1_CONV_TRAY_CHECK_IN,
            EMPTY1_CONV_TRAY_CHECK_OUT,
            SPARE_414,
            EMPTY2_CONV_TRAY_CHECK_IN,
            EMPTY2_CONV_TRAY_CHECK_OUT,
            SPARE_417,
            MAP_PK_N_TRAY_PK_COLLISION_CHECK,
            REJECT_BOX_CHECK,
            REJECT_BOX_FULL1,
            REJECT_BOX_FULL2,
            SPARE_422,                              //IO SHEET -> SPARE  (삭제?)
            SPARE_423,                              //IO SHEET -> SPARE  (삭제?)
            SPARE_424,                              //IO SHEET -> SPARE  (삭제?)
            SPARE_425,                              //IO SHEET -> SPARE  (삭제?)
            SPARE_426,                              //IO SHEET -> SPARE  (삭제?)
            SPARE_427,
            SUB_INTERFACE_1,                        //IO SHEET -> SUB INTERFACE #1  (변경)
            SUB_INTERFACE_2,                        //IO SHEET -> SUB INTERFACE #2  (변경)
            SUB_INTERFACE_3,                        //IO SHEET -> SUB INTERFACE #3  (변경)
            SUB_INTERFACE_4,                        //IO SHEET -> SUB INTERFACE #4  (변경)
            #endregion

            #region X500~X531 
            // Vision Communication Sig
            VISION_READY,                           //VISION COMM #1
            PRE_INSP_COMP,                          //VISION COMM #2
            PRE_INSP_GRAB_REQ,                      //VISION COMM #3
            TOP_ALIGN_CNT_RESET,                    //VISION COMM #4
            TOP_ALIGN_COMP,                         //VISION COMM #5
            MAP_CNT_RESET,                          //VISION COMM #6
            MAP_INSP_COMP,                          //VISION COMM #7
            SPARE_507,                              //VISION COMM #8
            BALL_CNT_RESET,                         //VISION COMM #9
            BALL_INSP_COMP,                         //VISION COMM #10
            PICKER_CAL_RESET,                       //VISION COMM #11
            PICKER_CAL_COMP,                        //VISION COMM #12
            SPARE_512,                              //VISION COMM #13
            SPARE_513,                              //VISION COMM #14
            SPARE_514,                              //VISION COMM #15
            SPARE_515,                              //VISION COMM #16
            SPARE_516,                              //VISION COMM #17
            SPARE_517,                              //VISION COMM #18
            SPARE_518,                              //VISION COMM #19
            SPARE_519,                              //VISION COMM #20
            SPARE_520,                              //VISION COMM #21
            SPARE_521,                              //VISION COMM #22
            SPARE_522,                              //VISION COMM #23
            SPARE_523,                              //VISION COMM #24
            SPARE_524,                              //VISION COMM #25
            SPARE_525,                              //VISION COMM #26
            SPARE_526,                              //VISION COMM #27
            SPARE_527,                              //VISION COMM #28
            SPARE_528,                              //VISION COMM #29
            SPARE_529,                              //VISION COMM #30
            SPARE_530,                              //VISION COMM #31
            SPARE_531,                              //VISION COMM #32
            #endregion

            #endregion

            #region Loader

            #region X2000~X2031
            EMO_FRONT_SWITCH_SAW,
            SPARE_2001,
            EMO_REAR_SWITCH_SAW,
            FRONT_RT_DOOR_SAW,
            FRONT_LF_DOOR_SAW,
            FRONT_MONITOR_DOOR_SAW,
            DICING_SERVO_1_MC_TRIP,
            DICING_SERVO_2_MC_TRIP,
            LD_RAIL_Y_FRONT_MATERIAL_CHECK,
            LD_RAIL_Y_REAR_MATERIAL_CHECK,
            IN_LET_TABLE_UP,
            IN_LET_TABLE_DOWN,
            LD_X_GRIP_UP,                           //IO SHEET -> GRIP UP  (변경)
            LD_X_GRIP_DOWN,                         //IO SHEET -> GRIP DOWN  (변경)
            LD_X_UNGRIP,
            LD_X_GRIP,
            GRIP_MATERIAL_CHECK,
            STRIP_PK_N_UNIT_PK_COLLISION_CHECK,
            SCRAP_BOX_CHECK,
            SCRAP_BOX_FULL_CHECK,
            INLET_TABLE_MATERIAL_IN_STOPPER_UP,     //IO SHEET -> INLET TABLE MATERIAL STOPPER UP  (변경)
            INLET_TABLE_MATERIAL_IN_STOPPER_DOWN,   //IO SHEET -> INLET TABLE MATERIAL STOPPER DOWN  (변경)
            INLET_TABLE_MATERIAL_CHECK,             //IO SHEET -> INLET TABLE MATERIAL CHECK  (변경)
            FRONT_LF_LF_DOOR_SAW,
            SPARE_2024,                             //IO SHEET -> SPARE  (삭제?)
            CLEANER_SWING_FWD,
            CLEANER_SWING_BWD,
            SPARE_2027,                             //IO SHEET -> SPARE  (삭제?)
            SPARE_2028,                             //IO SHEET -> SPARE  (삭제?)
            SPARE_2029,                             //IO SHEET -> SPARE  (삭제?)
            SPARE_2030,                             //IO SHEET -> SPARE  (삭제?)
            SPARE_2031,
            #endregion

            #region X2100~X2131
            //Dicing Communication
            SAW_PROGRAM_ON,                         //DICING COMM #1
            SAW_INITIALIZING,                       //DICING COMM #2
            SAW_CUTTING_R,                          //DICING COMM #3
            SAW_LD_REQUISITION_R,                   //DICING COMM #4
            SAW_LD_POS_R,                           //DICING COMM #5
            SAW_STAGE_VAC_ON_R,                     //DICING COMM #6
            SAW_ALIGN_COMP_R,                       //DICING COMM #7
            SAW_REPICK_ALIGN_R,                     //DICING COMM #8
            SAW_CUTTING,                            //DICING COMM #9
            SAW_LD_REQUISITION_L,                   //DICING COMM #10
            SAW_LD_POS,                             //DICING COMM #11
            SAW_STAGE_VAC_ON,                       //DICING COMM #12
            SAW_ALIGN_COMP,                         //DICING COMM #13
            SAW_REPICK_ALIGN_L,                     //DICING COMM #14
            SAW_ULD_REQUISITION_R,                  //DICING COMM #15
            SAW_STG_BLOW_ON_R,                      //DICING COMM #16
            SAW_ULD_POS_R,                          //DICING COMM #17
            SAW_ULD_REQUISITION,                    //DICING COMM #18
            SAW_STG_BLOW_ON,                        //DICING COMM #19
            SAW_ULD_POS,                            //DICING COMM #20
            SPARE_2120,                             //DICING COMM #21
            SPARE_2121,                             //DICING COMM #22
            SPARE_2122,                             //DICING COMM #23
            SPARE_2123,                             //DICING COMM #24
            SPARE_2124,                             //DICING COMM #25
            SPARE_2125,                             //DICING COMM #26
            SAW_HBC_MDL_SAFETY_POS,                 //DICING COMM #27
            SAW_STG_MOVE_INTERLOCK,                 //DICING COMM #28
            SAW_RUN,                                //DICING COMM #29
            SAW_TRIP,                               //DICING COMM #30
            SAW_ERROR,                              //DICING COMM #31
            SAW_MAINAIR,                            //DICING COMM #32
            #endregion

            #region X2200~X2231
            //Dicing Communication
            SPARE_2200, 
            SPARE_2201, 
            SPARE_2202, 
            SPARE_2203, 
            SPARE_2204, 
            SPARE_2205, 
            SPARE_2206, 
            SPARE_2207, 
            SPARE_2208, 
            SPARE_2209, 
            SPARE_2210, 
            SPARE_2211, 
            SPARE_2212, 
            SPARE_2213, 
            SPARE_2214, 
            SPARE_2215, 
            SPARE_2216, 
            SPARE_2217, 
            SPARE_2218, 
            SPARE_2219, 
            SPARE_2220, 
            SPARE_2221, 
            SPARE_2222, 
            SPARE_2223, 
            SPARE_2224, 
            SPARE_2225, 
            SPARE_2226, 
            TOP_INTERFACE_1,                         //TOP INTERFACE #1
            TOP_INTERFACE_2,                         //TOP INTERFACE #2 
            TOP_INTERFACE_3,                         //TOP INTERFACE #3 
            TOP_INTERFACE_4,                         //TOP INTERFACE #4 
            TOP_INTERFACE_5,                         //TOP INTERFACE #5 
            #endregion

            #region NOT IN IO SHEET

            #region X3001~X3032
            EMERGENCY_SWITCH_1_FRONT,
            EMERGENCY_SWITCH_2_LEFT_SIDE,
            EMERGENCY_SWITCH_3_REAR,
            _1F_CONVEYOR_INPUT_SWITCH,
            _1F_CONVEYOR_OUTPUT_SWITCH,
            _2F_CONVEYOR_INPUT_SWITCH,
            _2F_CONVEYOR_OUTPUT_SWITCH,
            LD_FRONT_RT_DOOR,
            LD_FRONT_LEFT_DOOR,
            CP20_TRIP,
            MC_30,
            MC_31,
            CP30_TRIP,
            CP31_TRIP,
            CP32_TRIP, //#1 Loading Conveyor M/Z Check Sensor
            LD_SAFETY_SENSOR_LT_SIDE, // 매거진 도착 센서 //X3015 // B접에서 A접으로 변경 되었음 //2022.07.19 HEP
            LD_MAIN_AIR_SENSOR,
            _1F_MZ_INPUT_CHECK, // UnLoading Conveyor M/Z Full Check Sensor
            _1F_MZ_CHECK_1,
            _1F_MZ_CHECK_2,
            _1F_MZ_CHECK_3,
            _1F_MZ_STOPPER_UP_CHECK,
            _1F_MZ_STOPPER_DOWN_CHECK,
            _2F_MZ_CHECK_1,
            _2F_MZ_CHECK_2,
            _2F_MZ_CHECK_3,
            _2F_MZ_STOPPER_UP_CHECK,
            _2F_MZ_STOPPER_DOWN_CHECK,
            PUSHER_FORWARD_SENSOR,
            PUSHER_BACKWARD_SENSOR,
            PUSHER_OVERLOAD_SENSOR,
            ME_AXIS_MZ_CHECK_SENSOR,
            #endregion

            #region X3101~X3132
            ME_AXIS_MZ_CHECK_SENSOR_2,
            ME_AXIS_MZ_CLAMP_CHECK_SENSOR,
            ME_AXIS_MZ_UNCLAMP_CHECK_SENSOR,
            ME_AXIS_MZ_DOOR_CHECK_SENSOR,
            ME_AXIS_MZ_DOOR_OPEN_SENSOR,
            ME_AXIS_MZ_DOOR_CLOSE_SENSOR,
            _1F_INPUT_STOPPER_UP,
            _1F_INPUT_STOPPER_DOWN,
            MATERIAL_PROTRUSION_CHECK_SENSOR,
            SPARE_3109,
            SPARE_3110,
            SPARE_3111,
            SPARE_3112,            //X3211 //2022.07.19 홍은표 B접에서 A접으로 변경
            SPARE_3113,            //X3212 //2022.07.19 홍은표 B접에서 A접으로 변경
            SPARE_3114,
            SPARE_3115,
            SPARE_3116,
            SPARE_3117,
            SPARE_3118,
            SPARE_3119,
            SPARE_3120,
            SPARE_3121,
            SPARE_3122,
            SPARE_3123,
            SPARE_3124,
            SPARE_3125,
            SPARE_3126,
            SPARE_3127,
            SPARE_3128,
            SPARE_3129,
            SPARE_3130,
            SPARE_3131,
            #endregion

            #region X3400~X3515
            DRIVER_AIR,
            SPARE_3401,
            SPARE_3402,
            SPARE_3403,
            SPARE_3404,
            SPARE_3405,
            SPARE_3406,
            SPARE_3407,
            SPARE_3408,
            SPARE_3409,
            SPARE_3410,
            SPARE_3411,
            SPARE_3412,
            SPARE_3413,
            SPARE_3414,
            SPARE_3415,
            SPARE_3316,
            SPARE_3317,
            SPARE_3318,
            SPARE_3319,
            SPARE_3320,
            SPARE_3321,
            SPARE_3322,
            SPARE_3323,
            SPARE_3324,
            SPARE_3325,
            SPARE_3326,
            SPARE_3327,
            SPARE_3328,
            SPARE_3329,
            SPARE_3330,
            SPARE_3331,
            #endregion

            #endregion NOT IN IO SHEET

            #endregion
            MAX,
        }

        public enum OUTPUT
        {
            NONE = -1,

            //IO SHEET DI32P
            #region Sorter

            #region Y000~Y031
            TEN_KEY_1,                              //IO SHEET -> SPARE  (삭제?)
            TEN_KEY_2,                              //IO SHEET -> SPARE  (삭제?)
            TEN_KEY_3,                              //IO SHEET -> SPARE  (삭제?)
            TEN_KEY_4,                              //IO SHEET -> SPARE  (삭제?)
            TEN_KEY_5,                              //IO SHEET -> SPARE  (삭제?)
            TEN_KEY_6,                              //IO SHEET -> SPARE  (삭제?)
            TEN_KEY_7,                              //IO SHEET -> SPARE  (삭제?)
            TEN_KEY_8,                              //IO SHEET -> SPARE  (삭제?)
            TEN_KEY_9,                              //IO SHEET -> SPARE  (삭제?)
            HPC_POWER_SWITCH_LED,
            VPC_POWER_SWITCH_LED,
            START_SWITCH_LED,
            STOP_SWITCH_LED,
            RESET_SWITCH_LED,
            DOOR_LOCK_SIGNAL,
            FLUORESCENT_LIGHT_ONOFF,
            TOWER_LAMP_RED_LED,
            TOWER_LAMP_YELLOW_LED,
            TOWER_LAMP_GREEN_LED,
            ERROR_BUZZER,
            END_BUZZER,
            ION_BLOWER_FAN_RUN_1,
            ION_BLOWER_FAN_TIP_CLEANING_1,
            ION_BLOWER_FAN_RUN_2,
            ION_BLOWER_FAN_TIP_CLEANING_2,
            ION_BAR_RUN_1,
            SPARE_026,                              //IO SHEET -> Y026 <-> Y027  (변경)
            SERVO_POWER_OFF,                        //IO SHEET -> Y027 <-> Y026  (변경)
            IONIZER_AIR,
            BTM_VISION_ALIGN_FWD,
            BTM_VISION_ALIGN_BWD,
            GD_TRAY_1_STG_UP,
            #endregion

            #region Y100~Y131
            GD_TRAY_1_STG_DOWN,
            GD_TRAY_2_STG_UP,
            GD_TRAY_2_STG_DOWN,
            GD_TRAY_1_STG_ALIGN_CLAMP,
            GD_TRAY_2_STG_ALIGN_CLAMP,
            RW_TRAY_STG_ALIGN_CLAMP,
            GD_TRAY_1_STG_GRIP,
            GD_TRAY_2_STG_GRIP,
            RW_TRAY_STG_GRIP,
            GD_1_ELV_TRAY_STACKER_UP,
            GD_1_ELV_TRAY_STACKER_DOWN,
            GD_2_ELV_TRAY_STACKER_UP,
            GD_2_ELV_TRAY_STACKER_DOWN,
            RW_ELV_TRAY_STACKER_UP,
            RW_ELV_TRAY_STACKER_DOWN,
            EMTY_1_ELV_TRAY_STACKER_UP,
            EMTY_1_ELV_TRAY_STACKER_DOWN,
            EMTY_2_ELV_TRAY_STACKER_UP,
            EMTY_2_ELV_TRAY_STACKER_DOWN,
            SPARE_119,
            SPARE_120,
            SPARE_121,
            TRAY_PK_ATC_CLAMP,
            SPARE_123,
            SPARE_124,
            SPARE_125,
            SPARE_126,
            SPARE_127,
            SPARE_128,
            DRY_ST_AIR_KNIFE,
            DRY_ST_AIR_PIPE_FRONT,                  //IO SHEET -> DRY_ST_AIR_PIPE_FRONT / DRY_ST_AIR_PIPE_REAR 로 나뉨 (FRONT : Y130 / REAR : Y326)
            SPARE_131,
            #endregion

            #region Y200~Y231
            GD1_CONV_STOP,
            GD1_CONV_RESET,
            GD2_CONV_STOP,
            GD2_CONV_RESET,
            RW_CONV_STOP,
            RW_CONV_RESET,
            EMPTY_CONV_STOP,
            EMPTY_CONV_RESET,
            SPARE_208,
            MARK_VISION_BLOW,                       
            MAP_PK_VAC_ON,                    //IO SHEET -> MAP PK WORK VAC PUMP
            MAP_PK_VAC_ON_PUMP,                          //IO SHEET -> MAP PK VAC
            MAP_PK_VAC_OFF_PUMP,                     //IO SHEET -> MAP PK WORK VAC PUMP OFF
            MAP_PK_BLOW,                            //IO SHEET -> MAP PK BACK VAC
            DRY_BLOCK_VAC_PUMP,
            DRY_BLOCK_VAC_ON,
            SPARE_216,
            DRY_BLOCK_BLOW,                         //IO SHEET -> DRY BLOCK BACK VAC
            MAP_ST_1_VAC_ON_PUMP,
            MAP_ST_1_VAC_ON,
            MAP_ST_1_BLOW,
            SPARE_221,
            SPARE_222,
            SPARE_223,
            MAP_ST_2_VAC_ON_PUMP,
            MAP_ST_2_VAC_ON,
            MAP_ST_2_BLOW,
            SPARE_227,
            SPARE_228,
            SPARE_229,
            SPARE_230,
            SPARE_231,
            #endregion

            #region Y300~Y331
            GD_1_ELV_STOPPER_FWD,
            GD_1_ELV_STOPPER_BWD,
            GD_2_ELV_STOPPER_FWD,
            GD_2_ELV_STOPPER_BWD,
            RW_ELV_STOPPER_FWD,
            RW_ELV_STOPPER_BWD,
            EMPT_1_ELV_STOPPER_FWD,
            EMPT_1_ELV_STOPPER_BWD,
            EMPT_2_ELV_STOPPER_FWD,
            EMPT_2_ELV_STOPPER_BWD,
            GD2_CONV_CCW,
            RW_CONV_CW,
            RW_CONV_CCW,
            SPARE_313,
            SPARE_314,
            SPARE_315,
            SPARE_316,
            SPARE_317,
            SPARE_318,
            SPARE_319,
            GD1_CONV_CW,
            GD1_CONV_CCW,
            GD2_CONV_CW,
            MAP_ST_AIR_KNIFE_1,                         //IO SHEET -> MAP ST AIR KNIFE 1
            MAP_ST_AIR_KNIFE_2,                         //IO SHEET -> MAP ST AIR KNIFE 2
            MAP_PK_AIR_KNIFE,                           //IO SHEET -> MAP PK AIR KNIFE
            EMPTY1_CONV_CW,                       //IO SHEET -> DRY_ST_AIR_PIPE_FRONT / DRY_ST_AIR_PIPE_REAR 로 나뉨 (FRONT : Y130 / REAR : Y326)
            EMPTY1_CONV_CCW,
            EMPTY2_CONV_CW,
            EMPTY2_CONV_CCW,
            EMPTY2_CONV_STOP,
            EMPTY2_CONV_RESET,
            #endregion

            #region Y400~Y431 
            //Vision Coummunication Sig
            MAP_STAGE_NO,                               //VISION COMM #1
            PRE_INSP_COMP,                              //VISION COMM #2
            PRE_INSP_GRAB_REQ,                          //VISION COMM #3
            TOP_ALIGN_CNT_RESET,                        //VISION COMM #4
            TOP_ALIGN_COMP,                             //VISION COMM #5
            MAP_CNT_RESET,                              //VISION COMM #6
            MAP_INSP_COMP,                              //VISION COMM #7
            BALL_HEAD_NO,                               //VISION COMM #8
            BALL_CNT_RESET,                             //VISION COMM #9
            BALL_INSP_COMP,                             //VISION COMM #10
            PICKER_CAL_RESET,                           //VISION COMM #11
            PICKER_CAL_COMP,                            //VISION COMM #12
            PICKER_CAL_LED_ON,                          //VISION COMM #13
            PICKER_CAL_LED_OFF,                         //VISION COMM #14
            SPARE_414,                                  //VISION COMM #15
            SPARE_415,                                  //VISION COMM #16
            SPARE_416,                                  //VISION COMM #17
            SPARE_417,                                  //VISION COMM #18
            SPARE_418,                                  //VISION COMM #19
            SPARE_419,                                  //VISION COMM #20
            SPARE_420,                                  //VISION COMM #21
            SPARE_421,                                  //VISION COMM #22
            SPARE_422,                                  //VISION COMM #23
            SPARE_423,                                  //VISION COMM #24
            SPARE_424,                                  //VISION COMM #25
            SPARE_425,                                  //VISION COMM #26
            SPARE_426,                                  //VISION COMM #27
            SPARE_427,                                  //VISION COMM #28
            SPARE_428,                                  //VISION COMM #29
            SPARE_429,                                  //VISION COMM #30
            SPARE_430,                                  //VISION COMM #31
            SPARE_431,                                  //VISION COMM #32
            #endregion

            #endregion

            #region Loader

            #region Y2000~Y2031
            IN_LET_TABLE_UP,
            IN_LET_TABLE_DOWN,
            IN_LET_TABLE_VAC,
            IN_LET_TABLE_BLOW,
            STRIP_PK_AIR_BLOW,
            LD_X_GRIP_UP,
            LD_X_GRIP_DOWN,
            LD_X_UNGRIP,                                //IO SHEET -> LD_X_UNGRIP
            LD_X_GRIP,
            INLET_TABLE_MATERIAL_IN_STOPPER_UP,         //IO SHEET -> INLET_TABLE_MATERIAL_STOPPER_UP 
            INLET_TABLE_MATERIAL_IN_STOPPER_DOWN,       //IO SHEET -> INLET_TABLE_MATERIAL_STOPPER_DOWN
            STRIP_PK_VAC_OFF_PUMP,
            STRIP_PK_VAC_SOL_QUAD,
            STRIP_PK_VAC_ON_PUMP,
            STRIP_PK_BLOW,
            SPARE_2015,
            SPARE_2016,
            SPARE_2017,
            UNIT_PK_VAC_OFF_PUMP,
            UNIT_PK_VAC_ON_PUMP,
            UNIT_PK_BLOW,
            STRIP_PK_VAC_ON,
            UNIT_PK_VAC_ON,
            UNIT_PK_SCRAP_1_VAC_OFF_PUMP,
            UNIT_PK_SCRAP_1_VAC_ON_PUMP,
            UNIT_PK_SCRAP_1_BLOW,
            UNIT_PK_SCRAP_1_VAC_ON,
            UNIT_PK_SCRAP_2_VAC_OFF_PUMP,
            UNIT_PK_SCRAP_2_VAC_ON_PUMP,
            UNIT_PK_SCRAP_2_BLOW,
            UNIT_PK_SCRAP_2_VAC_ON,
            SPARE_2031,
            #endregion

            #region Y2100~Y2131
            // Dicing Communication
            SPARE_2100,                                 //DICING COMM #1
            SPARE_2101,                                 //DICING COMM #2
            SPARE_2102,                                 //DICING COMM #3
            SPARE_2103,                                 //DICING COMM #4
            SPARE_2104,                                 //DICING COMM #5
            SPARE_2105,                                 //DICING COMM #6
            SPARE_2106,                                 //DICING COMM #7
            SPARE_2107,                                 //DICING COMM #8
            SORTER_PROGRAM_ON,                          //DICING COMM #9
            SORTER_STRIP_PK_PICK_POS_R_X,               //DICING COMM #10
            SORTER_STRIP_PK_PICK_POS_R_Z,               //DICING COMM #11
            SORTER_LD_OK_R,                             //DICING COMM #12
            SORTER_BLOW_OFF_R,                          //DICING COMM #13
            SORTER_STRIP_PK_PICK_POS_X,                 //DICING COMM #14
            SORTER_STRIP_PK_PICK_POS_Z,                 //DICING COMM #15
            SORTER_LD_OK,                               //DICING COMM #16
            SORTER_BLOW_OFF,                            //DICING COMM #17
            SORTER_UNIT_PK_PICK_POS_R_X,                //DICING COMM #18
            SORTER_UNIT_PK_PICK_POS_R_Z,                //DICING COMM #19
            SORTER_ULD_OK_R,                            //DICING COMM #20
            SORTER_UNIT_PK_PICK_POS_X,                  //DICING COMM #21
            SORTER_UNIT_PK_PICK_POS_Z,                  //DICING COMM #22
            SORTER_ULD_OK,                              //DICING COMM #23
            DICING_COMM_8,                              //DICING COMM #24
            DICING_COMM_9,                              //DICING COMM #25
            DICING_COMM_10,                             //DICING COMM #26
            SORTER_HBC_MDL_MOVE_PERMIT,                 //DICING COMM #27
            DICING_COMM_12,                             //DICING COMM #28
            SORTER_RCP_CHANGE,                          //DICING COMM #29
            SORTER_ERROR,                               //DICING COMM #30
            SORTER_PK_INTERLOCK_R,                      //DICING COMM #31
            SORTER_PK_INTERLOCK,                        //DICING COMM #32
            #endregion

            #region Y2200~2231
            CLEAN_WATER_1_WATER,                        //IO SHEET -> Y2200 (기존 Y3301 에서 변경)
            CLEAN_WATER_1_AIR,                          //IO SHEET -> Y2201 (기존 Y3302 에서 변경)
            CLEAN_WATER_2_WATER,                        //IO SHEET -> Y2202 (기존 Y3303 에서 변경)
            CLEAN_WATER_2_AIR,                          //IO SHEET -> Y2203 (기존 Y3304 에서 변경)
            CLEAN_WATER_3_WATER,                        //IO SHEET -> Y2204 (기존 Y3305 에서 변경)
            CLEAN_WATER_3_AIR,                          //IO SHEET -> Y2205 (기존 Y3306 에서 변경)
            CLEAN_WATER_4_WATER,                        //IO SHEET -> Y2206 (기존 Y3307 에서 변경)
            CLEAN_WATER_4_AIR,                          //IO SHEET -> Y2207 (기존 Y3308 에서 변경)
            CLEAN_BRUSH_WATER,                          //IO SHEET -> Y2208 (기존 Y3309 에서 변경)
            CLEAN_AIR_KNIFE_1,                          //IO SHEET -> Y2209 (기존 Y3310 에서 변경)
            CLEAN_AIR_KNIFE_2,                          //IO SHEET -> Y2210 (기존 Y3311 에서 변경)
            CLEAN_AIR_BLOW_1_PIPE,                      //IO SHEET -> Y2211 (기존 Y3312 에서 변경)
            CLEAN_AIR_BLOW_2_PIPE,                      //IO SHEET -> Y2212 (기존 Y3313 에서 변경)
            CLEAN_SWING_FWD,                            //IO SHEET -> Y2213 (기존 Y3314 에서 변경)
            CLEAN_SWING_BWD,                            //IO SHEET -> Y2214 (기존 Y3315 에서 변경)
            SPARE_2215,
            SPARE_2216,
            SPARE_2217,
            SPARE_2218,
            SPARE_2219,
            SPARE_2220,
            SPARE_2221,
            SPARE_2222,
            SPARE_2223,
            SPARE_2224,
            SPARE_2225,
            SPARE_2226,
            TOP_INTERFACE_1,
            TOP_INTERFACE_2,
            TOP_INTERFACE_3,
            TOP_INTERFACE_4,
            TOP_INTERFACE_5,

            #endregion Y2200~2231

            #region Y3000~Y3031
            _1F_CONVEYOR_INPUT_SWITCH_LED,
            _1F_CONVEYOR_OUTPUT_SWITCH_LED,
            _2F_CONVEYOR_INPUT_SWITCH_LED,
            _2F_CONVEYOR_OUTPUT_SWITCH_LED,
            DOOR_LOCK_SIGNAL_LD,
            SERVO_CONTROL_POWER_OFF_SIGNAL,
            _1F_MZ_CONVEYOR_FORWARD_SIGNAL,
            _1F_MZ_CONVEYOR_BACKWARD_SIGNAL,
            _1F_MZ_CONVEYOR_STOP_SIGNAL,
            _1F_MZ_CONVEYOR_RESET_SIGNAL,
            _2F_MZ_CONVEYOR_FORWARD_SIGNAL,
            _2F_MZ_CONVEYOR_BACKWARD_SIGNAL,
            _2F_MZ_CONVEYOR_STOP_SIGNAL,
            _2F_MZ_CONVEYOR_RESET_SIGNAL,
            ME_CONVEYOR_FORWARD_SIGNAL,
            ME_CONVEYOR_BACKWARD_SIGNAL,
            ME_CONVEYOR_STOP_SIGNAL,
            ME_CONVEYOR_RESET_SIGNAL,
            ME_MZ_CLAMP_SOL,
            ME_MZ_UNCLAMP_SOL,
            MZ_DOOR_OPEN_SOL,
            MZ_DOOR_CLOSE_SOL,
            _1F_MZ_STOPPER_UP_SOL,
            _1F_MZ_STOPPER_DOWN_SOL,
            _2F_MZ_STOPPER_UP_SOL,
            _2F_MZ_STOPPER_DOWN_SOL,
            _1F_INPUT_STOPPER_UP_SOL,
            _1F_INPUT_STOPPER_DOWN_SOL,
            LOADING_PUSHER_FORWARD_SOL,
            LOADING_PUSHER_BACKWARD_SOL,
            SPARE_3030,
            SPARE_3031,
            #endregion

            #region Y3100~3131
            SPARE_3100,
            SPARE_3101,
            SPARE_3102,
            SPARE_3103,
            SPARE_3104,
            SPARE_3105,
            SPARE_3106,
            SPARE_3107,
            SPARE_3108,
            SPARE_3109,
            SPARE_3110,
            SPARE_3111,
            SPARE_3112,
            SPARE_3113,
            SPARE_3114,
            SPARE_3115,
            SPARE_3116,
            SPARE_3117,                                         //IO SHEET -> (삭제)
            SPARE_3118,                                         //IO SHEET -> (삭제)
            SPARE_3119,                                         //IO SHEET -> (삭제)
            SPARE_3120,                                         //IO SHEET -> (삭제)
            SPARE_3121,                                         //IO SHEET -> (삭제)
            SPARE_3122,                                         //IO SHEET -> (삭제)
            SPARE_3123,                                         //IO SHEET -> (삭제)
            SPARE_3124,                                         //IO SHEET -> (삭제)
            SPARE_3125,                                         //IO SHEET -> (삭제)
            SPARE_3126,                                         //IO SHEET -> (삭제)
            SPARE_3127,                                         //IO SHEET -> (삭제)
            SPARE_3128,                                         //IO SHEET -> (삭제)
            SPARE_3129,                                         //IO SHEET -> (삭제)
            SPARE_3130,                                         //IO SHEET -> (삭제)
            SPARE_3131,                                         //IO SHEET -> (삭제)
            #endregion

            #endregion
            MAX,
        }

        static IODF()
        {

        }

        public enum ONOFF
        {
            ON = 0,
            OFF = 1,
            KEEP = 2
        }
    }
#endregion

#region ERDF


    public enum ERDF
    {
        E_NONE = 0,
        E_EQ_POWER_TURN_OFF = 1,
        E_ETHERCAT_COMM_FAIL = 2,
        E_WRONG_SEQUENCE_NUMBER = 3,
        E_LOT_END_STOP = 4,

        E_LON_BLOWER_POWER_TRIP = 6,
        E_DICING_SERVO_MC_1_TRIP = 7,
        E_DICING_SERVO_MC_2_TRIP = 8,
        E_CP_70_TRIP_SIGNAL,
        E_CP_71_TRIP_SIGNAL,
        E_CP_72_TRIP_SIGNAL,
        E_CP_73_TRIP_SIGNAL,

        E_EMO_1 = 15,       //(int)IODF.INPUT.EMO_FRONT_SWITCH_SAW,
        E_EMO_2,            //(int)IODF.INPUT.EMO_LF_SIDE_SWITCH_SAW,
        E_EMO_3,            //(int)IODF.INPUT.EMO_REAR_SWITCH_SAW,
        E_EMO_4,            //(int)IODF.INPUT.EMO_SWITCH_1,
        E_EMO_5,            //(int)IODF.INPUT.EMO_SWITCH_2,
        E_EMO_6,            //(int)IODF.INPUT.EMO_SWITCH_3,
        E_EMO_7,            //(int)IODF.INPUT.EMO_SWITCH_4
        E_EMO_8,            //(int)IODF.INPUT.EMO_SWITCH_4
        E_EMO_9,            //(int)IODF.INPUT.EMO_SWITCH_4

        E_DOOR_OPEN_1 = 25, //(int)IODF.INPUT.FORNT_MONITOR_DOOR, 
        E_DOOR_OPEN_2,      //(int)IODF.INPUT.FORNT_MONITOR_RT_SID_DOOR, 
        E_DOOR_OPEN_3,      //(int)IODF.INPUT.FORNT_TRAY_TOP_LF_DOOR,
        E_DOOR_OPEN_4,      //(int)IODF.INPUT.FORNT_TRAY_TOP_RT_DOOR, 
        E_DOOR_OPEN_5,      //(int)IODF.INPUT.FRONT_LF_DOOR_SAW, 
        E_DOOR_OPEN_6,      //(int)IODF.INPUT.FRONT_MONITOR_DOOR_SAW,
        E_DOOR_OPEN_7,      //(int)IODF.INPUT.FRONT_RT_DOOR_SAW,
        E_DOOR_OPEN_8,      //(int)IODF.INPUT.LF_SIDE_LF_DOOR,
        E_DOOR_OPEN_9,      //(int)IODF.INPUT.LF_SIDE_MID_DOOR,
        E_DOOR_OPEN_10,     //(int)IODF.INPUT.LF_SIDE_RT_DOOR,
        E_DOOR_OPEN_11,     //35 //(int)IODF.INPUT.OHT_FENCE_FRONT_DOOR,
        E_DOOR_OPEN_12, //(int)IODF.INPUT.REAR_CENTER_DOOR,
        E_DOOR_OPEN_13, //(int)IODF.INPUT.REAR_LF_DOOR,
        E_DOOR_OPEN_14, //(int)IODF.INPUT.REAR_RT_DOOR,
        E_DOOR_OPEN_15, //(int)IODF.INPUT.RT_SIDE_CENTER_DOOR,
        E_DOOR_OPEN_16, //40 //(int)IODF.INPUT.RT_SIDE_LF_DOOR,
        E_DOOR_OPEN_17, //(int)IODF.INPUT.RT_SIDE_RT_DOOR

        E_DOOR_LOCK_TIME_OUT_1 = 45,
        E_DOOR_LOCK_TIME_OUT_2 ,
        E_DOOR_LOCK_TIME_OUT_3 ,
        E_DOOR_LOCK_TIME_OUT_4 ,
        E_DOOR_LOCK_TIME_OUT_5 ,
        E_DOOR_LOCK_TIME_OUT_6 ,//50
        E_DOOR_LOCK_TIME_OUT_7 ,
        E_DOOR_LOCK_TIME_OUT_8 ,
        E_DOOR_LOCK_TIME_OUT_9 ,
        E_DOOR_LOCK_TIME_OUT_10,
        E_DOOR_LOCK_TIME_OUT_11,//55
        E_DOOR_LOCK_TIME_OUT_12,
        E_DOOR_LOCK_TIME_OUT_13,
        E_DOOR_LOCK_TIME_OUT_14,
        E_DOOR_LOCK_TIME_OUT_15,
        E_DOOR_LOCK_TIME_OUT_16,//60
        E_DOOR_LOCK_TIME_OUT_17,
        E_DOOR_LOCK_TIME_OUT_18,
        E_DOOR_LOCK_TIME_OUT_19,

        E_RT_SIDE_TOP_LF_FAN_ALARM = 65,
        E_RT_SIDE_TOP_RT_FAN_ALARM,
        E_REAR_SIDE_TOP_LF_FAN_ALARM,
        E_REAR_SIDE_TOP_RT_FAN_ALARM,
        E_TOP_LF_FRONT_FAN_ALARM,
        E_TOP_LF_REAR_FAN_ALARM,
        E_TOP_RT_FRONT_FAN_ALARM,
        E_TOP_RT_REAR_FAN_ALARM,
        E_SMOKE_DETECTOR,
        E_ION_BLOWER_FAN_ALARM_1,
        E_ION_BLOWER_FAN_ALARM_2 = 75,
        E_ION_BAR_HV_ABNOMAL_ALARM_1,
        E_ION_BAR_HV_ABNOMAL_ALARM_2,
        E_TRAY_LD_PORT_SAFETY_SENSOR_DETECT,

        E_MAP_PK_ATC_CLAMP_FAIL,
        E_MAP_PK_ATC_UNCLAMP_FAIL = 80,

        E_MAP_STAGE_1_ATC_GRIP_FWD_FAIL,
        E_MAP_STAGE_1_ATC_GRIP_BWD_FAIL,
        E_MAP_STAGE_2_ATC_GRIP_FWD_FAIL,
        E_MAP_STAGE_2_ATC_GRIP_BWD_FAIL,
        E_MAP_STAGE_1_APC_UP_FAIL = 85, //MAP_BLOCK_STAGE
        E_MAP_STAGE_1_APC_DOWN_FAIL,
        E_MAP_STAGE_1_APC_ALIGN_FWD_FAIL,
        E_MAP_STAGE_1_APC_ALIGN_BWD_FAIL,
        E_MAP_STAGE_1_ATC_CLAMP_FAIL,
        E_MAP_STAGE_1_ATC_UNCLAMP_FAIL,//90
        E_MAP_STAGE_2_APC_UP_FAIL,
        E_MAP_STAGE_2_APC_DOWN_FAIL,
        E_MAP_STAGE_2_APC_ALIGN_FWD_FAIL,
        E_MAP_STAGE_2_APC_ALIGN_BWD_FAIL,
        E_MAP_STAGE_2_ATC_CLAMP_FAIL,// = 95,
        E_MAP_STAGE_2_ATC_UNCLAMP_FAIL,
        E_BALL_VISION_ALIGN_FWD,
        E_BALL_VISION_ALIGN_BWD,
        E_GD_TRAY_STAGE_1_UP_FAIL,
        E_GD_TRAY_STAGE_1_DOWN_FAIL,//100
        E_GD_TRAY_STAGE_1_ALIGN_FWD_FAIL,
        E_GD_TRAY_STAGE_1_ALIGN_BWD_FAIL,
        E_GD_TRAY_STAGE_1_GRIP_FAIL,
        E_GD_TRAY_STAGE_1_UNGRIP_FAIL,
        E_GD_TRAY_STAGE_1_CHECK_FAIL,//105
        E_GD_TRAY_STAGE_2_UP_FAIL,
        E_GD_TRAY_STAGE_2_DOWN_FAIL,
        E_GD_TRAY_STAGE_2_ALIGN_FWD_FAIL,
        E_GD_TRAY_STAGE_2_ALIGN_BWD_FAIL,
        E_GD_TRAY_STAGE_2_GRIP_FAIL, // = 110,
        E_GD_TRAY_STAGE_2_UNGRIP_FAIL,
        E_GD_TRAY_STAGE_2_CHECK_FAIL,
        E_RW_TRAY_STAGE_ALIGN_FWD_FAIL,
        E_RW_TRAY_STAGE_ALIGN_BWD_FAIL,
        E_RW_TRAY_STAGE_GRIP_FAIL,// = 115,
        E_RW_TRAY_STAGE_UNGRIP_FAIL,
        E_RW_TRAY_STAGE_CHECK_FAIL,
        E_TRAY_PK_CLAMP_FAIL,
        E_TRAY_PK_UNCLAMP_FAIL,
        E_TRAY_PK_GRIP_CHECK_FAIL,// = 120,


        E_GD_1_TRAY_ELV_STACKER_UP_FAIL,
        E_GD_1_TRAY_ELV_STACKER_DOWN_FAIL,
        E_GD_1_TRAY_ELV_GUIDE_FWD_FAIL,
        E_GD_1_TRAY_ELV_GUIDE_BWD_FAIL,
        E_GD_1_TRAY_ELV_TRAY_NOT_EXIST,
        E_GD_1_TRAY_ELV_TRAY_MISPUT_UP,
        E_GD_1_TRAY_CONV_STOPPER_FWD_FAIL,
        E_GD_1_TRAY_CONV_STOPPER_BWD_FAIL,


        E_GD_2_TRAY_ELV_STACKER_UP_FAIL,
        E_GD_2_TRAY_ELV_STACKER_DOWN_FAIL,
        E_GD_2_TRAY_ELV_GUIDE_FWD_FAIL,
        E_GD_2_TRAY_ELV_GUIDE_BWD_FAIL,
        E_GD_2_TRAY_ELV_TRAY_NOT_EXIST,
        E_GD_2_TRAY_ELV_TRAY_MISPUT_UP,
        E_GD_2_TRAY_CONV_STOPPER_FWD_FAIL,
        E_GD_2_TRAY_CONV_STOPPER_BWD_FAIL,

        E_RW_TRAY_ELV_STACKER_UP_FAIL,
        E_RW_TRAY_ELV_STACKER_DOWN_FAIL,
        E_RW_TRAY_ELV_GUIDE_FWD_FAIL,
        E_RW_TRAY_ELV_GUIDE_BWD_FAIL,
        E_RW_TRAY_ELV_TRAY_IS_NOT_DETECTION,
        E_RW_TRAY_ELV_TRAY_MISPUT_UP,
        E_RW_TRAY_CONV_STOPPER_FWD_FAIL,
        E_RW_TRAY_CONV_STOPPER_BWD_FAIL,


        E_EMPTY_1_TRAY_ELV_STACKER_UP_FAIL,
        E_EMPTY_1_TRAY_ELV_STACKER_DOWN_FAIL,
        E_EMPTY_1_TRAY_ELV_GUIDE_FWD_FAIL,
        E_EMPTY_1_TRAY_ELV_GUIDE_BWD_FAIL,
        E_EMPTY_1_TRAY_ELV_TRAY_IS_NOT_DETECTION,
        E_EMPTY_1_TRAY_ELV_TRAY_MISPUT_UP,
        E_EMPTY_1_TRAY_CONV_STOPPER_FWD_FAIL,
        E_EMPTY_1_TRAY_CONV_STOPPER_BWD_FAIL,


        E_EMPTY_2_TRAY_ELV_STACKER_UP_FAIL,
        E_EMPTY_2_TRAY_ELV_STACKER_DOWN_FAIL,
        E_EMPTY_2_TRAY_ELV_GUIDE_FWD_FAIL,
        E_EMPTY_2_TRAY_ELV_GUIDE_BWD_FAIL,
        E_EMPTY_2_TRAY_ELV_TRAY_IS_NOT_DETECTION,
        E_EMPTY_2_TRAY_ELV_TRAY_MISPUT_UP,
        E_EMPTY_2_TRAY_CONV_STOPPER_FWD_FAIL,
        E_EMPTY_2_TRAY_CONV_STOPPER_BWD_FAIL,

        E_THERE_IS_NO_TRAY,//220528 pjh 트레이 없을 시 발생 알람

        E_PICKER_CAL_PATTERN_NOT_FOUND,
        E_PICKER_CAL_RETRY_OVER,

        E_NO_MSG_STOP,      // 무언정지 알람

        E_SECOND_CLEANING_ZONE_COVER_OPEN_FAIL = 165,
        E_SECOND_CLEANING_ZONE_COVER_CLOSE_FAIL,
        E_SECOND_CLEANING_ZONE_WJ_BACKWARD_FAIL,
        E_SECOND_CLEANING_ZONE_WJ_FORWARD_FAIL,

        //
        E_PRE_ALIGN_DATA_MISSING_ERROR = 170, //프리얼라인 데이터가 존재하지 않음
        E_TOP_ALIGN_DATA_MISSING_ERROR, //탑 얼라인 데이터가 존재하지 않음
        E_TOP_VISION_INSPECTION_DATA_MISSING_ERROR, //탑 비전 검사 데이터가 존재하지 않음
        E_BOTTOM_VISION_INSPECTION_DATA_MISSING_ERROR, //하부 비전 검사 데이터가 존재하지 않음
        E_X_MARK_INSPECTION_DATA_MISSING_ERROR, //XMARK 비전 검사 데이터가 존재하지 않음
        E_VISION_DATA_PARSING_ERROR = 177,
        E_MAP_INSPECTION_ERROR_COUNT_OVER,  // Map Inspection 시 검사 오류 수가 넘어갈 시 알람

        E_ITS_ID_STRIP_ID_UNMATCHED, //220604 ITS ID 앞 15자리와 스트립 앞 15자리가 불일치할 경우 알람 

#region VAC
        E_DRIVE_AIR_LOW = 180,
        E_DRIVE_AIR_HIGH,
        E_BLOW_AIR_LOW,
        E_BLOW_AIR_HIGH,
        E_CHIP_PK_AIR_LOW,
        E_CHIP_PK_AIR_HIGH,
        E_MAP_PK_AIR_LOW,
        E_MAP_PK_AIR_HIGH,
        E_MAP_STAGE_AIR_LOW,
        E_MAP_STAGE_AIR_HIGH,
        E_DRY_STAGE_AIR_LOW,
        E_DRY_STAGE_AIR_HIGH,
        E_PUMP_1_AIR_LOW,
        E_PUMP_1_AIR_HIGH,
        E_PUMP_2_AIR_LOW,
        E_PUMP_2_AIR_HIGH,
        E_MAP_PK_WORK_VAC_NOT_ON,
        E_MAP_PK_WORK_VAC_NOT_OFF,
        E_MAP_PK_CHUCK_VAC_NOT_ON,
        E_MAP_PK_CHUCK_VAC_NOT_OFF,
        E_DRY_BLOCK_WORK_VAC_NOT_ON,
        E_DRY_BLOCK_WORK_VAC_NOT_OFF,
        E_DRY_BLOCK_CHUCK_VAC_NOT_ON,
        E_DRY_BLOCK_CHUCK_VAC_NOT_OFF,
        SPARE_204,
        SPARE_205,
        SPARE_206,
        SPARE_207,
        E_MAP_STAGE_1_WORK_VAC_NOT_ON,
        E_MAP_STAGE_1_WORK_VAC_NOT_OFF,
        SPARE_210,
        SPARE_211,
        SPARE_212,
        E_MAP_STAGE_2_WORK_VAC_NOT_ON,
        E_MAP_STAGE_2_WORK_VAC_NOT_OFF,


        E_X1_PK_VAC_1_NOT_ON = 215,
        E_X1_PK_VAC_1_NOT_OFF,
        E_X1_PK_VAC_2_NOT_ON,
        E_X1_PK_VAC_2_NOT_OFF,
        E_X1_PK_VAC_3_NOT_ON,
        E_X1_PK_VAC_3_NOT_OFF,
        E_X1_PK_VAC_4_NOT_ON,
        E_X1_PK_VAC_4_NOT_OFF,
        E_X1_PK_VAC_5_NOT_ON,
        E_X1_PK_VAC_5_NOT_OFF,
        E_X1_PK_VAC_6_NOT_ON,
        E_X1_PK_VAC_6_NOT_OFF,
        E_X1_PK_VAC_7_NOT_ON,
        E_X1_PK_VAC_7_NOT_OFF,
        E_X1_PK_VAC_8_NOT_ON,
        E_X1_PK_VAC_8_NOT_OFF,
        E_X2_PK_VAC_1_NOT_ON,
        E_X2_PK_VAC_1_NOT_OFF,
        E_X2_PK_VAC_2_NOT_ON,
        E_X2_PK_VAC_2_NOT_OFF,
        E_X2_PK_VAC_3_NOT_ON,
        E_X2_PK_VAC_3_NOT_OFF,
        E_X2_PK_VAC_4_NOT_ON,
        E_X2_PK_VAC_4_NOT_OFF,
        E_X2_PK_VAC_5_NOT_ON,
        E_X2_PK_VAC_5_NOT_OFF,
        E_X2_PK_VAC_6_NOT_ON,
        E_X2_PK_VAC_6_NOT_OFF,
        E_X2_PK_VAC_7_NOT_ON,
        E_X2_PK_VAC_7_NOT_OFF,
        E_X2_PK_VAC_8_NOT_ON,
        E_X2_PK_VAC_8_NOT_OFF,
        E_UNIT_PK_VAC_NOT_ON_1,
        E_UNIT_PK_VAC_NOT_ON_2,
        E_STRIP_PK_VAC_NOT_ON,
        E_STRIP_PK_VAC_NOT_OFF,
        E_STRIP_PK_KIT_VAC_NOT_ON,
        E_STRIP_PK_KIT_VAC_NOT_OFF,
        E_UNIT_PK_VAC_NOT_ON,
        E_UNIT_PK_VAC_NOT_OFF,
        E_UNIT_PK_KIT_VAC_NOT_ON,
        E_UNIT_PK_KIT_VAC_NOT_OFF,
        E_LD_RAIL_VAC_NOT_ON,
        E_LD_RAIL_VAC_NOT_OFF,
        E_FIRST_ULTRASONIC_WATER_IN_HIGH,
        E_SECOND_ULTRASONIC_WATER_IN_LOW,
        E_HEATING_REGULATOR_HIGH,
        E_HEATING_REGULATOR_LOW,
        E_SECOND_ULTRASONIC_VAC_NOT_ON,
        E_SECOND_ULTRASONIC_VAC_NOT_OFF,
        E_CLEAN_PICKER_VAC_NOT_ON,
        E_CLEAN_PICKER_VAC_NOT_OFF,
        E_SECOND_CLEAN_VAC_NOT_ON,
        E_SECOND_CLEAN_VAC_NOT_OFF,
        E_HEATING_AIR_HIGH,
        E_HEATING_AIR_LOW,
        E_FIRST_CLEAN_AIR_HIGH,
        E_FIRST_CLEAN_AIR_LOW,
        E_FIRST_CLEAN_AIR_2_HIGH,
        E_FIRST_CLEAN_AIR_2_LOW,
        E_SECOND_CLEAN_AIR_HIGH,
        E_SECOND_CLEAN_AIR_LOW,
#endregion

#region DICING
        E_LD_RAIL_MATERIAL_IS_NOT_DETECTION = 300,
        E_IN_LET_TABLE_UP_FAIL,
        E_IN_LET_TABLE_DOWN_FAIL,
        E_LD_VISION_GRIPPER_UP_FAIL,
        E_LD_VISION_GRIPPER_DOWN_FAIL,
        E_LD_VISION_GRIPPER_UNGRIP_FAIL,
        E_LD_VISION_GRIPPER_GRIP_FAIL,
        E_LD_VISION_GRIPPER_MATERIAL_IS_NOT_DETECTION,
        E_STRIP_PK_n_UNIT_PK_COLLISION,
        E_STRIP_PK_n_UNIT_PK_INTERLOCK,

        E_STRIP_PK_KIT_CLAMP_FAIL,
        E_STRIP_PK_KIT_UNCLAMP_FAIL,
        E_UNIT_PK_KIT_CLAMP_FAIL,
        E_UNIT_PK_KIT_UNCLAMP_FAIL,
        E_CLEANER_SWING_FWD_FAIL,
        E_CLEANER_SWING_BWD_FAIL,
        E_CLEANER_BOX_WATER , //water 감지 시 알람인것 인지 감지 되지 않을때 알람인것인지 몰라서 일단 정의 만 해둠
        E_TURN_TABLE_WATER,

#endregion

#region COMM ERROR
        E_LD_CONV_COMM_FAIL = 335,
        E_ULD_CONV_COMM_FAIL,
        E_RFID_CASTING_FAIL,
        E_LOADER_RFIO_COMM_ERROR_1,
        E_LOADER_RFIO_COMM_ERROR_2,
        E_LOADER_RFIO_COMM_ERROR_3,
        E_LOADER_RFIO_COMM_ERROR_4,
        E_LOADER_RFIO_COMM_ERROR_5,
        E_LOADER_RFIO_COMM_ERROR_6,
        E_LOADER_RFIO_COMM_ERROR_7,
        E_LOADER_RFIO_COMM_ERROR_8,
        E_LOADER_RFIO_COMM_ERROR_9,
        E_LOADER_RFIO_COMM_ERROR_10,

        E_SAW_NOT_READY = 349,
        E_DICING_COMM_FAIL,
        E_DICING_COMM_TIME_OUT,
        E_DICING_DATA_CASTING_FAIL,

        E_GD_TRAY_ELV_RFIO_COMM_ERROR_1 = 355,
        E_GD_TRAY_ELV_RFIO_COMM_ERROR_2,
        E_GD_TRAY_ELV_RFIO_COMM_ERROR_3,
        E_GD_TRAY_ELV_RFIO_COMM_ERROR_4,
        E_GD_TRAY_ELV_RFIO_COMM_ERROR_5,

        E_RW_TRAY_ELV_RFIO_COMM_ERROR_1,
        E_RW_TRAY_ELV_RFIO_COMM_ERROR_2,
        E_RW_TRAY_ELV_RFIO_COMM_ERROR_3,
        E_RW_TRAY_ELV_RFIO_COMM_ERROR_4,
        E_RW_TRAY_ELV_RFIO_COMM_ERROR_5,

        E_EMPTY_1_TRAY_ELV_RFIO_COMM_ERROR_1,
        E_EMPTY_1_TRAY_ELV_RFIO_COMM_ERROR_2,
        E_EMPTY_1_TRAY_ELV_RFIO_COMM_ERROR_3,
        E_EMPTY_1_TRAY_ELV_RFIO_COMM_ERROR_4,
        E_EMPTY_1_TRAY_ELV_RFIO_COMM_ERROR_5,

        E_EMPTY_2_TRAY_ELV_RFIO_COMM_ERROR_1,
        E_EMPTY_2_TRAY_ELV_RFIO_COMM_ERROR_2,
        E_EMPTY_2_TRAY_ELV_RFIO_COMM_ERROR_3,
        E_EMPTY_2_TRAY_ELV_RFIO_COMM_ERROR_4,
        E_EMPTY_2_TRAY_ELV_RFIO_COMM_ERROR_5,

        E_COVER_TRAY_ELV_RFIO_COMM_ERROR_1,
        E_COVER_TRAY_ELV_RFIO_COMM_ERROR_2,
        E_COVER_TRAY_ELV_RFIO_COMM_ERROR_3,
        E_COVER_TRAY_ELV_RFIO_COMM_ERROR_4,
        E_COVER_TRAY_ELV_RFIO_COMM_ERROR_5,

        E_VISION_COMM_ERROR_1,
        E_VISION_COMM_ERROR_2,
        E_VISION_COMM_ERROR_3,
        E_VISION_COMM_ERROR_4,
        E_VISION_COMM_ERROR_5,

        E_IONIZER_VALUE_ERROR_1,
        E_IONIZER_VALUE_ERROR_2,
        E_IONIZER_VALUE_ERROR_3,
        E_IONIZER_VALUE_ERROR_4,
#endregion

#region MOTION
        E_SV_AMP = 400,
        E_SV_SERVO_ON = 500,
        E_SV_MOVE = 600,
        E_SV_HOME = 700,
        E_SV_SW_LIMIT_P = 800,
        E_SV_SW_LIMIT_N = 900,
        E_SV_MOT_INFO = 1000,
        E_SV_NOT_HOME = 1100,
        E_SV_HW_LIMIT_P = 1200,
        E_SV_HW_LIMIT_N = 1300,
        E_SV_HOME_INTERLOCK = 1400,
        E_SV_MOVE_INTERLOCK = 1500,
        E_SV_INVALID_VEL = 1600,
#endregion

#region OHT
#endregion

#region TIMEOUT
        E_PLASMA_SHOT_TIMEOUT = 1710,
        E_FIRST_ULTRA_WATER_IN_TIMEOUT,
        E_SECOND_ULTRA_WATER_IN_TIMEOUT,
        E_LOADER_CONV_5F_RFID_READ_TIMEOUT = 1720,
        E_LOADER_CONV_4F_RFID_READ_TIMEOUT,
        E_LOADER_MGZ_LD_ELV_RFID_READ_TIMEOUT,
        E_LOADER_MGZ_ULD_ELV_RFID_READ_TIMEOUT,
        E_LOADER_CONV_1F_ROLLING_TIMEOUT,
        E_LOADER_CONV_2F_ROLLING_TIMEOUT,
        E_LOADER_CONV_3F_ROLLING_TIMEOUT,
        E_LOADER_CONV_4F_ROLLING_TIMEOUT,
        E_LOADER_CONV_5F_ROLLING_TIMEOUT,

        E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT = 1730,
        E_STRIP_PK_UNLOAD_TO_RT_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT,
        E_STRIP_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT,
        E_STRIP_PK_LOAD_FROM_RT_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT,
        E_UNIT_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT,
        E_UNIT_PK_LOAD_FROM_RT_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT,
        E_UNIT_PK_GET_WORKING_DATA_FROM_SAW_MACHINE_INTERFACE_TIMEOUT,


        E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_REQ_ON = 1740,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_REQ_OFF,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_COMP_ON,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_COMP_OFF,

        E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_RESET_ON,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_RESET_OFF,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_COMP_ON,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_COMP_OFF,

        E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_RESET_ON,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_RESET_OFF,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_COMP_ON = 1750,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_COMP_OFF,

        E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_RESET_ON,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_RESET_OFF,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_ON,
        E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_OFF,

        E_VISION_PROGRAM_INTERFACE_TIMEOUT_READY,
        E_VISION_PROGRAM_IS_NOT_READY,
        E_VISION_PROGRAM_CODE_CAN_NOT_READ,
        //20
        E_MES_MGZ_IN_CONFIRM_RECV_TIMEOUT = 1760,
        E_MES_SUB_LOAD_CONFIRM_RECV_TIMEOUT,
        E_MES_SUBMAP_DOWNLOAD_REPLY_TIMEOUT,
        E_MES_SUB_UNLOAD_CONFIRM_RECV_TIMEOUT,
        E_MES_GD_TRAY_FULL_CONFIRM_RECV_TIMEOUT,
        E_MES_RW_TRAY_FULL_CONFIRM_RECV_TIMEOUT,
        E_MES_LOT_END_AUTO_CONFIRM_RECV_TIMEOUT,
        E_MES_NEW_TRAY_CONFIRM_RECV_TIMEOUT,
        E_MES_TRAY_VERIFY_CONFIRM_RECV_TIMEOUT,
        E_RECIPE_AUTO_CHANGE_TIMEOUT,

        E_VISION_PROGRAM_BALL_INSPECTION_PARSE_ERROR,
        E_TRAY_VISION_TIME_OUT,

        E_GOOD_1_ELV_COVER_CHECK_TIME_OUT,
        E_GOOD_2_ELV_COVER_CHECK_TIME_OUT,
        E_REWORK_ELV_COVER_CHECK_TIME_OUT,
        E_EMPTY_1_ELV_COVER_CHECK_TIME_OUT,
        E_EMPTY_2_ELV_COVER_CHECK_TIME_OUT,
#endregion

#region MES ERR (1780 ~ 1789)
        E_MES_MGZ_IN_CONFIRM_FAIL = 1780,
        E_MES_SUB_LOAD_CONFIRM_FAIL,
        E_MES_SUB_UNLOAD_CONFIRM_FAIL,
        E_MES_GD_TRAY_FULL_CONFIRM_FAIL,
        E_MES_RW_TRAY_FULL_CONFIRM_FAIL,
        E_MES_LOT_END_AUTO_CONFIRM_FAIL,
        E_MES_NEW_TRAY_CONFIRM_FAIL,
        E_MES_TRAY_VERIFY_CONFIRM_FAIL,
		E_RECIPE_AUTO_CHANGE_FAIL,
#endregion

#region ITS ERR (1790 ~ 1799)
        E_ITS_ROW_COL_INFO_IS_NOT_MATCH_EQ_SETTING,//ITS의 행렬 COUNT가 설비에 설정된 ROW,COL COUNT와 맞지 않음.
        E_ITS_ROW_COL_CASTING_ERROR, //220528 캐스팅 오류
        E_THERE_IS_NO_ITS_INFORMATION,//220528 ITS검색 안됨
#endregion

#region MARK INSP CONTI ERR (1800 ~ 1899)
        E_MARK_INSP_CONTI_ERR_001 = 1800,
        E_MARK_INSP_CONTI_ERR_002,
        E_MARK_INSP_CONTI_ERR_003,
        E_MARK_INSP_CONTI_ERR_004,
        E_MARK_INSP_CONTI_ERR_005,
        E_MARK_INSP_CONTI_ERR_006,
        E_MARK_INSP_CONTI_ERR_007,
        E_MARK_INSP_CONTI_ERR_008,
        E_MARK_INSP_CONTI_ERR_009,
        E_MARK_INSP_CONTI_ERR_010,
        E_MARK_INSP_CONTI_ERR_011,
        E_MARK_INSP_CONTI_ERR_012,
        E_MARK_INSP_CONTI_ERR_013,
        E_MARK_INSP_CONTI_ERR_014,
        E_MARK_INSP_CONTI_ERR_015,
        E_MARK_INSP_CONTI_ERR_016,
        E_MARK_INSP_CONTI_ERR_017,
        E_MARK_INSP_CONTI_ERR_018,
        E_MARK_INSP_CONTI_ERR_019,
        E_MARK_INSP_CONTI_ERR_020,
        E_MARK_INSP_CONTI_ERR_021,
        E_MARK_INSP_CONTI_ERR_022,
        E_MARK_INSP_CONTI_ERR_023,
        E_MARK_INSP_CONTI_ERR_024,
        E_MARK_INSP_CONTI_ERR_025,
        E_MARK_INSP_CONTI_ERR_026,
        E_MARK_INSP_CONTI_ERR_027,
        E_MARK_INSP_CONTI_ERR_028,
        E_MARK_INSP_CONTI_ERR_029,
        E_MARK_INSP_CONTI_ERR_030,
        E_MARK_INSP_CONTI_ERR_031,
        E_MARK_INSP_CONTI_ERR_032,
        E_MARK_INSP_CONTI_ERR_033,
        E_MARK_INSP_CONTI_ERR_034,
        E_MARK_INSP_CONTI_ERR_035,
        E_MARK_INSP_CONTI_ERR_036,
        E_MARK_INSP_CONTI_ERR_037,
        E_MARK_INSP_CONTI_ERR_038,
        E_MARK_INSP_CONTI_ERR_039,
        E_MARK_INSP_CONTI_ERR_040,
        E_MARK_INSP_CONTI_ERR_041,
        E_MARK_INSP_CONTI_ERR_042,
        E_MARK_INSP_CONTI_ERR_043,
        E_MARK_INSP_CONTI_ERR_044,
        E_MARK_INSP_CONTI_ERR_045,
        E_MARK_INSP_CONTI_ERR_046,
        E_MARK_INSP_CONTI_ERR_047,
        E_MARK_INSP_CONTI_ERR_048,
        E_MARK_INSP_CONTI_ERR_049,
        E_MARK_INSP_CONTI_ERR_050,
        E_MARK_INSP_CONTI_ERR_051,
        E_MARK_INSP_CONTI_ERR_052,
        E_MARK_INSP_CONTI_ERR_053,
        E_MARK_INSP_CONTI_ERR_054,
        E_MARK_INSP_CONTI_ERR_055,
        E_MARK_INSP_CONTI_ERR_056,
        E_MARK_INSP_CONTI_ERR_057,
        E_MARK_INSP_CONTI_ERR_058,
        E_MARK_INSP_CONTI_ERR_059,
        E_MARK_INSP_CONTI_ERR_060,
        E_MARK_INSP_CONTI_ERR_061,
        E_MARK_INSP_CONTI_ERR_062,
        E_MARK_INSP_CONTI_ERR_063,
        E_MARK_INSP_CONTI_ERR_064,
        E_MARK_INSP_CONTI_ERR_065,
        E_MARK_INSP_CONTI_ERR_066,
        E_MARK_INSP_CONTI_ERR_067,
        E_MARK_INSP_CONTI_ERR_068,
        E_MARK_INSP_CONTI_ERR_069,
        E_MARK_INSP_CONTI_ERR_070,
        E_MARK_INSP_CONTI_ERR_071,
        E_MARK_INSP_CONTI_ERR_072,
        E_MARK_INSP_CONTI_ERR_073,
        E_MARK_INSP_CONTI_ERR_074,
        E_MARK_INSP_CONTI_ERR_075,
        E_MARK_INSP_CONTI_ERR_076,
        E_MARK_INSP_CONTI_ERR_077,
        E_MARK_INSP_CONTI_ERR_078,
        E_MARK_INSP_CONTI_ERR_079,
        E_MARK_INSP_CONTI_ERR_080,
        E_MARK_INSP_CONTI_ERR_081,
        E_MARK_INSP_CONTI_ERR_082,
        E_MARK_INSP_CONTI_ERR_083,
        E_MARK_INSP_CONTI_ERR_084,
        E_MARK_INSP_CONTI_ERR_085,
        E_MARK_INSP_CONTI_ERR_086,
        E_MARK_INSP_CONTI_ERR_087,
        E_MARK_INSP_CONTI_ERR_088,
        E_MARK_INSP_CONTI_ERR_089,
        E_MARK_INSP_CONTI_ERR_090,
        E_MARK_INSP_CONTI_ERR_091,
        E_MARK_INSP_CONTI_ERR_092,
        E_MARK_INSP_CONTI_ERR_093,
        E_MARK_INSP_CONTI_ERR_094,
        E_MARK_INSP_CONTI_ERR_095,
        E_MARK_INSP_CONTI_ERR_096,
        E_MARK_INSP_CONTI_ERR_097,
        E_MARK_INSP_CONTI_ERR_098,
        E_MARK_INSP_CONTI_ERR_099,
        E_MARK_INSP_CONTI_ERR_100,
#endregion

#region BALL INSP MATCH FAIL (1900 ~ 1999)
        E_BALL_INSP_MATCH_FAIL_HEAD1_PAD1 = 1900,
        E_BALL_INSP_MATCH_FAIL_HEAD1_PAD2,
        E_BALL_INSP_MATCH_FAIL_HEAD1_PAD3,
        E_BALL_INSP_MATCH_FAIL_HEAD1_PAD4,
        E_BALL_INSP_MATCH_FAIL_HEAD1_PAD5,
        E_BALL_INSP_MATCH_FAIL_HEAD1_PAD6,
        E_BALL_INSP_MATCH_FAIL_HEAD2_PAD1,
        E_BALL_INSP_MATCH_FAIL_HEAD2_PAD2,
        E_BALL_INSP_MATCH_FAIL_HEAD2_PAD3,
        E_BALL_INSP_MATCH_FAIL_HEAD2_PAD4,
        E_BALL_INSP_MATCH_FAIL_HEAD2_PAD5,
        E_BALL_INSP_MATCH_FAIL_HEAD2_PAD6,
        E_BALL_INSP_CONTI_ERR_013,
        E_BALL_INSP_CONTI_ERR_014,
        E_BALL_INSP_CONTI_ERR_015,
        E_BALL_INSP_CONTI_ERR_016,
        E_BALL_INSP_CONTI_ERR_017,
        E_BALL_INSP_CONTI_ERR_018,
        E_BALL_INSP_CONTI_ERR_019,
        E_BALL_INSP_CONTI_ERR_020,
        E_BALL_INSP_CONTI_ERR_021,
        E_BALL_INSP_CONTI_ERR_022,
        E_BALL_INSP_CONTI_ERR_023,
        E_BALL_INSP_CONTI_ERR_024,
        E_BALL_INSP_CONTI_ERR_025,
        E_BALL_INSP_CONTI_ERR_026,
        E_BALL_INSP_CONTI_ERR_027,
        E_BALL_INSP_CONTI_ERR_028,
        E_BALL_INSP_CONTI_ERR_029,
        E_BALL_INSP_CONTI_ERR_030,
        E_BALL_INSP_CONTI_ERR_031,
        E_BALL_INSP_CONTI_ERR_032,
        E_BALL_INSP_CONTI_ERR_033,
        E_BALL_INSP_CONTI_ERR_034,
        E_BALL_INSP_CONTI_ERR_035,
        E_BALL_INSP_CONTI_ERR_036,
        E_BALL_INSP_CONTI_ERR_037,
        E_BALL_INSP_CONTI_ERR_038,
        E_BALL_INSP_CONTI_ERR_039,
        E_BALL_INSP_CONTI_ERR_040,
        E_BALL_INSP_CONTI_ERR_041,
        E_BALL_INSP_CONTI_ERR_042,
        E_BALL_INSP_CONTI_ERR_043,
        E_BALL_INSP_CONTI_ERR_044,
        E_BALL_INSP_CONTI_ERR_045,
        E_BALL_INSP_CONTI_ERR_046,
        E_BALL_INSP_CONTI_ERR_047,
        E_BALL_INSP_CONTI_ERR_048,
        E_BALL_INSP_CONTI_ERR_049,
        E_BALL_INSP_CONTI_ERR_050,
        E_BALL_INSP_CONTI_ERR_051,
        E_BALL_INSP_CONTI_ERR_052,
        E_BALL_INSP_CONTI_ERR_053,
        E_BALL_INSP_CONTI_ERR_054,
        E_BALL_INSP_CONTI_ERR_055,
        E_BALL_INSP_CONTI_ERR_056,
        E_BALL_INSP_CONTI_ERR_057,
        E_BALL_INSP_CONTI_ERR_058,
        E_BALL_INSP_CONTI_ERR_059,
        E_BALL_INSP_CONTI_ERR_060,
        E_BALL_INSP_CONTI_ERR_061,
        E_BALL_INSP_CONTI_ERR_062,
        E_BALL_INSP_CONTI_ERR_063,
        E_BALL_INSP_CONTI_ERR_064,
        E_BALL_INSP_CONTI_ERR_065,
        E_BALL_INSP_CONTI_ERR_066,
        E_BALL_INSP_CONTI_ERR_067,
        E_BALL_INSP_CONTI_ERR_068,
        E_BALL_INSP_CONTI_ERR_069,
        E_BALL_INSP_CONTI_ERR_070,
        E_BALL_INSP_CONTI_ERR_071,
        E_BALL_INSP_CONTI_ERR_072,
        E_BALL_INSP_CONTI_ERR_073,
        E_BALL_INSP_CONTI_ERR_074,
        E_BALL_INSP_CONTI_ERR_075,
        E_BALL_INSP_CONTI_ERR_076,
        E_BALL_INSP_CONTI_ERR_077,
        E_BALL_INSP_CONTI_ERR_078,
        E_BALL_INSP_CONTI_ERR_079,
        E_BALL_INSP_CONTI_ERR_080,
        E_BALL_INSP_CONTI_ERR_081,
        E_BALL_INSP_CONTI_ERR_082,
        E_BALL_INSP_CONTI_ERR_083,
        E_BALL_INSP_CONTI_ERR_084,
        E_BALL_INSP_CONTI_ERR_085,
        E_BALL_INSP_CONTI_ERR_086,
        E_BALL_INSP_CONTI_ERR_087,
        E_BALL_INSP_CONTI_ERR_088,
        E_BALL_INSP_CONTI_ERR_089,
        E_BALL_INSP_CONTI_ERR_090,
        E_BALL_INSP_CONTI_ERR_091,
        E_BALL_INSP_CONTI_ERR_092,
        E_BALL_INSP_CONTI_ERR_093,
        E_BALL_INSP_CONTI_ERR_094,
        E_BALL_INSP_CONTI_ERR_095,
        E_BALL_INSP_CONTI_ERR_096,
        E_BALL_INSP_CONTI_ERR_097,
        E_BALL_INSP_CONTI_ERR_098,
        E_BALL_INSP_CONTI_ERR_099,
        E_BALL_INSP_CONTI_ERR_100,
#endregion

#region ETC 2000 ~ 4000

#region CONVEYOR 2000 ~ 2099

        // 2000번대 에러 삭제 함 

        // LD CONV
        E_MGZ_LD_CONV_1_LD_MGZ_DETECT_FAIL = 2000,
        E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL,
        E_MGZ_LD_CONV_1_ULD_MGZ_DETECT_FAIL, // 로드 컨베이어에서 매거진 배출 하는 시점에서 센서 체크
        E_MGZ_LD_CONV_1_ULD_MGZ_ARRIVAL_FAIL,
        E_MGZ_BCR_READING_TIME_OUT,
        E_MGZ_BCR_READING_FAIL,

        // ULD CONV
        E_MGZ_ULD_1_CONV_LD_MGZ_DETECT_FAIL,
        E_MGZ_ULD_1_CONV_LD_MGZ_ARRIVAL_FAIL,
        E_MGZ_ULD_1_CONV_ULD_MGZ_DETECT_FAIL,
        E_MGZ_ULD_1_CONV_ULD_MGZ_ARRIVAL_FAIL,


        E_GD_1_TRAY_CONV_IN_DETECT_FAIL,
        E_GD_2_TRAY_CONV_IN_DETECT_FAIL,
        E_RW_TRAY_CONV_IN_DETECT_FAIL,
        E_EMPTY_1_TRAY_CONV_IN_DETECT_FAIL,
        E_EMPTY_2_TRAY_CONV_IN_DETECT_FAIL,


        E_GD_1_TRAY_CONV_OUT_DETECT_FAIL,
        E_GD_2_TRAY_CONV_OUT_DETECT_FAIL,
        E_RW_TRAY_CONV_OUT_DETECT_FAIL,
        E_EMPTY_1_TRAY_CONV_OUT_DETECT_FAIL,
        E_EMPTY_2_TRAY_CONV_OUT_DETECT_FAIL,

        E_GD_1_ELV_RESIDUAL_DETECT_FAIL,
        E_GD_2_ELV_RESIDUAL_DETECT_FAIL,
        E_RW_ELV_RESIDUAL_DETECT_FAIL,
        E_EMPTY_1_ELV_RESIDUAL_DETECT_FAIL,
        E_EMPTY_2_ELV_RESIDUAL_DETECT_FAIL,

        E_MGZ_LD_CONV_LOAD_MGZ_EMPTY,
        E_MGZ_ULD_CONV_UNLOAD_MGZ_FULL,

        //jy.yang LD부 컨베이어 알람 추가
        E_LD_1F_CV_INPUT_STOPPER_UP_FAIL,
        E_LD_1F_CV_INPUT_STOPPER_DOWN_FAIL,
        E_LD_1F_CV_STOPPER_UP_FAIL,
        E_LD_1F_CV_STOPPER_DOWN_FAIL,
        E_LD_2F_CV_STOPPER_UP_FAIL,
        E_LD_2F_CV_STOPPER_DOWN_FAIL,

        // 기존 코드
        //E_CONV_1F_MGZ_NOT_EXIST = 2000,
        //E_CONV_2F_MGZ_NOT_EXIST,
        //E_CONV_3F_MGZ_NOT_EXIST,
        //E_CONV_4F_MGZ_NOT_EXIST,
        //E_CONV_5F_MGZ_NOT_EXIST,
        //E_CONV_1F_MGZ_ALREADY_EXIST,
        //E_CONV_2F_MGZ_ALREADY_EXIST,
        //E_CONV_3F_MGZ_ALREADY_EXIST,
        //E_CONV_4F_MGZ_ALREADY_EXIST,
        //E_CONV_5F_MGZ_ALREADY_EXIST,

        //E_CONV_LD_MGZ_1_NOT_EXIST,
        //E_CONV_LD_MGZ_2_NOT_EXIST,
        //E_CONV_ULD_MGZ_1_ALREADY_EXIST,
        //E_CONV_ULD_MGZ_2_ALREADY_EXIST,

        //E_GD_1_TRAY_CONV_IN_DETECT_FAIL,
        //E_GD_2_TRAY_CONV_IN_DETECT_FAIL,
        //E_RW_TRAY_CONV_IN_DETECT_FAIL,
        //E_EMPTY_1_TRAY_CONV_IN_DETECT_FAIL,
        //E_EMPTY_2_TRAY_CONV_IN_DETECT_FAIL,


        //E_GD_1_TRAY_CONV_OUT_DETECT_FAIL,
        //E_GD_2_TRAY_CONV_OUT_DETECT_FAIL,
        //E_RW_TRAY_CONV_OUT_DETECT_FAIL,
        //E_EMPTY_1_TRAY_CONV_OUT_DETECT_FAIL,
        //E_EMPTY_2_TRAY_CONV_OUT_DETECT_FAIL,

        //E_GD_1_ELV_RESIDUAL_DETECT_FAIL,
        //E_GD_2_ELV_RESIDUAL_DETECT_FAIL,
        //E_RW_ELV_RESIDUAL_DETECT_FAIL,
        //E_EMPTY_1_ELV_RESIDUAL_DETECT_FAIL,
        //E_EMPTY_2_ELV_RESIDUAL_DETECT_FAIL,
#endregion

#region LD ELEVATOR 2100 ~ 2199
        E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST = 2100,
        E_MGZ_LD_ELEVATOR_MGZ_ALREADY_EXIST,
        E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST_POPUP,

        E_MGZ_LD_ELV_CLAMP_FAIL,
        E_MGZ_LD_ELV_UNCLAMP_FAIL,

        E_MGZ_LD_ELV_CONV_LD_MGZ_1_NOT_EXIST, //로드 컨베이어 내 매거진이 존재 하는지 확인
        E_MGZ_LD_ELV_CONV_LD_MGZ_2_NOT_EXIST,
        E_MGZ_LD_ELV_CONV_ULD_MGZ_1_ALREADY_EXIST, // 언로드 컨베이어에 이미 매거진이 있는지 확인 
        E_MGZ_LD_ELV_CONV_ULD_MGZ_2_ALREADY_EXIST,

        E_MGZ_LD_ELV_MGZ_DOOR_LOCK_OPEN_FAIL, // 매거진 공급 위치에서 도어 오픈 확인하는 에러코드


        E_MGZ_LD_ELV_MGZ_SLOT_COUNT_OVER, // 매거진 SLOT PITCH 동작 시 지정 된 카운트를 넘었을 경우 에러 처리
        E_MGZ_LD_ELV_ALL_EXHAUSTED, //매거진 소진 에러 (경알람)
        E_MGZ_LD_ELV_MATERIAL_PROTRUSION_CHECK, //sj.shin 매거진 언로딩 하기 전에 자재 도출 감지 센서 확인 하여 에러 처리

        E_MGZ_LD_ELV_DOOR_OPEN_FAIL,
        E_MGZ_LD_ELV_DOOR_CLOSE_FAIL,

        E_MGZ_LD_ELV_DOOR_NOT_OPEN,

        #endregion

        #region etc 2200 ~ 2299
        E_SCRAP_BOX_IS_NOT_EXIST = 2200,
        E_SCRAP_BOX_IS_FULL,
        E_REJECT_BOX_IS_NOT_EXIST,
        E_REJECT_BOX_IS_FULL,
#endregion

#region MAP PUSHER 2300 ~ 2399

        // 매거진 푸셔 오버로드 센서 체크
        E_MAP_PK_OVERLOAD = 2300, 
        E_MAP_PK_PUSHER_FWD_FAIL,
        E_MAP_PK_PUSHER_BWD_FAIL,


#endregion

#region LD RAIL 2400 ~ 2499
        E_STRIP_LOADING_BOTTOM_BARCODE_READ_FAIL = 2400,
        E_COGNEX_BARCODE_READER_NOT_CONNECTED,
        E_STRIP_LOADING_BOTTOM_BARCODE_TIMEOUT,
        E_LD_RAIL_Y_MATERIAL_SENSOR_NOT_ON,
        E_LD_RAIL_Y_MATERIAL_SENSOR_ALREADY_ON,
        E_PREALIGN_NG,
        E_ORIENT_CHECK_NG,
        E_2D_CODE_READ_FAIL,

        E_LD_MGZ_TO_RAIL_X_MATERIAL_ALREADY_DETECTED, // 매거진과 레일 사이 상단 자재 유무 감지 센서
        E_LD_GRIPPER_X_MATERIAL_DETECT_FAIL,    // 그리퍼 X축 자재 유무 감지 센서 

#endregion

#region STRIP PICKER 2500 ~ 2599
        E_STRIP_GRIPPER_MATERIAL_SENSOR_ON = 2500,

        E_STRIP_TRANSFER_PICKUP_ALREADY_VAC_ON,  // STRIP TRANSFER가 PICKUP 시 이미 자재 VAC이 들어와 있을 경우 에러
        E_STRIP_TRANSFER_LOAD_NO_STRIP_VAC_ON, // 자재 데이터가 없는데 VAC 출력인 상태
        E_STRIP_TRANSFER_LOAD_NO_STRIP_BLOW_ON, // 자재 데이터가 없는데 BLOW 출력인 상태

        E_STRIP_TRANSFER_UNLOAD_NO_STRIP_VAC_ON, // 자재 데이터가 없는데 VAC 출력인 상태
        E_STRIP_TRANSFER_UNLOAD_NO_STRIP_BLOW_ON, // 자재 데이터가 없는데 BLOW 출력인 상태
#endregion

#region UNIT PICKER 2600 ~ 2699
        E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_VAC_ON = 2600, // 자재 데이터가 없는데 VAC 출력인 상태
        E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_BLOW_ON,         // 자재 데이터가 없는데 BLOW 출력인 상태
        E_UNIT_TRANSFER_NO_STRIP_SAW_CUTTING_TABLE_1_REQ_ON,    // 유닛 트랜스퍼에서 SAW 테이블에 요청이 들어왔지만 자재 데이터가 없다
        E_UNIT_TRANSFER_NO_STRIP_SAW_CUTTING_TABLE_2_REQ_ON,    // 유닛 트랜스퍼에서 SAW 테이블에 요청이 들어왔지만 자재 데이터가 없다
#endregion

#region CLEANER 2700 ~ 2799
        E_CLEAN_END = 2700,
#endregion

#region DRY BLOCK STAGE 2800 ~ 2899
#endregion

#region MAP PICKER 2900 ~ 2999
        E_WRONG_POS_DRY_BLOCK_UNLOAD_POS,
#endregion

#region MAP STAGE 3000 ~ 3099
        E_MAP_STAGE_1_BLOW_ON_FAIL = 3000,
        E_MAP_STAGE_2_BLOW_ON_FAIL,
        E_MAP_STAGE_1_BLOW_OFF_FAIL,
        E_MAP_STAGE_2_BLOW_OFF_FAIL,
        E_MAP_STAGE_1_TOP_AIGN_NG,
        E_MAP_STAGE_2_TOP_AIGN_NG,
        E_MAP_STAGE_1_EXIST_SKIP_UNIT,
        E_MAP_STAGE_2_EXIST_SKIP_UNIT,
#endregion

#region MULTI PICKER 3100 ~ 3199
        CHIP_PK1_PAD1_SKIP = 3100,
        CHIP_PK1_PAD2_SKIP,
        CHIP_PK1_PAD3_SKIP,
        CHIP_PK1_PAD4_SKIP,
        CHIP_PK1_PAD5_SKIP,
        CHIP_PK1_PAD6_SKIP,
        CHIP_PK1_PAD7_SKIP,
        CHIP_PK1_PAD8_SKIP,
        CHIP_PK2_PAD1_SKIP,
        CHIP_PK2_PAD2_SKIP,
        CHIP_PK2_PAD3_SKIP,
        CHIP_PK2_PAD4_SKIP,
        CHIP_PK2_PAD5_SKIP,
        CHIP_PK2_PAD6_SKIP,
        CHIP_PK2_PAD7_SKIP,
        CHIP_PK2_PAD8_SKIP,
        #endregion

        #region BALL VISION 3200 ~ 3299
        #endregion

        #region TRAY STAGE 3300 ~ 3399
        E_WRONG_TRAY_STAGE_NUMBER = 3300,
        E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE,
        E_NOT_UP_STATUS_GOOD_TRAY_2_STAGE,
        E_NOT_UP_STATUS_REWORK_TRAY_STAGE,
        E_NOT_DOWN_STATUS_GOOD_TRAY_1_STAGE,
        E_NOT_DOWN_STATUS_GOOD_TRAY_2_STAGE,
        E_NOT_DOWN_STATUS_REWORK_TRAY_STAGE,
        E_WRONG_POS_LOADING_GOOD_TRAY_1_STAGE,
        E_WRONG_POS_LOADING_GOOD_TRAY_2_STAGE,
        E_WRONG_POS_LOADING_REWORK_TRAY_STAGE,
        E_WRONG_POS_UNLOADING_GOOD_TRAY_1_STAGE,
        E_WRONG_POS_UNLOADING_GOOD_TRAY_2_STAGE,
        E_WRONG_POS_UNLOADING_REWORK_TRAY_STAGE,
        E_WRONG_POS_WORKING_P1_GOOD_TRAY_1_STAGE,
        E_WRONG_POS_WORKING_P1_GOOD_TRAY_2_STAGE,
#endregion

#region REJECT BOX 3400 ~ 3499
        E_REJECT_BOX_FWD_FAIL = 3400,
        E_REJECT_BOX_BWD_FAIL,
#endregion

#region TRAY PICKER 3500 ~ 3599
        E_TRAY_PICKER_FRONT_TRAY_CHECK_SENSOR_NOT_ON = 3500,
        E_TRAY_PICKER_REAR_TRAY_CHECK_SENSOR_NOT_ON,
        E_TRAY_PICKER_FRONT_CLAMP_SENSOR_NOT_ON,
        E_TRAY_PICKER_FRONT_UNCLAMP_SENSOR_NOT_ON,
        E_TRAY_PICKER_REAR_CLAMP_SENSOR_NOT_ON,
        E_TRAY_PICKER_REAR_UNCLAMP_SENSOR_NOT_ON,
        E_TRAY_PICKER_FRONT_TRAY_CHECK_SENSOR_NOT_OFF,
        E_TRAY_PICKER_REAR_TRAY_CHECK_SENSOR_NOT_OFF,
        E_TRAY_PICKER_FRONT_CLAMP_SENSOR_NOT_OFF,
        E_TRAY_PICKER_FRONT_UNCLAMP_SENSOR_NOT_OFF,
        E_TRAY_PICKER_REAR_CLAMP_SENSOR_NOT_OFF,
        E_TRAY_PICKER_REAR_UNCLAMP_SENSOR_NOT_OFF,
        E_TRAY_RFID_READ_TIMEOUT,
        E_TRAY_RFID_READ_FAIL,
        E_WRONG_POS_TRAY_PICKER_UNLOAD_TO_GOOD_TRAY_STAGE_1,
        E_WRONG_POS_TRAY_PICKER_UNLOAD_TO_GOOD_TRAY_STAGE_2,
        E_WRONG_POS_TRAY_PICKER_UNLOAD_TO_RW_TRAY_STAGE,
        E_WRONG_POS_TRAY_PICKER_LOAD_TO_GOOD_TRAY_STAGE_1,
        E_WRONG_POS_TRAY_PICKER_LOAD_TO_GOOD_TRAY_STAGE_2,
        E_WRONG_POS_TRAY_PICKER_LOAD_TO_RW_TRAY_STAGE,
#endregion

#region TRAY ELEVATOR 3600 ~ 3699
        E_GOOD_TRAY_ELEVATOR_Z_VS_OHT_DANGER_POSITION = 3600,
        E_REWORK_TRAY_ELEVATOR_Z_VS_OHT_DANGER_POSITION,
        E_EMPTY1_TRAY_ELEVATOR_Z_VS_OHT_DANGER_POSITION,
        E_EMPTY2_TRAY_ELEVATOR_Z_VS_OHT_DANGER_POSITION,
        E_COVER_TRAY_ELEVATOR_Z_VS_OHT_DANGER_POSITION,
        E_GOOD_TRAY_ELEVATOR_GUIDE_FWD_CAN_NOT_CALL_OHT,
        E_REWORK_TRAY_ELEVATOR_GUIDE_FWD_CAN_NOT_CALL_OHT,
        E_EMPTY1_TRAY_ELEVATOR_GUIDE_FWD_CAN_NOT_CALL_OHT,
        E_EMPTY2_TRAY_ELEVATOR_GUIDE_FWD_CAN_NOT_CALL_OHT,
        E_COVER_TRAY_ELEVATOR_GUIDE_FWD_CAN_NOT_CALL_OHT,
        E_GOOD_1_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_OFF,
        E_GOOD_2_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_OFF,
        E_REWORK_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_OFF,
        E_EMPTY1_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_OFF,
        E_EMPTY2_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_OFF,
        E_GOOD_1_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_ON,
        E_GOOD_2_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_ON,
        E_REWORK_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_ON,
        E_EMPTY1_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_ON,
        E_EMPTY2_TRAY_ELEVATOR_TRAY_RESIDUE_CHECK_SENSOR_NOT_ON,
        E_GOOD1_ELEVATOR_TRAY_QTY_OVER,//220608 pjh 수량 over되어 이미 시그널 써치 센서가 감지되어있는 경우 알람
        E_GOOD2_ELEVATOR_TRAY_QTY_OVER,//220608 pjh 수량 over되어 이미 시그널 써치 센서가 감지되어있는 경우 알람
        E_REWORK_ELEVATOR_TRAY_QTY_OVER,
        E_EMPTY1_ELEVATOR_TRAY_QTY_OVER,
        E_EMPTY2_ELEVATOR_TRAY_QTY_OVER,

        E_GOOD1_TRAY_CONV_ALREADY_EXIST_TRAY,//220608 pjh 메뉴얼동작 시 에만 사용
        E_GOOD2_TRAY_CONV_ALREADY_EXIST_TRAY,//220608 pjh 메뉴얼동작 시 에만 사용
        E_REWORK_TRAY_CONV_ALREADY_EXIST_TRAY,
        E_EMPTY1_TRAY_CONV_ALREADY_EXIST_TRAY,
        E_EMPTY2_TRAY_CONV_ALREADY_EXIST_TRAY,

        E_GOOD1_ELV_STACKER_ALREADY_EXIST_TRAY,//220610 스테커에 이미 트레이가 있다
        E_GOOD2_ELV_STACKER_ALREADY_EXIST_TRAY,
        E_REWORK_ELV_STACKER_ALREADY_EXIST_TRAY,
        E_EMPTY1_ELV_STACKER_ALREADY_EXIST_TRAY,
        E_EMPTY2_ELV_STACKER_ALREADY_EXIST_TRAY,
#endregion

#region ULD OHT 3700 ~ 3799
        //220608 사용 안해서 삭제
#endregion

#region VAC n DATA MISMATCH 3800 ~ 3899
        E_LOADING_RAIL_VAC_N_DATA_MISMATCH = 3800,//220513 pjh
        E_STRIP_PICKER_VAC_N_DATA_MISMATCH,
        E_UNIT_PICKER_VAC_N_DATA_MISMATCH,
        E_CLEAN_FLIP_1_VAC_N_DATA_MISMATCH,
        E_CLEAN_FLIP_2_VAC_N_DATA_MISMATCH,
        E_CLEAN_PICKER_VAC_N_DATA_MISMATCH,
        E_DRY_BLOCK_VAC_N_DATA_MISMATCH,
        E_MAP_PICKER_VAC_N_DATA_MISMATCH,
        E_MAP_STAGE_1_VAC_N_DATA_MISMATCH,
        E_MAP_STAGE_2_VAC_N_DATA_MISMATCH,
#endregion
#endregion

#region DEVICE ERR #3900 ~ 3999
        E_TEMP_CONTROLLER_OVERHEAT = 3900,
        //키엔스 센서 에러
        E_GD_TRAY_STAGE_1_PLACE_MISS,
        E_GD_TRAY_STAGE_2_PLACE_MISS,
#endregion

#region INTERLOCK #4000 ~ 6000
#region CONVEYOR 4000 ~ 4099



#endregion

#region ULD ELEVATOR 4100 ~ 4199
        E_INTL_MGZ_ULD_ELV_X_CAN_NOT_MOVE_MGZ_ULD_ELV_Y_NOT_READY_POS = 4100,
        E_INTL_MGZ_ULD_ELV_Z_CAN_NOT_MOVE_MGZ_ULD_ELV_Y_NOT_READY_POS,

#endregion

#region LD ELEVATOR 4200 ~ 4299
        E_INTL_MGZ_CLAMP_ELV_Y_CAN_NOT_MOVE_PUSHER_FWD = 4200,
        E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_MOVE_NOT_SAFETY_Z_POS,
        E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_MOVE_MATERIAL_DETECTED,
        E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_MGZ_EXIST,
        E_INTL_MGZ_CLAMP_ELV_Y_CAN_NOT_HOME_PUSHER_FWD,
        E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_PUSHER_FWD,
        E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_IS_NOT_UNCLAMP,
        E_INTL_MGZ_CLAMP_ELV_X_CAN_NOT_MOVE_PUSHER_FWD,
        E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_MATERIAL_DETECTED,
#endregion

#region MAP PUSHER 4300 ~ 4399
        E_INTL_MAP_PUSHER_CAN_NOT_MOVE_MGZ_CLAMP_ELV_Y_NOT_READY_POS = 4300,
        E_INTL_MAP_PUSHER_CAN_NOT_PUSH_STRIP_PICKER_GRIPPER_DOWN,
        E_INTL_MAP_PUSHER_OVERLOAD_SENSOR_DETECTED,
        E_INTL_MAP_PUSHER_AXIS_IS_NOT_SAFE_POSITION,
        E_INTL_MAP_PUSHER_CYLINDER_IS_FWD_POSITION,
#endregion
        
#region LD RAIL 4400 ~ 4499
        E_INTL_LD_RAIL_T_CAN_NOT_MOVE_MAP_PUSHER_FWD_POS = 4400,
        E_INTL_LD_RAIL_T_CAN_NOT_MOVE_STRIP_PK_Z_NOT_READY_POS,
        E_INTL_LD_RAIL_Y_CAN_NOT_MOVE_MAP_PUSHER_FWD_POS,
        E_INTL_LD_RAIL_Y_CAN_NOT_MOVE_STRIP_PK_Z_NOT_READY_POS,
        E_INTL_LD_RAIL_T_CAN_NOT_HOME_STRIP_PK_Z_NOT_READY_POS,
        E_INTL_LD_RAIL_GRIP_X_CAN_NOT_MOVE_STRIP_PK_Z_NOT_READY_POS, 
        E_INTL_LD_GRIP_MATERIAL_DETECT_ALARM,
#endregion

#region STRIP PICKER 4500 ~ 4599
        E_INTL_LD_VISION_X_CAN_NOT_MOVE_VISION_GRIP_DOWN = 4500,
        E_INTL_STRIP_PK_X_CAN_NOT_MOVE_STRIP_PK_Z_NOT_READY_POS,
        E_INTL_STRIP_PK_X_CAN_NOT_MOVE_VISION_GRIP_DOWN,
        E_INTL_STRIP_PK_Z_CAN_NOT_MOVE_VISION_GRIP_DOWN,
        E_INTL_STRIP_PK_X_CAN_NOT_HOME_LD_VISION_Y_NOT_SAFE_POS,
        E_INTL_STRIP_PK_X_CAN_NOT_MOVE_UNIT_PK_X_NOT_READY_POS,
        E_INTL_STRIP_PK_Z_CAN_NOT_MOVE_STRIP_PK_X_IS_NOT_GRIP_POS,
        E_INTL_STRIP_PK_Z_CAN_NOT_MOVE_STRIP_PK_X_IS_NOT_GRIP_SAFETY_POS,
        E_INTL_STRIP_PK_Z_CAN_NOT_MOVE_STRIP_PK_X_IS_NOT_UNGRIP_SAFETY_POS,
        E_INTL_STRIP_PK_Z_CAN_NOT_MOVE_GRIPPER_X_IS_NOT_SAFETY_POS, // Z축 이동 전 GRIPPER X 축 안전 위치 확인
        E_INTL_STRIP_PK_X_CAN_NOT_MOVE_GRIPPER_DOWN,
#endregion

#region UNIT PICKER 4600 ~ 4699
        E_INTL_UNIT_PK_X_CAN_NOT_MOVE_UNIT_PK_Z_NOT_READY_POS = 4600,
        E_INTL_UNIT_PK_X_VS_MAP_PK_X_SAFETY_SENSOR_DETECTED,
        E_INTL_UNIT_PK_X_CAN_NOT_HOME_MAP_PK_X_POS_NOT_SAFE,
        E_INTL_UNIT_PK_X_CAN_NOT_HOME_CELAN_Z_POS_TOO_HIGH,
        E_INTL_UNIT_PK_X_IS_NOT_SPONGE_POSITION,
        E_INTL_UNIT_PK_X_IS_NOT_CLEANING_POSITION,
        E_INTL_UNIT_PK_X_CAN_NOT_MOVE_STRIP_PK_X_NOT_READY_POS,
        E_INTL_UNIT_PK_X_CAN_NOT_MOVE_CLEAN_Z_HIGH_POS,
        E_INTL_UNIT_PK_X_IS_NOT_ULTRASONIC_POSITION,
        E_INTL_UNIT_PK_X_IS_NOT_UNLOAD_POSITION,
        E_INTL_UNIT_PK_X_IS_NOT_SCRAP_1_POSITION,
        E_INTL_UNIT_PK_X_IS_NOT_SCRAP_2_POSITION,
        E_INTL_UNIT_PK_Z_IS_NOT_READY_POSITION,
        E_INTL_UNIT_PK_Z_IS_NOT_SPONGE_POSITION,
        E_INTL_UNIT_PK_X_CAN_NOT_MOVE_CLEAN_PK_IS_NOT_SAFETY_POSITION,
        E_INTL_THERE_IS_NO_UNIT_PK_STRIP_INFO,
        E_INTL_UNIT_PK_X_CAN_NOT_MOVE_UNIT_PK_Z_IS_ULTRASONIC_POS,
#endregion

#region CLEANER 4700 ~ 4799
        E_INTL_CLEANER_Z_CAN_NOT_MOVE_UNIT_PK_Z_NOT_READY_POS = 4700,
        E_INTL_CLEANER_Z_CAN_NOT_MOVE_DRY_STAGE_X_NOT_READY_POS,
        E_INTL_CLEANER_R_CAN_NOT_MOVE_DRY_STAGE_X_NOT_READY_POS,
        E_INTL_CLEANER_Z_CAN_NOT_HOME_DRY_BLOCK_STG_X_POS_NOT_SAFE,
        E_INTL_CLEANER_Z_CAN_NOT_UP_UNIT_PK_X_IS_NOT_SAFETY_POS,
        E_INTL_CLEANER_CAN_NOT_RUN_CYCLE_CLEANER_IS_NOT_DRY_POS,
        E_INTL_CLEANER_Z_CAN_NOT_HOME_CELAN_R_IS_NOT_SAFE,
        E_INTL_CLEANER_R_CAN_NOT_HOME_CLEAN_Z_POS_TOO_LOW,
        E_INTL_CLEANER_Z_CAN_NOT_MOVE_DRY_STAGE_X_NOT_SAFETY_POS,
        E_INTL_CLEANER_FLIP_T1_CYLINDER_COVER_CLOSE,
        E_INTL_CLEANER_FLIP_T2_FLIP_Z_IS_NOT_LOAD_POSITION,
        E_INTL_CLEANER_FLIP_Z_FLIP_T2_IS_NOT_WORK_POSITION,
        E_INTL_CLEANER_PICKER_X_CAN_NOT_MOVE_CLEANER_PICKER_Z_NOT_SAFETY_POS,
        E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_CLEANER_PICKER_X_NOT_SECOND_CLEAN_POS,
        E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_CLEANER_PICKER_X_NOT_SECOND_ULTRA_POS,
        E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_CLEANER_PICKER_X_NOT_SECOND_PLASMA_POS,
        E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_CLEANER_PICKER_X_NOT_DRY_POS,
        E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_MAP_PICKER_X_LESS_READY_POS,
        E_INTL_CLEANER_PICKER_X_CAN_NOT_MOVE_DRY_Y_COLLISION_POSITION,
        E_INTL_DRY_X_CAN_NOT_MOVE_CLEANER_X_COLLISION_POSITION,
        E_INTL_CLEANER_FLIP_T1_CAN_NOT_TURN_PICKER_INTERLOCK,
        E_INTL_CLEANER_FLIP_T1_IS_NOT_LOAD_POS,
        E_INTL_CLEANER_PICKER_X_CAN_NOT_MOVE_DRY_PLASMA_Z_NOT_SAFETY_POS,
        E_INTL_CLEANER_PICKER_X_CAN_NOT_MOVE_CLEANER_PICKER_R_NOT_SAFETY_POS,
        E_INTL_CLEANER_PICKER_X_CAN_NOT_MOVE_DRY_PLASMA_Y_NOT_SAFETY_POS,
        E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_CLEANER_PICKER_X_NOT_UNLOAD_DRY_POS,
        E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_CLEANER_PICKER_X_NOT_SAFETY_POS,
        E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_CLEANER_PICKER_Z_NOT_SAFETY_POS,
        E_INTL_CLEANER_FLIP_T2_SWING_POSITION_OVER,
        E_INTL_CLEANER_FLIP_T2_CAN_NOT_TURN_PICKER_INTERLOCK,
        E_INTL_THERE_IS_NO_SECOND_CLEAN_STRIP_INFO,
        E_INTL_THERE_IS_NO_SECOND_ULTRA_STRIP_INFO,
        E_INTL_THERE_IS_NO_CLEANER_PK_STRIP_INFO,
        E_INTL_AIR_KNIFE_CYLINDER_UP_FAIL,
        E_INTL_AIR_KNIFE_CYLINDER_DOWN_FAIL,
#endregion

#region DRY BLOCK STAGE 4800 ~ 4899
        E_INTL_DRY_BLOCK_STG_X_CAN_NOT_MOVE_UNIT_PK_DOWN = 4800,
        E_INTL_DRY_BLOCK_STG_X_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS,
        E_INTL_THERE_IS_NO_DRY_BLOCK_STRIP_INFO,
#endregion

#region MAP PICKER 4900 ~ 4999
        E_INTL_MAP_PK_Z_CAN_NOT_MOVE_DRY_BLOCK_STG_X_NOT_UNLOADING_POS = 4900,
        E_INTL_MAP_PK_Z_CAN_NOT_MOVE_MAP_STG_1_Y_NOT_LOADING_POS,
        E_INTL_MAP_PK_Z_CAN_NOT_MOVE_MAP_STG_2_Y_NOT_LOADING_POS,
        E_INTL_MAP_PK_X_CAN_NOT_MOVE_MAP_PK_Z_DOWN_POS,
        E_INTL_MAP_PK_X_CAN_NOT_HOME_UNIT_PK_X_NOT_AVOID_POS,
		E_INTL_MAP_PK_X_CAN_NOT_MOVE_UNIT_PK_X_NOT_SAFETY_POS,
        E_INTL_MAP_PK_X_CAN_NOT_MOVE_TRAY_PK_X_NOT_SAFETY_POS,
        E_INTL_MAP_PK_X_CAN_NOT_MOVE_CLEAN_PICKER_XZ_UNLOADING_POS,
#endregion

#region MAP STAGE 5000 ~ 5099
        E_INTL_MAP_STG_1_Y_CAN_NOT_MOVE_MAP_PK_Z_DOWN_POS = 5000,
        E_INTL_MAP_STG_2_Y_CAN_NOT_MOVE_MAP_PK_Z_DOWN_POS,
        E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS,
        E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS,
        E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS,
        E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS,
        E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS,
        E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS,
        E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS,
        E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS,
#endregion

#region MULTI PICKER 5100 ~ 5199
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS = 5100,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_2_IS_DOWN_POS,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_3_IS_DOWN_POS,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_4_IS_DOWN_POS,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_5_IS_DOWN_POS,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_6_IS_DOWN_POS,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_7_IS_DOWN_POS,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_8_IS_DOWN_POS,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_9_IS_DOWN_POS,
        E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_10_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_2_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_3_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_4_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_5_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_6_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_7_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_8_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_9_IS_DOWN_POS,
        E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_10_IS_DOWN_POS,

        E_INTL_MULTI_PK_1_Z_Down,
        E_INTL_MULTI_PK_2_Z_Down,
#endregion

        #region BALL VISION 5200 ~ 5299
        E_INTL_BALL_VISION_CYL_FWD_BWD = 5200,
        E_INTL_BALL_VISION_Y_CAN_NOT_HOME_BALL_VISION_Z_NOT_READY_POS,
        E_INTL_VISION_Y_CAN_NOT_MOVE_BALL_VISION_ALIGN_FWD,
        E_INTL_VISION_Z_CAN_NOT_MOVE_BALL_VISION_ALIGN_FWD,
#endregion

#region TRAY STAGE 5300 ~ 5399
        E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER = 5300,
        E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS,
        E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER,
        E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS,
        E_INTL_RW_TRAY_STG_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS,
        E_INTL_GD_TRAY_STAGE_1_CYL_CAN_NOT_UP,
        E_INTL_GD_TRAY_STAGE_1_CYL_CAN_NOT_DOWN,
        E_INTL_GD_TRAY_STAGE_2_CYL_CAN_NOT_UP,
        E_INTL_GD_TRAY_STAGE_2_CYL_CAN_NOT_DOWN,
        E_INTL_GD_TRAY_STAGE_CAN_NOT_CYLINDER_UP_DOWN,
        E_INTL_GD_TRAY_STAGE_1_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS,
        E_INTL_GD_TRAY_STAGE_2_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS,
        E_INTL_RW_TRAY_STAGE_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS,
        E_INTL_GD_TRAY_STG_1_OFF_UP_DOWN_OUTPUT,
        E_INTL_GD_TRAY_STG_2_OFF_UP_DOWN_OUTPUT,

        E_TRAY_BOTH_SORTING,
#endregion

        #region REJECT BOX 5400 ~ 5499
        E_INTL_REJECT_BOX_Y_CAN_NOT_MOVE_REJECT_BOX_FWD = 5400,
#endregion

#region TRAY PICKER 5500 ~ 5599
        E_INTL_TRAY_PK_Z_CAN_NOT_MOVE_GD_TRAY_STG_1_Y_NOT_UNLOADING_POS = 5500,
        E_INTL_TRAY_PK_Z_CAN_NOT_MOVE_GD_TRAY_STG_2_Y_NOT_UNLOADING_POS,
        E_INTL_TRAY_PK_Z_CAN_NOT_MOVE_RW_TRAY_STG_Y_NOT_UNLOADING_POS,
        E_INTL_TRAY_PK_X_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS,
        E_INTL_TRAY_PK_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS,
        E_INTL_TRAY_PK_X_CAN_NOT_HOME_TRAY_PK_Z_DOWN_POS,
        E_INTL_TRAY_PK_Y_CAN_NOT_HOME_TRAY_PK_Z_DOWN_POS,
#endregion

#region TRAY ELEVATOR 5600 ~ 5699
#endregion

#region ULD OHT 5700 ~ 5799
#endregion
#endregion

        ERR_CNT,
        SAW_ALARM_OFFSET = 6000,
        VISION_ALARM_OFFSET = 8000,
    }

    [Serializable]
    public class ErrorInfo
    {
        //public ErrorInfo(int nErrNo, string strCause, string strSolution, bool bHeavy, string strBuzzerNum)
        //{
        //    this.nErrNo = nErrNo;
        //    this.strCause = strCause;
        //    this.strSolution = strSolution;
        //    this.bHeavy = bHeavy;
        //    this.strBuzzerNum = strBuzzerNum;
        //}

        private int nErrNo = 0;
        private string strName = "Name";
        private string strCause = "Cause";
        private string strSolution = "Solution";
        private bool bHeavy = true;
        private string strBuzzerNum;

        public enum BUZZERLIST
        {
            BUZZER_ERR,
            BUZZER_END,
        }

        [CategoryAttribute("OPTION"), ReadOnlyAttribute(true), Description("에러 번호입니다")]
        public int NO
        {
            get { return nErrNo; }
            set { nErrNo = value; }
        }

        [CategoryAttribute("OPTION"), ReadOnlyAttribute(true), Description("에러 이름입니다.")]
        public string NAME
        {
            get { return strName; }
            set { strName = value; }
        }

        [CategoryAttribute("OPTION"), Description("발생 원인을 입력합니다")]
        public string CAUSE
        {
            get { return strCause; }
            set { strCause = value; }
        }
        [CategoryAttribute("OPTION"), Description("조치방법을 입력합니다")]
        public string SOLUTION
        {
            get { return strSolution; }
            set { strSolution = value; }
        }
        [CategoryAttribute("OPTION"), Description("중알람으로 사용할 경우 체크합니다")]
        public bool HEAVY
        {
            get { return bHeavy; }
            set { bHeavy = value; }
        }
        [CategoryAttribute("OPTION"), Description("사용할 부저를 선택합니다.")]
        [DefaultValue(BUZZERLIST.BUZZER_ERR)]
        [TypeConverter(typeof(FormatStringConverter))]
        public string BUZZER
        {
            get { return strBuzzerNum; }
            set { strBuzzerNum = value; }
        }

        public class FormatStringConverter : StringConverter
        {
            public override Boolean GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
            public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<String> list = new List<String>();
                foreach (BUZZERLIST item in Enum.GetValues(typeof(BUZZERLIST)))
                {
                    list.Add(item.ToString());
                }
                return new StandardValuesCollection(list);
            }
        }
    }
#endregion

#region WNDF
    public class WNDF
    {
        public const int LIGHT_CURTAIN = 0;
        public const int BIN_BOX_FULL = 1;

        public WNDF()
        {

        }
    }
#endregion

#region TTDF
    public class TTDF
    {
        public enum CYCLE_NAME
        {
#region MgzLdUldConv
            MGZ_LD_CONV_LOADING_MOVE = 0,
            MGZ_LD_CONV_UNLOADING_MOVE,
            MGZ_ULD_CONV_UNLOADING_MOVE,
#endregion

#region LdMzTransfer
            MGZ_LOADING,
            MGZ_SUPPLY,
            MGZ_UNLOADING,
            MGZ_CLIP_LOCK_CHECK,
            MGZ_LOADING_REQ,
            MGZ_LOADING_WAIT,
            MGZ_UNLOADING_REQ,
            MGZ_UNLOADING_WAIT,
#endregion

#region SawStripTransfer
            STRIP_PUSH_AND_GRIP,
            STRIP_BOTTOM_BARCODE,
            STRIP_RAIL_UNGRIP,
            STRIP_RAIL_LOADING,
            STRIP_PRESS_AND_VACUUM,
            STRIP_PREALIGN,
            STRIP_READ_BARCODE,
            LOADING_RAIL_STRIP,
            UNLOADING_CUTTING_TABLE,
            RELOADING_CUTTING_TABLE,
#endregion

#region FIRST CLEAN & ULTRASONIC
            FIRST_CLEAN,
            SPONGE_CLEAN,
            FIRST_ULTRASONIC,
            SCRAP_1,
            SCRAP_2,
            FIRST_UNLOAD,
#endregion

#region SECOND CLEAN & ULTRASONIC
            SECOND_CLEAN,
            SECOND_CLEAN_DRY,
            SECOND_CLEAN_PICK_UP,
            SECOND_CLEAN_BOTTOM_DRY,
            SECOND_ULTRASONIC_PLACE,
            SECOND_ULTRASONIC,
            SECOND_ULTRASONIC_DRY,
            SECOND_ULTRASONIC_BOTTOM_DRY,
#endregion

#region PLASMA & UNLOAD DRY BLOCK
            CLEANER_PICKER_PLASMA,
            FIRST_PLASMA,
            SECOND_PLASMA,
            UNLOAD_DRY_BLOCK,
#endregion

#region MapTransfer
            LOAD_DRY_BLOCK,
            UNLOAD_MAP_TABLE,
            MAP_VISION_INSP,
#endregion

#region UnitPnP1
            FORWARD_DIR_PICK_UP_PICKER_1,
            REVERSE_DIR_PICK_UP_PICKER_1,
            FORWARD_PLACE_GOOD_TRAY_PICKER_1,
            REVERSE_PLACE_GOOD_TRAY_PICKER_1,
            PLACE_REWORK_TRAY_PICKER_1,
            PLACE_REJECT_BOX_PICKER_1,
            BTM_INSP_PICKER_1,
            PICK_UP_ONE_CYCLE_PICKER_1,
            PLACE_ONE_CYCLE_PICKER_1,
            BALL_VISION_ONE_CYCLE_PICKER_1,
#endregion

#region UnitPnP2
            FORWARD_DIR_PICK_UP_PICKER_2,
            REVERSE_DIR_PICK_UP_PICKER_2,
            FORWARD_PLACE_GOOD_TRAY_PICKER_2,
            REVERSE_PLACE_GOOD_TRAY_PICKER_2,
            PLACE_REWORK_TRAY_PICKER_2,
            PLACE_REJECT_BOX_PICKER_2,
            BTM_INSP_PICKER_2,
            PICK_UP_ONE_CYCLE_PICKER_2,
            PLACE_ONE_CYCLE_PICKER_2,
            BALL_VISION_ONE_CYCLE_PICKER_2,
#endregion

            TRAY_VISION_INSP,
            MAX,
        }

        ////LOADER
        //public const int MAGAZINE_LOADING = 0;
        //public const int MAGAZINE_STRIP_SUPPLY = 1;
        //public const int MAGAZINE_UNLOADING = 2;

        ////SAW
        //public const int STRIP_PUSH_AND_LOADING = 10;
        //public const int STRIP_LOADING_TO_LEFT_TABLE = 11;
        //public const int STRIP_LOADING_TO_RIGHT_TABLE = 12;
        //public const int STRIP_CUTTING_LF = 13;
        //public const int STRIP_CUTTING_RT = 14;
        //public const int STRIP_UNLOADING_TO_CLEAN_TABLE = 15;

        ////SORTER
        //public const int STRIP_MARK_VISION_LEFT_TABLE = 20;
        //public const int STRIP_MARK_VISION_RIGHT_TABLE = 21;
        //public const int STRIP_PICK_AND_PLACE_LEFT_TABLE = 22;
        //public const int STRIP_PICK_AND_PLACE_RIGHT_TABLE = 23;

        ////UNLOADER
        //public const int TRAY_LOADING_GOOD_1 = 30;
        //public const int TRAY_LOADING_GOOD_2 = 31;
        //public const int TRAY_LOADING_REWORK = 32;
        //public const int TRAY_UNLOADING_GOOD_1 = 33;
        //public const int TRAY_UNLOADING_GOOD_2 = 34;
        //public const int TRAY_UNLOADING_REWORK = 35;

        //public const int MAX = 40;

        //public static DateTime[] dtStartTime = new DateTime[TTDF.MAX];
        //public static DateTime[] dtEndTime = new DateTime[TTDF.MAX];
        //public static bool[] bIsStarted = new bool[TTDF.MAX];
        //public static string[] strTactTime = new string[TTDF.MAX];

        public static DateTime[] dtStartTime = new DateTime[(int)TTDF.CYCLE_NAME.MAX];
        public static DateTime[] dtEndTime = new DateTime[(int)TTDF.CYCLE_NAME.MAX];
        public static bool[] bIsStarted = new bool[(int)TTDF.CYCLE_NAME.MAX];
        public static string[] strTactTime = new string[(int)TTDF.CYCLE_NAME.MAX];

        //public static string GetName(int nProcNo)
        //{
        //    switch (nProcNo)
        //    {
        //        case TTDF.MAGAZINE_LOADING: return "MAGAZINE LOADING";
        //        case TTDF.MAGAZINE_STRIP_SUPPLY: return "MAGAZINE STRIP SUPPLY";
        //        case TTDF.MAGAZINE_UNLOADING: return "MAGAZINE UNLOADING";

        //        case TTDF.STRIP_PUSH_AND_LOADING: return "STRIP PUSH AND LOADING";
        //        case TTDF.STRIP_LOADING_TO_LEFT_TABLE: return "STRIP LOADING TO LEFT TABLE";
        //        case TTDF.STRIP_LOADING_TO_RIGHT_TABLE: return "STRIP LOADING TO RIGHT TABLE";
        //        case TTDF.STRIP_CUTTING_LF: return "STRIP CUTTING LEFT TABLE";
        //        case TTDF.STRIP_CUTTING_RT: return "STRIP CUTTING RIGHT TABLE";
        //        case TTDF.STRIP_UNLOADING_TO_CLEAN_TABLE: return "STRIP UNLOAD TO CLEANING TABLE";

        //        case TTDF.STRIP_MARK_VISION_LEFT_TABLE: return "STRIP MARK VISION LEFT TABLE";
        //        case TTDF.STRIP_MARK_VISION_RIGHT_TABLE: return "STRIP MARK VISION RIGHT TABLE";
        //        case TTDF.STRIP_PICK_AND_PLACE_LEFT_TABLE: return "STRIP PICK AND PLACE LEFT TABLE";
        //        case TTDF.STRIP_PICK_AND_PLACE_RIGHT_TABLE: return "STRIP PICK AND PLACE RIGHT TABLE";

        //        case TTDF.TRAY_LOADING_GOOD_1: return "TRAY LOADING GOOD 1";
        //        case TTDF.TRAY_LOADING_GOOD_2: return "TRAY LOADING GOOD 2";
        //        case TTDF.TRAY_LOADING_REWORK: return "TRAY LOADING REWORK";
        //        case TTDF.TRAY_UNLOADING_GOOD_1: return "TRAY UNLOADING GOOD 1";
        //        case TTDF.TRAY_UNLOADING_GOOD_2: return "TRAY UNLOADING GOOD 2";
        //        case TTDF.TRAY_UNLOADING_REWORK: return "TRAY UNLOADING REWORK";
        //        default: return "";
        //    }
        //}

        /// <summary>
        /// 택타임 측정 시작 또는 종료
        /// </summary>
        /// <param name="nTTDF">싸이클 인덱스</param>
        /// <param name="bIsStart">시작이면 트루</param>
        public static void SetTact(int nTTDF, bool bIsStart)
        {
            if (bIsStart)
            {
                TTDF.bIsStarted[nTTDF] = true;
                TTDF.dtStartTime[nTTDF] = DateTime.Now;
            }
            else
            {
                if (!TTDF.bIsStarted[nTTDF]) return;
                TTDF.bIsStarted[nTTDF] = false;
                TTDF.dtEndTime[nTTDF] = DateTime.Now;

                TTDF.strTactTime[nTTDF] = string.Format("{0}:{1}:{2}.{3}",
                                    (TTDF.dtEndTime[nTTDF] - TTDF.dtStartTime[nTTDF]).Hours.ToString("00"),
                                    (TTDF.dtEndTime[nTTDF] - TTDF.dtStartTime[nTTDF]).Minutes.ToString("00"),
                                    (TTDF.dtEndTime[nTTDF] - TTDF.dtStartTime[nTTDF]).Seconds.ToString("00"),
                                    (TTDF.dtEndTime[nTTDF] - TTDF.dtStartTime[nTTDF]).Milliseconds.ToString("000"));

                //GbFunc.TACT_TIME_LogWrite(TTDF.GetName(nTTDF), TTDF.dtStartTime[nTTDF], TTDF.dtEndTime[nTTDF]);
            }
        }

        public static void InitTact()
        {
            DateTime dtInit = DateTime.Now;
            for (int i = 0; i < (int)TTDF.CYCLE_NAME.MAX; i++)
            {
                TTDF.dtStartTime[i] = dtInit;
                TTDF.dtEndTime[i] = dtInit;
                bIsStarted[i] = false;
                strTactTime[i] = "00:00:00.000";
            }
        }
    }
#endregion

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MOTINFO
    {
        public int nAxis;
        public double dPos;
        public double dVel;
        public double dAcc;
        public double dDec;
        public int nAmpErrNo;
        public int nFollowingErrNo;
        public int nMoveErrNo;
        public int nHomeErrNo;
        public int nSwLimitErrNoP;
        public int nSwLimitErrNoN;
        public int nMotionInfoFaultErrNo;
        public bool isUseInPos;
        public double dInposGap;
        public long lMoveTime;
        public double dJogLow;
        public double dJogMiddle;
        public double dJogHigh;

        public double TranslateMsToMmPerSec(double dMs)
        {
            double dValue = dVel / dMs;
            return dValue;
        }

        public double TranslateMsToMmPerSecAcc()
        {
            double dValue = dVel / dAcc;
            return dVel / dValue;
        }

        public double TranslateMsToMmPerSecDec()
        {
            double dValue = dVel / dDec;
            return dVel / dValue;
        }
    }

    public static class CFG_DF
    {

        public const int HEAD_1 = 0;
        public const int HEAD_2 = 1;
        public const int MAX_PICKER_HEAD_CNT = 2;
        public const int MAX_PICKER_PAD_CNT = 8;

        public const int MAX_VISION_TABLE_CNT = 2;

        public const int MAX_ULD_GOOD_TRAY_CNT = 2;

        public const int MAX_ULD_TRAY_ELV_EMPTY_CNT = 2;

        public const int MAP_STG_1 = 0;
        public const int MAP_STG_2 = 1;
        public const int MAP_STG_CNT = 2;

        public const int TRAY_STG_GD1 = 0;
        public const int TRAY_STG_GD2 = 1;
        public const int TRAY_STG_RWK = 2;
        public const int TRAY_STG_RJT = 3;
        public const int TRAY_STG_CNT = 4;

        public const int TRAY_ELV_GD1 = 0;
        public const int TRAY_ELV_GD2 = 1;
        public const int TRAY_ELV_RW = 2;
        public const int TRAY_ELV_EMPTY2 = 3;
        public const int TRAY_ELV_EMPTY1 = 4;

        public const int DEGREE_0 = 0;
        public const int DEGREE_P90 = 1;
        public const int DEGREE_P180 = 2;
        public const int DEGREE_P270 = 3;
		public const int DEGREE_N90 = 4;
        public const int DEGREE_N180 = 5;
        public const int DEGREE_N270 = 6;

        public const int MAX_DEGREE_NO = 7; //0: 0º,1: 90º,2: 180º,3: 270º
    }

    public static class VSDF
    {
        public enum VISION_INTERFACE
        {
            LOAD_PARAMETER_REQ_ON,
            LOAD_PARAMETER_REQ_OFF,
            SAVE_PARAMETER_REQ_ON,
            SAVE_PARAMETER_REQ_OFF,
            SEARCH_TRIGGER_ON,
            SEARCH_TRIGGER_OFF,
            SEARCH_JUDGE_ON,
            SEARCH_JUDGE_OFF,
        }
		
        public enum eTRIGGER
        {
            PRE_ALIGN = 0,
            MAP,
            BALL,
            HEAD1,
            HEAD2,
            TRAY,
        }


        public enum ePRE_INSP_MODE
        {
            ALIGN1 = 1,
            ALIGN2,
            //ORIENTATION,
            CODE,//2D_CODE
        }

        public enum eTOP_ALIGN_MODE
        {
            ALIGN1 = 1,
            ALIGN2,
        }

        public enum eJUDGE_MAP
        {
            NOCHIP = -1,
            OK = 0,
            RW,
            NG,
            //REF_MARK_NOT_FOUND = 1,
            //LIGHT,
            //PACKAGE_ABSENSE,
            //PACKAGE_SIZE,
            //PACKAGE_BURR,
            //PACKAGE_CHIPPING,
            //NO_MARKING,
            //MARKING_COUNT,
            //WRONG_CHAR,
            //SCRATCH,
            //HOST_MAP_REJECT,
            //HOST_MAP_SAT,
            //CASTING_ERROR,
            //UNKNOWN_ERROR,
        }

        public enum eJUDGE_BALL
        {
            NOCHIP = -1,
            OK = 0,
            RW,
            NG,
            //REF_MARK_NOT_FOUND = 1,
            //LIGHT,
            //PACKAGE_ABSENSE,
            //PACKAGE_SIZE,
            //SAW_OFFSET,
            //BALL_MISSING,
            //EXTRA_BALL,
            //BALL_SIZE,
            //BALL_POSITION,
            //BALL_PITCH,
            //DAMAGE_BALL,
            //SCRATCH,
            //BALL_LAND_F_M,
            //HOST_MAP_REJECT,
            //HOST_MAP_SAT,
            //CASTING_ERROR,
            //UNKNOWN_ERROR,
        }

#if _DBLOSS
        //기존 DB에서 LOSS QTY를 받아오는 방식에서는 아래의 LOSS CODE DEF를 사용한다.
        public enum eLOSS_CODE
        {
            CVAR,
            BADC,//볼 색 변색
            DMSN,//외부 크기 규정과 일치X
            BAMS,//볼이 없는 경우
            BABR,//볼과 볼이 연결 된 경우
            BASZ,//볼의 크기가 규정과 일치하지 않는 경우
            BAGO,//볼이 전체적으로 한 방향으로 SHIFT 된 경우
            BAPO,//볼이 전체적으로 한 방향으로 SHIFT 된 경우
            EXTB,
            BADM,//볼 손상
            SUSC,//스크레치
            FRMT,//FM
            MKMS,//NOMARKING
            MKPO,//마킹 위치
            MKFL,//LOT NUMBER와 마킹이 다른 경우
            MKBK,//마킹 끊어짐
            MIPK,//자재 외곽 못찾음
            OSSC,//스크레치
            UMIL,
            HOST_RJ,//호스트 코드
        }
#else
        public enum eLOSS_CODE
        {
            BAGO = 13,
            BAMS = 14,
            BABR = 15,
            BASZ = 16,
            BADM = 19,
            MKMS = 45,
            MKFL = 47,
            MKPO = 56,


            MKBK_46 = 46,
            MKBK_57 = 57,

            BAPO_17 = 17,
            BAPO_18 = 18,

            SUSC_20 = 20,
            SUSC_53 = 53,

            FRMT_35 = 35,
            FRMT_54 = 54,

            DMSN_12 = 12,
            DMSN_42 = 42,
            DMSN_44 = 44,

            
            CVAR = 99,// 위 항목을 제외한 모든 항목은 BIN5로 보고...
            MAX,
            //PKG_NONE = 49,
            //PKG_NONE = 24,
            //MISS_REF = 50, //Mark Not Found
            //MISS_REF = 25, //Mark Not Found


            HOST_RJ,//호스트 코드
        }
#endif


    }

    public class HostLotInfo
    {
        string strLot_id = "";
        string strIts_Id = "";
        string strLot_type = "";
        string strSub_qty = "";
        string strprod_type = "";
        string strtool_no = "";
        string strits_usage = "";
        string stritslotid_in = "";
        string stritslotid_ct = "";
        string strunitsize_x = "";
        string strunitsize_y = "";
        string strunitupper = "";
        string strunitlower = "";
        string strthick = "";
        string strthick_upper = "";
        string strthick_lower = "";
        string strabf = "";
        string strlandpkg_x = "";
        string strlandpkgx_upper = "";
        string strlandpkgx_lower = "";
        string strlandpkg_y = "";
        string strlandpkgy_upper = "";
        string strlandpkgy_lower = "";

        public HostLotInfo()
        {
            strLot_id = "";
            strIts_Id = "";
            strLot_type = "";
            strSub_qty = "";
            strprod_type = "";
            strtool_no = "";
            strits_usage = "";
            stritslotid_in = "";
            stritslotid_ct = "";
            strunitsize_x = "";
            strunitsize_y = "";
            strunitupper = "";
            strunitlower = "";
            strthick = "";
            strthick_upper = "";
            strthick_lower = "";
            strabf = "";
            strlandpkg_x = "";
            strlandpkgx_upper = "";
            strlandpkgx_lower = "";
            strlandpkg_y = "";
            strlandpkgy_upper = "";
            strlandpkgy_lower = "";
        }
        public string LOT_ID
        {
            get
            {
                return strLot_id;
            }
            set { strLot_id = value; }
        }

        //public string ITS_ID
        //{
        //    get
        //    {
        //        return strIts_Id;
        //    }
        //    set { strIts_Id = value; }
        //}


        public string LOT_TYPE
        {
            get
            {
                return strLot_type;
            }
            set { strLot_type = value; }
        }

        public string SUB_QTY
        {
            get
            {
                if (strSub_qty == null)
                {
                    strSub_qty = "";
                }
                return strSub_qty;
            }
            set { strSub_qty = value; }
        }

        public string PROD_TYPE
        {
            get
            {
                return strprod_type;
            }
            set { strprod_type = value; }
        }

        public string TOOL_NO
        {
            get
            {
                return strtool_no;
            }
            set { strtool_no = value; }
        }

        public string ITS_USAGE
        {
            get
            {
                return strits_usage;
            }
            set { strits_usage = value; }
        }

        public string ITSLOTID_IN
        {
            get
            {
                return stritslotid_in;
            }
            set { stritslotid_in = value; }
        }

        public string ITSLOTID_CT
        {
            get
            {
                return stritslotid_ct;
            }
            set { stritslotid_ct = value; }
        }

        public string UNITSIZE_X
        {
            get
            {
                return strunitsize_x;
            }
            set { strunitsize_x = value; }
        }

        public string UNITSIZE_Y
        {
            get
            {
                return strunitsize_y;
            }
            set { strunitsize_y = value; }
        }

        public string UNITUPPER
        {
            get
            {
                return strunitupper;
            }
            set { strunitupper = value; }
        }

        public string UNITLOWER
        {
            get
            {
                return strunitlower;
            }
            set { strunitlower = value; }
        }

        public string THICK
        {
            get
            {
                return strthick;
            }
            set { strthick = value; }
        }

        public string THICK_UPPER
        {
            get
            {
                return strthick_upper;
            }
            set { strthick_upper = value; }
        }

        public string THICK_LOWER
        {
            get
            {
                return strthick_lower;
            }
            set { strthick_lower = value; }
        }

        public string ABF
        {
            get
            {
                return strabf;
            }
            set { strabf = value; }
        }

        public string LANDPKG_X
        {
            get
            {
                return strlandpkg_x;
            }
            set { strlandpkg_x = value; }
        }

        public string LANDPKGX_UPPER
        {
            get
            {
                return strlandpkgx_upper;
            }
            set { strlandpkgx_upper = value; }
        }

        public string LANDPKGX_LOWER
        {
            get
            {
                return strlandpkgx_lower;
            }
            set { strlandpkgx_lower = value; }
        }

        public string LANDPKG_Y
        {
            get
            {
                return strlandpkg_y;
            }
            set { strlandpkg_y = value; }
        }

        public string LANDPKGY_UPPER
        {
            get
            {
                return strlandpkgy_upper;
            }
            set { strlandpkgy_upper = value; }
        }

        public string LANDPKGY_LOWER
        {
            get
            {
                return strlandpkgy_lower;
            }
            set { strlandpkgy_lower = value; }
        }


    }

    public class ProductLotInfo
    {
        string strLotId = "";
        string strCarrierId = "";

        string strCycle_time = "";
        string strPkg_prod_count = "";
        string strStrip_count = "";
        int nEqp_sq_chip_count = 0;
        int nEqp_nq_chip_count = 0;
        string strEqp_rj_chip_count = "";
        string strEqp_sq_tray_count = "";
        string strEqp_nq_tray_count = "";
        string strSat_count = "";
        string strTotal_chip_count = "";
        string strMark_vision_err_count = "";
        string strBall_vision_err_count = "";
        string strX_out_count = "";
        string strHost_sq_chip_count = "";
        string strHost_nq_chip_count = "";
        string strHost_rj_chip_count = "";
               
        string strProcess_err_count = "";
        string strStrip_cutting_count = "";
        string strUnit_picker_place_count = "";

        DateTime dtMove_in_time;
        DateTime dtMove_out_time;

        public ProductLotInfo()
        {
            strLotId = "";
            strCarrierId = "";

            strCycle_time = "";
            strPkg_prod_count = "";
            strStrip_count = "";
            nEqp_sq_chip_count = 0;
            nEqp_nq_chip_count = 0;
            strEqp_rj_chip_count = "";
            strEqp_sq_tray_count = "";
            strEqp_nq_tray_count = "";
            strSat_count = "";
            strTotal_chip_count = "";
            strMark_vision_err_count = "";
            strBall_vision_err_count = "";
            strX_out_count = "";
            strHost_sq_chip_count = "";
            strHost_nq_chip_count = "";
            strHost_rj_chip_count = "";

            strProcess_err_count = "";
            strStrip_cutting_count = "";
            strUnit_picker_place_count = "";

            dtMove_in_time = new DateTime();
            dtMove_out_time = new DateTime();
        }

        public void Clear()
        {
            strLotId = "";
            strCarrierId = "";

            strCycle_time = "";
            strPkg_prod_count = "";
            strStrip_count = "";
            nEqp_sq_chip_count = 0;
            nEqp_nq_chip_count = 0;
            strEqp_rj_chip_count = "";
            strEqp_sq_tray_count = "";
            strEqp_nq_tray_count = "";
            strSat_count = "";
            strTotal_chip_count = "";
            strMark_vision_err_count = "";
            strBall_vision_err_count = "";
            strX_out_count = "";
            strHost_sq_chip_count = "";
            strHost_nq_chip_count = "";
            strHost_rj_chip_count = "";

            strProcess_err_count = "";
            strStrip_cutting_count = "";
            strUnit_picker_place_count = "";

            dtMove_in_time = new DateTime();
            dtMove_out_time = new DateTime();
        }

        public string LOT_ID
        {
            get { return strLotId; }
            set { strLotId = value; }
        }

        public string CARRIER_ID
        {
            get { return strCarrierId; }
            set { strCarrierId = value; }
        }

        public string CYCLE_TIME 
        { 
            get { return strCycle_time; } 
            set { strCycle_time = value; } 
        }
        public string PKG_PROD_COUNT 
        { 
            get { return strPkg_prod_count; } 
            set { strPkg_prod_count = value; }
        }
        public string STRIP_COUNT 
        { 
            get { return strStrip_count; } 
            set { strStrip_count = value; } 
        }
        public int EQP_SQ_CHIP_COUNT 
        { 
            get { return nEqp_sq_chip_count; } 
            set { nEqp_sq_chip_count = value; } 
        }
        public int EQP_NQ_CHIP_COUNT 
        { 
            get { return nEqp_nq_chip_count; } 
            set { nEqp_nq_chip_count = value; } 
        }
        public string EQP_RJ_CHIP_COUNT 
        { 
            get { return strEqp_rj_chip_count; } 
            set { strEqp_rj_chip_count = value; } 
        }
        public string EQP_SQ_TRAY_COUNT 
        { 
            get { return strEqp_sq_tray_count; } 
            set { strEqp_sq_tray_count = value; } 
        }
        public string EQP_NQ_TRAY_COUNT 
        { 
            get { return strEqp_nq_tray_count; } 
            set { strEqp_nq_tray_count = value; } 
        }
        public string SAT_COUNT 
        { 
            get { return strSat_count; } 
            set { strSat_count = value; } 
        }
        public string TOTAL_CHIP_COUNT 
        { 
            get { return strTotal_chip_count; } 
            set { strTotal_chip_count = value; } 
        }
        public string MARK_VISION_ERR_COUNT 
        { 
            get { return strMark_vision_err_count; } 
            set { strMark_vision_err_count = value; } 
        }
        public string BALL_VISION_ERR_COUNT 
        { 
            get { return strBall_vision_err_count; } 
            set { strBall_vision_err_count = value; } 
        }
        public string X_OUT_COUNT 
        { 
            get { return strX_out_count; } 
            set { strX_out_count = value; } 
        }
        public string HOST_SQ_CHIP_COUNT 
        { 
            get { return strHost_sq_chip_count; } 
            set { strHost_sq_chip_count = value; }
        }
        public string HOST_NQ_CHIP_COUNT 
        { 
            get { return strHost_nq_chip_count; } 
            set { strHost_nq_chip_count = value; } 
        }
        public string HOST_RJ_CHIP_COUNT 
        { 
            get { return strHost_rj_chip_count; } 
            set { strHost_rj_chip_count = value; } 
        }

        public string PROCESS_ERR_COUNT 
        { 
            get { return strProcess_err_count; } 
            set { strProcess_err_count = value; } 
        } // SCRAP?
        public string STRIP_CUTTING_COUNT 
        {
            get { return strStrip_cutting_count; } 
            set { strStrip_cutting_count = value; } 
        }
        public string UNIT_PICKER_PLACE_COUNT 
        { 
            get { return strUnit_picker_place_count; } 
            set { strUnit_picker_place_count = value; } 
        }

        public DateTime MOVE_IN_TIME 
        { 
            get { return dtMove_in_time; } 
            set { dtMove_in_time = value; } 
        }
        public DateTime MOVE_OUT_TIME 
        { 
            get { return dtMove_out_time; } 
            set { dtMove_out_time = value; } 
        }
    }

    public class EqpProcInfo
    {
        string strLotId = "";
        string strItsId = "";
        int nMgz_Count = 0;
        int nStrip_input_count = 0;
        int nStrip_output_count = 0;
        int nStrip_cutting_count = 0;
        int nMulti_picker_1_pickup_count = 0;
        int nMulti_picker_2_pickup_count = 0;
        int nMulti_picker_1_place_count = 0;
        int nMulti_picker_2_place_count = 0;
        int nGood_table_1_tray_work_count = 0;
        int nGood_table_2_tray_work_count = 0;
        int nRework_table_tray_work_count = 0;
        int nMark_vision_ok_count = 0;
        int nMark_vision_rw_count = 0;
        int nBall_vision_ok_count = 0;
        int nBall_vision_ng_count = 0;
        int nTotal_chip_prod_count = 0;
        int nTotal_ok_count = 0;
        int nTotal_rw_count = 0;
        int nTotal_ng_count = 0;
        int nIts_count = 0;
        int nTotal_loss_unit_count = 0;

        string strEqp_running_time = "";
        string strEqp_stop_time = "";

        string strEqp_In_time = "";
        string strEqp_Out_time = "";

        string strStrip_In_time = "";
        string strStrip_Out_time = "";
        public EqpProcInfo()
        {
            strLotId = "";
            strItsId = "";
            nMgz_Count = 0;
            nStrip_input_count = 0;
            nStrip_output_count = 0;
            nStrip_cutting_count = 0;
            nMulti_picker_1_pickup_count = 0;
            nMulti_picker_2_pickup_count = 0;
            nMulti_picker_1_place_count = 0;
            nMulti_picker_2_place_count = 0;
            nGood_table_1_tray_work_count = 0;
            nGood_table_2_tray_work_count = 0;
            nRework_table_tray_work_count = 0;
            nMark_vision_ok_count = 0;
            nMark_vision_rw_count = 0;
            nBall_vision_ok_count = 0;
            nBall_vision_ng_count = 0;
            nTotal_chip_prod_count = 0;
            nTotal_ok_count = 0;
            nTotal_rw_count = 0;
            nTotal_ng_count = 0;
            nIts_count = 0;
            nTotal_loss_unit_count = 0;

            strEqp_running_time = "";
            strEqp_stop_time = "";

            strEqp_In_time = "";
            strEqp_Out_time = "";

            strStrip_In_time = "";
            strStrip_Out_time = "";
        }

        public EqpProcInfo(EqpProcInfo info)
        {
            strLotId = info.strLotId;
            strItsId = info.strItsId;
            nMgz_Count = info.nMgz_Count;
            nStrip_input_count = info.nStrip_input_count;
            nStrip_output_count = info.nStrip_output_count;
            nStrip_cutting_count = info.nStrip_cutting_count;
            nMulti_picker_1_pickup_count = info.nMulti_picker_1_pickup_count;
            nMulti_picker_2_pickup_count = info.nMulti_picker_2_pickup_count;
            nMulti_picker_1_place_count = info.nMulti_picker_1_place_count;
            nMulti_picker_2_place_count = info.nMulti_picker_2_place_count;
            nGood_table_1_tray_work_count = info.nGood_table_1_tray_work_count;
            nGood_table_2_tray_work_count = info.nGood_table_2_tray_work_count;
            nRework_table_tray_work_count = info.nRework_table_tray_work_count;
            nMark_vision_ok_count = info.nMark_vision_ok_count;
            nMark_vision_rw_count = info.nMark_vision_rw_count;
            nBall_vision_ok_count = info.nBall_vision_ok_count;
            nBall_vision_ng_count = info.nBall_vision_ng_count;
            nTotal_chip_prod_count = info.nTotal_chip_prod_count;
            nTotal_ok_count = info.nTotal_ok_count;
            nTotal_rw_count = info.nTotal_rw_count;
            nTotal_ng_count = info.nTotal_ng_count;
            nIts_count = info.nIts_count;
            nTotal_loss_unit_count = info.nTotal_loss_unit_count;

            strEqp_running_time = info.strEqp_running_time;
            strEqp_stop_time = info.strEqp_stop_time;

            strEqp_In_time = info.strEqp_In_time;
            strEqp_Out_time = info.strEqp_Out_time;

            strStrip_In_time = info.strStrip_In_time;
            strStrip_Out_time = info.strStrip_Out_time;
        }

        public void Clear()
        {
            strLotId = "";
            strItsId = "";
            nMgz_Count = 0;
            nStrip_input_count = 0;
            nStrip_output_count = 0;
            nStrip_cutting_count = 0;
            nMulti_picker_1_pickup_count = 0;
            nMulti_picker_2_pickup_count = 0;
            nMulti_picker_1_place_count = 0;
            nMulti_picker_2_place_count = 0;
            nGood_table_1_tray_work_count = 0;
            nGood_table_2_tray_work_count = 0;
            nRework_table_tray_work_count = 0;
            nMark_vision_ok_count = 0;
            nMark_vision_rw_count = 0;
            nBall_vision_ok_count = 0;
            nBall_vision_ng_count = 0;
            nTotal_chip_prod_count = 0;
            nTotal_ok_count = 0;
            nTotal_rw_count = 0;
            nTotal_ng_count = 0;
            nIts_count = 0;
            nTotal_loss_unit_count = 0;

            strEqp_running_time = "00:00:00";
            strEqp_stop_time = "00:00:00";

            strEqp_In_time = "0000-00-00 00:00:00";
            strEqp_Out_time = "0000-00-00 00:00:00";

            strStrip_In_time = "0000-00-00 00:00:00";
            strStrip_Out_time = "0000-00-00 00:00:00";
        }

        public string LOT_ID
        {
            get { return strLotId; }
            set { strLotId = value; }
        }

        public int MGZ_COUNT
        {
            get { return nMgz_Count; }
            set { nMgz_Count = value; }
        }
        public int STRIP_INPUT_COUNT 
        {
            get { return nStrip_input_count; }
            set { nStrip_input_count = value; } 
        }
        public int STRIP_OUTPUT_COUNT 
        { 
            get { return nStrip_output_count; } 
            set { nStrip_output_count = value; } 
        }
        public int STRIP_CUTTING_COUNT 
        { 
            get { return nStrip_cutting_count; } 
            set { nStrip_cutting_count = value; } 
        }
        public int MULTI_PICKER_1_PICKUP_COUNT 
        { 
            get { return nMulti_picker_1_pickup_count; } 
            set { nMulti_picker_1_pickup_count = value; } 
        }
        public int MULTI_PICKER_2_PICKUP_COUNT 
        { 
            get { return nMulti_picker_2_pickup_count; } 
            set { nMulti_picker_2_pickup_count = value; } 
        }
        public int MULTI_PICKER_1_PLACE_COUNT 
        { 
            get { return nMulti_picker_1_place_count; } 
            set { nMulti_picker_1_place_count = value; } 
        }
        public int MULTI_PICKER_2_PLACE_COUNT 
        { 
            get { return nMulti_picker_2_place_count; } 
            set { nMulti_picker_2_place_count = value; } 
        }
        public int GOOD_TABLE_1_TRAY_WORK_COUNT 
        { 
            get { return nGood_table_1_tray_work_count; } 
            set { nGood_table_1_tray_work_count = value; } 
        }
        public int GOOD_TABLE_2_TRAY_WORK_COUNT 
        { 
            get { return nGood_table_2_tray_work_count; } 
            set { nGood_table_2_tray_work_count = value; } 
        }
        public int REWORK_TABLE_TRAY_WORK_COUNT 
        { 
            get { return nRework_table_tray_work_count; } 
            set { nRework_table_tray_work_count = value; } 
        }
        public int MARK_VISION_OK_COUNT 
        { 
            get { return nMark_vision_ok_count; } 
            set { nMark_vision_ok_count = value; } 
        }
        public int MARK_VISION_RW_COUNT 
        { 
            get { return nMark_vision_rw_count; } 
            set { nMark_vision_rw_count = value; } 
        }
        public int BALL_VISION_OK_COUNT 
        {
            get { return nBall_vision_ok_count; } 
            set { nBall_vision_ok_count = value; } 
        }
        public int BALL_VISION_NG_COUNT 
        { 
            get { return nBall_vision_ng_count; } 
            set { nBall_vision_ng_count = value; } 
        }
        public int TOTAL_CHIP_PROD_COUNT 
        {
            get { return nTotal_chip_prod_count; }
            set { nTotal_chip_prod_count = value; }
        }

        public int TOTAL_OK_COUNT 
        { 
            get { return nTotal_ok_count; }
            set { nTotal_ok_count = value; } 
        }

        public int TOTAL_RW_COUNT
        {
            get { return nTotal_rw_count; }
            set { nTotal_rw_count = value; }
        }

        public int TOTAL_NG_COUNT 
        { 
            get { return nTotal_ng_count; }
            set { nTotal_ng_count = value; }
        } // SCRAP?

        public int ITS_XMARK_COUNT
        {
            get { return nIts_count; }
            set { nIts_count = value; }
        }

        public int LOSS_UNIT_COUNT
        {
            get { return nTotal_loss_unit_count; }
            set { nTotal_loss_unit_count = value; }
        }

        public string EQP_RUNNING_TIME 
        { 
            get { return strEqp_running_time; }
            set { strEqp_running_time = value; }
        }
        public string EQP_STOP_TIME 
        { 
            get { return strEqp_stop_time; } 
            set { strEqp_stop_time = value; } 
        }

        public string EQP_IN_TIME
        {
            get { return strEqp_In_time; }
            set { strEqp_In_time = value; }
        }

        public string EQP_OUT_TIME
        {
            get { return strEqp_Out_time; }
            set { strEqp_Out_time = value; }
        }
        public string STRIP_IN_TIME
        {
            get { return strStrip_In_time; }
            set { strStrip_In_time = value; }
        }

        public string STRIP_OUT_TIME
        {
            get { return strStrip_Out_time; }
            set { strStrip_Out_time = value; }
        }
    }
	
	public class InspectionData
    {
        public string TIME { get; set; }
        public string RUN_MODE { get; set; }

        public string LOT_ID { get; set; }

        public string STRIP_ID { get; set; }
        public string CHIP_2D_CODE { get; set; }
        public string CHIP_OCR{ get; set; }

        public double CHIP_SIZE_WIDTH_TOP { get; set; }

        public double CHIP_SIZE_HEIGHT_TOP { get; set; }

        public double CHIP_SIZE_OFFSET_X_TOP { get; set; }
        public double CHIP_SIZE_OFFSET_Y_TOP { get; set; }

        public double CHIP_SIZE_WIDTH_BTM { get; set; }

        public double CHIP_SIZE_HEIGHT_BTM { get; set; }

        public double CHIP_SIZE_OFFSET_X_BTM { get; set; }
        public double CHIP_SIZE_OFFSET_Y_BTM { get; set; }

        public string CHIP_ERR_CODE_TOP { get; set; }
        public string CHIP_ERR_CODE_BTM { get; set; }

        public InspectionData(string sTime, string sRunMode, string strLotID, string strStripID, string str2DCode, string strOCR,
                              double dSizeW_Top, double dSizeH_Top, double dOffsetX_Top, double dOffsetY_Top, double dAngle_Top,
                              double dSizeW_Btm, double dSizeH_Btm, double dOffsetX_Btm, double dOffsetY_Btm, double dAngle_Btm,
                              string strErrCodeTop, string strErrCodeBtm)
        {
            TIME = sTime;
            RUN_MODE = sRunMode;
        }
    }

    public static class MCDF
    {
        //설비상태
        public const int MC_IDLE = 0; //프로그램 구동후 초기화 전
        public const int MC_INITIALIZING = 1; //초기화 중일때
        public const int MC_STOP = 2; //초기화 된 상태로 정지
        public const int MC_ERR_STOP = 3; //에러로 정지된 상태
        public const int MC_RUN = 4; //RUN
        public const int MC_LOTEND = 5; //LOTEND


        //Run Event
        public const int CMD_CYCLE_STOP = 0;
        public const int CMD_STOP = 1;
        public const int CMD_RUN = 2;

        //Log 종류
        public const int EVENT_LOG = 0;
        public const int ERROR_LOG = 1;
        public const int RUN_LOG = 2;
        public const int COMM_LOG = 3; // 통신로그

        //Login Level
        public const int LEVEL_NOT_LOGIN = -1;
        public const int LEVEL_OP = 0;
        public const int LEVEL_ENG = 1;
        public const int LEVEL_MAK = 2;

        //Screen No
        public const int SCREEN_LOGIN = 0;
        public const int SCREEN_AUTO = 1;
        public const int SCREEN_MANUAL = 2; 
        public const int SCREEN_RECIPE = 3;
        public const int SCREEN_CONFIG = 4;
        public const int SCREEN_IO = 5;
        public const int SCREEN_ALARM = 6;
        public const int SCREEN_LOG = 7;
        public const int SCREEN_TEACH = 8;

        public const int RTC_BOARD = 0;

        //Safty
        public const int SAFETY_OK = 0;

        //CONV_FLOOR_NO
        public const int CONV_1F = 0;
        public const int CONV_2F = 1;
        public const int CONV_3F = 2;
        public const int CONV_4F = 3;
        public const int CONV_5F = 4;
        public const int CONV_MAX = 5;

        public const int CONV_LD_1 = 0;
        public const int CONV_LD_2 = 1;
        public const int CONV_ULD_1 = 0;
        public const int CONV_ULD_2 = 1;

        //TRAY_TABLE
        public const int ULD_TRAY_GOOD_1 = 0;
        public const int ULD_TRAY_GOOD_2 = 1;
        public const int ULD_TRAY_REWORK = 2;

        //TRAY_ELV_NO
        public const int ELV_TRAY_GOOD_1 = 0;
        public const int ELV_TRAY_GOOD_2 = 1;

        public const int ELV_TRAY_REWORK = 2;
        public const int ELV_TRAY_EMPTY_1 = 3;
        public const int ELV_TRAY_EMPTY_2 = 4;
        public const int ELV_MAX_CNT = 5;

        //== 2023-12-31 XYZT OFFSET 추가
        //HEAD NO
        public const int HEAD1 = 0;
        public const int HEAD2 = 1;
        public const int HEAD_MAX = 2;

        //TABLE NO
        public const int MAP_TABLE_1 = 0;
        public const int MAP_TABLE_2 = 1;
        public const int GOOD_TRAY_1 = 2;
        public const int GOOD_TRAY_2 = 3;
        public const int REWORK_TRAY = 4;
        public const int TABLE_NO_MAX = 5;

        //XYZT
        public const int XYZT_X = 0;
        public const int XYZT_Y = 1;
        public const int XYZT_Z = 2;
        public const int XYZT_T = 3;
        public const int XYZT_MAX = 4;

        //PAD NO
        public const int PAD_1 = 0;
        public const int PAD_2 = 1;
        public const int PAD_3 = 2;
        public const int PAD_4 = 3;
        public const int PAD_5 = 4;
        public const int PAD_6 = 5;
        public const int PAD_7 = 6;
        public const int PAD_8 = 7;
        public const int PAD_MAX = 8;
        //========== XYZT OFFSET 추가

        //TRAY_OHT_NO
        public const int OHT_TU_TRAY_GOOD = 0;
        public const int OHT_TU_TRAY_REWORK = 1;
        public const int OHT_TU_TRAY_EMPTY_1 = 2;
        public const int OHT_TU_TRAY_EMPTY_2 = 3;

        public const int OHT_TL_TRAY_EMPTY_1 = 0;
        public const int OHT_TL_TRAY_EMPTY_2 = 1;
        public const int OHT_TL_TRAY_COVER = 2;

        //CW/CCW
        public const bool CONV_CW = true;
        public const bool CONV_CCW = false;

        //LAMP, SWITCH
        public const int OFF = 0;
        public const int ON = 1;
        public const int BLINK = 2;


        //MACHINE NO
        public const int LOADER = 0;
        public const int SAW = 1;
        public const int SORTER = 2;
		public const int MACHINE_MAX = 3;

        //LotIndex
        public const int CURRLOT = 0;//220611 pjh
        public const int PREVLOT = 1;
        public const int LOTMAX = 2;

        public enum eLAMP_CONDITION
        {
            ERROR = 0,
            START_UP,
            STAND_BY,
            SEMI_AUTO,
            FULL_AUTO,
            LOT_END,
            CYCLE_STOP,
            OP_CALL,
            MAX
        }

        public enum eLD_RFID
        {
            RFID1 = 1,
            RFID2 = 2,
            RFID3 = 3,
            RFID6 = 6,
        }

        public enum eUNIT
        {
            NO1 = 0,
            NO2 = 1,
            MAX,
        }

        public enum eOutPort
        {
            NONE = -1,
            GD_1,
            GD_2,
            RW,
            X_OUT,
            S_G_NG,
            SAT_1,
            SAT_2,
            MAX,
        }
        
        public enum eStage
        {
            STAGE1 = 0,
            STAGE2,
            MAX,
        }
    }

    public static class MESDF
    {
#region ENUM


        public enum eControlStatus
        {
            OFFLINE = 1,//1, 2, 3 모두 Offline
            OFFLINE_2 = 2,
            OFFLINE_3 = 3,
            ONLINE_LOCAL = 4,
            ONLINE_REMOTE = 5,
        }

        public enum eEQPStatus
        {
            IDLE_1 = 1,//1, 3, 4 모두 IDLE
            IDLE_3 = 3,
            IDLE_4 = 4,
            RUN = 5,
            DOWN_6 = 6,
            DOWN_7 = 7,
            STANDBYRUN = 8,
        }

        public enum ePPChangeCode
        {
            SELECT = 0,
            CREATE = 1,
            DELETE = 2,
            SAVE = 3,
        }

        public enum eCELL_STATUS
        {
            OK = 1,
            XOUT = 4,//SAT 사용 시 원천 불량 코드
        }


        public enum eRCPACK
        {
            OK = 0,
            Permission_not_granted,
            Length_error,
            Matrix_overflow,
            PPID_not_found,
            Mode_unsupported,
            delay,
        }

        public enum eRCMDACK
        {
            OK = 0,
            CMD_DOSE_NOT_EXIST,
            CANNOT_PERFORM_NOW,
            AT_LEAST_ONE_PARAMETER_IS_INVALID,
            ACKNOWLEDGE_COMMAND_WILL_BE_PERFORMED_WITH_COMPLETION_SIGNALED_LATER_BY_AN_EVENT,
            REJECTED_ALREADY_IN_DESIRED_CONDITION,
            NO_SUCH_OBJECT_EXISTS,
        }
        
        public enum eML_PORT
        {
            ML01 = 0,
            ML02,
            ML03,
            ML04,
            MAX,
        }

        public enum eTL_PORT
        {
            TL01 = 0,
            TL02,
            TL03,
            MAX,
        }

        public enum eTU_PORT
        {
            TU01 = 0,
            TU02,
            TU03,
            TU04,
            MAX,
        }

        public enum eTF_PORT
        {
            TF01 = 0,
            TF02,
            TF03,
            MAX,
        }

        public enum eORIGIN
        {
            UR = 1,
            UL,
            LL,
            LR,
        }

        public enum eSVID
        {
            LD_PROC_PORT_CARRIER_LOT_ID = 101,
            PREV_MGZ_LOT_ID = 102,
            USER_ID = 103,
            STRIP_ID = 104,
            MID_BLADE_Z1 = 105,
            MID_BLADE_Z2 = 106,
            LD_PROC_PORT_CARRIER_RFID = 108,
            SUBMAP_MODE_LOT = 109,
            TRAY_LOT_ID_MANUAL = 112,
            PRODUCT_ID = 115,
            EQP_STATUS = 121,
            CONTROL_ID = 123,
            SOFTREV = 143,
            RCPID_CURRENT = 144,
            PP_CHANGE_CODE = 146,
            PPID_CURRENT = 147,
            LOT_BALL_VISION_YIELD = 206,
            LOT_MARK_VISION_YIELD = 207,
            AUTO_MODE = 250,
            LD_LOAD_PORT_STATUS_ML01 = 251,
            LD_PROC_PORT_STATUS_ML02 = 252,
            LD_PROC_PORT_STATUS_ML03 = 253,
            LD_LOAD_PORT_STATUS_ML04 = 254,
            UD_LOAD_PORT_STATUS_TL01 = 255,
            UD_LOAD_PORT_STATUS_TL02 = 256,
            UD_LOAD_PORT_STATUS_TU01 = 257,
            UD_LOAD_PORT_STATUS_TU02 = 258,
            UD_LOAD_PORT_STATUS_TU03 = 259,
            UD_LOAD_PORT_STATUS_TU04 = 260,
            UD_PROC_PORT_STATUS_TF01 = 261,
            UD_PROC_PORT_STATUS_TF02 = 262,
            UD_PROC_PORT_STATUS_TF03 = 263,
            LD_LOAD_PORT_CARRIER_RFID_ML01 = 264,
            LD_PROC_PORT_CARRIER_RFID_ML02 = 265,
            LD_PROC_PORT_CARRIER_RFID_ML03 = 266,
            LD_LOAD_PORT_CARRIER_RFID_ML04 = 267,
            UD_LOAD_PORT_CARRIER_RFID_TL01 = 268,
            UD_LOAD_PORT_CARRIER_RFID_TL02 = 269,
            UD_LOAD_PORT_CARRIER_RFID_TU01 = 270,
            UD_LOAD_PORT_CARRIER_RFID_TU02 = 271,
            UD_LOAD_PORT_CARRIER_RFID_TU03 = 272,
            UD_LOAD_PORT_CARRIER_RFID_TU04 = 273,
            LD_LOAD_PORT_ID_ML01 = 274,
            LD_PROC_PORT_ID_ML02 = 275,
            LD_PROC_PORT_ID_ML03 = 276,
            LD_LOAD_PORT_ID_ML04 = 277,
            UD_LOAD_PORT_ID_TL01 = 278,
            UD_LOAD_PORT_ID_TL02 = 279,
            UD_LOAD_PORT_ID_TU01 = 280,
            UD_LOAD_PORT_ID_TU02 = 281,
            UD_LOAD_PORT_ID_TU03 = 282,
            UD_LOAD_PORT_ID_TU04 = 283,
            UD_LOAD_PORT_ID_COM = 284,
            UD_LOAD_PORT_CARRIER_TYPE_TL01 = 285,
            UD_LOAD_PORT_CARRIER_TYPE_TL02 = 286,
            UD_LOAD_PORT_CARRIER_TYPE_TU01 = 287,
            UD_LOAD_PORT_CARRIER_TYPE_TU02 = 288,
            UD_LOAD_PORT_CARRIER_TYPE_TU03 = 289,
            UD_LOAD_PORT_CARRIER_TYPE_TU04 = 290,
            UD_LOAD_PORT_CARRIER_QTY_TU01 = 291,
            UD_LOAD_PORT_CARRIER_QTY_TU02 = 292,
            UD_LOAD_PORT_CARRIER_QTY_TU03 = 293,
            UD_LOAD_PORT_CARRIER_QTY_TU04 = 294,
            UD_LOAD_PORT_CARRIER_QTY_TL01 = 295,
            UD_LOAD_PORT_CARRIER_QTY_TL02 = 296,
            UD_LOAD_PORT_CARRIER_LOT_ID = 297,
            SUB_YN = 301,
            UD_LOAD_PORT_CARRIER_RFID_LIST_TU01 = 303,
            UD_LOAD_PORT_CARRIER_RFID_LIST_TU02 = 304,
            UD_LOAD_PORT_STATUS_TL03 = 306,
            UD_LOAD_PORT_CARRIER_RFID_TL03 = 307,
            UD_LOAD_PORT_ID_TL03 = 308,
            UD_LOAD_PORT_CARRIER_TYPE_TL03 = 309,
            UD_LOAD_PORT_CARRIER_QTY_TL03 = 310,
            UD_LOAD_PORT_CARRIER_RFID = 311,
            UD_LOAD_PORT_CARRIER_TYPE = 312,
            UD_LOAD_PORT_CARRIER_QTY = 313,
            UD_LOAD_PORT_CARRIER_MODEL = 314,
            SAW_LOADING_SUB_ID = 360,
            SAW_UNLOADING_SUB_ID = 362,
            SORTER_LOADING_SUB_ID,
            MAP_VISION_TABLE1_SUB_ID,
            MAP_VISION_TABLE2_SUB_ID,

            TF01_UNIT_STATUS = 368,
            TF02_UNIT_STATUS,
            TF03_UNIT_STATUS,
            TF01_CARRIER_2DID,
            TF02_CARRIER_2DID,
            TF03_CARRIER_2DID,
            FDC_CHAMBER_ID1 = 380,
            FDC_CHAMBER_ID2 = 381,
            FDC_KIT_ID1 = 382,
            FDC_KIT_ID2 = 383,
            SUBSTRATE_ID1 = 384,
            SUBSTRATE_ID2 = 385,
            SUBSTRATE_ARRAY_X = 386,
            SUBSTRATE_ARRAY_Y = 387,

            UD_PROC_PORT_ID_COM = 743,

            DUMMY_MODE = 771,
            DUMMY_QTY_SUB = 772,

            KIT_ID_SP = 2153,

            MAT_ID1 = 2324,
            MAT_ID2 = 2325,
            MAT_ID3 = 2326,
            MAT_ID4 = 2327,
            MAT_ID5 = 2328,
            MAT_ID6 = 2329,
            MAT_ID7 = 2330,
            MAT_ID8 = 2331,
            MAT_ID9 = 2332,
            MAT_ID10 = 2333,
            MAT_QTY1 = 2334,
            MAT_QTY2 = 2335,
            MAT_QTY3 = 2336,
            MAT_QTY4 = 2337,
            MAT_QTY5 = 2338,
            MAT_QTY6 = 2339,
            MAT_QTY7 = 2340,
            MAT_QTY8 = 2341,
            MAT_QTY9 = 2342,
            MAT_QTY10 = 2343,

            MAX,
        }

        /// <summary>
        /// ECV LIST는 Config의 enum OPTNUM와 순번 동일함.
        /// 단, OPTNUM에서 eECID.START만큼 더해서 한 번호로 보고한다.
        /// </summary>
        public enum eECID
        {
            START = 3150,
            MAX = 6150,
        }

        public enum eRCP_ITEMS
        {
            Mgz_Slot_Count = 0,
            Mgz_Slot_Pitch,

            Map_Unit_Count_X = 10,
            Map_Unit_Count_Y,
            Map_Unit_Pitch_X,
            Map_Unit_Pitch_Y,
            Map_Insp_Chip_View_Count_X,
            Map_Insp_Chip_View_Count_Y,

            Tray_Count_X = 30,
            Tray_Count_Y,
            Tray_Pitch_X,
            Tray_Pitch_Y,
            Tray_Thickness,
            Tray_Insp_Chip_Count_X,
            Tray_Insp_Chip_Count_Y,
            Picker_Place_T,

            Clean_Unit_Pk_Sponge_Count = 50,
            Clean_Unit_Pk_Sponge_Velocity,

            Clean_Unit_Pk_Water_Count,

            Clean_Unit_Pk_Air_Count,

            Clean_Unit_Pk_Dry_Blow_Count,
            Clean_Unit_Pk_Dry_Blow_Velocity,

            Clean_Unit_Pk_Kit_Air_Blow_Count,

            Clean_Table_Water_Count,
            Clean_Table_Air_Count,

            Clean_Dryblock_Air_Count,
            Clean_Dryblock_Air_Velocity,
       
            //Clean_Dry_block_Unloading_Velocity,

            Clean_Stage_Block_Dry_Blow_Count,
            Clean_Stage_Block_Dry_Blow_Velocity,
            Clean_Stage_Block_Dry_Blow_Delay,

            Clean_Map_Table_Dry_Blow_Count1,
            Clean_Map_Table_Dry_Blow_Count2,

            Clean_Map_Table_Dry_Blow_Velocity1,
            Clean_Map_Table_Dry_Blow_Velocity2,

            SORTER_MAX = 500,

            SAW_MAX = 1000,
            
            VISION_MAX = 1500,
        }

        public enum eRCMD_TRAY_FULL
        {
            TRAY_ID,
            CONFIRM_FLAG,
            PORT_NO,
            LOT_ID,
            ERR_MSG,
        }

        public enum eRCMD_NEW_TRAY
        {
            TRAY_ID, //	HITB6104
            CONFIRM_FLAG, //	T: 작업 계속, F: 작업 중지, C: Auto Empty Tray Change
            TRAY_QTY, //	25 (최대 30장)
            TRAY_MODEL, //	00.00X00.00
            ERR_MSG,
        }

        public enum eRCMD_LOT_END
        {
            LOT_ID,
            CONFIRM_FLAG,
            //이하 내용은 AUTO MODE사용안할 경우 라벨 프린터로 출력한다.
        }

        public enum eRCMD_LOT_START
        {
            MGZ_ID,
            CONFIRM_FLAG,
            LOT_ID,
            LOT_OPER,
            PROD_ID,
            LOT_QTY,
            MGZ_QTY,
            SUB_QTY,
            RECIPE_NAME,
            SORTING_FLAG,
            ERR_MSG,
        }

        public enum eRCMD_TRAY_VERIFY
        {
            TRAY_ID,
            CONFIRM_FLAG,
            TRAY_MODEL,
            ERR_MSG,
        }

        public enum eRCMD_SUB_UNLOAD
        {
            SUB_ID,
            CONFIRM_FLAG,
            ERR_MSG,
        }

        public enum eRCMD_SUB_LOAD
        {
            SUB_ID,
            CONFIRM_FLAG,
            ERR_MSG,
        }

        public enum eWORKING_DATA
        {
            ANSWER = 0,
            CMD,
            TABLE_NO,
            AUTO_DOWN_Z1,
            AUTO_DOWN_Z2,
            BLADE_EDGE_Z1,
            BLADE_EDGE_Z2,
            BLADE_WASTE_Z1,
            BLADE_WASTE_Z2,
            BLADE_LAST_Z1,
            BLADE_LAST_Z2,
            BLADE_L1_Z1,
            BLADE_L1_Z2,
            SETUP_L1_Z1,
            SETUP_L1_Z2,
            USER_L1_Z1,
            USER_L1_Z2,
            COUNT_BLADE_Z1,
            COUNT_BLADE_Z2,
            COUNT_SETUP_Z1,
            COUNT_SETUP_Z2,
            COUNT_USER_Z1,
            COUNT_USER_Z2,
            B_CT_POSW,
            B_CT_POSZ,
            NOW_CUT_L,
            NOW_SPEED,
            NOW_HEIGHT_Z1,
            NOT_HEIGHT_Z2,
            CH_Q0,
            CH_Q1,
            CH_Q2,
            CH_Q3,
            CH_Q4,
            CH_Q5,
            CH_Q6,
            CH_Q7,
            CH_Q8,
            CH_Q9,
            KERF_CENTER_Z1,
            KERF_CENTER_Z2,
            KERF_CHIP_A_Z1,
            KERF_CHIP_A_Z2,
            KERF_CHIP_W_Z1,
            KERF_CHIP_W_Z2,
            KERF_HALF_Z1,
            KERF_HALF_Z2,
            KERF_MAX_Z1,
            KERF_MAX_Z2,
            KERF_POINT_Z1,
            KERF_POINT_Z2,
            KERF_WIDTH_Z1,
            KERF_WIDTH_Z2,
            MAX,
        }

#endregion



        public const string CONFIRM_FAIL = "F";
        public const string CONFIRM_SUCCESS = "T";
        public const string CONFIRM_CONVERSION = "C";
        public const string CONFIRM_RCP_CHANGE = "R";

        public static List<EqRecipeProperties> EqRcpParam = new List<EqRecipeProperties>();
        public static List<HostRecipeProperties> HostRcpParam = new List<HostRecipeProperties>();
        public static List<ITEMVAL> SV_LIST = new List<ITEMVAL>();
        public static List<ITEMVAL> ECV_LIST = new List<ITEMVAL>();

        public static string LAST_PP_CHANGE_CODE = "";
        public static string LAST_LD_PROC_PORT_CARRIER_RFID = "";
        public static string LAST_TRAY_LOT_ID_MANUAL = "";
        public static string LAST_UD_LOAD_PORT_CARRIER_RFID = "";
        public static string LAST_UD_LOAD_PORT_CARRIER_TYPE = "";
        public static string LAST_UD_LOAD_PORT_CARRIER_QTY = "";
        public static string LAST_UD_LOAD_PORT_CARRIER_MODEL = "";
        public static string LAST_UD_LOAD_PORT_COM_ID = "";
        public static string[] LAST_LD_MGZ_ID = new string[4]{ "", "", "", "" };
        public static string[] LAST_LD_MGZ_STATUS = new string[4] { "0", "0", "0", "0" };
        public static bool IS_STOP_FROM_HOST = false;
        public static void init()
        {
            try
            {
                MESDF.ECV_LIST.Clear();

                if (MESDF.ECV_LIST.Count < (int)MESDF.eECID.MAX)
                {
                    ITEMVAL data;
                    for (int i = 0; i < (int)MESDF.eECID.MAX; i++)
                    {
                        data = new ITEMVAL(i, "", "");
                        MESDF.ECV_LIST.Add(data);

                    }
                }

                MESDF.SV_LIST.Clear();

                if (MESDF.SV_LIST.Count < (int)MESDF.eSVID.MAX)
                {
                    ITEMVAL data;
                    for (int i = 0; i < (int)MESDF.eSVID.MAX; i++)
                    {
                        data = new ITEMVAL(i, "", "");
                        MESDF.SV_LIST.Add(data);

                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public class ITEMVAL
    {
        public int ID = 0;
        public string BEFORE = "";
        public string AFTER = "";

        public ITEMVAL(int nid, string before, string after)
        {
            ID = nid;
            BEFORE = before;
            AFTER = after;
        }
    }
	
    public class SeqProcCycleInfo
    {
        public bool[] bIsCyceRun = new bool[50];
        //public DateTime[] dtLastCycleIn = new DateTime[50];
        //public DateTime[] dtLastCycleOut = new DateTime[50];
    }

#region 기본 정보
    public class SeqDefault
    {
        // 현재 시퀀스 번호
        public int nCurrentSeqNo = 0;

        public bool bIsReady = false;
        public bool bProcStart = false;
        public bool bProcEnd = false;

        public int nCurrCycleNo = 0;
        public SeqProcCycleInfo infoProcCyc = new SeqProcCycleInfo();
        
        // 진행 시간
        TimeSpan tsElapsed;

        public Stopwatch swSeq = new Stopwatch();
        public bool[] bSeqIfVar = new bool[50];

        /// <summary>
        /// 값 초기화
        /// </summary>
        public virtual void Init()
        {
            nCurrentSeqNo = 0;

            // Interface Var Clear
            Array.Clear(bSeqIfVar, 0, bSeqIfVar.Length);
        }

        /// <summary>
        /// Strip 투입 시간 저장
        /// </summary>
        public virtual void ProcCycleStart(int nCycleStep)
        {
            infoProcCyc.bIsCyceRun[nCycleStep] = true;
            bProcStart = true;
            bIsReady = false;
        }

        /// <summary>
        /// Strip 배출 시간 저장
        /// </summary>
        public virtual void ProcCycleEnd(int nCycleStep)
        {
            infoProcCyc.bIsCyceRun[nCycleStep] = false;
            bProcEnd = true;
        }

        public virtual void ProcCycleAllEnd()
        {
            for (int nCycStep = 0; nCycStep < infoProcCyc.bIsCyceRun.Length; nCycStep++)
            {
                infoProcCyc.bIsCyceRun[nCycStep] = false;
            }

            bProcEnd = true;
        }


        /// <summary>
        /// jig 진행 시간
        /// </summary>
        /// <returns></returns>
        //public virtual TimeSpan GetProcCycleTime(int nCycleUnitIdx)
        //{
        //    //if (bProcStart == false) tsElapsed = TimeSpan.Zero;
        //    //else if (bProcEnd == false) tsElapsed = DateTime.Now - dtLastProcIn[nProcIdx, nCycleUnitIdx];
        //    if (infoProcCyc.bIsCyceRun[nCycleUnitIdx] == true && nCurrCycleNo == nCycleUnitIdx) tsElapsed = DateTime.Now - infoProcCyc.dtLastCycleIn[nCycleUnitIdx];
        //    else tsElapsed = infoProcCyc.dtLastCycleOut[nCycleUnitIdx] - infoProcCyc.dtLastCycleIn[nCycleUnitIdx];

        //    return tsElapsed;
        //}


        public virtual void InitProcTime()
        {
            bProcStart = false;
            bProcEnd = false;
        }

        public virtual void SetProcReady()
        {
            bIsReady = true;
        }

        public virtual bool IsProcReady()
        {
            return bIsReady;
        }

        public virtual bool IsProcCycleRun()
        {
            for (int nCycStep = 0; nCycStep < infoProcCyc.bIsCyceRun.Length; nCycStep++)
            {
                if (infoProcCyc.bIsCyceRun[nCycStep])
                {
                    return true;
                }
            }
            
            return false;
        }
    }
#endregion

#region LOADER



    public class seqLoader_LdConv : SeqDefault
    {
        public enum SEQ_LOADER_LOAD_CONV
        {
            NONE = 0,
            INIT,
            CHECK_CONV_SENSOR,
            INPUT_MAGAZINE,
            LOADING_MAGAZINE_1,
            LOADING_MAGAZINE_2,
            READING_MAGAZINE_BCR,
            WAITING_REQ,
            MAGAZINE_LOADING_WAIT,
            MAGAZINE_LOADING_END,
            MAGAZINE_UNLOAD,
            FINISH
        }

        public string MGZ_ID;

#region I/F BIT INDEX

        public const int MGZ_CONV_RUN = 0;
        public const int MGZ_CONV_COMPLETED = 1;

        #endregion

        public seqLoader_LdConv()
        {
            MGZ_ID = "";
        }

        public override void Init()
        {
            base.Init();
        }
    }

    public class seqLoader_UldConv : SeqDefault
    {
        public enum SEQ_LOADER_UNLOAD_CONV
        {
            NONE = 0,
            INIT,
            CHECK_CONV_SENSOR,
            WAITING_REQ,
            //MAGAZINE_LOADING_START, // 언로드 컨베이어의 로딩은 필요 시 사용,
            MAGAZINE_LOADING_END,
            MAGAZINE_UNLOADING_RUN_1,
            MAGAZINE_UNLOADING_RUN_2,
            MAGAZINE_UNLOADING_WAIT,
            MAGAZINE_UNLOADING_END,

            FINISH
        }

#region I/F BIT INDEX

        public const int MGZ_CONV_RUN = 0;
        public const int MGZ_CONV_COMPLETED = 1;

#endregion

        public seqLoader_UldConv()
        {
        }

        public override void Init()
        {
            base.Init();
        }
    }


#endregion

    public class seqLdMzLoading : SeqDefault
    {
        public enum SEQ_LD_MZ_LOADING
        {
            NONE = 0,

            INIT,
            CHECK_INTERLOCK,
            CHECK_LOAD_STOP,

            // 투입 대기
            WAITING_LOAD_STATUS,

            //작업 할 컨베이어 매거진 로드 요청 / 대기
            //LOADING_MAGAIZNE_CONV_REQ_WAIT,
            LOADING_MAGAIZNE_CONV_REQ,
            LOADING_MAGAIZNE,

            // 매거진 트랜스퍼 픽업 시작
            LOADING_MAGAZINE_TRANSFER_END,

            ///////////////////////////////
            // MES 보고 및 대기 구간 필요 시 사용
            MES_REPORT_SEND, // 추후 명칭 변경하면 됨,
            MES_REPORT_WAITING,
            ///////////////////////////////

            // 매거진 공급 전 CLIP OPEN 확인 

            // 매거진 작업 위치로 공급 동작
            SUPPLY_MAGAZINE_CLIP_CHECK, // 공급 전 매거진 클립 오픈 확인
            SUPPLY_MAGAZINE_START,
            SUPPLY_MAGAZINE_END,


            // 자재 투입
            WORKING_MAGAZINE_START,
            WORKING_MAGAZINE_END,

            // 작업 완료 대기
            WAITING_UNLOAD_STATUS,

            //작업 할 컨베이어 매거진 언로드 요청 / 대기
            //UNLOADING_MAGAIZNE_CONV_REQ_WAIT,

            UNLOADING_MAGAIZNE_CONV_REQ,
            UNLOADING_MAGAIZNE,

            UNLOADING_MAGAZINE_END,

            FINISH                              
        }

        public enum TACT_CYCLE_UNIT_IDX
        {
            LD_ELV_MGZ_LOADING = 0,

            MAX,
        }

        public int nElvMzLoadCount;

#region I/F BIT INDEX

        public const int MGZ_TRANSFER_LOADING_RUN = 0;
        public const int MGZ_TRANSFER_LOADING_COMPLETE = 1;
        public const int MGZ_TRANSFER_UNLOADING_RUN = 2;
        public const int MGZ_TRANSFER_UNLOADING_COMPLETE = 3;

        public const int MGZ_LOAD_CONV_LOADING_REQ = 4;
        public const int MGZ_LOAD_CONV_2_LOADING_REQ = 5;
        public const int MGZ_LOAD_CONV_UNLOADING_REQ = 6;
        public const int MGZ_LOAD_CONV_2_UNLOADING_REQ = 7;

        public const int MGZ_ULOAD_CONV_UNLOADING_REQ = 8;
        public const int MGZ_ULOAD_2_CONV_UNLOADING_REQ = 9;
#endregion

        public bool bIsLoadStop;

        public bool bLdConvMgzLoadPushSW = false; // 매거진 배출 S/W 
        public bool bLdConvMgzUnloadPushSW = false; // 매거진 배출 S/W 
        public bool bUldConvMgzLoadPushSW = false; // 매거진 배출 S/W 
        public bool bUldConvMgzUnloadPushSW = false; // 매거진 배출 S/W 
        

        public bool bMgzUnloadReq = false; // 매거진이 존재하는 경우 강제 언로드 요청
        public bool bMgzPopErrSelectNewMgz = false; // 매거진 알람 발생 시 팝업창에서 신규 매거진 투입 FLAG

        public bool[] bInterfaceRcv = new bool[10];
        public bool[] bInterfaceSnd = new bool[10];

        public string MGZ_ID;
        public string LOT_ID;
        public string ITS_ID;

        public seqLdMzLoading()
        {
        }

        public override void Init()
        {
            base.Init();

            nElvMzLoadCount = 0;
            bIsLoadStop = false;
            bMgzUnloadReq = false;
            bMgzPopErrSelectNewMgz = false;
            bLdConvMgzUnloadPushSW = false;
            bUldConvMgzUnloadPushSW = false;
        }
    }

    public class seqLdMzUnloading : SeqDefault
    {
        public enum SEQ_LD_MZ_UNLOADING
        {
            NONE = 0,

            INIT,
            CHECK_INTERLOCK,
            CHECK_LOAD_STOP,

            // 투입 대기
            WAITING_LOAD_STATUS,

            //작업 할 컨베이어 매거진 로드 요청 / 대기
            //LOADING_MAGAIZNE_CONV_REQ_WAIT,
            LOADING_MAGAIZNE_CONV_REQ,
            LOADING_MAGAIZNE_CONV_WAIT,

            // 매거진 트랜스퍼 픽업 시작
            LOADING_MAGAIZNE_TRANSFER_START,
            LOADING_MAGAZINE_TRANSFER_END,

            ///////////////////////////////
            // MES 보고 및 대기 구간 필요 시 사용
            MES_REPORT_SEND, // 추후 명칭 변경하면 됨,
            MES_REPORT_WAITING,
            ///////////////////////////////

            // 매거진 공급 전 CLIP OPEN 확인 

            // 매거진 작업 위치로 공급 동작
            SUPPLY_MAGAZINE_CLIP_CHECK, // 공급 전 매거진 클립 오픈 확인
            SUPPLY_MAGAZINE_START,
            SUPPLY_MAGAZINE_END,


            // 자재 투입
            WORKING_MAGAZINE_START,
            WORKING_MAGAZINE_END,

            // 작업 완료 대기
            WAITING_UNLOAD_STATUS,

            //작업 할 컨베이어 매거진 언로드 요청 / 대기
            //UNLOADING_MAGAIZNE_CONV_REQ_WAIT,

            UNLOADING_MAGAIZNE_CONV_REQ,
            UNLOADING_MAGAIZNE_CONV_WAIT,


            // 매거진 언로드 동작
            UNLOADING_MAGAZINE_START,
            UNLOADING_MAGAZINE_END,

            FINISH
        }

        public enum TACT_CYCLE_UNIT_IDX
        {
            LD_ELV_MGZ_LOADING = 0,

            MAX,
        }

        public int nElvMzLoadCount;

        #region I/F BIT INDEX

        public const int MGZ_TRANSFER_LOADING_RUN = 0;
        public const int MGZ_TRANSFER_LOADING_COMPLETE = 1;
        public const int MGZ_TRANSFER_UNLOADING_RUN = 2;
        public const int MGZ_TRANSFER_UNLOADING_COMPLETE = 3;

        public const int MGZ_LOAD_CONV_LOADING_REQ = 4;
        public const int MGZ_LOAD_CONV_2_LOADING_REQ = 5;
        public const int MGZ_LOAD_CONV_UNLOADING_REQ = 6;
        public const int MGZ_LOAD_CONV_2_UNLOADING_REQ = 7;

        public const int MGZ_ULOAD_CONV_UNLOADING_REQ = 8;
        public const int MGZ_ULOAD_2_CONV_UNLOADING_REQ = 9;
        #endregion

        public bool bIsLoadStop;

        public bool bLdConvMgzUnloadPushSW = false; // 매거진 배출 S/W 
        public bool bUldConvMgzUnloadPushSW = false; // 매거진 배출 S/W 

        public bool bMgzUnloadReq = false; // 매거진이 존재하는 경우 강제 언로드 요청
        public bool bMgzPopErrSelectNewMgz = false; // 매거진 알람 발생 시 팝업창에서 신규 매거진 투입 FLAG

        public bool[] bInterfaceRcv = new bool[10];
        public bool[] bInterfaceSnd = new bool[10];

        public string MGZ_ID;
        public string LOT_ID;
        public string ITS_ID;

        public seqLdMzUnloading()
        {
        }

        public override void Init()
        {
            base.Init();

            nElvMzLoadCount = 0;
            bIsLoadStop = false;
            bMgzUnloadReq = false;
            bMgzPopErrSelectNewMgz = false;
            bLdConvMgzUnloadPushSW = false;
            bUldConvMgzUnloadPushSW = false;
        }
    }

    public class seqLdMzLotStart : SeqDefault
    {
        public int nLdPort = 0;

        string strLotId = "";
        string strMgzId = "";
        string strItsId = "";

        public string LOT_ID 
        { 
            get {return strLotId;}
            set {strLotId = value;} 
        }
        public string MGZ_ID
        {
            get { return strMgzId; }
            set { strMgzId = value; }
        }

        public string ITS_ID
        {
            get { return strItsId; }
            set { strItsId = value; }
        }

        public enum SEQ_LD_MZ_LOT_START
        {
            NONE = 0,

            INIT,
            READY_POS_Y_MOVE,
            READY_POS_Z_MOVE,

            IS_MGZ_LOAD_COMPLETED,
            MGZ_LOT_START,

            CYC_MGZ_COV_LOAD,

            WAIT_MGZ_SUPPLY,

            CYC_MOVE_TO_LOT_START,

            NEW_LOT_START_CONFIRM_CHECK,

            REPORT_MGZ_IN,
            WAIT_MGZ_IN_CONFIRM,
            CHECK_MGZ_IN_CONFIRM,

            AUTO_RECIPE_CHANGE,
            AUTO_RECIPE_CHANGE_FINISH,
          
		  	// Empty tray verify
		  	IS_EMPTY_TRAY,
            EMPTY_TRAY_LOAD_REQ,
            WAIT_NEW_EMPTY_TRAY_LOAD,
            USE_TRAY_REPORT,

            REQUEST_CARRIER_VERIFY,
            WAIT_CARRIER_VERIFY_CONFIRM,
            CHECK_CARRIER_VERIFY_CONFIRM,

            TRAY_VERIFY_CONFIRM_FAIL,
			
			// Lot start info send to vision
            LOT_INFO_SEND_TO_VISION,
            SET_VISION_LOT_START,
            CHECK_VISION_SIG,
            RESET_VISION_LOT_START,

            LOT_END_CHECK, // LOADING RAIL에서 모두 가져가면 
            
            MGZ_LOT_END_START,
            CYC_MOVE_TO_EMPTY_FLOOR, // 3F
            CYC_MGZ_CONV_UNLOAD_3F,

            FINISH
        }



        public enum TACT_CYCLE_UNIT_IDX
        {
            ULD_ELV_MGZ_LOT_START = 0,

            MAX,
        }

        public const int MGZ_WORK_END = 0;
        public const int MGZ_LOT_START_READY = 1;

        public const int MGZ_MGZ_DELETE = 5; // 매거진 제거 
        public const int MGZ_STRIP_PUSH_END = 6; // 매거진 푸셔 동작 상태

        public const int NEW_TRAY_LOAD_REQ = 10;

        public seqLdMzLotStart()
        {
        }

        public override void Init()
        {
            base.Init();
        }
    }

    public class seqStripRail : SeqDefault
    {
        public StripInfo Info = new StripInfo();

        public seqStripRail()
        {

        }

        public bool IS_STRIP
        {
            get { return Info.bIsStrip; }
            set { Info.bIsStrip = value; }
        }

        public override void Init()
        {
            base.Init();

            Info.Clear();
        }

        public void NewStripDataInsert()
        {
            string strInTimes = "";
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleIn.Length; i++)
            {
                strInTimes += Info.dtLastCycleIn[i].ToString("yyyyMMddHHmmssff") + ",";
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strInTimes = strInTimes.TrimEnd(',');
            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.STRIP_RAIL].InsertStripInfo(Info, strInTimes, strOutTimes);
        }

        public void StripDataUpdate()
        {
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleOut.Length; i++)
            {
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.STRIP_RAIL].UpdateStripInfo(Info, strOutTimes);
        }

        /// <summary>
        /// Strip 투입 시간 저장
        /// </summary>
        public virtual void SetProcCycleInTime(int nCycleUnitIdx)
        {
            nCurrCycleNo = nCycleUnitIdx;
            Info.dtLastCycleIn[nCycleUnitIdx] = DateTime.Now;
        }

        /// <summary>
        /// Strip 배출 시간 저장
        /// </summary>
        public virtual void SetProcCycleOutTime(int nCycleUnitIdx)
        {
            Info.dtLastCycleOut[nCycleUnitIdx] = DateTime.Now;
        }

    }

    public class seqStripTransfer : SeqDefault
    {
        public enum SEQ_SAW_STRIP_TRANSFER
        {
            NONE = 0,

            INIT,
            READY_POS_MOVE_Z,
            READY_POS_MOVE_XY,
            CUT_TABLE_RELOAD_CHECK_1,
            IS_MGZ_LOT_START,

            IS_LOAD_STOP,
            CHECK_MGZ_STATUS,
            CUT_TABLE_RELOAD_CHECK_2,
            STRIP_LOAD_COUNT_CHECK,
            // LOT-END, NEXTSEQ(INIT or FINISH)

            IS_STRIP_LOAD_RAIL,

            CYC_STRIP_PUSH_AND_BARCODE,
            CUT_TABLE_RELOAD_CHECK_3,
            IS_UNIT_TRANSFER_LOADING,

            // 여기서 부터 시퀀스 연속 동작
            CYC_STRIP_LOADING,
            CUT_TABLE_RELOAD_CHECK_4,
            CYC_STRIP_LOADING_RAIL_UNGRIP,
            CUT_TABLE_RELOAD_CHECK_5,
            CYC_STRIP_LOADING_RAIL_ALIGN,
            CUT_TABLE_RELOAD_CHECK_6,

            //CYC_STRIP_LOADING_BARCODE,


            CYC_STRIP_PRESS_AND_VACUUM,

            IS_VISION_PROGRAM_READY,
            CYC_STRIP_2D_CODE_READ,
            CUT_TABLE_RELOAD_CHECK_7,
            CYC_STRIP_PRE_ALIGN,
            CUT_TABLE_RELOAD_CHECK_8,
            CUT_TABLE_RELOAD_CONFIRM,

            CYC_STRIP_ORIENT_CHECK,

            REPORT_SUB_LOAD,
            WAIT_SUB_LOAD_CONFIRM,
            CHECK_SUB_LOAD_CONFIRM,

            CYC_STRIP_PICK_UP_FROM_RAIL,
            WAIT_CUTTING_TABLE_LOAD_REQ,

            // TARGET TABLE NO SET
            IS_UNIT_TRANSFER_RUN,

            CYC_STRIP_PLACE_TO_TABLE,
            IS_TABLE_ALIGN_OK,
            CYC_STRIP_RELOAD_TABLE,

            TRANSFER_X_READY_POS_MOVE,
            // NEXTSEQ(IS_LOAD_STOP);

            FINISH,

            STRIP_RELOAD_INTERLOCK,
            CYC_STRIP_RELOAD_TABLE_1,
            CYC_STRIP_RELOAD_TABLE_2,
            CYC_STRIP_RELOAD_TABLE_3,
            CYC_STRIP_RELOAD_TABLE_4,
            CYC_STRIP_RELOAD_TABLE_5,
            CYC_STRIP_RELOAD_TABLE_6,
            CYC_STRIP_RELOAD_TABLE_7,
            CYC_STRIP_RELOAD_TABLE_8,
            CYC_STRIP_RELOAD_TABLE_9,
            CYC_STRIP_RELOAD_TABLE_10,
            CYC_STRIP_RELOAD_TABLE_11,
            CYC_STRIP_RELOAD_TABLE_12,
            CYC_STRIP_RELOAD_TABLE_13,
            CYC_STRIP_RELOAD_TABLE_14,
            CYC_STRIP_RELOAD_TABLE_15,
            CYC_STRIP_RELOAD_TABLE_16,
            CYC_STRIP_RELOAD_TABLE_17,
            CYC_STRIP_RELOAD_TABLE_18,
            CYC_STRIP_RELOAD_TABLE_19,
            CYC_STRIP_RELOAD_TABLE_20,
            CYC_STRIP_RELOAD_TABLE_21,
            CYC_STRIP_RELOAD_TABLE_22,
            CYC_STRIP_RELOAD_TABLE_23,
            CYC_STRIP_RELOAD_TABLE_24,
            CYC_STRIP_RELOAD_TABLE_25,
            CYC_STRIP_RELOAD_TABLE_26,
            CYC_STRIP_RELOAD_TABLE_27,
            CYC_STRIP_RELOAD_TABLE_28,
            CYC_STRIP_RELOAD_TABLE_29,
            CYC_STRIP_RELOAD_TABLE_30,
        }

        public enum TACT_CYCLE_UNIT_IDX
        {
            STRIP_LOADING = 0,
            STRIP_PRE_ALIGN,
            STRIP_READ_2D_CODE,

            CYC_STRIP_PICK_UP_FROM_RAIL,
            CYC_STRIP_PLACE_TO_TABLE,

            MAX,
        }

        public StripInfo Info = new StripInfo();

        public int nMgzLoadSlotNum;
        public int nUnloadingTableNo;
        public bool bIsNotExistStrip;

        public double dAlignXCorrectPreOffset;
        public double dAlignXCorrectCurrOffset;
        public double dAlignYCorrectCurrOffset;

        public const int UNIT_TRANSFER_LOAD_INTERLOCK = 0; //Strip PreAlign 
        public const int UNLOAD_INTERLOCK_T1 = 1;
        public const int UNLOAD_INTERLOCK_T2 = 2;

        public double dReloadCorrOffsetX;
        public seqStripTransfer()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            bIsNotExistStrip = false;

            nMgzLoadSlotNum = 0;
            nUnloadingTableNo = 0;

            dAlignXCorrectPreOffset = 0.0f;
            dAlignXCorrectCurrOffset = 0.0f;

            Info.Clear();
        }
        public void NewStripDataInsert()
        {
            string strInTimes = "";
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleIn.Length; i++)
            {
                strInTimes += Info.dtLastCycleIn[i].ToString("yyyyMMddHHmmssff") + ",";
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strInTimes = strInTimes.TrimEnd(',');
            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.STRIP_TRANSFER].InsertStripInfo(Info, strInTimes, strOutTimes);
        }

        public void StripDataUpdate()
        {
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleOut.Length; i++)
            {
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.STRIP_TRANSFER].UpdateStripInfo(Info, strOutTimes);
        }

        public void DataShiftMgzToLoadingRail()
        {
            GbVar.Seq.sStripRail.Info.bIsStrip = true;
            GbVar.Seq.sStripRail.Info.LOT_ID = GbVar.Seq.sLdMzLoading.LOT_ID;
            GbVar.Seq.sStripRail.Info.ITS_ID = GbVar.Seq.sLdMzLoading.ITS_ID;
            GbVar.Seq.sStripRail.Info.MAGAZINE_SLOT_NO = nMgzLoadSlotNum;
            GbVar.Seq.sStripRail.Info.bIsError = false;


            for (int i = 0; i < GbVar.Seq.sStripRail.Info.UnitArr.Length; i++)
            {
                for (int j = 0; j < GbVar.Seq.sStripRail.Info.UnitArr[i].Length; j++)
                {
                    GbVar.Seq.sStripRail.Info.UnitArr[i][j].IS_UNIT = true;
                    GbVar.Seq.sStripRail.Info.UnitArr[i][j].ITS_XOUT = 1;
                    GbVar.Seq.sStripRail.Info.UnitArr[i][j].TOP_INSP_RESULT = 1;
                    GbVar.Seq.sStripRail.Info.UnitArr[i][j].BTM_INSP_RESULT = 1;
                }
            }

            GbVar.Seq.sStripRail.NewStripDataInsert();
            GbVar.Seq.sStripRail.Info.STRIP_IN_TIME = DateTime.Now;

        }

        public void DataShiftLoadingRailToTranser()
        {
            GbVar.Seq.sStripRail.StripDataUpdate();
            GbVar.Seq.sStripRail.Info.CopyTo(ref Info);
            GbVar.Seq.sStripRail.Info.Clear();

            GbVar.Seq.sStripTransfer.infoProcCyc = GbVar.Seq.sStripRail.infoProcCyc;
            GbVar.Seq.sStripTransfer.NewStripDataInsert();
        }

        public void DataShiftTranserToCuttingTable()
        {
            StripDataUpdate();
            Info.CopyTo(ref GbVar.Seq.sCuttingTable.Info);
            Info.Clear();

            GbVar.Seq.sCuttingTable.infoProcCyc = infoProcCyc;
            GbVar.Seq.sCuttingTable.NewStripDataInsert();
        }

        /// <summary>
        /// Strip 투입 시간 저장
        /// </summary>
        public virtual void SetProcCycleInTime(int nCycleUnitIdx)
        {
            nCurrCycleNo = nCycleUnitIdx;
            Info.dtLastCycleIn[nCycleUnitIdx] = DateTime.Now;
        }

        /// <summary>
        /// Strip 배출 시간 저장
        /// </summary>
        public virtual void SetProcCycleOutTime(int nCycleUnitIdx)
        {
            Info.dtLastCycleOut[nCycleUnitIdx] = DateTime.Now;
        }

    }

    public class seqSawCuttingTable : SeqDefault
    {
        public StripInfo Info = new StripInfo();

        public seqSawCuttingTable()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            Info.Clear();
        }

        public void NewStripDataInsert()
        {
            string strInTimes = "";
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleIn.Length; i++)
            {
                strInTimes += Info.dtLastCycleIn[i].ToString("yyyyMMddHHmmssff") + ",";
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strInTimes = strInTimes.TrimEnd(',');
            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.CUTTING_TABLE].InsertStripInfo(Info, strInTimes, strOutTimes);
        }

        public void StripDataUpdate()
        {
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleOut.Length; i++)
            {
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strOutTimes = strOutTimes.TrimEnd(',');
            GbVar.dbStripInfo[(int)STRIP_MDL.CUTTING_TABLE].UpdateStripInfo(Info, strOutTimes);
        }

        /// <summary>
        /// Strip 투입 시간 저장
        /// </summary>
        public virtual void SetProcCycleInTime(int nCycleUnitIdx)
        {
            nCurrCycleNo = nCycleUnitIdx;
            Info.dtLastCycleIn[nCycleUnitIdx] = DateTime.Now;
        }

        /// <summary>
        /// Strip 배출 시간 저장
        /// </summary>
        public virtual void SetProcCycleOutTime(int nCycleUnitIdx)
        {
            Info.dtLastCycleOut[nCycleUnitIdx] = DateTime.Now;
        }

    }

    public class seqUnitTransfer : SeqDefault
    {
        public enum SEQ_SAW_UNIT_TRANSFER
        {
            NONE = 0,

            INIT,
            READY_POS_MOVE_Z_AXIS,
            READY_POS_MOVE_X_AXIS,

            WAIT_CUTTING_TABLE_LOAD_REQ,
            IS_STRIP_TRANSFER_PLACE_CYCLE, // 안전위치 확인 절대 필요!!!

            CYC_UNIT_PICK_UP_FROM_TABLE,

            SCRAP_1,
            SCRAP_2,

            SPONGE_CLEANING, 

            CHECK_DRY_STAGE_STATUS,

            WATER_JET_CLEANING,

            REQ_UNIT_BTM_DRY,

            CHECK_INTERLOCK,
            UNLOADING_TO_DRY_STAGE,

            REQ_UNIT_TOP_DRY,

            NO_STRIP_READY_POS_MOVE_Z_AXIS,
            NO_STRIP_READY_POS_MOVE_X_AXIS,
            FINISH
        }

        public enum TACT_CYCLE_UNIT_IDX
        {
            CYC_UNIT_PICK_UP_FROM_TABLE = 0,
            CYC_UNIT_CLEANING,

            CYC_UNIT_PLACE_TO_DRY_BLOCK,

            MAX,
        }

        public StripInfo Info = new StripInfo();
        public int nLoadingTableNo;
        public int nUnloadingTableNo; //로딩과 같은 값이지만 인터락 해제할때 유닛피커가 로딩할수 있어서 값을 복사

        public const int LOAD_INTERLOCK_T1 = 1;
        public const int LOAD_INTERLOCK_T2 = 2;

        public const int MAP_TRANSFER_UNLOAD_INTERLOCK_1 = 5;
        public const int MAP_TRANSFER_UNLOAD_INTERLOCK_2 = 6;


        public const int UNIT_BTM_DRY_REQ = 7;

        public const int UNIT_UNLOAD_RUN = 8;
        public const int UNIT_UNLOAD_COMPLETE = 9;

        public const int UNIT_SPONGE_CLEAN = 10;


        public seqUnitTransfer()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            nLoadingTableNo = 0;

            Info.Clear();
        }
        public void NewStripDataInsert()
        {
            string strInTimes = "";
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleIn.Length; i++)
            {
                strInTimes += Info.dtLastCycleIn[i].ToString("yyyyMMddHHmmssff") + ",";
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strInTimes = strInTimes.TrimEnd(',');
            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.UNIT_TRANSFER].InsertStripInfo(Info, strInTimes, strOutTimes);
        }

        public void StripDataUpdate()
        {
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleOut.Length; i++)
            {
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.UNIT_TRANSFER].UpdateStripInfo(Info, strOutTimes);
        }

        public void DataShiftCutTableToTranser()
        {
            GbVar.Seq.sCuttingTable.StripDataUpdate();
            GbVar.Seq.sCuttingTable.Info.CopyTo(ref Info);
            GbVar.Seq.sCuttingTable.Info.Clear();

            infoProcCyc = GbVar.Seq.sCuttingTable.infoProcCyc;
            NewStripDataInsert();
        }

        public void DataShiftTransferToDryStage()
        {
            StripDataUpdate();
            Info.CopyTo(ref GbVar.Seq.sUnitDry.Info);
            Info.Clear();

            GbVar.Seq.sUnitDry.infoProcCyc = infoProcCyc;
            GbVar.Seq.sUnitDry.NewStripDataInsert();
        }
       
        /// <summary>
        /// Strip 투입 시간 저장
        /// </summary>
        public virtual void SetProcCycleInTime(int nCycleUnitIdx)
        {
            nCurrCycleNo = nCycleUnitIdx;
            Info.dtLastCycleIn[nCycleUnitIdx] = DateTime.Now;
        }

        /// <summary>
        /// Strip 배출 시간 저장
        /// </summary>
        public virtual void SetProcCycleOutTime(int nCycleUnitIdx)
        {
            Info.dtLastCycleOut[nCycleUnitIdx] = DateTime.Now;
        }

    }

    public class seqUnitDry : SeqDefault
    {
        public enum SEQ_SOTER_UNIT_DRY
        {
            NONE = 0,

            INIT,
            READY_POS_MOVE_X_AXIS,

            CHECK_UNIT_BTM_DRY_REQ,
            START_UNIT_BTM_DRY,

            LOAD_POS_MOVE_X_AXIS,
            WAIT_UNIT_PICKER_UNLOAD,

            CHECK_UNIT_TOP_DRY_REQ,
            START_UNIT_TOP_DRY,

            REPICK_STEP,

            UNLOAD_POS_MOVE_X_AXIS,
            WAIT_MAP_TRANSFER_LOAD,

            FINISH
        }


        public enum TACT_CYCLE_UNIT_IDX
        {
            CYC_UNIT_CLEANING = 0,
            CYC_UNIT_DRY = 1,
            CYC_UNIT_UNLOAING = 2,

            MAX,
        }

        public StripInfo Info = new StripInfo();

        public const int UNIT_MANUAL_LOAD_SET = 0;

        public const int UNIT_LOADING_READY = 1; //준비된 순간에만 ON
        public const int UNIT_UNLOADING_READY = 2; //준비된 순간에만 ON

        public const int UNIT_TOP_DRY_RUN = 3;

        public seqUnitDry()
        {
        }

        public override void Init()
        {
            base.Init();

            Info.Clear();
        }

        public void NewStripDataInsert()
        {
            string strInTimes = "";
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleIn.Length; i++)
            {
                strInTimes += Info.dtLastCycleIn[i].ToString("yyyyMMddHHmmssff") + ",";
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strInTimes = strInTimes.TrimEnd(',');
            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.UNIT_DRY].InsertStripInfo(Info, strInTimes, strOutTimes);
        }

        public void StripDataUpdate()
        {
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleOut.Length; i++)
            {
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.UNIT_DRY].UpdateStripInfo(Info, strOutTimes);
        }

        public void DataShiftDryBlockToTransfer()
        {
            StripDataUpdate();
            Info.CopyTo(ref GbVar.Seq.sUnitTransfer.Info);
            Info.Clear();

            GbVar.Seq.sUnitTransfer.infoProcCyc = infoProcCyc;
            GbVar.Seq.sUnitTransfer.NewStripDataInsert();
        }
        /// <summary>
        /// Strip 투입 시간 저장
        /// </summary>
        public virtual void SetProcCycleInTime(int nCycleUnitIdx)
        {
            nCurrCycleNo = nCycleUnitIdx;
            Info.dtLastCycleIn[nCycleUnitIdx] = DateTime.Now;
        }

        /// <summary>
        /// Strip 배출 시간 저장
        /// </summary>
        public virtual void SetProcCycleOutTime(int nCycleUnitIdx)
        {
            Info.dtLastCycleOut[nCycleUnitIdx] = DateTime.Now;
        }

    }

    public class seqMapTransfer : SeqDefault
    {
        public enum SEQ_SOTER_MAP_TRANSER
        {
            NONE = 0,

            INIT,
            Z_AXIS_READY_POS_MOVE,
            X_AXIS_READY_POS_MOVE,

            WAIT_DRY_TABLE_LOAD_REQ,
            WAIT_UNIT_LOAD_CONFIRM,
            //CYC_AIR_DRY,//220507 pjh
            CYC_UNIT_PICK_UP_FROM_DRY_BLOCK,

            WAIT_MAP_TABLE_LOADING_READY,
            CYC_UNIT_PLACE_TO_MAP_TABLE,

            WAIT_MAP_TABLE_INSPECTION_READY,

            REPORT_SUB_UNLOAD,
            IS_VISION_PROGRAM_READY,
            CYC_TOP_ALIGN,
            CYC_MAP_TABLE_INSPECTION,
            WAIT_SUB_UNLOAD_CONFIRM,
            CHECK_SUB_UNLOAD_CONFIRM,

            
            MAP_TABLE_UNLOAD_SET,

            FINISH
        }

        public enum TACT_CYCLE_UNIT_IDX
        {
            CYC_UNIT_PICK_UP_FROM_DRY_BLOCK = 0,
            CYC_UNIT_PLACE_TO_MAP_TABLE,

            CYC_MAP_TABLE_INSPECTION,

            MAX,
        }

        public StripInfo Info = new StripInfo();

        public int nNextUnloadingTableNo;
        public int nUnloadingTableNo;

        public int nCurrGroupCount;
        public int nCurrInspCount;

        // Map Inspection 시 일정 수 이상 에러 시 알람 후 재검사 및 진행 여부
        public bool bMapInspDone = false;
        public bool bRetryMapInsp = false;

        public bool bMapAlignDone = false;
        public bool bRetryMapAlign = false;


        public const int UNIT_TOP_DRY_REQ = 0;
        public const int MAP_UNIT_LOAD_RUN = 1;
        public const int MAP_UNIT_LOAD_COMPLETE = 2;

        public seqMapTransfer()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            nNextUnloadingTableNo = 0;
            nUnloadingTableNo = 0;
            nCurrInspCount = 0;

            Info.Clear();
        }

        public void NewStripDataInsert()
        {
            string strInTimes = "";
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleIn.Length; i++)
            {
                strInTimes += Info.dtLastCycleIn[i].ToString("yyyyMMddHHmmssff") + ",";
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strInTimes = strInTimes.TrimEnd(',');
            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.MAP_TRANSFER].InsertStripInfo(Info, strInTimes, strOutTimes);
        }

        public void StripDataUpdate()
        {
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleOut.Length; i++)
            {
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strOutTimes = strOutTimes.TrimEnd(',');
            GbVar.dbStripInfo[(int)STRIP_MDL.MAP_TRANSFER].UpdateStripInfo(Info, strOutTimes);
        }

        public void DataShiftDryTableToTransfer()
        {
            GbVar.Seq.sUnitDry.StripDataUpdate();
            GbVar.Seq.sUnitDry.Info.CopyTo(ref Info);
            GbVar.Seq.sUnitDry.Info.Clear();

            GbVar.Seq.sMapTransfer.infoProcCyc = GbVar.Seq.sUnitDry.infoProcCyc;
            GbVar.Seq.sMapTransfer.NewStripDataInsert();

            GbVar.StripTTLog.AddStripLog(GbVar.Seq.sMapTransfer.Info.STRIP_ID);
        }

        public void DataShiftTransferToMapStage(int nStageNo)
        {
            StripDataUpdate();
            Info.CopyTo(ref GbVar.Seq.sMapVisionTable[nStageNo].Info);

            GbVar.Seq.sMapVisionTable[nStageNo].ReportInfo.LOT_ID = Info.LOT_ID;
            GbVar.Seq.sMapVisionTable[nStageNo].ReportInfo.STRIP_ID = Info.STRIP_ID;
            GbVar.Seq.sMapVisionTable[nStageNo].ReportInfo.MAGAZINE_SLOT_NO = Info.MAGAZINE_SLOT_NO;
            GbVar.Seq.sMapVisionTable[nStageNo].ReportInfo.ITS_ID = Info.ITS_ID;
            GbVar.Seq.sMapVisionTable[nStageNo].ReportInfo.CUTTING_TABLE_NO = Info.CUTTING_TABLE_NO;
            GbVar.Seq.sMapVisionTable[nStageNo].ReportInfo.MAP_TABLE_NO = Info.MAP_TABLE_NO;
            GbVar.Seq.sMapVisionTable[nStageNo].ReportInfo.WorkingData = Info.WorkingData;
            GbVar.Seq.sMapVisionTable[nStageNo].ReportInfo.bIsError = Info.bIsError;
            //Info.CopyTo(ref GbVar.Seq.ReportStripBuffer[nStageNo]);

            Info.Clear();

            GbVar.Seq.sMapVisionTable[nStageNo].infoProcCyc = GbVar.Seq.sMapTransfer.infoProcCyc;
            GbVar.Seq.sMapVisionTable[nStageNo].NewStripDataInsert(nStageNo);
        }

        /// <summary>
        /// Strip 투입 시간 저장
        /// </summary>
        public virtual void SetProcCycleInTime(int nCycleUnitIdx)
        {
            nCurrCycleNo = nCycleUnitIdx;
            Info.dtLastCycleIn[nCycleUnitIdx] = DateTime.Now;
        }

        /// <summary>
        /// Strip 배출 시간 저장
        /// </summary>
        public virtual void SetProcCycleOutTime(int nCycleUnitIdx)
        {
            Info.dtLastCycleOut[nCycleUnitIdx] = DateTime.Now;
        }

    }

    // Seq에는 활용하지 않지만 Data Shift시 필요한 자료구조
    public class seqMapVisionTable : SeqDefault
    {
        public StripInfo Info = new StripInfo();
        public StripInfo ReportInfo = new StripInfo();
        //public int nProcSubCont = 0;

        public bool IsExistSkipUnit = false;

        public int nMapTablePickUpCount;
        
        public const int MAP_TABLE_LOAD_READY = 0;
        public const int MAP_TABLE_LOAD_COMPLETE = 1;
        public const int MAP_TABLE_LOAD_START = 2;

        public const int MAP_TABLE_INSPECTION_END = 5;

        public const int MAP_TABLE_UNLOAD_READY = 10;
        public const int MAP_TABLE_UNLOAD_COMPLETE = 11;

        public const int ALL_CHIP_PLACE_COMPLETE = 13;

        public const int MAP_TABLE_UNLOAD_START = 15;

        public seqMapVisionTable()
        {
            //
        }

        public override void Init()
        {
            base.Init();
            nMapTablePickUpCount = 0;

            Info.Clear();
        }

        public bool IsPickUpEmptyUnit(bool bFDir, int nPickUpCount)
        {
            return false;
        }


        public void NewStripDataInsert(int nTable)
        {
            string strInTimes = "";
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleIn.Length; i++)
            {
                strInTimes += Info.dtLastCycleIn[i].ToString("yyyyMMddHHmmssff") + ",";
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strInTimes = strInTimes.TrimEnd(',');
            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.MAP_VISION_TABLE_1 + nTable].InsertStripInfo(Info, strInTimes, strOutTimes);
        }

        public void StripDataUpdate(int nTable)
        {
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleOut.Length; i++)
            {
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripInfo[(int)STRIP_MDL.MAP_VISION_TABLE_1 + nTable].UpdateStripInfo(ReportInfo, strOutTimes);

            AddStripLog();
        }

        public void AddStripLog()
        {
            string strInTimes = "";
            string strOutTimes = "";

            for (int i = 0; i < Info.dtLastCycleIn.Length; i++)
            {
                strInTimes += Info.dtLastCycleIn[i].ToString("yyyyMMddHHmmssff") + ",";
                strOutTimes += Info.dtLastCycleOut[i].ToString("yyyyMMddHHmmssff") + ",";
            }

            strInTimes = strInTimes.TrimEnd(',');
            strOutTimes = strOutTimes.TrimEnd(',');

            GbVar.dbStripLog.InsertStripInfo(ReportInfo, strInTimes, strOutTimes);
        }

        /// <summary>
        /// Strip 투입 시간 저장
        /// </summary>
        public virtual void SetProcCycleInTime(int nCycleUnitIdx)
        {
            nCurrCycleNo = nCycleUnitIdx;
            Info.dtLastCycleIn[nCycleUnitIdx] = DateTime.Now;
        }

        /// <summary>
        /// Strip 배출 시간 저장
        /// </summary>
        public virtual void SetProcCycleOutTime(int nCycleUnitIdx)
        {
            Info.dtLastCycleOut[nCycleUnitIdx] = DateTime.Now;
        }

    }

    public class seqPkgPickNPlace
    {
        public enum SEQ_SOTER_UNIT_PICK_N_PLACE
        {
            NONE = 0,

            INIT,
            MOVE_TO_PICKER_ALL_ZR_AXIS_PICK_UP_POS,
            IS_OTHER_HEAD_PICK_UP_CYCLE_RUN_BEFORE_STANDBY_POS,
            MOVE_TO_PICKER_X_AXIS_PICK_UP_POS,
            IS_OTHER_HEAD_PICK_UP_CYCLE_RUN_STANDBY_POS,
            MOVE_TO_STAGE_Y_STANDBY_POS,
            IS_IN_STANDBY_POS,

            IS_OTHER_HEAD_PICK_UP_CYCLE_RUN,
            IS_MAP_TABLE_INSPECTION_COMPLETED,
            WAIT_PICKER_PICK_UP_CONFIRM,
            STAGE_VAC_OFF,

            FWD_PICK_UP_REVS_TURN,
            FWD_PICK_UP_REVS_CHECKING,

            IS_F_MAP_TABLE_GROUP_COUNT_OVER,
            IS_F_MAP_TABLE_ARRAY_COUNT_OVER, // ROW 바뀌면  // return PICK_UP_DIR_CHECK
            
            IS_F_PICK_UP_COUNT_OVER,
            IS_F_PICK_UP_SKIP,
            IS_F_MAP_PICK_UP_UNIT_EMPTY,

            CYC_F_UNIT_PICK_UP, // TABLE에 모두 소진이 되었을 때 다 찰 때까지 기다리지 않고 PLACE

            BWD_PICK_UP_REVS_TURN,
            BWD_PICK_UP_REVS_CHECKING,

            IS_R_MAP_TABLE_GROUP_COUNT_OVER,
            IS_R_MAP_TABLE_ARRAY_COUNT_OVER, // ROW 바뀌면  // return PICK_UP_DIR_CHECK
            
            IS_R_PICK_UP_COUNT_OVER,
            IS_R_PICK_UP_SKIP,
            IS_R_MAP_PICK_UP_UNIT_EMPTY,

            CYC_R_UNIT_PICK_UP, // TABLE에 모두 소진이 되었을 때 다 찰 때까지 기다리지 않고 PLACE

            IS_OTHER_HEAD_INSP_CYCLE_RUN,

#if !_NEW_BTM_INSPECTION
            CHECK_VISION_IF_IO_STATUS,//220610
            WAIT_PICKER_INSPECTION_CONFIRM,
            CHECK_BTM_INSPECTION_USE,

            IS_BALL_INSP_READY,

            BALL_INSP_INTF_INIT_REQ,
            BALL_INSP_INTF_INIT_REP,

            PICKER_Z_AXIS_ZERO_POS_CHECK_4_VISION_POS,
            MOVE_TO_PICKER_ALL_ZR_AXIS_VISION_POS,
            MOVE_TO_ALL_XYZ_AXIS_VISION_POS,

            IS_INSP_COUNT_OVER, // 10EA
            IS_INSP_SKIP,
            IS_INSP_UNIT_EMPTY,

            CYC_BTM_VISION_INSPECTION_MOVE,
            BALL_INSP_TRIG_ON,

            REQ_BTM_INSPECTION_COMPLETE,
            WAIT_REPLY_BTM_INSPECTION_COMPLETE,
#else
            CYC_BTM_VISION_INSPECTION,
#endif
            GET_BTM_INSPECTION_RESULT,

            MOVE_TO_PICKER_ALL_ZR_AXIS_GD_UNLOAD_POS,
            IS_OTHER_HEAD_GD_PLACE_CYCLE_RUN,

            WAIT_GODDD_TRAY_TABLE_READY, //(BIN BOX TABLE 포함)
            
            MOVE_TO_PICKER_X_AXIS_GD_UNLOAD_POS,
            WAIT_PICKER_PLACE_CONFIRM,

            //BWD_PLACE_REVS_TURN,
            //BWD_PLACE_REVS_CHECKING,

            IS_GOOD_TRAY_COUNT_OVER,
            IS_GOOD_PLACE_COUNT_OVER,
            IS_GOOD_PLACE_SKIP,
            
            IS_NOT_GOOD_UNIT,
            IS_GOOD_TRAY_UNIT_EMPTY,
            
            // Picker가 가지고 있는 Unit 갯수와 Tray에 Place할 수 있는 갯수를 미리 확인하여 작업해야 할 수 있음.
            CYC_GOOD_UNIT_PLACE, //TABLE에 모두 안착 되고 PICKER에 잔량의 UNIT가 있을 경우 TRAY 교체하여 마져 PLACE 
            
            //FWD_PLACE_REVS_TURN,
            //FWD_PLACE_REVS_CHECKING,

            //IS_F_GOOD_TRAY_COUNT_OVER,
            //IS_F_GOOD_PLACE_COUNT_OVER,
            //IS_F_GOOD_PLACE_SKIP,
            
            //IS_F_NOT_GOOD_UNIT,
            //IS_F_GOOD_TRAY_UNIT_EMPTY,

            // Picker가 가지고 있는 Unit 갯수와 Tray에 Place할 수 있는 갯수를 미리 확인하여 작업해야 할 수 있음.
            //CYC_F_GOOD_UNIT_PLACE, //TABLE에 모두 안착 되고 PICKER에 잔량의 UNIT가 있을 경우 TRAY 교체하여 마져 PLACE 

			// [2022.05.14.kmlee] 오이사님 변경. 순서 변경
            MOVE_TO_PICKER_ALL_ZR_AXIS_RW_UNLOAD_POS,
            IS_OTHER_HEAD_RW_PLACE_CYCLE_RUN,
            WAIT_REWORK_TRAY_TABLE_READY,

            MOVE_TO_PICKER_X_AXIS_RW_UNLOAD_POS,
            IS_REWORK_TRAY_COUNT_OVER,
            
            IS_REWORK_PLACE_COUNT_OVER,
            IS_REWORK_PLACE_SKIP,
            IS_NOT_REWORK_UNIT,
            IS_REWORK_TRAY_UNIT_EMPTY,
            
            // Picker가 가지고 있는 Unit 갯수와 Tray에 Place할 수 있는 갯수를 미리 확인하여 작업해야 할 수 있음.
            CYC_REWORK_UNIT_PLACE, //TABLE에 모두 안착 되고 PICKER에 잔량의 UNIT가 있을 경우 TRAY 교체하여 마져 PLACE 

			// [2022.05.14.kmlee] 오이사님 변경. 순서 변경
            MOVE_TO_PICKER_ALL_ZR_AXIS_NG_POS,
            IS_OTHER_HEAD_NG_PLACE_CYCLE_RUN,
            WAIT_NG_BOX_READY,
            
            MOVE_TO_PICKER_X_AXIS_NG_POS,
            THROW_OUT_TO_REJECT_BOX_1,
            THROW_OUT_TO_REJECT_BOX_2,
            THROW_OUT_TO_REJECT_BOX_3,

            IS_NG_PLACE_COUNT_OVER,
            IS_NG_PLACE_SKIP,
            IS_NOT_NG_UNIT,
            
            // Picker가 가지고 있는 Unit 갯수와 Tray에 Place할 수 있는 갯수를 미리 확인하여 작업해야 할 수 있음.
            CYC_NG_UNIT_PLACE, //TABLE에 모두 안착 되고 PICKER에 잔량의 UNIT가 있을 경우 TRAY 교체하여 마져 PLACE 

            FINISH
        }

        public enum TACT_CYCLE_UNIT_IDX
        {
            CYC_UNIT_PICK_UP = 0,
            CYC_BTM_VISION_INSPECTION,
            CYC_UNIT_PLACE,

            MAX,
        }
        public const int SYNC_PICK_UP_READY = 0;
        public const int SYNC_PICK_UP_START = 1;
        public const int SYNC_INSPECTION_START = 2;

        public const int SYNC_GD_PLACE_START = 3;
        public const int SYNC_RW_PLACE_START = 4;
        public const int SYNC_NG_PLACE_START = 5;

        public int nPickUpMapTableNo;
        public int nPickUpMapTableGroupCnt;
        public int nPlaceToGoodTableNo;

        public PickerHeadInfo[] pInfo = new PickerHeadInfo[CFG_DF.MAX_PICKER_HEAD_CNT];

        public seqPkgPickNPlace()
        {
            pInfo[CFG_DF.HEAD_1] = new PickerHeadInfo();
            pInfo[CFG_DF.HEAD_2] = new PickerHeadInfo(); 
        }
    }

    public class PickerHeadInfo : SeqDefault
    {
        public List<int> lstPickUpPicker = new List<int>();
        public List<int> lstPlacePicker = new List<int>();
        public bool bRevPickUp;
        public bool bRevPlace;

        public int nFwdPickupCount;
        public int nBwdPickupCount;

        public int nPickUpColCount;
        //public int nGoodPlaceColCount;
        //public int nReworkPlaceColCount;

		// [2022.05.14.kmlee] 오이사님 변경
        public bool bGDFirstPlace;
        public bool bRWFirstPlace;

        public int nFwdPlaceCount;
        public int nBwdPlaceCount;

        public int nCurrInspCount;

        public UnitInfo[] unitPickUp = new UnitInfo[CFG_DF.MAX_PICKER_PAD_CNT];

        public bool isExistSkipUnit = false;

        public PickerHeadInfo()
        {
            for (int cnt = 0; cnt < CFG_DF.MAX_PICKER_PAD_CNT; cnt++)
            {
                unitPickUp[cnt] = new UnitInfo();

                unitPickUp[cnt].ITS_XOUT = 1;
                unitPickUp[cnt].TOP_INSP_RESULT = -1;
                unitPickUp[cnt].BTM_INSP_RESULT = -1;
            }
        }

        public void ResetPickerHeadInfo()
        {
            for (int cnt = 0; cnt < CFG_DF.MAX_PICKER_PAD_CNT; cnt++)
            {  
                unitPickUp[cnt].IS_UNIT = false;//220505 pjh 스트립 배열에 따라 place버그 있어서 추가
                unitPickUp[cnt].ITS_XOUT = 1;
                unitPickUp[cnt].TOP_INSP_RESULT = -1;
                unitPickUp[cnt].BTM_INSP_RESULT = -1;
            }
        }
        public void PickerUpCountClear()
        {
            bRevPickUp = false;
            bRevPlace = false;

            nFwdPickupCount = 0;
            nBwdPickupCount = 0;

            nPickUpColCount = 0;

            lstPickUpPicker.Clear();
        }

        public void InspectionCountClear()
        {
            nCurrInspCount = 0;
        }

        public void PlaceCountClear()
        {
            nFwdPlaceCount = 0;
            nBwdPlaceCount = 0;

            bGDFirstPlace = false;
            bRWFirstPlace = false;

            lstPlacePicker.Clear();
        }

        public override void Init()
        {
            base.Init();

            bRevPickUp = false;
            bRevPlace = false;

            nFwdPickupCount = 0;
            nBwdPickupCount = 0;

            nPickUpColCount = 0;

            nFwdPlaceCount = 0;
            nBwdPlaceCount = 0;

            nCurrInspCount = 0;
        }

        public bool IsPickerAllEmptyUnit()
        {
            for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
            {
                if (unitPickUp[nCnt].IS_UNIT == true) return false;
            }

            return true;
        }

        public bool IsPickerEmptyUnit(int nPickUpCount)
        {
            if (unitPickUp[nPickUpCount].IS_UNIT == false) return true;
            return false;
        }

        public bool IsGoodUnit(bool bFDir, int nPlaceCount)
        {
            int nItsCode;

            if (bFDir)
            {
                nItsCode = unitPickUp[nPlaceCount].ITS_XOUT;
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_XOUT_USE].bOptionUse)
                {
                    nItsCode = 1;
                }

                if (IsPickerEmptyUnit(nPlaceCount))
                    return false;

                if (unitPickUp[nPlaceCount].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK &&
                    unitPickUp[nPlaceCount].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK &&
                    nItsCode != 2 &&
                    nItsCode != (int)MESDF.eCELL_STATUS.XOUT)
                {
                    return true;
                }
            }
            else
            {
                int nRvsPlaceCnt = CFG_DF.MAX_PICKER_PAD_CNT - nPlaceCount - 1;
                nItsCode = unitPickUp[nRvsPlaceCnt].ITS_XOUT;
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_XOUT_USE].bOptionUse)
                {
                    nItsCode = 1;
                }

                if (IsPickerEmptyUnit(nRvsPlaceCnt))
                    return false;

                if (unitPickUp[nRvsPlaceCnt].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK &&
                    unitPickUp[nRvsPlaceCnt].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK &&
                    nItsCode != 2 &&
                    nItsCode != (int)MESDF.eCELL_STATUS.XOUT)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// 220507 판정 식 수정
        /// </summary>
        /// <param name="nPlaceCount"></param>
        /// <returns></returns>
        public bool IsReworkUnit(int nPlaceCount)
        {
            int nRvsPickUpCnt = CFG_DF.MAX_PICKER_PAD_CNT - nPlaceCount - 1;
            int nItsCode = unitPickUp[nRvsPickUpCnt].ITS_XOUT;

            if (IsPickerEmptyUnit(nRvsPickUpCnt)) return false;
            
            if (nItsCode != (int)MESDF.eCELL_STATUS.XOUT)
            {
                if (unitPickUp[nRvsPickUpCnt].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.RW ||
                    unitPickUp[nRvsPickUpCnt].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.RW)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsNGUnit(int nPlaceCount)
        {
            int nHostCode = unitPickUp[nPlaceCount].ITS_XOUT;

            if (IsPickerEmptyUnit(nPlaceCount)) return false;

            // 이 조건문까지 온 것은 SQ, NQ 외 조건으로 여기서 다 버리지 않으면 다음 PICK-UP시 문제가 될 수 있음...
            //if (unitPickUp[nPlaceCount].TOP_INSP_RESULT >= (int)VSDF.eJUDGE_MAP.NG ||
            //    unitPickUp[nPlaceCount].BTM_INSP_RESULT >= (int)VSDF.eJUDGE_BALL.NG ||
            //    nHostCode == (int)MESDF.eCELL_STATUS.NG_2)
            //{
            //    return true;
            //}

            return true;
        }

        public bool IsPickerReworkUnitExist()
        {
            int nReworkCnt = 0;
            int nHostCode;

            for (int cnt = 0; cnt < CFG_DF.MAX_PICKER_PAD_CNT; cnt++)
            {
                if (!IsPickerEmptyUnit(cnt))
                {
                    nHostCode = unitPickUp[cnt].ITS_XOUT;
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
                    {
                        nHostCode = 1;
                    }

                    if (nHostCode == 2)
                    {
                        if (unitPickUp[cnt].TOP_INSP_RESULT != (int)VSDF.eJUDGE_MAP.NG &&
                            unitPickUp[cnt].BTM_INSP_RESULT != (int)VSDF.eJUDGE_BALL.NG)
                        {
                            nReworkCnt++;
                        }
                    }
                    else if (nHostCode == 1)
                    {
                        if (unitPickUp[cnt].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.RW ||
                            unitPickUp[cnt].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.RW)
                        {
                            nReworkCnt++;
                        }
                    }
                }
            }

            // 일단 주석 나중에 Code에 따라 분류하는 편집기능이 완료한 뒤 적용 필요
            if (nReworkCnt > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsPickerNGUnitExist()
        {
            int nNGCnt = 0;
            int nItsCode;

            for (int cnt = 0; cnt < CFG_DF.MAX_PICKER_PAD_CNT; cnt++)
            {
                nItsCode = unitPickUp[cnt].ITS_XOUT;
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_XOUT_USE].bOptionUse)
                {
                    nItsCode = 1;
                }

                if (unitPickUp[cnt].TOP_INSP_RESULT >= (int)VSDF.eJUDGE_MAP.NG ||
                    unitPickUp[cnt].BTM_INSP_RESULT >= (int)VSDF.eJUDGE_BALL.NG ||
                    (nItsCode != (int)MESDF.eCELL_STATUS.OK))
                {
                    nNGCnt++;
                }
            }

            // 일단 주석 나중에 Code에 따라 분류하는 편집기능이 완료한 뒤 적용 필요
            if (nNGCnt > 0)
            {
                return true;
            }

            return false;
        }
    }

    public class seqUldGoodTrayTable : SeqDefault
    {
        public TrayInfo Info = new TrayInfo();

        public int nUnitPlaceCount;

        public int nUnloadingTableNo;
        public int nMeasureCount;
        public bool bTrayExist = false;


        public const int TRAY_SORT_OUT_READY = 0;
        public const int TRAY_UNIT_FULL = 1;

        public const int TRAY_LOADING_READY = 10;   // 트레이 공급 완료 플래그
        public const int TRAY_LOADING_COMPLETE = 11;
        public const int TRAY_UNLOADING_READY = 12;
        public const int TRAY_UNLOADING_COMPLETE = 13;

        public const int TRAY_SORT_OUT_START = 15;
        //public const int TRAY_LOT_END = 16;

        public const int TRAY_VISION_START = 17;

        public const int TRAY_NOT_EXIST = 20;

        //sj.shin 시뮬레이션용 으로 변수 생성
        public bool smTableUP = false;
        public seqUldGoodTrayTable()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            nUnitPlaceCount = 0;
            nUnloadingTableNo = 0;

            bTrayExist = false;
        }
    }

    public class seqUldReworkTrayTable : SeqDefault
    {
        public TrayInfo Info = new TrayInfo();

        public int nUnitPlaceCount;
        public bool bTrayExist = false;

        public const int TRAY_SORT_OUT_READY = 0;
        public const int TRAY_UNIT_FULL = 1;
        public const int TRAY_LOT_END = 2;

        public const int TRAY_LOADING_READY = 10;
        public const int TRAY_LOADING_COMPLETE = 11;
        public const int TRAY_UNLOADING_READY = 12;
        public const int TRAY_UNLOADING_COMPLETE = 13;

        public const int TRAY_SORT_OUT_START = 15;

        public seqUldReworkTrayTable()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            nUnitPlaceCount = 0;
            bTrayExist = false;
        }
    }

    public class seqUldRejectBoxTable : SeqDefault
    {
        //이름과 함께 수량이    필요함.
        public int[] Loss_Qty = new int[4];

        public bool bIsBoxFull;
        public bool bIsBoxLoad;

        public seqUldRejectBoxTable()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            bIsBoxFull = false;
            bIsBoxLoad = false;

            Loss_Qty = new int[4];
        }

        public void Clear()
        {
            for (int i = 0; i < 4; i++)
			{
                Loss_Qty[i] = 0;
			}


        }

    }

    public class seqUldTrayTransfer : SeqDefault
    {
        public enum SEQ_SOTER_ULD_TRAY_TRANSFER
        {
            NONE = 0,

            INIT,
            READY_POS_MOVE_Z_AXIS,
            READY_POS_MOVE_Y_AXIS,
            READY_POS_MOVE_X_AXIS,
            START,

            // EMPTY TRAY #1, #2 둘 중 준비되어 있는 곳부터
            // EMPTY TRAY 모두 다 꺼냈으면 OHT 요청하게
            IS_TRAY_TABLE_1_LOAD_REQ, // GOOD1
            IS_TRAY_TABLE_2_LOAD_REQ, // GOOD2
            IS_TRAY_TABLE_3_LOAD_REQ, // REWORK

            IS_TRAY_1_UNLOAD_REQ, // GOOD1
            IS_TRAY_2_UNLOAD_REQ, // GOOD2
            IS_TRAY_3_UNLOAD_REQ, // REWORK

            IS_TRAY_1_COVER_LOAD_REQ, //GOOD1
            IS_TRAY_2_COVER_LOAD_REQ, //GOOD2
            IS_TRAY_3_COVER_LOAD_REQ, //REWORK

            CYC_EMPTY_TRAY_LOAD_FOR_GD1_TABLE,
            CYC_TRAY_UNLOAD_TO_TABLE_GD_1,

            CYC_EMPTY_TRAY_LOAD_FOR_GD2_TABLE,
            CYC_TRAY_UNLOAD_TO_TABLE_GD_2,

            CYC_EMPTY_TRAY_LOAD_FOR_RW_TABLE,
            CYC_TRAY_UNLOAD_TO_TABLE_RW,

            CYC_TRAY_LOAD_TO_TABLE_GD_1,
            CYC_TRAY_LOAD_TO_TABLE_GD_2,

            WAIT_GD_ELV_READY,
            CYC_TRAY_UNLOAD_TO_GOOD_ELV,
            CYC_TRAY_UNLOAD_TO_GOOD_1_ELV,
            CYC_TRAY_UNLOAD_TO_GOOD_2_ELV,
            TRAY_UNLOAD_GOOD_ELV_TRAY_IN_CHECKED,

            CYC_TRAY_LOAD_TO_TABLE_RW,
            WAIT_RW_ELV_READY,
            CYC_TRAY_UNLOAD_TO_REWORK_ELV,
            TRAY_UNLOAD_REWORK_ELV_TRAY_IN_CHECKED,

            CYC_TRAY_LOAD_FOR_GD1_ELV_COVER,
            CYC_TRAY_UNLOAD_TO_GD1_ELV_COVER,

            CYC_TRAY_LOAD_FOR_GD2_ELV_COVER,
            CYC_TRAY_UNLOAD_TO_GD2_ELV_COVER,

            CYC_TRAY_LOAD_FOR_REWORK_ELV_COVER,
            CYC_TRAY_UNLOAD_TO_REWORK_ELV_COVER,

            FINISH
        }

        public TrayInfo Info = new TrayInfo();
        public string CARRIER_2D_CODE = "";
        string strCoverRfid = "";
        //public const int LOT_END_TRAY = 0;
        //public const int GOOD_TRAY_OUT_COMPLETE = 1;
        //public const int REWORK_TRAY_OUT_COMPLETE = 2;
        public int nLoadElvNo;
        public int nLoadTableNo;

        public int nUnloadElvNo;
        public int nUnloadTableNo;

        public int nWorkEmptyTrayNo;

        public const int MAP_TRANSFER_INTERLOCK_START = 1;
        public const int TRAY_UNLOAD_RUN = 10;
        public bool bTrayPopErrSelectNewTray = false; // 트레이 알람 발생 시 팝업창에서 신규 트레이 투입 FLAG


        public string READ_RFID
        {
            get { return strCoverRfid; }
            set { strCoverRfid = value; }
        }

        public seqUldTrayTransfer()
        {
            //
        }

        public void DataAddToGdElv(int nStage, int nElv)
        {
            //GbVar.Seq.sUldGDTrayTable[nStage].Info.CopyTo(ref GbVar.Seq.sUldTrayTransfer.Info);

            TrayInfo AddInfo = new TrayInfo();
            AddInfo = GbVar.Seq.sUldGDTrayTable[nStage].Info.Clone();
            GbVar.Seq.sUldTrayElvGood[nElv].Info.Add(AddInfo);
        }

        public void DataAddToRwElv()
        {
            TrayInfo AddInfo = new TrayInfo();
            AddInfo = GbVar.Seq.sUldRWTrayTable.Info.Clone();
            GbVar.Seq.sUldTrayElvRework.Info.Add(AddInfo);
        }

        public override void Init()
        {
            base.Init();

            nLoadElvNo = 0;
            nLoadTableNo = 0;

            nUnloadElvNo = 0;
            nUnloadTableNo = 0;

            nWorkEmptyTrayNo = 0;
        }

        public void Clear()
        {
            Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);
            Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
        }
        
    }

    public class seqUldTrayElvGood : SeqDefault
    {
        public List<TrayInfo> Info = new List<TrayInfo>();
        public TrayBatchInfo BatchInfo = new TrayBatchInfo();

        public enum SEQ_SOTER_ULD_ELV_GOOD
        {
            NONE = 0,
            INIT,

            TRAY_LOAD_READY,
            //GUIDE_CYL_CLOSE,

            //INIT_STOPPER_OPEN,
            //INIT_SIGNAL_SEARCH,
            //ELV_WORK_SAFETY_POS_MOVE,
            //RECV_STOPPER_CLOSE,

            WAIT_TRAY_IN_TRANSFER,

            TRAY_LOAD,
            //WORK_OFFSET_MOVE,
            //RECV_STOPPER_OPEN,
            //SAFETY_OFFSET_MOVE,
            WAIT_TRAY_COVER,
            TRAY_UNLOAD,
            //LOT_END_OFFSET_MOVE,
            //LOT_END_STOPPER_OPEN,

            //GUIDE_CYL_OPEN,
            //ELV_MOVE_TO_PRE_SAFETY_POS,
            //ELV_MOVE_TO_SAFETY_POS,

            //ELV_MOVE_TO_READY_POS,


            TRAY_OUT,
            //CHECK_OUT_TRAY_EXIST,
            //ULD_CONV_GUIDE_CLOSE,
            //ULD_CONV_CW,
            //WAIT_TRAY_ARRIVAL,
            //ULD_CONV_STOP,

            FINISH
        }


        public const int TRAY_IN_READY = 0;
        public const int TRAY_IN_COMP = 1;
        public const int TRAY_TRANSFER_PLACE_PROCESS = 2;   // 트레이 트랜스퍼가 트레이를 엘리베이터에 플레이스 진행중인 플래그
        public const int TRAY_TRANSFER_PICKUP_PROCESS = 3;  // 트레이 트랜스퍼가 트레이를 엘리베이터에 픽업 진행중인 플래그

        public const int TRAY_COVER_READY = 5;
        public const int TRAY_COVER_COMP = 6;

        public const int LOT_END_CHECKED = 11;

        public const int TRAY_EXIST = 20; //20230426 choh : 트레이 엘리베이터에 트레이가 감지된 상태로 시작하면

        public int nTrayInCount;

        public void DataClear()
        {
            this.Info.Clear();
        }
        public string GetTrayIdList()
        {
            string TrayIdList = "";
            try
            {
                for (int i = 0; i < Info.Count; i++)
                {
                    TrayIdList += Info[i].Tray_2D_Code + ",";
                }

                TrayIdList = TrayIdList.Length > 0 ? TrayIdList.Substring(0, TrayIdList.Length - 1) : "";

            }
            catch (Exception)
            {
            }
            return TrayIdList;
        }

        public seqUldTrayElvGood()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            Info.Clear();
        }

        public void Clear()
        {
            Info.Clear();
        }
    }

    public class seqUldTrayElvRework : SeqDefault
    {
        public List<TrayInfo> Info = new List<TrayInfo>();
        public TrayBatchInfo BatchInfo = new TrayBatchInfo();

        public enum SEQ_SOTER_ULD_ELV_REWORK
        {
            NONE = 0,
            INIT,

            TRAY_LOAD_READY,
            //GUIDE_CYL_CLOSE,

            //INIT_STOPPER_OPEN,
            //INIT_SIGNAL_SEARCH,
            //ELV_WORK_SAFETY_POS_MOVE,

            //IS_REWORK_TRAY_LOT_END,
            //RECV_STOPPER_CLOSE,

            WAIT_TRAY_IN_TRANSFER,

            TRAY_LOAD,
            //WORK_OFFSET_MOVE,
            //RECV_STOPPER_OPEN,
            //SAFETY_OFFSET_MOVE,

            WAIT_TRAY_COVER,
            TRAY_UNLOAD,
            //LOT_END_OFFSET_MOVE,
            //LOT_END_STOPPER_OPEN,

            //GUIDE_CYL_OPEN,
            //ELV_MOVE_TO_PRE_SAFETY_POS,
            //ELV_MOVE_TO_SAFETY_POS,

            //ELV_MOVE_TO_READY_POS,

            TRAY_OUT,
            //CHECK_OUT_TRAY_EXIST,
            //ULD_CONV_GUIDE_CLOSE,
            //ULD_CONV_CW,
            //WAIT_TRAY_ARRIVAL,
            //ULD_CONV_STOP,

            FINISH
        }



        public const int TRAY_IN_READY = 0;
        public const int TRAY_IN_COMP = 1;
        public const int TRAY_TRANSFER_PLACE_PROCESS = 2;   // 트레이 트랜스퍼가 트레이를 엘리베이터에 플레이스 진행중인 플래그
        public const int TRAY_TRANSFER_PICKUP_PROCESS = 3;  // 트레이 트랜스퍼가 트레이를 엘리베이터에 픽업 진행중인 플래그

        public const int TRAY_COVER_READY = 5;
        public const int TRAY_COVER_COMP = 6;

        public const int LOT_END_CHECKED = 11;

        public const int TRAY_EXIST = 20; //20230426 choh : 트레이 엘리베이터에 트레이가 감지된 상태로 시작하면


        public int nTrayInCount;
        public string GetTrayIdList()
        {
            string TrayIdList = "";
            try
            {
                for (int i = 0; i < Info.Count; i++)
                {
                    TrayIdList += Info[i].Tray_2D_Code + ",";
                }

                TrayIdList = TrayIdList.Length > 0 ? TrayIdList.Substring(0, TrayIdList.Length - 1) : "";

            }
            catch (Exception)
            {
            }
            return TrayIdList;
        }

        public seqUldTrayElvRework()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            Info.Clear();
        }

        public void Clear()
        {
            Info.Clear();
        }
    }

    public class seqUldTrayElvEmpty : SeqDefault
    {
		public TrayBatchInfo BatchInfo = new TrayBatchInfo();
		
        public enum SEQ_SOTER_ULD_ELV_EMPTY
        {
            NONE = 0,
            INIT,

            GUIDE_CYL_CLOSE,
            CHECK_TRAY_QTY_OVER,//220608
            INIT_SIGNAL_SEARCH,
            IS_EXIST_TRAY,
            
            WORK_OFFSET_MOVE,
            CHECK_TRAY_OUT_READY,//220610우선 캐이스만 만들어놓음
            WAIT_TRAY_OUT_TO_TRANSFER,

            ELV_RESEARCH_OFFSET_MOVE,
            GUIDE_CYL_OPEN,

            ELV_MOVE_TO_READY_POS,

            EMPTY_TRAY_LOAD_REQ,

            TRAY_IN,

            TRAY_OUT,

            FINISH
        }

        public int nTrayCount;

        public const int EMPTY_TRAY_READY = 0;
        public const int TRAY_OUT_COMP = 1;
        public const int TRAY_TRANSFER_PLACE_PROCESS = 2;   // 트레이 트랜스퍼가 트레이를 엘리베이터에 플레이스 진행중인 플래그
        public const int TRAY_TRANSFER_PICKUP_PROCESS = 3;  // 트레이 트랜스퍼가 트레이를 엘리베이터에 픽업 진행중인 플래그

        public const int TRAY_COVER_READY = 5;
        public const int TRAY_COVER_COMP = 6;

        public const int TRAY_RETURN_BACK = 7;

        public const int LOT_END_CHECKED = 11;
        public const int LOT_END_DONE = 12;

        bool IsTray = false;
        public bool IS_TRAY
        {
            get { return IsTray; }
            set { IsTray = value; }
        }

        public seqUldTrayElvEmpty()
        {
            //
        }

        public override void Init()
        {
            base.Init();
        }
    }

    public class seqUldTrayElvCover : SeqDefault
    {
        public TrayBatchInfo BatchInfo = new TrayBatchInfo();
        public enum SEQ_SOTER_ULD_ELV_COVER
        {
            NONE = 0,
            INIT,

            INIT_SIGNAL_SEARCH,
            IS_EXIST_TRAY, // 이 조건은 현장에서 파악이 필요

            WORK_OFFSET_MOVE,
            WAIT_TRAY_TRANSFER,
            ELV_RESEARCH_OFFSET_MOVE,

            GUIDE_CYL_OPEN,
            ELV_MOVE_TO_READY_POS,
            EMPTY_TRAY_LOAD_REQ,
            IS_EMPTY_TRAY_ARRIVED,

            GUIDE_CYL_CLOSE,
            ELV_MOVE_TO_PRE_SAFETY_POS,
            ELV_MOVE_TO_SAFETY_POS,

            FINISH
        }

        public int nCoverTrayCount;

        public const int COVER_TRAY_READY = 0;

        public const int COVER_TRAY_IN_COMP = 5;
        public const int COVER_TRAY_OUT_COMP = 6;

        public const int OHT_COVER_IN_REQ = 10;
        public const int OHT_COVER_IN_COMP = 11;

        public seqUldTrayElvCover()
        {
            //
        }

        public override void Init()
        {
            base.Init();

            nCoverTrayCount = 0;
        }
    }

    public class mdProductCntInfo
    {
        public bool bIn = false;
        public bool bMcrOk = false;
        public bool bMcrMisMatch = false;
        public bool bMcrNg = false;
        public bool bMcrNull = false;
        
        public bool bInspNg = false;
        public bool bOut = false;
        public bool bSkipCell = false;

        public void Copy(mdProductCntInfo md)
        {
            this.bIn = md.bIn;
            this.bInspNg = md.bInspNg;
            this.bMcrMisMatch = md.bMcrMisMatch;
            this.bMcrNull = md.bMcrNull;
            this.bMcrNg = md.bMcrNg;
            this.bMcrOk = md.bMcrOk;
            this.bOut = md.bOut;
            this.bSkipCell = md.bSkipCell;
        }
    }

    public class WorkInspectionInfo
    {
        //

        public WorkInspectionInfo()
        {
        }

        public void Init()
        {
            //
        }
    }
	
    public class SeqShared
    {
        public bool bFirstAutoStart = false;

        // 로더 추가
        public seqLoader_LdConv sLoaderLdConv = new seqLoader_LdConv();
        public seqLoader_UldConv sLoaderUldConv = new seqLoader_UldConv();

        // sLoader_Transfer은 사용하지 않고 sLdMzLoading으로 사용 함
        //public seqLoader_LotStart sLoader_LotStart = new seqLoader_LotStart();
        //public seqLoader_Transfer sLoader_Transfer = new seqLoader_Transfer();

        public seqLdMzLoading sLdMzLoading = new seqLdMzLoading();
        public seqLdMzLotStart sLdMzLotStart = new seqLdMzLotStart();

        public seqStripRail sStripRail = new seqStripRail();
        public seqStripTransfer sStripTransfer = new seqStripTransfer();
        public seqUnitTransfer sUnitTransfer = new seqUnitTransfer();

        public seqSawCuttingTable sCuttingTable = new seqSawCuttingTable();

        public seqUnitDry sUnitDry = new seqUnitDry();

        public seqMapTransfer sMapTransfer = new seqMapTransfer();
        public seqMapVisionTable[] sMapVisionTable = new seqMapVisionTable[2];

        public seqPkgPickNPlace sPkgPickNPlace = new seqPkgPickNPlace();

        public seqUldGoodTrayTable[] sUldGDTrayTable = new seqUldGoodTrayTable[2];
        public seqUldReworkTrayTable sUldRWTrayTable = new seqUldReworkTrayTable();
        public seqUldRejectBoxTable sUldRjtBoxTable = new seqUldRejectBoxTable();

        public seqUldTrayTransfer sUldTrayTransfer = new seqUldTrayTransfer();

        public seqUldTrayElvGood[] sUldTrayElvGood = new seqUldTrayElvGood[2];
        public seqUldTrayElvRework sUldTrayElvRework = new seqUldTrayElvRework();
        public seqUldTrayElvEmpty[] sUldTrayElvEmpty = new seqUldTrayElvEmpty[2];

        //public seqCleaning[] sCleaning = new seqCleaning[3];
        public bool[] bTrayUnitMisPlace = new bool[2];

        public bool bLdMzReady;
        public bool bUldMzReady;

        public int nInspectionWorkIdx = 0;

        public bool bWorkEndReserve = false;
        public bool bIsLotEndRun = false;
        public bool bLotEndReserve = false;
        public int nLotPlaceCount;
        public bool bAutoLotEndCheck = false;
        // 소터에 자재가 없을 때 Good 트레이 배출 플래그
        public bool bGoodTrayOut = false;

        // 소터에 자재가 업을 때 Rework 트레이 배출 플래그
        public bool bReworkTrayOut = false;

        // 빈 트레이 엘리베이터 배출 플래그
        public bool[] bEmptyElvOut = new bool[2];

        public bool bOHTUsed;
        public bool bMgzLoadStop;

        public bool bOneStripLoad;

        public bool[] bAlignTeachStop = new bool[2];
        public bool[] bCleanTeachStop = new bool[2];

        public bool[] bTopVisionTeachStop = new bool[2];
        public bool[] bBtmVisionTeachStop = new bool[2];
        public bool[] bTrayVisionTeachStop = new bool[2];

        // Lot 시작 시간. Inspection Image 저장 시 폴더 구분을 두기 위해
        public string strStartTime = "";

        public SeqShared()
        {
            // 로더 컨베이어 설정
            sLoaderLdConv = new seqLoader_LdConv();
            sLoaderUldConv = new seqLoader_UldConv();

            sMapVisionTable[0] = new seqMapVisionTable();
            sMapVisionTable[1] = new seqMapVisionTable();

            sUldTrayElvGood[0] = new seqUldTrayElvGood();
            sUldTrayElvGood[1] = new seqUldTrayElvGood();

            sUldTrayElvEmpty[0] = new seqUldTrayElvEmpty();
            sUldTrayElvEmpty[1] = new seqUldTrayElvEmpty();

            sUldGDTrayTable[0] = new seqUldGoodTrayTable();
            sUldGDTrayTable[1] = new seqUldGoodTrayTable();

            //for (int nIdx = 0; nIdx < sCleaning.Length; nIdx++)
            //{
            //    sCleaning[nIdx] = new seqCleaning();
            //}

            bTrayUnitMisPlace[0] = false;
            bTrayUnitMisPlace[1] = false;
        }

        public void Init()
        {
            bLdMzReady = false;
            bUldMzReady = false;

            nInspectionWorkIdx = 0;

            bWorkEndReserve = false;
            bIsLotEndRun = false;
            bLotEndReserve = false;
            
            nLotPlaceCount = 0;

            sLdMzLoading.Init();
            sLdMzLotStart.Init();

            sLoaderLdConv.Init();
            sLoaderUldConv.Init();

            sStripRail.Init();
            sStripTransfer.Init();
            sUnitTransfer.Init();

            sCuttingTable.Init();

            sUnitDry.Init();

            sMapTransfer.Init();
            sMapVisionTable[0].Init();
            sMapVisionTable[1].Init();

            sPkgPickNPlace.pInfo[0].Init();
            sPkgPickNPlace.pInfo[1].Init();

            sUldGDTrayTable[0].Init();
            sUldGDTrayTable[1].Init();

            sUldRWTrayTable.Init();
            sUldRjtBoxTable.Init();

            sUldTrayTransfer.Init();

            sUldTrayElvGood[0].Init();
            sUldTrayElvGood[1].Init();

            sUldTrayElvRework.Init();

            sUldTrayElvEmpty[0].Init();
            sUldTrayElvEmpty[1].Init();

            //sCleaning[0].Init();
            //sCleaning[1].Init();
            //sCleaning[2].Init();

            sLdMzLoading.ProcCycleAllEnd();
            sLdMzLotStart.ProcCycleAllEnd();

            sLoaderLdConv.ProcCycleAllEnd();
            sLoaderUldConv.ProcCycleAllEnd();

            sStripRail.ProcCycleAllEnd();
            sStripTransfer.ProcCycleAllEnd();
            sUnitTransfer.ProcCycleAllEnd();

            sCuttingTable.ProcCycleAllEnd();

            sUnitDry.ProcCycleAllEnd();

            sMapTransfer.ProcCycleAllEnd();
            sMapVisionTable[0].ProcCycleAllEnd();
            sMapVisionTable[1].ProcCycleAllEnd();

            sPkgPickNPlace.pInfo[0].ProcCycleAllEnd();
            sPkgPickNPlace.pInfo[1].ProcCycleAllEnd();

            sUldGDTrayTable[0].ProcCycleAllEnd();
            sUldGDTrayTable[1].ProcCycleAllEnd();

            sUldRWTrayTable.ProcCycleAllEnd();
            sUldRjtBoxTable.ProcCycleAllEnd();

            sUldTrayTransfer.ProcCycleAllEnd();

            sUldTrayElvGood[0].ProcCycleAllEnd();
            sUldTrayElvGood[1].ProcCycleAllEnd();
            sUldTrayElvRework.ProcCycleAllEnd();
            sUldTrayElvEmpty[0].ProcCycleAllEnd();
            sUldTrayElvEmpty[1].ProcCycleAllEnd();

            //sCleaning[0].ProcCycleAllEnd();
            //sCleaning[1].ProcCycleAllEnd();
            //sCleaning[2].ProcCycleAllEnd();
        }
        public void TrayInit() // 20220620 pending item list Loader부분 2번 UI리셋 버튼 추가 _KTH
        {
            GbVar.Seq.sUldTrayElvGood[0].nTrayInCount = 0;
            GbVar.Seq.sUldTrayElvGood[1].nTrayInCount = 0;
            GbVar.Seq.sUldTrayElvRework.nTrayInCount = 0;
            GbVar.Seq.sUldTrayElvEmpty[0].nTrayCount = 0;
            GbVar.Seq.sUldTrayElvEmpty[1].nTrayCount = 0;
        }
    }

    public class ProductInfo
    {
        public Int64 nRunTime;
        public Int64 nStopTime;
        public Int64 nErrTime;

        public Int64 nLotRunTime;
        public Int64 nLotStopTime;
        public Int64 nLotErrTime;

        public Int64 nGDCount;
        public Int64 nRWCount;

        public Int64 nBin1Count;
        public Int64 nBin2Count;
        public Int64 nBin3Count;
        public Int64 nBin4Count;

        public double dRate;

        public Int64 nTotalCnt;
        public TimeSpan tmrTact;

        public TimeSpan[] tmrCuttingProc = new TimeSpan[2];

        public Int64 nPreCntOK;
        public Int64 nPreCntNG;

        public float fSamplingRate;
        public int nRunMode; // 0:Normal, 1:1회, 2:2회

        //Lot Name, Model Name, Operator Name
        public string strLotName;
        public string strModelName;
        public string OperatorName;

        public DateTime dateTimeLotInputTime;

        public ProductInfo()
        {
            nRunTime = 0;
            nStopTime = 0;
            nErrTime = 0;

            nGDCount = 0;
            nRWCount = 0;

            nBin1Count = 0;
            nBin2Count = 0;
            nBin3Count = 0;
            nBin4Count = 0;

            dRate = 0.0;
            nTotalCnt = 0;

            fSamplingRate = 100.0f;
            nRunMode = 0;
            dateTimeLotInputTime = DateTime.Now;
        }
    }

    [Serializable]
    public class LoginInfoList
    {
        public List<LoginInfo> m_listLogInfo = new List<LoginInfo>();

        public LoginInfoList()
        {

        }

        public LoginInfoList(List<LoginInfo> list)
        {
            m_listLogInfo = list;
        }
    }

    public class LoginInfo
    {
        public string USER_ID { get; set; }

        public string PASSWORD { get; set; }
        public int USER_LEVEL { get; set; }

        public LoginInfo()
        {
            USER_ID = "";
            PASSWORD = "";
            USER_LEVEL = MCDF.LEVEL_NOT_LOGIN;
        }

        public LoginInfo(string userID, string password, int userLevel)
        {
            USER_ID = userID;
            PASSWORD = password;
            USER_LEVEL = userLevel;
        }
    }

    [Serializable]
    public class UserColorInfoList
    {
        public List<UserColorInfo> m_listColorInfo = new List<UserColorInfo>();

        public UserColorInfoList()
        {

        }

        public UserColorInfoList(List<UserColorInfo> list)
        {
            m_listColorInfo = list;
        }
    }

    public class UserColorInfo
    {
        public string USER_ID { get; set; }
        public String USER_VisionNgColor { get; set; }
        public String USER_UnitEmptyColor { get; set; }
        public String USER_ITSColor { get; set; }
        public String USER_GoodColor { get; set; }
        public String USER_InvaildColor { get; set; }
        public String USER_ReworkColor { get; set; }
        public UserColorInfo()
        {
            USER_ID = "";
            USER_VisionNgColor = "Red";
            USER_UnitEmptyColor = "DarkViolet";
            USER_GoodColor = "Lime";
            USER_InvaildColor = "LightGray";
            USER_ITSColor = "Orange";
            USER_ReworkColor = "Yellow";
        }

        public UserColorInfo(string userID)
        {
            USER_ID = userID;
            USER_VisionNgColor = "Red";
            USER_UnitEmptyColor = "DarkViolet";
            USER_GoodColor = "Lime";
            USER_InvaildColor = "LightGray";
            USER_ITSColor = "Orange";
            USER_ReworkColor = "Yellow";

        }
    }

    [Serializable]
    public class TrayCountInfo
    {
        public string Save_DATE { get; set; }
        public string GOOD1_TrayCount { get; set; }
        public string GOOD2_TrayCount { get; set; }
        public string REWORK_TrayCount { get; set; }
        public string EMPTY1_TrayCount { get; set; }
        public string EMPTY2_TrayCount { get; set; }
        public TrayCountInfo()
        {
        }
    }

    public class McStage
    {
        public bool[] isRdy = new bool[10];          // 장비 초기화 확인 플래그
        public bool[] isRun = new bool[10];          // Auto 작동 확인 플래그

        public bool[] isCycleRunReq = new bool[(int)SEQ_ID.MAX]; // Cycle Run 요청 여부 (사용자가 on/off, 알람 시 off)
        public bool[] isCycleRun = new bool[(int)SEQ_ID.MAX];    // Cycle Run 동작 여부 (실제 돌고 있다)
        public bool[] isCycleErr = new bool[(int)SEQ_ID.MAX];    // Cycle Run 알람 여부
        public bool[] isCyclePause = new bool[(int)SEQ_ID.MAX];    // Cycle Run 멈춤

        public bool isManualRun;    // Manual 작동 확인 플래그
        public bool isVisionManualRun;    // Manual 작동 확인 플래그
        public bool isLotEnd = false;
        public bool[] isErrMain = new bool[(int)ERDF.ERR_CNT];

        public bool isLoadStop = false;

        public bool[] isInitializing = new bool[MCDF.MACHINE_MAX];
        public bool[] isInitialized = new bool[MCDF.MACHINE_MAX];

        public bool isWaitForStop;
        public bool isCheckStopPos;
        public bool isMovingStopPos;
        public bool isFinalizeBegin; 
        public bool isFinalizeEnd;
        public int nLastErrNo;
        public string strLastErrName;
        public bool[] isHomeComplete = new bool[SVDF.SV_CNT];
        public int nCurLoginLevel;
        public bool isDry;
        public double[] dStopPos;
        public string strCurRecipe;

        public int[] nSeqStep;
        public string[] strStepName;

        public int nControlStatus;
        public int nEqpStatus;
        public int nEqpStatusReason;
        public bool bIsIdleMode = false;

        public bool bStartInitAllMotor = false;
        public bool bPopupAccountWin = false;
        public bool bIsCimConnected = false;

        public bool IsError()
        {
            for (int nCnt = 0; nCnt < isCycleErr.Length; nCnt++)
            {
                if (isCycleErr[nCnt]) return true;
            }

            for (int nCnt = 0; nCnt < isErrMain.Length; nCnt++)
            {
                if (isErrMain[nCnt]) return true;
            }

            // Barcode Key-in 창이 활성화 되어 있다면 대기
            //if (GbVar.Seq.sLoading.bBCRUserKeyIn) return true;

            return false;
        }

        public bool IsMcInitailizing()
        {
            for (int nCnt = 0; nCnt < isInitializing.Length; nCnt++)
            {
                if (isInitializing[nCnt]) return true;
            }

            return false;
        }

        public bool IsRun()
        {
            for (int nCnt = 0; nCnt < isCycleRun.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                if (GbSeq.autoRun == null) return false;
                if (GbSeq.autoRun[(SEQ_ID)nCnt] == null) continue;

                if (isCycleRun[nCnt]) return true;
            }

            return false;
        }

        public bool IsRunReq()
        {
            for (int nCnt = 0; nCnt < isCycleRun.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                if (GbSeq.autoRun == null) return false;
                if (GbSeq.autoRun[(SEQ_ID)nCnt] == null) continue;

                if (isCycleRunReq[nCnt]) 
                    return true;
            }

            return false;
        }

        public bool IsAllRun()
        {
            for (int nCnt = 0; nCnt < isCycleRun.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                if (GbSeq.autoRun[(SEQ_ID)nCnt] == null) continue;

                if (!isCycleRun[nCnt]) return false;
            }

            return true;
        }

        public bool IsAllStopRun()
        {
            for (int nCnt = 0; nCnt < isCycleRun.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                if (GbSeq.autoRun[(SEQ_ID)nCnt] == null) continue;

                if (isCycleRun[nCnt]) return false;
            }

            return true;
        }

        public bool IsRunAndAllStopRunReq()
        {
            for (int nCnt = 0; nCnt < isCycleRun.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                if (GbSeq.autoRun[(SEQ_ID)nCnt] == null) continue;

                if (isCycleRunReq[nCnt]) return false;
            }

            for (int nCnt = 0; nCnt < isCycleRun.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                if (GbSeq.autoRun[(SEQ_ID)nCnt] == null) continue;

                if (isCycleRun[nCnt]) return true;
            }

            return false;
        }

        public void AllCycleRunReqStop()
        {
            for (int nCnt = 0; nCnt < isCycleRunReq.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                isCycleRunReq[nCnt] = false;
            }
        }

        public void AllCycleRunStop()
        {
            for (int nCnt = 0; nCnt < isCycleRunReq.Length; nCnt++)
            {
                isCycleRunReq[nCnt] = false;
                isCycleRun[nCnt] = false;
            }
        }

        public bool IsCycleRun(SEQ_ID seq)
        {
            if (isCycleRun[(int)seq]) return true;

            return false;
        }

        public bool IsCycleRunReq(SEQ_ID seq)
        {
            if (isCycleRunReq[(int)seq]) return true;

            return false;
        }

        public void AllCycleReady()
        {
            for (int nCnt = 0; nCnt < isRdy.Length; nCnt++)
            {
                isRdy[nCnt] = true;
            }
        }

        public void AllCycleInit()
        {
            for (int nCnt = 0; nCnt < isRdy.Length; nCnt++)
            {
                isRdy[nCnt] = false;
            }
        }

        public bool IsAllReady()
        {
            for (int nCnt = 0; nCnt < isRdy.Length; nCnt++)
            {
                if (isRdy[nCnt] == false) return false;
            }

            return true;
        }

        public bool IsInitializing()
        {
            for (int i = 0; i < GbVar.mcState.isInitializing.Length; i++)
            {
                if (GbVar.mcState.isInitializing[i]) return true;
            }

            return false;
        }
        public bool IsInitialized()
        {
            for (int i = 0; i < GbVar.mcState.isInitialized.Length; i++)
            {
                if (!GbVar.mcState.isInitialized[i]) return false;
            }

            return true;
        }
    };


    public enum SEQ_ID
    {
        // 신규 추가
        //::: 주의 순서 변경하면 안됨 :::
        LD_MZ_LD_CONV = 0, // 로드 컨베이어, OP방향 우측,  
        LD_MZ_ULD_CONV = 1,

        // LOADER
        LD_MZ_ELV_TRANSFER,

        STRIP_TRANSFER,
        UNIT_TRANSFER,

        DRY_UNIT,

        MAP_TRANSFER,
        MAP_VISION_TABLE_1,
        MAP_VISION_TABLE_2,

        PICK_N_PLACE_1,
        PICK_N_PLACE_2,

        UNLOAD_GD1_TRAY_TABLE,
        UNLOAD_GD2_TRAY_TABLE, 
        UNLOAD_RW_TRAY_TABLE, //UNLOAD_TRAY_TABLE_2_1,

        TRAY_TRANSFER,

        ULD_ELV_GOOD_1,
        ULD_ELV_GOOD_2,
        ULD_ELV_REWORK,
        ULD_ELV_EMPTY_1,
        ULD_ELV_EMPTY_2,


        MANUAL_RUN,
        ALWAYS_WORK,
        AUTO_LOT_END,

        //CIM 
        CIM_MANAGER,
        CIM_READ_DATA,

        TEN_KEY_MON,
        VISION_MOVE,

        MAX
    }

    public enum MODULE_TYPE
    {
        MOVE_TEACH_POS = 0,
        MOVE_CFG_POS,
        ALL_HOME,
        MANUAL_LD_CYCLE_RUN,
        MANUAL_SAW_CYCLE_RUN,
        MANUAL_SORTER_CYCLE_RUN,
        MANUAL_CLEANER_CYCLE_RUN,
        MANUAL_PRE_ALIGN,
        MANUAL_TOP_ALIGN,
        MANUAL_MAP_INSPECTION,
        MANUAL_BALL_INSPECTION,
        MANUAL_MOVE_CENTER_TOP_ALIGN,
        MANUAL_PICKER_CALIBRATION,
        MANUAL_PICKER_ZEROSET_PICK_UP,
        MANUAL_PICKER_ZEROSET_PLACE,
        SINGLE_HOME,
        CYCLE_VAC_CYL,
        POS_GROUP_MOVE,
        READ_MCR,
        INIT_CYCLE,
        MAP_VISION_VIEW_CYCLE_RUN,
        TRAY_VISION_VIEW_CYCLE_RUN,
        MAX,
    }

    public enum STRIP_MDL
    {
        NONE = -1,
        // OHT
        // SAW
        STRIP_RAIL,
        STRIP_TRANSFER,
        CUTTING_TABLE,
        UNIT_TRANSFER,
        // SORTER
        UNIT_CLEANING,
        UNIT_DRY,
        MAP_TRANSFER,
        MAP_VISION_TABLE_1,
        MAP_VISION_TABLE_2,

        MAX,
    }
    public enum STRIP_MDL_CHINA
    {
        NONE = -1,
        // OHT
        // SAW
        基板轨道,
        基板传送机,
        切割台,
        单元手臂,
        // SORTER
        清洁,
        干燥,
        检查手臂,
        检查台_1,
        检查台_2,

        MAX,
    }

    /// <summary>
    /// 자재의 진공, 블로우 상태를 나타내기 위함
    /// </summary>
    public enum AIRSTATUS
    {
        NONE,//진공, 블로우가 꺼져있는 상태
        VAC,
        BLOW,
        UNKNOWN,//어떤 상태도 아닌 상태
    }
    public class LotInfo
    {
        public string LOT_ID = "";
        public int LOT_UNIT_QTY = 0;
        public int SUB_QTY = 0;
        public string LOT_OPER = "";
        public string PROD_ID = "";
        public int LD_PROC_PORT_CARRIER_QTY = 0;
        public string RECIPE_NAME = "";
        public string MAT_ID = "";
        public string KIT_NO = "";
        public bool SORTING_FLAG = false;
        public bool CARRIERMAP_MODE = false;
        public bool MERGE_FLAG = false;
        public bool HWR_FLAG = false;


        public void Clear()
        {
            LOT_ID = "";
            LOT_UNIT_QTY = 0;
            SUB_QTY = 0;
            LOT_OPER = "";
            PROD_ID = "";
            LD_PROC_PORT_CARRIER_QTY = 0;
            RECIPE_NAME = "";
            MAT_ID = "";
            KIT_NO = "";
            SORTING_FLAG = false;
            CARRIERMAP_MODE = false;
            MERGE_FLAG = false;
            HWR_FLAG = false;
        }

        public int BALL_YIELD()
        {
            int nGdQty = 0;
            for (int i = 0; i < GbVar.Seq.sUldTrayElvGood.Length; i++)
            {
                for (int j = 0; j < GbVar.Seq.sUldTrayElvGood[i].Info.Count; j++)
                {
                    nGdQty += GbVar.Seq.sUldTrayElvGood[i].Info[j].OK_BALL_QTY();
                }
            }
            return (nGdQty / LOT_UNIT_QTY) * 100;
        }

        public int MAP_YIELD()
        {
            int nGdQty = 0;
            for (int i = 0; i < GbVar.Seq.sUldTrayElvGood.Length; i++)
            {
                for (int j = 0; j < GbVar.Seq.sUldTrayElvGood[i].Info.Count; j++)
                {
                    nGdQty += GbVar.Seq.sUldTrayElvGood[i].Info[j].OK_MAP_QTY();
                }
            }
            return (nGdQty / LOT_UNIT_QTY) * 100;
        }






    }
    public enum ALIGN_RESULT
    {
        //RETRY,
        //SKIP,
        NONE = 0, 
        NG,
        OK,
    }

    public class StripInfo
    {
        public bool bIsStrip = false;
        public int nArraySize = 0;
        public bool bIsError = false;

        public DateTime[] dtLastCycleIn = new DateTime[50];
        public DateTime[] dtLastCycleOut = new DateTime[50];

        public string MAGAZINE_ID
        {
            get;
            set;
        }
        public int MAGAZINE_SLOT_NO
        {
            get;
            set;
        }

        public string LOT_ID
        {
            get;
            set;
        }

        public string ITS_ID
        {
            get;
            set;
        }

        public string STRIP_ID
        {
            get;
            set;
        }

        public int CUTTING_TABLE_NO
        {
            get;
            set;
        }

        public int MAP_TABLE_NO
        {
            get;
            set;
        }

        public int PANEL_NO
        {
            get;
            set;
        }
        public DateTime STRIP_IN_TIME
        {
            get;
            set;
        }
        public DateTime STRIP_OUT_TIME
        {
            get;
            set;
        }
        public int GOOD_UNIT
        {
            get;
            set;
        }
        public int REWORK_UNIT
        {
            get;
            set;
        }
        public int X_MARK_UNIT
        {
            get;
            set;
        }
        public int LOSS_UNIT
        {
            get;
            set;
        }
        public string STAGE_SPEED_CH1
        {
            get;
            set;
        }
        public string SPINDLE_RPM_1_CH1
        {
            get;
            set;
        }
        public string SPINDLE_RPM_2_CH1
        {
            get;
            set;
        }

        public string STAGE_SPEED_CH2
        {
            get;
            set;
        }
        public string SPINDLE_RPM_1_CH2
        {
            get;
            set;
        }
        public string SPINDLE_RPM_2_CH2
        {
            get;
            set;
        }


        public string[] MoveInTime = new string[10];
        public string[] MoveOutTime = new string[10];

        public UnitInfo[][] UnitArr;    // 맴버 배열 array 선언

        public bool bPreResult1 = true;
        public bool bPreResult2 = true;

        public PointF ptPreOffset1 = new PointF();
        public PointF ptPreOffset2 = new PointF();
        public double dPreAngleOffset = 0.0;

        // Top Align 정보
        public bool[] bTopAlignResult = new bool[(int)MCDF.eStage.MAX];
        public dxy[] xyTopAlignOffset = new dxy[(int)MCDF.eStage.MAX];
        public double dTopAngleOffset = 0.0;
        public bool bTopAlignCorrectResult = false;
        public dxy xyTopAlignCorrectOffset = new dxy(); // Angle 보정 후 X,Y Offset 값

        public int[] WorkingData = new int[(int)MESDF.eWORKING_DATA.MAX];

        public StripInfo()
        { 
            MAGAZINE_SLOT_NO = 0;
            CUTTING_TABLE_NO = 0;
            MAP_TABLE_NO = 0;
            PANEL_NO = 0;
            LOT_ID = "";
            ITS_ID = "";
            STRIP_ID = "";
            bIsError = false;
            WorkingData = new int[(int)MESDF.eWORKING_DATA.MAX];
            STRIP_IN_TIME = DateTime.Now;
            STRIP_OUT_TIME = DateTime.Now;
            GOOD_UNIT = 0;
            REWORK_UNIT = 0;
            X_MARK_UNIT = 0;
            SPINDLE_RPM_1_CH1 = "";
            LOSS_UNIT = 0;
            for (int nCnt = 0; nCnt < xyTopAlignOffset.Length; nCnt++)
            {
                xyTopAlignOffset[nCnt] = new dxy();
            }
        }
        public void ClearItem()
        {
            MAGAZINE_SLOT_NO = 0;
            CUTTING_TABLE_NO = 0;
            MAP_TABLE_NO = 0;
            PANEL_NO = 0;
            LOT_ID = "";
            ITS_ID = "";
            STRIP_ID = "";
            bIsError = false;
            WorkingData = new int[(int)MESDF.eWORKING_DATA.MAX];
            STRIP_IN_TIME = DateTime.Now;
            STRIP_OUT_TIME = DateTime.Now;
            GOOD_UNIT = 0;
            REWORK_UNIT = 0;
            X_MARK_UNIT = 0;
            SPINDLE_RPM_1_CH1 = "";
            LOSS_UNIT = 0;
        }


        public bool IsPreOk
        {
            get
            {
                return bPreResult1 && bPreResult2;
            }
        }

        public bool IsTopAlignOk
        {
            get
            {
                return bTopAlignResult[0] && bTopAlignResult[1];
            }
        }

        /// <summary>
        /// Top Align에 대한 정보를 초기화한다
        /// </summary>
        public void InitTopAlignInfo()
        {
            for (int nCnt = 0; nCnt < bTopAlignResult.Length; nCnt++)
            {
                bTopAlignResult[nCnt] = false;
            }

            for (int nCnt = 0; nCnt < xyTopAlignOffset.Length; nCnt++)
            {
                xyTopAlignOffset[nCnt].x = 0;
                xyTopAlignOffset[nCnt].y = 0;
            }

            dTopAngleOffset = 0.0;

            bTopAlignCorrectResult = false;
            xyTopAlignCorrectOffset.x = 0;
            xyTopAlignCorrectOffset.y = 0;
        }

        public void GetUnitResult()
        {
            try
            {
                string strMapSize = "";
                string strBallSize = "";
                string strSawoffset = "";

                int nGdQty = 0;
                int nTotalQty = 0;

                int nBallQty = 0;
                int nMapQty = 0;

                int nGdStage1Qty = 0;
                int nGdStage2Qty = 0;
                int nrwStageQty = 0;
                int nScrap = 0;

                int nGdMapQty = 0;
                int nGdBallQty = 0;
                int nLossMapQty = 0;
                int nLossBallQty = 0;

                int nBurrNgQty = 0;


                int nIndex = 1;
                for (int nRow = UnitArr.Length - 1; nRow != 0; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {

                        //strSize += string.Format("({0}){1},{2}", (nRow * UnitArr[nRow].Length) + nCol, UnitArr[nRow][nCol].MAP_SIZE.Height, UnitArr[nRow][nCol].MAP_SIZE.Width);
                        strMapSize += string.Format("({0}){1},{2}", nIndex, UnitArr[nRow][nCol].MAP_SIZE.Width, UnitArr[nRow][nCol].MAP_SIZE.Height);
                        strBallSize += string.Format("({0}){1},{2}", nIndex, UnitArr[nRow][nCol].BALL_SIZE.Width, UnitArr[nRow][nCol].BALL_SIZE.Height);
                        strSawoffset += string.Format("({0}){1};{2}", nIndex, UnitArr[nRow][nCol].SAW_OFFSET_LT.X, UnitArr[nRow][nCol].SAW_OFFSET_LT.Y);

                        if (UnitArr[nRow][nCol].IS_UNIT == true)
                        {
                            nTotalQty++;

                            if (UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                            {
                                nGdBallQty++;
                            }

                            if (UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.NG)
                            {
                                nLossBallQty++;
                            }

                            if (UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                            {
                                nGdMapQty++;
                            }

                            if (UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_BALL.NG)
                            {
                                nLossMapQty++;
                            }

                            if (UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK && UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                            {
                                nGdQty++;
                            }
                        }

                        if (UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.NOCHIP)
                        {
                            nMapQty++;
                        }
                        if (UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.NOCHIP)
                        {
                            nBallQty++;
                        }
                        if (UnitArr[nRow][nCol].OUT_PORT == (int)MCDF.eOutPort.GD_1)
                        {
                            nGdStage1Qty++;
                        }
                        else if (UnitArr[nRow][nCol].OUT_PORT == (int)MCDF.eOutPort.GD_2)
                        {
                            nGdStage2Qty++;
                        }
                        else if (UnitArr[nRow][nCol].OUT_PORT == (int)MCDF.eOutPort.RW)
                        {
                            nrwStageQty++;
                        }
                        if (UnitArr[nRow][nCol].OUT_PORT >= (int)MCDF.eOutPort.X_OUT)
                        {
                            nScrap++;
                        }
                        nIndex++;
                    }
                }
            }
            catch (Exception)
            {
            }
        }



        /// <summary>
        /// ITS XOUT정보를 가져온 뒤 해당 STRIP에 적용합니다.
        /// </summary>
        /// <returns>성공 여부</returns>
        public bool GetXOutInfo()
        {
            bool ret = true;
            //        Point[] XOutLoc;
            //        int nX = 0, nY = 0;

            try
            {

                //            for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY; nRow++)
                //            {
                //                for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX; nCol++)
                //                {
                //                    UnitArr[nRow][nCol].SUB_ID = STRIP_ID;
                //                    UnitArr[nRow][nCol].IS_UNIT = true;
                //                    UnitArr[nRow][nCol].IS_SKIP_UNIT = false;
                //                    UnitArr[nRow][nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.OK;
                //                    UnitArr[nRow][nCol].BTM_INSP_RESULT = (int)VSDF.eJUDGE_BALL.OK;
                //                    UnitArr[nRow][nCol].ITS_XOUT = (int)MESDF.eCELL_STATUS.OK;
                //                }
                //            }

                //            //바코드 길이와 ITS에 저장된 스트립 ID의 길이가 같지 않을때도 있기때문에 유효한 바코드 길이로 검색한다.
                //            string strSearchStripId = STRIP_ID.Substring(0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_STRIP_ID_LENGTH].nValue);
                //            XOutLoc = GbVar.dbITSInfo.SelectXOutInfo(STRIP_ID);

                //            if (XOutLoc == null) return false;
                //            if (XOutLoc.Length < 1) return false;

                //            int nXOutCount = 0;

                //            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat)
                //            {
                //                #region ITS좌표기준 (Map T에서 보았을때) 
                //                //   ← Y
                //                //5,4,3,2,1,0
                //                //          1  X
                //                //          2  ↓
                //                //          3
                //                //          4
                //                //          5
                //                #endregion
                //                //1. Y좌표 반전(X Count - (바뀐 Y좌표-1)
                //                //2. X,Y반전
                //                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_VISION_BARCODE_READ_USE].bOptionUse)
                //                {
                //                    for (int nRow = 0; nRow < XOutLoc.Length; nRow++)
                //                    {
                //                        //nX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - (XOutLoc[nRow].Y - 1) - 1;
                //                        //nY = XOutLoc[nRow].X - 1;
                //                        nX = XOutLoc[nRow].Y - 1;
                //                        nY = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY - (XOutLoc[nRow].X - 1) - 1;
                //                        UnitArr[nY][nX].ITS_XOUT = (int)MESDF.eCELL_STATUS.XOUT;

                //                        nXOutCount++;
                //                    }
                //                }

                //                #region ITS좌표기준 (Map T에서 보았을때)
                //                //   → Y
                //                //0,1,2,3,4,5
                //                //1            X
                //                //2            ↓
                //                //3          
                //                //4          
                //                //5          
                //                // [2022.05.25.kmlee] 우측 하단이 기준
                //                //   ← Y
                //                //          5
                //                //          4  X
                //                //          3  
                //                //          2
                //                //          1
                //                //5,4,3,2,1,0
                //                #endregion
                //                //1. X,Y만 반전
                //                else
                //                {
                //                    for (int nRow = 0; nRow < XOutLoc.Length; nRow++)
                //                    {
                //                        //nX = XOutLoc[nRow].Y - 1;
                //                        //nY = XOutLoc[nRow].X - 1;
                //                        nX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - (XOutLoc[nRow].Y - 1) - 1;
                //                        nY = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY - (XOutLoc[nRow].X - 1) - 1;

                //                        UnitArr[nY][nX].ITS_XOUT = (int)MESDF.eCELL_STATUS.XOUT;

                //                        nXOutCount++;
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                #region ITS좌표기준 (Map T에서 보았을때)
                //                //   → Y
                //                //5
                //                //4            X
                //                //3            ↑
                //                //2          
                //                //1          
                //                //0,1,2,3,4,5          
                //                #endregion
                //                //1. Y좌표 반전(X Count - (바뀐 Y좌표-1)
                //                //2. X, Y반전
                //                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_VISION_BARCODE_READ_USE].bOptionUse)
                //                {
                //                    for (int nRow = 0; nRow < XOutLoc.Length; nRow++)
                //                    {
                //                        //220528
                //                        nX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - (XOutLoc[nRow].Y - 1) - 1;
                //                        nY = XOutLoc[nRow].X - 1;
                //                        UnitArr[nY][nX].ITS_XOUT = (int)MESDF.eCELL_STATUS.XOUT;

                //                        nXOutCount++;
                //                    }
                //                }

                //                #region ITS좌표기준 (Map T에서 보았을때)
                //                //   → Y
                //                //0,1,2,3,4,5
                //                //1            X
                //                //2            ↓
                //                //3          
                //                //4          
                //                //5          
                //                #endregion
                //                //1. X, Y 반전
                //                else
                //                {
                //                    for (int nRow = 0; nRow < XOutLoc.Length; nRow++)
                //                    {
                //                        //    nX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - (XOutLoc[nRow].Y - 1) - 1;
                //                        //    nY = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY - (XOutLoc[nRow].X - 1) - 1;
                //                        nX = XOutLoc[nRow].Y - 1;
                //                        nY = XOutLoc[nRow].X - 1;
                //                        UnitArr[nY][nX].ITS_XOUT = (int)MESDF.eCELL_STATUS.XOUT;

                //                        nXOutCount++;
                //                    }
                //                }
                //            }
                ////220608
                //            //XOUT수량 카운팅은 procStUnitPnP에서 함  
            }
            catch (Exception)
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// 스트립 단위 Map의 사이즈를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string MAP_PKG_SIZE()
        {
            string strSize = "";

            try
            {
                int nIndex = 1;
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        //strSize += string.Format("({0}){1},{2}", (nRow * UnitArr[nRow].Length) + nCol, UnitArr[nRow][nCol].MAP_SIZE.Height, UnitArr[nRow][nCol].MAP_SIZE.Width);
                        strSize += string.Format("({0}){1};{2}", nIndex, UnitArr[nRow][nCol].MAP_SIZE.Height, UnitArr[nRow][nCol].MAP_SIZE.Width);
                        nIndex++;
                    }
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return strSize;
        }

        /// <summary>
        /// 스트립 단위 Ball의 사이즈를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string BALL_PKG_SIZE()
        {
            string strSize = "";

            try
            {
                int nIndex = 1;
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        //strSize += string.Format("({0}){1};{2}", (nRow * UnitArr[nRow].Length) + nCol, UnitArr[nRow][nCol].BALL_SIZE.Height, UnitArr[nRow][nCol].BALL_SIZE.Width);
                        strSize += string.Format("({0}){1};{2}", nIndex, UnitArr[nRow][nCol].BALL_SIZE.Width, UnitArr[nRow][nCol].BALL_SIZE.Height);
                        nIndex++;
                    }
                }

            }
            catch (Exception)
            {

            }
            return strSize;
        }


        /// <summary>
        /// 스트립 단위 Ball의 Saw Offset을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string BALL_SAW_OFFSET()
        {
            string strSize = "";

            try
            {
                int nIndex = 1;

                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strSize += string.Format("({0}){1};{2}", nIndex, UnitArr[nRow][nCol].SAW_OFFSET_LT.X, UnitArr[nRow][nCol].SAW_OFFSET_LT.Y);
                        nIndex++;
                    }
                }

            }
            catch (Exception)
            {
            }
            return strSize;
        }

        public string HOST_CODE()
        {
            string strCode = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strCode += UnitArr[nRow][nCol].ITS_XOUT;
                    }
                }
            }
            catch (Exception)
            {
            }

            return strCode;
        }

        public string TOP_COORDI()
        {
            string strCode = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strCode += UnitArr[nRow][nCol].MAP_COORDI.X.ToString() + ",";
                        strCode += UnitArr[nRow][nCol].MAP_COORDI.Y.ToString() + ",";
                        strCode += UnitArr[nRow][nCol].MAP_COORDI.T.ToString() + ";";
                    }
                }
            }
            catch (Exception)
            {

            }

            return strCode;
        }

        public string BTM_COORDI()
        {
            string strCode = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strCode += UnitArr[nRow][nCol].BALL_COORDI.X.ToString() + ",";
                        strCode += UnitArr[nRow][nCol].BALL_COORDI.Y.ToString() + ",";
                        strCode += UnitArr[nRow][nCol].BALL_COORDI.T.ToString() + ";";
                    }
                }
            }
            catch (Exception)
            {

            }

            return strCode;
        }

        public string TOP_NG_CODE()
        {
            string strCode = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strCode += UnitArr[nRow][nCol].TOP_NG_CODE.ToString() + ",";
                    }
                }

                strCode = strCode.TrimEnd(',');
            }
            catch (Exception)
            {

            }

            return strCode;
        }

        public string BTM_NG_CODE()
        {
            string strCode = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strCode += UnitArr[nRow][nCol].BTM_NG_CODE.ToString() + ",";
                    }
                }

                strCode = strCode.TrimEnd(',');
            }
            catch (Exception)
            {

            }

            return strCode;
        }

        public string TOP_RESULT()
        {
            string strCode = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strCode += UnitArr[nRow][nCol].TOP_INSP_RESULT.ToString() + ",";
                    }
                }

                strCode = strCode.TrimEnd(',');
            }
            catch (Exception)
            {

            }

            return strCode;
        }


        public string BTM_RESULT()
        {
            string strCode = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strCode += UnitArr[nRow][nCol].BTM_INSP_RESULT.ToString() + ",";
                    }
                }

                strCode = strCode.TrimEnd(',');
            }
            catch (Exception)
            {

            }
            return strCode;
        }


        public string OUT_PORT()
        {
            string strCode = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strCode += UnitArr[nRow][nCol].OUT_PORT.ToString() + ",";
                    }
                }

                strCode = strCode.TrimEnd(',');
            }
            catch (Exception)
            {

            }
            return strCode;
        }

        public string ISUNIT()
        {
            string strIsUnit = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = 0; nCol < UnitArr[nRow].Length; nCol++)
                    {
                        strIsUnit += Convert.ToInt32(UnitArr[nRow][nCol].IS_UNIT);
                    }
                }
            }
            catch (Exception)
            {
            }

            return strIsUnit;
        }

        public void SetHostCode(string strCode)
        {
            try
            {
                int nRowIdx = UnitArr.Length - 1;

                int nCode = 0;
                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        if (!int.TryParse(strCode.Substring((UnitArr[nRowIdx].Length * nRow) + nCol, 1), out nCode)) nCode = 0;
                        UnitArr[nRowIdx][nCol].ITS_XOUT = nCode;
                    }

                    nRowIdx--;
                }

            }
            catch (Exception)
            {

            }
        }

        public void SetTopCoodi(string strCode)
        {
            try
            {
                string[] sArr = strCode.Split(';');
                double dCoordiX = 0.0;
                double dCoordiY = 0.0;
                double dCoordiT = 0.0;

                int nRowIdx = UnitArr.Length - 1;

                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    dCoordiX = 0.0;
                    dCoordiY = 0.0;
                    dCoordiT = 0.0;

                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        string[] sCoordi = sArr[(UnitArr[nRowIdx].Length * nRow) + nCol].Split(',');
                        if (sCoordi.Length == 3)
                        {
                            if (!double.TryParse(sCoordi[0], out dCoordiX)) dCoordiX = 0.0;
                            if (!double.TryParse(sCoordi[1], out dCoordiY)) dCoordiY = 0.0;
                            if (!double.TryParse(sCoordi[2], out dCoordiT)) dCoordiT = 0.0;
                        }

                        UnitArr[nRowIdx][nCol].MAP_COORDI.X = dCoordiX;
                        UnitArr[nRowIdx][nCol].MAP_COORDI.Y = dCoordiY;
                        UnitArr[nRowIdx][nCol].MAP_COORDI.T = dCoordiT;
                    }
                    nRowIdx--;
                }

            }
            catch (Exception)
            {

            }
        }

        public void SetBtmCoodi(string strCode)
        {
            try
            {
                string[] sArr = strCode.Split(';');
                double dCoordiX = 0.0;
                double dCoordiY = 0.0;
                double dCoordiT = 0.0;

                int nRowIdx = UnitArr.Length - 1;

                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        dCoordiX = 0.0;
                        dCoordiY = 0.0;
                        dCoordiT = 0.0;

                        string[] sCoordi = sArr[(UnitArr[nRowIdx].Length * nRow) + nCol].Split(',');
                        if (sCoordi.Length == 3)
                        {
                            if (!double.TryParse(sCoordi[0], out dCoordiX)) dCoordiX = 0.0;
                            if (!double.TryParse(sCoordi[1], out dCoordiY)) dCoordiY = 0.0;
                            if (!double.TryParse(sCoordi[2], out dCoordiT)) dCoordiT = 0.0;
                        }

                        UnitArr[nRowIdx][nCol].BALL_COORDI.X = dCoordiX;
                        UnitArr[nRowIdx][nCol].BALL_COORDI.Y = dCoordiY;
                        UnitArr[nRowIdx][nCol].BALL_COORDI.T = dCoordiT;
                    }
                    nRowIdx--;
                }
            }
            catch (Exception)
            {

            }
        }

        public void SetTopNgCode(string strCode)
        {
            try
            {
                string[] sCode = strCode.Split(',');
                int nRowIdx = UnitArr.Length - 1;

                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        UnitArr[nRowIdx][nCol].TOP_NG_CODE = sCode[(UnitArr[nRowIdx].Length * nRow) + nCol];
                    }
                    nRowIdx--;
                }
            }
            catch (Exception)
            {

            }
        }

        public void SetBtmNgCode(string strCode)
        {
            try
            {
                string[] sCode = strCode.Split(',');
                int nRowIdx = UnitArr.Length - 1;

                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        UnitArr[nRowIdx][nCol].BTM_NG_CODE = sCode[(UnitArr[nRowIdx].Length * nRow) + nCol];
                    }
                    nRowIdx--;
                }
            }
            catch (Exception)
            {

            }
        }

        public void SetTopResult(string strCode)
        {
            try
            {
                string[] sCode = strCode.Split(',');
                int nRowIdx = UnitArr.Length - 1;

                int nCode = 0;
                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        if (!int.TryParse(sCode[(UnitArr[nRowIdx].Length * nRow) + nCol], out nCode)) nCode = 0;
                        UnitArr[nRowIdx][nCol].TOP_INSP_RESULT = nCode;
                    }
                    nRowIdx--;
                }

            }
            catch (Exception)
            {

            }
        }

        public void SetBtmResult(string strCode)
        {
            try
            {
                string[] sCode = strCode.Split(',');
                int nRowIdx = UnitArr.Length - 1;

                int nCode = 0;
                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        if (!int.TryParse(sCode[(UnitArr[nRowIdx].Length * nRow) + nCol], out nCode)) nCode = 0;
                        UnitArr[nRowIdx][nCol].BTM_INSP_RESULT = nCode;
                    }
                    nRowIdx--;
                }

            }
            catch (Exception)
            {

            }
        }

        public void SetTopPkgSize(string strCode)
        {
            try
            {
                string[] sArr = strCode.Split('(');

                string sSize = "";
                double dWidth = 0.0;
                double dHeight = 0.0;
                int nRowIdx = UnitArr.Length - 1;

                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        dWidth = 0.0;
                        dHeight = 0.0;

                        sSize = sArr[(UnitArr[nRowIdx].Length * nRow) + nCol];
                        sSize = sSize.TrimStart(')').Substring(1);
                        string[] sArrSize = sSize.Split(';');

                        if (sArrSize.Length == 2)
                        {
                            if (!double.TryParse(sArrSize[0], out dWidth)) dWidth = 0;
                            if (!double.TryParse(sArrSize[1], out dHeight)) dHeight = 0;
                        }

                        UnitArr[nRowIdx][nCol].MAP_SIZE.Width = dWidth;
                        UnitArr[nRowIdx][nCol].MAP_SIZE.Height = dHeight;
                    } 
                    nRowIdx--;
                }
            }
            catch (Exception)
            {

            }
        }


        public void SetBtmPkgSize(string strCode)
        {
            try
            {
                string[] sArr = strCode.Split('(');

                string sSize = "";
                double dWidth = 0.0;
                double dHeight = 0.0;
                int nRowIdx = UnitArr.Length - 1;

                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        dWidth = 0.0;
                        dHeight = 0.0;

                        sSize = sArr[(UnitArr[nRowIdx].Length * nRow) + nCol];
                        sSize = sSize.TrimStart(')').Substring(1);
                        string[] sArrSize = sSize.Split(';');

                        if (sArrSize.Length == 2)
                        {
                            if (!double.TryParse(sArrSize[0], out dWidth)) dWidth = 0;
                            if (!double.TryParse(sArrSize[1], out dHeight)) dHeight = 0;
                        }

                        UnitArr[nRowIdx][nCol].BALL_SIZE.Width = dWidth;
                        UnitArr[nRowIdx][nCol].BALL_SIZE.Height = dHeight;
                    }
                    nRowIdx--;
                }
            }
            catch (Exception)
            {

            }
        }


        public void SetBtmSawOffsetLt(string strCode)
        {
            try
            {
                string[] sArr = strCode.Split('(');

                string sDist = "";
                double dX = 0.0;
                double dY = 0.0;
                int nRowIdx = UnitArr.Length - 1;

                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        dX = 0.0;
                        dY = 0.0;

                        sDist = sArr[(UnitArr[nRowIdx].Length * nRow) + nCol];
                        sDist = sDist.TrimStart(')').Substring(1);
                        string[] sArrSize = sDist.Split(';');

                        if (sArrSize.Length == 2)
                        {
                            if (!double.TryParse(sArrSize[0], out dX)) dX = 0;
                            if (!double.TryParse(sArrSize[1], out dY)) dY = 0;
                        }

                        UnitArr[nRowIdx][nCol].SAW_OFFSET_LT.X = dX;
                        UnitArr[nRowIdx][nCol].SAW_OFFSET_LT.Y = dY;
                    }
                    nRowIdx--;
                }
            }
            catch (Exception)
            {

            }
        }

        public void SetOutPort(string strCode)
        {
            try
            {
                string[] sArrPort = strCode.Split(',');
                int nRowIdx = UnitArr.Length - 1;

                int nPort = 0;
                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        if (!int.TryParse(sArrPort[(UnitArr[nRowIdx].Length * nRow) + nCol], out nPort)) nPort = 0;
                        UnitArr[nRowIdx][nCol].OUT_PORT = nPort;
                    }
                    nRowIdx--;
                }
            }
            catch (Exception)
            {

            }
        }
        public void SetIsUnit(string strCode)
        {
            try
            {
                bool bisUnit = true;
                int nRowIdx = UnitArr.Length - 1;

                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    for (int nCol = 0; nCol < UnitArr[nRowIdx].Length; nCol++)
                    {
                        if (!bool.TryParse(strCode.Substring((UnitArr[nRowIdx].Length * nRow) + nCol, 1), out bisUnit)) bisUnit = true;
                        UnitArr[nRowIdx][nCol].IS_UNIT = bisUnit;
                    }
                    nRowIdx--;
                }
            }
            catch (Exception)
            {

            }
        }

        public void Init(int nSize)
        {
            MAGAZINE_SLOT_NO = 0;
            nArraySize = nSize;
            UnitArr = new UnitInfo[nSize][];
        }

        /// <summary>
        /// Wafer가 있는지
        /// </summary>
        /// <returns></returns>
        public bool IsStrip()
        {
            return bIsStrip;
        }

        public void SetStrip(bool bSet)
        {
            bIsStrip = bSet;
        }

        public void Set(int nColCnt)    // 인덱스와 값을 매개변수로 받아 해당하는 인덱스에 값을 저장하는 매써드
        {
            for (int nRow = 0; nRow < UnitArr.Length; nRow++)
            {
                UnitInfo[] unit = new UnitInfo[nColCnt];

                for (int nCol = 0; nCol < nColCnt; nCol++)
                {
                    unit[nCol] = new UnitInfo();

                    unit[nCol].ROW = nRow;
                    unit[nCol].COL = nCol;
                }

                UnitArr[nRow] = unit;
            }
        }

        public void Clear()
        {
            WorkingData = new int[(int)MESDF.eWORKING_DATA.MAX];
            STRIP_ID = "";
            LOT_ID = "";
            ITS_ID = "";
            PANEL_NO = 0;

            SetStrip(false);

            for (int nRow = 0; nRow < UnitArr.Length; nRow++)
            {
                UnitInfo[] unit = new UnitInfo[UnitArr[0].Length];

                for (int nCol = 0; nCol < UnitArr[0].Length; nCol++)
                {
                    unit[nCol] = new UnitInfo();

                    unit[nCol].ROW = nRow;
                    unit[nCol].COL = nCol;

                    //bhoh 20211120
                    unit[nCol].IS_UNIT = false;
                    unit[nCol].ITS_XOUT = (int)MESDF.eCELL_STATUS.OK;

                    unit[nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.NOCHIP;
                    unit[nCol].BTM_INSP_RESULT = (int)VSDF.eJUDGE_BALL.NOCHIP;
                    unit[nCol].TOP_NG_CODE = "00";
                    unit[nCol].BTM_NG_CODE = "00";


                    unit[nCol].BALL_COORDI.X = 0.0f;
                    unit[nCol].BALL_COORDI.Y = 0.0f;
                    unit[nCol].BALL_COORDI.T = 0.0f;

                    unit[nCol].MAP_COORDI.X = 0.0f;
                    unit[nCol].MAP_COORDI.Y = 0.0f;
                    unit[nCol].MAP_COORDI.T = 0.0f;

                    unit[nCol].BALL_SIZE.Width = 0.0f;
                    unit[nCol].BALL_SIZE.Height = 0.0f;
                    unit[nCol].MAP_SIZE.Width = 0.0f;
                    unit[nCol].MAP_SIZE.Height = 0.0f;
                    unit[nCol].SAW_OFFSET_LT.X= 0.0f;
                    unit[nCol].SAW_OFFSET_LT.Y = 0.0f;

                }
                UnitArr[nRow] = unit;
            }


        }

        public void CopyTo(ref StripInfo info)
        {
            //info.WorkingData = this.WorkingData;
            Array.Copy(this.WorkingData, info.WorkingData, this.WorkingData.Length);
            info.bIsStrip = this.bIsStrip;
            info.nArraySize = this.nArraySize;
            info.bIsError = this.bIsError;

            info.MAGAZINE_SLOT_NO = this.MAGAZINE_SLOT_NO;
            info.CUTTING_TABLE_NO = this.CUTTING_TABLE_NO;
            info.MAP_TABLE_NO = this.MAP_TABLE_NO;

            info.LOT_ID = this.LOT_ID;
            info.ITS_ID = this.ITS_ID;
            info.STRIP_ID = this.STRIP_ID;
            info.PANEL_NO = this.PANEL_NO;
            info.STRIP_IN_TIME = this.STRIP_IN_TIME;
            info.STRIP_OUT_TIME = this.STRIP_OUT_TIME;
            info.GOOD_UNIT = this.GOOD_UNIT;
            info.REWORK_UNIT = this.REWORK_UNIT;
            info.X_MARK_UNIT = this.X_MARK_UNIT;
            info.LOSS_UNIT = this.LOSS_UNIT;
            info.STAGE_SPEED_CH1 = this.STAGE_SPEED_CH1;
            info.STAGE_SPEED_CH2 = this.STAGE_SPEED_CH2;
            info.SPINDLE_RPM_1_CH1 = this.SPINDLE_RPM_1_CH1;
            info.SPINDLE_RPM_2_CH1 = this.SPINDLE_RPM_2_CH1;
            info.SPINDLE_RPM_1_CH2 = this.SPINDLE_RPM_1_CH2;
            info.SPINDLE_RPM_2_CH2 = this.SPINDLE_RPM_2_CH2;

            info.bPreResult1 = this.bPreResult1;
            info.bPreResult2 = this.bPreResult2;
            info.ptPreOffset1 = this.ptPreOffset1;
            info.ptPreOffset2 = this.ptPreOffset2;
            info.dPreAngleOffset = this.dPreAngleOffset;
            info.dtLastCycleIn = this.dtLastCycleIn;
            info.dtLastCycleOut = this.dtLastCycleOut;

            for (int nCnt = 0; nCnt < (int)MCDF.eStage.MAX; nCnt++)
            {
                info.bTopAlignResult[nCnt] = this.bTopAlignResult[nCnt];
                info.xyTopAlignOffset[nCnt].x = this.xyTopAlignOffset[nCnt].x;
                info.xyTopAlignOffset[nCnt].y = this.xyTopAlignOffset[nCnt].y;
            }
            info.dTopAngleOffset = this.dTopAngleOffset;
            info.bTopAlignCorrectResult = this.bTopAlignCorrectResult;
            info.xyTopAlignCorrectOffset = this.xyTopAlignCorrectOffset;

            for (int nRow = 0; nRow < this.UnitArr.Length; nRow++)
            {
                for (int nCol = 0; nCol < this.UnitArr[nRow].GetLength(0); nCol++)
                {
                    info.UnitArr[nRow][nCol].BALL_COORDI.X = this.UnitArr[nRow][nCol].BALL_COORDI.X;
                    info.UnitArr[nRow][nCol].BALL_COORDI.Y = this.UnitArr[nRow][nCol].BALL_COORDI.Y;
                    info.UnitArr[nRow][nCol].BALL_COORDI.T = this.UnitArr[nRow][nCol].BALL_COORDI.T;

                    info.UnitArr[nRow][nCol].BALL_SIZE.Width = this.UnitArr[nRow][nCol].BALL_SIZE.Width;
                    info.UnitArr[nRow][nCol].BALL_SIZE.Height = this.UnitArr[nRow][nCol].BALL_SIZE.Height;

                    info.UnitArr[nRow][nCol].BTM_INSP_RESULT = this.UnitArr[nRow][nCol].BTM_INSP_RESULT;
                    info.UnitArr[nRow][nCol].BTM_NG_CODE = this.UnitArr[nRow][nCol].BTM_NG_CODE;

                    info.UnitArr[nRow][nCol].COL = this.UnitArr[nRow][nCol].COL;
                    info.UnitArr[nRow][nCol].ROW = this.UnitArr[nRow][nCol].ROW;

                    info.UnitArr[nRow][nCol].ITS_XOUT = this.UnitArr[nRow][nCol].ITS_XOUT;
                    info.UnitArr[nRow][nCol].IS_UNIT = this.UnitArr[nRow][nCol].IS_UNIT;

                    info.UnitArr[nRow][nCol].ISBALLINSP = this.UnitArr[nRow][nCol].ISBALLINSP;
                    info.UnitArr[nRow][nCol].ISMAPINSP = this.UnitArr[nRow][nCol].ISMAPINSP;
                    info.UnitArr[nRow][nCol].ISSCRAP = this.UnitArr[nRow][nCol].ISSCRAP;

                    info.UnitArr[nRow][nCol].MAP_COORDI.X = this.UnitArr[nRow][nCol].MAP_COORDI.X;
                    info.UnitArr[nRow][nCol].MAP_COORDI.Y = this.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    info.UnitArr[nRow][nCol].MAP_COORDI.T = this.UnitArr[nRow][nCol].MAP_COORDI.T;

                    info.UnitArr[nRow][nCol].MAP_SIZE.Width = this.UnitArr[nRow][nCol].MAP_SIZE.Width;
                    info.UnitArr[nRow][nCol].MAP_SIZE.Height = this.UnitArr[nRow][nCol].MAP_SIZE.Height;

                    info.UnitArr[nRow][nCol].OUT_PORT = this.UnitArr[nRow][nCol].OUT_PORT;

                    info.UnitArr[nRow][nCol].SAW_OFFSET_LB.X = this.UnitArr[nRow][nCol].SAW_OFFSET_LB.X;
                    info.UnitArr[nRow][nCol].SAW_OFFSET_LB.Y = this.UnitArr[nRow][nCol].SAW_OFFSET_LB.Y;
                    info.UnitArr[nRow][nCol].SAW_OFFSET_LT.X = this.UnitArr[nRow][nCol].SAW_OFFSET_LT.X;
                    info.UnitArr[nRow][nCol].SAW_OFFSET_LT.Y = this.UnitArr[nRow][nCol].SAW_OFFSET_LT.Y;
                    info.UnitArr[nRow][nCol].SAW_OFFSET_RB.X = this.UnitArr[nRow][nCol].SAW_OFFSET_RB.X;
                    info.UnitArr[nRow][nCol].SAW_OFFSET_RB.Y = this.UnitArr[nRow][nCol].SAW_OFFSET_RB.Y;
                    info.UnitArr[nRow][nCol].SAW_OFFSET_RT.X = this.UnitArr[nRow][nCol].SAW_OFFSET_RT.X;
                    info.UnitArr[nRow][nCol].SAW_OFFSET_RT.Y = this.UnitArr[nRow][nCol].SAW_OFFSET_RT.Y;

                    info.UnitArr[nRow][nCol].SUB_ID = this.UnitArr[nRow][nCol].SUB_ID;
                    info.UnitArr[nRow][nCol].TOP_INSP_RESULT = this.UnitArr[nRow][nCol].TOP_INSP_RESULT;
                    info.UnitArr[nRow][nCol].TOP_NG_CODE = this.UnitArr[nRow][nCol].TOP_NG_CODE;
                }
            }
        }

        //protected virtual StripInfo DeepCopy()
        //{
        //    StripInfo info = (StripInfo)this.MemberwiseClone();

        //    // Deep-copy children
        //    info.UnitArr = new UnitInfo[UnitArr.Length][];

        //    for (int i = 0; i < UnitArr.Length; i++)
        //    {
        //        info.UnitArr[i] = new UnitInfo[UnitArr[i].GetLength(0)];
        //        UnitArr[i].CopyTo(info.UnitArr[i], 0);
        //    }

        //    return info;
        //}

        //public StripInfo Clone()
        //{
        //    return DeepCopy();
        //}

        //object ICloneable.Clone()
        //{
        //    return DeepCopy();
        //}

        //public System.Collections.IEnumerator GetEnumerator()
        //{
        //    for (int i = 0; i < UnitArr.Length; i++)    // 배열길이 만큼 for문을 돌면서 array의 요소를 반환
        //        yield return UnitArr[i];            // yield는 foreach 문이 반복자를 이용해 접근한 위치를 기억
        //}
    }

    public class TrayInfo : ICloneable
    {
        public string Tray_2D_Code = "";

        public int nArraySize = 0;

        public UnitInfo[][] UnitArr;    // 맴버 배열 array 선언

        public TrayInfo()
        {
            //
        }

        public int nGdQty = 0;
        public int nRwQty = 0;
        public int nLossQty = 0;
        public int nTotalQty = 0;
        public int nMapQty = 0;
        public int nBallQty = 0;

        public string strGrade = "";
        public string stripId = "";
        public string strIsUnitRow = "";
        public string strIsUnitCol = "";
        public string strBinCode = "";
        public int nRowIndex = 0;
        public int nColIndex = 0;

        public void GetTrayResult()
        {
            try
            {

                nGdQty = 0;
                nRwQty = 0;
                nLossQty = 0;
                nTotalQty = 0;
                nMapQty = 0;
                nBallQty = 0;

                strGrade = "";
                stripId = "";
                strIsUnitRow = "";
                strIsUnitCol = "";
                strBinCode = "";
                nRowIndex = 0;
                nColIndex = 0;

                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    nColIndex = 0;
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].ITS_XOUT == (int)MESDF.eCELL_STATUS.OK)
                        {
                            if (UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK &&
                                UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                            {
                                nGdQty++;
                            }
                            else if (UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.RW ||
                                     (UnitArr[nRow][nCol].TOP_INSP_RESULT != (int)VSDF.eJUDGE_BALL.NG &&
                                     UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.RW))
                            {
                                nRwQty++;
                            }
                            else
                            {
                                nLossQty++;
                            }
                        }
                        else
                        {
                            if (UnitArr[nRow][nCol].ITS_XOUT != 0)
                            {
                                nLossQty++;
                            }
                        }

                        if (UnitArr[nRow][nCol].IS_UNIT)
                        {
                            strGrade += "01,";
                            stripId += UnitArr[nRow][nCol].SUB_ID + ",";
                            strIsUnitCol += nRowIndex.ToString().PadLeft(2, '0');
                            strIsUnitRow += nColIndex.ToString().PadLeft(2, '0');
                            strBinCode += UnitArr[nRow][nCol].ITS_XOUT.ToString().PadLeft(2, '0');
                            nTotalQty++;

                            if (UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK)
                            {
                                nMapQty++;
                            }

                            if (UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                            {
                                nBallQty++;
                            }
                        }
                        else
                        {
                            strGrade += "00,";
                            stripId += "0,";
                            strIsUnitCol += "00";
                            strIsUnitRow += "00";
                            strBinCode += "00";
                        }
                        strGrade = strGrade.Substring(0, strGrade.Length - 1);
                        stripId = stripId.Substring(0, stripId.Length - 1);

                        nColIndex++;
                    }
                    nRowIndex++;
                }

            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 양품 수량을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int GOOD_QTY()
        {
            int nQty = 0;
            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK 
                            && UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                        {
                            nQty++;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return nQty;
        }


        /// <summary>
        /// 스트립 단위 불량인 수량을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int LOSS_QTY()
        {
            int nQty = 0;
            try
            {

                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].TOP_INSP_RESULT != (int)VSDF.eJUDGE_MAP.OK && UnitArr[nRow][nCol].BTM_INSP_RESULT != (int)VSDF.eJUDGE_BALL.OK)
                        {
                            nQty++;
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
            return nQty;
        }

        /// <summary>
        /// 트레이에 적재된 유닛의 수량을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int TOTAL_QTY()
        {
            int nQty = 0;

            try
            {


                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].IS_UNIT)
                        {
                            nQty++;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return nQty;
        }


        /// <summary>
        ///          ← 시작 (Upper Right(UR))
        /// ***********＼ ↓ 
        /// ************  
        /// ************
        /// </summary>
        /// <param name="nRow"></param>
        /// <returns></returns>
        public string CELL_EXIST(int nRow)
        {
            string strValue = "";

            try
            {


                for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                {
                    strValue += Convert.ToInt32(UnitArr[nRow][nCol].IS_UNIT).ToString();
                }
            }
            catch (Exception)
            {
            }
            return strValue;
        }


        /// <summary>
        ///          ← 시작 (Upper Right(UR))
        /// ***********＼ ↓ 
        /// ************  
        /// ************
        /// </summary>
        /// <returns></returns>
        public string CELL_GRADE()
        {
            string strValue = "";

            try
            {


                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].IS_UNIT)
                        {
                            strValue += "01,";
                        }
                        else
                        {
                            strValue += "00,";
                        }
                    }
                }

                strValue = strValue.Substring(0, strValue.Length - 1);
            }
            catch (Exception)
            {
            }
            return strValue;
        }

        /// <summary>
        ///          ← 시작 (Upper Right(UR))
        /// ***********＼ ↓ 
        /// ************  
        /// ************
        /// </summary>
        /// <returns></returns>
        public string CELL_SUB_ID()
        {
            string strValue = "";

            try
            {

                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].IS_UNIT)
                        {
                            strValue += UnitArr[nRow][nCol].SUB_ID + ",";
                        }
                        else
                        {
                            strValue += "0,";
                        }
                    }
                }
                strValue = strValue.Substring(0, strValue.Length - 1);
            }
            catch (Exception)
            {
            }

            return strValue;
        }

        /// <summary>
        ///          ← 시작 (Upper Right(UR))
        /// ***********＼ ↓ 
        /// ************  
        /// ************
        /// </summary>
        /// <returns></returns>
        public string CELL_EXIST_COL()
        {
            string strValue = "";
            try
            {
                //설비 입장에서는 X, Y좌표와 방향이 뒤집힌 격이나,
                //상위보고시는 0번부터 순서대로 보고해야하므로 별도의 변수 선언
                int nRowIndex = 0;

                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].IS_UNIT)
                        {
                            strValue += nRowIndex.ToString("00");
                        }
                        else
                        {
                            strValue += "00";
                        }
                    }
                    nRowIndex++;
                }

            }
            catch (Exception)
            {
            }
            return strValue;
        }

        /// <summary>
        ///          ← 시작 (Upper Right(UR))
        /// ***********＼ ↓ 
        /// ************  
        /// ************
        /// </summary>
        /// <returns></returns>
        public string CELL_EXIST_ROW()
        {
            string strValue = "";

            try
            {
                int nColIndex = 0;
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    nColIndex = 0;
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].IS_UNIT)
                        {
                            strValue += nColIndex.ToString("00");
                        }
                        else
                        {
                            strValue += "00";
                        }
                        nColIndex++;
                    }
                }

            }
            catch (Exception)
            {
            }
            return strValue;
        }



        /// <summary>
        ///          ← 시작 (Upper Right(UR))
        /// ***********＼ ↓ 
        /// ************  
        /// ************
        /// </summary>
        /// <returns></returns>
        //CELL_STATUS
        public string CELL_BIN_CODE()
        {
            string strValue = "";

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].IS_UNIT)
                        {
                            strValue += UnitArr[nRow][nCol].ITS_XOUT.ToString("00");
                        }
                        else
                        {
                            strValue += "00";
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
            return strValue;
        }


        public int OK_BALL_QTY()
        {
            int nQty = 0;

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                        {
                            nQty++;
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
            return nQty;
        }


        public int OK_MAP_QTY()
        {
            int nQty = 0;

            try
            {
                for (int nRow = UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        if (UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK)
                        {
                            nQty++;
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
            return nQty;
        }

        public void Init(int nSize)
        {
            nArraySize = nSize;
            UnitArr = new UnitInfo[nSize][];
        }

        public void Set(int nColCnt)    // 인덱스와 값을 매개변수로 받아 해당하는 인덱스에 값을 저장하는 매써드
        {
            try
            {
                for (int nRow = 0; nRow < UnitArr.Length; nRow++)
                {
                    UnitInfo[] unit = new UnitInfo[nColCnt];

                    for (int nCol = 0; nCol < nColCnt; nCol++)
                    {
                        unit[nCol] = new UnitInfo();

                        unit[nCol].ROW = nRow;
                        unit[nCol].COL = nCol;
                    }

                    UnitArr[nRow] = unit;
                }

            }
            catch (Exception)
            {
            }
        }

        public void Clear()
        {
            //for (int nRow = 0; nRow < UnitArr.Length; nRow++)
            //{
            //    Array.Clear(UnitArr[nRow], 0, UnitArr[nRow].Length);
            //}
            for (int nRow = 0; nRow < UnitArr.Length; nRow++)
            {
                UnitInfo[] unit = new UnitInfo[UnitArr[0].Length];

                for (int nCol = 0; nCol < UnitArr[0].Length; nCol++)
                {
                    unit[nCol] = new UnitInfo();

                    unit[nCol].ROW = nRow;
                    unit[nCol].COL = nCol;
                }

                UnitArr[nRow] = unit;
            }
        }



        public void CopyTo(ref TrayInfo info)
        {
            info.nArraySize = this.nArraySize;
            info.Tray_2D_Code = this.Tray_2D_Code;

            for (int nRow = 0; nRow < this.UnitArr.Length; nRow++)
            {
                for (int nCol = 0; nCol < this.UnitArr[nRow].GetLength(0); nCol++)
                {
                    info.UnitArr[nRow][nCol] = this.UnitArr[nRow][nCol];
                }
            }
        }

        protected virtual TrayInfo DeepCopy()
        {
            TrayInfo info = (TrayInfo)this.MemberwiseClone();

            // Deep-copy children
            info.UnitArr = new UnitInfo[nArraySize][];

            //foreach (MdInfo[] md in Arr)
            //{
            //    other.Arr[0] = new MdInfo[Arr[0].GetLength(0)];
            //    md.CopyTo(other.Arr[0], 0);
            //}

            for (int i = 0; i < UnitArr.Length; i++)
            {
                info.UnitArr[i] = new UnitInfo[UnitArr[i].GetLength(0)];
                UnitArr[i].CopyTo(info.UnitArr[i], 0);
            }

            return info;
        }

        public TrayInfo Clone()
        {
            return DeepCopy();
        }

        object ICloneable.Clone()
        {
            return DeepCopy();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            for (int i = 0; i < UnitArr.Length; i++)    // 배열길이 만큼 for문을 돌면서 array의 요소를 반환
                yield return UnitArr[i];            // yield는 foreach 문이 반복자를 이용해 접근한 위치를 기억
        }
    }

    public class PickerPadInfo
    {
        public Coordi MAP_COORDI { get; set; }
        public Coordi BALL_COORDI { get; set; }
        public Size MAP_SIZE { get; set; }
        public Size BALL_SIZE { get; set; }

        public Offset SAW_OFSSET { get; set; }

        public bool IS_PICKER_SKIP { get; set; }

        public int ROW { get; set; }
        public int COL { get; set; }

        public int HOST_CODE { get; set; }
        public int TOP_INSP_RESULT { get; set; }

        public int BTM_INSP_RESULT { get; set; }

        string nTopNgCode = "00";
        public string TOP_NG_CODE
        {
            get { return nTopNgCode; }
            set { nTopNgCode = value; }
        }

        string strBtmNgCode = "00";
        public string BTM_NG_CODE
        {
            get { return strBtmNgCode; }
            set { strBtmNgCode = value; }
        } 

        public PickerPadInfo()
        {
            MAP_COORDI = new Coordi();
            BALL_COORDI = new Coordi();
            MAP_SIZE = new Size();
            BALL_SIZE = new Size();
            SAW_OFSSET = new Offset();
        }
    }

    public class UnitInfo
    {   
        string strSubId = "";

        public string SUB_ID
        {
            get { return strSubId; }
            set { strSubId = value; }
        }

        public bool IS_UNIT { get; set; }
        public bool IS_SKIP_UNIT { get; set; }//220621pjh
        public Coordi MAP_COORDI { get; set; }
        public Coordi BALL_COORDI { get; set; }

        public PkgSize MAP_SIZE { get; set; }
        public PkgSize BALL_SIZE { get; set; }

        public Offset SAW_OFFSET_LT { get; set; }
        public Offset SAW_OFFSET_RT { get; set; }
        public Offset SAW_OFFSET_LB { get; set; }
        public Offset SAW_OFFSET_RB { get; set; }


        public int ROW { get; set; }
        public int COL { get; set; }    
        
        public int TRAY_MEASURE_CODE { get; set; }
        /// <summary>
        /// 하이닉스의 경우 1 = SQ, 2 = NQ, 4= XOUT
        /// 기존 배출 판단식 유지하기 위해 정상 셀일경우 1, ITS XOUT 유닛일 경우 본 변수 4로 설정
        /// 4일경우 리젝
        /// </summary>
        public int ITS_XOUT { get; set; }

        int nTopInspResult = -1;
        public int TOP_INSP_RESULT 
        { 
            get { return nTopInspResult; } 
            set { nTopInspResult = value; } 
        }

        int nBtmInspResult = -1;
        public int BTM_INSP_RESULT
        {
            get { return nBtmInspResult; }
            set { nBtmInspResult = value; }
        }

        string nTopNgCode = "00";
        public string TOP_NG_CODE 
        { 
            get { return nTopNgCode; } 
            set { nTopNgCode = value; } 
        }

        string strBtmNgCode = "00";
        public string BTM_NG_CODE 
        { 
            get { return strBtmNgCode; } 
            set { strBtmNgCode = value; } 
        }

        public int OUT_PORT { get; set; }

        public bool ISSCRAP { get; set; }

        public bool ISBALLINSP { get; set; }

        public bool ISMAPINSP { get; set; }

        public UnitInfo()
        {
            OUT_PORT = (int)MCDF.eOutPort.NONE;
            MAP_COORDI = new Coordi();
            BALL_COORDI = new Coordi();
            MAP_SIZE = new PkgSize();
            BALL_SIZE = new PkgSize();
            SAW_OFFSET_LT = new Offset();
            SAW_OFFSET_RT = new Offset();
            SAW_OFFSET_LB = new Offset();
            SAW_OFFSET_RB = new Offset();
        }

        public void CopyTo(ref UnitInfo info)
        {
            info.SUB_ID = this.SUB_ID;
            info.OUT_PORT = this.OUT_PORT;

            info.MAP_COORDI.X = this.MAP_COORDI.X;
            info.MAP_COORDI.Y = this.MAP_COORDI.Y;
            info.MAP_COORDI.T = this.MAP_COORDI.T;

            info.BALL_COORDI.X = this.BALL_COORDI.X;
            info.BALL_COORDI.Y = this.BALL_COORDI.Y;
            info.BALL_COORDI.T = this.BALL_COORDI.T;

            info.SAW_OFFSET_LT.X = this.SAW_OFFSET_LT.X;
            info.SAW_OFFSET_LT.Y = this.SAW_OFFSET_LT.Y;

            info.BALL_SIZE.Width = this.BALL_SIZE.Width;
            info.BALL_SIZE.Height = this.BALL_SIZE.Height;

            info.MAP_SIZE.Width = this.MAP_SIZE.Width;
            info.MAP_SIZE.Height = this.MAP_SIZE.Height;

            info.IS_SKIP_UNIT = this.IS_SKIP_UNIT;
            info.IS_UNIT = this.IS_UNIT;
            info.ROW = this.ROW;
            info.COL = this.COL;
            info.ITS_XOUT = this.ITS_XOUT;
            info.TOP_INSP_RESULT = this.TOP_INSP_RESULT;
            info.BTM_INSP_RESULT = this.BTM_INSP_RESULT;
            info.TOP_NG_CODE = this.TOP_NG_CODE;
            info.BTM_NG_CODE = this.BTM_NG_CODE;
        }
    }


    public class Coordi
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double T { get; set; }

        // EDGE의 OFFSET 위치 (LT, RT, LB, RB)
        public double[] EDGE_X { get; set; }
        public double[] EDGE_Y { get; set; }

        public Coordi()
        {
            EDGE_X = new double[4];
            EDGE_Y = new double[4];
        }
    }

    public class PkgSize
    {
        public double Height { get; set; }
        public double Width{ get; set; }
    }
    
    public class Offset
    {
        public double X { get; set; }
        public double Y { get; set; }

    }
	
    [Serializable]
    public enum INSPECTION_DIR
    {
        LEFT,
        TOP,
        RIGHT,
        BOTTOM,
    }

    [Serializable]
    public class MCSTATEDBIf
    {
        public enum DB_MCSTATE
        {
            MC_STATE_EMO = 0,
            MC_STATE_Door = 1,

            PM = 10,

            EMO_Monitor = 15,

            Door_1_Open_Monitor = 21,
            Door_2_Open_Monitor = 22,
            Door_3_Open_Monitor = 23,
            Door_4_Open_Monitor = 24,
            Door_5_Open_Monitor = 25,
            Door_6_Open_Monitor = 26,
            Door_7_Open_Monitor = 27,
            Door_8_Open_Monitor = 28,
        }
    }

    /// <summary>
    /// Inspection NG 시 선택
    /// </summary>
    [Serializable]
    public enum INSPECTION_NG_SELECT
    {
        NONE = 0,
        OK,
        NG,
    }



    //EES DEF
    public enum eONLINE_STATUS
    {
        OFFLINE =0,
        ONLINE,
    }
    public enum eEQP_STATUS
    {
        IDLE = 1,
        RUN,
        STOP,
        SUDDENSTOP,
    }

    public enum eEES_MODE
    {
        OFFLINE = 0,
        TRACKIN,
        RMS,
    }
    public enum eSHEET_TYPE
    {
        P = 0, //PRODUCTION = 0,
        S, //SAMPLE,
    }
    public enum eLOSS_STATUS
    {
        LOSS_1 = 1,
        LOSS_2,
        LOSS_3,
        LOSS_4,
        LOSS_5,
        LOSS_6,
        LOSS_7,
        LOSS_8,
        LOSS_9,
        LOSS_10,
        LOSS_11,
        LOSS_12,
    }

    public enum eALARM_STATUS
    {
        SET = 0,
        RESET,
    }

    public enum eEES_FDCID_PARAM : int
    {
        None = 0,
        Main_Air = 1,
        Blow_Air,
        
        Max,
    }
    public enum eMatChangeMode
    {
        RESTORE = 1,
        ADD,
        NEW,
    }
    public enum eGrpCode
    {
        EMC,
        FLUX,
        BALL,
        BLADE
    }

    public class EES_FDC_PARAM
    {
        public string[] FDC_PARAM_INTERVAL = new string[(int)eEES_FDCID_PARAM.Max];
        public string[] FDC_PARAM_ID = new string[(int)eEES_FDCID_PARAM.Max];
        public string[] FDC_PARAM_VALUE = new string[(int)eEES_FDCID_PARAM.Max];

        public int REQ_FDC_PARAM_COUNT = 0;
        public string[] REQ_FDC_PARAM_INTERVAL = new string[10];
        public string[] REQ_FDC_PARAM_ID = new string[10];
        public string[] REQ_FDC_ID= new string[10];

        public EES_FDC_PARAM()
        {
            for(int i =0; i< (int)eEES_FDCID_PARAM.Max; i ++)
            {
                FDC_PARAM_ID[i] = string.Format("{0:D3}", i);
                FDC_PARAM_VALUE[i] = i.ToString();
            }
        }
    }

    public class EES_INFO
    {
        //public eReport[] bReport = new eReport[20];
        public bool[] bReport = new bool[20];
        public bool[] bRequest = new bool[20];
        public bool[] bReply = new bool[20];

//         public string[] FDC_ID = new string[10];
//         public string[] FDC_INTERVAL = new string[10];
//         public string[] FDC_PARAMID = new string[10];

        //public string REQ_TRANS_ID { get; set; }
        //public string REPLY_TRANS_ID { get; set; }

        public string[] REQ_TRANS_ID = new string[10];

        public string SEND_FDC_ID { get; set; }
        public string SEND_FDC_INTERVAL { get; set; }
        public string SEND_FDC_PARAMID { get; set; }
        public string SEND_FDC_VALUE { get; set; }


        public bool CONNECTED { get; set; }
        public int MCSTATE { get; set;} 
        public string SETTIME { get; set;}

        public bool VALID_READ_LOT_FLAG { get; set; }
        public bool VALID_FLAG { get; set; }
        public bool VALID_ERROR { get; set; }
        public bool VALID_RESULT { get; set; }
        public bool TRACKIN_FLAG { get; set; }
        public bool TRACKIN_RESULT { get; set; }
        public string TERMIANL_MSG { get; set; }
        public bool LOSS_POPUP { get; set; }

        public string VALIDATION_LOT_ID { get; set; }
        public string VALIDATION_PRODUCT_MODEL { get; set; }
        public string VALIDATION_RECIPE { get; set; }

        public string NEXT_SHEET_NO { get; set; }
        public string CUR_SHEET_NO { get; set; }
        public string CUR_SHEET_PATH { get; set; }
        public int COUNT_SHEET_NO { get; set; }

        public int COUNT_SHEET_INPUT { get; set; }
        public int COUNT_SHEET_OUTPUT { get; set; }

        public eEQP_STATUS EQP_STATUS { get; set; }
        public eEQP_STATUS EQP_STATUS_OLD { get; set; }
        public eONLINE_STATUS ONLINE_STATUS { get; set; }
        public eEES_MODE EES_MODE { get; set; }
        public eLOSS_STATUS LOSS_STATUS { get; set; }
        public eALARM_STATUS ALARM_STATUS { get; set; }
        public string ALARM_ID { get; set; }
        public string ALARM_TEXT { get; set; }
        public string LOT_ID { get; set; }
        public string PRODCUT_MODEL { get; set; }
        public string RECIPE_ID { get; set; }
        public string USER_ID { get; set; }
        public string SUB_EQP_ID { get; set; }
        //public string FDC_ID { get; set; }
        public int INTERVAL { get; set; }
        public eSHEET_TYPE SHEET_TYPE { get; set; }

        public string FDC_PARA_ID { get; set; }
        public string FDC_PARA_VALUE { get; set; }

        //public EES_FDC_PARAM REQ_FDC_PARAM = new EES_FDC_PARAM();

        public EES_INFO()
        {
            CONNECTED = false;

            EQP_STATUS_OLD = eEQP_STATUS.STOP;
            EQP_STATUS = eEQP_STATUS.IDLE;

            for(int i=0; i<10; i++)
            {
                //REQ_FDC_PARAM.FDC_PARAM_INTERVAL[i] = string.Empty;
                //REQ_FDC_PARAM.FDC_PARAM_ID[i] = string.Empty;
                //REQ_FDC_PARAM.FDC_PARAM_VALUE[i] = string.Empty;
            }

            for(int i=0; i<20; i++)
            {
                bReport[i] = false;
            }
        }
    }

    public class EqRecipeProperties
    {
        public string ItemName = "";
        public int Modified = 0;
        public int UseOption = 0;
        public int Confirm = 0;
        public string Before = "";
        public string After = "";

        public EqRecipeProperties()
        {
            ItemName = "";
            Modified = 0;
            UseOption = 0;
            Confirm = 0;
            Before = "";
            After = "";
        }
    }

    public class HostRecipeProperties
    {
        public string ItemName = "";
        public int Modified = 0;
        public string Value = "";

        public HostRecipeProperties()
        {
            ItemName = "";
            Modified = 0;
            Value = "";
        }
    }

    public class ErrStruct
    {
        public int No = 0;
        public int Set = 0;

        public ErrStruct(int nNo, int Set)
        {
            this.No = nNo;
            this.Set = Set;
        }
    }

    public class TrayBatchInfo
    {
        int nQty = 0;
        string strModel = "";
        string strTrayRfid = "";

        public int QTY
        {
            get { return nQty; }
            set {nQty = value; }
        }

        public string MODEL
        {
            get { return strModel; }
            set { strModel = value; }
        }

        public string TRAY_RFID
        {
            get { return strTrayRfid; }
            set { strTrayRfid = value; }
        }

        public void Clear()
        {
            nQty = 0;
            strModel = "";
            strTrayRfid = "";
        }
    }

    public class LabelPrint
    {
        public string CONFIRM_FLAG { get; set; }
        public long EQMSGID { get; set; }
        public string LOT_ID { get; set; }
        public string RCMD { get; set; }

        public string COMMENT { get; set; }
        public List<string> BIN_SORTING { get; set; }
        public string DEVICE { get; set; }
        public string FAB { get; set; }
        public string GRADE { get; set; }
        public string LABEL_TITLE { get; set; }
        public string LOT_NO { get; set; }
        public string NULL { get; set; }
        public List<string> OPER_CODE { get; set; }
        public List<string> OPER_DESC { get; set; }
        public int ORG { get; set; }
        public string OWNER { get; set; }
        public List<string> PI_NO { get; set; }
        public string PKG_SIZE { get; set; }
        public string PKG_TYPE1_LEAD { get; set; }
        public string PRODUCT { get; set; }
        public string TO_FLOW { get; set; }
        public List<int> UNIT_QTY { get; set; }





        public static string getPrintString(LabelPrint lb)
        {
            string PRINT_STRING = string.Empty;

            PRINT_STRING = PRINT_STRING + "^XA";

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO80,40^FDLOT TRAVELLER CARD^FS";
            PRINT_STRING = PRINT_STRING + "^FO60,90^FD==> TO^FS";
            PRINT_STRING = PRINT_STRING + "^CFA,20";
            PRINT_STRING = PRINT_STRING + String.Format("^FO170,90^FD{0}^FS", lb.TO_FLOW);

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO60,140^FDProduct^FS";
            PRINT_STRING = PRINT_STRING + "^CFA,20";
            PRINT_STRING = PRINT_STRING + String.Format("^FO40,170^FD{0}^FS", lb.PRODUCT);

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO60,260^FDPKG Type1/Lead^FS";
            PRINT_STRING = PRINT_STRING + "^CFA,20";
            PRINT_STRING = PRINT_STRING + String.Format("^FO60,290^FD{0}^FS", lb.PKG_TYPE1_LEAD);

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO50,350^FDFAB^FS";
            PRINT_STRING = PRINT_STRING + "^CFA,20";
            PRINT_STRING = PRINT_STRING + String.Format("^FO50,380^FD{0}^FS", lb.FAB);

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO140,350^FDGrade^FS";
            PRINT_STRING = PRINT_STRING + "^CFA,20";
            PRINT_STRING = PRINT_STRING + String.Format("^FO140,380^FD{0}^FS", lb.GRADE);

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO240,350^FDOwner^FS";
            PRINT_STRING = PRINT_STRING + "^CFA,20";
            PRINT_STRING = PRINT_STRING + String.Format("^FO240,380^FD{0}^FS", lb.OWNER);


            PRINT_STRING = PRINT_STRING + "^FO430,40^FDLot no : ^FS";
            PRINT_STRING = PRINT_STRING + String.Format("^FO540,40^FD{0}^FS", lb.LOT_ID);
            PRINT_STRING = PRINT_STRING + "^FX Third section with bar code.";
            PRINT_STRING = PRINT_STRING + "^BY2,2,100";
            PRINT_STRING = PRINT_STRING + String.Format("^FO400,100^BCN,100,N,N,N^FD{0}^FS", lb.LOT_ID);

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO400,260^FDDevice : ^FS";
            PRINT_STRING = PRINT_STRING + "^CFA,20";
            PRINT_STRING = PRINT_STRING + String.Format("^FO400,290^FD{0}^FS", lb.DEVICE);

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO400,350^FDPKG Size/Top Thickness^FS";
            PRINT_STRING = PRINT_STRING + "^CFA,20";
            PRINT_STRING = PRINT_STRING + String.Format("^FO400,380^FD{0}^FS", lb.PKG_SIZE);

            PRINT_STRING = PRINT_STRING + "^CF0,16";
            PRINT_STRING = PRINT_STRING + string.Format("^FO40,440^FDSpecial Notice / Inform : {0}^FS", "");

            PRINT_STRING = PRINT_STRING + "^FX Fourth section (the two boxes on the bottom).";
            PRINT_STRING = PRINT_STRING + "^FO20,20^GB300,50,2^FS";
            PRINT_STRING = PRINT_STRING + "^FO20,70^GB300,50,2^FS";
            PRINT_STRING = PRINT_STRING + "^FO20,120^GB300,120,2^FS";
            PRINT_STRING = PRINT_STRING + "^FO20,240^GB300,90,2^FS";

            PRINT_STRING = PRINT_STRING + "^FO20,330^GB100,90,2^FS";
            PRINT_STRING = PRINT_STRING + "^FO120,330^GB100,90,2^FS";
            PRINT_STRING = PRINT_STRING + "^FO220,330^GB100,90,2^FS";

            PRINT_STRING = PRINT_STRING + "^FO340,20^GB410,220,2^FS";
            PRINT_STRING = PRINT_STRING + "^FO340,240^GB410,90,2^FS";
            PRINT_STRING = PRINT_STRING + "^FO340,330^GB410,90,2^FS";

            PRINT_STRING = PRINT_STRING + "^FO20,420^GB730,50,2^FS";

            PRINT_STRING = PRINT_STRING + "^FO20,480^GB730,400,3^FS";

            PRINT_STRING = PRINT_STRING + "^FO120,480^GB3,400,3^FS";
            PRINT_STRING = PRINT_STRING + "^FO240,480^GB3,400,3^FS";
            PRINT_STRING = PRINT_STRING + "^FO440,480^GB3,400,3^FS";
            PRINT_STRING = PRINT_STRING + "^FO560,480^GB3,400,3^FS";

            PRINT_STRING = PRINT_STRING + "^FO20,538^GB730,3,3,B^FS";
            PRINT_STRING = PRINT_STRING + "^FO20,595^GB730,3,3,B^FS";
            PRINT_STRING = PRINT_STRING + "^FO20,652^GB730,3,3,B^FS";
            PRINT_STRING = PRINT_STRING + "^FO20,709^GB730,3,3,B^FS";
            PRINT_STRING = PRINT_STRING + "^FO20,766^GB730,3,3,B^FS";
            PRINT_STRING = PRINT_STRING + "^FO20,823^GB730,3,3,B^FS";

            PRINT_STRING = PRINT_STRING + "^CF0,20";
            PRINT_STRING = PRINT_STRING + "^FO50,505^FDOPER           DESC                   P/I No                     QTY                COMMENT^FS";

            PRINT_STRING = PRINT_STRING + "^CFA,20";
            for (int i = 0; i < lb.OPER_CODE.Count; i++)
            {
                PRINT_STRING = PRINT_STRING + string.Format("^FO30,{0}^FD{1}^FS", 563 + (57*i), lb.OPER_CODE[i]);//1234567
            }
            for (int i = 0; i < lb.OPER_DESC.Count; i++)
            {
                PRINT_STRING = PRINT_STRING + string.Format("^FO130,{0}^FD{1}^FS", 563 + (57 * i), lb.OPER_DESC[i]);//12345abcA
            }
            for (int i = 0; i < lb.PI_NO.Count; i++)
            {
                PRINT_STRING = PRINT_STRING + string.Format("^FO250,{0}^FD{1}^FS", 563 + (57 * i), lb.PI_NO[i]);//12345abcA12345A
            }
            for (int i = 0; i < lb.UNIT_QTY.Count; i++)
            {
                PRINT_STRING = PRINT_STRING + string.Format("^FO450,{0}^FD{1}^FS", 563 + (57 * i), lb.UNIT_QTY[i]);//12345abcA
            }
            for (int i = 0; i < lb.BIN_SORTING.Count; i++)
            {
                PRINT_STRING = PRINT_STRING + string.Format("^FO570,{0}^FD{1}^FS", 563 + (57 * i), lb.BIN_SORTING[i]);//12345abcA12345
            }

            PRINT_STRING = PRINT_STRING + "^XZ";

            return PRINT_STRING;
        }


        public static bool PrintLabel(LabelPrint lb)
        {
            bool bRet = true;
            try
            {
                PrintDialog pd = new PrintDialog();
                pd.PrinterSettings = new PrinterSettings();

                RawPrinter.SendToPrinter(getPrintString(lb), pd.PrinterSettings.PrinterName);

            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }
    }
    
	/// <summary>
    /// Vision과 UDP 통신 시 UI에 표시하기 위함
    /// </summary>
    public class MessageLog
    {
        public bool _bSend = false;
        public DateTime _dtCreate;
        public string _strMessage = "";

        public MessageLog()
        {
            _bSend = false;
            _dtCreate = DateTime.Now;
            _strMessage = "";
        }

        public MessageLog(bool bSend, string strMsg)
        {
            _bSend = bSend;
            _dtCreate = DateTime.Now;
            _strMessage = strMsg;
        }
    }

    public class VisionMoveCmd
    {
        public IFVision.E_VS_RECV_CMD _cmdRecv = IFVision.E_VS_RECV_CMD.MAPBLOCK1_TEACHING;
        public double _dPos = 0.0;

        public VisionMoveCmd()
        {
            _cmdRecv = IFVision.E_VS_RECV_CMD.MAPBLOCK1_TEACHING;
            _dPos = 0.0;
        }

        public VisionMoveCmd(IFVision.E_VS_RECV_CMD cmd, double dPos = 0.0)
        {
            _cmdRecv = cmd;
            _dPos = dPos;
        }
    }
}