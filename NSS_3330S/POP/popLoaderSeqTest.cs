using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.POP
{
    public partial class popLoaderSeqTest : Form
    {
        public popLoaderSeqTest()
        {
            InitializeComponent();
        }


        // -------------------------------------------------
        // 디버깅 테스트 목적으로 만듬, 확인 후 삭제 할 것
        // -------------------------------------------------


        private void button6_Click(object sender, EventArgs e)
        {
            // LD CONV 1 START
            GbSeq.autoRun.ResetCmd();
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_LD_CONV] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_LD_CONV] = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // LD CONV 1 STOP
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_LD_CONV] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_LD_CONV] = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // LD CONV 2 START
        }
        private void button3_Click(object sender, EventArgs e)
        {
            // LD CONV 2 STOP
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // ULD CONV 1 START
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // ULD CONV 1 STOP
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // ULD CONV 2 START
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // ULD CONV 2 STOP
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // MZ TRANSFER 
            GbSeq.autoRun.ResetCmd();

            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_ELV_TRANSFER] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_ELV_TRANSFER] = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // MZ TRANSFER STOP

            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_ELV_TRANSFER] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_ELV_TRANSFER] = false;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            // STRIP TRANSFER START
            GbSeq.autoRun.ResetCmd();
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.STRIP_TRANSFER] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.STRIP_TRANSFER] = true;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            // STRIP TRANSFER STOP
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.STRIP_TRANSFER] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.STRIP_TRANSFER] = false;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            // LOADER ALL START

            GbSeq.autoRun.ResetCmd();
            for (int i = 0; i <= (int)SEQ_ID.LD_MZ_ELV_TRANSFER; i++)
            {
                GbVar.mcState.isCycleRunReq[(int)i] = true;
                GbVar.mcState.isCycleRun[(int)i] = true;
            }

        }

        private void button11_Click(object sender, EventArgs e)
        {
            // LOADER ALL STOP

            for (int i = 0; i <= (int)SEQ_ID.LD_MZ_ELV_TRANSFER; i++)
            {
                GbVar.mcState.isCycleRunReq[(int)i] = false;
                GbVar.mcState.isCycleRun[(int)i] = false;
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            // Lot Start Ready On

            GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = true;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            // spare 2 
            GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = false;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            // spare 3
            GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] = true;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            // spare 4
            GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] = false;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            GbSeq.autoRun.ResetCmd();
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.UNIT_TRANSFER] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.UNIT_TRANSFER] = true;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.UNIT_TRANSFER] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.UNIT_TRANSFER] = false;
        }

        private void button21_Click(object sender, EventArgs e)
        {
            for (int nCnt = 0; nCnt < GbVar.mcState.isCycleRunReq.Length; nCnt++)
            {
                GbVar.mcState.isCycleRunReq[nCnt] = false;
                GbVar.mcState.isCycleRun[nCnt] = false;
            }

            MotionMgr.Inst.AllStop();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            Button[] btnSeqArray = { button6, button8, button1, button14, button19 };
            SEQ_ID[] seqIdArray = { SEQ_ID.LD_MZ_LD_CONV,
                                    SEQ_ID.LD_MZ_ELV_TRANSFER, SEQ_ID.STRIP_TRANSFER, SEQ_ID.UNIT_TRANSFER,};

            for (int nCnt = 0; nCnt < seqIdArray.Length; nCnt++)
            {
                if (GbVar.mcState.IsCycleRun(seqIdArray[nCnt]))
                {
                    if (btnSeqArray[nCnt].BackColor != Color.Cyan)
                        btnSeqArray[nCnt].BackColor = Color.Cyan;
                }
                else if (GbVar.mcState.IsCycleRunReq(seqIdArray[nCnt]))
                {
                    if (btnSeqArray[nCnt].BackColor != Color.Tan)
                        btnSeqArray[nCnt].BackColor = Color.Tan;
                }
                else
                {
                    if (btnSeqArray[nCnt].BackColor != SystemColors.Control)
                        btnSeqArray[nCnt].BackColor = SystemColors.Control;
                }
            }
        }

        private void popLoaderSeqTest_Load(object sender, EventArgs e)
        {
            tmrRefresh.Start();
        }

        private void popLoaderSeqTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrRefresh.Stop();
        }


        private void button29_Click(object sender, EventArgs e)
        {
            GbSeq.autoRun.ResetCmd();
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MAP_TRANSFER] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.MAP_TRANSFER] = true;
        }

        private void button28_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MAP_TRANSFER] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.MAP_TRANSFER] = false;
        }

        private void button31_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MAP_VISION_TABLE_1] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.MAP_VISION_TABLE_1] = true;
        }

        private void button30_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MAP_VISION_TABLE_1] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.MAP_VISION_TABLE_1] = false;
        }

        private void button33_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MAP_VISION_TABLE_2] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.MAP_VISION_TABLE_2] = true;
        }

        private void button32_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MAP_VISION_TABLE_2] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.MAP_VISION_TABLE_2] = false;
        }

        private void button35_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.PICK_N_PLACE_1] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.PICK_N_PLACE_1] = true;
        }

        private void button34_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.PICK_N_PLACE_1] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.PICK_N_PLACE_1] = false;
        }

        private void button37_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.PICK_N_PLACE_2] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.PICK_N_PLACE_2] = true;
        }

        private void button36_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.PICK_N_PLACE_2] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.PICK_N_PLACE_2] = false;
        }

        private void button39_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE] = true;
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE] = true;
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.UNLOAD_RW_TRAY_TABLE] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.UNLOAD_RW_TRAY_TABLE] = true;
        }

        private void button38_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE] = false;
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE] = false;
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.UNLOAD_RW_TRAY_TABLE] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.UNLOAD_RW_TRAY_TABLE] = false;
        }

        private void button41_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.TRAY_TRANSFER] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.TRAY_TRANSFER] = true;
        }

        private void button40_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.TRAY_TRANSFER] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.TRAY_TRANSFER] = false;
        }

        private void button43_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.ULD_ELV_GOOD_1] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.ULD_ELV_GOOD_1] = true;

            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.ULD_ELV_EMPTY_1] = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.ULD_ELV_EMPTY_1] = true;
        }

        private void button42_Click(object sender, EventArgs e)
        {
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.ULD_ELV_GOOD_1] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.ULD_ELV_GOOD_1] = false;

            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.ULD_ELV_EMPTY_1] = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.ULD_ELV_EMPTY_1] = false;
        }
    }
}
