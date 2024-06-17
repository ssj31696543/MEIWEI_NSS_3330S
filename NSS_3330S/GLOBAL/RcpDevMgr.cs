using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.GLOBAL
{
    class RcpDevMgr
    {
        // 파일 이름
        private string dirName = "DeviceMgr";
        //private string filenamePgmInfo = PathMgr.Inst.PATH_PROGRAM_INFO_NAME;
        private string pathExeFile = string.Empty;
        
        private string pathDevMgzName = string.Empty;
        private string pathDevMapName = string.Empty;
        private string pathDevTrayName = string.Empty;

        private static volatile RcpDevMgr instance;
        private static object syncRoot = new Object();

        public MagazineInfo MgzInfo;
        public MapTableInfo MapTbInfo;
        public UldTrayInfo TrayInfo;

        StringBuilder m_sbLog = new StringBuilder();

        public string GetDevMgrPath(string pathName)
        {
            string path = string.Format("{0}\\{1}\\{2}", pathExeFile, dirName, pathName);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            return path;
        }

        public static RcpDevMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new RcpDevMgr();
                    }
                }

                return instance;
            }
        }

        public RcpDevMgr()
        {
            pathExeFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            pathDevMgzName = GetDevMgrPath("MGZ_INFO");
            pathDevMapName = GetDevMgrPath("MAP_INFO");
            pathDevTrayName = GetDevMgrPath("TRAY_INFO");
        }

        public void Init()
        {
            MgzInfoInit();
            MapInfoInit();
            TrayInfoInit();
        }

        public void MgzInfoInit()
        {
            string strPath = PathMgr.Inst.PATH_DEV_MGR_MGZ + RecipeMgr.Inst.Rcp.MgzInfo.strMgzFileName;
            RecipeMgr.Inst.Rcp.MgzInfo = MagazineInfo.Deserialize(strPath);
            if (RecipeMgr.Inst.Rcp.MgzInfo == null)
            {
                RecipeMgr.Inst.Rcp.MgzInfo = new MagazineInfo();
            }

            RecipeMgr.Inst.Rcp.MgzInfo.Serialize(strPath);

            //temp recipe
            RecipeMgr.Inst.TempRcp.MgzInfo = MagazineInfo.Deserialize(strPath);
            if (RecipeMgr.Inst.TempRcp.MgzInfo == null)
            {
                RecipeMgr.Inst.TempRcp.MgzInfo = new MagazineInfo();

            }

            RecipeMgr.Inst.TempRcp.MgzInfo.Serialize(strPath);
        }

        public void MapInfoInit()
        {
            string strPath = PathMgr.Inst.PATH_DEV_MGR_MAP + RecipeMgr.Inst.Rcp.MapTbInfo.strMapTbFileName;
            RecipeMgr.Inst.Rcp.MapTbInfo = MapTableInfo.Deserialize(strPath);
            if (RecipeMgr.Inst.Rcp.MapTbInfo == null)
            {
                RecipeMgr.Inst.Rcp.MapTbInfo = new MapTableInfo();
            }

            RecipeMgr.Inst.Rcp.MapTbInfo.Serialize(strPath);

            //temp recipe
            RecipeMgr.Inst.TempRcp.MapTbInfo = MapTableInfo.Deserialize(strPath);
            if (RecipeMgr.Inst.TempRcp.MapTbInfo == null)
            {
                RecipeMgr.Inst.TempRcp.MapTbInfo = new MapTableInfo();

            }

            RecipeMgr.Inst.TempRcp.MapTbInfo.Serialize(strPath);
        }

        public void TrayInfoInit()
        {
            string strPath = PathMgr.Inst.PATH_DEV_MGR_TRAY + RecipeMgr.Inst.Rcp.TrayInfo.strTrayFileName;
            RecipeMgr.Inst.Rcp.TrayInfo = UldTrayInfo.Deserialize(strPath);
            if (RecipeMgr.Inst.Rcp.TrayInfo == null)
            {
                RecipeMgr.Inst.Rcp.TrayInfo = new UldTrayInfo();
            }

            RecipeMgr.Inst.Rcp.TrayInfo.Serialize(strPath);

            //temp recipe
            RecipeMgr.Inst.TempRcp.TrayInfo = UldTrayInfo.Deserialize(strPath);
            if (RecipeMgr.Inst.TempRcp.TrayInfo == null)
            {
                RecipeMgr.Inst.TempRcp.TrayInfo = new UldTrayInfo();
            }

            if (RecipeMgr.Inst.Rcp.TrayInfo == null)
            {
                RecipeMgr.Inst.Rcp.TrayInfo = new UldTrayInfo();
            }
            if (RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue == null)
            {
                RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue = new double[2];
            }
            if (RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue.Length == 0)
            {
                RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue = new double[2];
            }
            if (RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue == null)
            {
                RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue = new double[2];
            }
            if (RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue.Length == 0)
            {
                RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue = new double[2];
            }



            if (RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipMissMaxValue == null)
            {
                RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipMissMaxValue = new double[2];
            }
            if (RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipMissMaxValue.Length == 0)
            {
                RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipMissMaxValue = new double[2];
            }
            if (RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipFloatingMinValue == null)
            {
                RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipFloatingMinValue = new double[2];
            }
            if (RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipFloatingMinValue.Length == 0)
            {
                RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipFloatingMinValue = new double[2];
            }

            RecipeMgr.Inst.TempRcp.TrayInfo.Serialize(strPath);
        }

        public void SaveDevMgz(string strFileName)
        {
            if (!strFileName.Contains(".xml"))
                strFileName = strFileName + ".xml";

            string strPath = PathMgr.Inst.PATH_DEV_MGR_MGZ + strFileName;

            RecipeMgr.Inst.Rcp.MgzInfo.strMgzFileName = strFileName;
            RecipeMgr.Inst.Rcp.MgzInfo.Serialize(strPath);
        }

        public void SaveDevMap(string strFileName)
        {
            if (!strFileName.Contains(".xml"))
                strFileName = strFileName + ".xml";

            string strPath = PathMgr.Inst.PATH_DEV_MGR_MAP + strFileName;

            RecipeMgr.Inst.Rcp.MapTbInfo.strMapTbFileName = strFileName;
            RecipeMgr.Inst.Rcp.MapTbInfo.Serialize(strPath);
        }

        public void SaveDevTray(string strFileName)
        {
            if (!strFileName.Contains(".xml"))
                strFileName = strFileName + ".xml";

            string strPath = PathMgr.Inst.PATH_DEV_MGR_TRAY + strFileName;

            RecipeMgr.Inst.Rcp.TrayInfo.strTrayFileName = strFileName;
            RecipeMgr.Inst.Rcp.TrayInfo.Serialize(strPath);
        }

        public bool FindFileName(int nDevItem,  string strName)
        {
            System.IO.DirectoryInfo di = null;

            switch (nDevItem)
            {
                case 0:
                    di = new System.IO.DirectoryInfo(pathDevMgzName);
                    break;
                case 1:
                    di = new System.IO.DirectoryInfo(pathDevMapName);
                    break;
                case 2:
                    di = new System.IO.DirectoryInfo(pathDevTrayName);
                    break;

                default:
                    break;
            }
            

            foreach (System.IO.FileInfo f in di.GetFiles())
            {
                if (strName == f.Name)
                    return false;
            }

            return true;
        }

        public bool CreateDevFile(int nDevItem, string strFileName)
        {
            try
            {
                switch (nDevItem)
                {
                    case 0:
                        SaveDevMgz(strFileName);
                        break;

                    case 1:
                        SaveDevMap(strFileName);
                        break;

                    case 2:
                        SaveDevTray(strFileName);
                        break;

                    default:
                        break;
                }
                

            }
            catch (Exception ex)
            {
                //
                return false;
            }

            return true;
        }

        public bool ChangeDevFile(int nDevItem, string strChangeFileName)
        {
            if (!strChangeFileName.Contains(".xml"))
                strChangeFileName = strChangeFileName + ".xml";

            if (FindFileName(nDevItem, strChangeFileName))
                return false;

            switch (nDevItem)
            {
                case 0:
                    RecipeMgr.Inst.TempRcp.MgzInfo.strMgzFileName = strChangeFileName;

                    RecipeMgr.Inst.SaveTempRcp();
                    RecipeMgr.Inst.Save();

                    //RecipeMgr.Inst.Init();

                    MgzInfoInit();
                    //CopyTempRcp();
                    break;

                case 1:
                    RecipeMgr.Inst.TempRcp.MapTbInfo.strMapTbFileName = strChangeFileName;

                    RecipeMgr.Inst.SaveTempRcp();
                    RecipeMgr.Inst.Save();

                    //RecipeMgr.Inst.Init();

                    MapInfoInit();
                    //CopyTempRcp();
                    break;

                case 2:
                    RecipeMgr.Inst.TempRcp.TrayInfo.strTrayFileName = strChangeFileName;

                    RecipeMgr.Inst.SaveTempRcp();
                    RecipeMgr.Inst.Save();

                    //RecipeMgr.Inst.Init();

                    TrayInfoInit();
                    //CopyTempRcp();
                    break;
                default:
                    break;
            }
            

            return true;
        }

        public void DeleteMgzInfoFile(string DelFileName)
        {
            if (!DelFileName.Contains(".xml"))
                DelFileName = DelFileName + ".xml";

            if (RecipeMgr.Inst.TempRcp.MgzInfo.strMgzFileName != DelFileName)
            {
                string delFile = string.Format("{0}\\{1}", pathDevMgzName, DelFileName);
                System.IO.File.Delete(delFile);
            }
        }

        public void DeleteMapInfoFile(string DelFileName)
        {
            if (!DelFileName.Contains(".xml"))
                DelFileName = DelFileName + ".xml";

            if (RecipeMgr.Inst.TempRcp.MapTbInfo.strMapTbFileName != DelFileName)
            {
                string delFile = string.Format("{0}\\{1}", pathDevMapName, DelFileName);
                System.IO.File.Delete(delFile);
            }
        }

        public void DeleteTrayInfoFile(string DelFileName)
        {
            if (!DelFileName.Contains(".xml"))
                DelFileName = DelFileName + ".xml";

            if (RecipeMgr.Inst.TempRcp.TrayInfo.strTrayFileName != DelFileName)
            {
                string delFile = string.Format("{0}\\{1}", pathDevTrayName, DelFileName);
                System.IO.File.Delete(delFile);
            }
        }
    }
}
