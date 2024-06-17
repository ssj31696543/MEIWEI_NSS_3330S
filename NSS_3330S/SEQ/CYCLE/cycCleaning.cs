using NSSU_3400.MOTION;
using NSSU_3400.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSU_3400.SEQ.CYCLE
{
    public class cycCleaning : SeqBase
    {
        double m_dPos = 0.0;
        bool m_bAutoMode = true;
        bool m_bVacCheck = true;
        bool m_bIsStrip = true;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;

        int m_nFirstCleanAirCount = 0;
        int m_nFirstCleanWaterCount = 0;
        int m_nFirstDryCount = 0;
        int m_nSecondCleanAirCount = 0;
        int m_nSecondCleanWaterCount = 0;
        int m_nSecondDryCount = 0;

        int m_nPlasmaPickUpDryCount = 0;
        int m_nSecondCleanPickUpDryCount = 0;

        int nTotalWaterSwingCount = 0;
        int nTotalAirSwingCount = 0;

        int m_nSpongeCount = 0;
        int m_nFirstPitchCount = 0;
        int m_nSecondPitchCount = 0;
        int m_nFirstPlasmaRepeatCount = 0;
        int m_nSecondPlasmaRepeatCount = 0;
        int m_nPickerPlasmaRepeatCount = 0;

        int m_nScrap1Count = 0;
        int m_nScrap2Count = 0;

        #region 플라즈마 이동
        public int[]    m_nAxisArray = null;
        public double[] m_dPosArray = null;
        public uint[]   m_nOrderArray = null;
        public double[] m_dSpeedArray = null;
        public double[] m_dAccArray = null;
        public double[] m_dDecArray = null;

        double m_dPlasmaXPos = 0;
        double m_dPlasmaYPos = 0;
        #endregion

        //수세기 전용 [2022.05.05 작성자 : 홍은표]
        int m_nCleanUltraDryNum = 0;

        Stopwatch m_swFirstUltra= new Stopwatch();
        Stopwatch m_swSecondUltra = new Stopwatch();

        #region PROPERTY
        public seqCleaning SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sCleaning[m_nCleanUltraDryNum]; }
        }

        #endregion

        #region ENUM DEFINE
        enum CYCLE_FIRST_INIT_MOVE
        {
            #region 처음 시작시 이동
            NONE = -1,
            BEFORE_MOVE_VACUUM_CHECK,
            UNIT_PICKER_Z_READY_POS_MOVE,
            UNIT_PICKER_X_READY_POS_MOVE,
            FINISH,
            #endregion
        }
        enum CYCLE_FIRST_INIT
        {
            #region 초기화 및 대기
            NONE = -1,
            WARM_UP_WATER,
            INIT, //1차 CYLINDER 초기화
            ULTRASONIC_SET, //초음파 설정
            FINISH,
            #endregion
        }
        enum CYCLE_FIRST_CLEAN
        { 
            #region 1차 세정
            NONE = -1,
            INIT_CYL,
            INIT_MOVE,
            MOVE_CLEAN_X_POS,
            MOVE_CLEAN_Z_POS,
            CLEAN_OUTPUT_ON,
            WATER_JET_FORWARD,
            CLEAN_OUTPUT_OFF,
            WATER_JET_BACKWARD,
            CHECK_CLEAN_COUNT,
            AIR_OUTPUT_ON,
            AIR_WATER_JET_FORWARD,
            AIR_OUTPUT_OFF,
            AIR_WATER_JET_BACKWARD,
            AIR_CHECK_CLEAN_COUNT,
            MOVE_CLEAN_READY_Z_POS, //카운트 종료시 Z축 대기위치 이동
            FINISH,
            #endregion
        }
        enum CYCLE_FIRST_CLEAN_SWING
        {
            #region 1차 스폰지 세정
            NONE = -1,
            INIT_READY_POS,
            CHECK_SPONGE_ON, //스폰지 물없으면 ON
            MOVE_CLEAN_X_POS,
            MOVE_CLEAN_Z_POS,
            CHECK_CLEAN_COUNT,
            MOVE_CLEAN_READY_Z_POS, //카운트 종료시 Z축 대기위치 이동
            FINISH,
            #endregion
        }
        enum CYCLE_FIRST_US
        {
            #region 1차 초음파 US : Ultrasonic 초음파
            NONE = -1,
            INIT_READY_POS,
            CHECK_WATER_IN, //유량감지센서 확인하여물넘치는상황이면 센서 꺼질때까지 배출
            CHECK_US_SETTING, //레시피 설정 값과 현재 설정된 값 비교 후 다를 경우 통신 입력 하는 부분
            MOVE_US_X_POS,
            MOVE_US_Z_POS,
            US_START, //스탑워치 스타트 하여
            MOVE_LEFT,
            MOVE_RIGHT,
            MOVE_CHECK_CENTER,
            US_END,  //스탑워치 종료 하는 방식으로 
            MOVE_US_READY_Z_POS,
            FINISH,
            #endregion
        }
        enum CYCLE_SCRAP
        {
            #region 스크랩 구간
            NONE = -1,
            INIT,
            UNIT_PICKER_SCRAP_X_MOVE,
            UNIT_PICKER_SCRAP_Z_MOVE,
            SCRAP_BLOW_ON,
            WAIT_DELAY,
            UNIT_PICKER_READY_Z_MOVE,
            SCRAP_BLOW_OFF,
            CHECK_COUNT,
            FINISH,
            #endregion
        }

        enum CYCLE_FIRST_UNLOAD
        {
            #region 언로드 구간
            NONE = -1,
            WAIT_SECOND_CLEAN_FINISH, //안전한지 대기
            BEFORE_MOVE_CHECK_STRIP,
            MOVE_UNIT_PK_Z_READY_POS,
            CHECK_STRIP_INFO,
            AIR_BLOW_ON,
            MOVE_CLEAN_LOAD_POS,
            MOVE_UNLOAD_X_MOVE,
            AIR_BLOW_OFF,
            MOVE_UNLOAD_Z_PRE_MOVE,
            MOVE_UNLOAD_Z_MOVE,
            SECOND_CLEAN_ELV_VACUUM_ON,
            UNIT_PICKER_BLOW_ON,
            SECOND_CLEAN_ELV_VACUUM_ON_CHECK,
            MOVE_READY_Z_PRE_MOVE, 
            MOVE_READY_Z_MOVE, //Z -> X 순 이동, 회피
            UNIT_PICKER_BLOW_OFF,
            MOVE_READY_X_MOVE, //Z -> X 순 이동, 회피
            COMPLETE_FLAG_ON,
            FINISH,
            #endregion
        }
        enum CYCLE_SECOND_VAC_CHECK
        {
            #region 처음 시작시 이동
            NONE = -1,
            BEFORE_MOVE_VACUUM_CHECK,
            FINISH,
            #endregion
        }
        enum CYCLE_SECOND_INIT
        {
            #region 초기화 및 대기
            NONE = -1,
            WARM_UP_WATER,
            INIT, //2차 CYLINDER 초기화  //2차는 에어커튼과 셔터 확인해야함 여기서 미리 동작
            WAIT_SECOND_ULTRA_WATER_FLOW,
            WAIT_LOADING_COMPLETE, //대기 및 이전 작업 종료 확인
            FINISH,
            #endregion
        }
        enum CYCLE_SECOND_CLEAN
        {
            #region 2차 세정
            NONE = -1,
            CYL_INIT,
            CLEAN_FLIP,
            AFTER_FLIP_COVER_CLOSE,
            AIR_CURTAIN_ON,
            CLEAN_OUTPUT_ON,
            WATER_JET_FORWARD,
            CLEAN_OUTPUT_OFF,
            WATER_JET_BACKWARD,
            CHECK_CLEAN_COUNT,
            AIR_OUTPUT_ON,
            AIR_WATER_JET_FORWARD,
            AIR_OUTPUT_OFF,
            AIR_WATER_JET_BACKWARD,
            AIR_CHECK_CLEAN_COUNT,
            AIR_CURTAIN_OFF,
            AFTER_FLIP_COVER_OPEN,
            COVER_OPEN_DELAY,
            UNLOAD_FLIP, //카운트 종료시 정상 위치로 회전
            FINISH,
            #endregion
        }
        enum CYCLE_DRY_CLEAN
        {
            #region 드라이 2차 세척 테이블
            NONE = -1,
            CHECK_AND_INIT,
            MOVE_DRY_PLASMA_SAFETY_POS_Z_MOVE,
            MOVE_DRY_PLASMA_SAFETY_POS_Y_MOVE,
            MOVE_DRY_PLASMA_SAFETY_POS_X_MOVE,
            MOVE_DRY_START_READY_Z_POS,
            CLEANER_PICKER_SAFETY_X_POS,
            CLEANER_PICKER_TURN_POS,
            MOVE_FLIP_ELV_TURN_DRY_POS,

            MOVE_DRY_START_POS, //속도 따로 설정할 수 있게 변경
            MOVE_DRY_START_Z_POS,

            DRY_BLOW_ON,
            MOVE_DRY_END_POS,
            DRY_BLOW_OFF,
            CHECK_DRY_COUNT,
            MOVE_CLEANER_PICKER_DRY_SAFETY_POS_MOVE,
            MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE,
            MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE,
            DRY_UP_DOWN_SOL_CLEAR,
            FINISH,
            #endregion
        }
        enum CYCLE_SECOND_PICK_AND_PLACE
        {
            #region 세정 후 픽업 & 2차 초음파 플레이스
            NONE = -1,
            BEFORE_MOVE_CHECK_STRIP_INFO,
            CLEANER_PICKER_SAFETY_POS_Z_MOVE,
            CLEANER_PICKER_SAFETY_POS_X_MOVE,
            CHECK_STRIP_INFO,
            CLEANER_PICKER_PICK_UP_T_MOVE,
            CLEAN_FLIP_UNLOAD_POS_MOVE,
            CLEAN_LOAD_POS_X_MOVE,
            CLEAN_LOAD_POS_Z_PRE_MOVE,
            CLEAN_LOAD_POS_Z_MOVE,
            CLEAN_UNIT_PK_2_VACUUM_ON,
            CLEAN_ELV_BLOW_ON,
            CLEAN_UNIT_PK_2_VACUUM_ON_CHECK,
            CLEAN_UP_PRE_MOVE,
            CLEAN_UP_MOVE,
            CHECK_STRIP_INFO_PLACE,
            CLEAN_ELV_BLOW_OFF,
            ULTRASONIC_ELV_T_LOAD_MOVE,
            AIR_KNIFE_ON,
            ULTRASONIC_PLACE_POS_X_MOVE,
            AIR_KNIFE_OFF,
            ULTRASONIC_PLACE_POS_Z_PRE_MOVE,
            ULTRASONIC_PLACE_POS_Z_MOVE,
            ULTRASONIC_ELV_VAC_ON,
            CLEAN_UNIT_PK_2_BLOW_ON,
            ULTRASONIC_ELV_VAC_ON_CHECK,
            CLEAN_UP_PRE_MOVE_2,
            CLEAN_UP_MOVE_2,
            CLEAN_UNIT_PK_2_BLOW_OFF,
            CLEAN_SAFETY_X_MOVE,
            FINISH,
            #endregion
        }
        enum CYCLE_SECOND_US
        {
            #region 2차 초음파
            NONE = -1,
            CLEAN_FLIP_2_LOAD_POS_MOVE,
            CLEANER_PICKER_FLAG_ON,
            CHECK_WATER_IN, //유량감지센서 확인하여물넘치는상황이면 센서 꺼질때까지 배출
            CHECK_US_SETTING, //레시피 설정 값과 현재 설정된 값 비교 후 다를 경우 통신 입력 하는 부분
            US_FLIP,
            US_PFE_DOWN, //2차 초음파 Z축
            US_START, //스탑워치 스타트 하여
            MOVE_LEFT,
            MOVE_RIGHT,
            MOVE_CHECK_CENTER,
            US_END,  //스탑워치 종료 하는 방식으로 
            US_PFE_UP,
            US_UNLOAD_FLIP,
            CLEANER_PICKER_WAIT_ON,
            FINISH,
            #endregion
        }
        enum CYCLE_CLEANER_PICKER_PLASMA
        {
            NONE = -1,
            INTERLOCK_FLAG_CHECK,
            DRY_PICKER_Z_READY_MOVE,
            DRY_PICKER_Y_MOVE,
            DRY_PICKER_X_MOVE,
            CLEANER_PICKER_Z_SAFETY_MOVE,
            CLEANER_PICKER_X_PICKER_PLASMA_MOVE,
            CLEANER_PICKER_R_PICKER_PLASMA_MOVE,
            CLEANER_PICKER_Z_PLASMA_MOVE,
            PICKER_PLASMA_SHOT,
            DRY_PICKER_X_PICKER_PLASMA_MOVE,
            DRY_PICKER_Y_PICKER_PLASMA_MOVE,
            DRY_PICKER_Z_PICKER_PLASMA_MOVE,
            CL_PICKER_BLOW_SOL_ON,
            PICKER_PLASMA_WAIT,
            PICKER_PLASMA_START,
            PICKER_PLASMA_END,
            DRY_PICKER_SAFETY_Z_MOVE,
            DRY_PICKER_SAFETY_Y_MOVE,
            DRY_PICKER_SAFETY_X_MOVE,
            CL_PICKER_SAFETY_Z_MOVE,
            CL_PICKER_SAFETY_X_MOVE,
            FINISH,
        }
        enum CYCLE_SECOND_ULTRA_PICK_UP
        {
            #region 2차 초음파 완료후 자재 픽업
            NONE = -1,
            BEFORE_MOVE_CHECK_STRIP_INFO,
            CLEANER_PICKER_Z_SAFETY_MOVE,
            CLEANER_PICKER_X_SAFETY_MOVE,
            CLEANER_PICKER_R_SAFETY_MOVE,
            CHECK_STRIP_INFO,
            ULTRASONIC_LOAD_POS_X_MOVE,
            ULTRASONIC_LOAD_POS_Z_PRE_MOVE,
            ULTRASONIC_LOAD_POS_Z_MOVE,
            CLEANER_PICKER_VAC_ON,
            ULTRASONIC_BLOW_ON,
            CLEANER_PICKER_VAC_ON_CHECK,
            ULTRASONIC_UNLOAD_POS_Z_PRE_MOVE,
            ULTRASONIC_UNLOAD_POS_Z_MOVE,
            ULTRASONIC_BLOW_OFF,
            ULTRASONIC_UNLOAD_POS_X_MOVE,
            FINISH,
            #endregion
        }
        enum CYCLE_DRY_INIT
        {
            #region 초기화 & 플라즈마 온도 체크
            NONE = -1,
            INIT,
            CHECK_PLASMA_TEMP,
            FINISH,
            #endregion
        }
        enum CYCLE_DRY_FLIP_AND_MOVE
        {                                                                                                                           
            #region 플립
            NONE = -1,
            READY_Z_MOVE,
            DRY_FLIP_SAFETY_X_MOVE,
            DRY_FLIP_MOVE,
            CHECK_STRIP_SAFTEY_MOVE,
            CLEANER_PICKER_PLASMA_START_Z_MOVE,
            FINISH,
            #endregion
        }

        enum CYCLE_PLACE_DRY_STAGE
        {
            #region 드라이 스테이지 안착
            NONE = -1,
            WAIT_DRY_STAGE, //대기 및 이전 작업 종료 확인
            BEFORE_MOVE_CHECK_STRIP_INFO,
            DRY_PICKER_SAFETY_POS_Z_MOVE,
            DRY_PICKER_SAFETY_POS_Y_MOVE,
            DRY_PICKER_SAFETY_POS_X_MOVE,
            CLEANER_PICKER_SAFETY_Z_MOVE,
            CLEANER_PICKER_SAFETY_X_MOVE,
            CLEANER_PICKER_SAFETY_R_MOVE,
            CHECK_STRIP_INFO_PLACE,
            PLACE_DRY_STAGE_X_MOVE,
            CLEANER_PICKER_DRY_STAGE_X_MOVE,
            CLEANER_PICKER_DRY_STAGE_Z_PRE_MOVE,
            CLEANER_PICKER_DRY_STAGE_Z_MOVE,
            PLACE_DRY_STAGE_VAC_ON,
            CLEANER_PICKER_BLOW_ON,
            PLACE_DRY_STAGE_VAC_ON_CHECK,
            CLEANER_PICKER_READY_Z_PRE_MOVE,
            CLEANER_PICKER_READY_Z_MOVE,
            CLEANER_PICKER_BLOW_OFF,
            FINISH,
            #endregion
        }
        enum CYCLE_DRY_ULTRASONIC
        {
            #region 드라이 2차 초음파 테이블
            NONE = -1,
            CHECK_AND_INIT,
            MOVE_DRY_PLASMA_SAFETY_POS_Z_MOVE,
            MOVE_DRY_PLASMA_SAFETY_POS_Y_MOVE,
            MOVE_DRY_PLASMA_SAFETY_POS_X_MOVE,
            MOVE_DRY_START_READY_Z_POS,
            CLEANER_PICKER_SAFETY_X_POS,
            CLEANER_PICKER_TURN_POS,
            MOVE_FLIP_ELV_TURN_ENABLE_POS,
            MOVE_FLIP_ELV_TURN_DRY_POS,
            MOVE_FLIP_ELV_DRY_POS,

            MOVE_DRY_START_POS, //속도 따로 설정할 수 있게 변경
            MOVE_DRY_START_Z_POS,
   
            DRY_BLOW_ON,
            MOVE_DRY_END_POS,
            DRY_BLOW_OFF,
            CHECK_DRY_COUNT,
            CYL_UP_CHECK,
            MOVE_CLEANER_PICKER_DRY_SAFETY_POS_MOVE,
            MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE,
            MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE,
            DRY_UP_DOWN_SOL_CLEAR,
            MOVE_ULTRA_ELV_LOAD_POS,
            FINISH,
            #endregion
        }
        enum CYCLE_PLASMA_1
        {
            #region 플라즈마 1
            NONE = -1,
            ALARM_RESET_AND_ALARM_CHECK,
            MOVE_PLASMA_READY_Z_POS,
            MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE,
            MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE,
            MOVE_PLASMA_START_X_POS, //시퀀스 방식 좀 생각해봐야함.
            MOVE_PLASMA_START_Y_POS, //시퀀스 방식 좀 생각해봐야함.
            MOVE_PLASMA_START_Z_POS,
            TURN_ON_PLASMA,
            DELAY_PLASMA,
            MOVE_PLASMA_CHECK,
            //Y로 길게 움직이는 경우
            MOVE_PLASMA_X_MOVE,
            MOVE_PLASMA_Y_MOVE_CHECK,
            MOVE_PLASMA_Y_END_MOVE,
            MOVE_PLASMA_Y_START_MOVE, //Y로 길게 움직이는 경우
            //X로 길게 움직이는 경우
            MOVE_PLASMA_Y_MOVE,
            MOVE_PLASMA_X_MOVE_CHECK,
            MOVE_PLASMA_X_END_MOVE,
            MOVE_PLASMA_X_START_MOVE, //X로 길게 움직이는 경우

            TURN_OFF_PLASMA,
            RETURN_CHECK,
            MOVE_RETURN_Z_POS,
            MOVE_RETURN_Y_POS,
            MOVE_RETURN_X_POS,
            FINISH,
            #endregion
        }
        enum CYCLE_PLASMA_1_CONTINUOUS
        {
            #region 플라즈마 1
            NONE = -1,
            ALARM_RESET_AND_ALARM_CHECK,
            MOVE_PLASMA_READY_Z_POS,
            MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE,
            MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE,
            TURN_ON_PLASMA,
            MOVE_PLASMA_START_X_POS,
            MOVE_PLASMA_START_Y_POS, 
            MOVE_PLASMA_START_Z_POS,
            DELAY_PLASMA,
            MOVE_PLASMA_CHECK,
            CONTI_MOVE,
            PICKER_PLASMA_END,
            TURN_OFF_PLASMA,
            RETURN_CHECK,
            MOVE_RETURN_Z_POS,
            MOVE_RETURN_Y_POS,
            MOVE_RETURN_X_POS,
            FINISH,
            #endregion
        }
      
        enum CYCLE_PLASMA_2
        {
            #region 플라즈마 2
            NONE = -1,
            ALARM_RESET_AND_ALARM_CHECK,
            INIT_CYLINDER,
            MOVE_CLEANER_SAFETY_Z_POS,
            MOVE_CLEANER_PLASMA_X_POS,
            MOVE_CLEANER_PLASMA_T_POS,
            MOVE_CLEANER_PLASMA_Z_POS,
            MOVE_PLASMA_READY_Z_POS,
            MOVE_PLASMA_START_X_POS, //시퀀스 방식 좀 생각해봐야함.
            MOVE_PLASMA_START_Y_POS,
            MOVE_PLASMA_START_Z_POS,
            TURN_ON_PLASMA,
            DELAY_PLASMA,
            MOVE_PLASMA_CHECK,
            //Y로 길게 움직이는 경우
            MOVE_PLASMA_X_MOVE,
            MOVE_PLASMA_Y_MOVE_CHECK,
            MOVE_PLASMA_Y_END_MOVE,
            MOVE_PLASMA_Y_START_MOVE, //Y로 길게 움직이는 경우
            //X로 길게 움직이는 경우
            MOVE_PLASMA_Y_MOVE,
            MOVE_PLASMA_X_MOVE_CHECK,
            MOVE_PLASMA_X_END_MOVE,
            MOVE_PLASMA_X_START_MOVE, //X로 길게 움직이는 경우
            
            TURN_OFF_PLASMA,
            RETURN_CHECK,
            MOVE_RETURN_Z_POS,
            MOVE_RETURN_START_POS,
            FINISH,
            #endregion
        }
        enum CYCLE_PLASMA_2_CONTINUOUS
        {
            #region 플라즈마 2
            NONE = -1,
            INIT_CYLINDER,
            INTERLOCK_FLAG_CHECK,
            DRY_PICKER_Z_READY_MOVE,
            DRY_PICKER_Y_MOVE,
            DRY_PICKER_X_MOVE,
            CLEANER_PICKER_Z_SAFETY_MOVE,
            CLEANER_PICKER_X_PICKER_PLASMA_MOVE,
            CLEANER_PICKER_R_PICKER_PLASMA_MOVE,
            CLEANER_PICKER_Z_PLASMA_MOVE,
            PICKER_PLASMA_SHOT,
            DRY_PICKER_X_PICKER_PLASMA_MOVE,
            DRY_PICKER_Y_PICKER_PLASMA_MOVE,
            DRY_PICKER_Z_PICKER_PLASMA_MOVE,
            PICKER_PLASMA_WAIT,
            PICKER_PLASMA_START,
            PICKER_PLASMA_END,
            DRY_PICKER_SAFETY_Z_MOVE,
            DRY_PICKER_SAFETY_Y_MOVE,
            DRY_PICKER_SAFETY_X_MOVE,
            CL_PICKER_SAFETY_Z_MOVE,
            CL_PICKER_SAFETY_X_MOVE,
            CL_PICKER_SAFETY_R_MOVE,
            FINISH,
            #endregion
        }
        enum CYCLE_SORTER_UNLOAD
        {
            #region 언로드
            NONE = -1,
            //뒤집어야한다면 뒤집고 전달, 아니면 그냥 전달
            //MOVE_DRY_PICK_UP,
            //FLIP_AND_DRY_PLACE,
            CHECK_SORTER_SAFETY,
            UNLOAD_SORTER_MOVE,
            WAIT_UNLOAD_COMPLETE,
            FINISH,
            #endregion
        }

        #endregion

        public cycCleaning(int nSeqID)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_nCleanUltraDryNum = m_nSeqID - (int)SEQ_ID.CLEANING_FIRST;
            m_seqInfo = GbVar.Seq.sCleaning[m_nCleanUltraDryNum];

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        protected override void SetError(int nErrNo)
        {
            OnlyStopEvent(nErrNo);
        }

        #region INIT CYCLE
        public int InitCycle()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_nCurrentOrder = 0;
                    }
                    break;
                case 2:
                    {
                    }
                    break;
                case 20:
                    {
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                nFuncResult = SafetyMgr.Inst.GetAxisSafetyBeforeHome(m_nInitAxisNoArray[nCnt]);
                                if (FNC.IsErr(nFuncResult))
                                {
                                    return nFuncResult;
                                }
                            }
                        }
                    }
                    break;
                case 22:
                    {
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].HomeStart();
                            }
                        }
                    }
                    break;
                case 24:
                    {
                        if (FNC.IsBusy(RunLib.msecDelay(200)))
                            return FNC.BUSY;
                    }
                    break;
                case 26:
                    {
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                if (MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Fail)
                                {
                                    return (int)ERDF.E_SV_NOT_HOME + m_nInitAxisNoArray[nCnt];
                                }
                                else if (MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Process ||
                                         MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].IsBusy())
                                {
                                    return FNC.BUSY;
                                }
                            }
                        }
                    }
                    break;
                case 28:
                    {
                        m_nCurrentOrder++;

                        if (m_nCurrentOrder < 10)
                        {
                            NextSeq(20);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case 30:
                    {

                    }
                    break;
                case 40:
                    {
                        GbSeq.autoRun[SEQ_ID.ULD_ELV_REWORK].InitSeq();
                        SEQ_INFO_CURRENT.Init();

                        return FNC.SUCCESS;
                    }
                //break;
                default:
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region First Clean & Ultrasonic
        public int FirstInitMove()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_FIRST_INIT_MOVE)m_nSeqNo)
            {
                case CYCLE_FIRST_INIT_MOVE.NONE:
                    break;
                case CYCLE_FIRST_INIT_MOVE.BEFORE_MOVE_VACUUM_CHECK:
                    {
                        if (AirStatus(STRIP_MDL.UNIT_TRANSFER) != AIRSTATUS.NONE)
                        {
                            //배큠을 꺼야함
                            SetError((int)ERDF.E_INTL_THERE_IS_NO_UNIT_PK_STRIP_INFO);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case CYCLE_FIRST_INIT_MOVE.UNIT_PICKER_Z_READY_POS_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                    }
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_FIRST_INIT_MOVE.UNIT_PICKER_X_READY_POS_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosUnitPkX(POSDF.UNIT_PICKER_CLEAN_1)) break;
                    }
                    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_CLEAN_1);
                    break;
                case CYCLE_FIRST_INIT_MOVE.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
      
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int FirstInitAndWait()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_FIRST_INIT)m_nSeqNo)
            {
                case CYCLE_FIRST_INIT.NONE:
                    break;
                case CYCLE_FIRST_INIT.WARM_UP_WATER:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.WATER_FLOW_CHECK].bOptionUse == false) break;

                        if (GbVar.g_nFirstUltraWaterChangeCount == 0)
                        {
                            if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULTRASONIC_WATER_WARM_UP_TIME].nValue))
                            {
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                                // [2022.05.14.kmlee] 추가
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, true);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, true);

                                return FNC.BUSY;
                            }
                        }
                        
                        //else
                        //{
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                        //}
                    }
                    break;
                case CYCLE_FIRST_INIT.INIT:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
								 if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
	                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);
	                            	// [2022.05.14.kmlee] 추가
	                             if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
	                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
	                                return FNC.CYCLE_CHECK;
                            }
                        }

                        m_nFirstCleanAirCount = 0;
                        m_nFirstCleanWaterCount = 0;

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.WATER_FLOW_CHECK].bOptionUse == false) break;

                        if (GbVar.g_nFirstUltraWaterChangeCount == 0)
                        {
                            if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULTRASONIC_WATER_FILL_TIME].nValue))
                            {
                                // 물채우는 시간이 다 되면 물을 끔
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                                break;
                            }
                            else
                            {
                                //물 배출 종료
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);

                                return FNC.BUSY;
                            }
                        }
                            

                        #region 예전 소스 2

                        ////물 공급 중단
                        //if (GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_1] == 1 &&
                        //    GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_2] == 1 &&
                        //    GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_3] == 1)
                        //{
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);

                        //    // [2022.05.14.kmlee] 추가
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, false);

                        //    break;
                        //}
                        ////물 공급
                        //else
                        //{
                        //    //물을 공급해야 하는 경우는 센서가 하나라도 꺼지는 경우
                        //    if (GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_1] == 0 ||
                        //        GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_2] == 0 ||
                        //        GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_3] == 0)
                        //    {
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                        //        // [2022.05.14.kmlee] 추가
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, true);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                        //    }
                        //}
                        #endregion

                        #region 예전 소스 아까워서 남김
                        //if (GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_3] == 1)
                        //{
                        //    //물이 너무 많음
                        //    //WATER OUT 또는 에러
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 0)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, true);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 0)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, true);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 0)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, true);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 0)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, true);
                        //    return FNC.BUSY;
                        //}
                        //if (GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_1] == 1)
                        //{
                        //    //대기
                        //    if (GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_2] == 1)
                        //    {

                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                        //        //적정량 OK
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                        //        break;
                        //    }
                        //    else
                        //    {
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                        //        //감지 안되도 물부족
                        //        return FNC.BUSY;
                        //    }
                        //}
                        //else
                        //{
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                        //    //감지 안되도 물부족
                        //    return FNC.BUSY;
                        //}
                        //return FNC.BUSY;
                        #endregion

                        //물 배출에 대한 조건이 없음 위험함 //기술팀도 문제점 알고있음 (현동원프로님)
                       
                    }
                    break;
                case CYCLE_FIRST_INIT.ULTRASONIC_SET:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        //매뉴얼 모드 일때만 설정한다. [2022.05.12 작성자 홍은표]
                        if(m_bAutoMode == false)
                        {
                            //현재 출력과 현재 주파수 
                            if (GbDev.ultrasonic[0].Setting_Output != RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkOutput ||
                                GbDev.ultrasonic[1].Setting_Output != RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkOutput)
                            {
                                //레시피 값 대로 설정
                                GbDev.ultrasonic[0].SetOperationOutput(RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkOutput);
                                GbDev.ultrasonic[1].SetOperationOutput(RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkOutput);
                            }
                            int nValue = 1;
                            //레시피 값 대로 설정
                            switch (RecipeMgr.Inst.Rcp.cleaning.nFirstFreqChannelSetting)
                            {
                                case 40:
                                    nValue = 1;
                                    break;
                                case 80:
                                    nValue = 2;
                                    break;
                                case 120:
                                    nValue = 3;
                                    break;
                                default:
                                    //ERROR
                                    nValue = 1;
                                    break;
                            }
                            if (GbDev.ultrasonic[0].Setting_Freq_Channel != nValue ||
                                GbDev.ultrasonic[1].Setting_Freq_Channel != nValue)
                            {
                                GbDev.ultrasonic[0].SetFreqChannel(nValue);
                                GbDev.ultrasonic[1].SetFreqChannel(nValue);
                            }
                        }
                    }
                    break;
                case CYCLE_FIRST_INIT.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int FirstClean()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_FIRST_CLEAN)m_nSeqNo)
            {
                case CYCLE_FIRST_CLEAN.NONE:
                    break;
                case CYCLE_FIRST_CLEAN.INIT_CYL:
                    {
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.FIRST_CLEAN, true);
                        }
                        nFuncResult = FirstCleanerCylinder(false);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.INIT_MOVE:
                    {
                        //Z축만 안전위치로 보내면 됩니다 작성자 : 홍은표 22 04 28
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.MOVE_CLEAN_X_POS:
                    {
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_CLEAN_1);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.MOVE_CLEAN_Z_POS:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_CLEAN_1);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.CLEAN_OUTPUT_ON:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanWaterCount == 0) break;

                        nFuncResult = FirstCleanerWaterOn(100);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.WATER_JET_FORWARD:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanWaterCount == 0) break;
                        nFuncResult = FirstCleanerCylinder(true, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEAN_CYLINDER_DELAY].nValue);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.CLEAN_OUTPUT_OFF:
                    {
                        #region 사용 안함 
                        //작성자 : 홍은표 22.04.27
                        //if (m_bFirstSeqStep)
                        //{
                        //    m_bFirstSeqStep = false;
                        //    swTimeStamp.Restart();
                        //}
                        //if (swTimeStamp.ElapsedMilliseconds > 30000) return (int)ERDF.E_NONE; 
                        //if (GbVar.IO[IODF.INPUT.FIRST_CLEANING_ZONE_WJ_FORWARD_SENSOR] == 0) return FNC.BUSY;
                        //nFuncResult = FirstCleanerWaterAirOff(100);
                        #endregion

                        if (RecipeMgr.Inst.Rcp.cleaning.eFirstCleanWaterMode == WATER_MODE.ONE_WAY)
                        {
                            nFuncResult = FirstCleanerWaterOff(100);
                        }

                    }
                    break;
                case CYCLE_FIRST_CLEAN.WATER_JET_BACKWARD:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanWaterCount == 0) break;
                        nFuncResult = FirstCleanerCylinder(false, 100);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.CHECK_CLEAN_COUNT:
                    if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanWaterCount == 0) break;
                    m_nFirstCleanWaterCount++;
                    if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanWaterCount > m_nFirstCleanWaterCount)
                    {
                        NextSeq((int)CYCLE_FIRST_CLEAN.CLEAN_OUTPUT_ON);
                        return FNC.BUSY;
                    }
                    else
                    {
                        m_nFirstCleanWaterCount = 0;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANING_AIR_USE].bOptionUse == false)
                        {
                            FirstCleanerWaterOff(100);
                            NextSeq((int)CYCLE_FIRST_CLEAN.MOVE_CLEAN_READY_Z_POS);
                            return FNC.BUSY;
                        }
                        FirstCleanerWaterOff(100);
                        break;
                    }
                case CYCLE_FIRST_CLEAN.AIR_OUTPUT_ON:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanAirCount == 0) break;

                        //water Air 모드 나눠야함
                        nFuncResult = FirstCleanerAirOn(100);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.AIR_WATER_JET_FORWARD:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanAirCount == 0) break;
                        nFuncResult = FirstCleanerCylinder(true, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEAN_CYLINDER_DELAY].nValue);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.AIR_OUTPUT_OFF:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanAirCount == 0) break;
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            swTimeStamp.Restart();
                        }
                        if (swTimeStamp.ElapsedMilliseconds > 30000) return (int)ERDF.E_CLEANER_SWING_FWD_FAIL;
                        //if (GbVar.IO[IODF.INPUT.FIRST_CLEANING_ZONE_WJ_FORWARD_SENSOR] == 0) return FNC.BUSY;
                        //임시 처리 센서가 안들어와서
                        if (GbVar.IO[IODF.INPUT.FIRST_CLEANING_ZONE_WJ_BACKWARD_SENSOR] == 1) return FNC.BUSY;
                        nFuncResult = FirstCleanerAirOff(100);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.AIR_WATER_JET_BACKWARD:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanAirCount == 0) break;
                        nFuncResult = FirstCleanerCylinder(false, 100);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.AIR_CHECK_CLEAN_COUNT:
                    if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanAirCount == 0) break;
                    m_nFirstCleanAirCount++;
                    if (RecipeMgr.Inst.Rcp.cleaning.nFirstCleanAirCount > m_nFirstCleanAirCount)
                    {
                        NextSeq((int)CYCLE_FIRST_CLEAN.AIR_OUTPUT_ON);
                        return FNC.BUSY;
                    }
                    else
                    {
                        m_nFirstCleanAirCount = 0;
                        FirstCleanerWaterAirOff(100);
                        break;
                    }
                case CYCLE_FIRST_CLEAN.MOVE_CLEAN_READY_Z_POS:
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    }
                    break;
                case CYCLE_FIRST_CLEAN.FINISH:
                    nFuncResult = FirstCleanerWaterAirOff(100);
                    if(nFuncResult == FNC.SUCCESS)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.FIRST_CLEAN, false);

                        return FNC.SUCCESS;
                    }
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SpongeClean()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_nSpongeCount = 0;
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SPONGE_CLEAN, true);
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;
                case 6:
                    {
                        if (GbVar.IO[IODF.OUTPUT.FIRST_CLEANING_ZONE_SPONGE_SOL] == 0)
                        {
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_CLEANING_ZONE_SPONGE_SOL, true);
                        }
                    }
                    break;

                case 10:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 12:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nSpongeSwingCount <= 0)
                        {
                            // 스펀지 클리닝 종료
                            NextSeq(52);
                            return FNC.BUSY;
                        }
                    }
                    break;

                case 14:
                    // UNIT TRANSFER X AXIS SPONGE START POS MOVE
                    {
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_SWING_START);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 16:
                    // UNIT TRANSFER Z AXIS SPONGE START POS MOVE
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_SPONGE_SWING_START);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    // SPONGE COUNT CHECK
                    {
                        int nTotalCount = RecipeMgr.Inst.Rcp.cleaning.nSpongeSwingCount;
                        if (m_nSpongeCount >= nTotalCount)
                        {
                            // 스펀지 클리닝 종료
                            NextSeq(52);
                            return FNC.BUSY;

                        }
                        if (m_nSpongeCount > 0)
                        {
                            if (RecipeMgr.Inst.Rcp.cleaning.eSpongeMode == WATER_MODE.ONE_WAY)
                            {
                                //1회 이상인 경우 대기위치로 갔다가 X 이동
                                nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                            }
                            else
                            {
                                //따로 이동하지 않음
                            }
                                
                        }
                        // 스펀지 클리닝 해야함
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("UNIT PICKER SPONGE CLEAN START ({0}/{1})", m_nSpongeCount, nTotalCount), STEP_ELAPSED));
                        break;
                    }

                case 24:
                    // UNIT TRANSFER X AXIS SPONGE START POS MOVE
                    {
                        //double dSpongeSpeed = 40;
                        if (RecipeMgr.Inst.Rcp.cleaning.eSpongeMode == WATER_MODE.ONE_WAY)
                        {
                            nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_SWING_START);

                        }
                        else
                        {
                            double dSpongeSpeed = RecipeMgr.Inst.Rcp.cleaning.dSpongeSwingVel;
                            nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_SWING_START, true, dSpongeSpeed);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 25:
                    {
                        if (m_nSpongeCount > 0)
                        {
                            //1회 이상인 경우
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                            if (RecipeMgr.Inst.Rcp.cleaning.eSpongeMode == WATER_MODE.ONE_WAY)
                            {
                                nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_SPONGE_SWING_START);
                            }
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                    }
                    break;

                case 26:
                    // UNIT TRANSFER X AXIS SPONGE END POS MOVE
                    {
                        double dSpongeSpeed = RecipeMgr.Inst.Rcp.cleaning.dSpongeSwingVel;
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_SWING_END, true, dSpongeSpeed);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS SPONGE END POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 28:
                    // GO TO SPONGE COUNT CHECK
                    {
                        m_nSpongeCount++;

                        NextSeq(20);
                        return FNC.BUSY;
                    }

                case 52:
                    // UNIT TRANSFER Z AXIS UP POS MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 60:
                    {
                        //string strStripInfo = string.Format("{0}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.BRUSH_UNIT, false);
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SPONGE_CLEAN, false);

                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int FirstUltrasonic()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_FIRST_US)m_nSeqNo)
            {
                case CYCLE_FIRST_US.NONE:
                    break;
                case CYCLE_FIRST_US.INIT_READY_POS:
                    {
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.FIRST_ULTRASONIC, true);
                        }

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    }
                    break;
                case CYCLE_FIRST_US.CHECK_WATER_IN:
                    {
                        //m_nFirstCleanAirCount = 0;
                        //m_nFirstCleanWaterCount = 0;

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.WATER_FLOW_CHECK].bOptionUse == false) break;

                        //if (GbVar.g_nFirstUltraWaterChangeCount >= RecipeMgr.Inst.Rcp.cleaning.nFirstWaterChangeCount)
                        //{
                        //    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULTRASONIC_WATER_WARM_UP_TIME].nValue))
                        //    {
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                        //        // [2022.05.14.kmlee] 추가
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, true);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, true);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, true);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, true);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, true);

                        //        return FNC.BUSY;
                        //    }
                        //}
                        


                        //if (IsDelayOver(100000))
                        //{
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                        //    // [2022.05.14.kmlee] 추가
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 0)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);

                        //    SetError((int)ERDF.E_FIRST_ULTRA_WATER_IN_TIMEOUT);
                        //    return FNC.BUSY;
                        //}
                        ////물 공급 중단
                        //if (GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_1] == 1 &&
                        //    GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_2] == 1 &&
                        //    GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_3] == 1)
                        //{
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);

                        //    // [2022.05.14.kmlee] 추가
                        //    if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                        //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                        //    break;
                        //}
                        ////물 공급
                        //else
                        //{
                        //    //물을 공급해야 하는 경우는 센서가 하나라도 꺼지는 경우
                        //    if (GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_1] == 0 ||
                        //        GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_2] == 0 ||
                        //        GbVar.IO[IODF.INPUT.FIRST_ULTRASONIC_ZONE_WATER_LEVEL_SENSOR_3] == 0)
                        //    {
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                        //        // [2022.05.14.kmlee] 추가
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 0)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, true);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                        //    }
                        //}
                        //물 배출에 대한 조건이 없음 위험함 //기술팀도 문제점 알고있음 (현동원프로님)
                    }
                    break;
                case CYCLE_FIRST_US.CHECK_US_SETTING:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        //if (GbVar.g_nFirstUltraWaterChangeCount >= RecipeMgr.Inst.Rcp.cleaning.nFirstWaterChangeCount)
                        //{
                        //    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULTRASONIC_WATER_FILL_TIME].nValue))
                        //    {
                        //        // 물채우는 시간이 다 되면 물을 끔
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                        //        break;
                        //    }
                        //    else
                        //    {
                        //        //물 배출 종료
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                        //        if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                        //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);

                        //        return FNC.BUSY;
                        //    }
                        //}
                            
                        ////현재 출력과 현재 주파수 
                        //if (GbDev.ultrasonic[0].Setting_Output != RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkOutput ||
                        //    GbDev.ultrasonic[1].Setting_Output != RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkOutput)
                        //{
                        //    //레시피 값 대로 설정
                        //    GbDev.ultrasonic[0].SetOperationOutput(RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkOutput);
                        //    GbDev.ultrasonic[1].SetOperationOutput(RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkOutput);
                        //}
                        //    int nValue = 1;
                        //    //레시피 값 대로 설정
                        //    switch (RecipeMgr.Inst.Rcp.cleaning.nFirstFreqChannelSetting)
                        //    {
                        //        case 40:
                        //            nValue = 1;
                        //            break;
                        //        case 80:
                        //            nValue = 2;
                        //            break;
                        //        case 120:
                        //            nValue = 3;
                        //            break;
                        //        default:
                        //            //ERROR
                        //            nValue = 1;
                        //            break;
                        //    }
                        //if (GbDev.ultrasonic[0].Setting_Freq_Channel != nValue ||
                        //    GbDev.ultrasonic[1].Setting_Freq_Channel != nValue)
                        //{
                        //    GbDev.ultrasonic[0].SetFreqChannel(nValue);
                        //    GbDev.ultrasonic[1].SetFreqChannel(nValue);
                        //}
                    }
                    break;
                case CYCLE_FIRST_US.MOVE_US_X_POS:
                    {
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_US_1);
                    }
                    break;
                case CYCLE_FIRST_US.MOVE_US_Z_POS:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_US_1);
                    }
                    break;
                case CYCLE_FIRST_US.US_START:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ULTRASONIC_1_RUN, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ULTRASONIC_2_RUN, true);
                        m_swFirstUltra.Restart();
                    }
                    break;
                case CYCLE_FIRST_US.MOVE_LEFT:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.UNIT_PK_X].dPos[POSDF.UNIT_PICKER_US_1] - RecipeMgr.Inst.Rcp.cleaning.dFirstUltraSwingOffset;
                        nFuncResult = AxisMovePos((int)SVDF.AXES.UNIT_PK_X, dPos, RecipeMgr.Inst.Rcp.cleaning.dFirstUltraSwingVel, 0);
                    }
                    break;
                case CYCLE_FIRST_US.MOVE_RIGHT:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.UNIT_PK_X].dPos[POSDF.UNIT_PICKER_US_1] + RecipeMgr.Inst.Rcp.cleaning.dFirstUltraSwingOffset;
                        nFuncResult = AxisMovePos((int)SVDF.AXES.UNIT_PK_X, dPos, RecipeMgr.Inst.Rcp.cleaning.dFirstUltraSwingVel, 0);
                    }
                    break;
                case CYCLE_FIRST_US.MOVE_CHECK_CENTER:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (m_swFirstUltra.ElapsedMilliseconds < RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkTime)
                        {
                            NextSeq((int)CYCLE_FIRST_US.MOVE_LEFT);
                            return FNC.BUSY;
                        }
                        //센터로 안보내도 안전함 220503 작성자 : 홍은표
                        //nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_US_1);
                    }
                    break;
                case CYCLE_FIRST_US.US_END:
                    {
                        //if (m_bFirstSeqStep)
                        //{
                        //    m_bFirstSeqStep = false;
                        //    swTimeStamp.Restart();
                        //}
                        //if (swTimeStamp.ElapsedMilliseconds < RecipeMgr.Inst.Rcp.cleaning.nFirstUltrasonicWorkTime) return FNC.BUSY;
                        //초음파 카운트 증가
                        if(m_bAutoMode)
                        {
                            GbVar.g_nFirstUltraWaterChangeCount++;
                            if (GbVar.g_nFirstUltraWaterChangeCount >= RecipeMgr.Inst.Rcp.cleaning.nFirstWaterChangeCount)
                            {
                                GbVar.g_nFirstUltraWaterChangeCount = 0;

                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                                // [2022.05.14.kmlee] 추가
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, true);
                            }
                        }

                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ULTRASONIC_1_RUN, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ULTRASONIC_2_RUN, false);
                        
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    }
                    break;
                case CYCLE_FIRST_US.MOVE_US_READY_Z_POS:
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    }
                    break;
                case CYCLE_FIRST_US.FINISH:
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.FIRST_ULTRASONIC, false);

                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int FirstUnload()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_FIRST_UNLOAD)m_nSeqNo)
            {
                case CYCLE_FIRST_UNLOAD.NONE:
                    break;
                case CYCLE_FIRST_UNLOAD.WAIT_SECOND_CLEAN_FINISH:
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.FIRST_UNLOAD, true);
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.BEFORE_MOVE_CHECK_STRIP:
                    {
                        //오토런
                        if (m_bAutoMode)
                        {
                            // 이미 데이터가 넘어 갔거나 스텝 잘못 들어온 경우를 대비 하여
                            if ( GbVar.Seq.sCleaning[0].Info.IsStrip() &&
                                !GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                // 배큠이 정상적으로 동작되어 있으면 다음 스텝으로
                                if (GbFunc.IsAVacOn((int)IODF.A_INPUT.SECOND_CLEANING_ZONE_KIT_MATERIAL_VACUUM_SENSOR))
                                {
                                    NextSeq((int)CYCLE_FIRST_UNLOAD.MOVE_READY_Z_MOVE);
                                    return FNC.BUSY;
                                }
                                else
                                {
                                    nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_UNIT_PK_STRIP_INFO;
                                }
                            }
                            // 알람 발생 후 해당 위치에서 다시 배큠 체크 하기 위한 작업 
                            // 유닛피커 스트립 정보가 있음
                            if (GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                //유닛 피커 위치가 내려 놓는 위치임
                                if (IsInPosUnitPkXZ(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD))
                                {
                                    NextSeq((int)CYCLE_FIRST_UNLOAD.SECOND_CLEAN_ELV_VACUUM_ON);
                                    return FNC.BUSY;
                                }
                            }
                        }
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.MOVE_UNIT_PK_Z_READY_POS:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY) ||
                               (IsInPosUnitPkZ(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD) && 
                                IsInPosUnitPkX(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD))
                               ) break;
                        }
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.CHECK_STRIP_INFO:
                    {
                        //오토런
                        if (m_bAutoMode)
                        {
                            //------- 스크랩 처리 --------//
                            // 유닛피커에 자재가 없으면
                            if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                // OUTPUT이 정상적으로 꺼져있으면 종료
                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.NONE)
                                {
                                    return FNC.SUCCESS;
                                }
                                else
                                {
                                    nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_UNIT_PK_STRIP_INFO;
                                }
                            }
                        }   
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.AIR_BLOW_ON:
                    {
                        /// FIRST_ULTRA_TO_SECOND_CLEAN_AIR_BLOW가 2차세척 에어로 바뀌었다. 2022. 05 .29 작성자 홍은표
                        // [2022.05.14.kmlee] 추가
                        //MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRA_TO_SECOND_CLEAN_AIR_BLOW, true);
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.MOVE_CLEAN_LOAD_POS:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD)) break;
                        }
                        nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD);
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.MOVE_UNLOAD_X_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkX(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD)) break;
                        }
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD);
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.AIR_BLOW_OFF:
                    {
                        /// FIRST_ULTRA_TO_SECOND_CLEAN_AIR_BLOW가 2차세척 에어로 바뀌었다. 2022. 05 .29 작성자 홍은표
                        // [2022.05.14.kmlee] 추가
                        //MotionMgr.Inst.SetOutput(IODF.OUTPUT.FIRST_ULTRA_TO_SECOND_CLEAN_AIR_BLOW, false);
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.MOVE_UNLOAD_Z_PRE_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD)) break;
                        }
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD, -dOffset);
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.MOVE_UNLOAD_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD)) break;
                    }
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD, 0.0, true);
                    break;
                case CYCLE_FIRST_UNLOAD.SECOND_CLEAN_ELV_VACUUM_ON:
                    nFuncResult = SecondCleanVacuumOn(false);
                    break;
                case CYCLE_FIRST_UNLOAD.UNIT_PICKER_BLOW_ON:
                    nFuncResult = UnitPickerWorkBlow(true, 0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_FIRST_UNLOAD.SECOND_CLEAN_ELV_VACUUM_ON_CHECK:
                    nFuncResult = SecondCleanVacuumOn(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    //알람이 발생하는 경우
                    break;
                case CYCLE_FIRST_UNLOAD.MOVE_READY_Z_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_CLEAN_2_UNLOAD, -dOffset, true);
                    }
                    break;
                case CYCLE_FIRST_UNLOAD.MOVE_READY_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_FIRST_UNLOAD.UNIT_PICKER_BLOW_OFF:
                    nFuncResult = UnitPickerWorkBlow(false,0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_FIRST_UNLOAD.MOVE_READY_X_MOVE:
                    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_CLEAN_1);
                    break;
                case CYCLE_FIRST_UNLOAD.COMPLETE_FLAG_ON:
                    break;
                case CYCLE_FIRST_UNLOAD.FINISH:
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.FIRST_UNLOAD, false);

                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int UnitPickerScrap1()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;
            
            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SCRAP)m_nSeqNo)
            {
                case CYCLE_SCRAP.NONE:
                    break;
                case CYCLE_SCRAP.INIT:
                    if (RecipeMgr.Inst.Rcp.cleaning.nScrapCount1 == 0)
                    {
                        NextSeq((int)CYCLE_SCRAP.FINISH);
                        return FNC.BUSY;
                    }
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SCRAP_1, true);
                    }
                    m_nScrap1Count = 0;
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_SCRAP_X_MOVE:
                    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SCRAP_1);
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_SCRAP_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_SCRAP_1);
                    break;
                case CYCLE_SCRAP.SCRAP_BLOW_ON:
                    nFuncResult = UnitPickerScrap1Blow(true, false, RecipeMgr.Inst.Rcp.cleaning.nScrap1Delay);
                    break;
                case CYCLE_SCRAP.WAIT_DELAY:
					//블로우 딜레이로 조절
                    //if (WaitDelay(500)) return FNC.BUSY;
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_READY_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_SCRAP.SCRAP_BLOW_OFF:
                    nFuncResult = UnitPickerScrap1Blow(false);
                    break;
                case CYCLE_SCRAP.CHECK_COUNT:
                    if (m_nScrap1Count < RecipeMgr.Inst.Rcp.cleaning.nScrapCount1 -1) //0502 1132 KTH
                    {
                        m_nScrap1Count++;
                        NextSeq((int)CYCLE_SCRAP.UNIT_PICKER_SCRAP_Z_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_SCRAP.FINISH:
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SCRAP_1, false);
                    }
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int UnitPickerScrap2()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SCRAP)m_nSeqNo)
            {
                case CYCLE_SCRAP.NONE:
                    break;
                case CYCLE_SCRAP.INIT:
                    if(RecipeMgr.Inst.Rcp.cleaning.nScrapCount2 == 0)
                    {
                        NextSeq((int)CYCLE_SCRAP.FINISH);
                        return FNC.BUSY;
                    }
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SCRAP_2, true);
                    }
                    m_nScrap2Count = 0;
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_SCRAP_X_MOVE:
                    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SCRAP_2);
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_SCRAP_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_SCRAP_2);
                    break;
                case CYCLE_SCRAP.SCRAP_BLOW_ON:
                    nFuncResult = UnitPickerScrap2Blow(true, false, RecipeMgr.Inst.Rcp.cleaning.nScrap2Delay);
                    break;
                case CYCLE_SCRAP.WAIT_DELAY:
					//블로우 딜레이로 조절
                    //if (WaitDelay(2000)) return FNC.BUSY;
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_READY_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_SCRAP.SCRAP_BLOW_OFF:
                    nFuncResult = UnitPickerScrap2Blow(false);
                    break;
                case CYCLE_SCRAP.CHECK_COUNT:
                    if (m_nScrap2Count < RecipeMgr.Inst.Rcp.cleaning.nScrapCount2 -1) //0502 1132 KTH
                    {
                        m_nScrap2Count++;
                        NextSeq((int)CYCLE_SCRAP.UNIT_PICKER_SCRAP_Z_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_SCRAP.FINISH:
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SCRAP_2, false);
                    }
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        #endregion

        #region Second Clean & Ultrasonic
        public int SecondVacCheck()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SECOND_VAC_CHECK)m_nSeqNo)
            {
                case CYCLE_SECOND_VAC_CHECK.NONE:
                    break;

                case CYCLE_SECOND_VAC_CHECK.BEFORE_MOVE_VACUUM_CHECK:
                    {
                        if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                        {
                            if (AirStatus(STRIP_MDL.SECOND_CLEAN_ZONE) != AIRSTATUS.NONE)
                            {
                                //배큠을 꺼야함
                                SetError((int)ERDF.E_INTL_THERE_IS_NO_SECOND_CLEAN_STRIP_INFO);
                                return FNC.BUSY;
                            }
                        }
                        if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                        {
                            if (AirStatus(STRIP_MDL.SECOND_ULTRA_ZONE) != AIRSTATUS.NONE)
                            {
                                //배큠을 꺼야함
                                SetError((int)ERDF.E_INTL_THERE_IS_NO_SECOND_ULTRA_STRIP_INFO);
                                return FNC.BUSY;
                            }
                        }
                            
                    }
                    break;

                #region 2차 구역은 클리너피커 움직일 필요 없음 왜냐하면 배큠 플레이스 구간에서만 클리너피커 간섭함
                //case CYCLE_SECOND_INIT_MOVE.CLEANER_PICKER_Z_READY_POS_MOVE:
                //    if (m_bFirstSeqStep)
                //    {
                //        if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                //    }
                //    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                //    break;
                //case CYCLE_SECOND_INIT_MOVE.CLEANER_PICKER_X_READY_POS_MOVE:
                //    if (m_bFirstSeqStep)
                //    {
                //        if (IsInPosUnitPkX(POSDF.UNIT_PICKER_CLEAN_1)) break;
                //    }
                //    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_CLEAN_1);
                //    break;
                #endregion

                case CYCLE_SECOND_VAC_CHECK.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondInitAndWait()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SECOND_INIT)m_nSeqNo)
            {
                case CYCLE_SECOND_INIT.NONE:
                    break;
                case CYCLE_SECOND_INIT.WARM_UP_WATER:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.WATER_FLOW_CHECK].bOptionUse == false) break;

                        if (GbVar.g_nSecondUltraWaterChangeCount == 0)
                        {
                            if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULTRASONIC_WATER_WARM_UP_TIME].nValue))
                            {
                                if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                                // [2022.05.14.kmlee] 추가
                                if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2, true);
                                if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, true);
                                if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 0)
                                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, true);

                                return FNC.BUSY;
                            }
                        }
                        
                    }
                    break;
                case CYCLE_SECOND_INIT.INIT:
                    //초기화
                    //현재 출력과 현재 주파수 
                    //매뉴얼 모드 일때만 설정한다. [2022.05.12 작성자 홍은표]
                    if (m_bAutoMode == false)
                    {
                        if (GbDev.ultrasonic[2].Setting_Output != RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkOutput ||
                            GbDev.ultrasonic[3].Setting_Output != RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkOutput)
                        {
                            //레시피 값 대로 설정
                            GbDev.ultrasonic[2].SetOperationOutput(RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkOutput);
                            GbDev.ultrasonic[3].SetOperationOutput(RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkOutput);
                        }
                        int nValue = 1;
                        switch (RecipeMgr.Inst.Rcp.cleaning.nSecondFreqChannelSetting)
                        {
                            case 40:
                                nValue = 1;
                                break;
                            case 80:
                                nValue = 2;
                                break;
                            case 120:
                                nValue = 3;
                                break;
                            default:
                                //ERROR
                                nValue = 1;
                                break;
                        }
                        if (GbDev.ultrasonic[2].Setting_Freq_Channel != nValue ||
                            GbDev.ultrasonic[3].Setting_Freq_Channel != nValue)
                        {
                            GbDev.ultrasonic[2].SetFreqChannel(nValue);
                            GbDev.ultrasonic[3].SetFreqChannel(nValue);
                        }
                    }
                    break;
                case CYCLE_SECOND_INIT.WAIT_SECOND_ULTRA_WATER_FLOW:
                    if (m_bAutoMode)
                    {
                        if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                        {
                            if(GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);

                            // [2022.05.14.kmlee] 추가
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2, false);

                            return FNC.CYCLE_CHECK;
                        }
                    }
                    if (GbVar.g_nSecondUltraWaterChangeCount == 0)
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULTRASONIC_WATER_FILL_TIME].nValue))
                        {
                            // 물채우는 시간이 다 되면 물을 끔
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                            break;
                        }
                        else
                        {
                            //물 배출 종료
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);

                            return FNC.BUSY;
                        }
                    }
                        

                    // 시간으로 물 배출로 변경 됨. 이것도 안전하다고 볼수는 없음
                    #region [ 예전 소스 2 ] 2022.05.29 
                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.WATER_FLOW_CHECK].bOptionUse == false) break;
                    ////물 공급 중단
                    //if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_1] == 1 &&
                    //    GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_2] == 1 &&
                    //    GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_3] == 1)
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                    //    break;
                    //}
                    ////물 공급
                    //else
                    //{
                    //    //물을 공급해야 하는 경우는 센서가 하나라도 꺼지는 경우
                    //    if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_1] == 0 ||
                    //        GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_2] == 0 ||
                    //        GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_3] == 0)
                    //    {
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                    //    }
                    //    return FNC.BUSY;
                    //}

                    #endregion

                    // 물 배출에 대한 조건이 없음 위험함 //기술팀도 문제점 알고있음 (현동원프로님)
                    #region 예전 소스
                    //if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_3] == 1)
                    //{
                    //    //물이 너무 많음
                    //    //WATER OUT 또는 에러
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, true);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, true);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, true);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, true);
                    //    return FNC.BUSY;
                    //}

                    //if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_1] == 1)
                    //{
                    //    //WATER IN
                    //    if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_2] == 1)
                    //    {
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                    //        //적정량 OK
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                    //        //감지 되도 2번 안들어오면 물부족
                    //        return FNC.BUSY;
                    //    }
                    //}
                    //else
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                    //    //감지 안되도 물부족
                    //    return FNC.BUSY;
                    //}
                    #endregion

                    break;
                case CYCLE_SECOND_INIT.WAIT_LOADING_COMPLETE:
                    //플래그 대기 완료
                    //if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_WATER_CLEANING_UNLOAD]) return FNC.BUSY;
                    //GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING_COMPLETE] = false;
                    break;
                case CYCLE_SECOND_INIT.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondClean()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            
            switch ((CYCLE_SECOND_CLEAN)m_nSeqNo)
            {
                case CYCLE_SECOND_CLEAN.NONE:
                    break;
                case CYCLE_SECOND_CLEAN.CYL_INIT:
                    {

                        if(m_bAutoMode)
                        {
                            if(!GbVar.mcState.IsCycleRunReq(SEQ_ID.CLEANING_SECOND))
                            {
                                return FNC.CYCLE_CHECK;
                            }

                            #region 스트립 체크 
                            if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_BACKWARD_SOL, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_FORWARD_SOL, false);

                                NextSeq((int)CYCLE_SECOND_CLEAN.AIR_WATER_JET_BACKWARD);
                                return FNC.BUSY;
                            }
                            #endregion
                        }
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_CLEAN, true);
                        }
                        //if (GbVar.IO[IODF.INPUT.SECOND_CLEANING_ZONE_COVER_CLOSE_SENSOR_1] == 1)
                        //{
                        //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEANING_ZONE_COVER_CLOSE_SOL, false);
                        //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEANING_ZONE_COVER_OPEN_SOL, true);
                        //    return FNC.BUSY;
                        //}
                        if (GbVar.IO[IODF.INPUT.SECOND_CLEANING_ZONE_WJ_FORWARD_SENSOR] == 1)
                        {
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_BACKWARD_SOL, true);
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_FORWARD_SOL, false);
                            return FNC.BUSY;
                        }
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return (int)ERDF.E_CLEANER_SWING_BWD_FAIL;
                    }
                    break;
                case CYCLE_SECOND_CLEAN.CLEAN_FLIP:
                    #region 스트립 체크 
                    if(m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                        {
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_BACKWARD_SOL, true);
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_FORWARD_SOL, false);

                            NextSeq((int)CYCLE_SECOND_CLEAN.AIR_WATER_JET_BACKWARD);
                            return FNC.BUSY;
                        }
                    }
                    #endregion
                    //if (GbVar.IO[IODF.INPUT.SECOND_CLEANING_ZONE_COVER_CLOSE_SENSOR_1] == 1)
                    //{
                    //    return (int)ERDF.E_INTL_CLEANER_PICKER_Z_CAN_NOT_MOVE_CLEANER_PICKER_X_NOT_SECOND_CLEAN_POS;
                    //}
                    nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_CLEAN_WORK);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        if (m_bAutoMode)
                        {
                            //2차세척 인터락 발생 클리어 (T축 다 돌았으면 인터락 해제) - 2022 05 29 작성자 : 홍은표 _ 테스트중.
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] == true)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] = false;
                        }
                    }
                    break;
                case CYCLE_SECOND_CLEAN.AFTER_FLIP_COVER_CLOSE:
                    //nFuncResult = SecondCleanerCoverCylinder(false);
                    break;
                case CYCLE_SECOND_CLEAN.AIR_CURTAIN_ON:
                    //커버 온 전에 해야함
                    nFuncResult = SecondCleanerAirCurtainOn();
                    break;
                case CYCLE_SECOND_CLEAN.CLEAN_OUTPUT_ON:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanWaterCount == 0) break;

                    nFuncResult = SecondCleanerWaterOn(50);
                    break;
                case CYCLE_SECOND_CLEAN.WATER_JET_FORWARD:
                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanWaterCount == 0) break;

                    nFuncResult = SecondCleanerCylinder(true);
                    break;
                case CYCLE_SECOND_CLEAN.CLEAN_OUTPUT_OFF:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanWaterCount == 0) break;

                    #region 사용안함
                    //작성자 : 홍은표 22.04.27
                    //nFuncResult = SecondCleanerWaterAirOff();
                    #endregion

                    if (RecipeMgr.Inst.Rcp.cleaning.eSecondCleanWaterMode == WATER_MODE.ONE_WAY)
                    {
                        nFuncResult = SecondCleanerWaterOff(50);
                    }
                    break;
                case CYCLE_SECOND_CLEAN.WATER_JET_BACKWARD:
                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanWaterCount == 0) break;

                    nFuncResult = SecondCleanerCylinder(false);
                    break;
                case CYCLE_SECOND_CLEAN.CHECK_CLEAN_COUNT:
                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanWaterCount == 0) break;

                    m_nSecondCleanWaterCount++;

                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanWaterCount > m_nSecondCleanWaterCount)
                    {
                        NextSeq((int)CYCLE_SECOND_CLEAN.CLEAN_OUTPUT_ON);
                        return FNC.BUSY;
                    }
                    else
                    {
                        m_nSecondCleanWaterCount = 0;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANING_AIR_USE].bOptionUse == false)
                        {
                            SecondCleanerWaterOff();
                            NextSeq((int)CYCLE_SECOND_CLEAN.AFTER_FLIP_COVER_OPEN);
                            return FNC.BUSY;
                        }
                        SecondCleanerWaterAirOff();
                        break;
                    }
                    break;
                case CYCLE_SECOND_CLEAN.AIR_OUTPUT_ON:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    nFuncResult = SecondCleanerAirOn();
                    break;
                case CYCLE_SECOND_CLEAN.AIR_WATER_JET_FORWARD:
                    nFuncResult = SecondCleanerCylinder(true);
                    break;
                case CYCLE_SECOND_CLEAN.AIR_OUTPUT_OFF:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    nFuncResult = SecondCleanerAirOff();
                    break;
                case CYCLE_SECOND_CLEAN.AIR_WATER_JET_BACKWARD:
                    nFuncResult = SecondCleanerCylinder(false);
                    break;
                case CYCLE_SECOND_CLEAN.AIR_CHECK_CLEAN_COUNT:
                    m_nSecondCleanAirCount++;

                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanAirCount> m_nSecondCleanAirCount)
                    {
                        NextSeq((int)CYCLE_SECOND_CLEAN.AIR_OUTPUT_ON);
                        return FNC.BUSY;
                    }
                    else
                    {
                        m_nSecondCleanAirCount = 0;
                        break;
                    }
                    break;
                case CYCLE_SECOND_CLEAN.AFTER_FLIP_COVER_OPEN:
                    //nFuncResult = SecondCleanerCoverCylinder(true);
                    SecondCleanerWaterAirOff();
                    break;
                case CYCLE_SECOND_CLEAN.COVER_OPEN_DELAY:
                    if (WaitDelay(500)) return FNC.BUSY;
                    if (m_bAutoMode)
                    {
                        if (GbVar.Seq.sCleaning[1].Info.IsStrip()) return FNC.BUSY;
                    }
                    break;
                case CYCLE_SECOND_CLEAN.UNLOAD_FLIP:
                    if (m_bFirstSeqStep)
                    {
                        if (m_bAutoMode)
                        {
                            //2차세척 인터락 발생 (T축 돌기전 인터락 설정) - 2022 05 29 작성자 : 홍은표 _ 테스트중.
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] == false)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] = true;
                        }
                    }
                    nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        if (m_bAutoMode)
                        {
                            //2차세척 인터락 발생 클리어 (T축 다 돌았으면 인터락 해제) - 2022 05 29 작성자 : 홍은표 _ 테스트중.
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] == true)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] = false;
                        }
                    }
                    break;
                case CYCLE_SECOND_CLEAN.FINISH:
                    SecondCleanerAirCurtainOff();
                    SecondCleanerWaterAirOff();
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_CLEAN, false);
                    return FNC.SUCCESS;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondCleanPickUp()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            // [2022.05.12.kmlee] 추가되어야 함
            nFuncResult = FNC.SUCCESS;

            switch ((CYCLE_SECOND_PICK_AND_PLACE)m_nSeqNo)
            {
                case CYCLE_SECOND_PICK_AND_PLACE.NONE:
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.BEFORE_MOVE_CHECK_STRIP_INFO:
                    {
                        if (m_bAutoMode)
                        {
                            if ( GbVar.Seq.sCleaning[2].Info.IsStrip() &&  //클리너 피커에는 정보가 있고
                                !GbVar.Seq.sCleaning[0].Info.IsStrip())    //2차 세척에는 정보가 없으면 이미 떴다고 판단
                            {
                                if (AirStatus(STRIP_MDL.CLEANER_PICKER) == AIRSTATUS.VAC)
                                {
                                    // 시퀀스 번호  변경 시 주의
                                    NextSeq((int)CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE);
                                    return FNC.BUSY;
                                }
                                else
                                {
                                    nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_CLEANER_PK_STRIP_INFO;
                                }
                            }
                            // 알람 발생 후 해당 위치에서 다시 배큠 체크 하기 위한 작업 
                            // 2차 클린 테이블에 스트립 정보가 있음
                            if (GbVar.Seq.sCleaning[0].Info.IsStrip())
                            {
                                //클리너 피커 위치가 픽업하는 위치임
                                if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                                    IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN))
                                {
                                    NextSeq((int)CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON);
                                    return FNC.BUSY;
                                }
                            }
                        }
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_CLEAN_PICK_UP, true);
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_SAFETY_POS_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_READY)) break;
                        
                        if ((
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING) ||
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_END_ON_CLEANING)) ||
                            (
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                            IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                            IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                            IsInPosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD))) 
                            break;
                    }

                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_SAFETY_POS_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN) ||
                            IsInPosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CHECK_STRIP_INFO:
                    {
                        //오토런
                        if (m_bAutoMode)
                        {
                            //------- 스크랩 처리 --------//
                            // 2차 세척존에 자재가 없으면
                            if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                            {
                                //OUTPUT이 정상적으로 꺼져있으면 종료
                                if (AirStatus(STRIP_MDL.SECOND_CLEAN_ZONE) == AIRSTATUS.NONE)
                                {
                                    NextSeq((int)CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE);
                                    return FNC.BUSY;
                                }
                                else
                                {
                                    nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_SECOND_CLEAN_STRIP_INFO;
                                }
                            }
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_PICK_UP_T_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                    }
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN, 100);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_FLIP_UNLOAD_POS_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD)) break;
                    }
                    nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_Z_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (m_bFirstSeqStep)
                        {
                            //이미 내려갔으면 슬로우다운 안해도 됨
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                        }
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            m_strMotPos = String.Format("MOVE DONE");
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_Z_MOVE:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, 0, true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON:
                    //nFuncResult = CleanerPickerLoadSecondClean(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    nFuncResult = CleanerPickerVacuumOn(false);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_ELV_BLOW_ON:
                    nFuncResult = SecondCleanBlowOn(false);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON_CHECK:
                    //nFuncResult = CleanerPickerLoadSecondClean(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    nFuncResult = CleanerPickerVacuumOn(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    
                    //// 재시도 조건
                    //if (true)
                    //{
                    //    NextSeq((int)CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON);
                    //    return FNC.BUSY;
                    //}
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, -dOffset, true);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_ELV_BLOW_OFF:
                    nFuncResult = SecondCleanBlowOff(false);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_SAFETY_X_MOVE:
                    {
                        //------- 스크랩 처리 --------//
                        // 2차 세척존에 자재가 없으면
                        if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.FINISH:
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_CLEAN_PICK_UP, false);

                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondUltraPlace()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SECOND_PICK_AND_PLACE)m_nSeqNo)
            {
                case CYCLE_SECOND_PICK_AND_PLACE.NONE:
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.BEFORE_MOVE_CHECK_STRIP_INFO:
                    {
                        if (GbVar.Seq.sCleaning[1].Info.IsStrip() && // 2차 초음파에 자재가 있고
                           !GbVar.Seq.sCleaning[2].Info.IsStrip())   // 클리너 피커에 자재가 없으면
                        {
                            if (AirStatus(STRIP_MDL.SECOND_ULTRA_ZONE) == AIRSTATUS.VAC)
                            {
                                // 시퀀스 번호  변경 시 주의
                                NextSeq((int)CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE_2);
                                return FNC.BUSY;
                            }
                            else
                            {
                                nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_SECOND_ULTRA_STRIP_INFO;
                            }
                        }
                        // 알람 발생 후 해당 위치에서 다시 배큠 체크 하기 위한 작업 
                        // 클리너 스트립 정보가 있음
                        if (GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            //유닛 피커 위치가 내려 놓는 위치임
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) &&
                                IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC))
                            {
                                NextSeq((int)CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON);
                                return FNC.BUSY;
                            }
                        }
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_ULTRASONIC_PLACE, true);
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_READY) ||
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM)) break;
                    }

                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CHECK_STRIP_INFO_PLACE:
                    {
                        //오토런
                        if (m_bAutoMode)
                        {
                            //---- 스크랩 처리 --------//
                            // 클리너 피커에 자재가 없으면
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                //OUTPUT이 정상적으로 꺼져있으면 종료
                                if (AirStatus(STRIP_MDL.CLEANER_PICKER) == AIRSTATUS.NONE)
                                {
                                    NextSeq((int)CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE_2);
                                    return FNC.BUSY;
                                }
                                else
                                {
                                    nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_CLEANER_PK_STRIP_INFO;
                                }
                            }
                        }
                    }
                    break;

                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_ELV_T_LOAD_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosFlip2(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD)) break;
                        }
                        nFuncResult = MovePosFlip2(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.AIR_KNIFE_ON:
                    {
						// [2022.05.14.kmlee] 추가
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEAN_TO_SECOND_ULTRA_AIR_KNIFE, true);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_PLACE_POS_X_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                        }
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.AIR_KNIFE_OFF:
                    {
						// [2022.05.14.kmlee] 추가
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_CLEAN_TO_SECOND_ULTRA_AIR_KNIFE, false);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_PLACE_POS_Z_PRE_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                        }
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, -dOffset);
                    }
                    break;

                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_PLACE_POS_Z_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                        }
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, 0, true);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_ELV_VAC_ON:
                    nFuncResult = SecondUltrasonicVacuumOn(false);
                    //nFuncResult = CleanerPickerUnloadSecondUltrasonic(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_BLOW_ON:
                    nFuncResult = CleanerPickerBlowOn(false, 10);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_ELV_VAC_ON_CHECK:
                    nFuncResult = SecondUltrasonicVacuumOn(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_PRE_MOVE_2:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, -dOffset, true);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE_2:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_BLOW_OFF:
                    //220504 Blow Off 제거
                    //nFuncResult = CleanerPickerBlowOff(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_SAFETY_X_MOVE:
                    if (m_bAutoMode)
                    {
                        //---- 스크랩 처리 --------//
                        // 클리너 피커에 자재가 없으면
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                        }
                        else
                        {
                            //정상 동작
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                        }
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            m_strMotPos = String.Format("MOVE DONE");
                        }
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            m_strMotPos = String.Format("MOVE DONE");
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.FINISH:
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_ULTRASONIC_PLACE, false);

                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondPickAndPlace()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SECOND_PICK_AND_PLACE)m_nSeqNo)
            {
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_SAFETY_POS_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_READY)) break;
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                        if ((IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING) ||
                            (IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_END_ON_CLEANING))) &&
                            IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                            IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                            IsInPosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD)) break;
                    }

                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_SAFETY_POS_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN) ||
                            IsInPosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_PICK_UP_T_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                    }
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN, 100);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_FLIP_UNLOAD_POS_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD)) break;
                    }
                    nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL, true);
                    //}
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN);
                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL, false);
                    //}
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_Z_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (m_bFirstSeqStep)
                        {
                            //이미 내려갔으면 슬로우다운 안해도 됨
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                        }
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            m_strMotPos = String.Format("MOVE DONE");
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_Z_MOVE:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, 0, true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON:
                    //nFuncResult = CleanerPickerLoadSecondClean(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    nFuncResult = CleanerPickerVacuumOn(false);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_ELV_BLOW_ON:
                    nFuncResult = SecondCleanBlowOn(false);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON_CHECK:
                    //nFuncResult = CleanerPickerLoadSecondClean(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    nFuncResult = CleanerPickerVacuumOn(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, -dOffset, true);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_ELV_BLOW_OFF:
                    //블로우 오프 막으면 안됨 22 05 05 작성자 : 홍은표
                    nFuncResult = SecondCleanBlowOff(false);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_ELV_T_LOAD_MOVE:
                    nFuncResult = MovePosFlip2(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_PLACE_POS_X_MOVE:
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_PLACE_POS_Z_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, -dOffset);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_PLACE_POS_Z_MOVE:
                    {
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, 0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            m_strMotPos = String.Format("MOVE DONE");
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_ELV_VAC_ON:
                    nFuncResult = SecondUltrasonicVacuumOn(false);
                    //nFuncResult = CleanerPickerUnloadSecondUltrasonic(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_BLOW_ON:
                    nFuncResult = CleanerPickerBlowOn(false, 100);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.ULTRASONIC_ELV_VAC_ON_CHECK:
                    nFuncResult = SecondUltrasonicVacuumOn(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_PRE_MOVE_2:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, -dOffset, true);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE_2:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_strMotPos = String.Format("MOVE DONE");
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_BLOW_OFF:
                    //220504 Blow Off 제거
                    //nFuncResult = CleanerPickerBlowOff(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_SAFETY_X_MOVE:
                    if(m_bAutoMode)
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            m_strMotPos = String.Format("MOVE DONE");
                        }
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            m_strMotPos = String.Format("MOVE DONE");
                        }
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondPickUpByPassMove()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SECOND_PICK_AND_PLACE)m_nSeqNo)
            {
                case CYCLE_SECOND_PICK_AND_PLACE.NONE:
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_SAFETY_POS_Z_MOVE:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_SAFETY_POS_X_MOVE:
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEANER_PICKER_PICK_UP_T_MOVE:
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN, 100);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_FLIP_UNLOAD_POS_MOVE:
                    nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_X_MOVE:
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_Z_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, -dOffset);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_LOAD_POS_Z_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, 0, true);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON:
                    //nFuncResult = CleanerPickerLoadSecondClean(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    nFuncResult = CleanerPickerVacuumOn(false);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_ELV_BLOW_ON:
                    nFuncResult = SecondCleanBlowOn(false);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UNIT_PK_2_VACUUM_ON_CHECK:
                    //nFuncResult = CleanerPickerLoadSecondClean(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    nFuncResult = CleanerPickerVacuumOn(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_CLEAN, -dOffset, true);
                    }
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_UP_MOVE:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.CLEAN_SAFETY_X_MOVE:
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_TABLE);
                    break;
                case CYCLE_SECOND_PICK_AND_PLACE.FINISH:
                    return FNC.SUCCESS;
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondUltrasonic()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SECOND_US)m_nSeqNo)
            {
                case CYCLE_SECOND_US.NONE:
                    break;
                case CYCLE_SECOND_US.CLEAN_FLIP_2_LOAD_POS_MOVE:
                    //작업 시간이 0이면 바로 끝
                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkTime == 0)
                    {
                        NextSeq((int)CYCLE_SECOND_US.US_END);
                        return FNC.BUSY;
                    }
                    #region 스트립 체크 
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                        {
                            NextSeq((int)CYCLE_SECOND_US.US_END);
                            return FNC.BUSY;
                        }
                    }
                    #endregion

                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_ULTRASONIC, true);
                        if (IsInPosFlip2Elv(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD)) break;
                    }

                    nFuncResult = MovePosFlipElv(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);
                    if(nFuncResult == FNC.SUCCESS)
                    {

                    }
                    break;
                case CYCLE_SECOND_US.CHECK_WATER_IN:
                    if (m_bAutoMode)
                    {
                        if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                        {
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);

                            // [2022.05.14.kmlee] 추가
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2, false);

                            return FNC.CYCLE_CHECK;
                        }
                    }

                    //if (IsDelayOver(100000))
                    //{
                    //    SetError((int)ERDF.E_SECOND_ULTRA_WATER_IN_TIMEOUT);
                    //    return FNC.BUSY;
                    //}
                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.WATER_FLOW_CHECK].bOptionUse == false) break;
                    ////물 공급 중단
                    //if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_1] == 1 &&
                    //    GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_2] == 1 &&
                    //    GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_3] == 1)
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);

                    //    // [2022.05.14.kmlee] 추가
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                    //    break;
                    //}
                    ////물 공급
                    //else
                    //{
                    //    //물을 공급해야 하는 경우는 센서가 하나라도 꺼지는 경우
                    //    if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_1] == 0 ||
                    //        GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_2] == 0 ||
                    //        GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_3] == 0)
                    //    {
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                    //    }
                    //    return FNC.BUSY;
                    //}
                    //물 배출에 대한 조건이 없음 위험함 //기술팀도 문제점 알고있음 (현동원프로님)
                    #region 예전 소스
                    //if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_3] == 1)
                    //{
                    //    //물이 너무 많음
                    //    //WATER OUT 또는 에러
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, true);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, true);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, true);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, true);
                    //    return FNC.BUSY;
                    //}

                    //if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_1] == 1)
                    //{
                    //    //WATER IN
                    //    if (GbVar.IO[IODF.INPUT.SECOND_ULTRASONIC_WATER_LEVEL_CHECK_SENSOR_2] == 1)
                    //    {
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                    //        //적정량 OK
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                    //        if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                    //            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                    //        //감지 되도 2번 안들어오면 물부족
                    //        return FNC.BUSY;
                    //    }
                    //}
                    //else
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, true);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, false);
                    //    if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 1)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, false);
                    //    //감지 안되도 물부족
                    //    return FNC.BUSY;
                    //}
                    #endregion


                    break;
                case CYCLE_SECOND_US.CHECK_US_SETTING:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    ////현재 출력과 현재 주파수 
                    //if (GbDev.ultrasonic[2].Setting_Output != RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkOutput ||
                    //    GbDev.ultrasonic[3].Setting_Output != RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkOutput)
                    //{
                    //    //레시피 값 대로 설정
                    //    GbDev.ultrasonic[2].SetOperationOutput(RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkOutput);
                    //    GbDev.ultrasonic[3].SetOperationOutput(RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkOutput);
                    //}
                    
                    //    int nValue = 1;
                    //    //레시피 값 대로 설정
                    //    switch (RecipeMgr.Inst.Rcp.cleaning.nSecondFreqChannelSetting)
                    //    {
                    //        case 40:
                    //            nValue = 1;
                    //            break;
                    //        case 80:
                    //            nValue = 2;
                    //            break;
                    //        case 120:
                    //            nValue = 3;
                    //            break;
                    //        default:
                    //            //ERROR
                    //            nValue = 1;
                    //            break;
                    //    }
                    //if (GbDev.ultrasonic[2].Setting_Freq_Channel != nValue ||
                    //    GbDev.ultrasonic[3].Setting_Freq_Channel != nValue)
                    //{
                    //    GbDev.ultrasonic[2].SetFreqChannel(nValue);
                    //    GbDev.ultrasonic[3].SetFreqChannel(nValue);
                    //}
                    break;
                case CYCLE_SECOND_US.US_FLIP:
                    nFuncResult = MovePosFlip2(POSDF.SECOND_ULTRASONIC_ULTRASONIC_WORK);
                    break;
                case CYCLE_SECOND_US.US_PFE_DOWN:
                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    nFuncResult = MovePosFlipElv(POSDF.SECOND_ULTRASONIC_ULTRASONIC_WORK);
                    break;
                case CYCLE_SECOND_US.US_START:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.ULTRASONIC_3_RUN, true);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.ULTRASONIC_4_RUN, true);
                    //초음파 발생
                    m_swSecondUltra.Restart();
                    break;
                case CYCLE_SECOND_US.MOVE_LEFT:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        double dPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_PICKER_FLIP_2].dPos[POSDF.SECOND_ULTRASONIC_ULTRASONIC_WORK] + RecipeMgr.Inst.Rcp.cleaning.dSecondUltraSwingOffset;
                        nFuncResult = AxisMovePos((int)SVDF.AXES.CL_PICKER_FLIP_2, dPos, RecipeMgr.Inst.Rcp.cleaning.dSecondUltraSwingVel, 0);
                    }
                    break;
                case CYCLE_SECOND_US.MOVE_RIGHT:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        double dPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_PICKER_FLIP_2].dPos[POSDF.SECOND_ULTRASONIC_ULTRASONIC_WORK];
                        nFuncResult = AxisMovePos((int)SVDF.AXES.CL_PICKER_FLIP_2, dPos, RecipeMgr.Inst.Rcp.cleaning.dSecondUltraSwingVel, 0);
                    }
                    break;
                case CYCLE_SECOND_US.MOVE_CHECK_CENTER:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (m_swSecondUltra.ElapsedMilliseconds < RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkTime)
                        {
                            NextSeq((int)CYCLE_SECOND_US.MOVE_LEFT);
                            return FNC.BUSY;
                        }
                        nFuncResult = MovePosFlip2(POSDF.SECOND_ULTRASONIC_ULTRASONIC_WORK);
                    }
                    break;
                case CYCLE_SECOND_US.US_END:
                    ////초음파 끝
                    //if (m_bFirstSeqStep)
                    //{
                    //    m_bFirstSeqStep = false;
                    //    swTimeStamp.Restart();
                    //}
                    //if (swTimeStamp.ElapsedMilliseconds < RecipeMgr.Inst.Rcp.cleaning.nSecondUltrasonicWorkTime) return FNC.BUSY;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    
                    if(m_bAutoMode)
                    {
                        //초음파 카운트 증가
                        GbVar.g_nSecondUltraWaterChangeCount++;

                        // [2022.05.15] 작성자 : 홍은표
                        // 2차 초음파 발생 후 물을 항상 제거해줘야한다.
                        // 그렇기에 조건 주석 처리하였음
                        // 다시 주석 해제2022 05 30 작성자 홍은표
                        if (GbVar.g_nSecondUltraWaterChangeCount >= RecipeMgr.Inst.Rcp.cleaning.nSecondWaterChangeCount)
                        {
                            GbVar.g_nSecondUltraWaterChangeCount = 0;
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL, false);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2] == 1)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_IN_SOL2, false);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL] == 0)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_SOL, true);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL] == 0)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_SOL, true);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL] == 0)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_1_1_SOL, true);
                            if (GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL] == 0)
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_WATER_OUT_2_1_SOL, true);
                        }
                    }
                    

                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.ULTRASONIC_3_RUN, false);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.ULTRASONIC_4_RUN, false);
                    break;
                case CYCLE_SECOND_US.US_PFE_UP:
                    nFuncResult = MovePosFlipElv(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);
                    break;

                case CYCLE_SECOND_US.US_UNLOAD_FLIP:
                    nFuncResult = MovePosFlip2(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);
                    break;
                case CYCLE_SECOND_US.CLEANER_PICKER_WAIT_ON:
                    break;
                case CYCLE_SECOND_US.FINISH:
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_ULTRASONIC, false);

                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int CleanerPickerPlasma()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_CLEANER_PICKER_PLASMA)m_nSeqNo)
            {
                case CYCLE_CLEANER_PICKER_PLASMA.NONE:
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.INTERLOCK_FLAG_CHECK:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (GbVar.IO[IODF.INPUT.PLASMA_ALARM_SIGNAL] == 1 ||
                        GbVar.IO[IODF.INPUT.PLASMA_READY_SIGNAL] == 0)
                    {
                        if (GbVar.IO[IODF.OUTPUT.PLASMA_ALARM_RESET] == 0)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_ALARM_RESET, true);

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return 999; // TODO : ALARM 작업해야함 작성자 : HEP
                        return FNC.BUSY;
                    }
                    //if (GbVar.IO[IODF.INPUT.DUST_CONTROLLER_ON_SIGNAL] == 0 ||
                    //    GbVar.IO[IODF.INPUT.DUST_CONTROLLER_OFF_SIGNAL] == 1)
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.DUST_CONTROLLER_RUN] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DUST_CONTROLLER_RUN, true);

                    //    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return (int)ERDF.E_NONE; // TODO : ALARM 작업해야함 작성자 : HEP
                    //    return FNC.BUSY;
                    //}
                    if (RecipeMgr.Inst.Rcp.cleaning.nCleanerPickerPlasmaRepeatCount == 0)
                    {
                        NextSeq((int)CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_SAFETY_Z_MOVE);
                        return FNC.BUSY;
                    }
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.CLEANER_PICKER_PLASMA, true);
                    }
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_Z_READY_MOVE:
                    m_nPickerPlasmaRepeatCount = 0;

                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }

                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_Y_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosDryY(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosDryX(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.CLEANER_PICKER_Z_SAFETY_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_READY)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.CLEANER_PICKER_X_PICKER_PLASMA_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.CLEANER_PICKER_R_PICKER_PLASMA_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_PLASMA, 100);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.CLEANER_PICKER_Z_PLASMA_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    break;
                //[2022.05.12 작성자 : 홍은표] 딜레이를 줄이고자 X축 이동 후 플라즈마를 동작한다.
                case CYCLE_CLEANER_PICKER_PLASMA.PICKER_PLASMA_SHOT:
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_USE].bOptionUse) break;
                    TurnOnOffPlasma(true);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_X_PICKER_PLASMA_MOVE:
                    //nFuncResult = MovePosDryX(POSDF.PLASMA_SECOND_PLASMA_START);
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_X].dVel[POSDF.PLASMA_SECOND_PLASMA_START], 0);
                    break;
         
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_Y_PICKER_PLASMA_MOVE:
                    //nFuncResult = MovePosDryY(POSDF.PLASMA_SECOND_PLASMA_START);
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_Y].dVel[POSDF.PLASMA_SECOND_PLASMA_START], 0);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_Z_PICKER_PLASMA_MOVE:
                    //nFuncResult = MovePosDryZ(POSDF.PLASMA_CLEANER_PICKER_PLASMA_CLEAN_TABLE);
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Z, RecipeMgr.Inst.Rcp.cleaning.dPickerPlasmaZPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_Z].dVel[POSDF.PLASMA_SECOND_PLASMA_START], 0);

                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.CL_PICKER_BLOW_SOL_ON:
                    //플라즈마 블로우 체크 할 필요 없어서 nFuncResult 뻄
                    CleanerPickerVacuumOn();
                    break;
              
                case CYCLE_CLEANER_PICKER_PLASMA.PICKER_PLASMA_WAIT:
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_START_WAIT_TIME].nValue)) return FNC.BUSY;
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.PICKER_PLASMA_START:
                    if (RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode < 4)
                    {
                        nFuncResult = ContinueMultiMove(2);
                    }
                    else
                    {
                        nFuncResult = ContinueMultiMove(2, true);
                    }
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.PICKER_PLASMA_END:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        swTimeStamp.Restart();
                    }
                    if (swTimeStamp.ElapsedMilliseconds > 100000)
                    {
                        //CleanerPickerBlowOff();
                        CleanerPickerVacuumOff();

                        TurnOnOffPlasma(false);
                        return (int)ERDF.E_NONE; // TODO : ALARM 작업해야함 작성자 : HEP //ERDF.E_PLASMA_SHOT_TIMEOUT
                    }
                    uint uMotionValue = 0;
                    CAXM.AxmContiIsMotion(0, ref uMotionValue);
                    if (uMotionValue == 1) return FNC.BUSY;
                    if (MotionMgr.Inst[SVDF.AXES.CL_DRY_X].IsBusy() ||
                        MotionMgr.Inst[SVDF.AXES.CL_DRY_Y].IsBusy()) return FNC.BUSY;

                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue)) return FNC.BUSY;
                    //CleanerPickerBlowOff();
                    CleanerPickerVacuumOff();

                    TurnOnOffPlasma(false);
                    m_nPickerPlasmaRepeatCount++;
                    if (m_nPickerPlasmaRepeatCount < RecipeMgr.Inst.Rcp.cleaning.nCleanerPickerPlasmaRepeatCount)
                    {
                        NextSeq((int)CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_X_PICKER_PLASMA_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_SAFETY_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }

                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_SAFETY_Y_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryY(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.DRY_PICKER_SAFETY_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryX(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.CL_PICKER_SAFETY_Z_MOVE:
                    //if (m_bAutoMode) break;
                    //nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.CL_PICKER_SAFETY_X_MOVE:
                    //if (m_bAutoMode) break;
                    //nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                    break;
                case CYCLE_CLEANER_PICKER_PLASMA.FINISH:
                    // 여기서 false를 해주어야 procCleanSecond의 WAIT_CLEANER_PICKER_PLASMA가 진행된다
                    //GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_PLASMA] = false;
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.CLEANER_PICKER_PLASMA, false);

                    return FNC.SUCCESS;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondUltraPickUp()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SECOND_ULTRA_PICK_UP)m_nSeqNo)
            {
                case CYCLE_SECOND_ULTRA_PICK_UP.NONE:
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.BEFORE_MOVE_CHECK_STRIP_INFO:
                    {
                        if (m_bAutoMode)
                        {
                            if ( GbVar.Seq.sCleaning[2].Info.IsStrip() &&  // 클리너 피커에는 정보가 있고
                                !GbVar.Seq.sCleaning[1].Info.IsStrip())    // 2차 초음파에는 정보가 없으면 이미 떴다고 판단
                            {
                                if (AirStatus(STRIP_MDL.CLEANER_PICKER) == AIRSTATUS.VAC)
                                {
                                    // 시퀀스 번호  변경 시 주의
                                    NextSeq((int)CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_UNLOAD_POS_Z_MOVE);
                                    return FNC.BUSY;
                                }
                                else
                                {
                                    nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_CLEANER_PK_STRIP_INFO;
                                }
                            }

                            // 알람 발생 후 해당 위치에서 다시 배큠 체크 하기 위한 작업 
                            // 2차 초음파에 스트립 정보가 있음
                            if (GbVar.Seq.sCleaning[1].Info.IsStrip())
                            {
                                //클리너 피커 위치가 픽업 하는 위치임
                                if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) &&
                                    IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC))
                                {
                                    NextSeq((int)CYCLE_SECOND_ULTRA_PICK_UP.CLEANER_PICKER_VAC_ON);
                                    return FNC.BUSY;
                                }
                            }
                        }
                    }
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.CLEANER_PICKER_Z_SAFETY_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_READY)) break;

                        if ((
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC) ||
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_END_ON_ULTRASONIC)) ||
                            (
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) &&
                            IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) &&
                            IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) &&
                            IsInPosFlip2(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD)))
                            break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.CLEANER_PICKER_X_SAFETY_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        //한번만 태우면 됨 뜨기 전 블로우 오프 기능 추가 작성자 홍은표 220504
                        m_bFirstSeqStep = false;
                        CleanerPickerBlowOff();
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) ||
                            IsInPosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL, true);
                    //}
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.CLEANER_PICKER_R_SAFETY_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                    }
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, 100);
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.CHECK_STRIP_INFO:
                    {
                        //오토런
                        if (m_bAutoMode)
                        {
                            //---- 스크랩 처리 --------//
                            // 2차 초음파 정보가 없으면 (클리너피커)
                            if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                            {
                                //OUTPUT이 정상적으로 꺼져있으면 종료
                                if (AirStatus(STRIP_MDL.CLEANER_PICKER) == AIRSTATUS.NONE)
                                {
                                    NextSeq((int)CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_UNLOAD_POS_Z_MOVE);
                                    return FNC.BUSY;
                                }
                                else
                                {
                                    nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_SECOND_CLEAN_STRIP_INFO;
                                }
                            }
                        }
                    }
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_LOAD_POS_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC);
                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL, false);
                    //}
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_LOAD_POS_Z_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                        }
                            
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, -dOffset);
                    }
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_LOAD_POS_Z_MOVE:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                    }

                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, 0, true);
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.CLEANER_PICKER_VAC_ON:
                    nFuncResult = CleanerPickerVacuumOn(false);
                    //nFuncResult = CleanerPickerLoadUltrasonic(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_BLOW_ON:
                    nFuncResult = SecondUltrasonicBlowOn(false);
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.CLEANER_PICKER_VAC_ON_CHECK:
                    nFuncResult = CleanerPickerVacuumOn(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                    //알람이 발생하는 경우
                    
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_UNLOAD_POS_Z_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                        }

                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, -dOffset);
                    }
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_UNLOAD_POS_Z_MOVE:
                    if (m_bAutoMode)
                    {
                        //---- 스크랩 처리 --------//
                        // 2차 초음파 정보가 없으면 (클리너피커)
                        if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                        {
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                        }
                        else
                        {
                            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            {
                                //정상 동작
                                nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM);
                            }
                            else
                            {
                                nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP);
                            }
                        }
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    }
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_BLOW_OFF:
                    nFuncResult = SecondUltrasonicBlowOff(false);
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.ULTRASONIC_UNLOAD_POS_X_MOVE:
                    {
                        //---- 스크랩 처리 --------//
                        // 2차 초음파 정보가 없으면 (클리너피커)
                        if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                        }
                    }
                    break;
                case CYCLE_SECOND_ULTRA_PICK_UP.FINISH:
                    //GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING_COMPLETE] = true;
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        #endregion

        #region Dry & Plasma
        public int DryPlasmaInit()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_DRY_INIT)m_nSeqNo)
            {
                case CYCLE_DRY_INIT.NONE:
                    break;
                case CYCLE_DRY_INIT.INIT:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    //플라즈마 초기화
                    if (GbVar.IO[IODF.INPUT.PLASMA_CONTROLLER_MC_SIGNAL] == 0)
                    {
                        if(GbVar.IO[IODF.OUTPUT.PLASMA_CONTROLLER_POWER_ON_SIGNAL] == 0)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_CONTROLLER_POWER_ON_SIGNAL, true);
                    }
                    ////시작할때 On
                    //CleanerPickerBlowOn();

                    //if (GbVar.IO[IODF.OUTPUT.PLASMA_HEATING_AIR_SOL] == 0)
                    //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEATING_AIR_SOL, true);
                    break;
                case CYCLE_DRY_INIT.CHECK_PLASMA_TEMP:
                    //플라즈마 정보 체크
                    //TODO : 파워미터 점검
                    break;
                case CYCLE_DRY_INIT.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int DryPickerFlipAndPlasmaMove()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_DRY_FLIP_AND_MOVE)m_nSeqNo)
            {
                case CYCLE_DRY_FLIP_AND_MOVE.NONE:
                    break;
                case CYCLE_DRY_FLIP_AND_MOVE.READY_Z_MOVE:
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            NextSeq((int)CYCLE_DRY_FLIP_AND_MOVE.CHECK_STRIP_SAFTEY_MOVE);
                            return FNC.BUSY;
                        }
                    }
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_READY)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    break;
                case CYCLE_DRY_FLIP_AND_MOVE.DRY_FLIP_SAFETY_X_MOVE: //드라이가 없어졌음
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    break;
                case CYCLE_DRY_FLIP_AND_MOVE.DRY_FLIP_MOVE:
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_PLASMA, 100); //드라이 없어서 피커 플라즈마로 해야함
                    break;
                case CYCLE_DRY_FLIP_AND_MOVE.CHECK_STRIP_SAFTEY_MOVE:
                    //nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            // 자재가 없는 경우 Z를 올리고 
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                        }
                    }
                    break;
                case CYCLE_DRY_FLIP_AND_MOVE.CLEANER_PICKER_PLASMA_START_Z_MOVE:
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            // 자재가 없는경우 X를 보냄
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                        }
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    }
                    break;
                case CYCLE_DRY_FLIP_AND_MOVE.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondCleanPickUpDry()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_nSecondCleanPickUpDryCount = 0;
                            m_bFirstSeqStep = false;
                        }
                    }
                    break;
                case 1:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;

                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_CLEAN_BOTTOM_DRY, true);

                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 2://시작 위치 
                    {
                        #region 스트립 체크 
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                NextSeq(95);
                                return FNC.BUSY;
                            }
                        }
                        #endregion

                        if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                                IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                        }
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 3:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                                IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN)) break;
                        }
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_START_BOTTOM, 100);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP, 100);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 4://시작 위치 
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM) ||
                                IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP)) break;
                        }
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 5:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;

                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM) ||
                                IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP)) break;
                        }
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP);
                        }
                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 8:
                    {
                        // [2022.05.14.kmlee] 추가
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRA_TO_DRY_ZONE_AIR_KNIFE, true);
                    }
                    break;
                case 10://시작 위치 
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nSecondCleanPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;
                        
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM) ||
                                IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP)) break;
                        }

                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRA_TO_DRY_ZONE_AIR_KNIFE, true);
                        }
                    }
                    break;

                case 15://끝 위치 
                    {
                        double dVel = RecipeMgr.Inst.Rcp.cleaning.dSecondCleanPickUpDryBlowVel;

                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_BOTTOM, true, dVel);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_BOTTOM_STRIP, true, dVel);
                        }
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRA_TO_DRY_ZONE_AIR_KNIFE, false);
                        }
                    }
                    break;

                case 20://반복 횟수 확인
                    {
                        if (m_nSecondCleanPickUpDryCount + 1 >= RecipeMgr.Inst.Rcp.cleaning.nSecondCleanPickUpDryBlowCount)
                        {
                            NextSeq(100);
                            return FNC.BUSY;
                            return FNC.SUCCESS;
                        }

                        m_nSecondCleanPickUpDryCount++;

                        NextSeq(10);
                        return FNC.BUSY;
                    }
                case 95:
                    {
                        #region 스트립 체크 
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                            }
                        }
                        #endregion
                    }
                    break;
                case 100:
                    {
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            if (m_bAutoMode)
                            {
                                nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                            }
                            else
                            {
                                nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC);
                            }
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC);
                        }
                    }
                    break;
                case 101:
                    {
                        // [2022.05.14.kmlee] 추가
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRA_TO_DRY_ZONE_AIR_KNIFE, false);
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_CLEAN_BOTTOM_DRY, false);
                        return FNC.SUCCESS;
                    }
                    break;
                default:
                    break;
            }

            #region AFTER SWITCH
            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondCleanDry()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_DRY_CLEAN)m_nSeqNo)
            {
                case CYCLE_DRY_CLEAN.NONE:

                    break;
                case CYCLE_DRY_CLEAN.CHECK_AND_INIT:
                    {
                        #region 스트립 체크 
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                            {
                                
                                NextSeq((int)CYCLE_DRY_CLEAN.MOVE_CLEANER_PICKER_DRY_SAFETY_POS_MOVE);
                                return FNC.BUSY;
                            }
                        }
                        #endregion

                        m_nSecondDryCount = 0;
                        if (RecipeMgr.Inst.Rcp.cleaning.nSecondDryBlowCount == 0)
                        {
                            NextSeq((int)CYCLE_DRY_CLEAN.FINISH);
                            return FNC.BUSY;
                        }
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_CLEAN_DRY, true);
                        }
                    }
                    break;
                case CYCLE_DRY_CLEAN.MOVE_DRY_PLASMA_SAFETY_POS_Z_MOVE:
                    if(!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                        }
                        nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_DRY_CLEAN.MOVE_DRY_PLASMA_SAFETY_POS_Y_MOVE:
                    if (!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryY(POSDF.PLASMA_DRY_READY)) break;
                        }
                        nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_DRY_CLEAN.MOVE_DRY_PLASMA_SAFETY_POS_X_MOVE:
                    if (!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryX(POSDF.PLASMA_DRY_READY)) break;
                        }
                        nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_DRY_CLEAN.MOVE_DRY_START_READY_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_DRY_CLEAN.CLEANER_PICKER_SAFETY_X_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }

                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_DRY_CLEAN.CLEANER_PICKER_TURN_POS:
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING, 100);
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING_STRIP, 100);
                    }
                    break;
                case CYCLE_DRY_CLEAN.MOVE_FLIP_ELV_TURN_DRY_POS:
                    //nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_DRY_WITH_PICKER);
                    nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD);
                    break;
                case CYCLE_DRY_CLEAN.MOVE_DRY_START_POS:
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING);
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING_STRIP);
                    }
                    break;
                case CYCLE_DRY_CLEAN.MOVE_DRY_START_Z_POS:
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING);
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING_STRIP);

                    }
                    break;//


                case CYCLE_DRY_CLEAN.DRY_BLOW_ON:
                    //Down하면 충돌할 것으로 예상  2022. 05 05. 작성자 : 홍은표 
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, true);

                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL, true);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL2, true);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL3, true);

                    //if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].lValue)) return (int)ERDF.E_INTL_CLEANER_FLIP_T1_CYLINDER_COVER_CLOSE;

                    //if (GbVar.IO[IODF.INPUT.PR_AXIS_SPARE_SENSOR_1] == 1 &&
                    //   GbVar.IO[IODF.INPUT.PR_AXIS_SPARE_SENSOR_2] == 0) return FNC.BUSY;
                    //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, 100);
                    break;
                case CYCLE_DRY_CLEAN.MOVE_DRY_END_POS:
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_ON_CLEANING);
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_ON_CLEANING_STRIP);
                    }
                    break;
                case CYCLE_DRY_CLEAN.DRY_BLOW_OFF:
                    //if (WaitDelay(500)) return FNC.BUSY;
                    //TODO : 열풍기 동작 방법
                    //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, 0);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL, false);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL2, false);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL3, false);
                    break;
                case CYCLE_DRY_CLEAN.CHECK_DRY_COUNT:
                    m_nSecondDryCount++;
                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondDryBlowCount > m_nSecondDryCount)
                    {
                        NextSeq((int)CYCLE_DRY_CLEAN.MOVE_DRY_START_POS);
                        return FNC.BUSY;
                    }
                    else
                    {
                        m_nSecondDryCount = 0;
                        break;
                    }
                    break;
                case CYCLE_DRY_CLEAN.MOVE_CLEANER_PICKER_DRY_SAFETY_POS_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, false);
                        }

                        if (!m_bAutoMode)
                        {
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                        }

                        #region 스트립 제거 안전 위치 이동
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                            {
                                nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                            }
                        }
                        #endregion
                    }
                    break;
                case CYCLE_DRY_CLEAN.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE:
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, false);

                    //nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    
                    break;
                // [2022.05.14.kmlee] 실린더가 먼저 안내려가고 X가 먼저 움직인다. 확인 필요
                case CYCLE_DRY_CLEAN.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE:
                    // 임시
                    if (m_bFirstSeqStep)
                    {
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, false);
                    }
                    if (m_bAutoMode)
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_CLEAN);

                        #region 스트립 제거 안전 위치 이동
                        if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                        }
                        #endregion
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    }
                    break;
                case CYCLE_DRY_CLEAN.FINISH:
                    //20220505 안전체크 위에서 미리 끔
