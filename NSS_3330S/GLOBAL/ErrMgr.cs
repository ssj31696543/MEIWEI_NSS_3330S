using NSS_3330S;
using NSS_3330S.POP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;


namespace NSS_3330S
{
    public class ErrMgr
    {
        public List<ErrorInfo> errlist = new List<ErrorInfo>();
        private static readonly Lazy<ErrMgr> instance = new Lazy<ErrMgr>(() => new ErrMgr());

        public bool[] m_bLightAlarm = new bool[(int)ERDF.ERR_CNT];
        public int m_nLightAlarmCnt = 0;

        public List<ErrStruct> ErrStatus = new List<ErrStruct>();

        public static ErrMgr Inst
        {
            get
            {
                return instance.Value;
            }
        }
        private ErrMgr()
        {

        }
        ~ErrMgr()
        {
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.ENGLISH)
            {
                errlist = JSONHelper.Deserialize<List<ErrorInfo>>(new FileStream(PathMgr.Inst.FILE_PATH_ERROR_ENG, FileMode.OpenOrCreate));
            }
            else if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.KOREAN)
            {
                errlist = JSONHelper.Deserialize<List<ErrorInfo>>(new FileStream(PathMgr.Inst.FILE_PATH_ERROR, FileMode.OpenOrCreate));
            }
            else
            {
                errlist = JSONHelper.Deserialize<List<ErrorInfo>>(new FileStream(PathMgr.Inst.FILE_PATH_ERROR_CHN, FileMode.OpenOrCreate));
            }

