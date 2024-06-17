using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Reflection;
using NSS_3330S;

namespace NSS_3330S
{
    public partial class ucSubMap  : UserControl
    {
        int nRowCount, nColCount;
        int nTableIdx;
        public static bool[,] b_SolidArray;
        StripInfo info = new StripInfo();

        Color clrCellSQ = new Color();
        Color clrCellNQ = new Color();
        Color clrCellReject = new Color();
        Color clrCellSAT = new Color();
        Color clrCellUnknown = new Color();
        Color clrCellLine = new Color();
        Color clrGrpLine = new Color();

        STRIP_MDL eCurMdl = STRIP_MDL.NONE;

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color SQColor
        {
            get { return clrCellSQ; }
            set
            {
                if (clrCellSQ == value) return;

                clrCellSQ = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color NQColor
        {
            get { return clrCellNQ; }
            set
            {
                if (clrCellNQ == value) return;

                clrCellNQ = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color RejectColor
        {
            get { return clrCellReject; }
            set
            {
                if (clrCellReject == value) return;

                clrCellReject = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color SATColor
        {
            get { return clrCellSAT; }
            set
            {
                if (clrCellSAT == value) return;

                clrCellSAT = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color UnknownColor
        {
            get { return clrCellUnknown; }
            set
            {
                if (clrCellUnknown == value) return;

                clrCellUnknown = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color EdgeColor
        {
            get { return clrCellLine; }
            set
            {
                if (clrCellLine == value) return;

                clrCellLine = value;
                this.Invalidate();
            }
        }


        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color GrpColor
        {
            get { return clrGrpLine; }
            set
            {
                if (clrGrpLine == value) return;

                clrGrpLine = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public int TABLE_NO
        {
            get { return nTableIdx; }
            set
            {
                if (nTableIdx == value) return;

                nTableIdx = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public int ROW
        {
            get { return nRowCount; }
            set
            {
                if (nRowCount == value) return;

                nRowCount = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public int COL
        {
            get { return nColCount; }
            set
            {
                if (nColCount == value) return;

                nColCount = value;
                this.Invalidate();
            }
        }

        public bool DETECTED_FLAG
        {
            get;
            set;
        }

        public ucSubMap()
        {
            InitializeComponent();

            b_SolidArray = new bool[6, 10];
            SetDoubleBuffered(this);

            DETECTED_FLAG = false;
        }

        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered",
                                         BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                                         null, control, new object[] { true });
        }

        private void ucSubMap_Paint(object sender, PaintEventArgs e)
        {
            Pen edgepen = new Pen(clrCellLine, 1.0f);//배열 테두리
            Pen GrpEdgepen = new Pen(clrGrpLine, 1.0f);//배열 테두리
            //HatchBrush backbrush = new HatchBrush(HatchStyle.Max, Color.DarkGray, Color.Gray);//배경
            SolidBrush backbrush = new SolidBrush(Color.DarkGreen);

            SolidBrush SQbrush = new SolidBrush(clrCellSQ);
            SolidBrush NQbrush = new SolidBrush(clrCellNQ);
            SolidBrush Rejectbrush = new SolidBrush(clrCellReject);
            SolidBrush SATbrush = new SolidBrush(clrCellSAT);
            SolidBrush Unknownbrush = new SolidBrush(Color.Gray);

            SolidBrush emptybrush = new SolidBrush(Color.FromArgb(32, 32, 32)); //빈배열
            SolidBrush brush = new SolidBrush(Color.WhiteSmoke);
            SolidBrush Originbrush = new SolidBrush(Color.Red);

            e.Graphics.FillRectangle(backbrush, 0, 0, Size.Width, Size.Height);
            e.Graphics.DrawRectangle(edgepen, 10, 10, Size.Width - 20, Size.Height - 20);
            e.Graphics.FillRectangle(emptybrush, 11, 11, Size.Width - 21, Size.Height - 21);

            try
            {
                if (info.UnitArr == null) return;
                if (info.UnitArr.Length <= 0) return;

                if (info.UnitArr[0] == null) return;
                if (info.UnitArr[0].Length <= 0) return;

                double row = info.UnitArr.Length;
                double col = info.UnitArr[0].Length;

                if (col < 1 || row < 1) return;

                float W = GbFunc.FloatTryParse((Math.Round((Size.Width - 20) / row, 4)).ToString());
                float H = GbFunc.FloatTryParse((Math.Round((Size.Height - 20) / col, 4)).ToString());

                Font font = new Font("Arial", 8, FontStyle.Bold);
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                Point[] point = new Point[] { new Point(0, 0), new Point(0, 15), new Point(15, 0) };
                e.Graphics.FillPolygon(brush, point);
                e.Graphics.FillEllipse(Originbrush, Size.Width - 9, 2, 7, 7);
                if ((int)eCurMdl < (int)STRIP_MDL.MAP_VISION_TABLE_1)
                {
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < col; j++)
                        {
                            bool bIsUnit = info.UnitArr[i][j].IS_UNIT;
                            int nItsCode = info.UnitArr[i][j].ITS_XOUT;
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_XOUT_USE].bOptionUse)
                            {
                                nItsCode = 1;
                            }
                            if (bIsUnit)
                            {
                                if (nItsCode == (int)MESDF.eCELL_STATUS.OK)
                                {
                                    e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                    e.Graphics.FillRectangle(SQbrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                                }
                                else if ( nItsCode == (int)MESDF.eCELL_STATUS.XOUT)
                                {
                                    e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                    e.Graphics.FillRectangle(Rejectbrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                                }
                                else
                                {
                                    e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                    e.Graphics.FillRectangle(Unknownbrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                                }
                            }
                            else
                            {
                                e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                e.Graphics.FillRectangle(emptybrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                            }
                        }
                    }
                }
                else
	            {

                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < col; j++)
                        {
                            bool bIsUnit = info.UnitArr[i][j].IS_UNIT;
                            int nHostCode = info.UnitArr[i][j].ITS_XOUT;
                            int nTopInspRet = info.UnitArr[i][j].TOP_INSP_RESULT;

                            if (bIsUnit)
                            {
                                if (nHostCode == (int)MESDF.eCELL_STATUS.OK)
                                {
                                    if (nTopInspRet <= 0)
                                    {
                                        e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                        e.Graphics.FillRectangle(SQbrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                                    }
                                    else
                                    {
                                        e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                        e.Graphics.FillRectangle(Rejectbrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                                    }
                                }
                                else if (nHostCode == (int)MESDF.eCELL_STATUS.XOUT)
                                {
                                    e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                    e.Graphics.FillRectangle(Rejectbrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                                }
                                else
                                {
                                    e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                    e.Graphics.FillRectangle(Unknownbrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                                }
                            }
                            else
                            {
                                e.Graphics.DrawRectangle(edgepen, W * ((int)row - i - 1) + 10, H * j + 10, W, H);
                                e.Graphics.FillRectangle(emptybrush, W * ((int)row - i - 1) + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                            }
                        }
                    }
                }

                GrpEdgepen.Color = Color.Red;

                for (int nGrpX = 0; nGrpX < RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX; nGrpX++)
                {
                    for (int nGrpY = 0; nGrpY < RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY; nGrpY++)
                    {
                        e.Graphics.DrawRectangle(GrpEdgepen, 
                                                    W * ((int)nGrpY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY) + 10, 
                                                    H * nGrpX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX + 10, 
                                                    W * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY,
                                                    H * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);
                    }
                }

            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }

            edgepen.Dispose();
            backbrush.Dispose();

            SQbrush.Dispose();
            NQbrush.Dispose();
            Rejectbrush.Dispose();
            SATbrush.Dispose();
            Unknownbrush.Dispose();

            emptybrush.Dispose();
        }

        public void SetStripInfo(StripInfo info, STRIP_MDL eCurMdl)
        {
            this.eCurMdl = eCurMdl;

            this.info.Init(info.UnitArr.Length);
            this.info.Set(info.UnitArr[0].Length);

            info.CopyTo(ref this.info);
        }
    }
}
