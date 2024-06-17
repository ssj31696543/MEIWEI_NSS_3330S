using NSS_3330S;

namespace NSS_3330S
{
    public class Safety
    {
        public Safety()
        {
            //
        }


        //Servo Alarm, Home Complete 만 체크
        public int checkSafetyAxis(int nAxis, bool isIgnoreHomeComplete)
        {
            if (!isIgnoreHomeComplete)
            {
                if (!GbVar.mcState.isHomeComplete[nAxis]) { return (int)ERDF.E_SV_NOT_HOME + nAxis; }
            }


            //if (MotionManager.Instance[nAxis].IsAlarm()) { return ERDF.E_SV_AMP + nAxis; }
            return MCDF.SAFETY_OK;
        }

        //Servo 이동전 기본 인터록 체크
        public int checkSafetyAxisMove(int nAxis)
        {
            //switch(nAxis)
            //{
            //    case SVDF.AXIS_LASER_X: break;
            //    case SVDF.AXIS_LASER_Y: break;
            //    case SVDF.AXIS_LASER_Z: break;

            //    case SVDF.AXIS_VISION_LF_Z: break;
            //    case SVDF.AXIS_VISION_RT_Z: break;

            //    case SVDF.AXIS_STAGE_X: break;
            //    case SVDF.AXIS_STAGE_Y: break;
            //    case SVDF.AXIS_STAGE_Z: break;
            //    case SVDF.AXIS_STAGE_R: break;

            //    case SVDF.AXIS_VISION_LF_X: break;
            //    case SVDF.AXIS_VISION_RT_X: break;
            //}

            return MCDF.SAFETY_OK;
        }

        //Software Limit check
        public int checkSafetySwLimit(int nAxis, double dPos)
        {
            //if (dPos < FrmMain.sysCfg.servo[nAxis].dSwLimitN) { return ERDF.SV_SW_LIMIT_N_LD_X + nAxis; }
            //if (dPos > FrmMain.sysCfg.servo[nAxis].dSwLimitP) { return ERDF.SV_SW_LIMIT_P_LD_X + nAxis; }
            return MCDF.SAFETY_OK;
        }
    }
}