            if (errlist == null)
            {
                errlist = new List<ErrorInfo>();
                for (int i = 0; i < (int)ERDF.ERR_CNT; i++)
                {
                    ErrorInfo eInfo = new ErrorInfo();

                    eInfo.NO = i;

                    if (i >= (int)ERDF.E_SV_AMP && i < (int)ERDF.E_SV_AMP + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_AMP.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_AMP];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_AMP] + "축 서보 앰프 알람 발생";
                        eInfo.SOLUTION = "서보앰프를 확인하여 발생한 알람 코드에 맞는 조치 방법을 취해주십시오.";
                    }
                    else if (i >= (int)ERDF.E_SV_MOVE && i < (int)ERDF.E_SV_MOVE + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_MOVE.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_MOVE];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_MOVE] + "축 움직임 실패";
                        eInfo.SOLUTION = SVDF.axisNames[i - (int)ERDF.E_SV_MOVE] + "축을 구동하는데 방해되는 장애물이 있는지 확인하십시오.";
                    }
                    else if (i >= (int)ERDF.E_SV_NOT_HOME && i < (int)ERDF.E_SV_NOT_HOME + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_NOT_HOME.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_NOT_HOME];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_NOT_HOME] + "축 초기화 하지 않음";
                        eInfo.SOLUTION = SVDF.axisNames[i - (int)ERDF.E_SV_NOT_HOME] + "축의 원점을 잡은 후 구동하십시오.";
                    }
                    else if (i >= (int)ERDF.E_SV_HW_LIMIT_P && i < (int)ERDF.E_SV_HW_LIMIT_P + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_HW_LIMIT_P.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_HW_LIMIT_P];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_HW_LIMIT_P] + "축 플러스 리밋 감지";
                        eInfo.SOLUTION = "이동위치가 잘못 입력되진 않았는지 확인하시고 플러스 리밋이 해제 될때까지 이동하여 주십시오";
                    }
                    else if (i >= (int)ERDF.E_SV_HW_LIMIT_N && i < (int)ERDF.E_SV_HW_LIMIT_N + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_HW_LIMIT_N.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_HW_LIMIT_N];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_HW_LIMIT_N] + "축 마이너스 리밋 감지";
                        eInfo.SOLUTION = "이동위치가 잘못 입력되진 않았는지 확인하시고 마이너스 리밋이 해제 될때까지 이동하여 주십시오";
                    }
                    else if (i >= (int)ERDF.E_SV_MOVE_INTERLOCK && i < (int)ERDF.E_SV_MOVE_INTERLOCK + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_MOVE_INTERLOCK.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_MOVE_INTERLOCK];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_MOVE_INTERLOCK] + "축 구동중 인터락 발생";
                        eInfo.SOLUTION = "";
                    }
                    else if (i >= (int)ERDF.E_SV_INVALID_VEL && i < (int)ERDF.E_SV_INVALID_VEL + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_INVALID_VEL.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_INVALID_VEL];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_INVALID_VEL] + "축 잘못된 구동 속도";
                        eInfo.SOLUTION = "지정할 수 없는 값 혹은 너무 크거나 작은 속도값이 설정되어있는것은 아닌지 확인하여 주십시오.";
                    }
                    else if (i >= (int)ERDF.E_SV_SW_LIMIT_P && i < (int)ERDF.E_SV_SW_LIMIT_P + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_SW_LIMIT_P.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_SW_LIMIT_P];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_SW_LIMIT_P] + "축에 설정된 소프트웨어 플러스 리밋을 초과한 위치 지령";
                        eInfo.SOLUTION = "이동할 위치 값 또는 설정된 S/W 플러스 리밋 값을 확인하여 주십시오.";
                    }
                    else if (i >= (int)ERDF.E_SV_SW_LIMIT_N && i < (int)ERDF.E_SV_SW_LIMIT_N + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_SW_LIMIT_N.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_SW_LIMIT_N];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_SW_LIMIT_N] + "축에 설정된소프트웨어 마이너스 리밋을 초과한 위치 지령";
                        eInfo.SOLUTION = "이동할 위치 값 또는 설정된 S/W 마이너스 리밋 값을 확인하여 주십시오.";
                    }
                    else if (i >= (int)ERDF.E_SV_SERVO_ON && i < (int)ERDF.E_SV_SERVO_ON + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_SERVO_ON.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_SERVO_ON];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_SERVO_ON] + "축 서보 오프 상태";
                        eInfo.SOLUTION = "서보를 온해주십시오,";
                    }
                    else if (i >= (int)ERDF.E_SV_HOME && i < (int)ERDF.E_SV_HOME + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_HOME.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_HOME];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_HOME] + "축 서보 원점이동 실패";
                        eInfo.SOLUTION = SVDF.axisNames[i - (int)ERDF.E_SV_HOME] + "축을 서보 원점 이동시 방해되는 장애물이 있는지 확인하십시오.";
                    }
                    else if (i >= (int)ERDF.E_SV_MOT_INFO && i < (int)ERDF.E_SV_MOT_INFO + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_MOT_INFO.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_MOT_INFO];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_MOT_INFO] + "축 서보 모터 정보 오류";
                        eInfo.SOLUTION = SVDF.axisNames[i - (int)ERDF.E_SV_MOT_INFO] + "축을 서보 모터 정보를 확인 부탁 드립니다";
                    }
                    else if (i >= (int)ERDF.E_SV_HOME_INTERLOCK && i < (int)ERDF.E_SV_HOME_INTERLOCK + SVDF.SV_CNT)
                    {
                        eInfo.NAME = ERDF.E_SV_HOME_INTERLOCK.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_HOME_INTERLOCK];
                        eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_HOME_INTERLOCK] + "축 서보 원점이동 중 인터락 발생";
                        eInfo.SOLUTION = "";
                    }
                    else
                    {
                        if (Enum.IsDefined(typeof(ERDF), i))
                        {
                            eInfo.NAME = ((ERDF)i).ToString();
                        }
                        else
                        {
                            eInfo.NAME = "";
                        }
                        eInfo.CAUSE = "Please edit cause";
                        eInfo.SOLUTION = "Please edit solution";
                    }
                    eInfo.HEAVY = true;
                    eInfo.BUZZER = ErrorInfo.BUZZERLIST.BUZZER_END.ToString();

                    errlist.Add(eInfo);
                }
                if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.ENGLISH)
                {
                    JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR_ENG, FileMode.OpenOrCreate));
                }
                else if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.KOREAN)
                {
                    JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR, FileMode.OpenOrCreate));
                }
                else
                {
                    JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR_CHN, FileMode.OpenOrCreate));
                }
            }
            if (GbForm.fAlarm != null)
            {
                GbForm.fAlarm.Initdgv();
            }
        }

        public void Save()
        {
            if (GbVar.CurLoginInfo.USER_LEVEL == 2)
            {
                string str_msg = string.Format("ERDF 가 수정이 되었습니까?\n ");
                string str_title = "주의";
                popMessageBox messageBox = new popMessageBox(str_msg, str_title);
                if (messageBox.ShowDialog() == DialogResult.Cancel)
                {
                    try
                    {
                        if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.ENGLISH)
                        {
                            JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR_ENG, FileMode.OpenOrCreate));
                        }
                        else if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.KOREAN)
                        {
                            JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR, FileMode.OpenOrCreate));
                        }
                        else
                        {
                            JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR_CHN, FileMode.OpenOrCreate));
                        }
                    }
                    catch (Exception ex)
                    {
                        GbFunc.WriteExeptionLog(ex.ToString());
                    }
                    messageBox = new popMessageBox("저장되었습니다.", "알림", MessageBoxButtons.OK);
                    messageBox.ShowDialog();
                }
                else
                {
                    for (int i = 0; i < (int)ERDF.ERR_CNT; i++)
                    {

                        ErrorInfo eInfo = new ErrorInfo();
                        if (i >= (int)ERDF.E_SV_AMP && i < (int)ERDF.E_SV_AMP + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_AMP.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_AMP];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_AMP] + "축 서보 앰프 알람 발생";
                            eInfo.SOLUTION = "서보앰프를 확인하여 발생한 알람 코드에 맞는 조치 방법을 취해주십시오.";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_MOVE && i < (int)ERDF.E_SV_MOVE + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_MOVE.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_MOVE];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_MOVE] + "축 움직임 실패";
                            eInfo.SOLUTION = SVDF.axisNames[i - (int)ERDF.E_SV_MOVE] + "축을 구동하는데 방해되는 장애물이 있는지 확인하십시오.";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_NOT_HOME && i < (int)ERDF.E_SV_NOT_HOME + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_NOT_HOME.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_NOT_HOME];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_NOT_HOME] + "축 초기화 하지 않음";
                            eInfo.SOLUTION = SVDF.axisNames[i - (int)ERDF.E_SV_NOT_HOME] + "축의 원점을 잡은 후 구동하십시오.";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_HW_LIMIT_P && i < (int)ERDF.E_SV_HW_LIMIT_P + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_HW_LIMIT_P.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_HW_LIMIT_P];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_HW_LIMIT_P] + "축 플러스 리밋 감지";
                            eInfo.SOLUTION = "이동위치가 잘못 입력되진 않았는지 확인하시고 플러스 리밋이 해제 될때까지 이동하여 주십시오";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_HW_LIMIT_N && i < (int)ERDF.E_SV_HW_LIMIT_N + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_HW_LIMIT_N.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_HW_LIMIT_N];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_HW_LIMIT_N] + "축 마이너스 리밋 감지";
                            eInfo.SOLUTION = "이동위치가 잘못 입력되진 않았는지 확인하시고 마이너스 리밋이 해제 될때까지 이동하여 주십시오";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_MOVE_INTERLOCK && i < (int)ERDF.E_SV_MOVE_INTERLOCK + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_MOVE_INTERLOCK.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_MOVE_INTERLOCK];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_MOVE_INTERLOCK] + "축 구동중 인터락 발생";
                            eInfo.SOLUTION = "";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_INVALID_VEL && i < (int)ERDF.E_SV_INVALID_VEL + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_INVALID_VEL.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_INVALID_VEL];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_INVALID_VEL] + "축 잘못된 구동 속도";
                            eInfo.SOLUTION = "지정할 수 없는 값 혹은 너무 크거나 작은 속도값이 설정되어있는것은 아닌지 확인하여 주십시오.";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_SW_LIMIT_P && i < (int)ERDF.E_SV_SW_LIMIT_P + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_SW_LIMIT_P.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_SW_LIMIT_P];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_SW_LIMIT_P] + "축에 설정된 소프트웨어 플러스 리밋을 초과한 위치 지령";
                            eInfo.SOLUTION = "이동할 위치 값 또는 설정된 S/W 플러스 리밋 값을 확인하여 주십시오.";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_SW_LIMIT_N && i < (int)ERDF.E_SV_SW_LIMIT_N + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_SW_LIMIT_N.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_SW_LIMIT_N];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_SW_LIMIT_N] + "축에 설정된소프트웨어 마이너스 리밋을 초과한 위치 지령";
                            eInfo.SOLUTION = "이동할 위치 값 또는 설정된 S/W 마이너스 리밋 값을 확인하여 주십시오.";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_SERVO_ON && i < (int)ERDF.E_SV_SERVO_ON + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_SERVO_ON.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_SERVO_ON];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_SERVO_ON] + "축 서보 오프 상태";
                            eInfo.SOLUTION = "서보를 온해주십시오,";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_HOME && i < (int)ERDF.E_SV_HOME + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_HOME.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_HOME];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_HOME] + "축 서보 원점이동 실패";
                            eInfo.SOLUTION = SVDF.axisNames[i - (int)ERDF.E_SV_HOME] + "축을 서보 원점 이동시 방해되는 장애물이 있는지 확인하십시오.";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_MOT_INFO && i < (int)ERDF.E_SV_MOT_INFO + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_MOT_INFO.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_MOT_INFO];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_MOT_INFO] + "축 서보 모터 정보 오류";
                            eInfo.SOLUTION = SVDF.axisNames[i - (int)ERDF.E_SV_MOT_INFO] + "축을 서보 모터 정보를 확인 부탁 드립니다";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else if (i >= (int)ERDF.E_SV_HOME_INTERLOCK && i < (int)ERDF.E_SV_HOME_INTERLOCK + SVDF.SV_CNT)
                        {
                            eInfo.NAME = ERDF.E_SV_HOME_INTERLOCK.ToString() + "_" + SVDF.axisNames[i - (int)ERDF.E_SV_HOME_INTERLOCK];
                            eInfo.CAUSE = SVDF.axisNames[i - (int)ERDF.E_SV_HOME_INTERLOCK] + "축 서보 원점이동 중 인터락 발생";
                            eInfo.SOLUTION = "";
                            errlist[i].NAME = eInfo.NAME;
                            errlist[i].CAUSE = eInfo.CAUSE;
                            errlist[i].SOLUTION = eInfo.SOLUTION;
                        }
                        else
                        {
                            if (Enum.IsDefined(typeof(ERDF), i))
                            {
                                eInfo.NAME = ((ERDF)i).ToString();
                                errlist[i].NAME = eInfo.NAME;
                            }
                            else
                            {
                                eInfo.NAME = "";
                                errlist[i].NAME = eInfo.NAME;
                            }
                        }
                    }
                    try
                    {
                        if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.ENGLISH)
                        {
                            JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR_ENG, FileMode.OpenOrCreate));
                        }
                        else if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.KOREAN)
                        {
                            JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR, FileMode.OpenOrCreate));
                        }
                        else
                        {
                            JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR_CHN, FileMode.OpenOrCreate));
                        }
                    }
                    catch (Exception ex)
                    {
                        messageBox = new popMessageBox("저장실패!.", "알림", MessageBoxButtons.OK);
                        messageBox.ShowDialog();
                        GbFunc.WriteExeptionLog(ex.ToString());

                    }
                    messageBox = new popMessageBox("저장되었습니다.", "알림", MessageBoxButtons.OK);
                    messageBox.ShowDialog();
                }
            }
            else
            {
                try
                {
                    if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.ENGLISH)
                    {
                        JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR_ENG, FileMode.OpenOrCreate));
                    }
                    else if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.KOREAN)
                    {
                        JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR, FileMode.OpenOrCreate));
                    }
                    else
                    {
                        JSONHelper.Serialize(errlist, new FileStream(PathMgr.Inst.FILE_PATH_ERROR_CHN, FileMode.OpenOrCreate));
                    }
                }
                catch (Exception ex)
                {
                    GbFunc.WriteExeptionLog(ex.ToString());
                }
            }
        }

        public void ResetLightAlarm()
        {
            Array.Clear(m_bLightAlarm, 0, m_bLightAlarm.Length);
            m_nLightAlarmCnt = 0;
        }

        public ErrorInfo Get(int nErrNo)
        {
            ErrorInfo info = null;

            try
            {
                info = errlist.Single(d => d.NO == nErrNo);
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }

            if (info == null)
            {
                info = new ErrorInfo();
                info.NO = nErrNo;
                info.NAME = "UNKNOWN ERROR";
                info.CAUSE = "UNKNOWN ERROR";
                info.SOLUTION = "UNKNOWN ERROR";
            }

            return info;
        }
    }
}
