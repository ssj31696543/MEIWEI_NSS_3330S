using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinkMessage;

namespace NSS_3330S.POP
{
    public partial class popTest : Form
    {
        ProductLotInfo info = new ProductLotInfo();

        public delegate void LotStart(string strLotId);
        public event LotStart OnLotStart;

        public delegate void LotEnd(string strLotId);
        public event LotEnd OnLotEnd;

        public popTest()
        {
            InitializeComponent();
        }

        private void popTest_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        { 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            popErrorMsg pop = new popErrorMsg();
            pop.SetErrorText((int)numericUpDown1.Value);
            pop.TopMost = true;
            pop.StartPosition = FormStartPosition.CenterScreen;
            pop.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)
        {
        }

        private void button8_Click(object sender, EventArgs e)
        {
        }

        private void button9_Click(object sender, EventArgs e)
        {
        }

        private void button16_Click(object sender, EventArgs e)
        {
        }

        private void button10_Click(object sender, EventArgs e)
        {

        }
        private void button12_Click(object sender, EventArgs e)
        {
        }

        private void button14_Click(object sender, EventArgs e)
        {
        }

        private void button11_Click(object sender, EventArgs e)
        {
        }

        private void button13_Click(object sender, EventArgs e)
        {
        }

        private void button15_Click(object sender, EventArgs e)
        {
        }

        private void button17_Click(object sender, EventArgs e)
        {
        }

        private void button18_Click(object sender, EventArgs e)
        {
        }

        private void RccvMatRestore(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("RccvMatRestore|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("RccvMatRestore|{0},{1}", strFlag, ErrMsg));
            }
        }

        private void RecvMatInfo(string strFlag, string GrpCode, List<string> MatId, List<string> MatQty, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("RecvMatInfo|{0},{1},{2}", strFlag, GrpCode, ErrMsg));

                    for (int i = 0; i < MatId.Count; i++)
			        {
                        listBox1.Items.Add(string.Format("{0}|{1}", i, MatId[i]));
			        }
                    for (int i = 0; i < MatId.Count; i++)
                    {
                        listBox1.Items.Add(string.Format("{0}|{1}", i, MatQty[i]));
                    }
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("RecvMatInfo|{0},{1},{2}", strFlag, GrpCode, ErrMsg));

