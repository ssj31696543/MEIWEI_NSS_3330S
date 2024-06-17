using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NSS_3330S;

namespace NSS_3330S.GLOBAL
{
    class InspectionDataManager
    {
        private static InspectionDataManager instance = null;
        private static readonly object padlock = new object();

        InspectionDataManager()
        {
        }

        public static InspectionDataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new InspectionDataManager();

                            instance.Init();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        void Init()
        {
            // 폴더가 없으면 생성하기
        }


        public bool LoadInspectionDataFile(string strFileName)
        {
            bool bRtn = true;
            
            return bRtn;
        }

        public void SaveInspectionData(List<string> lstInspectionData)
        {
            // 추후 검사 후 DB에 있는 내용을 저장하는 방식으로 변경 필요
            //if (!Directory.Exists(PathMgr.Inst.PATH_INSP_DATA))
            //{
            //    Directory.CreateDirectory(PathMgr.Inst.PATH_INSP_DATA);
            //}

            //string strPath = string.Format("{0}{1}\\{2}.csv", PathMgr.Inst.PATH_INSP_DATA, DateTime.Now.ToString("yyyyMMdd"), "ASDFQWR-124");

            //FileStream fs = new FileStream(strPath, FileMode.Append, FileAccess.Write);
            //StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            //if (lstInspectionData != null)
            //{
            //    string line = string.Join(",", lstInspectionData.Cast<object>());
            //    sw.WriteLine(line);
            //}
            //sw.Close();
            //fs.Close();
        }
    }
}
