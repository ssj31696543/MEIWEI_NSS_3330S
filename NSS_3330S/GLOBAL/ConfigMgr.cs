using DionesTool.Objects;
using DionesTool.UTIL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S
{
    class ConfigMgr
    {
        private static volatile ConfigMgr instance;
        private static object syncRoot = new Object();

        public Config Cfg;
        public Config TempCfg;

        StringBuilder m_sbLog = new StringBuilder();

        public static ConfigMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ConfigMgr();
                    }
                }

                return instance;
            }
        }
        public ConfigMgr()
        {
            //
        }

        ~ConfigMgr()
        {
        }

        public void Init(bool bStartProgram = false)
        {
            Cfg = Config.Deserialize(PathMgr.Inst.FILE_PATH_CFG);
            if (Cfg == null)
            {
                Cfg = new Config();
                Cfg.MotData = new CfgMotionData[SVDF.SV_CNT];
                for (int nSvCnt = 0; nSvCnt < Cfg.MotData.Length; nSvCnt++)
                    Cfg.MotData[nSvCnt] = new CfgMotionData(nSvCnt);
            }

            CheckValidation(Cfg, bStartProgram);
            CopyTempCfg();
            ////TempCfg
            //TempCfg = Config.Deserialize(PathMgr.Inst.FILE_PATH_CFG);
            //if (TempCfg == null)
            //{
            //    TempCfg = new Config();
            //}

            Cfg.Serialize(PathMgr.Inst.FILE_PATH_CFG);
        }

        void CheckValidation(Config cfg, bool bStartProgram)
        {
            if (cfg == null)
            {
                cfg = new Config();
            }

            if (bStartProgram)
            {
                cfg.General.isDoorAlarmSkip = false;
            }

            if (cfg.Vac.Length != (int)IODF.A_INPUT.MAX)
            {
                cfg.Vac = new VacuumOption[(int)IODF.A_INPUT.MAX];
            }
            if (cfg.AOut.Length != (int)IODF.A_OUTPUT.MAX)
            {
                cfg.AOut = new AnalogOut[(int)IODF.A_OUTPUT.MAX];
            }
            for (int nOutCnt = 0; nOutCnt < (int)IODF.A_OUTPUT.MAX; nOutCnt++)
            {
                if (cfg.AOut[nOutCnt] == null)
                    cfg.AOut[nOutCnt] = new AnalogOut();
            }
            for (int nVacCnt = 0; nVacCnt < (int)IODF.A_INPUT.MAX; nVacCnt++)
            {
                if (cfg.Vac[nVacCnt] == null)
                    cfg.Vac[nVacCnt] = new VacuumOption();
            }

            if (cfg.itemOptions.Length != (int)OPTNUM.MAX)
            {
                cfg.itemOptions = new OptionItem[(int)OPTNUM.MAX];
            }

            Config _tmp = new Config();
            for (int i = 0; i < cfg.itemOptions.Length; i++)
            {
                cfg.itemOptions[i].strOptionPart = _tmp.itemOptions[i].strOptionPart;
                cfg.itemOptions[i].strOptionName = _tmp.itemOptions[i].strOptionName;
            }

            #region 현재 사용하지 않는 값 고정
            cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_MODE_VERTICAL_USE].bOptionUse = true;

            cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse = true;
            cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ONLY_REVERSE_USE].bOptionUse = true;
            #endregion

            #region Motor Data
            if (cfg.dMotPos == null)
            {
                Cfg.MotData = new CfgMotionData[SVDF.SV_CNT];
            }

            if (cfg.dMotPos.Length < SVDF.SV_CNT)
            {
                cfg.MotData = (CfgMotionData[])GbFunc.ResizeArray(Cfg.MotData, SVDF.SV_CNT);
            }

            for (int nSvCnt = 0; nSvCnt < Cfg.dMotPos.Length; nSvCnt++)
            {
                if (cfg.MotData[nSvCnt] == null)
                    Cfg.MotData[nSvCnt] = new CfgMotionData(nSvCnt);
                Cfg.MotData[nSvCnt].strAxisName = SVDF.GetAxisName(nSvCnt);

                if (cfg.MotData[nSvCnt].dVel <= 0) cfg.MotData[nSvCnt].dVel = 10;
                if (cfg.MotData[nSvCnt].dAcc <= cfg.MotData[nSvCnt].dVel || cfg.MotData[nSvCnt].dAcc <= 0)
                    cfg.MotData[nSvCnt].dAcc = cfg.MotData[nSvCnt].dVel * 5;
                if (cfg.MotData[nSvCnt].dDec <= cfg.MotData[nSvCnt].dVel || cfg.MotData[nSvCnt].dDec <= 0)
                    cfg.MotData[nSvCnt].dDec = cfg.MotData[nSvCnt].dVel * 5;
                if (cfg.MotData[nSvCnt].dInpositionBand < 0) cfg.MotData[nSvCnt].dInpositionBand = 0.1;
            }

            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            {
                if (cfg.OffsetPnP[0].dPnpOffsetZ[i] == null)
                    cfg.OffsetPnP[0].dPnpOffsetZ[i] = new CfgPickerPadOffset();
                if (cfg.OffsetPnP[0].dPnpOffsetT[i] == null)
                    cfg.OffsetPnP[0].dPnpOffsetT[i] = new CfgPickerPadOffset();
                if (cfg.OffsetPnP[1].dPnpOffsetZ[i] == null)
                    cfg.OffsetPnP[1].dPnpOffsetZ[i] = new CfgPickerPadOffset();
                if (cfg.OffsetPnP[1].dPnpOffsetT[i] == null)
                    cfg.OffsetPnP[1].dPnpOffsetT[i] = new CfgPickerPadOffset();
            }
            #endregion
        }

        void ArrayCheck(ref double[] dSource, int nMaxCount)
        {
            double[] dBackup;
            if (dSource.Length != nMaxCount)
            {
                dBackup = new double[dSource.Length];
                Array.Copy(dSource, dBackup, dBackup.Length);

                dSource = new double[nMaxCount];
                Array.Copy(dBackup, dSource, dBackup.Length > dSource.Length ? dSource.Length : dBackup.Length);
            }
        }

        void ArrayCheck(ref int[] nSource, int nMaxCount)
        {
            int[] nBackup;
            if (nSource.Length != nMaxCount)
            {
                nBackup = new int[nSource.Length];
                Array.Copy(nSource, nBackup, nBackup.Length);

                nSource = new int[nMaxCount];
                Array.Copy(nBackup, nSource, nBackup.Length > nSource.Length ? nSource.Length : nBackup.Length);
            }
        }

        void ArrayCheck(ref CfgPickerHeadOffset[] nSource, int nMaxCount)
        {
            CfgPickerHeadOffset[] nBackup;
            if (nSource.Length != nMaxCount)
            {
                nBackup = new CfgPickerHeadOffset[nSource.Length];
                Array.Copy(nSource, nBackup, nBackup.Length);

                nSource = new CfgPickerHeadOffset[nMaxCount];
                Array.Copy(nBackup, nSource, nBackup.Length > nSource.Length ? nSource.Length : nBackup.Length);
            }

            for (int nCnt = 0; nCnt < nSource.Length; nCnt++)
            {
                if (nSource[nCnt] == null)
                    nSource[nCnt] = new CfgPickerHeadOffset();
            }
        }

        public void Save()
        {
            Cfg.Serialize(PathMgr.Inst.FILE_PATH_CFG);

        }

        public void SaveBackup()
        {
            //BACKUP 폴더 생성
            string strPath = string.Format("{0}{1}", PathMgr.Inst[PathMgr.ePathInfo.eConfig], "BACKUP");

            //폴더 경로 생성
            if (!Directory.Exists(strPath))
                Directory.CreateDirectory(strPath);

            //백업 파일 생성
            Cfg.Serialize(PathMgr.Inst.FILE_PATH_BACKUP_CFG);            
        }

        public void CopyTempCfg()
        {
            TempCfg = CreateDeepCopy(Cfg);
        }

        public void SaveTempCfg()
        {
            m_sbLog.Clear();
            CompareValue(TempCfg, Cfg);

            if (m_sbLog.Length > 0)
            {
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("[Change Config]\r\n{0}", m_sbLog.ToString()));
            }

            Cfg = CreateDeepCopy(TempCfg);
        }

        private Config CreateDeepCopy(Config inputcls)
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, inputcls);
            m.Position = 0;
            return (Config)b.Deserialize(m);
        }

        #region WriteLogCompare
        void WriteLogCompare(int objNew, int objOld, string objName)
        {
            if (Equals(objNew, objOld) == false)
            {
                m_sbLog.AppendFormat("{0} : [{1}] -> [{2}],\r\n", objName, objOld, objNew);
            }
        }

        void WriteLogCompare(double objNew, double objOld, string objName)
        {
            if (Equals(objNew, objOld) == false)
            {
                m_sbLog.AppendFormat("{0} : [{1}] -> [{2}],\r\n", objName, objOld, objNew);
            }
        }

        void WriteLogCompare(string objNew, string objOld, string objName)
        {
            if (Equals(objNew, objOld) == false)
            {
                m_sbLog.AppendFormat("{0} : [{1}] -> [{2}],\r\n", objName, objOld, objNew);
            }
        }

        void WriteLogCompare(bool objNew, bool objOld, string objName)
        {
            if (Equals(objNew, objOld) == false)
            {
                m_sbLog.AppendFormat("{0} : [{1}] -> [{2}],\r\n", objName, objOld, objNew);
            }
        }

        #endregion

        public void CompareValue(Config cfgNew, Config cfgOld)
        {
            #region dMotPos
            int[] nAxisArray = new int[4];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_2_X;
            nAxisArray[2] = (int)SVDF.AXES.BALL_VISION_Y;
            nAxisArray[3] = (int)SVDF.AXES.BALL_VISION_Z;

            for (int nIdx = 0; nIdx < nAxisArray.Length; nIdx++)
            {
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_PICKER_HEAD_1_PITCH_OFFSET_READY],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_PICKER_HEAD_1_PITCH_OFFSET_READY],
                                string.Format("CFG_PICKER_HEAD_1_PITCH_OFFSET_READY").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_PICKER_HEAD_2_PITCH_OFFSET_READY],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_PICKER_HEAD_2_PITCH_OFFSET_READY],
                                string.Format("CFG_PICKER_HEAD_2_PITCH_OFFSET_READY").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY],
                                string.Format("CFG_PICKER_HEAD_1_VISION_OFFSET_READY").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_PICKER_HEAD_2_VISION_OFFSET_READY], 
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_PICKER_HEAD_2_VISION_OFFSET_READY],
                                string.Format("CFG_PICKER_HEAD_2_VISION_OFFSET_READY").Replace("_", " "));
            }

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_2_X;
            nAxisArray[2] = (int)SVDF.AXES.MAP_STG_1_Y;
            nAxisArray[3] = (int)SVDF.AXES.MAP_STG_2_Y;
            for (int nIdx = 0; nIdx < nAxisArray.Length; nIdx++)
            {

                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_START_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_P1],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_LAST_X_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_P1],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_LAST_Y_P1").Replace("_", " "));

                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P1],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_START_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_P1],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_LAST_X_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_P1],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_LAST_Y_P1").Replace("_", " "));

                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P2],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_START_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_P2],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_LAST_X_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_P2],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_LAST_Y_P2").Replace("_", " "));

                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P2],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_START_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_P2],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_LAST_X_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_P2],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_LAST_Y_P2").Replace("_", " "));

            }
            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y;
            nAxisArray[2] = (int)SVDF.AXES.MAP_STG_2_Y;
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {

                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_MV],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_MV],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_START_MV").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_MV],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_MV],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_LAST_X_MV").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_MV],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_MV],
                                string.Format("CFG_MAP_TABLE_1_OFFSET_LAST_Y_MV").Replace("_", " "));

                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_MV],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_MV],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_START_MV").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_MV],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_MV],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_LAST_X_MV").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_MV],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_MV],
                                string.Format("CFG_MAP_TABLE_2_OFFSET_LAST_Y_MV").Replace("_", " "));
            }
            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;
            nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y;

            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1],
                          cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1],
                          string.Format("CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1],
                                string.Format("CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1],
                                string.Format("CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1").Replace("_", " "));
            }

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;
            nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1],
                                string.Format("CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P1],
                                string.Format("CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P1],
                                string.Format("CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P1").Replace("_", " "));
            }

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;
            nAxisArray[1] = (int)SVDF.AXES.RW_TRAY_STG_Y;

            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1],
                                string.Format("CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P1],
                                string.Format("CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P1").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P1],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P1],
                                string.Format("CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P1").Replace("_", " "));
            }

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_2_X;
            nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y;

            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2").Replace("_", " "));
            }


            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_2_X;
            nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_2_OFFSET_START_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P2").Replace("_", " "));
            }

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_2_X;
            nAxisArray[1] = (int)SVDF.AXES.RW_TRAY_STG_Y;

            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_3_OFFSET_START_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P2").Replace("_", " "));
                WriteLogCompare(cfgNew.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P2],
                                cfgOld.dMotPos[nAxisArray[nIdx]].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P2],
                                string.Format("CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P2").Replace("_", " "));
            }
            
            #endregion

            #region General
            WriteLogCompare(Enum.GetName(typeof(Language), cfgNew.General.nLanguage), Enum.GetName(typeof(Language), cfgOld.General.nLanguage), string.Format("LANGUAGE CHANGED"));
            #endregion

            #region Operator

            for (int nIdx = 0; nIdx < (int)OPTNUM.MAX; nIdx++)
            {
                if (cfgNew.itemOptions[nIdx] == null ||
                    cfgOld.itemOptions[nIdx] == null ||
                    !Enum.IsDefined(typeof(OPTNUM), nIdx))
                {
                    continue;
                }
                WriteLogCompare(cfgNew.itemOptions[nIdx].bOptionUse, cfgOld.itemOptions[nIdx].bOptionUse, Enum.GetName(typeof(OPTNUM), nIdx));
            }
            #endregion

            #region MotData
            for (int nIdx = 0; nIdx < (int)SVDF.AXES.MAX; nIdx++)
            {
                WriteLogCompare(cfgNew.MotData[nIdx].dVel, cfgOld.MotData[nIdx].dVel, string.Format("{0} Velocity", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dAcc, cfgOld.MotData[nIdx].dAcc, string.Format("{0} Accel", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dDec, cfgOld.MotData[nIdx].dDec, string.Format("{0} Decel", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dHomeOffset, cfgOld.MotData[nIdx].dHomeOffset, string.Format("{0} HomeOffset", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dJogLowVel, cfgOld.MotData[nIdx].dJogLowVel, string.Format("{0} Jog Low Vel", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dJogMdlVel, cfgOld.MotData[nIdx].dJogMdlVel, string.Format("{0} Jog Mid Vel", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dJogHighVel, cfgOld.MotData[nIdx].dJogHighVel, string.Format("{0} Jog High Vel", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dInpositionBand, cfgOld.MotData[nIdx].dInpositionBand, string.Format("{0} Inposition", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].lMoveTime, cfgOld.MotData[nIdx].lMoveTime, string.Format("{0} Move Time", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].lHomeTime, cfgOld.MotData[nIdx].lHomeTime, string.Format("{0} Home Time", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dSwLimitN, cfgOld.MotData[nIdx].dSwLimitN, string.Format("{0} SW (-) Limit", Enum.GetName(typeof(SVDF.AXES), nIdx)));
                WriteLogCompare(cfgNew.MotData[nIdx].dSwLimitP, cfgOld.MotData[nIdx].dSwLimitP, string.Format("{0} SW (+) Limit", Enum.GetName(typeof(SVDF.AXES), nIdx)));
            }
            #endregion

            #region Vac
            {
                string strVacName = "";
                for (int nVacCnt = 0; nVacCnt < (int)IODF.A_INPUT.MAX; nVacCnt++)
                {
                    strVacName = "ANALOGVAC" + nVacCnt.ToString();
                    if (Enum.IsDefined(typeof(IODF.A_INPUT), nVacCnt)) strVacName = ((IODF.A_INPUT)nVacCnt).ToString();
                    WriteLogCompare(cfgNew.Vac[nVacCnt].lVacOnDelay, cfgOld.Vac[nVacCnt].lVacOnDelay, string.Format("{0} VACUUM ON TIME", strVacName));
                    WriteLogCompare(cfgNew.Vac[nVacCnt].lBlowOffDelay, cfgOld.Vac[nVacCnt].lBlowOffDelay, string.Format("{0} VACUUM OFF TIME", strVacName));
                    WriteLogCompare(cfgNew.Vac[nVacCnt].lBlowOnDelay, cfgOld.Vac[nVacCnt].lBlowOnDelay, string.Format("{0} BLOW ON TIME", strVacName));
                    WriteLogCompare(cfgNew.Vac[nVacCnt].dVacLevelLow, cfgOld.Vac[nVacCnt].dVacLevelLow, string.Format("{0} VACUUM LOW LEVEL", strVacName));
                    WriteLogCompare(cfgNew.Vac[nVacCnt].dVacLevelHigh, cfgOld.Vac[nVacCnt].dVacLevelHigh, string.Format("{0} VACUUM HIGH LEVEL", strVacName));
                    WriteLogCompare(cfgNew.Vac[nVacCnt].dRatio, cfgOld.Vac[nVacCnt].dRatio, string.Format("{0} VOLTAGE AND PRESSURE RATIO", strVacName));
                    WriteLogCompare(cfgNew.Vac[nVacCnt].dDefaultVoltage, cfgOld.Vac[nVacCnt].dDefaultVoltage, string.Format("{0} DEFAULT VOLTAGE", strVacName));
                }
            }
            #endregion

            #region cfgOption
            {
                for (int nOptNum = 0; nOptNum < (int)OPTNUM.MAX; nOptNum++)
                {
                    WriteLogCompare(cfgNew.itemOptions[nOptNum].strValue, cfgOld.itemOptions[nOptNum].strValue,
                        string.Format("{0} {1}", cfgOld.itemOptions[nOptNum].strOptionPart, cfgOld.itemOptions[nOptNum].strOptionName));
                }
            }

            #endregion

            #region OffsetPnP
            {
                string[] strTableName = new string[] { "GOOD TRAY STAGE1", "GOOD TRAY STAGE2", "REWORK TRAY STAGE", "REJECT BOX" };

                for (int nHeadNo = 0; nHeadNo < 2; nHeadNo++)
                {
                    WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dDefPickerPitchX, cfgOld.OffsetPnP[nHeadNo].dDefPickerPitchX,
                        string.Format("PICKER{0} DEFAULT PICKER PITCH X", nHeadNo + 1));
                    WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dDefPickerPitchY, cfgOld.OffsetPnP[nHeadNo].dDefPickerPitchY,
                        string.Format("PICKER{0} DEFAULT PICKER PITCH Y", nHeadNo + 1));

                    for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
                    {
                        for (int nDegreeNo = 0; nDegreeNo < CFG_DF.MAX_DEGREE_NO; nDegreeNo++)
                        {
                            WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegreeNo][nPadNo].x, cfgOld.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegreeNo][nPadNo].x,
                                string.Format("PICKER{0} PAD{1} PICKER HEAD REFERENCE X", nHeadNo + 1, nPadNo + 1));
                            WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegreeNo][nPadNo].y, cfgOld.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegreeNo][nPadNo].y,
                                string.Format("PICKER{0} PAD{1} PICKER HEAD REFERENCE Y", nHeadNo + 1, nPadNo + 1));
                        }

                        for (int nDegreeNo = 0; nDegreeNo < CFG_DF.MAX_DEGREE_NO; nDegreeNo++)
                        {
                            WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegreeNo][nPadNo].x, cfgOld.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegreeNo][nPadNo].x,
                                string.Format("PICKER{0} PAD{1} PICKER HEAD REFERENCE INSP X", nHeadNo + 1, nPadNo + 1));
                            WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegreeNo][nPadNo].y, cfgOld.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegreeNo][nPadNo].y,
                                string.Format("PICKER{0} PAD{1} PICKER HEAD REFERENCE INSP Y", nHeadNo + 1, nPadNo + 1));
                        }

                        for (int nTableNo = 0; nTableNo < CFG_DF.TRAY_STG_CNT; nTableNo++)
                        {
                            if (nTableNo < 2)
                            {
                                WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dPnpOffsetZ[nPadNo].dPickUpWait[nTableNo], cfgOld.OffsetPnP[nHeadNo].dPnpOffsetZ[nPadNo].dPickUpWait[nTableNo],
                                    string.Format("PICKER{0} PAD{1} MAP STAGE{2} PICK UP WAIT", nHeadNo + 1, nPadNo + 1, nTableNo + 1));
                                WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dPnpOffsetZ[nPadNo].dPickUp[nTableNo], cfgOld.OffsetPnP[nHeadNo].dPnpOffsetZ[nPadNo].dPickUp[nTableNo],
                                    string.Format("PICKER{0} PAD{1} MAP STAGE{2} PICK UP", nHeadNo + 1, nPadNo + 1, nTableNo + 1));
                            }

                            WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dPnpOffsetZ[nPadNo].dPlaceWait[nTableNo], cfgOld.OffsetPnP[nHeadNo].dPnpOffsetZ[nPadNo].dPlaceWait[nTableNo],
                                string.Format("PICKER{0} PAD{1} TRAY STAGE{2} PLACE WAIT", nHeadNo + 1, nPadNo + 1, nTableNo + 1));
                            WriteLogCompare(cfgNew.OffsetPnP[nHeadNo].dPnpOffsetZ[nPadNo].dPlace[nTableNo], cfgOld.OffsetPnP[nHeadNo].dPnpOffsetZ[nPadNo].dPlace[nTableNo],
                                string.Format("PICKER{0} PAD{1} {2} PLACE", nHeadNo + 1, nPadNo + 1, strTableName[nTableNo]));
                        }
                    }
                }
            }
            #endregion

            #region OffsetCnP
            {
                for (int nHeadNo = 0; nHeadNo < 2; nHeadNo++)
                {
                    WriteLogCompare(cfgNew.OffsetCnP[nHeadNo].dDefPickerOffsetX, cfgOld.OffsetCnP[nHeadNo].dDefPickerOffsetX, string.Format("PICKER{0} CAMERA AND PICKER OFFSET X", nHeadNo + 1));
                    WriteLogCompare(cfgNew.OffsetCnP[nHeadNo].dDefPickerOffsetY, cfgOld.OffsetCnP[nHeadNo].dDefPickerOffsetY, string.Format("PICKER{0} CAMERA AND PICKER OFFSET Y", nHeadNo + 1));
                }
            }
            #endregion

            #region OffsetXyzt
            string[] sHeadNo = { "HEAD_1 ", "HEAD_2 " };
            string[] sTableNo = { "MAP_1 ", "MAP_2 ", "GOOD_1 ", "GOOD_2 ", "REWORK " };
            string[] sAxisNo = { "OFFSET_X ", "OFFSET_Y ", "OFFSET_Z ", "OFFSET_T " };
            string[] sPadNo = { "PAD_1 ", "PAD_2 ", "PAD_3 ", "PAD_4 ", "PAD_5 ", "PAD_6 ", "PAD_7 ", "PAD_8 " };

            for (int i = 0; i < sHeadNo.Length; i++)
            {
                for (int j = 0; j < sTableNo.Length; j++)
                {
                    for (int k = 0; k < sAxisNo.Length; k++)
                    {
                        for (int l = 0; l < sPadNo.Length; l++)
                        {
                            WriteLogCompare(cfgNew.dOffsetXYZT[i][j][k][l], cfgOld.dOffsetXYZT[i][j][k][l], sHeadNo[i] + sTableNo[j] + sAxisNo[k] + sPadNo[l]);
                        }
                    }
                }
            }
            #endregion

        }

        public bool DataCheckBeforeSave()
        {
            // 잘못된 값이 있는지 확인
            bool bSuccess = true;
            string strMsg = "Invalid Value Detected";

            if (!ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.PICKER_1_USE].bOptionUse && 
                !ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.PICKER_2_USE].bOptionUse)
            {
                strMsg = string.Format("Name: {0} \nValue: {1} \nReason: {2}", "PICKER_1_USAGE", ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.PICKER_1_USE].bOptionUse,
                    "Can not skip both picker");
                bSuccess &= false;
            }

            if (!ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.MAP_STAGE_1_USE].bOptionUse &&
                !ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.MAP_STAGE_2_USE].bOptionUse)
            {
                strMsg = string.Format(FormTextLangMgr.FindKey("맵스테이지 1번과 2번 모두 SKIP할 수 없습니다.") + "\r\n" + FormTextLangMgr.FindKey("반드시 둘중 하나는 USE로 전환하십시오."));
                bSuccess &= false;
            }

			if (!ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_1_USE].bOptionUse &&
                !ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_2_USE].bOptionUse)
            {
                strMsg = string.Format(FormTextLangMgr.FindKey("GOOD 트레이 스테이지 1번과 2번 모두 SKIP할 수 없습니다.") + "\r\n" + FormTextLangMgr.FindKey("반드시 둘중 하나는 USE로 전환하십시오."));
                bSuccess &= false;
            }

            #region MotData
            for (int i = 0; i < SVDF.SV_CNT; i++)
            {
                // 최대속도 이상으로 속도를 설정할 수 없음
                if (ConfigMgr.Inst.TempCfg.MotData[i].dVel > ConfigMgr.Inst.TempCfg.MotData[i].dMaxVel)
                {
                    strMsg += "\n" + string.Format("Name: {0} Value: {1} Reason: {2}", SVDF.GetAxisName(i), ConfigMgr.Inst.TempCfg.dMotPos[i].dVel,
                    "Velocity can not over than MaxVel");
                    bSuccess &= false;
                }
                // 인포지션 밴드 1.0 이상 설정할 수 없음
                if (ConfigMgr.Inst.TempCfg.MotData[i].dInpositionBand > 5.0)
                {
                    strMsg += "\n" + string.Format("Name: {0} Value: {1} Reason: {2}", SVDF.GetAxisName(i), ConfigMgr.Inst.TempCfg.MotData[i].dInpositionBand,
                    "Inposition Band can not over than 1.0mm");
                    bSuccess &= false;
                }

                // Config 저장시 Motion Movetime 저장
                long lTime = ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.MOT_MOVE_TIME].lValue;
                ConfigMgr.Inst.TempCfg.MotData[i].lMoveTime = lTime;
            }
            #endregion

            if (!bSuccess)
            {
                POP.popMessageBox mbox = new POP.popMessageBox(strMsg, "FAIL TO SAVE DATA", System.Windows.Forms.MessageBoxButtons.OK);
                mbox.ShowDialog();
            }
            return bSuccess;
        }

        /// <summary>    
        /// 1번째 COL에서 부턱 픽업 할 COL 까지의 OFFSET 값 산출
        /// </summary>
        /// <param name="bFDir"></param>
        /// <param name="nTableCount"></param>
        /// <returns></returns>
        public double GetMapTablePickUpOffsetX(bool bFDir, int nTableCount)
        {
            double dStepOffsetX = 0.0f;

            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (bFDir == true)
            {
                dStepOffsetX = (nTableCount % nTotMapCountX) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX;

                if (nTableCount == RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX)
                {
                    dStepOffsetX += ((nTableCount % nTotMapCountX) * RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetX);
                }
            }
            else
            {
                int nPitchIdx = nTotMapCountX - (nTableCount % nTotMapCountX) - 1;
                dStepOffsetX = nPitchIdx * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX;

                if (nTableCount == RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX)
                {
                    dStepOffsetX += (nPitchIdx * RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetX);
                }
            }

            return dStepOffsetX;
        }

        public double GetMapTablePickUpOffsetY(int nTableCount)
        {
            double dStepOffsetY;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            dStepOffsetY = (nTableCount / nTotMapCountX) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY;

            if ((nTableCount / nTotMapCountX) % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX == 0)
            {
                dStepOffsetY += ((nTableCount / nTotMapCountX) * RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetY);
            }
            return dStepOffsetY;
        }

        public double GetGoodTrayTablePlaceOffsetX(bool bFDir, int nTableNo, int nPlaceTableCount)
        {
            double dStepOffsetX = 0.0f;

            // Table No에 따른 OFFSET 반드시 적용 필요 
            if (bFDir == true)
            {
                dStepOffsetX = (nPlaceTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) * RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchX;
            }
            else
            {
                int nPitchIdx = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nPlaceTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
                dStepOffsetX = nPitchIdx * RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchX;
            }

            return dStepOffsetX;
        }

        public double GetGoodTrayTablePlaceOffsetY(int nTableNo, int nTableCount)
        {
            double dStepOffsetY;

            //220606 pjh
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
            {
                dStepOffsetY = (RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX)) * -RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY;
            }
            else
            {
                dStepOffsetY = (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) * -RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY;
            }

            return dStepOffsetY;
        }

        public double GetReworkTrayTablePlaceOffsetX(int nPlaceTableCount)
        {
            double dStepOffsetX = 0.0f;

            int nPitchIdx = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nPlaceTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
            dStepOffsetX = nPitchIdx * RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchX;

            return dStepOffsetX;
        }

        public double GetReworkTrayTablePlaceOffsetY(int nTableCount)
        {
            double dStepOffsetY;

            //220606 pjh
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
            {
                dStepOffsetY = (RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX)) * -RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY;
            }
            else
            {
                dStepOffsetY = (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) * -RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY;
            }

            return dStepOffsetY;
        }

        public dxy GetPickerPitchOffset(bool bRotate, int nHeadNo, int nPickerNo)
        {
            dxy dpRtn;
            if (bRotate == true)
            {
                dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPickerNo].x;
                dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPickerNo].y;
                if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPickerNo].y;
                }
            }
            else
            {
                dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPickerNo].x;
                dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPickerNo].y;
            }

            return dpRtn;
        }

        public dxy GetPickerInspPitchOffset(bool bRotate, int nHeadNo, int nPickerNo)
        {
            dxy dpRtn;
            if (bRotate == true)
            {
                dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPickerNo].x;
                dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPickerNo].y;
                if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPickerNo].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270)
                {
                    dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPickerNo].x;
                    dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPickerNo].y;
                }
            }
            else
            {
                dpRtn.x = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPickerNo].x;
                dpRtn.y = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPickerNo].y;
            }

            return dpRtn;
        }

        public double GetPlaceAngleOffset(/*int nStageNo, int nHeadNo, int nPickerNo*/)
        {
            return RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle;

            //return ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dPnpOffsetT[nPickerNo].dPlace[nStageNo];
        }

        public double GetPickerPickUpWaitPosOffset(int nStageNo, int nHeadNo, int nPickerNo)
        {
            //대기위치 0으로 했을때 -리미트 에러 발생 
            if (ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dPnpOffsetZ[nPickerNo].dPickUpWait[nStageNo] - RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness < 0)
            {
                return 0;
            }
            return ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dPnpOffsetZ[nPickerNo].dPickUpWait[nStageNo] - RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness;
        }

        public double GetPickerPickUpDownPosOffset(int nStageNo, int nHeadNo, int nPickerNo)
        {
            return ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dPnpOffsetZ[nPickerNo].dPickUp[nStageNo] - RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness;
        }

        public double GetPickerPlaceWaitPosOffset(int nStageNo, int nHeadNo, int nPickerNo)
        {

            //대기위치 0으로 했을때 -리미트 에러 발생 
            if (ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dPnpOffsetZ[nPickerNo].dPlaceWait[nStageNo] - RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness < 0)
            {
                return 0;
            }
            return ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dPnpOffsetZ[nPickerNo].dPlaceWait[nStageNo] - RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness;
        }

        public double GetPickerPlaceDownPosOffset(int nStageNo, int nHeadNo, int nPickerNo)
        {
            //return ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dPnpOffsetZ[nPickerNo].dPlace[nStageNo];
            //칩 두께 티칭 및 영점 티칭 완료 되면 해당 주석 사용
            return ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dPnpOffsetZ[nPickerNo].dPlace[nStageNo] - RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness;
        }

        public dxy GetCamToPickerOffset(int nHeadNo, bool bRotate)
        {
            dxy dpRtn;

            if (bRotate)
            {
                double dViewPosX = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY + nHeadNo];
                double dViewPosY = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.BALL_VISION_Y].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY + nHeadNo];
                double dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].x;
                double dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].y;

                if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0)
                {
                    dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x;
                    dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 90)
                {
                    dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].x;
                    dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180)
                {
                    dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].x;
                    dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270)
                {
                    dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].x;
                    dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90)
                {
                    dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].x;
                    dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180)
                {
                    dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].x;
                    dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].y;
                }
                else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270)
                {
                    dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].x;
                    dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].y;
                }


                dpRtn.x = dViewPosX - dP1PosX;
                dpRtn.y = dViewPosY - dP1PosY;
            }
            else
            {
                double dViewPosX = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY + nHeadNo];
                double dViewPosY = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.BALL_VISION_Y].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY + nHeadNo];

                double dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x;
                double dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y;

                dpRtn.x = dViewPosX - dP1PosX;
                dpRtn.y = dViewPosY - dP1PosY;
            }

            return dpRtn;
        }

        public dxy GetCamToPickerupPos(int nHeadNo, double dVisionPosX, double dVisionPosY)
        {
            dxy dpRtn;

            //double dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x;
            //double dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y;
            double dP1PosX = ConfigMgr.Inst.Cfg.OffsetCnP[nHeadNo].dDefPickerOffsetX;
            //double dP1PosY = ConfigMgr.Inst.Cfg.OffsetCnP[nHeadNo].dDefPickerOffsetY;
            double dP1PosY = - ConfigMgr.Inst.Cfg.OffsetCnP[nHeadNo].dDefPickerOffsetY; //2024-02-01 sj.shinn 부호 반대
            dpRtn.x = dVisionPosX - dP1PosX;
            dpRtn.y = dVisionPosY + dP1PosY;

            return dpRtn;
        }

        public dxy GetCamToPlacePos(int nHeadNo, double dVisionPosX, double dVisionPosY)
        {
            dxy dpRtn;

            double dP1PosX = ConfigMgr.Inst.Cfg.OffsetCnP[nHeadNo].dDefPickerOffsetX;
            //double dP1PosY = ConfigMgr.Inst.Cfg.OffsetCnP[nHeadNo].dDefPickerOffsetY;
            double dP1PosY = - ConfigMgr.Inst.Cfg.OffsetCnP[nHeadNo].dDefPickerOffsetY; //sj.shin 2024-02-01 부호 반대

            // 2024-01-24 ic.choi 중국 출장에서 주석 처리 함. 적용  sj.shin
            //double dAngleOffsetX;
            //double dAngleOffsetY;

            //double dP1PosX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].x;
            //double dP1PosY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].y;

            //if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 90)
            //{
            //    dAngleOffsetX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].x;
            //    dAngleOffsetY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].y;

            //    dP1PosX += dAngleOffsetX;
            //    dP1PosY += dAngleOffsetY;
            //}
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180)
            //{
            //    dAngleOffsetX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].x;
            //    dAngleOffsetY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].y;

            //    dP1PosX += dAngleOffsetX;
            //    dP1PosY += dAngleOffsetY;
            //}
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270)
            //{
            //    dAngleOffsetX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].x;
            //    dAngleOffsetY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].y;

            //    dP1PosX += dAngleOffsetX;
            //    dP1PosY += dAngleOffsetY;
            //}
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90)
            //{
            //    dAngleOffsetX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].x;
            //    dAngleOffsetY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].y;

            //    dP1PosX += dAngleOffsetX;
            //    dP1PosY += dAngleOffsetY;
            //}
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180)
            //{
            //    dAngleOffsetX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].x;
            //    dAngleOffsetY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].y;

            //    dP1PosX += dAngleOffsetX;
            //    dP1PosY += dAngleOffsetY;
            //}
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270)
            //{
            //    dAngleOffsetX = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].x;
            //    dAngleOffsetY = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y - ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].y;

            //    dP1PosX += dAngleOffsetX;
            //    dP1PosY += dAngleOffsetY;
            //}


            dpRtn.x = dVisionPosX - dP1PosX;
            dpRtn.y = dVisionPosY + dP1PosY;
            //dpRtn.y = dVisionPosY - dP1PosY;

            return dpRtn;
        }

        public dxy GetPickerHeadOffset()
        {
            dxy dpRtn;

            double dViewPosX1 = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY];
            double dViewPosY1 = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.BALL_VISION_Y].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY];

            double dViewPosX2 = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_PICKER_HEAD_2_VISION_OFFSET_READY];
            double dViewPosY2 = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.BALL_VISION_Y].dPos[POSDF.CFG_PICKER_HEAD_2_VISION_OFFSET_READY];

            //double dViewPosX1 = ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x;
            //double dViewPosY1 = ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y;

            //double dViewPosX2 = ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_2].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x;
            //double dViewPosY2 = ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_2].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y;


            dpRtn.x = dViewPosX1 - dViewPosX2;
            dpRtn.y = dViewPosY2 - dViewPosY1;

            return dpRtn;
        }

        public dxy GetMapStageToStageOffset()
        {
            dxy dpRtn;

            dpRtn.x = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1] - ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P1];
            dpRtn.y = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1] - ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P1];

            return dpRtn;
        }

        public dxy[] GetTrayTableToTableOffset()
        {
            dxy[] dpRtn = new dxy[2];

            dpRtn[0].x = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1] - ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1];
            dpRtn[0].y = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1] - ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1];

            dpRtn[1].x = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1] - ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1];
            dpRtn[1].y = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1] - ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1];

            return dpRtn;
        }

        public dxy GetMapStageInspCountAngleOffset(int nStageNo, double dStartPosX, double dStartPosY, double dTargetPosX, double dTargetPosY)
        {
            double dHorAngle = 0.0f, dVerAngle = 0.0f;
            dxy dpStageAngleOffset = new dxy(0.0f, 0.0f);

            switch (nStageNo)
            {
                case 0:
                    dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_MV]);

                    dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_MV]);

                    break;

                case 1:
                    dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_MV]);

                    dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_MV],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_MV]);

                    break;


                default:
                    break;
            }

            dpStageAngleOffset = MathUtil.GetAnglePos(dStartPosX, dStartPosY, dVerAngle - 90, dTargetPosX, dTargetPosY);
            return dpStageAngleOffset;
        }

        public dxy GetMapStagePickUpCountAngleOffset(int nStageNo, int nHeadNo, double dStartPosX, double dStartPosY, double dTargetPosX, double dTargetPosY)
        {
            double dHorAngle = 0.0f, dVerAngle = 0.0f;
            dxy dpStageAngleOffset = new dxy(0.0f, 0.0f);

            switch (nStageNo)
            {
                case 0:
                    dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1 + (10 *  + nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1 + (10 * +nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_P1 + (10 * +nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_X_P1 + (10 * +nHeadNo)]);

                    dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1 + (10 * +nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1 + (10 * +nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_P1 + (10 * +nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_LAST_Y_P1 + (10 * +nHeadNo)]);

                    break;

                case 1:
                    dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P1 + (10 * +nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P1 + (10 * +nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_P1 + (10 * +nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_X_P1 + (10 * +nHeadNo)]);

                    dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P1 + (10 *  + nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_START_P1 + (10 *  + nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_P1 + (10 *  + nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.CFG_MAP_TABLE_2_OFFSET_LAST_Y_P1 + (10 *  + nHeadNo)]);

                    break;


                default:
                    break;
            }

            dpStageAngleOffset = MathUtil.GetAnglePos(dStartPosX, dStartPosY, dVerAngle - 90, dTargetPosX, dTargetPosY);
            return dpStageAngleOffset;
        }

        public dxy GetTrayTablePlaceCountAngleOffset(int nTableNo, int nHeadNo, double dStartPosX, double dStartPosY, double dTargetPosX, double dTargetPosY)
        {
            double dHorAngle = 0.0f, dVerAngle = 0.0f;
            dxy dpStageAngleOffset = new dxy(0.0f, 0.0f);

            switch (nTableNo)
            {
                case 0:
                    dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1 + (10 * nHeadNo)]);

                    dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1 + (10 * nHeadNo)]);

                    break;

                case 1:
                    dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_X_P1 + (10 * nHeadNo)]);

                    dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_2_OFFSET_LAST_Y_P1 + (10 * nHeadNo)]);

                    break;

                case 2:
                    dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_X_P1 + (10 * nHeadNo)]);

                    dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_START_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P1 + (10 * nHeadNo)],
                                                  ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.CFG_ULD_TRAY_TABLE_3_OFFSET_LAST_Y_P1 + (10 * nHeadNo)]);


                    break;


                default:
                    break;
            }

            dpStageAngleOffset = MathUtil.GetAnglePos(dStartPosX, dStartPosY, dVerAngle - 90, dTargetPosX, dTargetPosY);
            return dpStageAngleOffset;

        }
    }
}