                for (int i = 0; i < MatId.Count; i++)
                {
                    listBox1.Items.Add(string.Format("{0}|{1}", i, MatId[i]));
                }
                for (int i = 0; i < MatId.Count; i++)
                {
                    listBox1.Items.Add(string.Format("{0}|{1}", i, MatQty[i]));
                }
            }
        }

        private void MAT_ASSIGN_CONFIRM(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("MAT_ASSIGN_CONFIRM|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("MAT_ASSIGN_CONFIRM|{0},{1}", strFlag, ErrMsg));
            }
        }
        private void MAT_STOCK_CONFIRM(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("MAT_STOCK_CONFIRM|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("MAT_STOCK_CONFIRM|{0},{1}", strFlag, ErrMsg));
            }
        }
        private void AMC_START_CONFIRM(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("AMC_START_CONFIRM|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("AMC_START_CONFIRM|{0},{1}", strFlag, ErrMsg));
            }
        }
        private void AMC_END_CONFIRM(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("AMC_END_CONFIRM|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("AMC_END_CONFIRM|{0},{1}", strFlag, ErrMsg));
            }
        }
        private void KIT_RESTORE_CONFIRM(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("KIT_RESTORE_CONFIRM|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("KIT_RESTORE_CONFIRM|{0},{1}", strFlag, ErrMsg));
            }
        }
        private void KIT_ASSIGN_CONFIRM(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("KIT_ASSIGN_CONFIRM|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("KIT_ASSIGN_CONFIRM|{0},{1}", strFlag, ErrMsg));
            }
        }
        private void KIT_STOCK_CHANGE_CONFIRM(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("KIT_STOCK_CHANGE_CONFIRM|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("KIT_STOCK_CHANGE_CONFIRM|{0},{1}", strFlag, ErrMsg));
            }
        }
        private void ATC_START_CONFIRM(string strFlag, string strLotId, string strRcpName, string strKitNo, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("ATC_START_CONFIRM|{0},{1},{2},{3},{4}", strFlag, strLotId, strRcpName, strKitNo, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("ATC_START_CONFIRM|{0},{1},{2},{3},{4}", strFlag, strLotId, strRcpName, strKitNo, ErrMsg));
            }
        }
        private void ATC_END_CONFIRM(string strFlag, string ErrMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    listBox1.Items.Add(string.Format("ATC_END_CONFIRM|{0},{1}", strFlag, ErrMsg));
                });
            }
            else
            {
                listBox1.Items.Add(string.Format("ATC_END_CONFIRM|{0},{1}", strFlag, ErrMsg));
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
        }

        private void button20_Click(object sender, EventArgs e)
        {
        }

        private void button21_Click(object sender, EventArgs e)
        {
        }

        private void button22_Click(object sender, EventArgs e)
        {
        }

        private void button23_Click(object sender, EventArgs e)
        {
        }

        private void button24_Click(object sender, EventArgs e)
        {
        }

        private void button25_Click(object sender, EventArgs e)
        {
        }

        private void button31_Click(object sender, EventArgs e)
        {
        }

        private void button28_Click(object sender, EventArgs e)
        {
        }

        private void button26_Click(object sender, EventArgs e)
        {
        }

        private void button29_Click(object sender, EventArgs e)
        {
        }

        private void button30_Click(object sender, EventArgs e)
        {
        }

        private void button27_Click(object sender, EventArgs e)
        {
        }

        private void button50_Click(object sender, EventArgs e)
        {
        }

        private void button32_Click(object sender, EventArgs e)
        {
        }

        private void button33_Click(object sender, EventArgs e)
        {
        }

        private void button34_Click(object sender, EventArgs e)
        {
        }

        private void button35_Click(object sender, EventArgs e)
        {
        }

        private void button36_Click(object sender, EventArgs e)
        {
        }

        private void button38_Click(object sender, EventArgs e)
        {

        }


        private void button92_Click(object sender, EventArgs e)
        {
        }

        private void button81_Click(object sender, EventArgs e)
        {
            ////Lot 등록할때 부여됨
            //GbVar.Seq.sLdMzLoading.LOT_ID = textBox10.Text;

            //바코드 읽을 때 부여됨
            GbVar.Seq.sStripRail.Info.STRIP_ID = textBox1.Text;

            GbVar.Seq.sStripTransfer.DataShiftMgzToLoadingRail();
        }

        private void button39_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sStripTransfer.DataShiftLoadingRailToTranser();
        }

        private void button40_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sStripTransfer.DataShiftTranserToCuttingTable();
        }

        private void button41_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sUnitTransfer.DataShiftCutTableToTranser();
        }

        private void button42_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sUnitTransfer.DataShiftTransferToDryStage();
        }

        private void button45_Click(object sender, EventArgs e)
        {
            //clean flip 1 to filp 2
            //GbVar.Seq.sCleaning[0].DataShiftCleanFlip1ToCleanFlip2();
        }

        private void button44_Click(object sender, EventArgs e)
        {
            //clean flip 2 to clean pk
            //GbVar.Seq.sCleaning[1].DataShiftCleanFlip2ToTransfer();
        }
        private void button91_Click(object sender, EventArgs e)
        {
            //clean pk to dry stg
            //GbVar.Seq.sCleaning[2].DataShiftTransferToDryStage();
        }

        private void button43_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sMapTransfer.DataShiftDryTableToTransfer();
        }

        private void button82_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sMapTransfer.DataShiftTransferToMapStage((int)numericUpDown3.Value);

            for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY; nRow++)
            {
                for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX; nCol++)
                {
                    GbVar.Seq.sMapVisionTable[(int)numericUpDown3.Value].Info.UnitArr[nRow][nCol].BTM_INSP_RESULT = (int)VSDF.eJUDGE_MAP.OK;
                    GbVar.Seq.sMapVisionTable[(int)numericUpDown3.Value].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.OK;
                }
            }
        }


        private void button46_Click(object sender, EventArgs e)
        {
        }

        private void button47_Click(object sender, EventArgs e)
        {
            button47.Text = "LOT INFO CLEAR";
            GbVar.lstBinding_ProdLot.Clear();
            GbVar.lstBinding_HostLot.Clear();
            GbVar.lstBinding_EqpProc.Clear();

            string S = "";
            for (int nRow = 29; nRow > -1; nRow--)
            {
                S += nRow.ToString().PadLeft(2, '0');
            }
        }

        private void button48_Click(object sender, EventArgs e)
        {
        }

        private void button49_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sStripRail.Info.STRIP_ID = textBox1.Text;

            GbVar.Seq.sStripRail.SetProcCycleInTime((int)MCC.LOAD_RAIL);
            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sStripRail.SetProcCycleOutTime((int)MCC.LOAD_RAIL);
            GbVar.Seq.sStripTransfer.DataShiftLoadingRailToTranser();

            System.Threading.Thread.Sleep(50); 
            GbVar.Seq.sStripTransfer.SetProcCycleInTime((int)MCC.STRIP_PICKER);
            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sStripTransfer.SetProcCycleOutTime((int)MCC.STRIP_PICKER);
            GbVar.Seq.sStripTransfer.DataShiftTranserToCuttingTable();

            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sCuttingTable.SetProcCycleInTime((int)MCC.TURN_TABLE);
            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sCuttingTable.SetProcCycleOutTime((int)MCC.TURN_TABLE);
            GbVar.Seq.sUnitTransfer.DataShiftCutTableToTranser();

            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sUnitTransfer.SetProcCycleInTime((int)MCC.UNIT_PICKER);
            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sUnitTransfer.SetProcCycleOutTime((int)MCC.UNIT_PICKER);
            GbVar.Seq.sUnitTransfer.DataShiftTransferToDryStage();

            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sUnitDry.SetProcCycleInTime((int)MCC.DRY_BLOCK);
            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sUnitDry.SetProcCycleOutTime((int)MCC.DRY_BLOCK);
            GbVar.Seq.sMapTransfer.DataShiftDryTableToTransfer();

            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sMapTransfer.SetProcCycleInTime((int)MCC.MAP_PICKER);
            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sMapTransfer.SetProcCycleOutTime((int)MCC.MAP_PICKER);
            GbVar.Seq.sMapTransfer.DataShiftTransferToMapStage(0);


            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sMapVisionTable[0].SetProcCycleInTime((int)MCC.VISION_TABLE1);
            System.Threading.Thread.Sleep(50);
            GbVar.Seq.sMapVisionTable[0].SetProcCycleOutTime((int)MCC.VISION_TABLE1);

        }

        private void button52_Click(object sender, EventArgs e)
        {
        }

        private void button51_Click(object sender, EventArgs e)
        {
        }

        private void button53_Click(object sender, EventArgs e)
        {
        }

        private void button37_Click(object sender, EventArgs e)
        {
        }

        private void button54_Click(object sender, EventArgs e)
        {

        }

        private void button55_Click(object sender, EventArgs e)
        {
        }

        private void button56_Click(object sender, EventArgs e)
        {
        }

        private void button57_Click(object sender, EventArgs e)
        {
        }

        private void button58_Click(object sender, EventArgs e)
        {
        }

        private void button59_Click(object sender, EventArgs e)
        {
            IFMgr.Inst.SAW.RequestWorkingData(0);
        }

        private void button62_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;

            if (nTag == 0)
            {
                IFMgr.Inst.VISION.ifRcpChange.PPID = textBox5.Text;
                IFMgr.Inst.VISION.ifRcpChange.SEQSTART = true;
            }
            else
            {
                IFMgr.Inst.SAW.ifRcpChange.PPID = textBox6.Text;
                IFMgr.Inst.SAW.ifRcpChange.SEQSTART = true;
            }
        }

        private void button60_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;

            if (nTag == 0)
            {
                IFMgr.Inst.VISION.ifHostRcpParaChange.PPID = textBox5.Text;
                IFMgr.Inst.VISION.ifHostRcpParaChange.SEQSTART = true;
            }
            else
            {
                IFMgr.Inst.SAW.ifHostRcpParaChange.PPID = textBox6.Text;
                IFMgr.Inst.SAW.ifHostRcpParaChange.SEQSTART = true;
            }
        }

        private void button61_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;

            if (nTag == 0)
            {
                IFMgr.Inst.VISION.ifRcpCreate.PPID = textBox5.Text;
                IFMgr.Inst.VISION.ifRcpCreate.SEQSTART = true;
            }
            else
            {
                IFMgr.Inst.SAW.ifRcpCreate.PPID = textBox6.Text;
                IFMgr.Inst.SAW.ifRcpCreate.SEQSTART = true;

            }
        }

        private void button63_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;

            if (nTag == 0)
            {
                IFMgr.Inst.VISION.ifRcpDelete.PPID = textBox5.Text;
                IFMgr.Inst.VISION.ifRcpDelete.SEQSTART = true;
            }
            else
            {
                IFMgr.Inst.SAW.ifRcpDelete.PPID = textBox6.Text;
                IFMgr.Inst.SAW.ifRcpDelete.SEQSTART = true;
            }
        }
        private void button64_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;

            if (nTag == 0)
            {
                IFMgr.Inst.VISION.ifRcpUpload.PPID = textBox5.Text;
                IFMgr.Inst.VISION.ifRcpUpload.SEQSTART = true;
            }
            else
            {
                IFMgr.Inst.SAW.ifRcpUpload.PPID = textBox6.Text;
                IFMgr.Inst.SAW.ifRcpUpload.SEQSTART = true;
            }
        }

        private void button65_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;

            if (nTag == 0)
            {
                IFMgr.Inst.VISION.ifRcpSelected.SEQSTART = true;
            }
            else
            {
                IFMgr.Inst.SAW.ifRcpSelected.SEQSTART = true;
            }
        }

        private void button66_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;

            if (nTag == 0)
            {
                IFMgr.Inst.VISION.ifRcpList.SEQSTART = true;
            }
            else
            {
                IFMgr.Inst.SAW.ifRcpList.SEQSTART = true;
            }
        }

        private void button74_Click(object sender, EventArgs e)
        {
            IFMgr.Inst.SAW.ifBladeId.SEQSTART = true;
        }

        private void button75_Click(object sender, EventArgs e)
        {
        }

        private void button76_Click(object sender, EventArgs e)
        {
        }

        private void button77_Click(object sender, EventArgs e)
        {
        }

        private void button78_Click(object sender, EventArgs e)
        {
        }

        private void button79_Click(object sender, EventArgs e)
        {
            GbVar.SeqMgr.UploadSeq();
        }

        private void button80_Click(object sender, EventArgs e)
        {
            GbVar.SeqMgr.DownLoadSeq();
        }


        private void button83_Click(object sender, EventArgs e)
        {
            GbVar.StripMgr.DownloadAllMdlStripInfo();
        }

        private void button84_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sMapVisionTable[(int)numericUpDown8.Value].AddStripLog();
        }

        private void button85_Click(object sender, EventArgs e)
        {
            //GbVar.Seq.sMapVisionTable[0].Info.STRIP_ID = "Q0569200004B 014B";
            //GbVar.Seq.sMapVisionTable[0].Info.GetXOutInfo();

            //info.MOVE_IN_TIME = DateTime.Now;
            //info.LOT_ID = textBox12.Text;
            //info.CARRIER_ID = textBox13.Text;
            //GbVar.LotMgr.InsertLotLog(info);
            //GbVar.lstBinding_ProdLot.Add(info);

            //HostLotInfo hostinfo = new HostLotInfo();

            //GbVar.lstBinding_HostLot.Add(hostinfo);
            //GbVar.LotMgr.InsertLotInfo(hostinfo);

            //OnLotStart(textBox12.Text);
        }

        private void button86_Click(object sender, EventArgs e)
        {
            info.MOVE_OUT_TIME = DateTime.Now;
            info.LOT_ID = textBox12.Text;
            info.CARRIER_ID = textBox13.Text;

            TimeSpan ts = new TimeSpan();
            ts = info.MOVE_OUT_TIME - info.MOVE_IN_TIME;
            info.CYCLE_TIME = ts.ToString();

            info.EQP_NQ_CHIP_COUNT = 1;
            info.EQP_SQ_CHIP_COUNT = 2;
            info.EQP_RJ_CHIP_COUNT = "3";
            info.EQP_NQ_TRAY_COUNT = "4";
            info.EQP_SQ_TRAY_COUNT = "5";
            info.BALL_VISION_ERR_COUNT = "6";
            info.MARK_VISION_ERR_COUNT = "6";

            //GbVar.LotMgr.UpdateLotLog(info);

            if (GbVar.lstBinding_HostLot.Count > 0)
            {
                GbVar.LotMgr.DeleteLotInfo(GbVar.lstBinding_HostLot[0].LOT_ID);
                GbVar.lstBinding_HostLot.RemoveAt(0);
            }

            if (GbVar.lstBinding_ProdLot.Count > 0)
                GbVar.lstBinding_ProdLot.RemoveAt(0);

            OnLotEnd(textBox12.Text);
        }

        private void button87_Click(object sender, EventArgs e)
        {
        }

        private void button88_Click(object sender, EventArgs e)
        {
        }

        private void button89_Click(object sender, EventArgs e)
        { 
        }

        private void button90_Click(object sender, EventArgs e)
        {
        }

        private void button93_Click(object sender, EventArgs e)
        {

        }

        private void button94_Click(object sender, EventArgs e)
        {
        }
    }
}
