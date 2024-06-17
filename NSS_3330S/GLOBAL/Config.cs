using DionesTool.Objects;
//using NSS_3330S.VISION;
using System;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NSS_3330S
{
    [Serializable]
    public class Config
    {
        public CfgServo[] dMotPos = new CfgServo[SVDF.SV_CNT];

        public CfgGeneral General = new CfgGeneral();
        public CfgMotionData[] MotData = new CfgMotionData[SVDF.SV_CNT];
        public VacuumOption[] Vac = new VacuumOption[(int)IODF.A_INPUT.MAX];
        public AnalogOut[] AOut = new AnalogOut[(int)IODF.A_OUTPUT.MAX];
        public CfgPickerHeadOffset[] OffsetPnP = new CfgPickerHeadOffset[2];
        public CfgCamCenterOffset[] OffsetCnP = new CfgCamCenterOffset[2];

        public OptionItem[] itemOptions = new OptionItem[(int)OPTNUM.MAX];

        public double[][][][] dOffsetXYZT = new double[2][][][];

        public double dPickUpZerosetStartHead1Z = 0;
        public double dPickUpZerosetStepHead1Z = 0;

        public double dPickUpZerosetStartHead2Z = 0;
        public double dPickUpZerosetStepHead2Z = 0;

        public double dPlaceZerosetStartHead1Z = 0;
        public double dPlaceZerosetStepHead1Z = 0;

        public double dPlaceZerosetStartHead2Z = 0;
        public double dPlaceZerosetStepHead2Z = 0;

        public Config()
        {
            for (int nMotCount = 0; nMotCount < SVDF.SV_CNT; nMotCount++)
            {
                dMotPos[nMotCount] = new CfgServo();
            }

            for (int nVacCount = 0; nVacCount < (int)IODF.A_INPUT.MAX; nVacCount++)
            {
                Vac[nVacCount] = new VacuumOption();
            }
            for (int nAOutCount = 0; nAOutCount < (int)IODF.A_OUTPUT.MAX; nAOutCount++)
            {
                AOut[nAOutCount] = new AnalogOut();
            }
            OffsetPnP[0] = new CfgPickerHeadOffset();
            OffsetPnP[1] = new CfgPickerHeadOffset();

            OffsetCnP[0] = new CfgCamCenterOffset();
            OffsetCnP[1] = new CfgCamCenterOffset();

            InitItemOptions();

            for (int i = 0; i < MCDF.HEAD_MAX; i++)
            {
                dOffsetXYZT[i] = new double[MCDF.TABLE_NO_MAX][][]; // 3차원 배열 초기화
                for (int j = 0; j < MCDF.TABLE_NO_MAX; j++)
                {
                    dOffsetXYZT[i][j] = new double[MCDF.XYZT_MAX][]; // 2차원 배열 초기화
                    for (int k = 0; k < MCDF.XYZT_MAX; k++)
                    {
                        dOffsetXYZT[i][j][k] = new double[MCDF.PAD_MAX]; // 1차원 배열 초기화
                    }
                }
            }
        }
        public void InitItemOptions()
        {
            itemOptions = new OptionItem[(int)OPTNUM.MAX];
            //if (ConfigMgr.Inst.Cfg == null)
            //{
            //    return;
            //}

            for (int i = 0; i < itemOptions.Length; i++)
            {
                switch (i)
                {
                    case (int)OPTNUM.GEM_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "GEM을 사용합니다"); } break;
                    case (int)OPTNUM.VAC_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "배큠 센서를 확인합니다"); } break;
                    case (int)OPTNUM.DRY_RUN_MODE: { itemOptions[i] = new OptionItem(i, "OPERATION", "드라이 런 모드를 사용합니다"); } break;
                    case (int)OPTNUM.DRY_RUN_SAW_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "드라이 런 모드일때 SAW와 인터페이스를 사용합니다."); } break;
                    case (int)OPTNUM.DRY_RUN_RANDOM_SORT: { itemOptions[i] = new OptionItem(i, "OPERATION", "드라이 런 모드일때 자재를 무작위로 배출합니다"); } break;
                    case (int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "드라이 런 모드일때 비젼과 인터페이스를 사용합니다."); } break;

                    case (int)OPTNUM.TOP_VISION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "상부 비전을 사용합니다"); } break;
                    case (int)OPTNUM.TOP_ALIGN_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "상부 얼라인을 사용합니다."); } break;
                    case (int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE: { itemOptions[i] = new OptionItem(i, "OPERATION", "VISION DATA 읽기 전 CLEAR 기능을 사용합니다."); } break;
                    case (int)OPTNUM.BOTTOM_VISION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "하부 비전을 사용합니다"); } break;
                    case (int)OPTNUM.NO_MSG_STOP_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "설비 무언 정지 판단 시간 (단위 : 분)"); } break;
                    case (int)OPTNUM.ULD_MGZ_FULL_CHECK_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "매거진 배출 센서 감지 시간 설정 (단위 : 밀리초)"); } break;
                    case (int)OPTNUM.IN_LET_TABLE_UNUSED_MODE: { itemOptions[i] = new OptionItem(i, "OPERATION", "인렛테이블을 사용합니다."); } break;

                    case (int)OPTNUM.TRAY_INSPECTION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "트레이 유닛 미안착 검사를 사용합니다"); } break;
                    case (int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY: { itemOptions[i] = new OptionItem(i, "OPERATION", "트레이 유닛 미안착 검사 시 딜레이(msec)"); } break;

                    case (int)OPTNUM.EQP_INIT_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "장비 초기화 제한시간(msec)"); } break;
                    case (int)OPTNUM.CYL_MOVE_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "실린더 동작 제한시간(msec)"); } break;
                    case (int)OPTNUM.VAC_CHECK_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "배큠 센서 확인 제한시간(msec)"); } break;
                    case (int)OPTNUM.MOT_MOVE_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "모터 이동 제한시간(msec)"); } break;
                    case (int)OPTNUM.MES_REPLY_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "MES 인터페이스 응답 제한시간(msec)"); } break;

                    case (int)OPTNUM.BOTTOM_VISION_OFFSET_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "하부 비전 시 오프셋을 적용합니다"); } break;
                    case (int)OPTNUM.BLOW_VAC_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "블로우 시 배큠 센서를 확인합니다"); } break;
                    case (int)OPTNUM.ITS_XOUT_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS결과로 XOUT을 배출합니다."); } break;

                    case (int)OPTNUM.DOOR_SAFETY_CHECK_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "설비 전후면 도어 및 EMO SW 상태를 체크 합니다."); } break;
                    case (int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT: { itemOptions[i] = new OptionItem(i, "OPERATION", "스크랩 박스 알람 발생 후 클리어 안되었을 시 재알림하는 시간 (ms)"); } break;
                    case (int)OPTNUM.REJECT_BOX_ALARM_CLEAR_TIME_OUT: { itemOptions[i] = new OptionItem(i, "OPERATION", "리젝 박스 알람 발생 후 클리어 안되었을 시 재알림하는 시간 (ms)"); } break;

                    case (int)OPTNUM.EVENT_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "이벤트 로그 보관 기간"); } break;
                    case (int)OPTNUM.ALARM_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "알람 로그 보관 기간"); } break;
                    case (int)OPTNUM.SEQ_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "시퀀스 로그 보관 기간"); } break;
                    case (int)OPTNUM.LOT_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "랏 로그 보관 기간"); } break;
                    case (int)OPTNUM.ITS_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS 로그 보관 기간"); } break;
                    case (int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "생산 수량 로그 보관 기간"); } break;

                    case (int)OPTNUM.GROUP1_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "GROUP1을 사용합니다"); } break;  //sj.shin 2023-10-31
                    case (int)OPTNUM.MAPVISIONDATA_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "MAP VISION DATA 을 사용합니다"); } break;  //sj.shin 2023-11-02
                    case (int)OPTNUM.AIRKNIFE_RUN_START_ON_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "설비 시작시 에어나이프를 사용합니다"); } break; 
                    case (int)OPTNUM.USE_MAGAZINE_BARCODE: { itemOptions[i] = new OptionItem(i, "OPERATION", "MAGAZINE BARCODE 를 사용합니다."); }break;
                    case (int)OPTNUM.MAGAZINE_BARCODE_READING_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "MAGAZINE BARCODE READING 시간"); } break;

                    case (int)OPTNUM.ITS_ID_STRIP_ID_VALID_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS ID와 STRIP ID의 앞 N자리 비교 옵션을 사용합니다."); } break;
                    case (int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS ID와 STRIP ID의 공통 문자 길이"); } break;
                    case (int)OPTNUM.BOTTOM_VISION_ALARM_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "하부 비전 매칭 실패 시 알람을 사용합니다. 미사용 시 재사용 트레이에 안착합니다."); } break;
                    case (int)OPTNUM.VISION_INTERFACE_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "OPERATION", "하부 비전 인터페이스 재시도 횟수"); } break;//220615
                    case (int)OPTNUM.VISION_INTERFACE_TIMEOUT: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "VISION 인터페이스 응답 제한시간(msec)"); } break;
                    case (int)OPTNUM.SAW_INTERFACE_TIMEOUT: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "SAW 인터페이스 응답 제한시간(msec)"); } break;
                    case (int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "로드 비전 프리얼라인 동작시 그랩 전 딜레이(msec)"); } break;
                    case (int)OPTNUM.LD_VISION_BARCODE_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "로드 비전 바코드 동작시 그랩 전 딜레이(msec)"); } break;
                    case (int)OPTNUM.MAP_VISION_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "맵 비전 비전검사 동작시 그랩 전 딜레이(msec)"); } break;
                    case (int)OPTNUM.BALL_VISION_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "볼 비전 비전검사 동작시 그랩 전 딜레이(msec)"); } break;
                    case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "컨베이어 최대 동작시간(msec)"); } break;
                    case (int)OPTNUM.CONV_ROLLING_TIME: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "컨베이어 도착 센서감지 후 컨베이어 돌리는 시간(msec)"); } break;
                    case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "컨베이어 최대 동작시간(msec)을 사용 합니다."); } break;
                    case (int)OPTNUM.CONV_SAFETY_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "컨베이어 투입구 안전 센서 체크를 사용합니다."); } break;


                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 하부 바코드 읽기 전 딜레이(msec)"); } break;
                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 하부 바코드 읽기 재시도 횟수"); } break;
                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 하부 바코드 읽기 성공 제한시간(msec)"); } break;
                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 하부 바코드 읽기를 사용합니다"); } break;
                    case (int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 푸셔의 오버로드 센서를 사용합니다"); } break;
                    // 로더 레일 상단 매거진 감지 센서 사용 모드
                    case (int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "레일 상단 자재 감지 센서(A접점) 를 사용합니다."); } break;
                    case (int)OPTNUM.STRIP_RAIL_Y_UNLOAD_PRE_MOVE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 피커 픽업 전 로드 레일 Y 언로드 위치로 미리 이동기능을 사용합니다."); } break;

                    case (int)OPTNUM.STRIP_PREALIGN_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 프리얼라인을 사용합니다"); } break;
                    case (int)OPTNUM.STRIP_VISION_BARCODE_READ_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 상부 바코드 읽기를 사용합니다(PRE ALIGN 카메라로 읽음)"); } break;
                    case (int)OPTNUM.STRIP_ORIENT_CHECK_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 오리엔트 검사를 사용합니다"); } break;
                    case (int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립의 프리얼라인 보정 각도 오프셋"); } break;
                    case (int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 프레스 모드를 사용합니다."); } break;

                    case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "스트립 피커의 슬로우 다운 높이(mm)"); } break;
                    case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "스트립 피커의 슬로우 다운 속도(mm/sec)"); } break;
                    case (int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "스트립 피커와 유닛 피커 사이의 인터락 거리(유닛+스트립 mm)"); } break;
                    case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "유닛 피커의 슬로우 다운 높이(mm)"); } break;
                    case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "유닛 피커의 슬로우 다운 속도(mm/sec)"); } break;
                    case (int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "유닛 피커 스크랩 시 1번 위치만 사용합니다."); } break;
                    case (int)OPTNUM.UNIT_PICKER_VACUUM_DELAY: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "다이싱 소우 커팅 테이블 안착 후 진공 대기 시간."); } break;
                    case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "맵 피커의 슬로우 다운 높이(mm)"); } break;
                    case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "맵 피커의 슬로우 다운 속도(mm/sec)"); } break;
                    case (int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "맵 피커 에어 드라이 반복 횟수 "); } break;
                    case (int)OPTNUM.MAP_INSPECTION_CHECK_COUNT: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "맵 인스펙션 실패 시 알람 발생 수량"); } break;

                    case (int)OPTNUM.MAP_STAGE_SIZE_HEIGHT: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵 스테이지의 세로 사이즈(mm)"); } break;
                    case (int)OPTNUM.MAP_STAGE_SIZE_WIDTH: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵 스테이지의 가로 사이즈(mm)"); } break;
                    case (int)OPTNUM.MAP_STAGE_1_USE: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵스테이지 1번을 사용합니다."); } break;
                    case (int)OPTNUM.MAP_STAGE_2_USE: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵스테이지 2번을 사용합니다."); } break;
                    case (int)OPTNUM.MAP_STAGE_AIRKNIFE_USE: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵스테이지 에어나이프 동작을 사용합니다."); } break;
                    case (int)OPTNUM.MAP_STAGE_AIRKNIFE_COUNT: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵스테이지 에어나이프 동작 횟수."); } break;
                    case (int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "픽업 후 T축 미리 이동을 사용합니다"); } break; //220617
                    case (int)OPTNUM.MULTI_PICKER_TTLOG_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "TTLOG를 사용합니다"); } break; //220617
                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 진공을 확인하는 지연시간(msec)"); } break;
                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 진공을 파기하는 시간(msec)"); } break;
                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 진공 파기를 끄고 기다리는 시간(msec)"); } break;
                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 픽업 위치에 이동 후 기다리는 시간(msec)"); } break;
                    case (int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 볼 검사 위치에 이동 후 기다리는 시간(msec)"); } break;
                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 플레이스 위치에 이동 후 기다리는 시간(msec)"); } break;
                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 픽업 실패 시 재시도하는 횟수"); } break;
                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 픽업 실패 시 진공을 확인하는 지연시간(msec)"); } break;
                    case (int)OPTNUM.BOTTOM_CAM_ANGLE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "플레이스 시 각도 오프셋"); } break;

                    case (int)OPTNUM.PICKER_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_1_PAD_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 1번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_1_PAD_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 2번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_1_PAD_3_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 3번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_1_PAD_4_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 4번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_1_PAD_5_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 5번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_1_PAD_6_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 6번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_1_PAD_7_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 7번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_1_PAD_8_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 8번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_PAD_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 1번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_PAD_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 2번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_PAD_3_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 3번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_PAD_4_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 4번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_PAD_5_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 5번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_PAD_6_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 6번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_PAD_7_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 7번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKER_2_PAD_8_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 8번을 사용합니다"); } break;
                    case (int)OPTNUM.PICKUP_UNIT_SKIP_USAGE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "픽업 실패 시 맵테이블의 UNIT을 스킵하는 기능을 사용합니다."); } break;

                    case (int)OPTNUM.GOOD_TRAY_STAGE_SIZE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "트레이 스테이지 UP / DOWN 동작 및 충돌 방지를 위한 스테이지 사이즈(mm)"); } break;
                    case (int)OPTNUM.GOOD_TRAY_1_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD 트레이 스테이지 1 CLAMP 반복 횟수"); } break;
                    case (int)OPTNUM.GOOD_TRAY_2_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD 트레이 스테이지 2 CLAMP 반복 횟수"); } break;
                    case (int)OPTNUM.REWORK_TRAY_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "REWORK 트레이 스테이지 CLAMP 반복 횟수"); } break;
                    case (int)OPTNUM.GOOD_TRAY_STAGE_1_USE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD 트레이 1을 사용합니다"); } break;
                    case (int)OPTNUM.GOOD_TRAY_STAGE_2_USE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD 트레이 2를 사용합니다"); } break;
                    case (int)OPTNUM.USE_ELV_COVER_UNLOAD: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "TRAY OUT 시 COVER TRAY 를 사용합니다."); } break;

                    case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이 피커의 슬로우 다운 높이(mm)"); } break;
                    case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이 피커의 슬로우 다운 속도(mm/sec)"); } break;
                    case (int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이 피커의 감지 센서를 사용합니다."); } break;
                    case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_USE: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이를 뒤집어서 사용합니다."); } break;//220516
                    case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_PITCH: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이를 뒤집어서 공정을 진행할 시 피커 Z축에 더해지는 피치(mm)."); } break;//220516
                    case (int)OPTNUM.TRAY_PICKER_MOVE_READY_WAIT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이 피커가 준비 위치로 이동하는 대기 시간(sec)."); } break;

                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 트레이를 받기위해 대기하는 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 트레이를 받기위해 대기하는 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_RW_TRAY_ELV_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 트레이를 받기위해 대기하는 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 트레이를 받기위해 떠받히는 위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 트레이를 받기위해 떠받히는 위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_RW_TRAY_ELV_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 트레이를 받기위해 떠받히는 위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 트레이 유무 판단할 수 있는 기준위치"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 트레이 유무 판단할 수 있는 기준위치"); } break;
                    case (int)OPTNUM.ULD_RW_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"); } break;
                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"); } break;
                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"); } break;
                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 트레이를 배출 할 수 있는 준비위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_WORKING_WAIT_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 트레이를 배출 할 수 있는 준비위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 준비위치를 다시 설정 하기 위한 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_RESEARCH_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 준비위치를 다시 설정 하기 위한 안전위치 오프셋"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1) 만재 배출 개수"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2) 만재 배출 개수"); } break;
                    case (int)OPTNUM.ULD_RW_TRAY_ELV_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터 만재 배출 개수"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 오프셋 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 오프셋 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_RW_TRAY_ELV_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 오프셋 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_1_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이 엘리베이터(1)가 오프셋 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_2_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이 엘리베이터(2)가 오프셋 이동하는 저속 스피드"); } break;
                    case (int)OPTNUM.ULD_ELV_COVER_WAIT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "엘리베이터 커버가 닫히고 나서 기다리는 시간"); } break;
                    case (int)OPTNUM.ULD_TRAY_CONV_IN_TIME_USE: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "자동 TRAY IN 동작 사용"); } break;
                    case (int)OPTNUM.ULD_TRAY_CONV_IN_SEN_DETECT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "자동 TRAY IN 동작 센서 감지 시간"); } break;


                    case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "CLEANER", "클리너 테이블의 슬로우 다운 높이(mm)"); } break;
                    case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "CLEANER", "클리너 테이블의 슬로우 다운 속도(mm/sec)"); } break;
                    case (int)OPTNUM.SCRAP_MODE_USE: { itemOptions[i] = new OptionItem(i, "CLEANER", "스크랩 모드를 사용합니다."); } break;
                    case (int)OPTNUM.CLEANER_LOG_ANALYSIS: { itemOptions[i] = new OptionItem(i, "CLEANER", "클리너 분석 로그 옵션을 사용합니다. (테스트)"); } break;
                    case (int)OPTNUM.CLEAN_CYLINDER_DELAY: { itemOptions[i] = new OptionItem(i, "CLEANER", "클리너 실린더 딜레이 시간(msec)"); } break;
                    default: { itemOptions[i] = new OptionItem(i); } break;
                }
            }

        }

        //public void InitItemOptions()
        //{
        //    itemOptions = new OptionItem[(int)OPTNUM.MAX];
        //    if (ConfigMgr.Inst.Cfg == null)
        //    {
        //        return;
        //    }
        //    switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
        //    {
        //        case Language.ENGLISH:
        //            for (int i = 0; i < itemOptions.Length; i++)
        //            {
        //                switch (i)
        //                {
        //                    case (int)OPTNUM.GEM_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Use GEM"); } break;
        //                    case (int)OPTNUM.VAC_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Check the VACUM Sensor"); } break;
        //                    case (int)OPTNUM.DRY_RUN_MODE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Use DRY MODE"); } break;
        //                    case (int)OPTNUM.DRY_RUN_SAW_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Use the interface with SAW when in DRY mode."); } break;
        //                    case (int)OPTNUM.DRY_RUN_RANDOM_SORT: { itemOptions[i] = new OptionItem(i, "OPERATION", "Randomly eject materials when in dry mode"); } break;
        //                    case (int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Use the vision and interface when in DRY mode."); } break;

        //                    case (int)OPTNUM.TOP_VISION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Use upper vision"); } break;
        //                    case (int)OPTNUM.TOP_ALIGN_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Upper alignment is used."); } break;
        //                    case (int)OPTNUM.BOTTOM_VISION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Use lower vision"); } break;
        //                    case (int)OPTNUM.NO_MSG_STOP_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "Facility silent stop judgment time (unit: minutes)"); } break;
        //                    case (int)OPTNUM.ULD_MGZ_FULL_CHECK_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "Magazine Emission Sensor Detection Time Set (in milliseconds)"); } break;

        //                    case (int)OPTNUM.TRAY_INSPECTION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Use tray unit sorry check"); } break;
        //                    case (int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY: { itemOptions[i] = new OptionItem(i, "OPERATION", "Delay (msec) during tray unit sorry test"); } break;

        //                    case (int)OPTNUM.EQP_INIT_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "Equipment Initialization Timeout (msec)"); } break;
        //                    case (int)OPTNUM.CYL_MOVE_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "Cylinder Operation Timeout (msec)"); } break;
        //                    case (int)OPTNUM.VAC_CHECK_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "Vacuum Sensor Check Timeout (msec)"); } break;
        //                    case (int)OPTNUM.MOT_MOVE_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "Motor Movement Timeout (msec)"); } break;
        //                    case (int)OPTNUM.MES_REPLY_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "MES Interface Response Timeout (msec)"); } break;

        //                    case (int)OPTNUM.BOTTOM_VISION_OFFSET_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Applies offset for lower vision"); } break;
        //                    case (int)OPTNUM.BLOW_VAC_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Check the VACUUM Sensor on Blow"); } break;
        //                    case (int)OPTNUM.ITS_XOUT_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS Eject XOUT as a result."); } break;

        //                    case (int)OPTNUM.DOOR_SAFETY_CHECK_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Check the front and rear doors and EMO SW status of the facility."); } break;
        //                    case (int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT: { itemOptions[i] = new OptionItem(i, "OPERATION", "Time to remind again if it is not cleared after the scrapbox alarm occurs (ms)"); } break;
        //                    case (int)OPTNUM.REJECT_BOX_ALARM_CLEAR_TIME_OUT: { itemOptions[i] = new OptionItem(i, "OPERATION", "Time to re-notify if it has not been cleared after the REJECT box alarm (ms)"); } break;
        //                    case (int)OPTNUM.EVENT_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "Event Log Archive"); } break;
        //                    case (int)OPTNUM.ALARM_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "alarm log retention period"); } break;
        //                    case (int)OPTNUM.SEQ_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "sequence log retention period"); } break;
        //                    case (int)OPTNUM.LOT_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "Rat Log Archive"); } break;
        //                    case (int)OPTNUM.ITS_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS log retention period"); } break;
        //                    case (int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "Production Quantity Log Archive"); } break;

        //                    case (int)OPTNUM.ITS_ID_STRIP_ID_VALID_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS ID and STRIP ID compare front N digits option."); } break;
        //                    case (int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN: { itemOptions[i] = new OptionItem(i, "OPERATION", "Common character length of ITS ID and STRIP ID"); } break;
        //                    case (int)OPTNUM.BOTTOM_VISION_ALARM_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "Use alarm when lower vision matching fails. If not in use, it will be seated on the reuse tray."); } break;
        //                    case (int)OPTNUM.VISION_INTERFACE_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "OPERATION", "lower vision interface retry count"); } break;//220615
        //                    case (int)OPTNUM.VISION_INTERFACE_TIMEOUT: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "VISION Interface Response Timeout (msec)"); } break;
        //                    case (int)OPTNUM.SAW_INTERFACE_TIMEOUT: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "SAW Interface Response Timeout (msec)"); } break;
        //                    case (int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "Msec before grab during load vision pre-align operation"); } break;
        //                    case (int)OPTNUM.LD_VISION_BARCODE_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "Msec before grab during load vision barcode operation"); } break;
        //                    case (int)OPTNUM.MAP_VISION_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "MAP VISION VISIONAL INSPECTION OPERATION DELAY (msec)"); } break;
        //                    case (int)OPTNUM.BALL_VISION_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "Msec before grab during ball vision inspection"); } break;
        //                    case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "conveyor maximum operating time (msec)"); } break;
        //                    case (int)OPTNUM.CONV_ROLLING_TIME: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "time to turn the conveyor after detecting the conveyor arrival sensor (msec)"); } break;
        //                    case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "Use conveyor maximum operating time (msec)"); } break;
        //                    case (int)OPTNUM.CONV_SAFETY_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "Use conveyor inlet safety sensor check."); } break;


        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Msec before reading the strip lower barcode"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "strip lower barcode read retry count"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "STRIP BOTTOM_BARCODE_READ_TIMEOUT"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Use Strip Lower Barcode Read"); } break;
        //                    case (int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Use the overload sensor on the strip pusher"); } break;
        //                    // Loader Rail Top Magazine Sense Sensor Usage Mode
        //                    case (int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Rail Top Material Detection Sensor (A Contact)"); } break;
        //                    case (int)OPTNUM.STRIP_RAIL_Y_UNLOAD_PRE_MOVE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Pre-move to load rail Y unload position before pick-up of strip picker."); } break;

        //                    case (int)OPTNUM.STRIP_PREALIGN_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Use Strip Pre-align"); } break;
        //                    case (int)OPTNUM.STRIP_VISION_BARCODE_READ_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Strip Upper Barcode Read (read with PREALIGN camera"); } break;
        //                    case (int)OPTNUM.STRIP_ORIENT_CHECK_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Use Strip Orient Check"); } break;
        //                    case (int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Pre-align calibration angle offset on the strip"); } break;
        //                    case (int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "Strip Press Mode."); } break;
        //                    case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "Slow Down Height (mm) of Strip Picker"); } break;
        //                    case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "Slow Down Rate (mm/sec) of Strip Picker"); } break;
        //                    case (int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "interlock distance between the strip picker and the unit picker (unit+strip mm)"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "Slow Down Height (mm) of Unit Picker"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "Slow Down Rate (mm/sec) of Unit Picker"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "Use only position 1 for unit picker scrap"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_VACUUM_DELAY: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "Vacuum waiting time after settling the dicing cow cutting table."); } break;
        //                    case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "MAPPICKER", "Slow Down Height (mm) of Map Picker"); } break;
        //                    case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "MAPPICKER", "MAPPICKER's slow-down rate (mm/sec)"); } break;
        //                    case (int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT: { itemOptions[i] = new OptionItem(i, "MAPPICKER", "MAPPICKER air dry repeat count"); } break;
        //                    case (int)OPTNUM.MAP_INSPECTION_CHECK_COUNT: { itemOptions[i] = new OptionItem(i, "MAPPICKER", "Quantity of alarm when map inspection fails"); } break;

        //                    case (int)OPTNUM.MAP_STAGE_SIZE_HEIGHT:{ itemOptions[i] = new OptionItem(i, "MAP STAGE", " vertical size(mm)) of map stage"); } break;
        //                    case (int)OPTNUM.MAP_STAGE_SIZE_WIDTH: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "horizontal size (mm) of map stage"); } break;
        //                    case (int)OPTNUM.MAP_STAGE_1_USE: { itemOptions[i] = new OptionItem(i, "MAPSTAGE", "use map stage 1."); } break;
        //                    case (int)OPTNUM.MAP_STAGE_2_USE: { itemOptions[i] = new OptionItem(i, "MAPSTAGE", "use map stage 2."); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use T-axis pre-move after pickup"); } break; //220617
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Delay time for multi-picker to check vacuum (msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "time for multi-picker to vacuum (msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Time for multi-picker to wait for vacuum dig (msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Time for multi-picker to wait (msec)) after moving to the pickup location"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Time for multi-picker to wait after moving to the ball inspection position (msec)"); }  break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Time for multi-picker to wait (msec)) after moving to the place location"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Number of times the multi-picker retries if pick-up fails"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Delay time (msec) for multi-picker to check vacuum if pick-up fails"); } break;
        //                    case (int)OPTNUM.BOTTOM_CAM_ANGLE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Place time angle offset"); } break;

        //                    case (int)OPTNUM.PICKER_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use multi-picker head 1"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad number 1 of multi-picker head 1"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad number 2 of multi-picker head 1"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_3_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad number 3 of multi-picker head 1"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_4_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad #4 of multi-picker head 1"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_5_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad no.5 of multi-picker head 1"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_6_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad #6 of multi-picker head 1"); } break;
        //                    case (int)OPTNUM.PICKER_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use multi-picker head 2"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad number 1 of multi-picker head 2"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad number 2 of multi-picker head"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_3_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad #3 of multi-picker head #2)"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_4_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad #4 of multi-picker head #2)"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_5_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad no.5 of multi-picker head no.2"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_6_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use pad no.6 of multi-picker head no.2"); } break;
        //                    case (int)OPTNUM.PICKUP_UNIT_SKIP_USAGE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "Use the ability to skip UNIT in the map table in case of a pickup failure."); } break;

        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_SIZE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "TRAY STAGE UP/DOWN size(mm) for action and collision avoidance)"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_1_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD tray stage 1 CLAMP repeat count"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_2_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD Tray Stage 2 CLAMP Repeat Count"); } break;
        //                    case (int)OPTNUM.REWORK_TRAY_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "REWORK Tray Stage CLAMP Repeat Count"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_1_USE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "Use GOOD Tray 1"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_2_USE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "Use GOOD Tray 2"); } break;

        //                    case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "Slow-down height (mm) of tray picker"); } break;
        //                    case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "Tray Picker's Slow Down Speed (mm/sec)"); } break;
        //                    case (int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "use the tray picker's sense sensor."); } break;
        //                    case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_USE: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "Flipping the tray is used."); } break;//220516
        //                    case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_PITCH: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "Pitch in mm to be added to the picker Z axis when the process is reversed."); } break;//220516
        //                    case (int)OPTNUM.TRAY_PICKER_MOVE_READY_WAIT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "Waiting time for tray picker to move to the ready position (sec.)"); } break;

        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Safe position offset where the good tray elevator (1) waits to receive the tray"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Safe Position Offset where the good tray elevator (2) waits to receive the tray"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Safe position offset waiting for rework tray elevator to receive tray"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Position offset where the good tray elevator (1) is held up for receiving the tray"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Position offset where the good tray elevator (2) is supported for receiving the tray"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Offset of position where the rework tray elevator is supported for receiving the tray"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Offset safe position before the pedestal is reached by the good tray elevator (1)"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Default safe position before the pedestal is reached by the good tray elevator (2)"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Offset safe position before rework tray elevator reaches pedestal"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "empty tray (1) offset to safe position before elevator reaches pedestal when fully loaded tray"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "empty tray (2) offset before elevator reaches pedestal"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "low speed to the pedestal when the good tray elevator (1) ejects the full tray"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Low speed to the pedestal when the good tray elevator (2) ejects the loaded tray"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "low speed at which the rework tray elevator moves to the pedestal when it ejects the loaded tray"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "an empty rack tray is taken when draining the tray (1) The elevator is full load to travel to low speed and I"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Low speed that moves to the pedestal when the elevator ejects the loaded tray"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "reference position where the good tray elevator (1) can determine whether a tray is present"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "reference position where the good tray elevator (2) can determine whether a tray is present"); } break;


        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "rework tray elevator can determine if a tray is present"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "the reference position where the empty tray (1) elevator can determine if the tray is present"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "empty tray (2) reference position where elevator can determine if tray exists"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "empty tray (1) offset for elevator to drain tray"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_WORKING_WAIT_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "empty tray (2) offset for elevator to drain tray"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "empty tray (1) safe position offset for elevator to reset the ready position"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_RESEARCH_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "empty tray (2) safe position offset for elevator to reset the ready position"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Good Tray Elevator (1) Full Emission Count"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Good Tray Elevator (2) Full Emission Count"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Number of rework tray elevator exhaust emissions"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "low speed at which the good tray elevator (1) moves offset"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "low speed at which the good tray elevator (2) moves offset"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Low speed at which the rework tray elevator moves offset"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_1_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "low speed at which the empty tray elevator (1) moves offset"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_2_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "low speed with empty tray elevator (2) offset"); } break;
        //                    case (int)OPTNUM.ULD_ELV_COVER_WAIT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "Time to wait after the elevator cover is closed"); } break;
        //                    case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "CLEANER", "Slow Down Height (mm) of Cleaner Table"); } break;
        //                    case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "CLEANER", "slowdown rate (mm/sec)) of the cleaner table"); } break;
        //                    case (int)OPTNUM.SCRAP_MODE_USE: { itemOptions[i] = new OptionItem(i, "CLEANER", "use scrap mode."); } break;
        //                    case (int)OPTNUM.CLEANER_LOG_ANALYSIS: { itemOptions[i] = new OptionItem(i, "CLEANER", "Cleaner Analysis Log option. (test)"); } break;
        //                    case (int)OPTNUM.CLEAN_CYLINDER_DELAY: { itemOptions[i] = new OptionItem(i, "CLEANER", "Cleaner cylinder delay time (msec)"); } break;
        //                    default: { itemOptions[i] = new OptionItem(i); } break;
        //                }
        //            }
        //            break;

        //        case Language.CHINA:
        //            for (int i = 0; i < itemOptions.Length; i++)
        //            {
        //                switch (i)
        //                {
        //                    case (int)OPTNUM.GEM_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "请使用GEM"); } break;
        //                    case (int)OPTNUM.VAC_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "请确认真空感应器"); } break;
        //                    case (int)OPTNUM.DRY_RUN_MODE: { itemOptions[i] = new OptionItem(i, "OPERATION", "请使用干燥模式"); } break;
        //                    case (int)OPTNUM.DRY_RUN_SAW_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "在干燥开始模式时 与SAW一起使用界面."); } break;
        //                    case (int)OPTNUM.DRY_RUN_RANDOM_SORT: { itemOptions[i] = new OptionItem(i, "OPERATION", "干燥开始模式时素材任何位置可排除"); } break;
        //                    case (int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "干燥开始模式时 与视觉检测进行连接"); } break;

        //                    case (int)OPTNUM.TOP_VISION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "使用上部视觉检测"); } break;
        //                    case (int)OPTNUM.TOP_ALIGN_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "使用 上部对齐功能"); } break;
        //                    case (int)OPTNUM.BOTTOM_VISION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "使用下部视觉检测"); } break;
        //                    case (int)OPTNUM.NO_MSG_STOP_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "设备停止确认时间（单位：分）"); } break;
        //                    case (int)OPTNUM.ULD_MGZ_FULL_CHECK_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "料盒 排出感应时间设定 (单位 :MS)"); } break;

        //                    case (int)OPTNUM.TRAY_INSPECTION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "使用萃盘UNIT未放置检查"); } break;
        //                    case (int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY: { itemOptions[i] = new OptionItem(i, "OPERATION", "萃盘UNIT未放置检查时延迟(msec)"); } break;

        //                    case (int)OPTNUM.EQP_INIT_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "设备初始化 限制时间(msec)"); } break;
        //                    case (int)OPTNUM.CYL_MOVE_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "气缸动作制约时间(msec)"); } break;
        //                    case (int)OPTNUM.VAC_CHECK_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "真空感应确认限制时间(msec)"); } break;
        //                    case (int)OPTNUM.MOT_MOVE_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "马达移动限制时间(msec)"); } break;
        //                    case (int)OPTNUM.MES_REPLY_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "MES 连接时 回应限制时间(msec)"); } break;
        //                    case (int)OPTNUM.BOTTOM_VISION_OFFSET_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "下方视觉检测相机适用OFFSET"); } break;
        //                    case (int)OPTNUM.BLOW_VAC_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "吹气时 确认真空感应"); } break;
        //                    case (int)OPTNUM.ITS_XOUT_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "用ITS结果 进行XOUT排出"); } break;

        //                    case (int)OPTNUM.DOOR_SAFETY_CHECK_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "设备 前后门及EMO SW 状态确认"); } break;
        //                    case (int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT: { itemOptions[i] = new OptionItem(i, "OPERATION", "废料箱报警后不能清洗时限制的时间 (ms)"); } break;
        //                    case (int)OPTNUM.REJECT_BOX_ALARM_CLEAR_TIME_OUT: { itemOptions[i] = new OptionItem(i, "OPERATION", "报废箱报警后不能清洗时限制的时间 (ms)"); } break;

        //                    case (int)OPTNUM.EVENT_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "项目LOG保存时效"); } break;
        //                    case (int)OPTNUM.ALARM_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "报警记录保存时效"); } break;
        //                    case (int)OPTNUM.SEQ_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "日记记录保存时效"); } break;
        //                    case (int)OPTNUM.LOT_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "批次记录保存时效"); } break;
        //                    case (int)OPTNUM.ITS_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS 记录保存时效"); } break;
        //                    case (int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "生产数量记录保存时效"); } break;


        //                    case (int)OPTNUM.ITS_ID_STRIP_ID_VALID_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "默认ITS ID和 STRIP ID前 N位置 进行对比"); } break;
        //                    case (int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS ID和 STRIP ID共同字符长度"); } break;
        //                    case (int)OPTNUM.BOTTOM_VISION_ALARM_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "下方视觉检测匹配失败时报警,未使用时 放置在再使用萃盘中"); } break;
        //                    case (int)OPTNUM.VISION_INTERFACE_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "OPERATION", "下方视觉检测连接指示次数"); } break;//220615
        //                    case (int)OPTNUM.VISION_INTERFACE_TIMEOUT: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "视觉检测连接回应等待时间(msec)"); } break;
        //                    case (int)OPTNUM.SAW_INTERFACE_TIMEOUT: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "SAW 连接回应等待时间(msec)"); } break;
        //                    case (int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "入料检测对齐运行时，抓图延迟时间(msec)"); } break;
        //                    case (int)OPTNUM.LD_VISION_BARCODE_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "入料二维码检测启用时，推料延迟(msec)"); } break;
        //                    case (int)OPTNUM.MAP_VISION_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "MAP 视觉检测运行时抓图延迟时间(msec)"); } break;
        //                    case (int)OPTNUM.BALL_VISION_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "球图形检测运行时，抓图延迟(msec)"); } break;
        //                    case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "传送带最长运行时间(msec)"); } break;
        //                    case (int)OPTNUM.CONV_ROLLING_TIME: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "传送带到达感应区位置后，传送带继续转动时间(msec)"); } break;
        //                    case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "使用传送带最长运行时间(msec)."); } break;
        //                    case (int)OPTNUM.CONV_SAFETY_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "使用确认传送带入料口安全位感应"); } break;


        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "STIRP 下方条形码识别前延迟(msec)"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "STIRP下方 条形码再指示识别次数"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "STIRP 下方 条形码成功识别等待时间(msec)"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "选择使用STRIP 下方 条形码进行识别"); } break;
        //                    case (int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "使用STRIP 推进器 过载感应"); } break;
        //                    case (int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "矫正导轨上方素材感应（A接点）"); } break;
        //                    case (int)OPTNUM.STRIP_RAIL_Y_UNLOAD_PRE_MOVE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "STIRP 吸取产品前，矫正导轨 提前移动到 Y轴出料位置"); } break;

        //                    case (int)OPTNUM.STRIP_PREALIGN_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "使用STRIP 校直功能"); } break;
        //                    case (int)OPTNUM.STRIP_VISION_BARCODE_READ_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "使用STRIP 上方 条形码识别(PRE ALIGN相机来识别)"); } break;
        //                    case (int)OPTNUM.STRIP_ORIENT_CHECK_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "使用STRIP 方向检查"); } break;
        //                    case (int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "STIRP 校直 补正角度设置"); } break;
        //                    case (int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "使用STIRP下压模式"); } break;

        //                    case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "STIRP_PK 缓慢下降高度(mm)"); } break;
        //                    case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "STIRP_PK 缓慢下降速度(mm/sec)"); } break;
        //                    case (int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "搬运(STRIP)吸盘与 手臂（UNIT）吸盘 接触距离(STIRP+UNIT mm)"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "手臂(UNIT)吸盘 缓慢下降高度(mm)"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "手臂(UNIT)吸盘 缓慢下降速度(mm/sec)"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "手臂(UNIT)吸盘 废料移除时，只使用1号位置"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_VACUUM_DELAY: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "切割机 放置到切割台面后，真空等待时间."); } break;
        //                    case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "MAP PK吸盘 缓慢下降高度(mm)"); } break;
        //                    case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "MAP_PK吸盘缓慢下降速度(mm/sec)"); } break;
        //                    case (int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "MAP_PK吸盘干燥反复次数"); } break;
        //                    case (int)OPTNUM.MAP_INSPECTION_CHECK_COUNT: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "MAP_PK 选取失败时发生警报数量"); } break;

        //                    case (int)OPTNUM.MAP_STAGE_SIZE_HEIGHT: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "MAP 台面 竖向尺寸(mm)"); } break;
        //                    case (int)OPTNUM.MAP_STAGE_SIZE_WIDTH: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "MAP 台面 横向尺寸(mm)"); } break;
        //                    case (int)OPTNUM.MAP_STAGE_1_USE: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "使用 1号MAP 台面"); } break;
        //                    case (int)OPTNUM.MAP_STAGE_2_USE: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "使用 2号MAP 台面"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用吸附后  提前让T轴移动"); } break; //220617
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "MULTI PICKER 确认真空 延迟时间(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "MULTI PICKER 开真空延迟时间(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "MULTI PICKER 关真空后等待时间(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "MULTI PICKER 移动到吸附位置后等待时间(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "MULTI PICKER 移动到球形检测位置后 等待时间(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "MULTI PICKER 移动到指定下压位后 等待时间(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "MULTI PICKER 吸附失败时重新开启次数"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "MULTI PICKER 吸附失败时 确认真空延迟时间(msec)"); } break;
        //                    case (int)OPTNUM.BOTTOM_CAM_ANGLE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "下压时 角度选择"); } break;

        //                    case (int)OPTNUM.PICKER_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 1号"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 1中 1号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 1中 2号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_3_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 1中 3号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_4_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 1中 4号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_5_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 1中 5号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_6_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 1中 6号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 2号"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 2中 1号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 2中 2号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_3_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 2中 3号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_4_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 2中 4号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_5_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 2中 5号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_6_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "使用MULTI PICKER 2中 6号吸嘴"); } break;
        //                    case (int)OPTNUM.PICKUP_UNIT_SKIP_USAGE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "吸附失败时，MAP台面的 UNIT进行隐藏"); } break;

        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_SIZE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "为了托盘台面 UP / DOWN 动作及防止冲突而定的台面尺寸(mm)"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_1_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD1台面 夹子反复使用次数"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_2_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD2台面 夹子反复使用次数"); } break;
        //                    case (int)OPTNUM.REWORK_TRAY_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "REWORK 台面 夹子反复使用次数"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_1_USE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "使用GOOD1中的托盘"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_2_USE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "使用GOOD2中的托盘"); } break;

        //                    case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "托盘手臂 吸附时 缓慢下降高度(mm)"); } break;
        //                    case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "托盘手臂 缓慢下降速度(mm/sec)"); } break;
        //                    case (int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "使用托盘手臂感应器"); } break;
        //                    case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_USE: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "托盘翻转后使用"); } break;//220516
        //                    case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_PITCH: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "托盘翻转后继续后工序时 Z轴增加幅度(mm)."); } break;//220516
        //                    case (int)OPTNUM.TRAY_PICKER_MOVE_READY_WAIT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "托盘搬运手臂移动到准备位置时的等待时间(sec)."); } break;

        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品1托盘升降板 为了接受托盘 等待安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品2托盘升降板 为了接受托盘 等待安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "待检查托盘升降板 为了接受托盘 等待安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品1升降板 为了接受托盘，支撑位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品1升降板 为了接受托盘，支撑位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "待检查升降板 为了接受托盘，支撑位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(1)升降板将满载后的托盘移出时 支撑台面到达安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(2)升降板将满载后的托盘移出时 支撑台面到达安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "待检查升降板将满载后的托盘移出时 支撑台面到达安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(1)升降板将满载后托盘移出时 支撑台面到达安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(2)升降板将满载后托盘移出时 支撑台面到达安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(1)升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(2)升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "待检查升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(1)升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(2)升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(1) 升降板 确认有无托盘的判定基准位"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(2) 升降板 确认有无托盘的判定基准位"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "待检查升降板 确认有无托盘的判定基准位"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(1) 升降板 确认有无托盘的判定基准位"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(2) 升降板 确认有无托盘的判定基准位"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(1) 升降板 移出托盘时 基准位补偿"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_WORKING_WAIT_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(2) 升降板 移出托盘时 基准位补偿"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(1) 升降板 为了重新设定准备位置时所需的安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_RESEARCH_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(2) 升降板 为了重新设定准备位置时所需的安全位置补偿"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(1)升降板 满载时 可排出的个数"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(2)升降板 满载时 可排出的个数"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "待检查升降板 满载时 可排出的个数"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(1)升降板 补偿位移动时的低速度 "); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "良品(2)升降板 补偿位移动时的低速度"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "待检查升降板 补偿位移动时的低速度"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_1_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(1)升降板 补偿位移动时的低速度"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_2_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "空托盘(2)升降板 补偿位移动时的低速度"); } break;
        //                    case (int)OPTNUM.ULD_ELV_COVER_WAIT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "升降板 门关闭后 等待时间"); } break;
        //                    case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "CLEANER", "清洗台面 缓慢下降高度(mm)"); } break;
        //                    case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "CLEANER", "清洗台面 缓慢下降速度(mm/sec)"); } break;
        //                    case (int)OPTNUM.SCRAP_MODE_USE: { itemOptions[i] = new OptionItem(i, "CLEANER", "启用废料模式"); } break;
        //                    case (int)OPTNUM.CLEANER_LOG_ANALYSIS: { itemOptions[i] = new OptionItem(i, "CLEANER", "默认使用清洗 分析记录(测试中)"); } break;
        //                    case (int)OPTNUM.CLEAN_CYLINDER_DELAY: { itemOptions[i] = new OptionItem(i, "CLEANER", "清洗气缸延迟时间(msec)"); } break;
        //                    default: { itemOptions[i] = new OptionItem(i); } break;
        //                }
        //            }
        //            break;
        //        case Language.KOREAN:
        //            for (int i = 0; i < itemOptions.Length; i++)
        //            {
        //                switch (i)
        //                {
        //                    case (int)OPTNUM.GEM_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "GEM을 사용합니다"); } break;
        //                    case (int)OPTNUM.VAC_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "배큠 센서를 확인합니다"); } break;
        //                    case (int)OPTNUM.DRY_RUN_MODE: { itemOptions[i] = new OptionItem(i, "OPERATION", "드라이 런 모드를 사용합니다"); } break;
        //                    case (int)OPTNUM.DRY_RUN_SAW_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "드라이 런 모드일때 SAW와 인터페이스를 사용합니다."); } break;
        //                    case (int)OPTNUM.DRY_RUN_RANDOM_SORT: { itemOptions[i] = new OptionItem(i, "OPERATION", "드라이 런 모드일때 자재를 무작위로 배출합니다"); } break;
        //                    case (int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "드라이 런 모드일때 비젼과 인터페이스를 사용합니다."); } break;

        //                    case (int)OPTNUM.TOP_VISION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "상부 비전을 사용합니다"); } break;
        //                    case (int)OPTNUM.TOP_ALIGN_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "상부 얼라인을 사용합니다."); } break;
        //                    case (int)OPTNUM.BOTTOM_VISION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "하부 비전을 사용합니다"); } break;
        //                    case (int)OPTNUM.NO_MSG_STOP_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "설비 무언 정지 판단 시간 (단위 : 분)"); } break;
        //                    case (int)OPTNUM.ULD_MGZ_FULL_CHECK_TIME: { itemOptions[i] = new OptionItem(i, "OPERATION", "매거진 배출 센서 감지 시간 설정 (단위 : 밀리초)"); } break;

        //                    case (int)OPTNUM.TRAY_INSPECTION_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "트레이 유닛 미안착 검사를 사용합니다"); } break;
        //                    case (int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY: { itemOptions[i] = new OptionItem(i, "OPERATION", "트레이 유닛 미안착 검사 시 딜레이(msec)"); } break;

        //                    case (int)OPTNUM.EQP_INIT_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "장비 초기화 제한시간(msec)"); } break;
        //                    case (int)OPTNUM.CYL_MOVE_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "실린더 동작 제한시간(msec)"); } break;
        //                    case (int)OPTNUM.VAC_CHECK_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "배큠 센서 확인 제한시간(msec)"); } break;
        //                    case (int)OPTNUM.MOT_MOVE_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "모터 이동 제한시간(msec)"); } break;
        //                    case (int)OPTNUM.MES_REPLY_TIME: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "MES 인터페이스 응답 제한시간(msec)"); } break;

        //                    case (int)OPTNUM.BOTTOM_VISION_OFFSET_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "하부 비전 시 오프셋을 적용합니다"); } break;
        //                    case (int)OPTNUM.BLOW_VAC_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "블로우 시 배큠 센서를 확인합니다"); } break;
        //                    case (int)OPTNUM.ITS_XOUT_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS결과로 XOUT을 배출합니다."); } break;

        //                    case (int)OPTNUM.DOOR_SAFETY_CHECK_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "설비 전후면 도어 및 EMO SW 상태를 체크 합니다."); } break;
        //                    case (int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT: { itemOptions[i] = new OptionItem(i, "OPERATION", "스크랩 박스 알람 발생 후 클리어 안되었을 시 재알림하는 시간 (ms)"); } break;
        //                    case (int)OPTNUM.REJECT_BOX_ALARM_CLEAR_TIME_OUT: { itemOptions[i] = new OptionItem(i, "OPERATION", "리젝 박스 알람 발생 후 클리어 안되었을 시 재알림하는 시간 (ms)"); } break;

        //                    case (int)OPTNUM.EVENT_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "이벤트 로그 보관 기간"); } break;
        //                    case (int)OPTNUM.ALARM_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "알람 로그 보관 기간"); } break;
        //                    case (int)OPTNUM.SEQ_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "시퀀스 로그 보관 기간"); } break;
        //                    case (int)OPTNUM.LOT_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "랏 로그 보관 기간"); } break;
        //                    case (int)OPTNUM.ITS_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS 로그 보관 기간"); } break;
        //                    case (int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD: { itemOptions[i] = new OptionItem(i, "OPERATION", "생산 수량 로그 보관 기간"); } break;

        //                    case (int)OPTNUM.ITS_ID_STRIP_ID_VALID_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS ID와 STRIP ID의 앞 N자리 비교 옵션을 사용합니다."); } break;
        //                    case (int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN: { itemOptions[i] = new OptionItem(i, "OPERATION", "ITS ID와 STRIP ID의 공통 문자 길이"); } break;
        //                    case (int)OPTNUM.BOTTOM_VISION_ALARM_USE: { itemOptions[i] = new OptionItem(i, "OPERATION", "하부 비전 매칭 실패 시 알람을 사용합니다. 미사용 시 재사용 트레이에 안착합니다."); } break;
        //                    case (int)OPTNUM.VISION_INTERFACE_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "OPERATION", "하부 비전 인터페이스 재시도 횟수"); } break;//220615
        //                    case (int)OPTNUM.VISION_INTERFACE_TIMEOUT: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "VISION 인터페이스 응답 제한시간(msec)"); } break;
        //                    case (int)OPTNUM.SAW_INTERFACE_TIMEOUT: { itemOptions[i] = new OptionItem(i, "TIMEOUT", "SAW 인터페이스 응답 제한시간(msec)"); } break;
        //                    case (int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "로드 비전 프리얼라인 동작시 그랩 전 딜레이(msec)"); } break;
        //                    case (int)OPTNUM.LD_VISION_BARCODE_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "로드 비전 바코드 동작시 그랩 전 딜레이(msec)"); } break;
        //                    case (int)OPTNUM.MAP_VISION_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "맵 비전 비전검사 동작시 그랩 전 딜레이(msec)"); } break;
        //                    case (int)OPTNUM.BALL_VISION_GRAB_DELAY: { itemOptions[i] = new OptionItem(i, "DELAY", "볼 비전 비전검사 동작시 그랩 전 딜레이(msec)"); } break;
        //                    case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "컨베이어 최대 동작시간(msec)"); } break;
        //                    case (int)OPTNUM.CONV_ROLLING_TIME: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "컨베이어 도착 센서감지 후 컨베이어 돌리는 시간(msec)"); } break;
        //                    case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "컨베이어 최대 동작시간(msec)을 사용 합니다."); } break;
        //                    case (int)OPTNUM.CONV_SAFETY_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "CONVEYOR", "컨베이어 투입구 안전 센서 체크를 사용합니다."); } break;


        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 하부 바코드 읽기 전 딜레이(msec)"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 하부 바코드 읽기 재시도 횟수"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 하부 바코드 읽기 성공 제한시간(msec)"); } break;
        //                    case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 하부 바코드 읽기를 사용합니다"); } break;
        //                    case (int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 푸셔의 오버로드 센서를 사용합니다"); } break;
        //                    // 로더 레일 상단 매거진 감지 센서 사용 모드
        //                    case (int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "레일 상단 자재 감지 센서(A접점) 를 사용합니다."); } break;
        //                    case (int)OPTNUM.STRIP_RAIL_Y_UNLOAD_PRE_MOVE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 피커 픽업 전 로드 레일 Y 언로드 위치로 미리 이동기능을 사용합니다."); } break;

        //                    case (int)OPTNUM.STRIP_PREALIGN_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 프리얼라인을 사용합니다"); } break;
        //                    case (int)OPTNUM.STRIP_VISION_BARCODE_READ_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 상부 바코드 읽기를 사용합니다(PRE ALIGN 카메라로 읽음)"); } break;
        //                    case (int)OPTNUM.STRIP_ORIENT_CHECK_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 오리엔트 검사를 사용합니다"); } break;
        //                    case (int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립의 프리얼라인 보정 각도 오프셋"); } break;
        //                    case (int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE: { itemOptions[i] = new OptionItem(i, "LOAD RAIL", "스트립 프레스 모드를 사용합니다."); } break;

        //                    case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "스트립 피커의 슬로우 다운 높이(mm)"); } break;
        //                    case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "스트립 피커의 슬로우 다운 속도(mm/sec)"); } break;
        //                    case (int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP: { itemOptions[i] = new OptionItem(i, "STRIP PICKER", "스트립 피커와 유닛 피커 사이의 인터락 거리(유닛+스트립 mm)"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "유닛 피커의 슬로우 다운 높이(mm)"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "유닛 피커의 슬로우 다운 속도(mm/sec)"); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "유닛 피커 스크랩 시 1번 위치만 사용합니다."); } break;
        //                    case (int)OPTNUM.UNIT_PICKER_VACUUM_DELAY: { itemOptions[i] = new OptionItem(i, "UNIT PICKER", "다이싱 소우 커팅 테이블 안착 후 진공 대기 시간."); } break;
        //                    case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "맵 피커의 슬로우 다운 높이(mm)"); } break;
        //                    case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "맵 피커의 슬로우 다운 속도(mm/sec)"); } break;
        //                    case (int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "맵 피커 에어 드라이 반복 횟수 "); } break;
        //                    case (int)OPTNUM.MAP_INSPECTION_CHECK_COUNT: { itemOptions[i] = new OptionItem(i, "MAP PICKER", "맵 인스펙션 실패 시 알람 발생 수량"); } break;

        //                    case (int)OPTNUM.MAP_STAGE_SIZE_HEIGHT: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵 스테이지의 세로 사이즈(mm)"); } break;
        //                    case (int)OPTNUM.MAP_STAGE_SIZE_WIDTH: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵 스테이지의 가로 사이즈(mm)"); } break;
        //                    case (int)OPTNUM.MAP_STAGE_1_USE: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵스테이지 1번을 사용합니다."); } break;
        //                    case (int)OPTNUM.MAP_STAGE_2_USE: { itemOptions[i] = new OptionItem(i, "MAP STAGE", "맵스테이지 2번을 사용합니다."); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "픽업 후 T축 미리 이동을 사용합니다"); } break; //220617
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 진공을 확인하는 지연시간(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 진공을 파기하는 시간(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 진공 파기를 끄고 기다리는 시간(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 픽업 위치에 이동 후 기다리는 시간(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 볼 검사 위치에 이동 후 기다리는 시간(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 플레이스 위치에 이동 후 기다리는 시간(msec)"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 픽업 실패 시 재시도하는 횟수"); } break;
        //                    case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커가 픽업 실패 시 진공을 확인하는 지연시간(msec)"); } break;
        //                    case (int)OPTNUM.BOTTOM_CAM_ANGLE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "플레이스 시 각도 오프셋"); } break;

        //                    case (int)OPTNUM.PICKER_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 1번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 2번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_3_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 3번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_4_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 4번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_5_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 5번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_1_PAD_6_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 1번의 패드 6번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_1_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 1번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_2_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 2번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_3_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 3번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_4_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 4번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_5_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 5번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKER_2_PAD_6_USE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "멀티 피커 헤드 2번의 패드 6번을 사용합니다"); } break;
        //                    case (int)OPTNUM.PICKUP_UNIT_SKIP_USAGE: { itemOptions[i] = new OptionItem(i, "MULTI PICKER", "픽업 실패 시 맵테이블의 UNIT을 스킵하는 기능을 사용합니다."); } break;

        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_SIZE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "트레이 스테이지 UP / DOWN 동작 및 충돌 방지를 위한 스테이지 사이즈(mm)"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_1_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD 트레이 스테이지 1 CLAMP 반복 횟수"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_2_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD 트레이 스테이지 2 CLAMP 반복 횟수"); } break;
        //                    case (int)OPTNUM.REWORK_TRAY_ALIGN_RETRY_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "REWORK 트레이 스테이지 CLAMP 반복 횟수"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_1_USE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD 트레이 1을 사용합니다"); } break;
        //                    case (int)OPTNUM.GOOD_TRAY_STAGE_2_USE: { itemOptions[i] = new OptionItem(i, "TRAY TABLE", "GOOD 트레이 2를 사용합니다"); } break;

        //                    case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이 피커의 슬로우 다운 높이(mm)"); } break;
        //                    case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이 피커의 슬로우 다운 속도(mm/sec)"); } break;
        //                    case (int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이 피커의 감지 센서를 사용합니다."); } break;
        //                    case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_USE: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이를 뒤집어서 사용합니다."); } break;//220516
        //                    case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_PITCH: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이를 뒤집어서 공정을 진행할 시 피커 Z축에 더해지는 피치(mm)."); } break;//220516
        //                    case (int)OPTNUM.TRAY_PICKER_MOVE_READY_WAIT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY PICKER", "트레이 피커가 준비 위치로 이동하는 대기 시간(sec)."); } break;

        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 트레이를 받기위해 대기하는 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 트레이를 받기위해 대기하는 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_WORK_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 트레이를 받기위해 대기하는 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 트레이를 받기위해 떠받히는 위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 트레이를 받기위해 떠받히는 위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_GETTING_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 트레이를 받기위해 떠받히는 위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 트레이 유무 판단할 수 있는 기준위치"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 트레이 유무 판단할 수 있는 기준위치"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 트레이를 배출 할 수 있는 준비위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_WORKING_WAIT_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 트레이를 배출 할 수 있는 준비위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(1) 엘리베이터가 준비위치를 다시 설정 하기 위한 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_RESEARCH_OFFSET: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이(2) 엘리베이터가 준비위치를 다시 설정 하기 위한 안전위치 오프셋"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1) 만재 배출 개수"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2) 만재 배출 개수"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_OUT_MAX_COUNT: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터 만재 배출 개수"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(1)가 오프셋 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "양품 트레이 엘리베이터(2)가 오프셋 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_RW_TRAY_ELV_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "재작업 트레이 엘리베이터가 오프셋 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_1_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이 엘리베이터(1)가 오프셋 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_2_OFFSET_MOVE_VEL: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "빈 트레이 엘리베이터(2)가 오프셋 이동하는 저속 스피드"); } break;
        //                    case (int)OPTNUM.ULD_ELV_COVER_WAIT_TIME: { itemOptions[i] = new OptionItem(i, "TRAY ELEVATOR", "엘리베이터 커버가 닫히고 나서 기다리는 시간"); } break;

        //                    case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET: { itemOptions[i] = new OptionItem(i, "CLEANER", "클리너 테이블의 슬로우 다운 높이(mm)"); } break;
        //                    case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_VEL: { itemOptions[i] = new OptionItem(i, "CLEANER", "클리너 테이블의 슬로우 다운 속도(mm/sec)"); } break;
        //                    case (int)OPTNUM.SCRAP_MODE_USE: { itemOptions[i] = new OptionItem(i, "CLEANER", "스크랩 모드를 사용합니다."); } break;
        //                    case (int)OPTNUM.CLEANER_LOG_ANALYSIS: { itemOptions[i] = new OptionItem(i, "CLEANER", "클리너 분석 로그 옵션을 사용합니다. (테스트)"); } break;
        //                    case (int)OPTNUM.CLEAN_CYLINDER_DELAY: { itemOptions[i] = new OptionItem(i, "CLEANER", "클리너 실린더 딜레이 시간(msec)"); } break;
        //                    default: { itemOptions[i] = new OptionItem(i); } break;
        //                }
        //            }
        //            break;
        //        default:
        //            break;
        //    }

        //}

        #region Serialization
        /// <summary>
        /// 파일에 xml형태로 정보를 저장합니다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>성공 여부</returns>
        public bool Serialize(string path)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, this);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(new System.Windows.Forms.Form() { TopMost = true }, string.Format("Cannot Serialize Config : {0}", ex));
                return false;
            }

            return true;
        }
        /// <summary>
        /// xml형태의 파일에서 정보를 불러옵니다.
        /// </summary>
        /// <param name="path">파일 경로</param>
        /// <returns>반환 객체</returns>
        public static Config Deserialize(string path)
        {
            Config inst = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
                using (StreamReader rd = new StreamReader(path))
                {
                    inst = (Config)xmlSerializer.Deserialize(rd);
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(new System.Windows.Forms.Form() { TopMost = true }, string.Format("Cannot Deserialize Config : {0}", ex));
            }

            return inst;
        }
        #endregion
    }

    [Serializable]
    public class CfgServo
    {
        public double[] dPos = new double[SVDF.POS_CNT];
        //아래 속도와 가감속은 일반적인경우 사용하지 않는다.. 특별히 모델별로 속도조정이 필요한 시퀀스에서만 추가해서 쓸것..
        public double[] dVel = new double[SVDF.POS_CNT];
        public double[] dAcc = new double[SVDF.POS_CNT];
        public double[] dDec = new double[SVDF.POS_CNT];
    }

    [Serializable]
    public class TowerLampOpt
    {
        public bool bBuzzerOn_1;
        public bool bBuzzerOn_2;

        public int nRed;
        public int nYellow;
        public int nGreen;
    }

    public enum Language : int
    {
        ENGLISH = 0,
        CHINA = 1,
        KOREAN = 2,
    }

    [Serializable]
    public class CfgGeneral
    {
        public string strEquipName = "";
        public string strEquipNumber = "";
        public int nLanguage = (int)Language.ENGLISH;

        public bool bEQPStandard = false;
        public bool bEQPMirror = false;

        public bool isDoorAlarmSkip = false;

        //public int nExpAlarm = 30;
        //public int nExpLog = 30;

        public bool bEesOnlineState = false;

        public int nPickerCalRetryCount = 10;
        public double dPickerCalTolerence = 0.005;
        public bool bPickerCalMoveTeach = false;
    }

    [Serializable]
    public class CfgMotionData
    {
        public int nAxis = 0;
        public string strAxisName = "";
        public double dSwLimitP = 999999;
        public double dSwLimitN = -999999;
        public double dVel = 10;
        public double dMaxVel = 1500;
        public double dAcc = 50;
        public double dDec = 50;
        public long lMoveTime = 10000;
        public long lHomeTime = 10000;
        public double dJogLowVel = 0.1;
        public double dJogMdlVel = 10;
        public double dJogHighVel = 50;
        public double dHomeInitPos = 0;
        public double dHomeOffset = 0.0;
        public double dInpositionBand = 0.1;

        public double[] dSafetyPos = new double[10];

        public CfgMotionData()
        {

        }

        public CfgMotionData(int index)
        {
            nAxis = index;
            for (int i = 0; i < 10; i++)
            {
                dSafetyPos[i] = 0;
            }
        }

        public void CopyTo(ref CfgMotionData info, bool bCopyName = false)
        {
            if (bCopyName) info.strAxisName = this.strAxisName;
            info.dSwLimitN = this.dSwLimitN;
            info.dSwLimitP = this.dSwLimitP;
            info.dVel = this.dVel;
            info.dMaxVel = this.dMaxVel;
            info.dAcc = this.dAcc;
            info.lMoveTime = this.lMoveTime;
            info.lHomeTime = this.lHomeTime;
            info.dJogLowVel = this.dJogLowVel;
            info.dJogMdlVel = this.dJogMdlVel;
            info.dJogHighVel = this.dJogHighVel;
            info.dHomeInitPos = this.dHomeInitPos;

            for (int nCnt = 0; nCnt < dSafetyPos.Length; nCnt++)
            {
                info.dSafetyPos[nCnt] = this.dSafetyPos[nCnt];
            }
        }
    }

    /// <summary>
    /// Picker Head Offset
    /// </summary>
    [Serializable]
    public class CfgCamCenterOffset
    {
        public double dDefPickerOffsetX = 0.0;
        public double dDefPickerOffsetY = 0.0;

        public CfgCamCenterOffset()
        {
            //
        }

    }
    /// <summary>
    /// Picker Head Offset
    /// </summary>
    [Serializable]
    public class CfgPickerHeadOffset
    {
        public double dDefPickerPitchX = 0.0;
        public double dDefPickerPitchY = 0.0;

        // PNP용
        public dxy[][] dpPickerPitchAbsRef = new dxy[CFG_DF.MAX_DEGREE_NO][];
        public dxy[][] dpPickerPitchOffset = new dxy[CFG_DF.MAX_DEGREE_NO][];

        // 인스펙션용
        public dxy[][] dpPickerPitchInspAbsRef = new dxy[CFG_DF.MAX_DEGREE_NO][];
        public dxy[][] dpPickerPitchInspOffset = new dxy[CFG_DF.MAX_DEGREE_NO][];

        public CfgPickerPadOffset[] dPnpOffsetZ = new CfgPickerPadOffset[CFG_DF.MAX_PICKER_PAD_CNT];
        public CfgPickerPadOffset[] dPnpOffsetT = new CfgPickerPadOffset[CFG_DF.MAX_PICKER_PAD_CNT];
        public CfgPickerHeadOffset()
        {
            //
            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            {
                dPnpOffsetZ[i] = new CfgPickerPadOffset();
                dPnpOffsetT[i] = new CfgPickerPadOffset();
            }

            for (int nDegCnt = 0; nDegCnt < CFG_DF.MAX_DEGREE_NO; nDegCnt++)
            {
                dpPickerPitchAbsRef[nDegCnt] = new dxy[CFG_DF.MAX_PICKER_PAD_CNT];
                dpPickerPitchOffset[nDegCnt] = new dxy[CFG_DF.MAX_PICKER_PAD_CNT];

                dpPickerPitchInspAbsRef[nDegCnt] = new dxy[CFG_DF.MAX_PICKER_PAD_CNT];
                dpPickerPitchInspOffset[nDegCnt] = new dxy[CFG_DF.MAX_PICKER_PAD_CNT];

                for (int nPadCnt = 0; nPadCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPadCnt++)
                {
                    dpPickerPitchAbsRef[nDegCnt][nPadCnt] = new dxy();
                    dpPickerPitchOffset[nDegCnt][nPadCnt] = new dxy();

                    dpPickerPitchInspAbsRef[nDegCnt][nPadCnt] = new dxy();
                    dpPickerPitchInspOffset[nDegCnt][nPadCnt] = new dxy();
                }
            }
        }
    }

    [Serializable]
    public class CfgPickerPadOffset
    {
        public double[] dPickUpWait = new double[CFG_DF.MAP_STG_CNT];
        public double[] dPickUp = new double[CFG_DF.MAP_STG_CNT];
        public double[] dPlaceWait = new double[CFG_DF.TRAY_STG_CNT];
        public double[] dPlace = new double[CFG_DF.TRAY_STG_CNT];

        public CfgPickerPadOffset()
        {
            //
            for (int i = 0; i < CFG_DF.MAP_STG_CNT; i++)
            {
                dPickUpWait[i] = 0.0;
                dPickUp[i] = 0.0;
            }
            for (int i = 0; i < CFG_DF.TRAY_STG_CNT; i++)
            {
                dPlaceWait[i] = 0.0;
                dPlace[i] = 0.0;
            }
        }
    }

    /// <summary>
    /// Vision Table Offset
    /// </summary>
    [Serializable]
    public class CfgVisionTableOffset
    {
        public double dDefVisionTablePitchX = 0.0;
        public double dDefVisionTablePitchY = 0.0;
    }

    /// <summary>
    /// Tray Table Offset
    /// </summary>
    [Serializable]
    public class CfgTrayTableOffset
    {
        public double dDefTrayTablePitchX = 0.0;
        public double dDefTrayTablePitchY = 0.0;
    }

    [Serializable]
    public class CfgTime
    {
        public bool bUse = false;
        public int nHour = 0;
        public int nMin = 0;
    }

    [Serializable]
    public class OptionItem
    {
        public string strValue = "0";

        public int nIndex
        {
            get;
            set;
        }
        public string strOptionPart
        {
            get;
            set;
        }
        public string strOptionName
        {
            get;
            set;
        }
        public bool bOptionUse
        {
            get;
            set;
        }
        public int nValue
        {
            get
            {
                int Tmp = 0;
                if (!int.TryParse(strValue, out Tmp)) return Tmp;
                return Tmp;
            }
        }
        public double dValue
        {
            get
            {
                double Tmp = 0;
                if (!double.TryParse(strValue, out Tmp)) return Tmp;
                return Tmp;
            }
        }
        public long lValue
        {
            get
            {
                long Tmp = 0;
                if (!long.TryParse(strValue, out Tmp)) return Tmp;
                return Tmp;
            }
        }
        public OptionItem()
        {

        }
        public OptionItem(int nOptionIdx)
        {
            nIndex = nOptionIdx;
            strOptionPart = "NotDefined";
            strOptionName = "NotDefined";
            strValue = "0";
            bOptionUse = false;
        }
        public OptionItem(int nOptionIdx, string strPart = "NotDefined", string strName = "NotDefined")
        {
            nIndex = nOptionIdx;
            strOptionPart = strPart;
            strOptionName = strName;
            strValue = "0";
            bOptionUse = false;
        }
    }

    [Serializable]
    public enum OPTNUM
    {
        #region OPERATION (USE / SKIP) 0 ~ 999
        GEM_USE = 0,
        VAC_SENSOR_USE,
        DRY_RUN_MODE,
        DRY_RUN_VISION_INTERFACE_USE,   // DRY_RUN_STRIP_USE 대신 넣음
        DRY_RUN_SAW_USE,
        DRY_RUN_RANDOM_SORT,
        BUZZER_USE,
        IONIZER_USE,
        TOP_VISION_USE,
        BOTTOM_VISION_USE,
        TOP_ALIGN_USE,
        BIN_SORTING_MODE,
        DUMMY_MODE,
        TRAY_INSPECTION_USE,
        TRAY_INSPECTION_DETECT_DELAY,
        EQP_INIT_TIME,
        CYL_MOVE_TIME,
        VAC_CHECK_TIME,
        MOT_MOVE_TIME,
        MES_REPLY_TIME,
        MAP_INSPECTION_MODE_VERTICAL_USE,
        BOTTOM_VISION_OFFSET_USE,
        BLOW_VAC_SENSOR_USE,
        ITS_XOUT_USE,

        DOOR_SAFETY_CHECK_USE, // 도어 세이프티 체크 사용 모드
        ITS_ID_STRIP_ID_VALID_USE,//220604 ITS ID와 Strip Id의 앞 12자리가 일치 하는지 확인 옵션
        ITS_ID_STRIP_ID_COMMON_STR_LEN,
        BOTTOM_VISION_ALARM_USE,
        VISION_INTERFACE_RETRY_COUNT,//220615

        NO_MSG_STOP_TIME,
        ULD_MGZ_FULL_CHECK_TIME,
        IN_LET_TABLE_UNUSED_MODE, //221012 HEP 인렛테이블 없이 사용한다고 하여 추가
        VISION_INTERFACE_DATA_CLEAR_MODE,

        VISION_INTERFACE_TIMEOUT = 500,
        SAW_INTERFACE_TIMEOUT,

        LD_VISION_PREALIGN_GRAB_DELAY,
        LD_VISION_ORIENT_GRAB_DELAY,
        LD_VISION_BARCODE_GRAB_DELAY,
        MAP_VISION_GRAB_DELAY,
        BALL_VISION_GRAB_DELAY,
        IONIZER_MONITOR_ALARM,
        IONIZER_MAX_VALUE,
        IONIZER_MIN_VALUE,
        IONIZER_MONITOR_OFFSET,
        IONIZER_MONITOR_1_OFFSET,
        IONIZER_MONITOR_2_OFFSET,
        IONIZER_MONITOR_3_OFFSET,
        IONIZER_MONITOR_4_OFFSET,
        SCRAP_BOX_ALARM_CLEAR_TIME_OUT,
        REJECT_BOX_ALARM_CLEAR_TIME_OUT,

        //220621 삭제 기한
        EVENT_LOG_EXP_PERIOD = 600,
        ALARM_LOG_EXP_PERIOD,
        SEQ_LOG_EXP_PERIOD,
        ITS_LOG_EXP_PERIOD,
        LOT_LOG_EXP_PERIOD,
        STRIP_LOG_EXP_PERIOD,
        PROC_QTY_LOG_EXP_PERIOD,

        GROUP1_USE = 700, //sj.shin 2023-10-31 현재 1열 그룹으로 되어져 있어 비전에서 에러 발생, 다중 열 그룹으로 변경
        MAPVISIONDATA_USE , //sj.shin 2023-10-31 현재 1열 그룹으로 되어져 있어 비전에서 에러 발생, 다중 열 그룹으로 변경

        AIRKNIFE_RUN_START_ON_USE,
        USE_MAGAZINE_BARCODE,
        MAGAZINE_BARCODE_READING_TIME,
        #endregion

        #region CONVEYOR 1000 ~ 1099
        CONV_MAX_ROLLING_TIMEOUT = 1000,
        CONV_ROLLING_TIME,
        CONV_MAX_ROLLING_TIMEOUT_USE, // 컨베이어 구동 시 타임아웃 체크 사용 모드
        CONV_SAFETY_SENSOR_USE, // 컨베이어 라이트커튼 센서 사용 모드
        #endregion

        #region ULD ELEVATOR 1100 ~ 1199
        #endregion

        #region LD ELEVATOR 1200 ~ 1299
        #endregion

        #region MAP PUSHER 1300 ~ 1399
        STRIP_BOTTOM_BARCODE_READ_DELAY = 1300,
        STRIP_BOTTOM_BARCODE_RETRY_COUNT,
        STRIP_BOTTOM_BARCODE_READ_TIMEOUT,
        STRIP_BOTTOM_BARCODE_READ_USE,
        STRIP_PUSHER_OVERLOAD_CHECK,
        STRIP_MAGAZINE_DOOR_CLIP_OPEN_CHECK, // 매거진 공급 위치 투입 전 클립이 오픈 됐는지 확인하는 모드
        STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE, // 로더 레일 상단에 자재 체크하는 센서 사용 모드
        STRIP_RAIL_Y_UNLOAD_PRE_MOVE, // 로더 레일 픽업 시 Z축 하강 전 로드 레일 Y 언로딩 위치로 미리이동
        #endregion

        #region LD RAIL 1400 ~ 1499
        STRIP_PREALIGN_USE = 1400,
        STRIP_VISION_BARCODE_READ_USE,
        STRIP_ORIENT_CHECK_USE,
        STRIP_PREALIGN_TARGET_ANGLE,
        STRIP_PICKER_PRESS_TO_STRIP_USE,

        #endregion

        #region STRIP PICKER 1500 ~ 1599
        STRIP_PICKER_Z_SLOW_DOWN_OFFSET = 1500,
        STRIP_PICKER_Z_SLOW_DOWN_VEL,
        STRIP_PICKER_INTERLOCK_MIN_X_POSITION,
        STRIP_PICKER_INTERLOCK_MAX_X_POSITION,
        STRIP_PICKER_X_HOME_OFFSET_VALUE,
        STRIP_PICKER_X_INTERLOCK_SAFETY_POSITION,
        STRIP_PK_n_UNIT_PK_INTERLOCK_GAP,
        #endregion

        #region UNIT PICKER 1600 ~ 1699
        UNIT_PICKER_Z_SLOW_DOWN_OFFSET = 1600,
        UNIT_PICKER_Z_SLOW_DOWN_VEL,
        UNIT_PICKER_SCRAP_ONE_POINT,
        UNIT_PICKER_VACUUM_DELAY,
        #endregion

        #region CLEANING TABLE 1700 ~ 1799

        #endregion

        #region DRY BLOCK STAGE 1800 ~ 1899
        #endregion

        #region MAP PICKER 1900 ~ 1999
        MAP_PICKER_Z_SLOW_DOWN_OFFSET = 1900,
        MAP_PICKER_Z_SLOW_DOWN_VEL,
        MAP_PICKER_AIR_DRY_REPEAT_COUNT,//220507 pjh 추가
        MAP_INSPECTION_CHECK_COUNT,
        #endregion

        #region MAP STAGE 2000 ~ 2099
        MAP_STAGE_SIZE_HEIGHT = 2000,
        MAP_STAGE_SIZE_WIDTH,
        MAP_STAGE_1_USE,
        MAP_STAGE_2_USE,
        MAP_STAGE_AIRKNIFE_USE,
        MAP_STAGE_AIRKNIFE_COUNT,
        #endregion

        #region MULTI PICKER 2100 ~ 2199
        MULTI_PICKER_CHIP_PICK_UP_DELAY = 2100,
        MULTI_PICKER_CHIP_PLACE_ON_DELAY,
        MULTI_PICKER_CHIP_PLACE_OFF_DELAY,

        MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY,
        MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY,
        MULTI_PICKER_CHIP_PLACE_MOTION_DELAY,

        MULTI_PICKER_CHIP_PICK_UP_RETRY,
        MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY,

        MULTI_PICKER_CHIP_PLACE_ONLY_REVERSE_USE,
        MULTI_PICKER_CHIP_PLACE_TOP_TRAY,

        BOTTOM_CAM_ANGLE,

        MULTI_PICKER_PAD_PRE_TURN_USE, //220617
        MULTI_PICKER_TTLOG_USE,

        PICKER_1_USE = 2178,
        PICKER_1_PAD_1_USE,
        PICKER_1_PAD_2_USE,
        PICKER_1_PAD_3_USE,
        PICKER_1_PAD_4_USE,
        PICKER_1_PAD_5_USE,
        PICKER_1_PAD_6_USE,
        PICKER_1_PAD_7_USE,
        PICKER_1_PAD_8_USE,
        PICKER_2_USE,
        PICKER_2_PAD_1_USE,
        PICKER_2_PAD_2_USE,
        PICKER_2_PAD_3_USE,
        PICKER_2_PAD_4_USE,
        PICKER_2_PAD_5_USE,
        PICKER_2_PAD_6_USE,
        PICKER_2_PAD_7_USE,
        PICKER_2_PAD_8_USE,
        PICKUP_UNIT_SKIP_USAGE,
        #endregion

        #region BALL VISION 2200 ~ 2299
        #endregion

        #region TRAY STAGE 2300 ~ 2399
        GOOD_TRAY_STAGE_SIZE = 2300,
        GOOD_TRAY_1_ALIGN_RETRY_COUNT,
        GOOD_TRAY_2_ALIGN_RETRY_COUNT,
        REWORK_TRAY_ALIGN_RETRY_COUNT,
        GOOD_TRAY_STAGE_1_USE,
        GOOD_TRAY_STAGE_2_USE,
        USE_ELV_COVER_UNLOAD,
        #endregion

        #region REJECT BOX 2400 ~ 2499
        #endregion

        #region TRAY PICKER 2500 ~ 2599
        TRAY_PICKER_Z_SLOW_DOWN_OFFSET = 2500,
        TRAY_PICKER_Z_SLOW_DOWN_VEL,
        TRAY_PICKER_DETECT_SENSOR_USE,
        TRAY_FLIP_LD_N_ULD_USE,//220516
        TRAY_FLIP_LD_N_ULD_PITCH,//220516
        TRAY_PICKER_MOVE_READY_WAIT_TIME,
        #endregion

        #region TRAY ELEVATOR 2600 ~ 2699
        ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET = 2600,
        ULD_GD_TRAY_ELV_2_WORK_SAFETY_OFFSET,
        ULD_RW_TRAY_ELV_WORK_SAFETY_OFFSET,
        ULD_GD_TRAY_ELV_1_GETTING_OFFSET,
        ULD_GD_TRAY_ELV_2_GETTING_OFFSET,
        ULD_RW_TRAY_ELV_GETTING_OFFSET,

        ULD_GD_TRAY_ELV_1_SAFETY_OFFSET,
        ULD_GD_TRAY_ELV_2_SAFETY_OFFSET,
        ULD_RW_TRAY_ELV_SAFETY_OFFSET,
        ULD_EMPTY1_TRAY_ELV_SAFETY_OFFSET,
        ULD_EMPTY2_TRAY_ELV_SAFETY_OFFSET,
        ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL,
        ULD_GD_TRAY_ELV_2_SAFETY_MOVE_VEL,
        ULD_RW_TRAY_ELV_SAFETY_MOVE_VEL,
        ULD_EMPTY1_TRAY_ELV_SAFETY_MOVE_VEL,
        ULD_EMPTY2_TRAY_ELV_SAFETY_MOVE_VEL,
        ULD_GD_TRAY_ELV_1_EXIST_CHECK_POS_LIMIT,
        ULD_GD_TRAY_ELV_2_EXIST_CHECK_POS_LIMIT,
        ULD_RW_TRAY_ELV_EXIST_CHECK_POS_LIMIT,
        ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT,
        ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT,
        ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET,
        ULD_EMPTY2_TRAY_ELV_WORKING_WAIT_OFFSET,
        ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET,
        ULD_EMPTY2_TRAY_ELV_RESEARCH_OFFSET,

        ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT,
        ULD_GD_TRAY_ELV_2_OUT_MAX_COUNT,
        ULD_RW_TRAY_ELV_OUT_MAX_COUNT,
        ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL,
        ULD_GD_TRAY_ELV_2_OFFSET_MOVE_VEL,
        ULD_RW_TRAY_ELV_OFFSET_MOVE_VEL,
        ULD_EMPTY_TRAY_ELV_1_OFFSET_MOVE_VEL,
        ULD_EMPTY_TRAY_ELV_2_OFFSET_MOVE_VEL,
        ULD_ELV_COVER_WAIT_TIME,
        ULD_TRAY_CONV_IN_TIME_USE,//220604
        ULD_TRAY_CONV_IN_SEN_DETECT_TIME,
        #endregion

        #region CLEANER 2700 ~ 2799
        //CLEANER_BY_PASS_USE = 2700,
        //CLEANER_END_USE,
        //PLASMA_USE,
        //PLASMA_START_WAIT_TIME,
        //PLASMA_END_WAIT_TIME,
        //WATER_FLOW_CHECK,
        //CLEANING_AIR_USE,
        CLEANER_Z_SLOW_DOWN_OFFSET,
        CLEANER_Z_SLOW_DOWN_VEL,
        SCRAP_MODE_USE,
        //CLEAN_PICKER_DRY_INTERLOCK_Y_POS,
        //CLEAN_PICKER_DRY_INTERLOCK_Z_POS,
        //CLEAN_PICKER_DRY_INTERLOCK_X_MIN_VALUE_GAP,
        //CLEAN_PICKER_DRY_INTERLOCK_X_MAX_VALUE_GAP,
        //HEATING_SV_VALUE,
        //HEATING_MAX_SV_VALUE,
        //HEATING_MODE,
        //SECOND_CLEAN_DRY_MODE,
        CLEANER_LOG_ANALYSIS,
        //CLEANER_PICKER_PLASMA_MODE,
        //HEATING_MODE_AUTO,
        //FIRST_PLASMA_MODE,
        //SECOND_PLASMA_MODE,
        CLEAN_CYLINDER_DELAY,
        //SWAP_SPONGE_FIRSTCLEAN_USE,
        //SECOND_CLEAN_PICKUP_DRY_MODE,
        //ULTRASONIC_WATER_WARM_UP_TIME,
        //FIRST_ULTRASONIC_MODE,
        //ULTRASONIC_WATER_FILL_TIME,
        #endregion

        #region VISION 2800 ~ 2999
        #endregion
        MAX = 3000,
    }

    [Serializable]
    public class CfgVision
    {
        public int[] nLedBright = new int[2];

        public bool bBlackClass;
        public int nObjMinArea;
        public int nObjMaxArea;
        public int nObjThreshold;

        public double dColPitch;
        public double dRowPitch;

        public double dXResolution;
        public double dYResolution;
        public double dSkewedAngle;

        public double dXCorrectAngle;
        public double dYCorrectAngle;

        public double dCamCorrectAngle;

        public string strCalPath;

        public CfgVision()
        {
            //
        }

        public void CopyTo(ref CfgVision info)
        {
            for (int i = 0; i < 2; i++)
            {
                info.nLedBright[i] = this.nLedBright[i];
            }

            info.bBlackClass = this.bBlackClass;
            info.nObjMinArea = this.nObjMinArea;
            info.nObjMaxArea = this.nObjMaxArea;
            info.nObjThreshold = this.nObjThreshold;
            info.dColPitch = this.dColPitch;
            info.dRowPitch = this.dRowPitch;

            info.dXResolution = this.dXResolution;
            info.dYResolution = this.dYResolution;
            info.dSkewedAngle = this.dSkewedAngle;
            info.dXCorrectAngle = this.dXCorrectAngle;
            info.dYCorrectAngle = this.dYCorrectAngle;
            info.strCalPath = this.strCalPath;
        }
    }
    [Serializable]
    public class AnalogOut
    {
        public long lAnalogOutputValue;
        public AnalogOut()
        {
            this.lAnalogOutputValue = 100;
        }
    }

    [Serializable]
    public class VacuumOption
    {
        public long lVacOnDelay;
        public long lBlowOnDelay;
        public long lBlowOffDelay;
        public double dVacLevelLow;
        public double dVacLevelHigh;
        public double dRatio;
        public double dDefaultVoltage;

        public VacuumOption()
        {
            lVacOnDelay = 3000;
            lBlowOnDelay = 3000;
            lBlowOffDelay = 3000;
            dVacLevelLow = -20;
            dVacLevelHigh = 10;
            dRatio = 1.0;
            dDefaultVoltage = 1.0;
        }
    }

    public class MyBoolEditor : UITypeEditor
    {
        public override bool GetPaintValueSupported
            (System.ComponentModel.ITypeDescriptorContext context)
        { return true; }
        public override void PaintValue(PaintValueEventArgs e)
        {
            var rect = e.Bounds;
            rect.Inflate(1, 1);
            ControlPaint.DrawCheckBox(e.Graphics, rect, ButtonState.Flat |
                (((bool)e.Value) ? ButtonState.Checked : ButtonState.Normal));
        }
    }
}
