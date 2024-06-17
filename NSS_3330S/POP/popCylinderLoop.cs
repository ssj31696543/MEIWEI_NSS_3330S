using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Threading;
using NSS_3330S.MOTION;

namespace NSS_3330S.POP
{
    public partial class popCylinderLoop : Form
    {
        private static bool Cyl_Switch = true;
        private static bool Btn_Switch = true;
        private static Thread Th_Cylmove = null;
        private static int Wait_Time = 1000;

        public popCylinderLoop()
        {
            InitializeComponent();
        }

        private void BtnCylOnOff_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;
            string str_msg;
            bool cycle_sw;
            if(popCylinderLoop.Btn_Switch)
            {
                str_msg = string.Format("동작 하시겠습니까?\n ");
                popCylinderLoop.Cyl_Switch = true;
                popCylinderLoop.Btn_Switch = false;
                cycle_sw = true;
            }
            else
            {
                str_msg = string.Format("중지 하시겠습니까?\n ");
                popCylinderLoop.Cyl_Switch = false;
                popCylinderLoop.Btn_Switch = true;
                cycle_sw = false;
            }
            string str_title = "주의";
            popMessageBox messageBox = new popMessageBox(str_msg, str_title);
            if (messageBox.ShowDialog() == DialogResult.Cancel)
            {
                if (cycle_sw)
                {
                    popCylinderLoop.Cyl_Switch = false;
                    popCylinderLoop.Btn_Switch = true;
                }
                else
                {
                    popCylinderLoop.Cyl_Switch = true;
                    popCylinderLoop.Btn_Switch = false;
                }
                return;
            }
            
            ManualCylmove(nTag);

        }
        
        public static void ManualCylmove(int nTag)
        {
            if(popCylinderLoop.Th_Cylmove == null)
            {
                popCylinderLoop.Th_Cylmove = new Thread(() => popCylinderLoop.th_Cylmove(nTag));
            }
            if (popCylinderLoop.Cyl_Switch)
            {
                Th_Cylmove.Start();
            }
            else
            {
                Th_Cylmove.Join(5000);

                Th_Cylmove.Abort();

                popCylinderLoop.th_Cylmove(nTag);

                Th_Cylmove = null;

            }
        }

