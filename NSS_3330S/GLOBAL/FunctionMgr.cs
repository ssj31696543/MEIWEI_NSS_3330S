using DionesTool.Objects;
using DionesTool.UTIL;
using NSS_3330S;
using NSS_3330S;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.GLOBAL
{
    class FunctionMgr
    {
        private static volatile FunctionMgr instance;
        private static object syncRoot = new Object();

        public Function Func;
        public Function TempFunc;


        public static FunctionMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new FunctionMgr();
                    }
                }

                return instance;
            }
        }
        public FunctionMgr()
        {
            //
        }

        ~FunctionMgr()
        {
        }

        public void Init()
        {
            Func = Function.Deserialize(PathMgr.Inst.FILE_PATH_FUNC);
            if (Func == null)
            {
                Func = new Function();
            }

            CheckValidation(Func);
            Func.Serialize(PathMgr.Inst.FILE_PATH_FUNC);

            //TempCfg
            TempFunc = Function.Deserialize(PathMgr.Inst.FILE_PATH_FUNC);
            if (TempFunc == null)
            {
                TempFunc = new Function();
            }
        }

        void CheckValidation(Function func)
        {
        }

        public void SaveBackup()
        {
            //BACKUP 폴더 생성
            string strPath = string.Format("{0}{1}", PathMgr.Inst[PathMgr.ePathInfo.eSetting], "BACKUP");

            //폴더 경로 생성
            if (!Directory.Exists(strPath))
                Directory.CreateDirectory(strPath);

            //백업 파일 생성
            Func.Serialize(PathMgr.Inst.FILE_PATH_BACKUP_FUNC);
        }

        public void Save()
        {
            Func.Serialize(PathMgr.Inst.FILE_PATH_FUNC);
        }

        public void CopyTempFunc()
        {
            TempFunc = CreateDeepCopy(Func);
        }

        public void SaveTempFunc()
        {
            Func = CreateDeepCopy(TempFunc);
        }

        private Function CreateDeepCopy(Function inputcls)
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, inputcls);
            m.Position = 0;
            return (Function)b.Deserialize(m);
        }

    }
}
