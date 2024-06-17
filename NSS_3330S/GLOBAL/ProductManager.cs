using NSS_3330S;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NSS_3330S
{
    //PRODUCT LOG
    public enum eMaxCount
    {
        MaxArray = 24,
    }

    public enum eDataSet
    {
        ProdCount,
    }

    public enum eDS_Col : int
    {
        TIME,
        IN,
        MCR_OK,
        MCR_MISMATCH,
        MCR_NULL,
        PRE_ALIGN_OK,
        PRE_ALIGN_NG,
        FINE_ALIGN_OK,
        FINE_ALIGN_NG,
        LINESCAN_NG,
        INSPECTION_NG,
        OUT,
        MaxCol
    }

    public struct stData
    {
        public int[] nData; //테스트용 구조체 변수
    }

        
    [Serializable]
    public class ProductManager
    {
        private static ProductManager instance = null;
        private static readonly object padlock = new object();


        //ADD XML
        const int nMaxArray = 24;
        public int[] nCntIN = new int[nMaxArray];
        public int[] nCntMcrOk = new int[nMaxArray];
        public int[] nCntMcrMis = new int[nMaxArray];
        public int[] nCntMcrNull = new int[nMaxArray];
        public int[] nCntMcrNg = new int[nMaxArray];
        public int[] nCntPreAlignOk = new int[nMaxArray];
        public int[] nCntPreAlignNg = new int[nMaxArray];
        public int[] nCntFineAlignOk = new int[nMaxArray];
        public int[] nCntFineAlignNg = new int[nMaxArray];
        public int[] nCntLineScanNg = new int[nMaxArray];
        public int[] nCntInspNg = new int[nMaxArray];
        public int[] nCntOut = new int[nMaxArray];

        //데이터 테이블 생성
        public DataSet dsCount = new DataSet("DataSet");
        public DataSet dsSum = new DataSet("DataSet");


        private string strDirName = "Setting";
        private string strFilenamePgmInfo = string.Empty;
        private string strPathExeFile = string.Empty;
        private string strPathName = string.Empty;

        private List<string>[] lstCelltData_arr = new List<string>[2]; //20181023 2장 기록 할때 사용 하였음 확인 후 삭제해 주세요
        private List<string> lstCelltData = new List<string>();

        ProductManager()
        {
            strFilenamePgmInfo = PathMgr.Inst.PATH_PROGRAM_INFO_NAME;

            strPathExeFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            strPathName = GetPath(strDirName);
        }

        public static ProductManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new ProductManager();

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


            strPathExeFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            strPathName = GetPath(strDirName);

            // D//LOG//PRODUCT 
            if (!System.IO.Directory.Exists(string.Format(PathMgr.Inst.PATH_LOG_PRODUCT)))
            {
                DirectoryInfo di = new DirectoryInfo(string.Format(PathMgr.Inst.PATH_LOG_PRODUCT));
                di.Create();
            }

            instance = LoadCountData();
        }

        private string GetPath(string pathName)
        {
            string path = string.Format("{0}\\{1}\\", strPathExeFile, pathName);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            return path;
        }

        #region Old Data
        public string TOTAL_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "TOTAL_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "TOTAL_COUNT", "0");
            }
        }

        public string PRE_GOOD_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "PRE_GOOD_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "PRE_GOOD_COUNT", "0");
            }
        }

        public string PRE_ALIGN_NG_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "PRE_ALIGN_NG_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "PRE_ALIGN_NG_COUNT", "0");
            }
        }

        public string PRE_VCR_FAIL_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "PRE_VCR_FAIL_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "PRE_VCR_FAIL_COUNT", "0");
            }
        }

        public string PEEL_GOOD_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "PEEL_GOOD_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "PEEL_GOOD_COUNT", "0");
            }
        }

        public string PEEL_NG_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "PEEL_NG_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "PEEL_NG_COUNT", "0");
            }
        }

        public string FINE_GOOD_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "FINE_GOOD_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "FINE_GOOD_COUNT", "0");
            }
        }

        public string FINE_ALIGN_NG_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "FINE_ALIGN_NG_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "FINE_ALIGN_NG_COUNT", "0");
            }
        }

        public string FINE_PEEL_NG_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "FINE_PEEL_NG_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "FINE_PEEL_NG_COUNT", "0");
            }
        }

        public string CUT_EXCUTE_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "CUT_EXCUTE_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "CUT_EXCUTE_COUNT", "0");
            }
        }

        public string CUT_PASS_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "CUT_PASS_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "CUT_PASS_COUNT", "0");
            }
        }

        public string INSP_GOOD_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "INSP_GOOD_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "INSP_GOOD_COUNT", "0");
            }
        }

        public string INSP_SIZE_NG_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "INSP_SIZE_NG_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "INSP_SIZE_NG_COUNT", "0");
            }
        }

        public string INSP_CHAMFER_NG_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                ini.SetIniValue("PRODUCT_WORKING_COUNT", "INSP_CHAMFER_NG_COUNT", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(strFilenamePgmInfo);
                return ini.GetIniValue("PRODUCT_WORKING_COUNT", "INSP_CHAMFER_NG_COUNT", "0");
            }
        }
        #endregion

        //PRODUCT LOG

        /////////////////////////////////////
        //시간 확인 함수 
        /////////////////////////////////////
        public string CheckNextDay(DateTime dtTime)
        {
            //현재 시간을 계산해서 오후 8:00가 넘었을 경우 다음 날짜로 파일을 남긴다.
            //DateTime dtTime = DateTime.Now;
//             if (dtTime.Hour >= 20)
//             {
//                 dtTime = dtTime.AddDays(1); //현재 날짜에서 하루 더함
//             }
            return dtTime.ToString("yyyyMMdd");
        }

        public int GetTimeIndex()
        {
            DateTime dtTime = DateTime.Now;
            int nCurTime = Convert.ToInt32(dtTime.Hour);
            return nCurTime;
        }

        /////////////////////////////////////
        //Xml File Save/Load 함수 
        /////////////////////////////////////

        public string GetFilePath(string strDirPath, string strFileName)
        {
            string strPath = string.Format("{0}\\{1}.xml", strDirPath, strFileName);
            return strPath;
        }

        public void SaveDataSet(DateTime dt)
        {
            //테이블 헤더 설정
            string strTableName = eDataSet.ProdCount.ToString();

            dsCount.Clear(); //DataSet 초기화

            if (dsCount.Tables[eDataSet.ProdCount.ToString()] == null)
            {
                //기존 테이블 삭제
                //dsCount.Tables.Remove(strTableName);

                //새 테이블 생성
                dsCount.Tables.Add(strTableName);

                //Col 항목 추가 
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.TIME.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.IN.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.MCR_OK.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.MCR_MISMATCH.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.MCR_NULL.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.PRE_ALIGN_OK.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.PRE_ALIGN_NG.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.FINE_ALIGN_OK.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.FINE_ALIGN_NG.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.LINESCAN_NG.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.INSPECTION_NG.ToString());
                dsCount.Tables[strTableName].Columns.Add(eDS_Col.OUT.ToString());
            }

            //Data Row 생성
            DataRow drCount = dsCount.Tables[strTableName].NewRow();

            for (int i = 0; i < nMaxArray; i++)
            {
                drCount = dsCount.Tables[strTableName].NewRow();
                drCount[eDS_Col.TIME.ToString()] = i.ToString();
                drCount[eDS_Col.IN.ToString()] = nCntIN[i].ToString();
                drCount[eDS_Col.MCR_OK.ToString()] = nCntMcrOk[i].ToString();
                drCount[eDS_Col.MCR_MISMATCH.ToString()] = nCntMcrMis[i].ToString();
                drCount[eDS_Col.MCR_NULL.ToString()] = nCntMcrNull[i].ToString();
                drCount[eDS_Col.PRE_ALIGN_OK.ToString()] = nCntPreAlignOk[i].ToString();
                drCount[eDS_Col.PRE_ALIGN_NG.ToString()] = nCntPreAlignNg[i].ToString();
                drCount[eDS_Col.FINE_ALIGN_OK.ToString()] = nCntFineAlignOk[i].ToString();
                drCount[eDS_Col.FINE_ALIGN_NG.ToString()] = nCntFineAlignNg[i].ToString();
                drCount[eDS_Col.LINESCAN_NG.ToString()] = nCntLineScanNg[i].ToString();
                drCount[eDS_Col.INSPECTION_NG.ToString()] = nCntInspNg[i].ToString();
                drCount[eDS_Col.OUT.ToString()] = nCntOut[i].ToString();

                //Table Row 추가
                dsCount.Tables[strTableName].Rows.InsertAt(drCount, i);
            }

            //파일 날짜 받아옴
            string strFileName = CheckNextDay(dt);

            //xml 파일 생성
            string strPath = GetFilePath(PathMgr.Inst.PATH_LOG_PRODUCT, strFileName);
            dsCount.WriteXml(strPath); 
        }

        //////////////////////////////////////
        //현재 카운트 변수값 저장
        //////////////////////////////////////
               
        public string PathFile()
        {
            string strPath = string.Format("{0}Count.xml", strPathName);
            return strPath;
        }
        public string PathFile_CountDay(string LogProductPath, string CountDay)
        {
            string strPath = string.Format("{0}\\Count_{1}.xml", LogProductPath, CountDay);
            return strPath;
        }

        public bool SaveCountData(DateTime dtTime)
        {
            //string strPath = PathFile(); //sjs : 전날짜 데이터와 합산 돼서 날짜별 카운트 파일 생성

            //파일 날짜 받아옴
            string strdt = CheckNextDay(dtTime);
            string strFileName = PathFile_CountDay(PathMgr.Inst.PATH_LOG_PRODUCT, strdt);

            return Serialize(strFileName);
        }
       
        public ProductManager LoadCountData()
        {
            string strPath = PathFile();
            //instance = Deserialize(strPath);


            //파일 날짜 받아옴
            DateTime dtTime = DateTime.Now;
            string strdt =  CheckNextDay(dtTime);
            string strFileName = PathFile_CountDay(PathMgr.Inst.PATH_LOG_PRODUCT, strdt);

            if (!System.IO.File.Exists(strFileName))
            {
                bool filecreate = Serialize(strFileName);
                if (! filecreate) System.IO.File.Copy(strPath, strFileName);
            }

            return Deserialize(strFileName);
        }

        /// <summary>
        /// 파일에 xml형태로 정보를 저장합니다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>성공 여부</returns>
        public bool Serialize(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    ProductManager.Instance.Init();
                }
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductManager));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, this);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// xml형태의 파일에서 정보를 불러옵니다.
        /// </summary>
        /// <param name="path">파일 경로</param>
        /// <returns>반환 객체</returns>
        public static ProductManager Deserialize(string path)
        {
            ProductManager inst = null;

            if (!File.Exists(path))
            {
                inst = instance;
                return inst;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductManager));
                using (StreamReader rd = new StreamReader(path))
                {
                    inst = (ProductManager)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
            }

            return inst;
        }

    }
}
