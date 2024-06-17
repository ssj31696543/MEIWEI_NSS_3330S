using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DWORD = System.Int32;
using WORD = System.Int16;

namespace TypeConst
{

    //작업할 순서의 방향
    //NORMAL : 그룹단위 작업, 가로방향 좌->우로 순방향
    //ZIGZAG : 그룹단위 작업, 가로방향 좌->우, 좌<-우 교차 지그재그 방향
    public enum FLOW_MODE : int
    {
        NORMAL,
        ZIGZAG
    };


    [Flags]
    public enum ALIGN_DRAW
    {
        NORMAL = 0x00,
        TOP_SCH_ROI_FRAME_ON = 0x01,
        TOP_SCH_ROI_FRAME_OFF = 0x02,
        TOP_MRK_ROI_FRAME_ON = 0x04,
        TOP_MRK_FIND_RESULT = 0x08,

        BTM_SCH_ROI_FRAME_ON = 0x10,
        BTM_SCH_ROI_FRAME_OFF = 0x20,
        BTM_MRK_ROI_FRAME_ON = 0x40,
        BTM_MRK_FIND_RESULT = 0x80,

        CAL_ROI_FRAME_ON = 0x100,
        CAL_MRK_FIND_RESULT = 0x200,

        ALL = 0xFFFF,

    }


    [Flags]
    public enum INS_DRAW
    {
        NORMAL = 0x00,
        SCH_ROI_FRAME_ON = 0x01,
        SCH_ROI_FRAME_OFF = 0x02,
        EDGE_RESULT = 0x04,
        FIND_ROI_FRAME_ON = 0x08,
        FIND_RESULT = 0x10,
        FIND_CAL_ROI_FRAME_ON = 0x20,
        FIND_CAL_RESULT = 0x40,
        INS_RESULT = 0x80,
        ALL = 0xFF,

    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MATRIX3X3
    {
        public double m11, m12, m13;
        public double m21, m22, m23;
        public double m31, m32, m33;

        //생성자에서 초기화
        public MATRIX3X3(double d11, double d12, double d13, double d21, double d22, double d23, double d31, double d32, double d33)
        {
            this.m11 = d11;
            this.m12 = d12;
            this.m13 = d13;
            this.m21 = d21;
            this.m22 = d22;
            this.m23 = d23;
            this.m31 = d31;
            this.m32 = d32;
            this.m33 = d33;
        }
    };


    //[Serializable]
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    //public struct dxy
    //{
    //    public double x;
    //    public double y;
    //};

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dxy
    {
        public double x;
        public double y;

        public dxy(double dX, double dY)
        {
            x = dX;
            y = dY;
        }

        public dxy(dxy xy)
        {
            x = xy.x;
            y = xy.y;
        }

        public bool IsSamePoint(dxy xy, double dLimitSame = 0.001)
        {
            if (Math.Abs(x - xy.x) > dLimitSame)
                return false;

            if (Math.Abs(y - xy.y) > dLimitSame)
                return false;

            return true;
        }
    };




    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dxyz
    {
        public double x;
        public double y;
        public double z;
    };


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dxyzt
    {
        public double x;
        public double y;
        public double z;
        public double t;
    };


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dxyt
    {
        public double x;
        public double y;
        public double t;
    };


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct nxy
    {
        public int x;
        public int y;
    };


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct nroi
    {
        public int x;
        public int y;
        public int width;
        public int height;
    };

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LoginInfo
    {
        public string strPassOp;
        public string strPassEng;
        public string strPassMak;
    };


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MOTINFO
    {
        public int nAxis;
        public double dPos;
        public double dVel;
        public double dAcc;
        public double dDec;
        public int nAmpErrNo;
        public int nMoveErrNo;
        public int nHomeErrNo;
        public int nSwLimitErrNoP;
        public int nSwLimitErrNoN;
        public int nMotionInfoFaultErrNo;
        public bool isUseInPos;
        public double dInposGap;
        public long lMoveTime;
    }

    [Serializable]
    public struct GLASS_PROC_DATA
    {
        public string strProductSpecName;
        public string strWorkOrderId;
        public string strPPID;
        public string strProcOperation;
        public string strPanelId;
        public string strUserId;
        public string strInsJudge;
    }


    [Serializable]
    public struct PRODUCT
    {
        public Int64 n64RunTime;
        public Int64 n64StopTime;
        public Int64 n64ErrTime;

        public Int64 nGlassCnt;
        public Int64 nCellCnt;
        public Int64 nTrayCnt;

        public Int64 nInsOkCnt;
        public Int64 nInsNgCnt;
    }
}
