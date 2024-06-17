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

namespace NSS_3330S.UC
{
    public partial class ucUldTrayMap : UserControl
    {
        int nRowCount, nColCount;
        int nTableIdx;
        public static bool[,] b_SolidArray;

        Color clrCellGood = new Color();
        Color clrCellNg = new Color();
        Color clrCellLine = new Color();

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color GoodColor
        {
            get { return clrCellGood; }
            set
            {
                if (clrCellGood == value) return;

                clrCellGood = value;
                this.Invalidate();
            }
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Color NgColor
        {
            get { return clrCellNg; }
            set
            {
                if (clrCellNg == value) return;

                clrCellNg = value;
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
        public ucUldTrayMap()
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

        private void ucUldTrayMap_Paint(object sender, PaintEventArgs e)
        {
            Pen edgepen = new Pen(clrCellLine, 1.0f);//배열 테두리
            HatchBrush backbrush = new HatchBrush(HatchStyle.Max, Color.DarkGray, Color.Gray);//배경


            SolidBrush goodbrush = new SolidBrush(clrCellGood);
            SolidBrush ngbrush = new SolidBrush(clrCellNg);
            SolidBrush emptybrush = new SolidBrush(Color.FromArgb(32, 32, 32)); //빈배열

            e.Graphics.FillRectangle(backbrush, 0, 0, Size.Width, Size.Height);
            e.Graphics.DrawRectangle(edgepen, 10, 10, Size.Width - 20, Size.Height - 20);
            e.Graphics.FillRectangle(emptybrush, 11, 11, Size.Width - 21, Size.Height - 21);

            try
            {
                double col, row;
                int nCol, nRow;

                if (TABLE_NO == 0)
                {
				    //TODO null되지 않게 맵 배열 변경 시 객체 생성 하기
				    //if (GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr == null) return;
                    //if (GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr.Length <= 0) return;
                    if (GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr == null)
                    {
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                        return;
                    }
                    if (GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr.Length <= 0)
                    {
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);
                        GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                        return;
                    }

                    if (GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr[0] == null) return;
                    if (GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr[0].Length <= 0) return;

                    nCol = GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr[0].GetLength(0);
                    nRow = GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr.Length;
                }
                else
                {
				    //TODO null되지 않게 맵 배열 변경 시 객체 생성 하기
				    //if (GbVar.Seq.sUldRWTrayTable.Info.UnitArr == null) return;
                    //if (GbVar.Seq.sUldRWTrayTable.Info.UnitArr.Length <= 0) return;
                    if (GbVar.Seq.sUldRWTrayTable.Info.UnitArr == null)
                    {
                        GbVar.Seq.sUldRWTrayTable.Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);
                        GbVar.Seq.sUldRWTrayTable.Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                        
                        return;
                    }
                    if (GbVar.Seq.sUldRWTrayTable.Info.UnitArr.Length <= 0)
                    {
                        GbVar.Seq.sUldRWTrayTable.Info.Init(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY);
                        GbVar.Seq.sUldRWTrayTable.Info.Set(RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);

                        return;
                    }

                    if (GbVar.Seq.sUldRWTrayTable.Info.UnitArr[0] == null) return;
                    if (GbVar.Seq.sUldRWTrayTable.Info.UnitArr[0].Length <= 0) return;

                    nCol = GbVar.Seq.sUldRWTrayTable.Info.UnitArr[0].GetLength(0);
                    nRow = GbVar.Seq.sUldRWTrayTable.Info.UnitArr.Length;
                }
                
                if (nColCount < 1 || nRowCount < 1) return;

                float W = GbFunc.FloatTryParse((Math.Round((Size.Width - 20) / (double)nCol, 4)).ToString());
                float H = GbFunc.FloatTryParse((Math.Round((Size.Height - 20) / (double)nRow, 4)).ToString());

                Font font = new Font("Arial", 8, FontStyle.Bold);
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                for (int i = 0; i < nCol; i++)
                {
                    int nRowIdx = 0;
                    for (int j = nRow-1; j >= 0; j--)
                    {
                        int nResult;
                        bool bIsUnit;

                        if (TABLE_NO == 1)
                        {
                            nResult = GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRowIdx][i].TOP_INSP_RESULT;
                            bIsUnit = GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRowIdx][i].IS_UNIT;
                        }
                        else
                        {
                            // 나중에 IsUnit로 변경 필요
                            nResult = GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr[nRowIdx][i].TOP_INSP_RESULT;
                            bIsUnit = GbVar.Seq.sUldGDTrayTable[GbVar.Seq.sPkgPickNPlace.nPlaceToGoodTableNo].Info.UnitArr[nRowIdx][i].IS_UNIT;
                        }

                        if (bIsUnit != true)
                        {
                            e.Graphics.DrawRectangle(edgepen, W * i + 10, H * j + 10, W, H);
                            e.Graphics.FillRectangle(emptybrush, W * i + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                        }
                        else
                        {
                            e.Graphics.DrawRectangle(edgepen, W * i + 10, H * j + 10, W, H);
                            e.Graphics.FillRectangle(goodbrush, W * i + 12.5f, H * j + 12.5f, W - 5.0f, H - 5.0f);
                        }

                        nRowIdx++;
                    }
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }

            edgepen.Dispose();
            backbrush.Dispose();

            goodbrush.Dispose();
            ngbrush.Dispose();
            emptybrush.Dispose();
        }
    }
}
