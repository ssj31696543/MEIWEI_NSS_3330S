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

namespace NSS_3330S.UC
{
    public partial class ucInspectionMap : UserControl
    {
        int nRowCount, nColCount;
        int nTableIdx;
        public static bool[,] b_SolidArray;

        Color clrGroup = new Color();
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

        public ucInspectionMap()
        {
            InitializeComponent();

            b_SolidArray = new bool[6, 10];
            SetDoubleBuffered(this);

            clrGroup = Color.OrangeRed;
            DETECTED_FLAG = false;
        }

        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered",
                                         BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                                         null, control, new object[] { true });
        }

        /// <summary>
        /// pjh 220728 수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ucInspMap_Paint(object sender, PaintEventArgs e)
        {
            if (this.DesignMode) return;

            Pen grppen = new Pen(clrGroup, 1.0f);//그룹 테두리
            Pen edgepen = new Pen(clrCellLine, 1.0f);//배열 테두리
            HatchBrush backbrush = new HatchBrush(HatchStyle.Max, Color.DarkGray, Color.Gray);//배경


            SolidBrush goodbrush = new SolidBrush(clrCellGood);
            SolidBrush rwbrush = new SolidBrush(Color.Yellow);
            SolidBrush ngbrush = new SolidBrush(clrCellNg);
            SolidBrush emptybrush = new SolidBrush(Color.FromArgb(32, 32, 32)); //빈배열

            e.Graphics.FillRectangle(backbrush, 0, 0, Size.Width, Size.Height);
            e.Graphics.DrawRectangle(edgepen, 10, 10, Size.Width - 20, Size.Height - 20);
            e.Graphics.FillRectangle(emptybrush, 11, 11, Size.Width - 21, Size.Height - 21);

            try
            {
			    //TODO null되지 않게 맵 배열 변경 시 객체 생성 하기
			    //if (GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr == null) return;
                //if (GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr.Length <= 0) return;
                if (GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr == null)
                {
                    GbVar.Seq.sMapVisionTable[TABLE_NO].Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);
                    GbVar.Seq.sMapVisionTable[TABLE_NO].Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                    return;
                }
                if (GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr.Length <= 0)
                {
                    GbVar.Seq.sMapVisionTable[TABLE_NO].Info.Init(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);
                    GbVar.Seq.sMapVisionTable[TABLE_NO].Info.Set(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                    return;
                }

                if (RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX <= 0 || RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY <= 0) return;

                if (GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr[0] == null) return;
                if (GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr[0].Length <= 0) return;

                double col = GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr[0].GetLength(0);
                double row = GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr.Length;

                if (nColCount < 1 || nRowCount < 1) return;

                float groupW = GbFunc.FloatTryParse((Math.Round((Size.Width - 20 - (5 * (RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX - 1))) / (double)RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX, 4)).ToString());
                float groupH = GbFunc.FloatTryParse((Math.Round((Size.Height - 20 - (5 * (RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY - 1))) / (double)RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY, 4)).ToString());

                //float W = GbFunc.FloatTryParse((Math.Round((Size.Width - 20) / col, 4)).ToString());
                //float H = GbFunc.FloatTryParse((Math.Round((Size.Height - 20) / row, 4)).ToString());

                float unitW = GbFunc.FloatTryParse((Math.Round((Size.Width - 20 - (5 * (RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX - 1))) / col, 4)).ToString());
                float unitH = GbFunc.FloatTryParse((Math.Round((Size.Height - 20 - (5 * (RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY - 1))) / row, 4)).ToString());

                Font font = new Font("Arial", 8, FontStyle.Bold);
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                for (int i = 0; i < col; i++)
                {
                    for (int j = 0; j < row; j++)
                    {
                        bool bIsUnit = GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr[j][i].IS_UNIT;
                        int nTopVisionResult = GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr[j][i].TOP_INSP_RESULT;
                        int nItsCode = GbVar.Seq.sMapVisionTable[TABLE_NO].Info.UnitArr[j][i].ITS_XOUT;

                        //int nGroupDrawOffsetX = i / (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX / RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX);
                        //int nGroupDrawOffsetY = j / (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY / RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);

                        int nGroupDrawOffsetX = i / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                        int nGroupDrawOffsetY = j / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_XOUT_USE].bOptionUse)
                        {
                            nItsCode = 1;
                        }
                        if (bIsUnit)
                        {
                            if (nItsCode == 1)
                            {
                                if (nTopVisionResult == 0)
                                {
                                    e.Graphics.DrawRectangle(edgepen, unitW * i + 10 + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 10 + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW, unitH);
                                    e.Graphics.FillRectangle(goodbrush, unitW * i + 12.5f + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 12.5f + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW - 5.0f, unitH - 5.0f);
                                }
                                else if (nTopVisionResult == 1)
                                {
                                    e.Graphics.DrawRectangle(edgepen, unitW * i + 10 + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 10 + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW, unitH);
                                    e.Graphics.FillRectangle(rwbrush, unitW * i + 12.5f + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 12.5f + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW - 5.0f, unitH - 5.0f);
                                }
                                else
                                {
                                    e.Graphics.DrawRectangle(edgepen, unitW * i + 10 + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 10 + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW, unitH);
                                    e.Graphics.FillRectangle(ngbrush, unitW * i + 12.5f + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 12.5f + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW - 5.0f, unitH - 5.0f);
                                }
                            }
                            else if (nItsCode == 2)
                            {
                                if (nTopVisionResult >= 2)
                                {
                                    e.Graphics.DrawRectangle(edgepen, unitW * i + 10 + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 10 + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW, unitH);
                                    e.Graphics.FillRectangle(ngbrush, unitW * i + 12.5f + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 12.5f + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW - 5.0f, unitH - 5.0f);
                                }
                                else
                                {
                                    e.Graphics.DrawRectangle(edgepen, unitW * i + 10 + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 10 + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW, unitH);
                                    e.Graphics.FillRectangle(rwbrush, unitW * i + 12.5f + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 12.5f + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW - 5.0f, unitH - 5.0f);
                                }
                            }
                            else
                            {
                                e.Graphics.DrawRectangle(edgepen, unitW * i + 10 + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 10 + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW, unitH);
                                e.Graphics.FillRectangle(ngbrush, unitW * i + 12.5f + (nGroupDrawOffsetX + (5 * nGroupDrawOffsetX)), unitH * j + 12.5f + (nGroupDrawOffsetY + (5 * nGroupDrawOffsetY)), unitW - 5.0f, unitH - 5.0f);
                            }
                        }
                        else
                        {
                            e.Graphics.DrawRectangle(edgepen, unitW * i + 10 + ((5 * (nGroupDrawOffsetX))), unitH * j + 10 + ((5 * (nGroupDrawOffsetY ))), unitW, unitH);
                            e.Graphics.FillRectangle(emptybrush, unitW * i + 12.5f + ((5 * (nGroupDrawOffsetX))), unitH * j + 12.5f + ((5 * (nGroupDrawOffsetY))), unitW - 5.0f, unitH - 5.0f);
                        }
                    }
                }

                for (int nGrpX = 0; nGrpX < RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX; nGrpX++)
                {
                    for (int nGrpY = 0; nGrpY < RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY; nGrpY++)
                    {
                        e.Graphics.DrawRectangle(grppen, groupW * nGrpX + 10 + (5 * nGrpX), groupH * nGrpY + 10 + (5 * nGrpY), groupW, groupH);
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
