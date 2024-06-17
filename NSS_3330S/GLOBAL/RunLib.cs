using DionesTool.Motion;
using DionesTool.DEVICE.READER;
using System.Diagnostics;
using System.Threading;
using DionesTool.Objects;
using NSS_3330S.MOTION;

namespace NSS_3330S
{
    public class RunLib
    {
        MotionBase[] motion = null;
        private Stopwatch timeStamp = Stopwatch.StartNew();

        private enum runCmd
        {
            execute = 0,
            check,
            delay
        };

        private runCmd nCmd = runCmd.execute;

        public RunLib()
        {
            motion = MotionMgr.Inst.MOTION;
            timeStamp.Stop();
        }



        public int SvMove(MOTINFO[] mInfo, long lDelay, bool bResetFinish = true)
        {
            bool isDone = true;
            long lMoveTime = 5000;

            switch (nCmd)
            {
                case runCmd.execute:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;
                        if (motion[mInfo[i].nAxis].IsAlarm())
                        {
                            GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                            ResetCmd();
                            return mInfo[i].nAmpErrNo;
                        }
                    }

                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        // 아진인 경우 ms를 mm/s로 변경
                        if (motion[mInfo[i].nAxis].GetDeviceType() == DeviceType.DEVICE_AJIN)
                        {
                            if (!motion[mInfo[i].nAxis].MoveAbs(mInfo[i].dPos, mInfo[i].dVel, mInfo[i].TranslateMsToMmPerSecAcc(), mInfo[i].TranslateMsToMmPerSecDec(), false))
                            {
                                ResetCmd();
                                return mInfo[i].nMoveErrNo;
                            }
                        }
                        else
                        {
                            if (!motion[mInfo[i].nAxis].MoveAbs(mInfo[i].dPos, mInfo[i].dVel, mInfo[i].dAcc, mInfo[i].dDec, false))
                            {
                                ResetCmd();
                                return mInfo[i].nMoveErrNo;
                            }
                        }
                    }
                    timeStamp.Restart();
                    break;
                case runCmd.check:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        lMoveTime = ConfigMgr.Inst.Cfg.MotData[mInfo[i].nAxis].lMoveTime;
                        break;
                    }