#if _NOTEBOOK
                    return FNC.SUCCESS;
#endif
                    if (GbVar.IO[IODF.INPUT.PR_AXIS_SPARE_SENSOR_1] == 0)
                    {
                        if (GbVar.IO[IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL] == 1)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, false);

                        return FNC.BUSY;
                    }
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_CLEAN_DRY, false);

                    return FNC.SUCCESS;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondUltraDry()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_DRY_ULTRASONIC)m_nSeqNo)
            {
                case CYCLE_DRY_ULTRASONIC.NONE:
                    //if (GbVar.IO[IODF.INPUT.HEATING_MC_SIGNAL] == 0)
                    //{
                    //    if(GbVar.IO[IODF.OUTPUT.HEATING_POWER_ON_SIGNAL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, true);
                    //}
                    break;
                case CYCLE_DRY_ULTRASONIC.CHECK_AND_INIT:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                            {

                                NextSeq((int)CYCLE_DRY_ULTRASONIC.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE);
                                return FNC.BUSY;
                            }
                        }

                        m_nFirstDryCount = 0;
                        if (RecipeMgr.Inst.Rcp.cleaning.nFirstDryBlowCount == 0)
                        {
                            NextSeq((int)CYCLE_DRY_ULTRASONIC.FINISH);
                            return FNC.BUSY;
                        }
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_ULTRASONIC_DRY, true);
                        }
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_PLASMA_SAFETY_POS_Z_MOVE:
                    if (!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                        }
                        nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_PLASMA_SAFETY_POS_Y_MOVE:
                    if (!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryY(POSDF.PLASMA_DRY_READY)) break;
                        }
                        nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_PLASMA_SAFETY_POS_X_MOVE:
                    if (!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryX(POSDF.PLASMA_DRY_READY)) break;
                        }
                        nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    }
                    break;
				case CYCLE_DRY_ULTRASONIC.MOVE_DRY_START_READY_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if(IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA) ||
                           IsInPosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_DRY_ULTRASONIC.CLEANER_PICKER_SAFETY_X_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA) ||
                            IsInPosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_DRY_ULTRASONIC.CLEANER_PICKER_TURN_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            if (IsInPosCleanerR(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC)) break;
                        }
                        else
                        {
                            if (IsInPosCleanerR(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC_STRIP)) break;
                        }
                    }
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC, 100);
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC_STRIP, 100);
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_FLIP_ELV_TURN_ENABLE_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosFlip2Elv(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD)) break;
                    }
                    nFuncResult = MovePosFlipElv(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_FLIP_ELV_TURN_DRY_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            if (IsInPosFlip2(POSDF.SECOND_ULTRASONIC_DRY_WORK)) break;
                        }
                        else
                        {
                            if (IsInPosFlip2(POSDF.SECOND_ULTRASONIC_DRY_WORK_STRIP)) break;
                        }
                    }
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosFlip2(POSDF.SECOND_ULTRASONIC_DRY_WORK);
                    }
                    else
                    {
                        nFuncResult = MovePosFlip2(POSDF.SECOND_ULTRASONIC_DRY_WORK_STRIP);
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_FLIP_ELV_DRY_POS:
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosFlipElv(POSDF.SECOND_ULTRASONIC_DRY_WORK);
                    }
                    else
                    {
                        nFuncResult = MovePosFlipElv(POSDF.SECOND_ULTRASONIC_DRY_WORK_STRIP);
                    }
                    break;

                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_START_POS:
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC);
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC_STRIP);
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_START_Z_POS:
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC);
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC_STRIP);
                    }
                    break;//
   

                case CYCLE_DRY_ULTRASONIC.DRY_BLOW_ON:
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, true);

                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL, true);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL2, true);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL3, true);
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].lValue)) return (int)ERDF.E_INTL_AIR_KNIFE_CYLINDER_DOWN_FAIL;

                    if (GbVar.IO[IODF.INPUT.PR_AXIS_SPARE_SENSOR_1] == 1 &&
                       GbVar.IO[IODF.INPUT.PR_AXIS_SPARE_SENSOR_2] == 0) return FNC.BUSY;
                    //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, 100);
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_END_POS:
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_ON_ULTRASONIC);
                    }
                    else
                    {
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_ON_ULTRASONIC_STRIP);
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.DRY_BLOW_OFF:
                    //if (WaitDelay(1000)) return FNC.BUSY;
                    //TODO : 열풍기 동작 방법
                    //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, 0);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL, false);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL2, false);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_BLOW_SOL3, false);
                    break;
                case CYCLE_DRY_ULTRASONIC.CHECK_DRY_COUNT:
                    m_nFirstDryCount++;
                    if (RecipeMgr.Inst.Rcp.cleaning.nFirstDryBlowCount > m_nFirstDryCount)
                    {
                        NextSeq((int)CYCLE_DRY_ULTRASONIC.MOVE_DRY_START_POS);
                        return FNC.BUSY;
                    }
                    else
                    {
                        
                        m_nFirstDryCount = 0;
                        break;
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.CYL_UP_CHECK:
                    {
                        if (GbVar.IO[IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL] == 1)
                        {
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, false);
                        }

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].lValue)) return (int)ERDF.E_INTL_AIR_KNIFE_CYLINDER_DOWN_FAIL;

                        if (GbVar.IO[IODF.INPUT.PR_AXIS_SPARE_SENSOR_1] == 0) return FNC.BUSY;
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE:
                    // [2022.05.14.kmlee] X를 먼저 움직여서 추가함
                    if (m_bFirstSeqStep)
                    {
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, false);
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE:
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_DRY_ULTRASONIC.DRY_UP_DOWN_SOL_CLEAR:
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, false);
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_ULTRA_ELV_LOAD_POS:
                    nFuncResult = MovePosFlipElv(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);
                    break;
                case CYCLE_DRY_ULTRASONIC.FINISH:
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_ULTRASONIC_DRY, false);
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int PlasmaBottomDry()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {

                                NextSeq(95);
                                return FNC.BUSY;
                            }
                        }

                        if (m_bFirstSeqStep)
                        {
                            m_nPlasmaPickUpDryCount = 0;
                            m_bFirstSeqStep = false;
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_ULTRASONIC_BOTTOM_DRY, true);
                        }
                    }
                    break;
                case 1:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nPlasmaPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;

                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM) ||
                                IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP) ||
                                IsInPosCleanerZ(POSDF.CLEANER_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 2://시작 위치 
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nPlasmaPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) &&
                                IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                        }
                        nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 3:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) &&
                                IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC)) break;
                        }
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_START_BOTTOM, 100);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP, 100);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 4://시작 위치 
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nPlasmaPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM) ||
                                IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP)) break;
                        }
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 5:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nPlasmaPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;

                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM) ||
                                IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP)) break;
                        }
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                case 8:
                    {
                        // [2022.05.14.kmlee] 추가
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRA_TO_DRY_ZONE_AIR_KNIFE, true);
                    }
                    break;
                case 10://시작 위치 
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nPlasmaPickUpDryBlowCount == 0)
                            return FNC.SUCCESS;
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM)) break;
                        }
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM);
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_BOTTOM_STRIP);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            // 20220516 작성자 : 홍은표
                            // 드라이존 에어나이프 도착후 껐다가 시작하면 다시 키는 방식으로 변경 
                            // 시작위치 도착하면 다시 에어나이프 온
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRA_TO_DRY_ZONE_AIR_KNIFE, true);
                        }
                    }
                    break;

                case 15://끝 위치 
                    {
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            // 파라미터 위치가 달라서 100들어간게 offset이어서 뺐음 [2022 05 16 작성자 홍은표]
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_BOTTOM);
                        }
                        else
                        {
                            // 파라미터 위치가 달라서 100들어간게 offset이어서 뺐음 [2022 05 16 작성자 홍은표]
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_BOTTOM_STRIP);
                        }
                            

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            // 20220516 작성자 : 홍은표
                            // 드라이존 에어나이프 도착후 껐다가 시작하면 다시 키는 방식으로 변경 
                            // 종료위치 도착하면 다시 에어나이프 오프
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRA_TO_DRY_ZONE_AIR_KNIFE, false);
                        }
                    }
                    break;

                case 20://반복 횟수 확인
                    {
                        if (m_nPlasmaPickUpDryCount + 1 >= RecipeMgr.Inst.Rcp.cleaning.nPlasmaPickUpDryBlowCount)
                        {
                            NextSeq(100);
                            return FNC.BUSY;
                            return FNC.SUCCESS;
                        }

                        m_nPlasmaPickUpDryCount++;

                        NextSeq(10);
                        return FNC.BUSY;
                    }
                case 95:
                    {
                        if(m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                            }
                        }
                    }
                    break;
                case 100:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                            }
                            else
                            {
                                nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                            }
                        }
                        else
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                        }
                    }
                    break;
                case 101:
                    {
                        // [2022.05.14.kmlee] 추가
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.SECOND_ULTRA_TO_DRY_ZONE_AIR_KNIFE, false);
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_ULTRASONIC_BOTTOM_DRY, false);

                        return FNC.SUCCESS;
                    }
                    break;
                default:
                    break;
            }

            #region AFTER SWITCH
            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int CleanerPickerPlasmaSafetyMove()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_DRY_ULTRASONIC)m_nSeqNo)
            {
                case CYCLE_DRY_ULTRASONIC.NONE:
                    //if (GbVar.IO[IODF.INPUT.HEATING_MC_SIGNAL] == 0)
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.HEATING_POWER_ON_SIGNAL] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, true);
                    //}
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_START_READY_Z_POS:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_START_POS:
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC);
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_START_Z_POS:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC);
                    break;
                case CYCLE_DRY_ULTRASONIC.DRY_BLOW_ON:
                    //TODO : 열풍기 동작 방법
                    //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, 100);
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_DRY_END_POS:
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_END_ON_ULTRASONIC);
                    break;
                case CYCLE_DRY_ULTRASONIC.DRY_BLOW_OFF:
                    //TODO : 열풍기 동작 방법
                    //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, 0);
                    break;
                case CYCLE_DRY_ULTRASONIC.CHECK_DRY_COUNT:
                    if (RecipeMgr.Inst.Rcp.cleaning.nFirstDryBlowCount > m_nFirstDryCount)
                    {
                        m_nFirstDryCount++;
                        NextSeq((int)CYCLE_DRY_ULTRASONIC.MOVE_DRY_START_POS);
                        return FNC.BUSY;
                    }
                    else
                    {
                        m_nFirstDryCount = 0;
                        break;
                    }
                    break;
                case CYCLE_DRY_ULTRASONIC.MOVE_CLEANER_PICKER_DRY_SAFETY_POS_MOVE:
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                    break;
                //case CYCLE_DRY_1.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE:
                //    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_PLASMA_SAFETY_DOWN);
                //    break;
                //case CYCLE_DRY_1.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE:
                //    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_PLASMA_SAFETY_DOWN);
                //    break;
                case CYCLE_DRY_ULTRASONIC.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int FirstPlasmaStartPosMove()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_PLASMA_1)m_nSeqNo)
            {
                case CYCLE_PLASMA_1.NONE:
                    break;
                case CYCLE_PLASMA_1.ALARM_RESET_AND_ALARM_CHECK:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        swTimeStamp.Restart();
                    }
                    if (GbVar.IO[IODF.INPUT.PLASMA_ALARM_SIGNAL] == 1)
                    {
                        if (GbVar.IO[IODF.OUTPUT.PLASMA_ALARM_RESET] == 0)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_ALARM_RESET, true);

                        if (swTimeStamp.ElapsedMilliseconds > 10000) return (int)ERDF.E_NONE; // TODO : ALARM 작업해야함 작성자 : HEP
                        return FNC.BUSY;
                    }
                    else
                    {
                        break;
                    }
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_READY_Z_POS:
                    if (!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                        }
                        nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_START_X_POS:
                    //nFuncResult = MovePosDryXY(POSDF.PLASMA_FIRST_PLASMA_START);
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, 100);
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_START_Y_POS:
                    //nFuncResult = MovePosDryXY(POSDF.PLASMA_FIRST_PLASMA_START);
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, 100);
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_START_Z_POS:
                    //nFuncResult = MovePosDryZ(POSDF.PLASMA_FIRST_PLASMA_START);
                    break;

                case CYCLE_PLASMA_1.FINISH:
                    return FNC.SUCCESS;
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int FirstPlasma()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_PLASMA_1)m_nSeqNo)
            {
                case CYCLE_PLASMA_1.NONE:
                    break;
                case CYCLE_PLASMA_1.ALARM_RESET_AND_ALARM_CHECK:
                    m_nFirstPitchCount = 0;
                    m_nFirstPlasmaRepeatCount = 0;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (GbVar.IO[IODF.INPUT.PLASMA_ALARM_SIGNAL] == 1 ||
                        GbVar.IO[IODF.INPUT.PLASMA_READY_SIGNAL] == 0)
                    {
                        if (GbVar.IO[IODF.OUTPUT.PLASMA_ALARM_RESET] == 0)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_ALARM_RESET, true);

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return 999; // TODO : ALARM 작업해야함 작성자 : HEP
                        return FNC.BUSY;
                    }
                    //if (GbVar.IO[IODF.INPUT.DUST_CONTROLLER_ON_SIGNAL] == 0 ||
                    //    GbVar.IO[IODF.INPUT.DUST_CONTROLLER_OFF_SIGNAL] == 1)
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.DUST_CONTROLLER_RUN] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DUST_CONTROLLER_RUN, true);

                    //    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return (int)ERDF.E_NONE; // TODO : ALARM 작업해야함 작성자 : HEP
                    //    return FNC.BUSY;
                    //}
                    if (RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaRepeatCount == 0)
                    {
                        NextSeq((int)CYCLE_PLASMA_1.MOVE_RETURN_Z_POS);
                        return FNC.BUSY;
                    }
                    break;

                case CYCLE_PLASMA_1.MOVE_PLASMA_READY_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_1.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }                    
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_PLASMA_1.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_START_X_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_FIRST_PLASMA_START)) break;
                    }                    
                    nFuncResult = MovePosDryX(POSDF.PLASMA_FIRST_PLASMA_START);
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_START_Y_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_FIRST_PLASMA_START)) break;
                    }
                    nFuncResult = MovePosDryY(POSDF.PLASMA_FIRST_PLASMA_START);
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_START_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_FIRST_PLASMA_START)) break;
                    }
                    nFuncResult = MovePosDryZ(POSDF.PLASMA_FIRST_PLASMA_START);
                    break;
                case CYCLE_PLASMA_1.TURN_ON_PLASMA:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_USE].bOptionUse) break;

                    TurnOnOffPlasma(true);
                    break;
                case CYCLE_PLASMA_1.DELAY_PLASMA:
                    if (m_bFirstSeqStep)
                    {
                        swTimeStamp.Restart();
                        m_bFirstSeqStep = false;
                    }
                    if (swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_START_WAIT_TIME].nValue) break;
                    return FNC.BUSY;

                case CYCLE_PLASMA_1.MOVE_PLASMA_CHECK:
                    if (RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode < 4)
                    {
                        #region Y를 길게 움직임
                        double dChkPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos + RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStepPitch * m_nFirstPitchCount;
                        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
                        {
                            if (dChkPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
                            {
                                if (m_bFirstSeqStep)
                                {
                                    swTimeStamp.Restart();
                                    m_bFirstSeqStep = false;
                                }
                                if (swTimeStamp.ElapsedMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue) return FNC.BUSY;

                                TurnOnOffPlasma(false);

                                m_nFirstPlasmaRepeatCount++;
                                //플라즈마 반복 체크
                                if (m_nFirstPlasmaRepeatCount >= RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaRepeatCount)
                                {
                                    m_nFirstPitchCount = 0;
                                    m_nFirstPlasmaRepeatCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_1.MOVE_RETURN_Z_POS);
                                }
                                else
                                {
                                    m_nFirstPitchCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_START_X_POS);
                                }
                                return FNC.BUSY;
                            }
                            else
                            {
                                NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_X_MOVE);
                                return FNC.BUSY;
                            }
                        }
                        else
                        {
                            if (dChkPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
                            {
                                if (m_bFirstSeqStep)
                                {
                                    swTimeStamp.Restart();
                                    m_bFirstSeqStep = false;
                                }
                                if (swTimeStamp.ElapsedMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue) return FNC.BUSY;

                                TurnOnOffPlasma(false);

                                m_nFirstPlasmaRepeatCount++;
                                //플라즈마 반복 체크
                                if (m_nFirstPlasmaRepeatCount >= RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaRepeatCount)
                                {
                                    m_nFirstPitchCount = 0;
                                    m_nFirstPlasmaRepeatCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_1.MOVE_RETURN_Z_POS);
                                }
                                else
                                {
                                    m_nFirstPitchCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_START_X_POS);
                                }
                                return FNC.BUSY;
                            }
                            else
                            {
                                NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_X_MOVE);
                                return FNC.BUSY;
                            }

                        }
                        #endregion

                    }
                    else
                    {
                        #region X를 길게 움직임
                        double dChkPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos + RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStepPitch * m_nFirstPitchCount;
                        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
                        {
                            if (dChkPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
                            {
                                if (m_bFirstSeqStep)
                                {
                                    swTimeStamp.Restart();
                                    m_bFirstSeqStep = false;
                                }
                                if (swTimeStamp.ElapsedMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue) return FNC.BUSY;

                                TurnOnOffPlasma(false);

                                m_nFirstPlasmaRepeatCount++;
                                //플라즈마 반복 체크
                                if (m_nFirstPlasmaRepeatCount >= RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaRepeatCount)
                                {
                                    m_nFirstPitchCount = 0;
                                    m_nFirstPlasmaRepeatCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_1.MOVE_RETURN_Z_POS);
                                }
                                else
                                {
                                    m_nFirstPitchCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_START_X_POS);
                                }
                                return FNC.BUSY;
                            }
                            else
                            {
                                NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_Y_MOVE);
                                return FNC.BUSY;
                            }
                        }
                        else
                        {
                            if (dChkPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
                            {
                                if (m_bFirstSeqStep)
                                {
                                    swTimeStamp.Restart();
                                    m_bFirstSeqStep = false;
                                }
                                if (swTimeStamp.ElapsedMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue) return FNC.BUSY;

                                TurnOnOffPlasma(false);

                                m_nFirstPlasmaRepeatCount++;
                                //플라즈마 반복 체크
                                if (m_nFirstPlasmaRepeatCount >= RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaRepeatCount)
                                {
                                    m_nFirstPitchCount = 0;
                                    m_nFirstPlasmaRepeatCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_1.MOVE_RETURN_Z_POS);
                                }
                                else
                                {
                                    m_nFirstPitchCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_START_X_POS);
                                }
                                return FNC.BUSY;
                            }
                            else
                            {
                                NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_Y_MOVE);
                                return FNC.BUSY;
                            }
                        }
                        #endregion
                    }
                    break;

                #region Y를 길게 움직임
                case CYCLE_PLASMA_1.MOVE_PLASMA_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        m_dPlasmaXPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos + RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStepPitch * m_nFirstPitchCount;
                    }

                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, m_dPlasmaXPos, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel, 0);
                    break;

                case CYCLE_PLASMA_1.MOVE_PLASMA_Y_MOVE_CHECK:
                    if (IsInPosDryY(RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos))
                    {
                        NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_Y_END_MOVE);
                        return FNC.BUSY;
                    }
                    else
                    {
                        NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_Y_START_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_Y_END_MOVE:
                    m_dPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos;
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, m_dPos, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel, 0);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq((int)CYCLE_PLASMA_1.TURN_OFF_PLASMA);
                        return FNC.BUSY;
                    }
                    break;

                case CYCLE_PLASMA_1.MOVE_PLASMA_Y_START_MOVE:
                    m_dPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, m_dPos, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel, 0);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq((int)CYCLE_PLASMA_1.TURN_OFF_PLASMA);
                        return FNC.BUSY;
                    }
                    break;
                #endregion

                #region X를 길게 움직임
                case CYCLE_PLASMA_1.MOVE_PLASMA_Y_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        m_dPlasmaYPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos + RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStepPitch * m_nFirstPitchCount;
                    }

                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, m_dPlasmaYPos, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel, 0);
                    break;


                case CYCLE_PLASMA_1.MOVE_PLASMA_X_MOVE_CHECK:
                    if (IsInPosDryX(RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos))
                    {
                        NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_X_END_MOVE);
                        return FNC.BUSY;
                    }
                    else
                    {
                        NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_X_START_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_PLASMA_1.MOVE_PLASMA_X_END_MOVE:
                    m_dPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos;
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, m_dPos, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel, 0);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq((int)CYCLE_PLASMA_1.TURN_OFF_PLASMA);
                        return FNC.BUSY;
                    }
                    break;

                case CYCLE_PLASMA_1.MOVE_PLASMA_X_START_MOVE:
                    m_dPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, m_dPos, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel, 0);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq((int)CYCLE_PLASMA_1.TURN_OFF_PLASMA);
                        return FNC.BUSY;
                    }
                    break;
                #endregion

                case CYCLE_PLASMA_1.TURN_OFF_PLASMA:
                    break;
                case CYCLE_PLASMA_1.RETURN_CHECK:
                    m_nFirstPitchCount++;
                    NextSeq((int)CYCLE_PLASMA_1.MOVE_PLASMA_CHECK);
                    return FNC.BUSY;
                case CYCLE_PLASMA_1.MOVE_RETURN_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_1.MOVE_RETURN_Y_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryY(POSDF.PLASMA_DRY_READY)) break;
                    }
                    if (m_bAutoMode)
                    {
                        nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    }
                    else
                    {
                        nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_PLASMA_1.MOVE_RETURN_X_POS:
                    //if (m_bAutoMode)
                    //{
                    //    nFuncResult = MovePosDryX(POSDF.PLASMA_FIRST_PLASMA_START);
                    //}
                    //else
                    //{
                    //    nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    //}
                    break;
                case CYCLE_PLASMA_1.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
           
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int FirstPlasma_ContinousMode()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_PLASMA_1_CONTINUOUS)m_nSeqNo)
            {
                case CYCLE_PLASMA_1_CONTINUOUS.NONE:
                    //클리너 피커 XY 동시 이동 넣어야함
                    //플라즈마 발생하는 IO도 넣어야함
                    break;

                case CYCLE_PLASMA_1_CONTINUOUS.ALARM_RESET_AND_ALARM_CHECK:
                    #region 스트립 체크
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                        {

                            NextSeq((int)CYCLE_PLASMA_1_CONTINUOUS.MOVE_RETURN_Z_POS);
                            return FNC.BUSY;
                        }
                    }
                    #endregion

                    m_nFirstPitchCount = 0;
                    m_nFirstPlasmaRepeatCount = 0;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (GbVar.IO[IODF.INPUT.PLASMA_ALARM_SIGNAL] == 1 ||
                        GbVar.IO[IODF.INPUT.PLASMA_READY_SIGNAL] == 0)
                    {
                        if (GbVar.IO[IODF.OUTPUT.PLASMA_ALARM_RESET] == 0)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_ALARM_RESET, true);

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return 999; // TODO : ALARM 작업해야함 작성자 : HEP
                        return FNC.BUSY;
                    }
                    //if (GbVar.IO[IODF.INPUT.DUST_CONTROLLER_ON_SIGNAL] == 0 ||
                    //    GbVar.IO[IODF.INPUT.DUST_CONTROLLER_OFF_SIGNAL] == 1)
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.DUST_CONTROLLER_RUN] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DUST_CONTROLLER_RUN, true);

                    //    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return (int)ERDF.E_NONE; // TODO : ALARM 작업해야함 작성자 : HEP
                    //    return FNC.BUSY;
                    //}
                    if (RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaRepeatCount == 0)
                    {
                        NextSeq((int)CYCLE_PLASMA_1_CONTINUOUS.MOVE_RETURN_Z_POS);
                        return FNC.BUSY;
                    }
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.FIRST_PLASMA, true);
                    }
                    break;

                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_PLASMA_READY_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }

                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }

                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_CLEANER_PICKER_PLASMA_SAFETY_POS_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }

                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                //[2022.05.12 작성자 : 홍은표] 딜레이를 줄이고자 미리 플라즈마를 동작한다.
                case CYCLE_PLASMA_1_CONTINUOUS.TURN_ON_PLASMA:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_USE].bOptionUse) break;

                    //TurnOnOffPlasma(true);
                    TurnOnPlasma(RecipeMgr.Inst.Rcp.cleaning.eFirstPlasmaOutput[m_nFirstPlasmaRepeatCount]);
                    break;

                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_PLASMA_START_X_POS:
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_X].dVel[POSDF.PLASMA_FIRST_PLASMA_START], 0);
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_PLASMA_START_Y_POS:
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_Y].dVel[POSDF.PLASMA_FIRST_PLASMA_START], 0);
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_PLASMA_START_Z_POS:
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Z, RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaZPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_Z].dVel[POSDF.PLASMA_FIRST_PLASMA_START], 100);
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.DELAY_PLASMA:
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_START_WAIT_TIME].lValue)) return FNC.BUSY;
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.CONTI_MOVE:
                    if (RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode < 4)
                    {
                        nFuncResult = ContinueMultiMove(0);
                    }
                    else
                    {
                        nFuncResult = ContinueMultiMove(0, true);
                    }
                    break;

                case CYCLE_PLASMA_1_CONTINUOUS.PICKER_PLASMA_END:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        swTimeStamp.Restart();
                    }
                    if (swTimeStamp.ElapsedMilliseconds > 100000)
                    {
                        //CleanerPickerBlowOff();
                        CleanerPickerVacuumOff();

                        TurnOnOffPlasma(false);
                        return (int)ERDF.E_NONE; // TODO : ALARM 작업해야함 작성자 : HEP //ERDF.E_PLASMA_SHOT_TIMEOUT
                    }
                    uint uMotionValue = 0;
                    CAXM.AxmContiIsMotion(0, ref uMotionValue);
                    if (uMotionValue == 1) return FNC.BUSY;
                    if (MotionMgr.Inst[SVDF.AXES.CL_DRY_X].IsBusy() ||
                        MotionMgr.Inst[SVDF.AXES.CL_DRY_Y].IsBusy()) return FNC.BUSY;

                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue)) return FNC.BUSY;
                    //CleanerPickerBlowOff();
                    CleanerPickerVacuumOff();

                    TurnOnOffPlasma(false);
                    m_nFirstPlasmaRepeatCount++;
                    if (m_nFirstPlasmaRepeatCount < RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaRepeatCount)
                    {
                        NextSeq((int)CYCLE_PLASMA_1_CONTINUOUS.MOVE_PLASMA_START_X_POS);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_RETURN_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_RETURN_Y_POS:
                    nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.MOVE_RETURN_X_POS:
                    nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_1_CONTINUOUS.FINISH:
                    m_nFirstPlasmaRepeatCount = 0;
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.FIRST_PLASMA, false);
                    return FNC.SUCCESS;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int LoadDryTable()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_PLACE_DRY_STAGE)m_nSeqNo)
            {
                case CYCLE_PLACE_DRY_STAGE.NONE:
                    break;
                case CYCLE_PLACE_DRY_STAGE.WAIT_DRY_STAGE:
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.UNLOAD_DRY_BLOCK, true);
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.BEFORE_MOVE_CHECK_STRIP_INFO:
                    {
                        if (GbVar.Seq.sUnitDry.Info.IsStrip() &&     // 드라이 테이블에 자재가 있고
                           !GbVar.Seq.sCleaning[2].Info.IsStrip())   // 클리너 피커에 자재가 없으면
                        {
                            if (AirStatus(STRIP_MDL.UNIT_DRY) == AIRSTATUS.VAC)
                            {
                                // 시퀀스 번호  변경 시 주의
                                NextSeq((int)CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_READY_Z_MOVE);
                                return FNC.BUSY;
                            }
                            else
                            {
                                nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_DRY_BLOCK_STRIP_INFO;
                            }
                        }

                        // 알람 발생 후 해당 위치에서 다시 배큠 체크 하기 위한 작업 
                        // 클리너 피커 스트립 정보가 있음
                        if (GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            //유닛 피커 위치가 내려 놓는 위치임
                            if (IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_TABLE) &&
                                IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_TABLE))
                            {
                                NextSeq((int)CYCLE_PLACE_DRY_STAGE.PLACE_DRY_STAGE_VAC_ON);
                                return FNC.BUSY;
                            }
                        }
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.DRY_PICKER_SAFETY_POS_Z_MOVE:
                    if(!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                        }
                        //드라이 피커 안전 위치 이동
                        nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.DRY_PICKER_SAFETY_POS_Y_MOVE:
                    if (!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryY(POSDF.PLASMA_DRY_READY)) break;
                        }
                        //드라이 피커 안전 위치 이동
                        nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.DRY_PICKER_SAFETY_POS_X_MOVE:
                    if (!m_bAutoMode)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosDryX(POSDF.PLASMA_DRY_READY)) break;
                        }
                        // 드라이 피커 안전 위치 이동
                        nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    }
                    break;
					
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_SAFETY_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE) ||
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA) ||
                            IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_TABLE)) break;
                    }
                    //if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_SAFETY_X_MOVE:
                    //X를 바로 드라이 테이블로 보냄
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_TABLE)) break;
                        if (IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break; //2022.05.23 홍은표 T축이 2차초음파위치이면 R축 먼저 돌리려고 break;
                    }
                    //if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_TABLE);
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_SAFETY_R_MOVE:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerR(POSDF.CLEANER_PICKER_DRY_TABLE)) break;
                    }
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_DRY_TABLE, 100);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //2022.05.23 홍은표 T축 돌렸는데 X축이 드라이 테이블 위치가 아니면
                        if (!IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_TABLE))
                        {
                            NextSeq((int)CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_SAFETY_X_MOVE);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.CHECK_STRIP_INFO_PLACE:
                    {
                        //오토런
                        if (m_bAutoMode)
                        {
                            // ------- 스크랩 처리 -------
                            // 클리너 피커에 자재가 없으면
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                //OUTPUT이 정상적으로 꺼져있으면 종료
                                if (AirStatus(STRIP_MDL.CLEANER_PICKER) == AIRSTATUS.NONE)
                                {
                                    NextSeq((int)CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_READY_Z_MOVE);
                                    return FNC.BUSY;
                                }
                                else
                                {
                                    nFuncResult = (int)ERDF.E_INTL_THERE_IS_NO_CLEANER_PK_STRIP_INFO;
                                }
                            }
                        }
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.PLACE_DRY_STAGE_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_LOADING)) break;
                    }
                    nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_LOADING);
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_DRY_STAGE_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_DRY_TABLE)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_DRY_TABLE);
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_DRY_STAGE_Z_PRE_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_TABLE)) break;
                        }
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_TABLE, -dOffset);
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_DRY_STAGE_Z_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_DRY_TABLE)) break;
                        }
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_TABLE, 0, true);
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.PLACE_DRY_STAGE_VAC_ON:
                    nFuncResult = DryBlockWorkVac(true,false,10);
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_BLOW_ON:
                    nFuncResult = CleanerPickerBlowOn(false);
                    break;
                case CYCLE_PLACE_DRY_STAGE.PLACE_DRY_STAGE_VAC_ON_CHECK:
                    {
                        nFuncResult = DryBlockWorkVac(true,true);
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_READY_Z_PRE_MOVE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_DRY_TABLE, -dOffset, true);
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_READY_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_PLACE_DRY_STAGE.CLEANER_PICKER_BLOW_OFF:
                    if(m_bFirstSeqStep)
                    {
                        CleanerPickerBlowOff();
                    }
                    // ------- 스크랩 처리 -------
                    // 클리너 피커에 자재가 없으면
                    if (!GbVar.Seq.sUnitDry.Info.IsStrip())
                    {
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_READY);
                    }
                    break;
                case CYCLE_PLACE_DRY_STAGE.FINISH:
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.UNLOAD_DRY_BLOCK, false);

                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        //public int SecondDry()
        //{
        //    if (m_nSeqNo != m_nPreSeqNo)
        //    {
        //        ResetCmd();
        //    }
        //    m_nPreSeqNo = m_nSeqNo;

        //    nFuncResult = FNC.SUCCESS;
        //    switch ((CYCLE_DRY_2)m_nSeqNo)
        //    {
        //        case CYCLE_DRY_2.NONE:
        //            break;
        //        case CYCLE_DRY_2.SECOND_MOVE_DRY_STAGE_START_POS:
        //            nFuncResult = MovePosDryBlockStgX(POSDF.CLEANER_SECOND_DRY_START);
        //            break;
        //        case CYCLE_DRY_2.SECOND_MOVE_DRY_PICKER_START_POS:
        //            nFuncResult = MovePosDryX(POSDF.CLEANER_SECOND_DRY_START);
        //            break;
        //        case CYCLE_DRY_2.SECOND_DRY_BLOW_ON:
        //            //TODO : 열풍기 동작 방법
        //            //MotionMgr.Inst.SetAOutput()
        //            break;
        //        case CYCLE_DRY_2.SECOND_MOVE_DRY_END_POS:
        //            nFuncResult = MovePosDryBlockStgX(POSDF.CLEANER_SECOND_DRY_START);
        //            break;
        //        case CYCLE_DRY_2.SECOND_DRY_BLOW_OFF:
        //            //TODO : 열풍기 동작 방법
        //            //MotionMgr.Inst.SetAOutput()
        //            break;
        //        case CYCLE_DRY_2.SECOND_CHECK_DRY_COUNT:
        //            if (RecipeMgr.Inst.Rcp.cleaning.nSecondDryBlowCount > m_nSecondDryCount)
        //            {
        //                m_nSecondDryCount++;
        //                NextSeq((int)CYCLE_DRY_2.SECOND_MOVE_DRY_PICKER_START_POS);
        //                return FNC.BUSY;
        //            }
        //            else
        //            {
        //                m_nSecondDryCount = 0;
        //                break;
        //            }
        //            break;
        //        case CYCLE_DRY_2.SECOND_MOVE_DRY_SAFETY_POS_MOVE:
        //            break;
        //        case CYCLE_DRY_2.FINISH:
        //            return FNC.SUCCESS;
        //            break;
        //        default:
        //            break;
        //    }
        //    #region AFTER SWITCH

        //    if (m_bFirstSeqStep)
        //    {
        //        // Position Log
        //        if (string.IsNullOrEmpty(m_strMotPos) == false)
        //        {
        //            SeqHistory(m_strMotPos);
        //        }

        //        m_bFirstSeqStep = false;
        //    }

        //    if (FNC.IsErr(nFuncResult))
        //    {
        //        return nFuncResult;
        //    }
        //    else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

        //    m_nSeqNo++;

        //    if (m_nSeqNo > 10000)
        //    {
        //        System.Diagnostics.Debugger.Break();
        //        FINISH = true;
        //        return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
        //    }

        //    return FNC.BUSY;
        //    #endregion
        //}
        public int SecondPlasma()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_PLASMA_2)m_nSeqNo)
            {
                case CYCLE_PLASMA_2.NONE:
                    break;
                case CYCLE_PLASMA_2.ALARM_RESET_AND_ALARM_CHECK:
                    m_nSecondPitchCount = 0;
                    m_nSecondPlasmaRepeatCount = 0;
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        swTimeStamp.Restart();
                    }
                    if (GbVar.IO[IODF.INPUT.PLASMA_ALARM_SIGNAL] == 1)
                    {
                        if (GbVar.IO[IODF.OUTPUT.PLASMA_ALARM_RESET] == 0)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_ALARM_RESET, true);

                        if (swTimeStamp.ElapsedMilliseconds > 10000) return (int)ERDF.E_NONE; // TODO : ALARM 작업해야함 작성자 : HEP
                        return FNC.BUSY;
                    }
                    else
                    {
                        if(RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaRepeatCount == 0)
                        {
                            NextSeq((int)CYCLE_PLASMA_2.MOVE_RETURN_Z_POS);
                            return FNC.BUSY;
                        }
                        break;
                    }
                    break;
                case CYCLE_PLASMA_2.INIT_CYLINDER:
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL, false);
                    break;
                case CYCLE_PLASMA_2.MOVE_CLEANER_SAFETY_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE); //2차 플라즈마
                    break;
                case CYCLE_PLASMA_2.MOVE_CLEANER_PLASMA_X_POS:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    
                    break;
                case CYCLE_PLASMA_2.MOVE_CLEANER_PLASMA_T_POS:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_PLASMA, 100);
                    break;
                case CYCLE_PLASMA_2.MOVE_CLEANER_PLASMA_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    break;
                case CYCLE_PLASMA_2.MOVE_PLASMA_READY_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                        if (IsInPosDryZ(POSDF.PLASMA_SECOND_PLASMA_START)) break;
                    }

                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2.MOVE_PLASMA_START_X_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryX(POSDF.PLASMA_SECOND_PLASMA_START)) break;
                    }
                    nFuncResult = MovePosDryX(POSDF.PLASMA_SECOND_PLASMA_START);
                    break;
                case CYCLE_PLASMA_2.MOVE_PLASMA_START_Y_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryY(POSDF.PLASMA_SECOND_PLASMA_START)) break;
                    }
                    nFuncResult = MovePosDryY(POSDF.PLASMA_SECOND_PLASMA_START);
                    break;
                case CYCLE_PLASMA_2.MOVE_PLASMA_START_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_SECOND_PLASMA_START)) break;
                    }
                    nFuncResult = MovePosDryZ(POSDF.PLASMA_SECOND_PLASMA_START);
                    break;
                case CYCLE_PLASMA_2.TURN_ON_PLASMA:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_USE].bOptionUse) break;
                    TurnOnOffPlasma(true);
                    break;
                case CYCLE_PLASMA_2.DELAY_PLASMA:
                    if (m_bFirstSeqStep)
                    {
                        swTimeStamp.Restart();
                        m_bFirstSeqStep = false;
                    }
                    if (swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_START_WAIT_TIME].nValue) break;
                    return FNC.BUSY;
                case CYCLE_PLASMA_2.MOVE_PLASMA_CHECK:
                    if (RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode < 4)
                    {
                        #region Y를 길게 움직임
                        double dChkPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos + RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStepPitch * m_nSecondPitchCount;
                        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
                        {
                            if (dChkPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
                            {
                                if (m_bFirstSeqStep)
                                {
                                    swTimeStamp.Restart();
                                    m_bFirstSeqStep = false;
                                }
                                if (swTimeStamp.ElapsedMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue) return FNC.BUSY;

                                TurnOnOffPlasma(false);

                                m_nSecondPlasmaRepeatCount++;
                                //플라즈마 반복 체크
                                if (m_nSecondPlasmaRepeatCount >= RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaRepeatCount)
                                {
                                    m_nSecondPitchCount = 0;
                                    m_nSecondPlasmaRepeatCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_2.MOVE_RETURN_Z_POS);
                                }
                                else
                                {
                                    m_nSecondPitchCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_START_X_POS);
                                }
                                return FNC.BUSY;
                            }
                            else
                            {
                                NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_X_MOVE);
                                return FNC.BUSY;
                            }
                        }
                        else
                        {
                            if (dChkPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
                            {
                                if (m_bFirstSeqStep)
                                {
                                    swTimeStamp.Restart();
                                    m_bFirstSeqStep = false;
                                }
                                if (swTimeStamp.ElapsedMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue) return FNC.BUSY;

                                TurnOnOffPlasma(false);

                                m_nSecondPlasmaRepeatCount++;
                                //플라즈마 반복 체크
                                if (m_nSecondPlasmaRepeatCount >= RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaRepeatCount)
                                {
                                    m_nSecondPitchCount = 0;
                                    m_nSecondPlasmaRepeatCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_2.MOVE_RETURN_Z_POS);
                                }
                                else
                                {
                                    m_nSecondPitchCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_START_X_POS);
                                }
                                return FNC.BUSY;
                            }
                            else
                            {
                                NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_X_MOVE);
                                return FNC.BUSY;
                            }

                        }
                        #endregion

                    }
                    else
                    {
                        #region X를 길게 움직임
                        double dChkPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos + RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStepPitch * m_nSecondPitchCount;
                        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
                        {
                            if (dChkPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
                            {
                                if (m_bFirstSeqStep)
                                {
                                    swTimeStamp.Restart();
                                    m_bFirstSeqStep = false;
                                }
                                if (swTimeStamp.ElapsedMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue) return FNC.BUSY;

                                TurnOnOffPlasma(false);

                                m_nSecondPlasmaRepeatCount++;
                                //플라즈마 반복 체크
                                if (m_nSecondPlasmaRepeatCount >= RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaRepeatCount)
                                {
                                    m_nSecondPitchCount = 0;
                                    m_nSecondPlasmaRepeatCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_2.MOVE_RETURN_Z_POS);
                                }
                                else
                                {
                                    m_nSecondPitchCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_START_X_POS);
                                }
                                return FNC.BUSY;
                            }
                            else
                            {
                                NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_Y_MOVE);
                                return FNC.BUSY;
                            }
                        }
                        else
                        {
                            if (dChkPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
                            {
                                if (m_bFirstSeqStep)
                                {
                                    swTimeStamp.Restart();
                                    m_bFirstSeqStep = false;
                                }
                                if (swTimeStamp.ElapsedMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue) return FNC.BUSY;

                                TurnOnOffPlasma(false);

                                m_nSecondPlasmaRepeatCount++;
                                //플라즈마 반복 체크
                                if (m_nSecondPlasmaRepeatCount >= RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaRepeatCount)
                                {
                                    m_nSecondPitchCount = 0;
                                    m_nSecondPlasmaRepeatCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_2.MOVE_RETURN_Z_POS);
                                }
                                else
                                {
                                    m_nSecondPitchCount = 0;
                                    NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_START_X_POS);
                                }
                                return FNC.BUSY;
                            }
                            else
                            {
                                NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_Y_MOVE);
                                return FNC.BUSY;
                            }
                        }
                        #endregion
                    }
                    break;

                #region Y를 길게 움직임
                case CYCLE_PLASMA_2.MOVE_PLASMA_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        m_dPlasmaXPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos + RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStepPitch * m_nSecondPitchCount;
                    }

                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, m_dPlasmaXPos, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel, 0);
                    break;

                case CYCLE_PLASMA_2.MOVE_PLASMA_Y_MOVE_CHECK:
                    if (IsInPosDryY(RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos))
                    {
                        NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_Y_END_MOVE);
                        return FNC.BUSY;
                    }
                    else
                    {
                        NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_Y_START_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_PLASMA_2.MOVE_PLASMA_Y_END_MOVE:
                    m_dPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos;
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, m_dPos, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel, 0);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq((int)CYCLE_PLASMA_2.TURN_OFF_PLASMA);
                        return FNC.BUSY;
                    }
                    break;

                case CYCLE_PLASMA_2.MOVE_PLASMA_Y_START_MOVE:
                    m_dPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos;
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, m_dPos, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel, 0);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq((int)CYCLE_PLASMA_2.TURN_OFF_PLASMA);
                        return FNC.BUSY;
                    }
                    break;
                #endregion

                #region X를 길게 움직임
                case CYCLE_PLASMA_2.MOVE_PLASMA_Y_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        m_dPlasmaYPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos + RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStepPitch * m_nSecondPitchCount;
                    }

                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, m_dPlasmaYPos, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel, 0);
                    break;


                case CYCLE_PLASMA_2.MOVE_PLASMA_X_MOVE_CHECK:
                    if (IsInPosDryX(RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos))
                    {
                        NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_X_END_MOVE);
                        return FNC.BUSY;
                    }
                    else
                    {
                        NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_X_START_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_PLASMA_2.MOVE_PLASMA_X_END_MOVE:
                    m_dPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos;
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, m_dPos, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel, 0);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq((int)CYCLE_PLASMA_2.TURN_OFF_PLASMA);
                        return FNC.BUSY;
                    }
                    break;

                case CYCLE_PLASMA_2.MOVE_PLASMA_X_START_MOVE:
                    m_dPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos;
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, m_dPos, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel, 0);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq((int)CYCLE_PLASMA_2.TURN_OFF_PLASMA);
                        return FNC.BUSY;
                    }
                    break;
                #endregion

                case CYCLE_PLASMA_2.TURN_OFF_PLASMA:
                    //TurnOnOffPlasma(false);
                    break;
                case CYCLE_PLASMA_2.RETURN_CHECK:
                    m_nSecondPitchCount++;
                    NextSeq((int)CYCLE_PLASMA_2.MOVE_PLASMA_CHECK);
                    return FNC.BUSY;
                case CYCLE_PLASMA_2.MOVE_RETURN_Z_POS:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2.MOVE_RETURN_START_POS:
                    nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2.FINISH:
                    nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    if(nFuncResult == FNC.SUCCESS)
                    {
                        //if (MotionMgr.Inst[SVDF.AXES.CL_DRY_X].IsBusy() ||
                        //   MotionMgr.Inst[SVDF.AXES.CL_DRY_Y].IsBusy()) return FNC.BUSY;
                        TurnOnOffPlasma(false);

                        return FNC.SUCCESS;
                    }
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SecondPlasma_Continuous()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_PLASMA_2_CONTINUOUS)m_nSeqNo)
            {
                case CYCLE_PLASMA_2_CONTINUOUS.NONE:
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.INTERLOCK_FLAG_CHECK:
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            NextSeq((int)CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_SAFETY_Z_MOVE);
                            return FNC.BUSY;
                        }
                    }

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (GbVar.IO[IODF.INPUT.PLASMA_ALARM_SIGNAL] == 1 ||
                        GbVar.IO[IODF.INPUT.PLASMA_READY_SIGNAL] == 0)
                    {
                        if (GbVar.IO[IODF.OUTPUT.PLASMA_ALARM_RESET] == 0)
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_ALARM_RESET, true);

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return 999; // TODO : ALARM 작업해야함 작성자 : HEP
                        return FNC.BUSY;
                    }
                    //if (GbVar.IO[IODF.INPUT.DUST_CONTROLLER_ON_SIGNAL] == 0 ||
                    //    GbVar.IO[IODF.INPUT.DUST_CONTROLLER_OFF_SIGNAL] == 1)
                    //{
                    //    if (GbVar.IO[IODF.OUTPUT.DUST_CONTROLLER_RUN] == 0)
                    //        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DUST_CONTROLLER_RUN, true);

                    //    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue)) return (int)ERDF.E_NONE; // TODO : ALARM 작업해야함 작성자 : HEP
                    //    return FNC.BUSY;
                    //}
                    if (RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaRepeatCount == 0)
                    {
                        NextSeq((int)CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_SAFETY_Z_MOVE);
                        return FNC.BUSY;
                    }
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_PLASMA, true);
                    }
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_Z_READY_MOVE:
                    m_nSecondPlasmaRepeatCount = 0;

                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_Y_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosDryY(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_X_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosDryX(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.CLEANER_PICKER_Z_SAFETY_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SAFETY_MOVE);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.CLEANER_PICKER_X_PICKER_PLASMA_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.CLEANER_PICKER_R_PICKER_PLASMA_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerR(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_PLASMA, 100);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.CLEANER_PICKER_Z_PLASMA_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (IsInPosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA)) break;
                    }
                    nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_SECOND_PLASMA);
                    break;
                //[2022.05.12 작성자 : 홍은표] 딜레이를 줄이고자 X축 이동 후 플라즈마를 동작한다.
                case CYCLE_PLASMA_2_CONTINUOUS.PICKER_PLASMA_SHOT:
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_USE].bOptionUse) break;
                    //TurnOnOffPlasma(true);
                    TurnOnPlasma(RecipeMgr.Inst.Rcp.cleaning.eSecondPlasmaOutput[m_nSecondPlasmaRepeatCount]);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_X_PICKER_PLASMA_MOVE:
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_X, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_X].dVel[POSDF.PLASMA_SECOND_PLASMA_START], 0);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_Y_PICKER_PLASMA_MOVE:
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Y, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_Y].dVel[POSDF.PLASMA_SECOND_PLASMA_START], 0);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_Z_PICKER_PLASMA_MOVE:
                    nFuncResult = AxisMovePos((int)SVDF.AXES.CL_DRY_Z, RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaZPos, TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_Z].dVel[POSDF.PLASMA_SECOND_PLASMA_START], 100);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.PICKER_PLASMA_WAIT:
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_START_WAIT_TIME].nValue)) return FNC.BUSY;
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.PICKER_PLASMA_START:
                    if (RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode < 4)
                    {
                        nFuncResult = ContinueMultiMove(1);
                    }
                    else
                    {
                        nFuncResult = ContinueMultiMove(1, true);
                    }
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.PICKER_PLASMA_END:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        swTimeStamp.Restart();
                    }
                    if (swTimeStamp.ElapsedMilliseconds > 100000)
                    {
                        TurnOnOffPlasma(false);
                        return (int)ERDF.E_PLASMA_SHOT_TIMEOUT; // TODO : ALARM 작업해야함 작성자 : HEP //ERDF.E_PLASMA_SHOT_TIMEOUT
                    }
                    uint uMotionValue = 0;
                    CAXM.AxmContiIsMotion(0, ref uMotionValue);
                    if (uMotionValue == 1) return FNC.BUSY;
                    if (MotionMgr.Inst[SVDF.AXES.CL_DRY_X].IsBusy() ||
                        MotionMgr.Inst[SVDF.AXES.CL_DRY_Y].IsBusy()) return FNC.BUSY;

                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PLASMA_END_WAIT_TIME].nValue)) return FNC.BUSY;

                    TurnOnOffPlasma(false);
                    m_nSecondPlasmaRepeatCount++;
                    if (m_nSecondPlasmaRepeatCount < RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaRepeatCount)
                    {
                        NextSeq((int)CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_X_PICKER_PLASMA_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_SAFETY_Z_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosDryZ(POSDF.PLASMA_DRY_READY)) break;
                    }
                    nFuncResult = MovePosDryZ(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_SAFETY_Y_MOVE:
                    nFuncResult = MovePosDryY(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.DRY_PICKER_SAFETY_X_MOVE:
                    nFuncResult = MovePosDryX(POSDF.PLASMA_DRY_READY);
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.CL_PICKER_SAFETY_Z_MOVE:
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            nFuncResult = MovePosCleanerZ(POSDF.CLEANER_PICKER_READY);
                        }
                    }
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.CL_PICKER_SAFETY_X_MOVE:
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                        }
                    }
                    //if (m_bAutoMode) break;
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.CL_PICKER_SAFETY_R_MOVE:
                    if (m_bAutoMode)
                    {
                        if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                        {
                            nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_CLEAN);
                        }
                    }
                    break;
                case CYCLE_PLASMA_2_CONTINUOUS.FINISH:
                    TTDF.SetTact((int)TTDF.CYCLE_NAME.SECOND_PLASMA, false);

                    return FNC.SUCCESS;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        public int SorterUnload()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SORTER_UNLOAD)m_nSeqNo)
            {
                case CYCLE_SORTER_UNLOAD.NONE:
                    break;
                case CYCLE_SORTER_UNLOAD.CHECK_SORTER_SAFETY:
                    //인터락 확인
                    break;
                case CYCLE_SORTER_UNLOAD.UNLOAD_SORTER_MOVE:
                    nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_UNLOADING);
                    break;
                case CYCLE_SORTER_UNLOAD.WAIT_UNLOAD_COMPLETE:
                    //인터락
                    break;
                case CYCLE_SORTER_UNLOAD.FINISH:
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region 내부 함수
        public int ContinueMultiMove(int nSelectNo, bool bChangeXY = false)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nAxis = new int[2];
            double[] dPos = new double[2];
            uint nPosSize = 2;
            int nCoordinate = 0;
            double dVelocity = 20, dAccel = 40;
            nAxis[0] = (int)SVDF.AXES.CL_DRY_X;
            nAxis[1] = (int)SVDF.AXES.CL_DRY_Y;

            double dStartPosX = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
            double dStartPosY = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
            double dEndPosX = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos;
            double dEndPosY = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos;
            double dPitch = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStepPitch;
            int nStep = RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaStepCount;
            dVelocity = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel;
            dAccel = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel * 10;


            if (nSelectNo == 0)
            {
                dStartPosX = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
                dStartPosY = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
                dEndPosX = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos;
                dEndPosY = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos;
                dPitch = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStepPitch;
                nStep = RecipeMgr.Inst.Rcp.cleaning.nFirstPlasmaStepCount;
                dVelocity = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel;
                dAccel = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel * 10;
            }
            else if (nSelectNo == 1)
            {
                dStartPosX = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos;
                dStartPosY = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos;
                dEndPosX = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos;
                dEndPosY = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos;
                dPitch = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStepPitch;
                nStep = RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaStepCount;
                dVelocity = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel;
                dAccel = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel * 10;
            }
            else
            {
                dStartPosX = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos;
                dStartPosY = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos;
                dEndPosX = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos;
                dEndPosY = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos;
                dPitch = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStepPitch;
                nStep = RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaStepCount;
                dVelocity = RecipeMgr.Inst.Rcp.cleaning.dCleanerPickerPlasmaVel;
                dAccel = RecipeMgr.Inst.Rcp.cleaning.dCleanerPickerPlasmaVel * 10;
            }

            #region 아진 연속 보간 작업 설정
            CAXM.AxmContiWriteClear(nCoordinate);
            CAXM.AxmContiSetAxisMap(nCoordinate, nPosSize, nAxis);

            //절대 좌표 위치 구동 등록//0이면 절대, 1이면 상대
            CAXM.AxmContiSetAbsRelMode(nCoordinate, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            CAXM.AxmContiBeginNode(nCoordinate);
            #endregion

            if (bChangeXY == false)
            {
                for (int nIdx = 0; nIdx < nStep + 1; nIdx++)
                {
                    int nDir = nIdx % 2 == 0 ? 1 : 0;

                    //길게 이동
                    dPos[0] = dStartPosX + (nIdx * dPitch);
                    dPos[1] = nDir == 0 ? dStartPosY : dEndPosY;
                    CAXM.AxmLineMove(nCoordinate, dPos, dVelocity, dAccel, dAccel);

                    //스텝 이동
                    dPos[0] = dStartPosX + ((nIdx + 1) * dPitch);
                    dPos[1] = nDir == 0 ? dStartPosY : dEndPosY;
                    if (nIdx != nStep)
                    {
                        CAXM.AxmLineMove(nCoordinate, dPos, dVelocity, dAccel, dAccel);
                    }
                }
            }
            else
            {
                for (int nIdx = 0; nIdx < nStep + 1; nIdx++)
                {
                    int nDir = nIdx % 2 == 0 ? 1 : 0;

                    //길게 이동
                    dPos[0] = nDir == 0 ? dStartPosX : dEndPosX;
                    dPos[1] = dStartPosY + (nIdx * dPitch);
                    CAXM.AxmLineMove(nCoordinate, dPos, dVelocity, dAccel, dAccel);

                    //스텝 이동
                    dPos[0] = nDir == 0 ? dStartPosX : dEndPosX;
                    dPos[1] = dStartPosY + ((nIdx + 1) * dPitch);
                    if (nIdx != nStep)
                    {
                        CAXM.AxmLineMove(nCoordinate, dPos, dVelocity, dAccel, dAccel);
                    }
                }
            }

            


            #region 아진 연속 보간 작업 설정
            CAXM.AxmContiEndNode(nCoordinate);
            #endregion

            uint uRet = CAXM.AxmContiStart(nCoordinate, 0, 0);
            if (uRet != 0)
            {
                return (int)uRet;
            }
            return FNC.SUCCESS;
        }

        public int ContinueMultiMoveCleanPicker()
        {
            int[] nAxis = new int[2];
            double[] dPos = new double[2];
            uint nPosSize = 2;
            int nCoordinate = 0;
            double dVelocity = 20, dAccel = 40;
            nAxis[0] = (int)SVDF.AXES.CL_DRY_X;
            nAxis[1] = (int)SVDF.AXES.CL_DRY_Y;


            //double dStartPosX = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
            //double dStartPosY = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
            double dStartPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_X].dPos[POSDF.PLASMA_SECOND_PLASMA_START];
            double dStartPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_Y].dPos[POSDF.PLASMA_SECOND_PLASMA_START];
            double dEndPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_X].dPos[POSDF.PLASMA_SECOND_PLASMA_END];
            double dEndPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_DRY_Y].dPos[POSDF.PLASMA_SECOND_PLASMA_END];
            double dPitch = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStepPitch;
            int nStep = RecipeMgr.Inst.Rcp.cleaning.nSecondPlasmaStepCount;
            dVelocity = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel;
            dAccel = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel * 10;

            #region 아진 연속 보간 작업 설정
            CAXM.AxmContiWriteClear(nCoordinate);
            CAXM.AxmContiSetAxisMap(nCoordinate, nPosSize, nAxis);

            //절대 좌표 위치 구동 등록//0이면 절대, 1이면 상대
            CAXM.AxmContiSetAbsRelMode(nCoordinate, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            CAXM.AxmContiBeginNode(nCoordinate);
            #endregion


            for (int nIdx = 0; nIdx < nStep + 1; nIdx++)
            {
                int nDir = nIdx % 2 == 0 ? 1 : 0;

                //길게 이동
                dPos[0] = dStartPosX + (nIdx * dPitch);
                dPos[1] = nDir == 1 ? dStartPosY : dEndPosY;
                CAXM.AxmLineMove(nCoordinate, dPos, dVelocity, dAccel, dAccel);

                //스텝 이동
                dPos[0] = dStartPosX + ((nIdx + 1) * dPitch);
                dPos[1] = nDir == 1 ? dStartPosY : dEndPosY;
                if (nIdx != nStep)
                {
                    CAXM.AxmLineMove(nCoordinate, dPos, dVelocity, dAccel, dAccel);
                }
            }


            #region 아진 연속 보간 작업 설정
            CAXM.AxmContiEndNode(nCoordinate);
            #endregion

            uint uRet = CAXM.AxmContiStart(nCoordinate, 0, 0);
            if (uRet != 0)
            {
                return (int)uRet;
            }
            return FNC.SUCCESS;
        }
        public void TurnOnOffPlasma(bool bOn = true)
        {
            if (bOn)
            {
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_START] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_START, true);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_STOP] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_STOP, false);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR, true);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR, true);

                if(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.HEATING_MODE].bOptionUse)
                {
                    if (GbVar.IO[IODF.OUTPUT.HEATING_POWER_ON_SIGNAL] == 0)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, true);
                    if (GbVar.IO[IODF.OUTPUT.PLASMA_HEATING_AIR_SOL] == 0)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEATING_AIR_SOL, true);
                }
                //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.HEATING_REGULATOR].dAnalogOutputValue);
            }
            else
            {
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_START] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_START, false);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_STOP] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_STOP, true);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR, false);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR, false);

                if (GbVar.IO[IODF.OUTPUT.HEATING_POWER_ON_SIGNAL] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, false);

                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEATING_AIR_SOL] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEATING_AIR_SOL, false);
                //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, 0);
            }
        }

        public void TurnOnPlasma(PLASMA_OUTPUT eMode)
        {
            // 플라즈마, 열풍기 같이
            if (eMode == PLASMA_OUTPUT.PLASMA_HEATING)
            {
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_START] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_START, true);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_STOP] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_STOP, false);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR, true);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR, true);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.HEATING_MODE].bOptionUse)
                {
                    if (GbVar.IO[IODF.OUTPUT.HEATING_POWER_ON_SIGNAL] == 0)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, true);
                    if (GbVar.IO[IODF.OUTPUT.PLASMA_HEATING_AIR_SOL] == 0)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEATING_AIR_SOL, true);
                }
                //MotionMgr.Inst.SetAOutput((int)IODF.A_OUTPUT.HEATING_CONTROLLER_REGULATOR, ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.HEATING_REGULATOR].dAnalogOutputValue);
            }
            // 플라즈마만
            else if (eMode == PLASMA_OUTPUT.PLASMA)
            {
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_START] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_START, true);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_STOP] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_STOP, false);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR, true);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR, true);

                if (GbVar.IO[IODF.OUTPUT.HEATING_POWER_ON_SIGNAL] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, false);
            }
            // 열풍기만
            else if (eMode == PLASMA_OUTPUT.HEATING)
            {
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_START] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_START, false);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_STOP] == 0)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_STOP, true);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_1_SELECTOR, false);
                if (GbVar.IO[IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR] == 1)
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEAD_2_SELECTOR, false);

                //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.HEATING_MODE].bOptionUse)
                {
                    if (GbVar.IO[IODF.OUTPUT.HEATING_POWER_ON_SIGNAL] == 0)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, true);
                    if (GbVar.IO[IODF.OUTPUT.PLASMA_HEATING_AIR_SOL] == 0)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEATING_AIR_SOL, true);
                }
            }
        }

        #endregion
    }
}