        public static void th_Cylmove(int nTag)
        {
            bool loop_sw = true;
            switch (nTag)
            {
                case 0:
                    {  
                        while(popCylinderLoop.Cyl_Switch)
                        {
                            if(loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.MZ_DOOR_CLOSE_SOL, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.MZ_DOOR_CLOSE_SOL, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MZ_DOOR_CLOSE_SOL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL, true);
                        break;
                    }
                case 1:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_MZ_UNCLAMP_SOL, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.MZ_DOOR_OPEN_SOL, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_MZ_UNCLAMP_SOL, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.MZ_DOOR_OPEN_SOL, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_MZ_UNCLAMP_SOL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MZ_DOOR_OPEN_SOL, true);
                        break ;
                    }
                case 2  :
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.BTM_VISION_ALIGN_FWD, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.BTM_VISION_ALIGN_BWD, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.BTM_VISION_ALIGN_FWD, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.BTM_VISION_ALIGN_BWD, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.BTM_VISION_ALIGN_FWD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.BTM_VISION_ALIGN_BWD, true);
                        break;
                    }
                case 3:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.IN_LET_TABLE_UP, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.IN_LET_TABLE_DOWN, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.IN_LET_TABLE_UP, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.IN_LET_TABLE_DOWN, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.IN_LET_TABLE_UP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.IN_LET_TABLE_DOWN, true);
                        break;
                    }
                case 4:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.LD_X_GRIP, true);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.LD_X_GRIP, false);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.LD_X_GRIP, false);
                        break;
                    }
                case 5:
                    {
                        break;
                    }
                case 6:
                    {
                        break;
                    }
                case 7:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.TRAY_PK_ATC_CLAMP, true);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.TRAY_PK_ATC_CLAMP, false);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.TRAY_PK_ATC_CLAMP, false);
                        break;
                    }
                case 8:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_GRIP, true);
                                loop_sw = false;
                                
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_GRIP, false);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_GRIP, false);
                        break;
                    }
                case 9:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_UP, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_DOWN, false);
                                loop_sw = false;
                                
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_UP, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_DOWN, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_UP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_DOWN, true);
                        break;
                    }
                case 10:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_GRIP, true);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_GRIP, false);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_GRIP, false);
                        break;
                    }
                case 11:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_UP, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_DOWN, false);
                                loop_sw = false;
                                
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_UP, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_DOWN, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_UP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_DOWN, true);
                        break;
                    }
                case 12:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_TRAY_STG_GRIP, true);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_TRAY_STG_GRIP, false);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_TRAY_STG_GRIP, false);
                        break;
                    }
                case 13:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT. GD_1_ELV_TRAY_STACKER_UP, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT. GD_1_ELV_TRAY_STACKER_UP, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_UP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN, true);
                        break;
                    }
                case 14:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_STOPPER_FWD, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_STOPPER_BWD, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_STOPPER_FWD, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_STOPPER_BWD, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_STOPPER_FWD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_1_ELV_STOPPER_BWD, true);
                        break;
                    }
                case 15:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN, true);
                        break;

                    }
                case 16:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_STOPPER_FWD, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_STOPPER_BWD, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_STOPPER_FWD, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_STOPPER_BWD, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_STOPPER_FWD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_2_ELV_STOPPER_BWD, true);
                        break;
                    }
                case 17:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN, true);
                        break;
                    }
                case 18:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_STOPPER_FWD, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_STOPPER_BWD, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_STOPPER_FWD, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_STOPPER_BWD, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_STOPPER_FWD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_ELV_STOPPER_BWD, true);
                        break;
                    }
                case 19:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN, true);
                        break;

                    }
                case 20:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD, true);
                        break;
                    }
                case 21:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN, true);
                        break;
                    }
                case 22:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD, true);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD, false);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD, false);
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD, true);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD, true);
                        break;
                    }
                case 23:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP, true);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP, false);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP, false);
                        break;
                    }
                case 24:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP, true);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP, false);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP, false);
                        break;
                    }
                case 25:
                    {
                        while (popCylinderLoop.Cyl_Switch)
                        {
                            if (loop_sw)
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP, true);
                                loop_sw = false;
                            }
                            else
                            {
                                MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP, false);
                                loop_sw = true;
                            }

                            Thread.Sleep(popCylinderLoop.Wait_Time);
                        }
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP, false);
                        break;
                    }
                case 27:
                    {
                        break;
                    }
                case 28:
                    {
                        break;
                    }
                case 29:
                    {
                        break;
                    }
                case 30:
                    {
                        break;
                    }
                case 31:
                    { 
                        break;
                    }
                case 32:
                    {
                        break;
                    }
                case 33:
                    {
                        break;
                    }
                default:
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(popCylinderLoop.Th_Cylmove != null)
            {
                popMessageBox messageBox = new popMessageBox("현재 반복동작 중 입니다.", "경고");
                messageBox.ShowDialog();
            }
            else
            {
                NSS_3330S.FORM.frmDio.CylinderLoop_SW = true;
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(popCylinderLoop.Th_Cylmove != null)
            {
                popMessageBox messageBox = new popMessageBox("현재 반복동작 중 입니다.", "경고");
                messageBox.ShowDialog();
                return;
            }
            popCylinderLoop.Wait_Time = 1000;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (popCylinderLoop.Th_Cylmove != null)
            {
                popMessageBox messageBox = new popMessageBox("현재 반복동작 중 입니다.", "경고");
                messageBox.ShowDialog();
                return;
            }
            popCylinderLoop.Wait_Time = 2000;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (popCylinderLoop.Th_Cylmove != null)
            {
                popMessageBox messageBox = new popMessageBox("현재 반복동작 중 입니다.", "경고");
                messageBox.ShowDialog();
                return;
            }
            popCylinderLoop.Wait_Time = 3000;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (popCylinderLoop.Th_Cylmove != null)
            {
                popMessageBox messageBox = new popMessageBox("현재 반복동작 중 입니다.", "경고");
                messageBox.ShowDialog();
                return;
            }
            popCylinderLoop.Wait_Time = 5000;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (popCylinderLoop.Th_Cylmove != null)
            {
                popMessageBox messageBox = new popMessageBox("현재 반복동작 중 입니다.", "경고");
                messageBox.ShowDialog();
                return;
            }
            popCylinderLoop.Wait_Time = 10000;
        }
    }

}

