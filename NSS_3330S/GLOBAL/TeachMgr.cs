using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NSS_3330S
{
    public class TeachMgr
    {
        private string pathExeFile = string.Empty;

        private static volatile TeachMgr instance;
        private static object syncRoot = new Object();

        public Teach Tch;
        public Teach TempTch;

        StringBuilder m_sbLog = new StringBuilder();
        public string GetPath(string pathName)
        {
            string path = string.Format("{0}\\{1}\\", pathExeFile, pathName);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            return path;
        }

        public static TeachMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TeachMgr();
                    }
                }

                return instance;
            }
        }

        public TeachMgr()
        {
        }

        ~TeachMgr()
        {
        }

        public void Init()
        {
            string strPath = PathMgr.Inst.FILE_PATH_TCH;
            Tch = Teach.Deserialize(strPath);
            if (Tch == null)
            {
                Tch = new Teach();
            }

            CheckValidation(Tch);

            Tch.Serialize(strPath);
            //temp recipe
            TempTch = Teach.Deserialize(strPath);
            if (TempTch == null)
            {
                TempTch = new Teach();

            }
            TempTch.Serialize(strPath);
        }


        public void Save()
        {
            string strPath = PathMgr.Inst.FILE_PATH_TCH;
            Tch.Serialize(strPath);
        }

        public void SaveBackup()
        {
            //BACKUP 폴더 생성
            string strPath = string.Format("{0}{1}", PathMgr.Inst[PathMgr.ePathInfo.eTeach], "BACKUP");

            //폴더 경로 생성
            if (!Directory.Exists(strPath))
                Directory.CreateDirectory(strPath);

            //백업 파일 생성
            Tch.Serialize(PathMgr.Inst.FILE_PATH_BACKUP_TCH);
        }

        public void CopyTempTch()
        {
            TempTch = CreateDeepCopy(Tch);
        }

        public Teach CopyTch(Teach tch)
        {
            return CreateDeepCopy(tch);
        }

        public void SaveTempTch()
        {
            CheckValidation(TempTch);

            m_sbLog.Clear();
            CompareValue(TempTch, Tch);

            if (m_sbLog.Length > 0)
            {
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("[Change Teach]\r\n{0}", m_sbLog.ToString()));
            }

            Tch = CreateDeepCopy(TempTch);
        }

        private Teach CreateDeepCopy(Teach inputcls)
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, inputcls);
            m.Position = 0;
            return (Teach)b.Deserialize(m);
        }

        void CheckValidation(Teach tch)
        {
            int[] nAxisArray = new int[8];
            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_Z_2;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_3;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_4;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_5;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_Z_6;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_Z_7;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_Z_8;
            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nCnt == 0) continue;

                for (int nPosNo = 0; nPosNo < POSDF.MAX_POS_COUNT; nPosNo++)
                {
                    tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo] = tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo] = tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo] = tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
            }

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_2_Z_1;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_2_Z_2;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_2_Z_3;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_2_Z_4;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_2_Z_5;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_2_Z_6;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_2_Z_7;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_2_Z_8;
            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nCnt == 0) continue;

                for (int nPosNo = 0; nPosNo < POSDF.MAX_POS_COUNT; nPosNo++)
                {
                    tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo] = tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo] = tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo] = tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
            }

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_T_1;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_T_2;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_T_3;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_4;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_T_5;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_T_6;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_T_7;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_T_8;
            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nCnt == 0) continue;

                for (int nPosNo = 0; nPosNo < POSDF.MAX_POS_COUNT; nPosNo++)
                {
                    tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo] = tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo] = tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo] = tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
            }

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_2_T_1;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_2_T_2;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_2_T_3;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_2_T_4;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_2_T_5;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_2_T_6;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_2_T_6;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_2_T_6;
            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nCnt == 0) continue;

                for (int nPosNo = 0; nPosNo < POSDF.MAX_POS_COUNT; nPosNo++)
                {
                    tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo] = tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo] = tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo] = tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
            }
        }

        #region WriteLogCompare
        void WriteLogCompare(int objNew, int objOld, string objName)
        {
            if (Equals(objNew, objOld) == false)
            {
                m_sbLog.AppendFormat("{0} : [{1}] -> [{2}]\r\n", objName, objOld, objNew);
            }
        }

        void WriteLogCompare(double objNew, double objOld, string objName)
        {
            if (Equals(objNew, objOld) == false)
            {
                m_sbLog.AppendFormat("{0} : [{1}] -> [{2}]\r\n", objName, objOld, objNew);
            }
        }

        void WriteLogCompare(string objNew, string objOld, string objName)
        {
            if (Equals(objNew, objOld) == false)
            {
                m_sbLog.AppendFormat("{0} : [{1}] -> [{2}]\r\n", objName, objOld, objNew);
            }
        }

        void WriteLogCompare(bool objNew, bool objOld, string objName)
        {
            if (Equals(objNew, objOld) == false)
            {
                m_sbLog.AppendFormat("{0} : [{1}] -> [{2}]\r\n", objName, objOld, objNew);
            }
        }

        #endregion

        public void CompareValue(Teach rcpNew, Teach rcpOld)
        {
            {
                for (int nAxis = 0; nAxis < SVDF.SV_CNT; nAxis++)
                {
                    for (int nPos = 0; nPos < SVDF.POS_CNT; nPos++)
                    {
                        WriteLogCompare(rcpNew.dMotPos[nAxis].dPos[nPos], rcpOld.dMotPos[nAxis].dPos[nPos],
                            string.Format("AXIS {0} {1} POSITION", SVDF.GetAxisName(nAxis), POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxis), nPos)));
                    }
                }
            }
        }

        public bool DataCheckBeforeSave()
        {
            // 잘못된 값이 있는지 확인
            bool bSuccess = true;

            return bSuccess;
        }
    }
}
