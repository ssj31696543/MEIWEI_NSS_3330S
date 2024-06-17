using Glass;
using NSS_3330S.UC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.POP
{
    public partial class popSubMap : Form
    {
        StripInfo[] strips;
        STRIP_MDL eCurMdl = STRIP_MDL.NONE;

        bool bRefreshBlockFlag = false;
        public popSubMap()
        {
            InitializeComponent();
            this.Size = new Size(975, 353);
            this.CenterToScreen();
        }

        private void popSubMap_Load(object sender, EventArgs e)
        {
            btnfold.ImageIndex = 0;
            this.Size = new Size(975, 353);
            this.CenterToScreen();

            UpdateCurInfo();
            UpdateStripList();
        }

        private void btnfold_Click(object sender, EventArgs e)
        {
            if (btnfold.ImageIndex == 0)
            {
                btnfold.ImageIndex = 1;
                this.Size = new Size(975, 540);
            }
            else
            {
                btnfold.ImageIndex = 0;
                this.Size = new Size(975, 353);
            }
        }

        private void btnOverWrite_Click(object sender, EventArgs e)
        {
            UpdateSelectInfo(lstSubMap.SelectedIndices[0]);
        }

        /// <summary>
        /// 해당 모듈의 생산된 스트립 정보를 불러옵니다. 
        /// </summary>
        /// <param name="eMdl"></param>
        public void LoadStripInfos(STRIP_MDL eMdl)
        {
            StripInfo[] stripinfos;
            eCurMdl = eMdl;
            stripinfos = GbVar.StripMgr.LoadStripsInfo(eMdl);
            strips = new StripInfo[stripinfos.Length];

            for (int i = 0; i < stripinfos.Length; i++)
            {
                if (stripinfos[i] == null) continue;
                strips[i] = new StripInfo();
                strips[i].Init(stripinfos[i].UnitArr.Length);
                strips[i].Set(stripinfos[i].UnitArr[0].Length);
                stripinfos[i].CopyTo(ref strips[i]);
            }
        }

        /// <summary>
        /// 현재 모듈에 놓여 있는 스트립 정보를 화면에 업데이트합니다..
        /// </summary>
        /// <returns></returns>
        private bool UpdateCurInfo()
        {
            bool bRet = false;
            StripInfo strip = new StripInfo();
            try
            {
                switch (eCurMdl)
                {
                    case STRIP_MDL.STRIP_RAIL:
                        strip.Init(GbVar.Seq.sStripRail.Info.UnitArr.Length);
                        strip.Set(GbVar.Seq.sStripRail.Info.UnitArr[0].Length);
                        GbVar.Seq.sStripRail.Info.CopyTo(ref strip);
                        break;
                    case STRIP_MDL.STRIP_TRANSFER:
                        strip.Init(GbVar.Seq.sStripTransfer.Info.UnitArr.Length);
                        strip.Set(GbVar.Seq.sStripTransfer.Info.UnitArr[0].Length);
                        GbVar.Seq.sStripTransfer.Info.CopyTo(ref strip);
                        break;
                    case STRIP_MDL.CUTTING_TABLE:
                        strip.Init(GbVar.Seq.sCuttingTable.Info.UnitArr.Length);
                        strip.Set(GbVar.Seq.sCuttingTable.Info.UnitArr[0].Length);
                        GbVar.Seq.sCuttingTable.Info.CopyTo(ref strip);
                        break;
                    case STRIP_MDL.UNIT_TRANSFER:
                        strip.Init(GbVar.Seq.sUnitTransfer.Info.UnitArr.Length);
                        strip.Set(GbVar.Seq.sUnitTransfer.Info.UnitArr[0].Length);
                        GbVar.Seq.sUnitTransfer.Info.CopyTo(ref strip);
                        break;
                    case STRIP_MDL.UNIT_DRY:
                        strip.Init(GbVar.Seq.sUnitDry.Info.UnitArr.Length);
                        strip.Set(GbVar.Seq.sUnitDry.Info.UnitArr[0].Length);
                        GbVar.Seq.sUnitDry.Info.CopyTo(ref strip);
                        break;
                    case STRIP_MDL.MAP_TRANSFER:
                        strip.Init(GbVar.Seq.sMapTransfer.Info.UnitArr.Length);
                        strip.Set(GbVar.Seq.sMapTransfer.Info.UnitArr[0].Length);
                        GbVar.Seq.sMapTransfer.Info.CopyTo(ref strip);
                        break;
                    case STRIP_MDL.MAP_VISION_TABLE_1:
                        strip.Init(GbVar.Seq.sMapVisionTable[0].Info.UnitArr.Length);
                        strip.Set(GbVar.Seq.sMapVisionTable[0].Info.UnitArr[0].Length);
                        GbVar.Seq.sMapVisionTable[0].Info.CopyTo(ref strip);
                        break;
                    case STRIP_MDL.MAP_VISION_TABLE_2:
                        strip.Init(GbVar.Seq.sMapVisionTable[1].Info.UnitArr.Length);
                        strip.Set(GbVar.Seq.sMapVisionTable[1].Info.UnitArr[0].Length);
                        GbVar.Seq.sMapVisionTable[1].Info.CopyTo(ref strip);
                        break;
                    default:
                        break;
                }

                if (strip != null)
                {
                    ShowStripInfo(strip);
                }
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        /// <summary>
        /// 이전 스트립의 목록을 표시합니다.
        /// </summary>
        /// <returns></returns>
        private bool UpdateStripList()
        {
            bool bRet = false;
            try
            {
                lstSubMap.Items.Clear();

                for (int i = 0; i < strips.Length; i++)
                {
                    if (strips[i] == null) continue;

                    ListViewItem lvi = new ListViewItem((i + 1).ToString());
                    lvi.SubItems.Add(strips[i].dtLastCycleIn[(int)eCurMdl].ToString());
                    lvi.SubItems.Add(strips[i].STRIP_ID.ToString());
                    lstSubMap.View = View.Details;
                    lstSubMap.Items.Add(lvi);
                    lstSubMap.Items[lstSubMap.Items.Count - 1].EnsureVisible();
                }
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        /// <summary>
        /// 이전 생산 목록 중 선택한 Info를 현재 모듈에 업데이트 합니다.
        /// </summary>
        /// <param name="nIndex">선택한 리스트의 순번</param>
        /// <returns></returns>
        private bool UpdateSelectInfo(int nIndex)
        {
            bool bRet = false;
            try
            {
                GbVar.StripMgr.DownloadStripInfo(strips[nIndex], eCurMdl);
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if (bRefreshBlockFlag == true) return;
            bRefreshBlockFlag = true;

            ucSubMap1.Invalidate();
            
            bRefreshBlockFlag = false;
        }

        private void lstSubMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView ctrl = sender as ListView;

            if (ctrl.SelectedIndices.Count > 0)
            {
                int nIndex = ctrl.SelectedIndices[0];

                if (strips[nIndex] != null)
                {
                    ShowStripInfo(strips[nIndex]);
                }
            }
        }

        private void ShowStripInfo(StripInfo strip)
        {
            if (strip != null)
            {
                lbLotId.Text = strip.LOT_ID;
                lbStripId.Text = strip.STRIP_ID;

                int nSq = 0;
                int nNq = 0;
                int nReject = 0;
                int nSat = 0;

                if ((int)eCurMdl < (int)STRIP_MDL.MAP_VISION_TABLE_1)
                {
                    for (int nRow = 0; nRow < strip.UnitArr.Length; nRow++)
                    {
                        for (int nCol = 0; nCol < strip.UnitArr[0].Length; nCol++)
                        {
                            if (strip.UnitArr[nRow][nCol].ITS_XOUT == (int)MESDF.eCELL_STATUS.OK)
                            {
                                nSq++;
                            }
                            else if (strip.UnitArr[nRow][nCol].ITS_XOUT == (int)MESDF.eCELL_STATUS.XOUT)
                            {
                                nReject++;
                            }
                        }
                    }
                }
                else
                {
                    for (int nRow = 0; nRow < strip.UnitArr.Length; nRow++)
                    {
                        for (int nCol = 0; nCol < strip.UnitArr[0].Length; nCol++)
                        {
                            if (strip.UnitArr[nRow][nCol].ITS_XOUT == (int)MESDF.eCELL_STATUS.OK)
                            {
                                if (strip.UnitArr[nRow][nCol].TOP_INSP_RESULT <= (int)VSDF.eJUDGE_MAP.OK)
                                {
                                    nSq++;
                                }
                                else
                                {
                                    nReject++;
                                }
                            }
                            else if (strip.UnitArr[nRow][nCol].ITS_XOUT == (int)MESDF.eCELL_STATUS.XOUT)
                            {
                                nReject++;
                            }
                        }
                    }
                }


                lbSqQty.Text = nSq.ToString();
                lbNqQty.Text = nNq.ToString();
                lbRejectQty.Text = nReject.ToString();
                lbSatQty.Text = nSat.ToString();
                ucSubMap1.SetStripInfo(strip, eCurMdl);
            }
        }
    }
}
