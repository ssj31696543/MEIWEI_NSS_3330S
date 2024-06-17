using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Drawing.Design;
using DionesTool.Objects;

namespace NSS_3330S
{
    [Serializable]
    public class Recipe : ICloneable
    {
        public string strNo = "99";
        public string strRecipeId = "noname";
        //public string strRecipeName = "noname";
        public string strRecipeDescription = "";

        public double dAlignXCorrOffset = 0.000f;

        public RcpServo[] dMotPos = new RcpServo[SVDF.SV_CNT];
        public MagazineInfo MgzInfo = new MagazineInfo();
        public MapTableInfo MapTbInfo = new MapTableInfo();
        //public BallInspInfo BallInfo = new BallInspInfo();
        public UldTrayInfo TrayInfo = new UldTrayInfo();
        public CleaningInfo cleaning = new CleaningInfo();

        public List<MapGroupInfo> listMapGrpInfoL = new List<MapGroupInfo>();
        public List<MapGroupInfo> listMapGrpInfoR = new List<MapGroupInfo>();

        public Recipe()
        {
            for (int nMotCount = 0; nMotCount < SVDF.SV_CNT; nMotCount++)
            {
                dMotPos[nMotCount] = new RcpServo();
            }
        }


        public void CopyTo(ref Recipe info)
        {
            for (int nMotCount = 0; nMotCount < SVDF.SV_CNT; nMotCount++)
            {
                info.dMotPos[nMotCount] = dMotPos[nMotCount];
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool Serialize(string path)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Recipe));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, this);
                }
            }
            catch (Exception ex)
            {
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
                return false;
            }

            return true;
        }

        public static Recipe Deserialize(string path)
        {
            Recipe inst = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Recipe));
                using (StreamReader rd = new StreamReader(path))
                {
                    inst = (Recipe)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
            }

            return inst;
        }
    }

    #region SERVO
    [Serializable]
    public class RcpServo
    {
        public double[] dPos = new double[SVDF.POS_CNT];
        //아래 속도와 가감속은 일반적인경우 사용하지 않는다.. 특별히 모델별로 속도조정이 필요한 시퀀스에서만 추가해서 쓸것..
        public double[] dVel = new double[SVDF.POS_CNT];
        public double[] dAcc = new double[SVDF.POS_CNT];
        public double[] dDec = new double[SVDF.POS_CNT];
    }
    #endregion

    [Serializable]
    public enum CLEAN_TYPE
    {
        SPONGE,
        WATER_CLEAN,
        AIR_CLEAN,
        BOX_BLOW,
    }

    [Serializable]
    public enum INSP_GG_TYPE
    {
        BLACK_TO_WHITE,
        WHITE_TO_BLACK,
        WHITE_TO_BLACK_OR_BLACK_TO_WHITE,
        BLACK_TO_WHITE_TO_BLACK,
        WHITE_TO_BLACK_WHITE,
    }

    [Serializable]
    public enum INSP_GG_CHOICE
    {
        FROM_BEGIN,
        FROM_END,
        LARGEST_AMPLITUDE,
        LARGEST_AREA,
        CLOSEST,
        ALL,
    }

    [Serializable]
    public class MagazineInfo
    {
        public string strMgzFileName = "test.xml";

        public int nSlotCount;
        public double dSlotPitch;

        public MagazineInfo()
        {
            //
        }


        public void CopyTo(ref MagazineInfo info)
        {
            //
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool Serialize(string path)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(MagazineInfo));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, this);
                }
            }
            catch (Exception ex)
            {
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
                return false;
            }

            return true;
        }

        public static MagazineInfo Deserialize(string path)
        {
            MagazineInfo inst = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(MagazineInfo));
                using (StreamReader rd = new StreamReader(path))
                {
                    inst = (MagazineInfo)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
            }

            return inst;
        }
    }

    [Serializable]
    public class MapTableInfo
    {
        public string strMapTbFileName = "test.xml";

        public int nMapGroupCntX;
        public int nMapGroupCntY;

        public double dMapGroupOffsetX;
        public double dMapGroupOffsetY;

        public int nUnitCountX;
        public int nUnitCountY;

        public int nUnitInspCountX;
        public int nUnitInspCountY;

        public double dUnitPitchX;
        public double dUnitPitchY;

        public double dUnitSizeX;
        public double dUnitSizeY;

        public int nInspChipViewCountX;
        public int nInspChipViewCountY;

        public bool isStripMat = false;

        public double dUnitColGap;
        public double dUnitRowGap;

        public double dChipThickness;

        public bool bGrabOneEdge = false;
        public MapTableInfo()
        {
            //
        }


        public void CopyTo(ref MapTableInfo info)
        {
            //
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool Serialize(string path)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(MapTableInfo));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, this);
                }
            }
            catch (Exception ex)
            {
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
                return false;
            }

            return true;
        }

        public static MapTableInfo Deserialize(string path)
        {
            MapTableInfo inst = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(MapTableInfo));
                using (StreamReader rd = new StreamReader(path))
                {
                    inst = (MapTableInfo)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
            }

            return inst;
        }
    }

    /// <summary>
    /// Map group 
    /// 항목을 추가하면 반드시 생성자에 복사하는 기능을 만들어줘야 저장이 된다
    /// </summary>
    [Serializable]
    public class MapGroupInfo : ICloneable
    {
        public int nArrGroupNo;

        public double dTopInspPosX;
        public double dTopInspPosY;

        public double[] dPickupPosX = new double[2];
        public double[] dPickupPosY = new double[2];

        #region Properties

        public int GROUP_NO
        {
            get { return nArrGroupNo; }
            set { nArrGroupNo = value; }
        }

        public double TOP_INSP_POS_X
        {
            get { return dTopInspPosX; }
            set { dTopInspPosX = value; }
        }

        public double TOP_INSP_POS_Y
        {
            get { return dTopInspPosY; }
            set { dTopInspPosY = value; }
        }

        public double PICK_UP_POS_X1
        {
            get { return dPickupPosX[0]; }
            set { dPickupPosX[0] = value; }
        }

        public double PICK_UP_POS_Y1
        {
            get { return dPickupPosY[0]; }
            set { dPickupPosY[0] = value; }
        }

        public double PICK_UP_POS_X2
        {
            get { return dPickupPosX[1]; }
            set { dPickupPosX[1] = value; }
        }

        public double PICK_UP_POS_Y2
        {
            get { return dPickupPosY[1]; }
            set { dPickupPosY[1] = value; }
        }

        #endregion

#pragma warning disable 612,618
        public MapGroupInfo()
        {
            //
        }

        public MapGroupInfo(MapGroupInfo info)
        {
            //
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
#pragma warning restore 612,618
    }

    [Serializable]
    public class BallInspInfo
    {
        public int nInspCountX = 0;
        public double dInspPitchX;

        public int nInspChipCount;
    }

    /// <summary>
    ///  [2022.05.26.kmlee] 하부 비젼 기준 값 삭제
    /// </summary>
    [Serializable]
    public class UldTrayInfo
    {
        public string strTrayFileName = "test.xml";

        public int nTrayCountX;
        public int nTrayCountY;

        public double dTrayPitchX;
        public double dTrayPitchY;
        public double dTrayThickness;

        public int nInspChipCountX;
        public int nInspChipCountY;

        public double dInspChipOffsetY;
        public double[] dInspChipMissMaxValue = new double[2];
        public double[] dInspChipFloatingMinValue = new double[2];

        public int nKeyencePgmNo;

        public int nPlaceAngle = 0;

        public dxy[][] dGdTrayOffset = new dxy[2][]; // new double[PICKER,TABLE]

        public dxy[] dRwTrayOffset = new dxy[2];

        public UldTrayInfo()
        {

            //
            for (int nPICKER = 0; nPICKER < 2; nPICKER++)
            {
                dGdTrayOffset[nPICKER] = new dxy[2];

                for (int nTABLE = 0; nTABLE < 2; nTABLE++)
                {
                    dGdTrayOffset[nPICKER][nTABLE] = new dxy();
                }
            }
            for (int nPICKER = 0; nPICKER < 2; nPICKER++)
            {
                dRwTrayOffset[nPICKER] = new dxy();
            }
            for (int nTABLE = 0; nTABLE < 2; nTABLE++)
            {
                dInspChipMissMaxValue[nTABLE] = 0;
                dInspChipFloatingMinValue[nTABLE] = 0;
            }
        }


        public void CopyTo(ref UldTrayInfo info)
        {
            //
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool Serialize(string path)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UldTrayInfo));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, this);
                }
            }
            catch (Exception ex)
            {
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
                return false;
            }

            return true;
        }

        public static UldTrayInfo Deserialize(string path)
        {
            UldTrayInfo inst = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UldTrayInfo));
                using (StreamReader rd = new StreamReader(path))
                {
                    inst = (UldTrayInfo)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
            }

            return inst;
        }
    }

    [Serializable]
    public class CleaningInfo
    {
        //211001 CHOH: 아래 리스트는 나중에 필요하면 쓰고, 일단은 안씀
        public List<CleaningItem_NotUse> UnitTrsfItem;
        public List<CleaningItem_NotUse> CleanTableItem;
        public List<CleaningItem_NotUse> DryTableItem;

        public int nUnitPkSpongeCount = 1;
        public double dUnitPkSpongeVel = 10;
        public long lUnitPkSpongeStartDelay = 100;
        public long lUnitPkSpongeEndDelay = 100;

        public int nUnitPkCleanerSwingCount = 1;

        public int nUnitPkDryBlowCount = 1;
        public double dUnitPkDryBlowVel = 10;

        public int nUnitPkKitAirBlowCount = 1;

        public int nCleanTableWaterCount = 1;
        public int nCleanTableAirCount = 1;

        public int nDryBlockAirCount = 1;
        public double dDryBlockAirVel = 10;

        public double dDryBlockUnloadingVel = 10;

        public int nStageBlockDryBlowCount = 1;
        public double dStageBlockDryBlowVel = 10;
        public long lStageBlockDryBlowDelay = 100;

        public int nMapTableDryBlowCount1 = 1;
        public int nMapTableDryBlowCount2 = 1;

        public double dMapTableDryBlowVel1 = 10;
        public double dMapTableDryBlowVel2 = 10;

        public int nScrapCount1 = 1; 
        public int nScrapCount2 = 1;

        public int nScrap1Delay = 100;
        public int nScrap2Delay = 100;

        public int nWaterSwingCnt = 1;
        public long lWaterSwingStartDelay = 100;
        public long lnWaterSwingEndDelay = 100;

        public int nBtmDryCnt = 1;
        public long lBtmDryStartDelay = 10;
        public long lBtmDryEndDelay = 10;
        public int nBtmDryVel = 10;

        public int nTopDryCnt = 1;
        public long lTopDryStartDelay = 10;
        public long lTopDryEndDelay = 10;
        public int nTopDryVel = 10;

        public int nScrapMode = 0;

        public int nRepickCount = 0;
    }

    [Serializable]
    public class CleaningItem_NotUse
    {
        public int nType;
        public int nRepeatCount;

        public CleaningItem_NotUse()
        {
            nType = (int)CLEAN_TYPE.AIR_CLEAN;
            nRepeatCount = 1;
        }

        public CleaningItem_NotUse(CLEAN_TYPE Type, int Count)
        {
            nType = (int)Type;
            nRepeatCount = Count;
        }

        public CleaningItem_NotUse(int Type, int Count)
        {
            nType = (int)Type;
            nRepeatCount = Count;
        }
    }
}