                    if (timeStamp.Elapsed.TotalMilliseconds < lMoveTime)
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0)
                                continue;

                            isDone &= (!motion[mInfo[i].nAxis].IsBusy() && 
                                        motion[mInfo[i].nAxis].IsInPosition(mInfo[i].dPos));

                            if (motion[mInfo[i].nAxis].IsAlarm())
                            {
                                GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                ResetCmd();
                                return mInfo[i].nAmpErrNo;
                            }
                        }
                        if (isDone)
                        {
                            if (lDelay > 0) { timeStamp.Restart(); break; }
                            else
                            {
                                if (bResetFinish) ResetCmd();
                                timeStamp.Stop();
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0) continue;

                            motion[mInfo[i].nAxis].MoveStop();
                        }
                        ResetCmd();
                        // 실제 InPosition이 되지 않은 축 번호로 에러를 반환한다
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0) continue;

                            if (motion[mInfo[i].nAxis].IsBusy() || !motion[mInfo[i].nAxis].IsInPosition(mInfo[i].dPos))
                            {
                                return mInfo[i].nMoveErrNo;
                            }
                        }
                        return mInfo[0].nMoveErrNo;
                    }
                    return FNC.BUSY;
                case runCmd.delay:
                    {
                        if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                        {
                            for (int i = 0; i < mInfo.Length; i++)
                            {
                                if (mInfo[i].nAxis < 0) continue;

                                if (motion[mInfo[i].nAxis].IsAlarm())
                                {
                                    GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                    ResetCmd();
                                    return (int)mInfo[i].nAmpErrNo;
                                }
                            }
                            return FNC.BUSY;
                        }
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                if (bResetFinish) ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        public int SvMove(MOTINFO[] mInfo, bool[] bSkipMoveDone, long lDelay, bool bResetFinish = true)
        {
            bool isDone = true;
            long lMoveTime = 5000;

            switch (nCmd)
            {
                case runCmd.execute:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        if (motion[mInfo[i].nAxis].IsAlarm())
                        {
                            GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                            ResetCmd();
                            return mInfo[i].nAmpErrNo;
                        }
                    }

                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        // 아진인 경우 ms를 mm/s로 변경
                        if (motion[mInfo[i].nAxis].GetDeviceType() == DeviceType.DEVICE_AJIN)
                        {
                            if (!motion[mInfo[i].nAxis].MoveAbs(mInfo[i].dPos, mInfo[i].dVel, mInfo[i].TranslateMsToMmPerSecAcc(), mInfo[i].TranslateMsToMmPerSecDec(), false))
                            {
                                ResetCmd();
                                return mInfo[i].nMoveErrNo;
                            }
                        }
                        else
                        {
                            if (!motion[mInfo[i].nAxis].MoveAbs(mInfo[i].dPos, mInfo[i].dVel, mInfo[i].dAcc, mInfo[i].dDec, false))
                            {
                                ResetCmd();
                                return mInfo[i].nMoveErrNo;
                            }
                        }
                    }
                    timeStamp.Restart();
                    break;
                case runCmd.check:

                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        lMoveTime = ConfigMgr.Inst.Cfg.MotData[mInfo[i].nAxis].lMoveTime;
                        break;
                    }

                    if (timeStamp.Elapsed.TotalMilliseconds < lMoveTime)
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0) continue;

                            if (!bSkipMoveDone[i])
                            {
                                isDone &= (!motion[mInfo[i].nAxis].IsBusy() &&
                                            motion[mInfo[i].nAxis].IsInPosition(mInfo[i].dPos));
                            }

                            if (motion[mInfo[i].nAxis].IsAlarm())
                            {
                                GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                ResetCmd();
                                return mInfo[i].nAmpErrNo;
                            }
                        }
                        if (isDone)
                        {
                            if (lDelay > 0) { timeStamp.Restart(); break; }
                            else
                            {
                                if (bResetFinish) ResetCmd();
                                timeStamp.Stop();
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0) continue;

                            motion[mInfo[i].nAxis].MoveStop();
                        }
                        ResetCmd();
                        // 실제 InPosition이 되지 않은 축 번호로 에러를 반환한다
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0) continue;

                            if (motion[mInfo[i].nAxis].IsBusy() || !motion[mInfo[i].nAxis].IsInPosition(mInfo[i].dPos))
                                return mInfo[i].nMoveErrNo;
                        }
                        return mInfo[0].nMoveErrNo;
                    }
                    return FNC.BUSY;
                case runCmd.delay:
                    {
                        if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                        {
                            for (int i = 0; i < mInfo.Length; i++)
                            {
                                if (mInfo[i].nAxis < 0) continue;

                                if (motion[mInfo[i].nAxis].IsAlarm())
                                {
                                    GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                    ResetCmd();
                                    return (int)mInfo[i].nAmpErrNo;
                                }
                            }
                            return FNC.BUSY;
                        }
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                if (bResetFinish) ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        public int SvLinearMoveEx(MOTINFO[] mInfo, int nMaxAxis, int nCoordnate, long lDelay)
        {
            bool isDone = true;
            long lMoveTime = 5000;

            switch (nCmd)
            {
                case runCmd.execute:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        if (motion[mInfo[i].nAxis].IsAlarm())
                        {
                            GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                            ResetCmd();
                            return mInfo[i].nAmpErrNo;
                        }
                    }

                    if (!SvLinearMove(mInfo, nMaxAxis, nCoordnate))
                    {
                        ResetCmd();
                        return mInfo[0].nMoveErrNo;
                    }
                    timeStamp.Restart();
                    break;
                case runCmd.check:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        lMoveTime = ConfigMgr.Inst.Cfg.MotData[mInfo[i].nAxis].lMoveTime;
                        break;
                    }

                    if (timeStamp.Elapsed.TotalMilliseconds < lMoveTime)
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0) continue;

                            isDone &= (!motion[mInfo[i].nAxis].IsBusy() && motion[mInfo[i].nAxis].IsInPosition(mInfo[i].dPos));

                            if (motion[mInfo[i].nAxis].IsAlarm())
                            {
                                GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                ResetCmd();
                                return mInfo[i].nAmpErrNo;
                            }
                        }
                        if (isDone)
                        {
                            if (lDelay > 0) { timeStamp.Restart(); break; }
                            else { ResetCmd(); timeStamp.Stop(); return FNC.SUCCESS; }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0) continue;

                            motion[mInfo[i].nAxis].MoveStop();
                        }
                        ResetCmd();
                        // 실제 InPosition이 되지 않은 축 번호로 에러를 반환한다
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (mInfo[i].nAxis < 0) continue;

                            if (motion[mInfo[i].nAxis].IsBusy() || !motion[mInfo[i].nAxis].IsInPosition(mInfo[i].dPos))
                                return mInfo[i].nMoveErrNo;
                        }
                        return mInfo[0].nMoveErrNo;
                    }
                    return FNC.BUSY;
                case runCmd.delay:
                    {
                        if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                        {
                            for (int i = 0; i < mInfo.Length; i++)
                            {
                                if (mInfo[i].nAxis < 0) continue;

                                if (motion[mInfo[i].nAxis].IsAlarm())
                                {
                                    GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                    ResetCmd();
                                    return (int)mInfo[i].nAmpErrNo;
                                }

                            }
                            return FNC.BUSY;
                        }
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        public int SvSignalSearchMove(MOTINFO[] mInfo, long lDelay, bool bResetFinish = true)
        {
            bool isDone = true;
            long lMoveTime = 5000;

            switch (nCmd)
            {
                case runCmd.execute:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (motion[mInfo[i].nAxis].IsAlarm())
                        {
                            GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                            ResetCmd();
                            return mInfo[i].nAmpErrNo;
                        }
                    }

                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        // 이미 시그널 감지상태이면 무브 하지 못하도록 처리함
                        uint nBit = 0;
                        if (CAXM.AxmSignalReadInputBit(mInfo[i].nAxis, 2, ref nBit) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                        {
                            // 시그널 읽기 실패시 알람
                            return mInfo[i].nMoveErrNo;
                        }
                        else
                        {
                            // 이미 시그널 감지상태면 SUCCESS
                            if (nBit == 1) return FNC.SUCCESS;
                        }

                        // 아진인 경우 ms를 mm/s로 변경
                        //if (motion[mInfo[i].nAxis].GetDeviceType() == DeviceType.DEVICE_AJIN)
                        {
                            if (!motion[mInfo[i].nAxis].MoveSignalSearch())
                            {
                                ResetCmd();
                                return mInfo[i].nMoveErrNo;
                            }
                        }
                        //else
                        //{
                        //    if (!motion[mInfo[i].nAxis].MoveSignalSearch(mInfo[i].dPos, mInfo[i].dVel, mInfo[i].dAcc, mInfo[i].dDec, false))
                        //    {
                        //        ResetCmd();
                        //        return mInfo[i].nMoveErrNo;
                        //    }
                        //}
                    }
                    timeStamp.Restart();
                    break;
                case runCmd.check:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        lMoveTime = ConfigMgr.Inst.Cfg.MotData[mInfo[i].nAxis].lMoveTime;
                        break;
                    }

                    if (timeStamp.Elapsed.TotalMilliseconds < lMoveTime)
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            isDone &= (!motion[mInfo[i].nAxis].IsBusy());

                            if (motion[mInfo[i].nAxis].IsAlarm())
                            {
                                GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                ResetCmd();
                                return mInfo[i].nAmpErrNo;
                            }
                        }
                        if (isDone)
                        {
                            if (lDelay > 0) { timeStamp.Restart(); break; }
                            else
                            {
                                if (bResetFinish) ResetCmd();
                                timeStamp.Stop();
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            motion[mInfo[i].nAxis].MoveStop();
                        }
                        ResetCmd();
                        // 실제 InPosition이 되지 않은 축 번호로 에러를 반환한다
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (motion[mInfo[i].nAxis].IsBusy() || !motion[mInfo[i].nAxis].IsInPosition(mInfo[i].dPos))
                                return mInfo[i].nMoveErrNo;
                        }
                        return mInfo[0].nMoveErrNo;
                    }
                    return FNC.BUSY;
                case runCmd.delay:
                    {
                        if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                        {
                            for (int i = 0; i < mInfo.Length; i++)
                            {
                                if (motion[mInfo[i].nAxis].IsAlarm())
                                {
                                    GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                    ResetCmd();
                                    return (int)mInfo[i].nAmpErrNo;
                                }
                            }
                            return FNC.BUSY;
                        }
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                if (bResetFinish) ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        public int SvPlusSignalSearchMove(MOTINFO[] mInfo, long lDelay, bool bResetFinish = true)
        {
            bool isDone = true;
            long lMoveTime = 5000;

            switch (nCmd)
            {
                case runCmd.execute:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (motion[mInfo[i].nAxis].IsAlarm())
                        {
                            GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                            ResetCmd();
                            return mInfo[i].nAmpErrNo;
                        }
                    }

                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (motion[mInfo[i].nAxis].IsPlusLimit())
                        {
                            // 이미 시그널 감지상태면 SUCCESS
                            return FNC.SUCCESS;
                        }
                        // 아진인 경우 ms를 mm/s로 변경
                        //if (motion[mInfo[i].nAxis].GetDeviceType() == DeviceType.DEVICE_AJIN)
                        {
                            if (!motion[mInfo[i].nAxis].MovePlusSignalSearch())
                            {
                                ResetCmd();
                                return mInfo[i].nMoveErrNo;
                            }
                        }
                        //else
                        //{
                        //    if (!motion[mInfo[i].nAxis].MoveSignalSearch(mInfo[i].dPos, mInfo[i].dVel, mInfo[i].dAcc, mInfo[i].dDec, false))
                        //    {
                        //        ResetCmd();
                        //        return mInfo[i].nMoveErrNo;
                        //    }
                        //}
                    }
                    timeStamp.Restart();
                    break;
                case runCmd.check:
                    for (int i = 0; i < mInfo.Length; i++)
                    {
                        if (mInfo[i].nAxis < 0) continue;

                        lMoveTime = ConfigMgr.Inst.Cfg.MotData[mInfo[i].nAxis].lMoveTime;
                        break;
                    }

                    if (timeStamp.Elapsed.TotalMilliseconds < lMoveTime)
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            isDone &= (!motion[mInfo[i].nAxis].IsBusy());

                            if (motion[mInfo[i].nAxis].IsAlarm())
                            {
                                GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                ResetCmd();
                                return mInfo[i].nAmpErrNo;
                            }
                        }
                        if (isDone)
                        {
                            if (lDelay > 0) { timeStamp.Restart(); break; }
                            else
                            {
                                if (bResetFinish) ResetCmd();
                                timeStamp.Stop();
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            motion[mInfo[i].nAxis].MoveStop();
                        }
                        ResetCmd();
                        // 실제 InPosition이 되지 않은 축 번호로 에러를 반환한다
                        for (int i = 0; i < mInfo.Length; i++)
                        {
                            if (motion[mInfo[i].nAxis].IsBusy() || !motion[mInfo[i].nAxis].IsInPosition(mInfo[i].dPos))
                                return mInfo[i].nMoveErrNo;
                        }
                        return mInfo[0].nMoveErrNo;
                    }
                    return FNC.BUSY;
                case runCmd.delay:
                    {
                        if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                        {
                            for (int i = 0; i < mInfo.Length; i++)
                            {
                                if (motion[mInfo[i].nAxis].IsAlarm())
                                {
                                    GbVar.mcState.isHomeComplete[mInfo[i].nAxis] = false;
                                    ResetCmd();
                                    return (int)mInfo[i].nAmpErrNo;
                                }
                            }
                            return FNC.BUSY;
                        }
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                if (bResetFinish) ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        /// <summary>
        /// Multi Axis 직선보간 구동
        /// 라이브러리 지원이 4축까지 가능
        /// 축 맵핑은 반드시 낮은축 번호부터 맵핑해야한다.
        /// 리미트 신호가 입력되어 있다면 구동할수 없다
        /// 보간 축들의 Unit/Pulse는 반드시 동일하게 설정되어 있어야 한다.
        /// 1~4, 5~8 등으로 축그룹이 형성되어 있어야 한다.
        /// !!!QI-2Axis 등의 보드는 보간 2축이 최대임!!!
        /// </summary>
        /// <param name="mInfo"></param>
        /// <param name="nMaxAxis"></param>
        /// <param name="nCoordnate"></param>
        /// <returns></returns>
        public bool SvLinearMove(MOTINFO[] mInfo, int nMaxAxis, int nCoordnate)
        {
            if (nMaxAxis > 4) return false;

            //최대 4 Axis
            int[] nAxisList = new int[nMaxAxis];
            double[] dMultiPos = new double[nMaxAxis];
            double[] dMultiVel = new double[nMaxAxis];
            double[] dMultiAcc = new double[nMaxAxis];
            double[] dMultiDec = new double[nMaxAxis];

            for (int i = 0; i < nMaxAxis; i++)
            {
                if (motion[mInfo[i].nAxis].IsBusy() || motion[mInfo[i].nAxis].IsAlarm()) return false;
                if (mInfo[i].dVel <= 0.0)
                {
                    mInfo[i].dVel = 1.0;
                    mInfo[i].dAcc = 10.0;
                    mInfo[i].dDec = 10.0;
                }
                nAxisList[i] = mInfo[i].nAxis;
                dMultiPos[i] = mInfo[i].dPos;
                dMultiVel[i] = mInfo[i].dVel;
                dMultiAcc[i] = mInfo[i].dAcc;
                dMultiDec[i] = mInfo[i].dDec;
                CAXM.AxmMotSetAbsRelMode(mInfo[i].nAxis, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            }

            CAXM.AxmContiSetAxisMap(nCoordnate, (uint)nMaxAxis, nAxisList);
            CAXM.AxmContiWriteClear(nCoordnate);
            CAXM.AxmContiSetAbsRelMode(nCoordnate, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            //첫번째 축의 속도정보를 따라간다.
            //duRetCode =
            CAXM.AxmLineMove(nCoordnate, dMultiPos, mInfo[0].dVel, mInfo[0].dAcc, mInfo[0].dDec);
            //if (duRetCode == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            //{
            return true;
            //}
            //return false;
        }

        //---------------------------------------------------------------------------
        public bool SvCirclePointMove(MOTINFO[] mInfo, dxy dxyMidPos, dxy dxyEndPos, int nMaxAxis, int nCoordnate)
        {
            if (nMaxAxis > 4) return false;
            //             if (mInfo[0].nAxis >= DF.EZI_SERVO_INDEX) return false;
            //             if (mInfo[1].nAxis >= DF.EZI_SERVO_INDEX) return false;

            //uint duRetCode = 0;
            //int nCoordnate = 0;

            //최대 4 Axis
            int[] nAxisList = { 0, 0 };
            double[] dMidPosition = { 0.0, 0.0 };
            double[] dEndPosition = { 0.0, 0.0 };
            double[] dMultiVel = { 0.0, 0.0 };
            double[] dMultiAcc = { 0.0, 0.0 };
            double[] dMultiDec = { 0.0, 0.0 };

            dMidPosition[0] = dxyMidPos.x;
            dMidPosition[1] = dxyMidPos.y;
            dEndPosition[0] = dxyEndPos.x;
            dEndPosition[1] = dxyEndPos.y;

            for (int i = 0; i < nMaxAxis; i++)
            {
                if (motion[mInfo[i].nAxis].IsBusy() || motion[mInfo[i].nAxis].IsAlarm()) return false;
                if (mInfo[i].dVel <= 0.0)
                {
                    mInfo[i].dVel = 1.0;
                    mInfo[i].dAcc = 10.0;
                    mInfo[i].dDec = 10.0;
                }
                nAxisList[i] = mInfo[i].nAxis;
                CAXM.AxmMotSetAbsRelMode(mInfo[i].nAxis, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            }

            CAXM.AxmContiSetAxisMap(nCoordnate, (uint)nMaxAxis, nAxisList);
            CAXM.AxmContiWriteClear(nCoordnate);
            CAXM.AxmContiSetAbsRelMode(nCoordnate, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            //첫번째 축의 속도정보를 따라간다.
            //duRetCode =
            //CAXM.AxmLineMove(nCoordnate, dMultiPos, mInfo[0].dVel, mInfo[0].dAcc, mInfo[0].dDec);

            int nArcCircle = 0; //아크=0, 원=1
            CAXM.AxmCirclePointMove(nCoordnate, nAxisList, dMidPosition, dEndPosition, mInfo[0].dVel, mInfo[0].dAcc, mInfo[0].dDec, nArcCircle);

            //if (duRetCode == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            //{
            return true;
            //}
            //return false;
        }

        //---------------------------------------------------------------------------
        /// <summary>
        /// Line, 2Pattern 2 Axis 연속보간
        /// </summary>
        /// <param name="mInfo"></param>
        /// <param name="dxyMidPos"></param>
        /// <param name="dxyEndPos"></param>
        /// <param name="nMaxAxis"></param>
        /// <param name="nCoordnate"></param>
        /// <returns></returns>
        public bool SvLine2PatternContiLinearMove(MOTINFO[] mInfo, dxy dxyLinePos1, dxy dxyLinePos2, int nCoordnate)
        {
            double[] dMovePos1 = new double[2];
            double[] dMovePos2 = new double[2];
            int[] nAxisList = new int[2] { 0, 0 };
            double dVelocity, dAccel, dDecel;

            for (int i = 0; i < 2; i++)
            {
                if (motion[mInfo[i].nAxis].IsBusy() || motion[mInfo[i].nAxis].IsAlarm()) return false;
                if (mInfo[i].dVel <= 0.0)
                {
                    dVelocity = 1.0;
                    dAccel = 10.0;
                    dDecel = 10.0;
                }
                nAxisList[i] = mInfo[i].nAxis;
            }
            dVelocity = mInfo[0].dVel;
            dAccel = dVelocity * 2.0;//mInfo[0].dAcc;
            dDecel = dVelocity * 2.0;

            //++ 지정한 좌표계에 연속보간 축들을 맵핑합니다.
            CAXM.AxmContiSetAxisMap(nCoordnate, 2, nAxisList);
            //++ 지정한 좌표계에 등록되어있는 보간데이타를 모두 지웁니다.
            CAXM.AxmContiWriteClear(nCoordnate);
            //++ 지정한 좌표계에 구동모드을 설정합니다.(절대구동/상대구동)
            CAXM.AxmMotSetAbsRelMode(mInfo[0].nAxis, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            //++ 지정한 좌표계에 연속보간을 수행할 작업 등록을 시작합니다.
            // ※ [CAUTION] 이 함수가 실행된 후 에는 보간함수들이 즉시 구동되지 않고 보간데이타 Queue에 예약됩니다.
            CAXM.AxmContiBeginNode(nCoordnate);
            dMovePos1[0] = dxyLinePos1.x;
            dMovePos1[1] = dxyLinePos1.y;
            CAXM.AxmLineMove(nCoordnate, dMovePos1, dVelocity, dAccel, dDecel);

            dMovePos2[0] = dxyLinePos2.x;
            dMovePos2[1] = dxyLinePos2.y;
            CAXM.AxmLineMove(nCoordnate, dMovePos2, dVelocity, dAccel, dDecel);
            CAXM.AxmContiEndNode(nCoordnate);
            CAXM.AxmContiStart(nCoordnate, 0, 0);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="mInfo"></param>
        /// <param name="dxyLinePos1"></param>
        /// <param name="dxyArcCenPos"></param>
        /// <param name="dxyArcEndPos"></param>
        /// <param name="nCoordnate"></param>
        /// <param name="nDir (0=반시계방향, 1=시계방향)"></param>
        /// <returns></returns>
        public bool SvLineArcLine3PatternContiLinearMove(MOTINFO[] mInfo, dxy dxyLinePos1, dxy dxyLinePos2, dxy dxyArcCenPos, dxy dxyArcEndPos, int nCoordnate, uint nDir)
        {
            double[] dMovePosLine1 = new double[2];
            double[] dMovePosLine2 = new double[2];
            double[] dMovePosArcCen = new double[2];
            double[] dMovePosArcEnd = new double[2];
            int[] nAxisList = new int[2] { 0, 0 };
            double dVelocity, dAccel, dDecel;

            for (int i = 0; i < 2; i++)
            {
                if (motion[mInfo[i].nAxis].IsBusy() || motion[mInfo[i].nAxis].IsAlarm()) return false;
                if (mInfo[i].dVel <= 0.0)
                {
                    dVelocity = 1.0;
                    dAccel = 10.0;
                    dDecel = 10.0;
                }
                nAxisList[i] = mInfo[i].nAxis;
            }
            dVelocity = mInfo[0].dVel;
            dAccel = dVelocity * 10.0;// mInfo[0].dAcc;//dVelocity * 2.0;//mInfo[0].dAcc;
            dDecel = dVelocity * 10.0; //mInfo[0].dDec;//dVelocity * 2.0;

            //++ 지정한 좌표계에 등록되어있는 보간데이타를 모두 지웁니다.
            CAXM.AxmContiWriteClear(nCoordnate);
            //++ 지정한 좌표계에 연속보간 축들을 맵핑합니다.
            CAXM.AxmContiSetAxisMap(nCoordnate, 2, nAxisList);

            //++ 지정한 좌표계에 구동모드을 설정합니다.(절대구동/상대구동)
            //CAXM.AxmMotSetAbsRelMode(mInfo[0].nAxis, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            //++ 지정한 좌표계에 구동모드을 설정합니다.(절대구동/상대구동)
            CAXM.AxmContiSetAbsRelMode(nCoordnate, (uint)AXT_MOTION_ABSREL.POS_ABS_MODE);
            //++ 지정한 좌표계에 연속보간을 수행할 작업 등록을 시작합니다.
            // ※ [CAUTION] 이 함수가 실행된 후 에는 보간함수들이 즉시 구동되지 않고 보간데이타 Queue에 예약됩니다.

            double dLastVel = mInfo[0].dVel / 2;
            double dLastAcc = mInfo[0].dVel * 2;
            double dLastDec = mInfo[0].dVel * 2;

            uint duRet = 0;
            duRet = CAXM.AxmContiBeginNode(nCoordnate);
            dMovePosLine1[0] = dxyLinePos1.x;
            dMovePosLine1[1] = dxyLinePos1.y;
            duRet = CAXM.AxmLineMove(nCoordnate, dMovePosLine1, dVelocity, dAccel, dDecel);
            if (duRet != 0)
            {
                return false;
            }
            dMovePosArcCen[0] = dxyArcCenPos.x;
            dMovePosArcCen[1] = dxyArcCenPos.y;
            dMovePosArcEnd[0] = dxyArcEndPos.x;
            dMovePosArcEnd[1] = dxyArcEndPos.y;
            duRet = CAXM.AxmCircleCenterMove(nCoordnate, nAxisList, dMovePosArcCen, dMovePosArcEnd, dVelocity, dAccel, dDecel, nDir);
            if (duRet != 0)
            {
                return false;
            }

            dMovePosLine2[0] = dxyLinePos2.x;
            dMovePosLine2[1] = dxyLinePos2.y;
            duRet = CAXM.AxmLineMove(nCoordnate, dMovePosLine2, dVelocity, dAccel, dDecel);
            if (duRet != 0)
            {
                return false;
            }

            duRet = CAXM.AxmContiEndNode(nCoordnate);
            duRet = CAXM.AxmContiStart(nCoordnate, 0, 0);
            if (duRet != 0)
            {
                return false;
            }
            return true;
        }


        public int GetCurRunCmd()
        {
            return (int)nCmd;
        }

        public bool IsCurRunCmdStart()
        {
            if (nCmd == runCmd.execute) return true;
            return false;
        }

        public void ResetCmd()
        {
            nCmd = runCmd.execute;
            timeStamp.Stop();
        }

        public void SetTimeBegin()
        {
            timeStamp.Reset();
            timeStamp.Start();
        }

        public void SetTimeEnd()
        {
            timeStamp.Stop();
        }

        public bool IsTimeOut(long lTimeOut)
        {
            if (timeStamp.Elapsed.TotalMilliseconds > lTimeOut)
            {
                return true;
            }
            return false;
        }

        public long GetElapsedTime()
        {
            return (long)timeStamp.Elapsed.TotalMilliseconds;
        }



        //실린더 리스트 구동
        public int CylListMove(int[] nOnOutput, int[] nOffOutput, int[] nOnInput, int[] nOffInput, int nErr, long lDelay, int nMode = 0)
        {
#if _NOTEBOOK
            return msecDelay(300);
#endif
            bool bOnCheck = true;
            bool bOffCheck = true;
            switch (nCmd)
            {
                case runCmd.execute:
                    for (int i = 0; i < nOffOutput.Length; i++)
                    {
                        if (nOffOutput[i] < 0) continue;
                        MotionMgr.Inst.SetOutput(nOffOutput[i], false);
                    }
                    //MotionMgr.Inst.SetOutput(nOffOutput, false);

                    for (int i = 0; i < nOnOutput.Length; i++)
                    {
                        if (nOnOutput[i] < 0) continue;
                        MotionMgr.Inst.SetOutput(nOnOutput[i], true);
                    }
                    //MotionMgr.Inst.SetOutput(nOnOutput, true, nMode);

                    timeStamp.Restart();
                    break;

                case runCmd.check:
                    if (timeStamp.Elapsed.TotalMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].lValue)
                    {
                        for (int i = 0; i < nOnInput.Length; i++)
                        {
                            if (nOnInput[i] < 0) continue;
                            bOnCheck = (MotionMgr.Inst.GetInput(nOnInput[i]) == true) & bOnCheck;
                        }
                        for (int i = 0; i < nOffInput.Length; i++)
                        {
                            if (nOffInput[i] < 0) continue;
                            bOffCheck = (MotionMgr.Inst.GetInput(nOffInput[i]) == false) & bOffCheck;
                        }
                        if (bOnCheck && bOffCheck)
                        {
                            if (lDelay > 0)
                            {
                                timeStamp.Restart();
                                break;
                            }
                            //DIONES HEP 추가 성공하면 리셋시엠디
                            ResetCmd();
                            timeStamp.Stop();
                            return FNC.SUCCESS;
                        }
                        return FNC.BUSY;
                    }
                    else
                    {
                        ResetCmd();
                        return nErr;
                    }

                case runCmd.delay:
                    if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                    {
                        for (int i = 0; i < nOnInput.Length; i++)
                        {
                            if (nOnInput[i] < 0) continue;
                            if (MotionMgr.Inst.GetInput(nOnInput[i]) == false) nCmd = runCmd.execute;
                        }
                        for (int i = 0; i < nOffInput.Length; i++)
                        {
                            if (nOffInput[i] < 0) continue;
                            if (MotionMgr.Inst.GetInput(nOffInput[i]) == true) nCmd = runCmd.execute;
                        }
                        return FNC.BUSY;
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        public int CylListMove(int[] nOnOutput, int[] nOffOutput, int[] nOnInput, int[] nOffInput, int[] nErr, long lDelay, bool isChkIn = true, bool bResetFinish = true, int nMode = 0)
        {
            bool bOnCheck = true;
            bool bOffCheck = true;
            bool bOutputOffCheck = true;
            bool bOutputOnCheck = true;
            int nFuncResult = FNC.SUCCESS;

            for (int i = 0; i < nOnOutput.Length; i++)
            {
                int nInterlock = SafetyMgr.Inst.GetCylinderSafetyBeforeOn(nOnOutput[i]);

                if (FNC.IsErr(nInterlock))
                    return nInterlock;
            }

            switch (nCmd)
            {
                case runCmd.execute:
                    // 전부 Off인지 확인
                    for (int i = 0; i < nOffOutput.Length; i++)
                    {
                        if (nOffOutput[i] < 0) continue;

                        if (GbVar.GB_OUTPUT[nOffOutput[i]] == 1)
                        {
                            bOutputOffCheck = false;
                            break;
                        }
                    }

                    // 하나라도 On이면 Off
                    if (!bOutputOffCheck)
                    {
                        for (int i = 0; i < nOffOutput.Length; i++)
                        {
                            if (nOffOutput[i] < 0) continue;
                            MotionMgr.Inst.SetOutput(nOffOutput[i], false);
                        }
                    }

                    // 전부 On인지 확인
                    for (int i = 0; i < nOnOutput.Length; i++)
                    {
                        if (nOnOutput[i] < 0) continue;

                        if (GbVar.GB_OUTPUT[nOnOutput[i]] == 0)
                        {
                            bOutputOnCheck = false;
                            break;
                        }
                    }

                    // 하나라도 Off이면 On
                    if (!bOutputOnCheck)
                    {
                        for (int i = 0; i < nOnOutput.Length; i++)
                        {
                            if (nOnOutput[i] < 0) continue;
                            MotionMgr.Inst.SetOutput(nOnOutput[i], true);
                        }
                    }

                    if (!isChkIn)
                    {
                        timeStamp.Restart();
                        nCmd = runCmd.delay;
                        return FNC.BUSY;
                    }

                    //시작단계에서 완료확인
                    if (lDelay <= 0)
                    {
                        for (int i = 0; i < nOnInput.Length; i++)
                        {
                            if (nOnInput[i] < 0) continue;
                            bOnCheck = (GbVar.GB_INPUT[nOnInput[i]] == 1) & bOnCheck;
                        }
                        if (bOnCheck)
                        {
                            return FNC.SUCCESS;
                        }
                    }
                    timeStamp.Restart();
                    break;

                case runCmd.check:
                    if (timeStamp.Elapsed.TotalMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].lValue)
                    {
                        for (int i = 0; i < nOnInput.Length; i++)
                        {
                            if (nOnInput[i] < 0) continue;
                            bOnCheck = (GbVar.GB_INPUT[nOnInput[i]] == 1) & bOnCheck;

#if !_NOTEBOOK
                            //MotionMgr.Inst.SetInput(nOnInput[i], true, nMode);
#endif
                        }
                        for (int i = 0; i < nOffInput.Length; i++)
                        {
                            if (nOffInput[i] < 0) continue;
                            bOffCheck = (GbVar.GB_INPUT[nOffInput[i]] == 0) & bOffCheck;

#if !_NOTEBOOK
                            //MotionMgr.Inst.SetInput(nOffInput[i], false, nMode);
#endif
                        }
                        if (bOnCheck && bOffCheck)
                        {
                            if (lDelay > 0)
                            {
                                timeStamp.Restart();
                                break;
                            }
                            if (bResetFinish) ResetCmd();
                            timeStamp.Stop();
                            return FNC.SUCCESS;
                        }
                        return FNC.BUSY;
                    }
                    else
                    {
                        ResetCmd();

                        int nErrCyl = nErr[0];
                        for (int i = 0; i < nOnInput.Length; i++)
                        {
                            if (nOnInput[i] < 0) continue;
                            if (GbVar.GB_INPUT[nOnInput[i]] == 0)
                            {
                                if (i < nErr.Length) nErrCyl = nErr[i];
                                else nErrCyl = nErr[0];
                                break;
                            }
                        }
                        for (int i = 0; i < nOffInput.Length; i++)
                        {
                            if (nOffInput[i] < 0) continue;
                            if (GbVar.GB_INPUT[nOffInput[i]] == 1)
                            {
                                if (i < nErr.Length) nErrCyl = nErr[i];
                                else nErrCyl = nErr[0];
                                break;
                            }
                        }

                        return nErrCyl;
                    }

                case runCmd.delay:
                    if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                    {
                        for (int i = 0; i < nOnInput.Length; i++)
                        {
                            if (nOnInput[i] < 0) continue;
                            if (GbVar.GB_INPUT[nOnInput[i]] == 0) nCmd = runCmd.execute;
                        }
                        for (int i = 0; i < nOffInput.Length; i++)
                        {
                            if (nOffInput[i] < 0) continue;
                            if (GbVar.GB_INPUT[nOffInput[i]] == 1) nCmd = runCmd.execute;
                        }
                        return FNC.BUSY;
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                if (bResetFinish) ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        //msec Delay
        public int msecDelay(long lDelay)
        {
            switch (nCmd)
            {
                case runCmd.execute:
                    timeStamp.Restart();
                    break;

                case runCmd.check:
                    break;

                case runCmd.delay:
                    if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                    {
                        return FNC.BUSY;
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        public int VacuumListOn(int[] nOnOUT, int[] nOffOUT, int[] nOnIN, int[] nOffIN, int[] nErr, long lDelay, bool isChk = true, bool bOnlyChk = false, int nMode = 0)
        {
            bool bOnCheck = true;
            bool bOffCheck = true;
            bool bOutputOffCheck = true;
            bool bOutputOnCheck = true;
            switch (nCmd)
            {
                case runCmd.execute:
                    {
                        if (bOnlyChk == false)
                        {
                            // 전부 Off인지 확인
                            for (int i = 0; i < nOffOUT.Length; i++)
                            {
                                if (nOffOUT[i] < 0) continue;

                                if (GbVar.GB_OUTPUT[nOffOUT[i]] == 1)
                                {
                                    bOutputOffCheck = false;
                                    break;
                                }
                            }

                            if (!bOutputOffCheck)
                            {
                                for (int i = 0; i < nOffOUT.Length; i++)
                                {
                                    if (nOffOUT[i] < 0) continue;

                                    MotionMgr.Inst.SetOutput(nOffOUT[i], false);
                                }
                            }

                            // 전부 On인지 확인
                            for (int i = 0; i < nOnOUT.Length; i++)
                            {
                                if (nOnOUT[i] < 0) continue;

                                if (GbVar.GB_OUTPUT[nOnOUT[i]] == 0)
                                {
                                    bOutputOnCheck = false;
                                    break;
                                }
                            }

                            if (!bOutputOnCheck)
                            {
                                for (int i = 0; i < nOnOUT.Length; i++)
                                {
                                    if (nOnOUT[i] < 0) continue;

                                    MotionMgr.Inst.SetOutput(nOnOUT[i], true);
                                }
                            }
                        }

                        //if (!isChk) return FNC.SUCCESS;
                        // 체크를 안할 때는 딜레이만
                        if (!isChk)
                        {
                            timeStamp.Restart();
                            nCmd = runCmd.delay;
                            return FNC.BUSY;
                        }

                        //시작단계에서 완료확인
                        if (lDelay <= 0)
                        {
                            for (int i = 0; i < nOnIN.Length; i++)
                            {
                                if (nOnIN[i] < 0) continue;
                                bOnCheck = IsAVacOn(nOnIN[i]) & bOnCheck;
                            }
                            for (int i = 0; i < nOffIN.Length; i++)
                            {
                                if (nOffIN[i] < 0) continue;
                                bOffCheck = IsAVacOff(nOffIN[i]) & bOffCheck;
                            }
                            if (bOnCheck && bOffCheck)
                            {
                                return FNC.SUCCESS;
                            }
                        }
                        timeStamp.Restart();
                    }
                    break;

                case runCmd.check:
                    if (timeStamp.Elapsed.TotalMilliseconds < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_CHECK_TIME].lValue)
                    {
                        for (int i = 0; i < nOnIN.Length; i++)
                        {
                            if (nOnIN[i] < 0) continue;
                            bOnCheck = IsAVacOn(nOnIN[i]) & bOnCheck;
                        }
                        for (int i = 0; i < nOffIN.Length; i++)
                        {
                            if (nOffIN[i] < 0) continue;
                            bOffCheck = IsAVacOff(nOffIN[i]) & bOffCheck;
                        }
                        if (bOnCheck && bOffCheck)
                        {
                            if (lDelay > 0)
                            {
                                timeStamp.Restart();
                                break;
                            }
                            return FNC.SUCCESS;
                        }
                        return FNC.BUSY;
                    }
                    else
                    {
                        ResetCmd();
                        for (int i = 0; i < nOnIN.Length; i++)
                        {
                            if (nOnIN[i] < 0) continue;
                            if (nErr.Length - 1 < i) continue;
                            if (!IsAVacOn(nOnIN[i]))
                            {
                                return nErr[i];
                            }
                        }
                        for (int i = 0; i < nOffIN.Length; i++)
                        {
                            if (nOffIN[i] < 0) continue;
                            if (nErr.Length - 1 < i) continue;
                            if (!IsAVacOff(nOffIN[i]))
                            {
                                return nErr[i];
                            }
                        }
                        return nErr[0];
                    }

                case runCmd.delay:
                    if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                    {
                        if (!isChk)
                        {
                            return FNC.BUSY;
                        }

                        // 해당부분으로 인해 다시 시작한다. 사용하려면 Output을 봐야함
                        //for (int i = 0; i < nOnIN.Length; i++)
                        //{
                        //    if (nOnIN[i] < 0) continue;
                        //    if (!IsAVacOn(nOnIN[i])) nCmd = runCmd.execute;
                        //}
                        // 해당부분으로 인해 다시 시작한다. 사용하려면 Output을 봐야함
                        //for (int i = 0; i < nOffIN.Length; i++)
                        //{
                        //    if (nOffIN[i] < 0) continue;
                        //    if (!IsAVacOff(nOffIN[i])) nCmd = runCmd.execute;
                        //}
                        return FNC.BUSY;
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        //Vacuum List Off
        public int VacuumListOff(int[] nOnOUT, int[] nOffOUT, int[] nOffIN, int[] nErr, long lDelay, bool isChk, int nMode)
        {
            bool bOffCheck = true;
            bool bOutputOffCheck = true;
            bool bOutputOnCheck = true;

            switch (nCmd)
            {
                case runCmd.execute:
                    // 전부 Off인지 확인
                    for (int i = 0; i < nOffOUT.Length; i++)
                    {
                        if (nOffOUT[i] < 0) continue;

                        if (GbVar.GB_OUTPUT[nOffOUT[i]] == 1)
                        {
                            bOutputOffCheck = false;
                            break;
                        }
                    }

                    if (!bOutputOffCheck)
                    {
                        for (int i = 0; i < nOffOUT.Length; i++)
                        {
                            if (nOffOUT[i] < 0) continue;

                            MotionMgr.Inst.SetOutput(nOffOUT[i], false);
                        }
                    }

                    // 전부 On인지 확인
                    for (int i = 0; i < nOnOUT.Length; i++)
                    {
                        if (nOnOUT[i] < 0) continue;

                        if (GbVar.GB_OUTPUT[nOnOUT[i]] == 0)
                        {
                            bOutputOnCheck = false;
                            break;
                        }
                    }

                    if (!bOutputOnCheck)
                    {
                        for (int i = 0; i < nOnOUT.Length; i++)
                        {
                            if (nOnOUT[i] < 0) continue;

                            MotionMgr.Inst.SetOutput(nOnOUT[i], true);
                        }
                    }

                    timeStamp.Restart();
                    break;

                case runCmd.check:
                    if (timeStamp.Elapsed.TotalMilliseconds < 5000)
                    {
                        for (int i = 0; i < nOffIN.Length; i++)
                        {
                            if (nOffIN[i] < 0) continue;
                            bOffCheck = !IsAVacOn(nOffIN[i]) & bOffCheck;

#if !_NOTEBOOK
                            //MotionMgr.Inst.SetInput(nOffIN[i], false, nMode);
#endif
                        }
                        if (bOffCheck)
                        {
                            if (lDelay > 0)
                            {
                                timeStamp.Restart();
                                break;
                            }
                            //Blow Off

                            for (int i = 0; i < nOnOUT.Length; i++)
                            {
                                if (nOnOUT[i] < 0) continue;

                                MotionMgr.Inst.SetOutput(nOnOUT[i], false);
                            }
                            return FNC.SUCCESS;
                        }
                        return FNC.BUSY;
                    }
                    else
                    {
                        ResetCmd();
                        for (int i = 0; i < nOffIN.Length; i++)
                        {
                            if (nOffIN[i] < 0) continue;
                            if (IsAVacOn(nOffIN[i]))
                            {
                                return nErr[i];
                            }
                        }
                        return nErr[0];
                    }

                case runCmd.delay:
                    if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                    {
                        //for (int i = 0; i < nOffIN.Length; i++)
                        //{
                        //    if (nOffIN[i] < 0) continue;
                        //    if (MotionMgr.inst.GetInput(nOffIN[i])) nCmd = runCmd.execute;
                        //}
                        return FNC.BUSY;
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                ResetCmd();
                timeStamp.Stop();
                //Blow Off
                for (int i = 0; i < nOnOUT.Length; i++)
                {
                    if (nOnOUT[i] < 0) continue;

                    MotionMgr.Inst.SetOutput(nOnOUT[i], false);
                }
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }

        public bool IsAInputLow(int nInputNo)
        {
            return ((GbVar.GB_AINPUT[nInputNo] - ConfigMgr.Inst.Cfg.Vac[nInputNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nInputNo].dRatio) 
                < ConfigMgr.Inst.Cfg.Vac[nInputNo].dVacLevelLow;
        }

        public bool IsAVacOn(int nInputNo)
        {
            return IsAInputLow(nInputNo);
        }

        public bool IsAInputMiddle(int nInputNo)
        {
            return !IsAInputLow(nInputNo) && !IsAInputHigh(nInputNo);
        }

        public bool IsAInputHigh(int nInputNo)
        {
            return ((GbVar.GB_AINPUT[nInputNo] - ConfigMgr.Inst.Cfg.Vac[nInputNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nInputNo].dRatio)
                > ConfigMgr.Inst.Cfg.Vac[nInputNo].dVacLevelHigh;
        }

        public bool IsAVacOff(int nInputNo)
        {
            return IsAInputHigh(nInputNo);
        }


        public int IsInputOnOff(int[] nOnIn, int[] nOffIn, long lTimeOut, int[] nErr, long lDelay)
        {
            bool bOnCheck = true;
            bool bOffCheck = true;

            switch (nCmd)
            {
                case runCmd.execute:
                    {
                        //시작단계에서 완료확인
                        if (lDelay <= 0)
                        {
                            for (int i = 0; i < nOnIn.Length; i++)
                            {
                                if (nOnIn[i] < 0) continue;
                                bOnCheck = (GbVar.GB_INPUT[nOnIn[i]] == 1) & bOnCheck;
                            }
                            for (int i = 0; i < nOffIn.Length; i++)
                            {
                                if (nOffIn[i] < 0) continue;
                                bOffCheck = (GbVar.GB_INPUT[nOffIn[i]] == 0) & bOffCheck;
                            }
                            if (bOnCheck && bOffCheck)
                            {
                                return FNC.SUCCESS;
                            }
                        }
                        timeStamp.Restart();
                        break;
                    }

                case runCmd.check:
                    if (timeStamp.Elapsed.TotalMilliseconds < lTimeOut)
                    {
                        for (int i = 0; i < nOnIn.Length; i++)
                        {
                            if (nOnIn[i] < 0) continue;
                            bOnCheck = (GbVar.GB_INPUT[nOnIn[i]] == 1) & bOnCheck;
                        }
                        for (int i = 0; i < nOffIn.Length; i++)
                        {
                            if (nOffIn[i] < 0) continue;
                            bOffCheck = (GbVar.GB_INPUT[nOffIn[i]] == 0) & bOffCheck;
                        }
                        if (bOnCheck && bOffCheck)
                        {
                            if (lDelay > 0)
                            {
                                timeStamp.Restart();
                                break;
                            }
                            return FNC.SUCCESS;
                        }
                        return FNC.BUSY;
                    }
                    else
                    {
                        ResetCmd();
                        for (int i = 0; i < nOnIn.Length; i++)
                        {
                            if (nOnIn[i] < 0) continue;
                            if (nErr.Length - 1 < i) continue;
                            if (GbVar.GB_INPUT[nOnIn[i]] == 0)
                            {
                                return nErr[i];
                            }
                        }
                        for (int i = 0; i < nOffIn.Length; i++)
                        {
                            if (nOffIn[i] < 0) continue;
                            if (nErr.Length - 1 < i) continue;
                            if (GbVar.GB_INPUT[nOffIn[i]] == 1)
                            {
                                return nErr[i];
                            }
                        }
                        return nErr[0];
                    }

                case runCmd.delay:
                    if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                    {
                        for (int i = 0; i < nOnIn.Length; i++)
                        {
                            if (nOnIn[i] < 0) continue;
                            if (GbVar.GB_INPUT[nOnIn[i]] == 0) nCmd = runCmd.execute;
                        }
                        for (int i = 0; i < nOffIn.Length; i++)
                        {
                            if (nOffIn[i] < 0) continue;
                            if (GbVar.GB_INPUT[nOffIn[i]] == 1) nCmd = runCmd.execute;
                        }
                        return FNC.BUSY;
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }



        public int IsVacBlowOnOff(int[] nOnIn, int[] nOffIn, long lTimeOut, int[] nErr, long lDelay)
        {
            bool bOnCheck = true;
            bool bOffCheck = true;

            switch (nCmd)
            {
                case runCmd.execute:
                    {
                        //시작단계에서 완료확인
                        if (lDelay <= 0)
                        {
                            for (int i = 0; i < nOnIn.Length; i++)
                            {
                                if (nOnIn[i] < 0) continue;
                                bOnCheck = (IsAVacOn(nOnIn[i])) & bOnCheck;
                            }
                            for (int i = 0; i < nOffIn.Length; i++)
                            {
                                if (nOffIn[i] < 0) continue;
                                bOffCheck = (IsAVacOff(nOffIn[i])) & bOffCheck;
                            }
                            if (bOnCheck && bOffCheck)
                            {
                                return FNC.SUCCESS;
                            }
                        }
                        timeStamp.Restart();
                        break;
                    }

                case runCmd.check:
                    if (timeStamp.Elapsed.TotalMilliseconds < lTimeOut)
                    {
                        for (int i = 0; i < nOnIn.Length; i++)
                        {
                            if (nOnIn[i] < 0) continue;
                            bOnCheck = (IsAVacOn(nOnIn[i])) & bOnCheck;
                        }
                        for (int i = 0; i < nOffIn.Length; i++)
                        {
                            if (nOffIn[i] < 0) continue;
                            bOffCheck = (IsAVacOff(nOffIn[i])) & bOffCheck;
                        }
                        if (bOnCheck && bOffCheck)
                        {
                            if (lDelay > 0)
                            {
                                timeStamp.Restart();
                                break;
                            }
                            return FNC.SUCCESS;
                        }
                        return FNC.BUSY;
                    }
                    else
                    {
                        ResetCmd();
                        for (int i = 0; i < nOnIn.Length; i++)
                        {
                            if (nOnIn[i] < 0) continue;
                            if (nErr.Length - 1 < i) continue;
                            if (!IsAVacOff(nOnIn[i]))
                            {
                                return nErr[i];
                            }
                        }
                        for (int i = 0; i < nOffIn.Length; i++)
                        {
                            if (nOffIn[i] < 0) continue;
                            if (nErr.Length - 1 < i) continue;
                            if (IsAVacOff(nOffIn[i]))
                            {
                                return nErr[i];
                            }
                        }
                        return nErr[0];
                    }

                case runCmd.delay:
                    if (timeStamp.Elapsed.TotalMilliseconds < lDelay)
                    {
                        for (int i = 0; i < nOnIn.Length; i++)
                        {
                            if (nOnIn[i] < 0) continue;
                            if (!IsAVacOff(nOnIn[i])) nCmd = runCmd.execute;
                        }
                        for (int i = 0; i < nOffIn.Length; i++)
                        {
                            if (nOffIn[i] < 0) continue;
                            if (IsAVacOff(nOffIn[i])) nCmd = runCmd.execute;
                        }
                        return FNC.BUSY;
                    }
                    break;
            }
            nCmd++;
            if (nCmd > runCmd.delay)
            {
                ResetCmd();
                timeStamp.Stop();
                return FNC.SUCCESS;
            }
            return FNC.BUSY;
        }
    }
}