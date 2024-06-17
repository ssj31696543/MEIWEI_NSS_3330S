using NSS_3330S.POP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public partial class UcScrapInfo : UserControl
    {
        Button[] m_btnScrapArray;
        TextBox[] m_tbxBarcode;

        public UcScrapInfo()
        {
            InitializeComponent();

            m_btnScrapArray = new Button[] { btnScrap0, btnScrap1, btnScrap2, btnScrap4, btnScrap8, btnScrap9, btnScrap10, btnScrap11 };
            m_tbxBarcode = new TextBox[] { tbxBarcodeStripRail, tbxBarcodeStripPk, tbxBarcodeCutTableL, tbxBarcodeUnitPk,
                                            tbxBarcodeDryBlock, tbxBarcodeMapPk, tbxBarcodeMapTable1, tbxBarcodeMapTable2 };
        }

        private void UcScrapInfo_Load(object sender, EventArgs e)
        {

        }

        public void VisibleChange(bool bVisible)
        {
            tmrRefresh.Enabled = bVisible;
        }

        private void btnScrap_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            if (nTag < 0) return;

            STRIP_MDL eMdl = GetModuleFromTag(nTag);

            if (eMdl != STRIP_MDL.NONE)
            {
                popMessageBox msg;

                if (!GbFunc.IsStrip(eMdl))
                {
                    return;
                }

                switch (eMdl)
                {
                    case STRIP_MDL.MAP_VISION_TABLE_1:
                        {
                            if (GbVar.Seq.sMapVisionTable[0].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START])
                            {
                                msg = new popMessageBox(string.Format("{0} {1}{2}", FormTextLangMgr.FindKey("픽업 중인"), FormTextLangMgr.FindKey(GbFunc.GetStripName(eMdl)), FormTextLangMgr.FindKey("의 자재는 제거할 수 없습니다.")), "WARNING", MessageBoxButtons.OK);
                                msg.TopMost = true;
                                msg.ShowDialog(this);
                                return;
                            }
                        }
                        break;
                    case STRIP_MDL.MAP_VISION_TABLE_2:
                        {
                            if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START])
                            {
                                msg = new popMessageBox(string.Format("{0} {1}{2}", FormTextLangMgr.FindKey("픽업 중인"), FormTextLangMgr.FindKey(GbFunc.GetStripName(eMdl)), FormTextLangMgr.FindKey("의 자재는 제거할 수 없습니다.")), "WARNING", MessageBoxButtons.OK);
                                msg.TopMost = true;
                                msg.ShowDialog(this);
                                return;
                            }
                        }
                        break;
                }
                msg = new popMessageBox(string.Format("{0}{1}", FormTextLangMgr.FindKey(GbFunc.GetStripName(eMdl)), FormTextLangMgr.FindKey(" 자재를 제거하시겠습니까?")), "스크랩");
                msg.TopMost = true;
                if (msg.ShowDialog(this) != DialogResult.OK) return;

                GbFunc.StripScrap(eMdl);
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            bool bIsOn = false;

            for (int nCnt = 0; nCnt < m_btnScrapArray.Length; nCnt++)
            {
                if (m_btnScrapArray[nCnt] == null)
                    continue;

                int nTag = m_btnScrapArray[nCnt].GetTag();
                if (nTag < 0) continue;

                STRIP_MDL eMdl = GetModuleFromTag(nTag);

                if (GbVar.mcState.IsRun() || !GbFunc.IsStrip(eMdl))
                {
                    if (m_btnScrapArray[nCnt].Enabled)
                    {
                        m_btnScrapArray[nCnt].Enabled = false;
                    }
                }
                else
                {
                    if (!m_btnScrapArray[nCnt].Enabled)
                    {
                        m_btnScrapArray[nCnt].Enabled = true;
                    }
                }
            }

            for (int nCnt = 0; nCnt < m_tbxBarcode.Length; nCnt++)
            {
                if (m_tbxBarcode[nCnt] == null)
                    continue;

                int nTag = m_tbxBarcode[nCnt].GetTag();
                if (nTag < 0) continue;

                STRIP_MDL eMdl = GetModuleFromTag(nTag);

                StripInfo stripInfo = GbFunc.GetStripInfo(eMdl);

                if (m_tbxBarcode[nCnt].Text != stripInfo.STRIP_ID)
                    m_tbxBarcode[nCnt].Text = stripInfo.STRIP_ID;
            }

            ShowIsStripA1Panel();

            #region Analog Vac
            double dValue = 0.0;
            int nVacNo = 0;
            AIRSTATUS eAirStatus = AIRSTATUS.NONE;
            SeqBase seqBase = new SeqBase();
            Label lblCurrent = null;
            Color colorVacErr = Color.Red;

            nVacNo = (int)IODF.A_INPUT.LD_RAIL_VAC;
            lblCurrent = lbStripRailVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.STRIP_RAIL);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.STRIP_PK_VAC;
            lblCurrent = lbStripPickerVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.STRIP_TRANSFER);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.UNIT_PK_VAC;
            lblCurrent = lbUnitPickerVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.UNIT_TRANSFER);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");


            nVacNo = (int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC;
            lblCurrent = lbDryBlockVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.UNIT_DRY);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.MAP_PK_WORK_VAC;
            lblCurrent = lbMapPickerVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.MAP_TRANSFER);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.MAP_STG_1_WORK_VAC;
            lblCurrent = lbMapStage1Vac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.MAP_VISION_TABLE_1);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.MAP_STG_2_WORK_VAC;
            lblCurrent = lbMapStage2Vac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.MAP_VISION_TABLE_2);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");
            #endregion
        }

        void ShowIsStripA1Panel()
        {
            #region Exists Strip
            bool bIsOn = false;
            Color clScrapNeed = Color.Orange;
            Color clStripExists = Color.LawnGreen;
            bIsOn = GbVar.Seq.sStripRail.Info.IsStrip();
            if (bIsOn)
            {
                if (GbFunc.IsScrapNeed(STRIP_MDL.STRIP_RAIL))
                {
                    if (a1pStripRail.GradientEndColor != clScrapNeed)
                    {
                        a1pStripRail.GradientEndColor = clScrapNeed;
                        a1pStripRail.GradientStartColor = clScrapNeed;
                    }
                }
                else
                {
                    if (a1pStripRail.GradientEndColor != Color.LawnGreen)
                    {
                        a1pStripRail.GradientEndColor = Color.LawnGreen;
                        a1pStripRail.GradientStartColor = Color.LawnGreen;
                    }
                }
            }
            else if (!bIsOn && a1pStripRail.GradientEndColor != SystemColors.Control)
            {
                a1pStripRail.GradientEndColor = SystemColors.Control;
                a1pStripRail.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sStripTransfer.Info.IsStrip();
            if (bIsOn)
            {
                if (GbFunc.IsScrapNeed(STRIP_MDL.STRIP_TRANSFER))
                {
                    if (a1pStripPk.GradientEndColor != clScrapNeed)
                    {
                        a1pStripPk.GradientEndColor = clScrapNeed;
                        a1pStripPk.GradientStartColor = clScrapNeed;
                    }
                }
                else
                {
                    if (a1pStripPk.GradientEndColor != Color.LawnGreen)
                    {
                        a1pStripPk.GradientEndColor = Color.LawnGreen;
                        a1pStripPk.GradientStartColor = Color.LawnGreen;
                    }
                }
            }
            else if (!bIsOn && a1pStripPk.GradientEndColor != SystemColors.Control)
            {
                a1pStripPk.GradientEndColor = SystemColors.Control;
                a1pStripPk.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sUnitTransfer.Info.IsStrip();
            if (bIsOn)
            {
                if (GbFunc.IsScrapNeed(STRIP_MDL.UNIT_TRANSFER))
                {
                    if (a1pUnitPk.GradientEndColor != clScrapNeed)
                    {
                        a1pUnitPk.GradientEndColor = clScrapNeed;
                        a1pUnitPk.GradientStartColor = clScrapNeed;
                    }
                }
                else
                {
                    if (a1pUnitPk.GradientEndColor != Color.LawnGreen)
                    {
                        a1pUnitPk.GradientEndColor = Color.LawnGreen;
                        a1pUnitPk.GradientStartColor = Color.LawnGreen;
                    }
                }
            }
            else if (!bIsOn && a1pUnitPk.GradientEndColor != SystemColors.Control)
            {
                a1pUnitPk.GradientEndColor = SystemColors.Control;
                a1pUnitPk.GradientStartColor = SystemColors.Control;
            }


            bIsOn = GbVar.Seq.sUnitDry.Info.IsStrip();
            if (bIsOn)
            {
                if (GbFunc.IsScrapNeed(STRIP_MDL.UNIT_DRY))
                {
                    if (a1pDryBlock.GradientEndColor != clScrapNeed)
                    {
                        a1pDryBlock.GradientEndColor = clScrapNeed;
                        a1pDryBlock.GradientStartColor = clScrapNeed;
                    }
                }
                else
                {
                    if (a1pDryBlock.GradientEndColor != Color.LawnGreen)
                    {
                        a1pDryBlock.GradientEndColor = Color.LawnGreen;
                        a1pDryBlock.GradientStartColor = Color.LawnGreen;
                    }
                }
            }
            else if (!bIsOn && a1pDryBlock.GradientEndColor != SystemColors.Control)
            {
                a1pDryBlock.GradientEndColor = SystemColors.Control;
                a1pDryBlock.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sMapTransfer.Info.IsStrip();
            if (bIsOn)
            {
                if (GbFunc.IsScrapNeed(STRIP_MDL.MAP_TRANSFER))
                {
                    if (a1pMapPk.GradientEndColor != clScrapNeed)
                    {
                        a1pMapPk.GradientEndColor = clScrapNeed;
                        a1pMapPk.GradientStartColor = clScrapNeed;
                    }
                }
                else
                {
                    if (a1pMapPk.GradientEndColor != Color.LawnGreen)
                    {
                        a1pMapPk.GradientEndColor = Color.LawnGreen;
                        a1pMapPk.GradientStartColor = Color.LawnGreen;
                    }
                }
            }
            else if (!bIsOn && a1pMapPk.GradientEndColor != SystemColors.Control)
            {
                a1pMapPk.GradientEndColor = SystemColors.Control;
                a1pMapPk.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sMapVisionTable[0].Info.IsStrip();
            if (bIsOn)
            {
                if (GbFunc.IsScrapNeed(STRIP_MDL.MAP_VISION_TABLE_1))
                {
                    if (a1pMapTable1.GradientEndColor != clScrapNeed)
                    {
                        a1pMapTable1.GradientEndColor = clScrapNeed;
                        a1pMapTable1.GradientStartColor = clScrapNeed;
                    }
                }
                else
                {
                    if (a1pMapTable1.GradientEndColor != Color.LawnGreen)
                    {
                        a1pMapTable1.GradientEndColor = Color.LawnGreen;
                        a1pMapTable1.GradientStartColor = Color.LawnGreen;
                    }
                }
            }
            else if (!bIsOn && a1pMapTable1.GradientEndColor != SystemColors.Control)
            {
                a1pMapTable1.GradientEndColor = SystemColors.Control;
                a1pMapTable1.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sMapVisionTable[1].Info.IsStrip();
            if (bIsOn)
            {
                if (GbFunc.IsScrapNeed(STRIP_MDL.MAP_VISION_TABLE_2))
                {
                    if (a1pMapTable2.GradientEndColor != clScrapNeed)
                    {
                        a1pMapTable2.GradientEndColor = clScrapNeed;
                        a1pMapTable2.GradientStartColor = clScrapNeed;
                    }
                }
                else
                {
                    if (a1pMapTable2.GradientEndColor != Color.LawnGreen)
                    {
                        a1pMapTable2.GradientEndColor = Color.LawnGreen;
                        a1pMapTable2.GradientStartColor = Color.LawnGreen;
                    }
                }
            }
            else if (!bIsOn && a1pMapTable2.GradientEndColor != SystemColors.Control)
            {
                a1pMapTable2.GradientEndColor = SystemColors.Control;
                a1pMapTable2.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sCuttingTable.Info.IsStrip();
            if (bIsOn)
            {
                if (GbFunc.IsScrapNeed(STRIP_MDL.CUTTING_TABLE))
                {
                    if (a1pChuckT1.GradientEndColor != clScrapNeed)
                    {
                        a1pChuckT1.GradientEndColor = clScrapNeed;
                        a1pChuckT1.GradientStartColor = clScrapNeed;
                    }
                }
                else
                {
                    if (a1pChuckT1.GradientEndColor != Color.LawnGreen)
                    {
                        a1pChuckT1.GradientEndColor = Color.LawnGreen;
                        a1pChuckT1.GradientStartColor = Color.LawnGreen;
                    }
                }                
            }
            else if (!bIsOn && a1pChuckT1.GradientEndColor != SystemColors.Control)
            {
                a1pChuckT1.GradientEndColor = SystemColors.Control;
                a1pChuckT1.GradientStartColor = SystemColors.Control;
            }
            #endregion
        }

        STRIP_MDL GetModuleFromTag(int nTag)
        {
            STRIP_MDL eMdl = STRIP_MDL.NONE;
            switch (nTag)
            {
                // 스트립 레일
                case 0:
                    eMdl = STRIP_MDL.STRIP_RAIL;
                    break;
                // 스트립 피커
                case 1:
                    eMdl = STRIP_MDL.STRIP_TRANSFER;
                    break;
                // 커팅 테이블
                case 2:
                    eMdl = STRIP_MDL.CUTTING_TABLE;
                    break;
                // 유닛 피커
                case 4:
                    eMdl = STRIP_MDL.UNIT_TRANSFER;
                    break;
                case 8:
                    eMdl = STRIP_MDL.UNIT_DRY;
                    break;
                // 맵 피커
                case 9:
                    eMdl = STRIP_MDL.MAP_TRANSFER;
                    break;
                // 맵 테이블 1
                case 10:
                    eMdl = STRIP_MDL.MAP_VISION_TABLE_1;
                    break;
                // 맵 테이블 2
                case 11:
                    eMdl = STRIP_MDL.MAP_VISION_TABLE_2;
                    break;
                default:
                    break;
            }

            return eMdl;
        }
    }
}
