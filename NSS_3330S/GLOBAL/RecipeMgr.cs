using DionesTool.DXF;
using DionesTool.Objects;
using DionesTool.UTIL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S
{
    class RecipeMgr
    {
        // 파일 이름
        private string dirName = "WorkFile";
        private string filenamePgmInfo = PathMgr.Inst.PATH_PROGRAM_INFO_NAME;
        private string pathExeFile = string.Empty;
        private string pathName = string.Empty;

        private static volatile RecipeMgr instance;
        private static object syncRoot = new Object();

        public Recipe Rcp;
        public Recipe TempRcp;

        public List<Recipe> RcpList = new List<Recipe>();
        public List<string> RcpNameList = new List<string>();

        StringBuilder m_sbLog = new StringBuilder();

        bool _bChangedStripInfo = true;
        bool _bChangedTrayInfo = true;

        public int LAST_MODEL_NO
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(filenamePgmInfo);
                ini.SetIniValue("WORK_PRODUCT_INFO", "LAST_MODEL_NO", value.ToString());
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(filenamePgmInfo);
                string str = ini.GetIniValue("WORK_PRODUCT_INFO", "LAST_MODEL_NO", "1");
                int nNo = 1;
                int.TryParse(str, out nNo);

                return nNo;
            }
        }

        public string LAST_MODEL_NAME
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(filenamePgmInfo);
                ini.SetIniValue("WORK_PRODUCT_INFO", "LAST_MODEL_NAME", value);
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(filenamePgmInfo);
                return ini.GetIniValue("WORK_PRODUCT_INFO", "LAST_MODEL_NAME", "99.mdl");
            }
        }

        public string GetPath(string pathName)
        {
            string path = string.Format("{0}\\{1}\\", pathExeFile, pathName);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            return path;
        }

        public static RecipeMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new RecipeMgr();
                    }
                }

                return instance;
            }
        }

        public RecipeMgr()
        {
            pathExeFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            pathName = GetPath(dirName);
        }

        ~RecipeMgr()
        {
        }

        /// <summary>
        /// 레시피 번호로 해당 레시피 경로 반환
        /// </summary>
        /// <param name="nRcpNo">레시피 번호 (1부터 시작)</param>
        /// <returns></returns>
        public string GetRecipePath(string strRecipeId)
        {
            return string.Format("{0}{1}", PathMgr.Inst.PATH_RECIPE, strRecipeId);
        }

        public void Init(bool bChangeRecipe = false)
        {
            string strPath = PathMgr.Inst.PATH_RECIPE + LAST_MODEL_NAME;
            Rcp = Recipe.Deserialize(strPath);
            if (Rcp == null)
            {
                Rcp = new Recipe();
            }

            CheckValidation(Rcp);
            Rcp.Serialize(strPath);
            //temp recipe
            TempRcp = Recipe.Deserialize(strPath);
            if (TempRcp == null)
            {
                TempRcp = new Recipe();

            }
            TempRcp.Serialize(strPath);

            CopyToTeachPos();

            // bChangeRecipe 인자 값이 true (프로그램을 켜거나 레시피를 변경할 때)
            // 이 후 Recipe 저장 시 Count 값이 다르다면 true 시켜줌
            if (bChangeRecipe || _bChangedStripInfo)
            {
                InitStripInfo();
                _bChangedStripInfo = false;
            }
            if (bChangeRecipe || _bChangedTrayInfo)
            {
                InitTrayInfo();
                _bChangedTrayInfo = false;
            }

            MESDF.EqRcpParam.Clear();
            EqRecipeProperties data;
            for (int i = 0; i < (int)MESDF.eRCP_ITEMS.SORTER_MAX; i++)
            {
                data = new EqRecipeProperties();
                data.ItemName = ((MESDF.eRCP_ITEMS)i).ToString().Replace('_', ' ');
                data.Modified = 0;
                data.Confirm = 0;
                data.UseOption = 0;
                data.Before = "";
                data.After = "";

                MESDF.EqRcpParam.Add(data);
            }
        }
        public void CopyToTeachPos()
        {

            try
            {
                #region Loading Rail
                //Loading
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];

                //Unloadnig
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING];

                //Grip Safety
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_GRIP] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_GRIP];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY];

                //Grip
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_GRIP] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_GRIP];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_GRIP] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_GRIP];

                //Ungrip
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP];

                //Ungrip Safety
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY];

                //Centering
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];

                //Barcode
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.BARCODE_Y].dPos[POSDF.BOT_BARCDOE_SCAN] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.BARCODE_Y].dPos[POSDF.BOT_BARCDOE_SCAN];
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE];
                #endregion

                #region Pre align
                //Grab 1
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_1] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_1];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];

                //Grab
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_2] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_2];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_2] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_2];
                #endregion

                #region Strip Info
                //Orient
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK];
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK];
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ORIENT_CHECK] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ORIENT_CHECK];

                //2D Code
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE];
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE];
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_BARCODE] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_BARCODE];
                #endregion

                #region Inspection Pos
                if (RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count > 0)
                {
                    //T1 Align
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1] =
                         RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX -
                         (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK1] =
                        RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY -
                        (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                    try
                    {
                        TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK2] =
                         RecipeMgr.Inst.Rcp.listMapGrpInfoL[RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count - 1].dTopInspPosY -
                         (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                    }
                    catch (Exception)
                    {

                    }

                }

                if (RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count > 0)
                {
                    //T2 Align
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_ALIGN_T2] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX -
                    (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK1] =
                        RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY -
                        (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                    try
                    {
                        TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK2] =
                       RecipeMgr.Inst.Rcp.listMapGrpInfoR[RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count - 1].dTopInspPosY -
                       (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                    }
                    catch (Exception)
                    {
                    }

                }

                if (RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count > 0)
                {
                    //T1 Inspection
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX -
                    (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_MAP_VISION_START] =
                        RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY -
                        (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                }

                if (RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count > 0)
                {
                    //T2 Inspection
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T2] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX -
                    (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_MAP_VISION_START] =
                        RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY -
                        (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                }
                #endregion

                #region Pick Up

                if (RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count > 0)
                {
                    //T1
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1] =
                ConfigMgr.Inst.GetCamToPickerupPos(0, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).x;


                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1] =
                    ConfigMgr.Inst.GetCamToPickerupPos(1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).x;

                    // T1 VISION
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T1] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1;

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T1] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X2;

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] =
                    ConfigMgr.Inst.GetCamToPickerupPos(0, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).y;

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] =
                    ConfigMgr.Inst.GetCamToPickerupPos(1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).y;
                }

                if (RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count > 0)
                {
                    //T2
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2] =
                ConfigMgr.Inst.GetCamToPickerupPos(0, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).x;

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2] =
                    ConfigMgr.Inst.GetCamToPickerupPos(1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).x;

                    // T2 VISION
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T2] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1;

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T2] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X2;

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] =
                    ConfigMgr.Inst.GetCamToPickerupPos(0, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).y;

                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] =
                    ConfigMgr.Inst.GetCamToPickerupPos(1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).y;
                }

                //TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P1] =
                //RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P1];

                //TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P2] =
                //RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P2];
                #endregion

                #region Place
                //GD1
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];


                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];


                //GD2
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];


                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];


                //RW
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];


                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
                RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];
                #endregion
            }
            catch (Exception)
            {

     
            }            

        }

        void CheckValidation(Recipe rcp)
        {
#pragma warning disable 612,618
            for (int nSvCnt = 0; nSvCnt < rcp.dMotPos.Length; nSvCnt++)
            {
                for (int nPosCnt = 0; nPosCnt < rcp.dMotPos[nSvCnt].dVel.Length; nPosCnt++)
                {
                    if (rcp.dMotPos[nSvCnt].dVel[nPosCnt] <= 1) rcp.dMotPos[nSvCnt].dVel[nPosCnt] =
                        ConfigMgr.Inst.Cfg.MotData[nSvCnt].dVel;
                    if (rcp.dMotPos[nSvCnt].dAcc[nPosCnt] <= 0) rcp.dMotPos[nSvCnt].dAcc[nPosCnt] =
                        ConfigMgr.Inst.Cfg.MotData[nSvCnt].dAcc;
                    if (rcp.dMotPos[nSvCnt].dDec[nPosCnt] <= 0) rcp.dMotPos[nSvCnt].dDec[nPosCnt] =
                        ConfigMgr.Inst.Cfg.MotData[nSvCnt].dDec;
                }
            }
            if (rcp.TrayInfo.dInspChipMissMaxValue == null)
            {
                rcp.TrayInfo.dInspChipMissMaxValue = new double[2];
            }
            if (rcp.TrayInfo.dInspChipMissMaxValue.Length == 0)
            {
                rcp.TrayInfo.dInspChipMissMaxValue = new double[2];
            }
            if (rcp.TrayInfo.dInspChipFloatingMinValue == null)
            {
                rcp.TrayInfo.dInspChipFloatingMinValue = new double[2];
            }
            if (rcp.TrayInfo.dInspChipFloatingMinValue.Length == 0)
            {
                rcp.TrayInfo.dInspChipFloatingMinValue = new double[2];
            }
            #pragma warning restore 612, 618

            #region 현재 사용하지 않는 값 고정
            rcp.MapTbInfo.nInspChipViewCountX = 1;
            rcp.MapTbInfo.nInspChipViewCountY = 1; 
            #endregion
        }


        public void Save()
        {
            string strPath = PathMgr.Inst.PATH_RECIPE + LAST_MODEL_NAME;
            Rcp.Serialize(strPath);
        }

        public void SaveModelFinder()
        {
            try
            {

            }
            catch (System.Exception ex)
            {
                throw ex;
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
            }
        }

        public void LoadModelFinder()
        {
            try
            {

            }
            catch (System.Exception ex)
            {
                throw ex;
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
            }
        }

        public void CopyTempRcp()
        {
            TempRcp = CreateDeepCopy(Rcp);
        }

        public Recipe CopyRcp(Recipe rcp)
        {
            return CreateDeepCopy(rcp);
        }

        /// <summary>
        /// pjh 220728 수정
        /// </summary>
        public void SaveTempRcp()
        {
            m_sbLog.Clear();
            CompareValue(TempRcp, Rcp);

            // [2022.04.19.kmlee] 변경 시 로그 남기고 Init할 때 UnitArr 갱신
            if (Rcp.MapTbInfo.nUnitCountX != TempRcp.MapTbInfo.nUnitCountX ||
                Rcp.MapTbInfo.nUnitCountY != TempRcp.MapTbInfo.nUnitCountY ||
                Rcp.MapTbInfo.nMapGroupCntX != TempRcp.MapTbInfo.nMapGroupCntX ||
                Rcp.MapTbInfo.nMapGroupCntY != TempRcp.MapTbInfo.nMapGroupCntY)
            {
                m_sbLog.AppendFormat("_bChangedStripInfo true\r\n");
                _bChangedStripInfo = true;
            }

            if (Rcp.TrayInfo.nTrayCountX != TempRcp.TrayInfo.nTrayCountX ||
                Rcp.TrayInfo.nTrayCountY != TempRcp.TrayInfo.nTrayCountY)
            {
                m_sbLog.AppendFormat("_bChangedStripInfo true\r\n");
                _bChangedTrayInfo = true;
            }

            if (m_sbLog.Length > 0)
            {
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("[Change Recipe]\r\n{0}", m_sbLog.ToString()));
            }

            Rcp = CreateDeepCopy(TempRcp);
        }

        private Recipe CreateDeepCopy(Recipe inputcls)
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, inputcls);
            m.Position = 0;
            return (Recipe)b.Deserialize(m);
        }

        public void LoadModelFile(string strRecipeID, out Recipe md)
        {
            string fullfilename = string.Format("{0}{1}", pathName, strRecipeID);
            md = Recipe.Deserialize(fullfilename);
        }

        public void SaveAs(string strRecipeID, Recipe md)
        {
            string fullfilename = string.Format("{0}{1}", pathName, strRecipeID);
            md.Serialize(fullfilename);
        }

        public bool CopyBackupFile(string strFolder, string sRcpPPID, string sRcpName, string sPPID = "NULL")
        {
            // [20181115.kmlee] 저장할 때마다 날짜,시간 별로 저장한다..
            DateTime dtNow = DateTime.Now;
            string sourceFile = string.Format("{0}{1}", pathName, sRcpPPID);
            string destFile = string.Format("{0}{1}\\{2}\\{3}\\{4}_{5}", pathName, strFolder, dtNow.ToString("yyyyMM"), dtNow.ToString("dd"), dtNow.ToString("hhmmss"), sRcpName);
            try
            {
                if (System.IO.File.Exists(sourceFile))
                {
                    if (!Directory.Exists(destFile)) Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                    System.IO.File.Copy(sourceFile, destFile, true);
                }
            }
            catch (Exception ex)
            {
                //
                return false;
            }

            return true;
        }

        //레시피 파일 복사
        public bool CopyToModel(string sRcpNo, string sRecipeID, string sRcpDesc = "")
        {
            string sourceFile = string.Format("{0}{1}", pathName, LAST_MODEL_NAME);
            string destFile = string.Format("{0}{1}", pathName, sRecipeID);

            try
            {
                if (System.IO.File.Exists(sourceFile))
                {
                    System.IO.File.Copy(sourceFile, destFile, true);

                    Recipe mdUpload = new Recipe();
                    LoadModelFile(sRecipeID, out mdUpload);
                    mdUpload.strRecipeDescription = sRcpDesc;
                    mdUpload.strRecipeId = sRecipeID.Replace(".mdl", "");
                    mdUpload.strNo = sRcpNo;

                    SaveAs(sRecipeID, mdUpload);
                }
            }
            catch (Exception ex)
            {
                //
                return false;
            }

            return true;
        }

        public bool ModelDescriptionChange(string sRecipeID, string sRcpDescription)
        {
            string targetFile = RecipeMgr.Inst.GetRecipePath(sRecipeID);

            if (System.IO.File.Exists(targetFile))
            {
                Recipe mdUpload = new Recipe();
                LoadModelFile(sRecipeID, out mdUpload);
                mdUpload.strRecipeDescription = sRcpDescription;

                SaveAs(sRecipeID, mdUpload);
            }

            return true;
        }

        public void SetPPID(string strRecipeID)
        {
            Recipe mdUpload = new Recipe();
            mdUpload.strRecipeId = strRecipeID;
        }

        public void ChangeModelFile(string ChnageFileName)
        {
            if (!ChnageFileName.Contains(".mdl"))
                ChnageFileName = ChnageFileName + ".mdl";

            LAST_MODEL_NAME = ChnageFileName;
            //220528 pjh
            //레시피 변경 시 맵 배열 초기화를 위해 change변수 true
            Init(true);
        }

        public void DeleteModelFile(string DelFileName)
        {
            if (!DelFileName.Contains(".mdl"))
                DelFileName = DelFileName + ".mdl";

            if (LAST_MODEL_NAME != DelFileName)
            {
                string delFile = string.Format("{0}{1}", pathName, DelFileName);
                System.IO.File.Delete(delFile);
            }
        }
        public void FolderDelete(string FolderName)
        {
            //string Fname = @"D:/" + FolderName;
            System.IO.Directory.Delete(FolderName, true);
        }

        public double TranslateToDuty(double dSpeed_mm_s, double dOverlap_um, double dPulseWidth_us)
        {
            // Frequency(Hz) = 1 / 주기(s)
            // Duty(%) = Pulse Width(s) / 주기(s)
            // Overlap(mm) = Speed (mm/s) / Frequency(Hz)

            // Frequency (Hz) = Speed (mm/s) / Overlap (mm)
            // Frequency (kHz) = Speed (mm/s) / Overlap (um)

            // 1um = 1/0.2 count = 5 count
            double dFreq_kHz = dSpeed_mm_s / dOverlap_um;

            //double dEncoderUm = MotionMgr.inst[AXES.CHX].GetEncoderReal(0.001);
            //double dMotorDivide_per_um = 0.0;
            //double dOverlap_count = 0.0;
            //double dIntervalTime = 0.0;

            //dMotorDivide_per_um = (1.0 / dEncoderUm);
            //dOverlap_count = dOverlap * dEncoderUm;

            //dIntervalTime = ((dOverlap_count * dMotorDivide_per_um) / dSpeed) * 1000.0;
            //dPower = (dPulseWidth_us / dIntervalTime) * 100.0;

            double dPower = 0.0;
            double dPeriod_sec = 0.0;
            double dPeriod_us = 0.0;

            dPeriod_sec = 1.0 / dFreq_kHz;
            dPeriod_us = dPeriod_sec * 1000.0;

            dPower = (dPulseWidth_us / dPeriod_us) * 100.0;

            return dPower;
        }

        /// <summary>
        /// 레시피 파일 복사 기능
        /// </summary>
        /// <param name="strCurMD"></param>
        /// <returns></returns>
        public bool BackupRcpFile(string strCurMD)
        {
            string str = strCurMD.Replace(".mdl", ".bak");
            string strFolderName = "BACKUP";
            string strPath = string.Format("{0}{1}", pathName, strFolderName);
            //string SrcModelName = str; //백업하는 파일 이름을 같은 이름으로 사용함 
            string SrcModelPPID = strCurMD; //[20181115.kmlee] 이걸 왜 백업하는 이름과 같이 사용하는지?
            string strBackupName = str;

            if (str == "") return false;

            //폴더 경로 생성
            if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);

            if (!RecipeMgr.Inst.CopyBackupFile(strFolderName, SrcModelPPID, strBackupName))
            {
                return false;
            }

            return true;
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

        public void CompareValue(Recipe rcpNew, Recipe rcpOld)
        {
            // TODO : 
            #region dMotPos
            {
                for (int nAxis = 0; nAxis < SVDF.SV_CNT; nAxis++)
                {
                    for (int nPos = 0; nPos < 20; nPos++)
                    {
                        try
                        {
                            if (rcpNew.dMotPos[nAxis].dPos.Length < nPos) continue;
                            WriteLogCompare(rcpNew.dMotPos[nAxis].dPos[nPos], rcpOld.dMotPos[nAxis].dPos[nPos],
                                string.Format("AXIS {0} {1} POSITION", SVDF.GetAxisName(nAxis), POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxis), nPos)));

                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            #endregion

            #region MgzInfo
            WriteLogCompare(rcpNew.MgzInfo.strMgzFileName, rcpOld.MgzInfo.strMgzFileName, string.Format("MGZ INFO FILENAME"));
            WriteLogCompare(rcpNew.MgzInfo.nSlotCount, rcpOld.MgzInfo.nSlotCount, string.Format("MGZ INFO SLOT COUNT"));
            WriteLogCompare(rcpNew.MgzInfo.dSlotPitch, rcpOld.MgzInfo.dSlotPitch, string.Format("MGZ INFO SLOT PITCH"));
            #endregion

            #region MapTbInfo
            WriteLogCompare(rcpNew.MapTbInfo.strMapTbFileName, rcpOld.MapTbInfo.strMapTbFileName, string.Format("MAP TABLE INFO FILENAME"));
            WriteLogCompare(rcpNew.MapTbInfo.nUnitCountX, rcpOld.MapTbInfo.nUnitCountX, string.Format("MAP TABLE INFO UNIT COUNT X"));
            WriteLogCompare(rcpNew.MapTbInfo.nUnitCountY, rcpOld.MapTbInfo.nUnitCountY, string.Format("MAP TABLE INFO UNIT COUNT Y"));
            WriteLogCompare(rcpNew.MapTbInfo.nUnitInspCountX, rcpOld.MapTbInfo.nUnitInspCountX, string.Format("MAP TABLE INFO INSP COUNT X"));
            WriteLogCompare(rcpNew.MapTbInfo.nUnitInspCountY, rcpOld.MapTbInfo.nUnitInspCountY, string.Format("MAP TABLE INFO INSP COUNT Y"));
            WriteLogCompare(rcpNew.MapTbInfo.dUnitSizeX, rcpOld.MapTbInfo.dUnitSizeX, string.Format("MAP TABLE INFO UNIT SIZE X"));
            WriteLogCompare(rcpNew.MapTbInfo.dUnitSizeY, rcpOld.MapTbInfo.dUnitSizeY, string.Format("MAP TABLE INFO UNIT SIZE Y"));
            WriteLogCompare(rcpNew.MapTbInfo.dUnitPitchX, rcpOld.MapTbInfo.dUnitPitchX, string.Format("MAP TABLE INFO UNIT PITCH X"));
            WriteLogCompare(rcpNew.MapTbInfo.dUnitPitchY, rcpOld.MapTbInfo.dUnitPitchY, string.Format("MAP TABLE INFO UNIT PITCH Y"));
            WriteLogCompare(rcpNew.MapTbInfo.nInspChipViewCountX, rcpOld.MapTbInfo.nInspChipViewCountX, string.Format("MAP TABLE INFO INSP UNIT COUNT X"));
            WriteLogCompare(rcpNew.MapTbInfo.nInspChipViewCountY, rcpOld.MapTbInfo.nInspChipViewCountY, string.Format("MAP TABLE INFO INSP UNIT COUNT Y"));
            WriteLogCompare(rcpNew.MapTbInfo.dChipThickness, rcpOld.MapTbInfo.dChipThickness, string.Format("MAP TABLE INFO CHIP THICKNESS"));

            WriteLogCompare(rcpNew.MapTbInfo.nMapGroupCntX, rcpOld.MapTbInfo.nMapGroupCntX, string.Format("MAP TABLE INFO MAP GROUP COUNT X"));
            WriteLogCompare(rcpNew.MapTbInfo.nMapGroupCntY, rcpOld.MapTbInfo.nMapGroupCntY, string.Format("MAP TABLE INFO MAP GROUP COUNT Y"));
            WriteLogCompare(rcpNew.MapTbInfo.dMapGroupOffsetX, rcpOld.MapTbInfo.dMapGroupOffsetX, string.Format("MAP TABLE INFO MAP GROUP OFFSET X"));
            WriteLogCompare(rcpNew.MapTbInfo.dMapGroupOffsetY, rcpOld.MapTbInfo.dMapGroupOffsetY, string.Format("MAP TABLE INFO MAP GROUP OFFSET Y"));
            WriteLogCompare(rcpNew.MapTbInfo.bGrabOneEdge ? "USE" : "UNUSE", rcpOld.MapTbInfo.bGrabOneEdge ? "USE" : "UNUSE", string.Format("MAP TABLE INFO MAP GROUP OFFSET Y"));
            WriteLogCompare(rcpNew.listMapGrpInfoL.Count, rcpOld.listMapGrpInfoL.Count, string.Format("MAP TABLE INFO MAP #1 COUNT"));
            WriteLogCompare(rcpNew.listMapGrpInfoR.Count, rcpOld.listMapGrpInfoR.Count, string.Format("MAP TABLE INFO MAP #2 COUNT"));
            if (rcpNew.listMapGrpInfoL.Count == rcpOld.listMapGrpInfoL.Count)
            {
                for (int nIdx = 0; nIdx < rcpNew.listMapGrpInfoL.Count; nIdx++)
                {
                    WriteLogCompare(rcpNew.listMapGrpInfoL[nIdx].PICK_UP_POS_X1, rcpOld.listMapGrpInfoL[nIdx].PICK_UP_POS_X1, string.Format("LINE NO : {0},MAP #1 PICK UP POS X1",nIdx +1));
                    WriteLogCompare(rcpNew.listMapGrpInfoL[nIdx].PICK_UP_POS_X2, rcpOld.listMapGrpInfoL[nIdx].PICK_UP_POS_X2, string.Format("LINE NO : {0},MAP #1 PICK UP POS X2",nIdx +1));
                    WriteLogCompare(rcpNew.listMapGrpInfoL[nIdx].PICK_UP_POS_Y1, rcpOld.listMapGrpInfoL[nIdx].PICK_UP_POS_Y1, string.Format("LINE NO : {0},MAP #1 PICK UP POS Y1",nIdx +1));
                    WriteLogCompare(rcpNew.listMapGrpInfoL[nIdx].PICK_UP_POS_Y2, rcpOld.listMapGrpInfoL[nIdx].PICK_UP_POS_Y2, string.Format("LINE NO : {0},MAP #1 PICK UP POS Y2",nIdx +1));
                    WriteLogCompare(rcpNew.listMapGrpInfoL[nIdx].TOP_INSP_POS_X, rcpOld.listMapGrpInfoL[nIdx].TOP_INSP_POS_X, string.Format("LINE NO : {0},MAP #1 TOP INSP POS X",nIdx +1));
                    WriteLogCompare(rcpNew.listMapGrpInfoL[nIdx].TOP_INSP_POS_Y, rcpOld.listMapGrpInfoL[nIdx].TOP_INSP_POS_Y, string.Format("LINE NO : {0},MAP #1 TOP INSP POS Y",nIdx +1));
                }
            }
            if (rcpNew.listMapGrpInfoR.Count == rcpOld.listMapGrpInfoR.Count)
            {
                for (int nIdx = 0; nIdx < rcpNew.listMapGrpInfoR.Count; nIdx++)
                {
                    WriteLogCompare(rcpNew.listMapGrpInfoR[nIdx].PICK_UP_POS_X1, rcpOld.listMapGrpInfoR[nIdx].PICK_UP_POS_X1, string.Format("LINE NO : {0},MAP #2 PICK UP POS X1",nIdx + 1));
                    WriteLogCompare(rcpNew.listMapGrpInfoR[nIdx].PICK_UP_POS_X2, rcpOld.listMapGrpInfoR[nIdx].PICK_UP_POS_X2, string.Format("LINE NO : {0},MAP #2 PICK UP POS X2",nIdx + 1));
                    WriteLogCompare(rcpNew.listMapGrpInfoR[nIdx].PICK_UP_POS_Y1, rcpOld.listMapGrpInfoR[nIdx].PICK_UP_POS_Y1, string.Format("LINE NO : {0},MAP #2 PICK UP POS Y1",nIdx + 1));
                    WriteLogCompare(rcpNew.listMapGrpInfoR[nIdx].PICK_UP_POS_Y2, rcpOld.listMapGrpInfoR[nIdx].PICK_UP_POS_Y2, string.Format("LINE NO : {0},MAP #2 PICK UP POS Y2",nIdx + 1));
                    WriteLogCompare(rcpNew.listMapGrpInfoR[nIdx].TOP_INSP_POS_X, rcpOld.listMapGrpInfoR[nIdx].TOP_INSP_POS_X, string.Format("LINE NO : {0},MAP #2 TOP INSP POS X",nIdx + 1));
                    WriteLogCompare(rcpNew.listMapGrpInfoR[nIdx].TOP_INSP_POS_Y, rcpOld.listMapGrpInfoR[nIdx].TOP_INSP_POS_Y, string.Format("LINE NO : {0},MAP #2 TOP INSP POS Y",nIdx + 1));
                }
            }
            //bGrabOneEdge
            //
            #endregion

            #region TrayInfo
            
            WriteLogCompare(rcpNew.TrayInfo.strTrayFileName, rcpOld.TrayInfo.strTrayFileName, string.Format("TRAY INFO FILENAME"));
            WriteLogCompare(rcpNew.TrayInfo.nTrayCountX, rcpOld.TrayInfo.nTrayCountX, string.Format("TRAY INFO TRAY SLOT COUNT X"));
            WriteLogCompare(rcpNew.TrayInfo.nTrayCountY, rcpOld.TrayInfo.nTrayCountY, string.Format("TRAY INFO TRAY SLOT COUNT Y"));
            WriteLogCompare(rcpNew.TrayInfo.dTrayPitchX, rcpOld.TrayInfo.dTrayPitchX, string.Format("TRAY INFO TRAY SLOT PITCH X"));
            WriteLogCompare(rcpNew.TrayInfo.dTrayPitchY, rcpOld.TrayInfo.dTrayPitchY, string.Format("TRAY INFO TRAY SLOT PITCH Y"));
            WriteLogCompare(rcpNew.TrayInfo.dTrayThickness, rcpOld.TrayInfo.dTrayThickness, string.Format("TRAY INFO TRAY THICKNESS"));
            WriteLogCompare(rcpNew.TrayInfo.nInspChipCountX, rcpOld.TrayInfo.nInspChipCountX, string.Format("TRAY INFO INSP UNIT COUNT X"));
            WriteLogCompare(rcpNew.TrayInfo.nInspChipCountY, rcpOld.TrayInfo.nInspChipCountY, string.Format("TRAY INFO INSP UNIT COUNT Y"));
            WriteLogCompare(rcpNew.TrayInfo.nPlaceAngle, rcpOld.TrayInfo.nPlaceAngle, string.Format("TRAY INFO INSP PlaceAngle"));
            WriteLogCompare(rcpNew.TrayInfo.dGdTrayOffset[0][0].x, rcpOld.TrayInfo.dGdTrayOffset[0][0].x, string.Format("TRAY INFO INSP Head1, Good Tray1 Offset X"));
            WriteLogCompare(rcpNew.TrayInfo.dGdTrayOffset[0][0].y, rcpOld.TrayInfo.dGdTrayOffset[0][0].y, string.Format("TRAY INFO INSP Head1, Good Tray1 Offset Y"));
            WriteLogCompare(rcpNew.TrayInfo.dGdTrayOffset[0][1].x, rcpOld.TrayInfo.dGdTrayOffset[0][1].x, string.Format("TRAY INFO INSP Head1, Good Tray2 Offset X"));
            WriteLogCompare(rcpNew.TrayInfo.dGdTrayOffset[0][1].y, rcpOld.TrayInfo.dGdTrayOffset[0][1].y, string.Format("TRAY INFO INSP Head1, Good Tray2 Offset Y"));
            WriteLogCompare(rcpNew.TrayInfo.dRwTrayOffset[0].x, rcpOld.TrayInfo.dRwTrayOffset[0].x, string.Format("TRAY INFO INSP Head1, Rework Tray Offset X"));
            WriteLogCompare(rcpNew.TrayInfo.dRwTrayOffset[0].y, rcpOld.TrayInfo.dRwTrayOffset[0].y, string.Format("TRAY INFO INSP Head1, Rework Tray Offset Y"));

            WriteLogCompare(rcpNew.TrayInfo.dGdTrayOffset[1][0].x, rcpOld.TrayInfo.dGdTrayOffset[1][0].x, string.Format("TRAY INFO INSP Head2,Good Tray1 Offset X"));
            WriteLogCompare(rcpNew.TrayInfo.dGdTrayOffset[1][0].y, rcpOld.TrayInfo.dGdTrayOffset[1][0].y, string.Format("TRAY INFO INSP Head2,Good Tray1 Offset Y"));
            WriteLogCompare(rcpNew.TrayInfo.dGdTrayOffset[1][1].x, rcpOld.TrayInfo.dGdTrayOffset[1][1].x, string.Format("TRAY INFO INSP Head2,Good Tray2 Offset X"));
            WriteLogCompare(rcpNew.TrayInfo.dGdTrayOffset[1][1].y, rcpOld.TrayInfo.dGdTrayOffset[1][1].y, string.Format("TRAY INFO INSP Head2,Good Tray2 Offset Y"));
            WriteLogCompare(rcpNew.TrayInfo.dRwTrayOffset[1].x, rcpOld.TrayInfo.dRwTrayOffset[1].x, string.Format("TRAY INFO INSP Head2, Rework Tray Offset X"));
            WriteLogCompare(rcpNew.TrayInfo.dRwTrayOffset[1].y, rcpOld.TrayInfo.dRwTrayOffset[1].y, string.Format("TRAY INFO INSP Head2, Rework Tray Offset Y"));

            WriteLogCompare(rcpNew.TrayInfo.dInspChipOffsetY, rcpOld.TrayInfo.dInspChipOffsetY, string.Format("TRAY INFO INSP TrayInspChipOffset"));
            WriteLogCompare(rcpNew.TrayInfo.dInspChipMissMaxValue[0], rcpOld.TrayInfo.dInspChipMissMaxValue[0], string.Format("TRAY INFO INSP Tray1 dInspChipMissMaxValue"));
            WriteLogCompare(rcpNew.TrayInfo.dInspChipFloatingMinValue[0], rcpOld.TrayInfo.dInspChipFloatingMinValue[0], string.Format("TRAY INFO INSP Tray1 dInspChipFloatingMinValue"));
            WriteLogCompare(rcpNew.TrayInfo.dInspChipMissMaxValue[1], rcpOld.TrayInfo.dInspChipMissMaxValue[1], string.Format("TRAY INFO INSP Tray2 dInspChipMissMaxValue"));
            WriteLogCompare(rcpNew.TrayInfo.dInspChipFloatingMinValue[1], rcpOld.TrayInfo.dInspChipFloatingMinValue[1], string.Format("TRAY INFO INSP Tray2 dInspChipFloatingMinValue"));
            WriteLogCompare(rcpNew.TrayInfo.nKeyencePgmNo, rcpOld.TrayInfo.nKeyencePgmNo, string.Format("TRAY INFO INSP nKeyencePgmNo"));

            #endregion

            #region cleaning
            WriteLogCompare(rcpNew.cleaning.nUnitPkDryBlowCount, rcpOld.cleaning.nUnitPkDryBlowCount, string.Format("CLEANING INFO UNIT PICKER DRY AIR COUNT"));
            WriteLogCompare(rcpNew.cleaning.dUnitPkDryBlowVel, rcpOld.cleaning.dUnitPkDryBlowVel, string.Format("CLEANING INFO UNIT PICKER DRY AIR MOVE SPEED"));

            WriteLogCompare(rcpNew.cleaning.nRepickCount, rcpOld.cleaning.nRepickCount, string.Format("Repick Count"));
            WriteLogCompare(rcpNew.cleaning.nUnitPkSpongeCount, rcpOld.cleaning.nUnitPkSpongeCount, string.Format("Unit Picker Sponge Count"));
            WriteLogCompare(rcpNew.cleaning.dUnitPkSpongeVel, rcpOld.cleaning.dUnitPkSpongeVel, string.Format("CLEANING INFO UNIT PICKER SPONGE SPEED"));
            WriteLogCompare(rcpNew.cleaning.lUnitPkSpongeStartDelay, rcpOld.cleaning.lUnitPkSpongeStartDelay, string.Format("CLEANING INFO UNIT PICKER SPONGE START DELAY"));
            WriteLogCompare(rcpNew.cleaning.lUnitPkSpongeEndDelay, rcpOld.cleaning.lUnitPkSpongeEndDelay, string.Format("CLEANING INFO UNIT PICKER SPONGE END DELAY"));

            WriteLogCompare(rcpNew.cleaning.nTopDryCnt, rcpOld.cleaning.nTopDryCnt, string.Format("CLEANING INFO UNIT PICKER TOP DRY COUNT"));
            WriteLogCompare(rcpNew.cleaning.nTopDryVel, rcpOld.cleaning.nTopDryVel, string.Format("CLEANING INFO DRY STAGE TOP DRY SPEED"));
            WriteLogCompare(rcpNew.cleaning.lTopDryStartDelay, rcpOld.cleaning.lTopDryStartDelay, string.Format("CLEANING INFO DRY STAGE TOP DRY START DELAY"));
            WriteLogCompare(rcpNew.cleaning.lTopDryEndDelay, rcpOld.cleaning.lTopDryEndDelay, string.Format("CLEANING INFO DRY STAGE TOP DRY END DELAY"));

            WriteLogCompare(rcpNew.cleaning.nBtmDryCnt, rcpOld.cleaning.nBtmDryCnt, string.Format("CLEANING INFO UNIT PICKER BOTTOM DRY COUNT"));
            WriteLogCompare(rcpNew.cleaning.nBtmDryVel, rcpOld.cleaning.nBtmDryVel, string.Format("CLEANING INFO UNIT PICKER BOTTOM DRY SPEED"));
            WriteLogCompare(rcpNew.cleaning.lBtmDryStartDelay, rcpOld.cleaning.lBtmDryStartDelay, string.Format("CLEANING INFO UNIT PICKER BOTTOM DRY START DELAY"));
            WriteLogCompare(rcpNew.cleaning.lBtmDryEndDelay, rcpOld.cleaning.lBtmDryEndDelay, string.Format("CLEANING INFO UNIT PICKER BOTTOM DRY END DELAY"));

            WriteLogCompare(rcpNew.cleaning.lWaterSwingStartDelay, rcpOld.cleaning.lWaterSwingStartDelay, string.Format("CLEANING INFO UNIT PICKER SWING START DELAY"));
            WriteLogCompare(rcpNew.cleaning.lnWaterSwingEndDelay, rcpOld.cleaning.lnWaterSwingEndDelay, string.Format("CLEANING INFO UNIT PICKER SWING END DELAY"));

            WriteLogCompare(rcpNew.cleaning.nDryBlockAirCount, rcpOld.cleaning.nDryBlockAirCount, string.Format("CLEANING INFO DRY BLOCK AIR BLOW COUNT"));
            WriteLogCompare(rcpNew.cleaning.dDryBlockAirVel, rcpOld.cleaning.dDryBlockAirVel, string.Format("CLEANING INFO DRY BLOCK AIR BLOW MOVE SPEED"));
            WriteLogCompare(rcpNew.cleaning.dDryBlockUnloadingVel, rcpOld.cleaning.dDryBlockUnloadingVel, string.Format("CLEANING INFO DRY BLOCK UNLOAD SPEED"));
            WriteLogCompare(rcpNew.cleaning.nStageBlockDryBlowCount, rcpOld.cleaning.nStageBlockDryBlowCount, string.Format("CLEANING INFO STAGE KIT DRY AIR BLOW COUNT"));
            WriteLogCompare(rcpNew.cleaning.dStageBlockDryBlowVel, rcpOld.cleaning.dStageBlockDryBlowVel, string.Format("CLEANING INFO STAGE KIT DRY AIR BLOW MOVE SPEED"));
            WriteLogCompare(rcpNew.cleaning.lStageBlockDryBlowDelay, rcpOld.cleaning.lStageBlockDryBlowDelay, string.Format("CLEANING INFO STAGE KIT DRY AIR BLOW FINISH DELAY"));
            //20211119 CHOH: 맵스테이지 블로우 카운트 추가
            WriteLogCompare(rcpNew.cleaning.nMapTableDryBlowCount1, rcpOld.cleaning.nMapTableDryBlowCount1, string.Format("CLEANING INFO MAP STAGE 1 BLOW COUNT"));
            WriteLogCompare(rcpNew.cleaning.dMapTableDryBlowVel1, rcpOld.cleaning.dMapTableDryBlowVel1, string.Format("CLEANING INFO MAP STAGE 1 BLOW SPEED"));
            WriteLogCompare(rcpNew.cleaning.nMapTableDryBlowCount2, rcpOld.cleaning.nMapTableDryBlowCount2, string.Format("CLEANING INFO MAP STAGE 2 BLOW COUNT"));
            WriteLogCompare(rcpNew.cleaning.dMapTableDryBlowVel2, rcpOld.cleaning.dMapTableDryBlowVel2, string.Format("CLEANING INFO MAP STAGE 2 BLOW SPEED"));

            //스크랩 ㄹ추가
            WriteLogCompare(rcpNew.cleaning.nScrap1Delay, rcpOld.cleaning.nScrap1Delay, string.Format("Scrap #1 Delay"));
            WriteLogCompare(rcpNew.cleaning.nScrap2Delay, rcpOld.cleaning.nScrap2Delay, string.Format("Scrap #2 Delay"));
            WriteLogCompare(rcpNew.cleaning.nScrapCount1, rcpOld.cleaning.nScrapCount1, string.Format("Scrap #1 Count"));
            WriteLogCompare(rcpNew.cleaning.nScrapCount2, rcpOld.cleaning.nScrapCount2, string.Format("Scrap #2 Count"));
            WriteLogCompare(rcpNew.cleaning.nScrapMode == 0 ? "SCRAP AT THE SAME TIME": "SCRAP DIVIDED", rcpOld.cleaning.nScrapMode == 0 ? "SCRAP AT THE SAME TIME" : "SCRAP DIVIDED", string.Format("Scrap Mode"));
            #endregion

            #region 트레이 비전

            #endregion
        }

        public bool GetRcpList()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(PathMgr.Inst.PATH_RECIPE);

                RcpList.Clear();
                RcpNameList.Clear();
                int nCnt = 0;
                foreach (FileInfo f in dir.GetFiles("*.mdl"))
                {
                    //string strPath = RecipeMgr.Inst.GetRecipePath(f.Name.Replace(".mdl", ""));
                    string strPath = RecipeMgr.Inst.GetRecipePath(f.Name);
                    Recipe rcp = Recipe.Deserialize(strPath);

                    RcpList.Add(rcp);
                    RcpNameList.Add(rcp.strRecipeId);

                    nCnt++;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool isExistRecipe(string strRcpName)
        {
            bool bRet = true;
            try
            {
                if(!GetRcpList()) return false;
                
                int Index = 0;
                Index = RcpNameList.FindIndex(x => x == strRcpName);

                if (Index > 0) bRet = true;
                else bRet = false;           
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        /// <summary>
        /// 스트립 정보중 Unit 정보(2차원 배열)을 초기화 합니다.
        /// pjh 220728수정
        /// </summary>
        /// <returns></returns>
        public bool InitStripInfo()
        {
            try
            {
                // ROWs
                GbVar.Seq.sStripRail.Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);
                GbVar.Seq.sStripTransfer.Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);
                GbVar.Seq.sCuttingTable.Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);
                GbVar.Seq.sUnitTransfer.Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);

                //GbVar.Seq.sCleaning[0].Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY);
                //GbVar.Seq.sCleaning[1].Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY);
                //GbVar.Seq.sCleaning[2].Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY);

                GbVar.Seq.sUnitDry.Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);

                GbVar.Seq.sMapTransfer.Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);

                GbVar.Seq.sMapVisionTable[0].Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);
                GbVar.Seq.sMapVisionTable[1].Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);
                GbVar.Seq.sMapVisionTable[0].ReportInfo.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);
                GbVar.Seq.sMapVisionTable[1].ReportInfo.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);

                // COLs
                GbVar.Seq.sStripRail.Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                GbVar.Seq.sStripTransfer.Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                GbVar.Seq.sCuttingTable.Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                GbVar.Seq.sUnitTransfer.Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);

                //GbVar.Seq.sCleaning[0].Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);
                //GbVar.Seq.sCleaning[1].Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);
                //GbVar.Seq.sCleaning[2].Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);

                GbVar.Seq.sUnitDry.Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);

                GbVar.Seq.sMapTransfer.Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                GbVar.Seq.sMapVisionTable[0].Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                GbVar.Seq.sMapVisionTable[1].Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                GbVar.Seq.sMapVisionTable[0].ReportInfo.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                GbVar.Seq.sMapVisionTable[1].ReportInfo.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                //}
                return true;
            }
            catch (Exception)
            {
                return false;
            }
  
        }

        public bool InitTrayInfo()
        {
            try
            {
                GbVar.Seq.sUldGDTrayTable[0].Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);
                GbVar.Seq.sUldGDTrayTable[1].Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);

                GbVar.Seq.sUldRWTrayTable.Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);

                GbVar.Seq.sUldGDTrayTable[0].Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                GbVar.Seq.sUldGDTrayTable[1].Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);

                GbVar.Seq.sUldRWTrayTable.Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);


                GbVar.Seq.sUldTrayTransfer.Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);
                GbVar.Seq.sUldTrayTransfer.Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        /// <summary>
        /// 변경 전 후의 레시피 값을 배열버퍼에 업데이트 합니다.
        /// 변경된 레시피의 경우 변경 비트를 1로 저장합니다.
        /// DB에 업데이트 합니다.
        /// </summary>
        public bool UpdateRecipeParam()
        {
            bool bChange = false;
            try
            {
                #region BEFORE
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Count].Before = RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Pitch].Before = RecipeMgr.Inst.Rcp.MgzInfo.dSlotPitch.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_X].Before = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_Y].Before = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_X].Before = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_Y].Before = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_X].Before = RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_Y].Before = RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_X].Before = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_Y].Before = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_X].Before = RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_Y].Before = RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Thickness].Before = RecipeMgr.Inst.Rcp.TrayInfo.dTrayThickness.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_X].Before = RecipeMgr.Inst.Rcp.TrayInfo.nInspChipCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_Y].Before = RecipeMgr.Inst.Rcp.TrayInfo.nInspChipCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Picker_Place_T].Before = RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nUnitPkSpongeCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Velocity].Before = RecipeMgr.Inst.Rcp.cleaning.dUnitPkSpongeVel.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Water_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nFirstCleanWaterCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Air_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nUnitPkAirCleanCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nUnitPkDryBlowCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Velocity].Before = RecipeMgr.Inst.Rcp.cleaning.dUnitPkDryBlowVel.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Kit_Air_Blow_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nUnitPkKitAirBlowCount.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Water_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nCleanTableWaterCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Air_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nCleanTableAirCount.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nDryBlockAirCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Velocity].Before = RecipeMgr.Inst.Rcp.cleaning.dDryBlockAirVel.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dry_block_Unloading_Velocity].Before = RecipeMgr.Inst.Rcp.cleaning.dDryBlockUnloadingVel.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Count].Before = RecipeMgr.Inst.Rcp.cleaning.nStageBlockDryBlowCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Velocity].Before = RecipeMgr.Inst.Rcp.cleaning.dStageBlockDryBlowVel.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Delay].Before = RecipeMgr.Inst.Rcp.cleaning.lStageBlockDryBlowDelay.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count1].Before = RecipeMgr.Inst.Rcp.cleaning.nMapTableDryBlowCount1.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count2].Before = RecipeMgr.Inst.Rcp.cleaning.nMapTableDryBlowCount2.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity1].Before = RecipeMgr.Inst.Rcp.cleaning.dMapTableDryBlowVel1.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity2].Before = RecipeMgr.Inst.Rcp.cleaning.dMapTableDryBlowVel2.ToString();

                #endregion

                #region AFTER
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Count].After = RecipeMgr.Inst.TempRcp.MgzInfo.nSlotCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Pitch].After = RecipeMgr.Inst.TempRcp.MgzInfo.dSlotPitch.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_X].After = RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_Y].After = RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_X].After = RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_Y].After = RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_X].After = RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_Y].After = RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountY.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_X].After = RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_Y].After = RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_X].After = RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_Y].After = RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Thickness].After = RecipeMgr.Inst.TempRcp.TrayInfo.dTrayThickness.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_X].After = RecipeMgr.Inst.TempRcp.TrayInfo.nInspChipCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_Y].After = RecipeMgr.Inst.TempRcp.TrayInfo.nInspChipCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Picker_Place_T].After = RecipeMgr.Inst.TempRcp.TrayInfo.nPlaceAngle.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nUnitPkSpongeCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Velocity].After = RecipeMgr.Inst.TempRcp.cleaning.dUnitPkSpongeVel.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Water_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nFirstCleanWaterCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Air_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nUnitPkAirCleanCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nUnitPkDryBlowCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Velocity].After = RecipeMgr.Inst.TempRcp.cleaning.dUnitPkDryBlowVel.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Kit_Air_Blow_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nUnitPkKitAirBlowCount.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Water_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nCleanTableWaterCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Air_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nCleanTableAirCount.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nDryBlockAirCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Velocity].After = RecipeMgr.Inst.TempRcp.cleaning.dDryBlockAirVel.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dry_block_Unloading_Velocity].After = RecipeMgr.Inst.TempRcp.cleaning.dDryBlockUnloadingVel.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Count].After = RecipeMgr.Inst.TempRcp.cleaning.nStageBlockDryBlowCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Velocity].After = RecipeMgr.Inst.TempRcp.cleaning.dStageBlockDryBlowVel.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Delay].After = RecipeMgr.Inst.TempRcp.cleaning.lStageBlockDryBlowDelay.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count1].After = RecipeMgr.Inst.TempRcp.cleaning.nMapTableDryBlowCount1.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count2].After = RecipeMgr.Inst.TempRcp.cleaning.nMapTableDryBlowCount2.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity1].After = RecipeMgr.Inst.TempRcp.cleaning.dMapTableDryBlowVel1.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity2].After = RecipeMgr.Inst.TempRcp.cleaning.dMapTableDryBlowVel2.ToString();
                #endregion

                #region USE CONFIFM
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Count].UseOption = Convert.ToInt32(GbVar.bMgzConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Pitch].UseOption = Convert.ToInt32(GbVar.bMgzConfirm);

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_X].UseOption = Convert.ToInt32(GbVar.bVisionConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_Y].UseOption = Convert.ToInt32(GbVar.bVisionConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_X].UseOption = Convert.ToInt32(GbVar.bVisionConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_Y].UseOption = Convert.ToInt32(GbVar.bVisionConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_X].UseOption = Convert.ToInt32(GbVar.bVisionConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_Y].UseOption = Convert.ToInt32(GbVar.bVisionConfirm);

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_X].UseOption = Convert.ToInt32(GbVar.bTrayConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_Y].UseOption = Convert.ToInt32(GbVar.bTrayConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_X].UseOption = Convert.ToInt32(GbVar.bTrayConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_Y].UseOption = Convert.ToInt32(GbVar.bTrayConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Thickness].UseOption = Convert.ToInt32(GbVar.bTrayConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_X].UseOption = Convert.ToInt32(GbVar.bTrayConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_Y].UseOption = Convert.ToInt32(GbVar.bTrayConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Picker_Place_T].UseOption = Convert.ToInt32(GbVar.bTrayConfirm);

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Velocity].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Water_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Air_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Velocity].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Kit_Air_Blow_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);;
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Water_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Air_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Velocity].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dry_block_Unloading_Velocity].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Count].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Velocity].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Delay].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);  
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count1].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);   
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count2].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);   
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity1].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity2].UseOption = Convert.ToInt32(GbVar.bCleanConfirm);      
	            #endregion

                #region COMPARE
                for (int i = 0; i < MESDF.EqRcpParam.Count; i++)
                {
                    if (!Enum.IsDefined(typeof(MESDF.eRCP_ITEMS), i)) continue;

                    if (MESDF.EqRcpParam[i].After != MESDF.EqRcpParam[i].Before)
                    {
                        MESDF.EqRcpParam[i].Modified = 1;
                        bChange = true;
                    }
                    else
                    {
                        MESDF.EqRcpParam[i].Modified = 0;
                    }

                    //임의로 0으로 해줌
                    //MESDF.EqRcpParam[i].UseOption = 0;
                }
                #endregion

                return bChange;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Host가 레시피 값을 요청할 시 배열 버퍼에 업로드 합니다.
        /// </summary>
        /// <param name="strRcpName"></param>
        public void UploadRecipeValue(string strRcpName)
        {
            Recipe Rcp = null;

            try
            {
                GetRcpList();
                for (int i = 0; i < RcpList.Count; i++)
                {
                    if (RcpList[i].strRecipeId == strRcpName)
                    {
                        Rcp = RcpList[i];
                        break;
                    }
                }

                #region AFTER
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Count].After = Rcp.MgzInfo.nSlotCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Pitch].After = Rcp.MgzInfo.dSlotPitch.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_X].After = Rcp.MapTbInfo.nUnitCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_Y].After = Rcp.MapTbInfo.nUnitCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_X].After = Rcp.MapTbInfo.dUnitPitchX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_Y].After = Rcp.MapTbInfo.dUnitPitchY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_X].After = Rcp.MapTbInfo.nInspChipViewCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_Y].After = Rcp.MapTbInfo.nInspChipViewCountY.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_X].After = Rcp.TrayInfo.nTrayCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_Y].After = Rcp.TrayInfo.nTrayCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_X].After = Rcp.TrayInfo.dTrayPitchX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_Y].After = Rcp.TrayInfo.dTrayPitchY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Thickness].After = Rcp.TrayInfo.dTrayThickness.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_X].After = Rcp.TrayInfo.nInspChipCountX.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_Y].After = Rcp.TrayInfo.nInspChipCountY.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Picker_Place_T].After = Rcp.TrayInfo.nPlaceAngle.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Count].After = Rcp.cleaning.nUnitPkSpongeCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Velocity].After = Rcp.cleaning.dUnitPkSpongeVel.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Water_Count].After = Rcp.cleaning.nFirstCleanWaterCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Air_Count].After = Rcp.cleaning.nUnitPkAirCleanCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Count].After = Rcp.cleaning.nUnitPkDryBlowCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Velocity].After = Rcp.cleaning.dUnitPkDryBlowVel.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Kit_Air_Blow_Count].After = Rcp.cleaning.nUnitPkKitAirBlowCount.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Water_Count].After = Rcp.cleaning.nCleanTableWaterCount.ToString();
                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Air_Count].After = Rcp.cleaning.nCleanTableAirCount.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Count].After = Rcp.cleaning.nDryBlockAirCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Velocity].After = Rcp.cleaning.dDryBlockAirVel.ToString();

                //MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dry_block_Unloading_Velocity].After = Rcp.cleaning.dDryBlockUnloadingVel.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Count].After = Rcp.cleaning.nStageBlockDryBlowCount.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Velocity].After = Rcp.cleaning.dStageBlockDryBlowVel.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Delay].After = Rcp.cleaning.lStageBlockDryBlowDelay.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count1].After = Rcp.cleaning.nMapTableDryBlowCount1.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count2].After = Rcp.cleaning.nMapTableDryBlowCount2.ToString();

                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity1].After = Rcp.cleaning.dMapTableDryBlowVel1.ToString();
                MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity2].After = Rcp.cleaning.dMapTableDryBlowVel2.ToString();
               
                #endregion

            }
            catch (Exception ex)
            {
            }
        }
    }
}
