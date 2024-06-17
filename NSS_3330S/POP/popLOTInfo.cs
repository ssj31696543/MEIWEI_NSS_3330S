using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NSS_3330S.POP
{
    public partial class popLOTInfo : Form
    {
        int nCheckLotKind = 0;
        public popLOTInfo()
        {
            InitializeComponent();
        }

        private void popLOTInfo_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();
            
            if (GbVar.lstBinding_HostLot == null) return;

            if (GbVar.lstBinding_HostLot.Count <= 0) return;

            int nSubQty = 0;

            tb_LOTID.Text = GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID;
            //tb_ITSID.Text = GbVar.lstBinding_HostLot[MCDF.CURRLOT].ITS_ID;
            if (!int.TryParse(GbVar.lstBinding_HostLot[MCDF.CURRLOT].SUB_QTY, out nSubQty)) nSubQty = 0;
            NUD_STRIPCnt.Value = nSubQty;
        }

        private void Rbtn_CheckedChanged(object sender, EventArgs e)
        {
            if (Rbtn_LOTKind1.Checked == true) nCheckLotKind = Rbtn_LOTKind1.TabIndex;
            else if (Rbtn_LOTKind2.Checked == true) nCheckLotKind = Rbtn_LOTKind2.TabIndex;
            else if (Rbtn_LOTKind3.Checked == true) nCheckLotKind = Rbtn_LOTKind3.TabIndex;
            else if (Rbtn_LOTKind4.Checked == true) nCheckLotKind = Rbtn_LOTKind4.TabIndex;
            else if (Rbtn_LOTKind5.Checked == true) nCheckLotKind = Rbtn_LOTKind5.TabIndex;
            else { }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            {
                bool bIsStrip = false;
                bIsStrip |= GbVar.Seq.sStripRail.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.LD_RAIL_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.IN_LET_TABLE_VAC] == 1;

                bIsStrip |= GbVar.Seq.sStripTransfer.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.STRIP_PK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.STRIP_PK_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sCuttingTable.Info.IsStrip();

                bIsStrip |= GbVar.Seq.sUnitTransfer.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.UNIT_PK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.UNIT_PK_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sUnitDry.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sMapTransfer.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.MAP_PK_WORK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.MAP_PK_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sMapVisionTable[0].Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.MAP_STG_1_WORK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.MAP_ST_1_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sMapVisionTable[1].Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.MAP_STG_2_WORK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.MAP_ST_2_VAC_ON] == 1;

                bIsStrip |= !GbVar.Seq.sPkgPickNPlace.pInfo[0].IsPickerAllEmptyUnit();
                bIsStrip |= !GbVar.Seq.sPkgPickNPlace.pInfo[1].IsPickerAllEmptyUnit();

                IODF.A_INPUT[] _arrInput = new IODF.A_INPUT[16] {
                                                                IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_2_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_3_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_4_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_5_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_6_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_7_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_8_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_1_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_2_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_3_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_4_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_5_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_6_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_7_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_8_PICKER_VACUUM,
                };

                for (int i = 0; i < 16; i++)
                {
                    int nIoIdx = (int)_arrInput[i];
                    double dVacValue = (GbVar.GB_AINPUT[nIoIdx] - ConfigMgr.Inst.Cfg.Vac[nIoIdx].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nIoIdx].dRatio;

                    if (dVacValue < ConfigMgr.Inst.Cfg.Vac[nIoIdx].dVacLevelLow)
                    {
                        bIsStrip |= true;
                    }
                    else
                    {
                        bIsStrip |= false;
                    }
                    bIsStrip |= GbVar.GB_CHIP_PK_VAC[i];
                }
                for (int nRow = GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        bIsStrip |= GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr[nRow][nCol].IS_UNIT;
                    }
                }
                for (int nRow = GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        bIsStrip |= GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr[nRow][nCol].IS_UNIT;
                    }
                }
                for (int nRow = GbVar.Seq.sUldRWTrayTable.Info.UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        bIsStrip |= GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].IS_UNIT;
                    }
                }
                if (bIsStrip)
                {
                    popMessageBox pMsgStartIntl = new popMessageBox(FormTextLangMgr.FindKey("PLEASE REMOVE ALL STRIP, UNIT, TRAY !!."), "Lot Input Warning");
                    pMsgStartIntl.TopMost = true;
                    if (pMsgStartIntl.ShowDialog(this) != DialogResult.OK) return; else return;
                }
            }

            #region ID Valid
            popMessageBox msg;
            if (tb_LOTID.Text == "")
            {
                msg = new popMessageBox("LOT ID Error! \r\nConfirm LOT ID.", "Waring", MessageBoxButtons.OK);
                msg.ShowDialog();
                return;
            }

            msg = new popMessageBox(FormTextLangMgr.FindKey("Do you want to register LOT with the information you entered?"), FormTextLangMgr.FindKey("Question"), MessageBoxButtons.OKCancel);
            if (msg.ShowDialog() != DialogResult.OK) return;
            #endregion

            #region Check Registered Lot
            if (GbVar.lstBinding_HostLot == null)
            {
                GbVar.lstBinding_HostLot = new BindingList<HostLotInfo>();
            }

            #endregion
            //랏엔드 안하고 랏투입 또하면
            if (!GbVar.bLotEndComp)
            {
                if (tb_LOTID.Text != GbVar.lstBinding_HostLot[0].LOT_ID)
                {
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].EQP_OUT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    GbVar.LotReportLog.AddStripLog(GbVar.lstBinding_HostLot[0].LOT_ID, true);
                    GbVar.bLotEndComp = true;
                }
              
            }

            //220611 pjh
            #region EqpProc 
            if (GbVar.lstBinding_EqpProc == null)
            {
                GbVar.lstBinding_EqpProc = new BindingList<EqpProcInfo>();
            }

            EqpProcInfo eqpInfo = new EqpProcInfo();
            eqpInfo.LOT_ID = tb_LOTID.Text;

            GbVar.lstBinding_EqpProc.Insert(MCDF.CURRLOT, eqpInfo);//220816
            if (GbVar.lstBinding_EqpProc.Count > 2)
            {
                //등록된 리스트가 3개 이상이면 0, 1번째를 제외한 나머지 삭제
                for (int nCnt = GbVar.lstBinding_EqpProc.Count - 1; nCnt > 1; nCnt--)
                {
                    GbVar.lstBinding_EqpProc.RemoveAt(nCnt);
                }
            }

            //현재 상태 DB업데이트
            GbVar.LotMgr.UpdateAllProcQtyInfo(GbVar.lstBinding_EqpProc);
            #endregion

            #region HostLot            
            GbVar.lstBinding_HostLot.Clear();

            HostLotInfo hostinfo = new HostLotInfo();
            hostinfo.LOT_ID = tb_LOTID.Text;
            hostinfo.SUB_QTY = NUD_STRIPCnt.Value.ToString();
            hostinfo.LOT_TYPE = nCheckLotKind.ToString();
            GbVar.lstBinding_HostLot.Add(hostinfo);            

            GbVar.Seq.sLdMzLoading.LOT_ID = GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID;
            #endregion

            //Vision DB Update
            GbVar.dbSetVision.UpdateLotID(hostinfo.LOT_ID);
            
            //GEM 미사용 시 Lot정보 업데이트
            //GEM 사용 시 상위 결과내려 온 후 등록  
            if(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse == false)
                GbVar.LotMgr.UpdateAllLotInfo(GbVar.lstBinding_HostLot);

            //ITS Info Loading
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_XOUT_USE].bOptionUse)
            {
                //GbVar.dbITS.SelectXoutLoc(GbVar.lstBinding_HostLot[MCDF.CURRLOT].ITS_ID, GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID);
            }
            GbVar.LotReportLog.SetFileName(DateTime.Now, GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID);
            GbVar.StripTTLog.SetFileName(DateTime.Now, GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID);//220817 pjh


            msg = new popMessageBox(FormTextLangMgr.FindKey("REGISTER COMPLETE"), "COMP", MessageBoxButtons.OK);
            msg.ShowDialog();
            this.Hide();

            GbVar.product.nRunTime = 0;
            GbVar.product.nStopTime = 0;
            GbVar.product.nErrTime = 0;

            GbVar.bLotInsert = true;
            GbVar.bFirstLotCheck = true;

            GbVar.product.dateTimeLotInputTime = DateTime.Now;

        }

        private void btn_EXIT_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        #region Move Window Form Without Tilte Bar (드래그로 창 움직이기)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void lbTitle_MouseDown(object sender,
        System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        private void panel1_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                lbTitle.Text = FormTextLangMgr.FindKey("LOT REGISTRATION");
                btn_OK.Text = FormTextLangMgr.FindKey("REGISTER");
                btn_EXIT.Text = FormTextLangMgr.FindKey("EXIT");
                lbl_STRIPCnt.Text = FormTextLangMgr.FindKey("STRIP COUNT :");
            }
        }
    }
}
