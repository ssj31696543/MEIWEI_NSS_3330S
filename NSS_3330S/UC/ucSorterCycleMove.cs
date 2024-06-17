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
using static NSS_3330S.MCDF;

namespace NSS_3330S.UC
{
    public partial class ucSorterCycleMove : UserControl
    {
        public int Map_stage_select = 0; // 맵 스테이지 선택
        public int Map_stage_select_T = 0; // 맵 스테이지 선택
        public int Chip_picker_or_pickerPad_select = 0; // 칩 피커, 칩 피커 패드선택
        public int Chip_picker_select = 0; // 칩 피커 선택
        public int Chip_picker_pad_select = 0; // 칩 피커 패드 선택
        public int Tray_picker_XorY_select = 0; // 트레이 피커 X,Y 선택
        public int Tray_stage_select = 0; // 트레이 스테이지 선택
        public int Tray_ELV_select = 0; // 트레이 엘리베이터 선택
        protected POP.popManualRun m_popManualRun = null;
        public ucSorterCycleMove()
        {
            InitializeComponent();
        }
        private void Btn_Ready_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int ntag = btn.GetTag();

            #region rd버튼선택
            if (rd_MSTG_Y1.Checked)
                Map_stage_select = 0;
            else
                Map_stage_select = 1;

            if (rd_MSTG_T1.Checked)
                Map_stage_select_T = 0;
            else
                Map_stage_select_T = 1;

            if (rd_picker.Checked)
            {
                Chip_picker_or_pickerPad_select = 0;

                if (rd_picker1.Checked)
                    Chip_picker_select = 0;
                else
                    Chip_picker_select = 1;
            }
            else
            {
                Chip_picker_or_pickerPad_select = 1;

                if (rd_picker1.Checked)
                    Chip_picker_select = 0;
                else
                    Chip_picker_select = 1;

                if (rd_picker_pad1.Checked)
                    Chip_picker_pad_select = 0;
                else if (rd_picker_pad2.Checked)
                    Chip_picker_pad_select = 1;
                else if (rd_picker_pad3.Checked)
                    Chip_picker_pad_select = 2;
                else if (rd_picker_pad4.Checked)
                    Chip_picker_pad_select = 3;
                else if (rd_picker_pad5.Checked)
                    Chip_picker_pad_select = 4;
                else
                    Chip_picker_pad_select = 5;
            }

            if (rd_traypicker_X.Checked)
                Tray_picker_XorY_select = 0;
            else
                Tray_picker_XorY_select = 1;

            if (rd_traySTG_1.Checked)
                Tray_stage_select = 0;
            else if (rd_traySTG_2.Checked)
                Tray_stage_select = 1;
            else
                Tray_stage_select = 2;

            if (rd_trayELV_1.Checked)
                Tray_ELV_select = 0;
            else if (rd_trayELV_2.Checked)
                Tray_ELV_select = 1;
            else if (rd_trayELV_3.Checked)
                Tray_ELV_select = 2;
            else if (rd_trayELV_4.Checked)
                Tray_ELV_select = 3;
            else
                Tray_ELV_select = 4;
            #endregion
            #region 준비동작 위치 이동
            if (ntag == 5) // 칩 피커 or 칩 피커 패드 이동
                Sorterseq(ntag + Chip_picker_or_pickerPad_select);
            else if (ntag == 9) // 트레이 피커 X or Y 이동
                Sorterseq(ntag + Tray_picker_XorY_select);
            else if (ntag == 12) // 트레이 엘리베이터 이동
                Sorterseq(ntag + Tray_ELV_select);
            else if (ntag == 17) // 트레이 스테이지 이동
            {
                if (Tray_stage_select == 2)
                    Sorterseq(ntag +Tray_stage_select); // 리워크 트레이 스테이지
                else
                    Sorterseq(ntag); //굿트레이 스테이지
            }
            #endregion
            else
                Sorterseq(ntag);
        }
        public void Sorterseq(int move_seq)
        {
            int pos_no = 0;
            int axis_no = 0;
            int nAxisNoX;
            int nAxisNoY;
            switch (move_seq)
            {
                #region 준비위치
                case 0://  맵피커 X 준비 위치
                    pos_no = (int)POSDF.MAP_PICKER_READY;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 1://  맵피커 Z 준비 위치
                    pos_no = (int)POSDF.MAP_PICKER_READY;
                    axis_no = (int)SVDF.AXES.MAP_PK_Z;
                    break;
                case 2://  맵스테이지1,2 Y 준비 위치
                    pos_no = (int)POSDF.MAP_STAGE_READY;
                    if(Map_stage_select == 0)
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                    else
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                    break;
                case 3://  맵스테이지1,2 T 준비 위치
                    pos_no = (int)POSDF.MAP_STAGE_READY;
                    if (Map_stage_select_T == 0)
                        axis_no = (int)SVDF.AXES.MAP_STG_1_T;
                    else
                        axis_no = (int)SVDF.AXES.MAP_STG_2_T;
                    break;
                case 4://  맵 비전 Z 준비 위치
                    pos_no = (int)POSDF.MAP_VISION_READY;
                    axis_no = (int)SVDF.AXES.MAP_VISION_Z;
                    break;
                case 5://  칩 피커 X 준비 위치
                    pos_no = (int)POSDF.CHIP_PICKER_READY;
                    if (Chip_picker_select == 0)
                        axis_no = (int)SVDF.AXES.CHIP_PK_1_X;
                    else
                        axis_no = (int)SVDF.AXES.CHIP_PK_2_X;
                    break;
                case 6://  칩 피커 패드 준비 위치
                    pos_no = (int)POSDF.CHIP_PICKER_READY;
                    if (Chip_picker_select == 0)
                    {
                        switch (Chip_picker_pad_select)
                        {
                            case 0:
                                axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_1;
                                break;
                            case 1:
                                axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_2;
                                break;
                            case 2:
                                axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_3;
                                break;
                            case 3:
                                axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_4;
                                break;
                            case 4:
                                axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_5;
                                break;
                            case 5:
                                axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_6;
                                break;
                        }
                    }
                    else
                    {
                        switch (Chip_picker_pad_select)
                        {
                            case 0:
                                axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_1;
                                break;
                            case 1:
                                axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_2;
                                break;
                            case 2:
                                axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_3;
                                break;
                            case 3:
                                axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_4;
                                break;
                            case 4:
                                axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_5;
                                break;
                            case 5:
                                axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_6;
                                break;
                        }
                    }
                    break;
                case 7://  볼 비전 Y 준비 위치
                    pos_no = (int)POSDF.BALL_VISION_READY;
                    axis_no = (int)SVDF.AXES.BALL_VISION_Y;
                    break;
                case 8://  볼 비전 Z 준비 위치
                    pos_no = (int)POSDF.BALL_VISION_READY;
                    axis_no = (int)SVDF.AXES.BALL_VISION_Z;
                    break;
                case 9://  트레이 피커 X 준비 위치
                    pos_no = (int)POSDF.TRAY_PICKER_READY;
                    axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    break;
                case 10://  트레이 피커 Y 준비 위치
                    pos_no = (int)POSDF.TRAY_PICKER_READY;
                    axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 11://  트레이 피커 Z 준비 위치
                    pos_no = (int)POSDF.TRAY_PICKER_READY;
                    axis_no = (int)SVDF.AXES.TRAY_PK_Z;
                    break;
                case 12://  굿 트레이 엘리베이터1 Z 준비 위치
                    pos_no = (int)POSDF.TRAY_ELEV_READY;
                    axis_no = (int)SVDF.AXES.GD_TRAY_1_ELV_Z;
                    break;
                case 13://  굿 트레이 엘리베이터2 Z 준비 위치
                    pos_no = (int)POSDF.TRAY_ELEV_READY;
                    axis_no = (int)SVDF.AXES.GD_TRAY_2_ELV_Z;
                    break;
                case 14://  리워크 트레이 엘리베이터 Z 준비 위치
                    pos_no = (int)POSDF.TRAY_ELEV_READY;
                    axis_no = (int)SVDF.AXES.RW_TRAY_ELV_Z;
                    break;
                case 15://  엠티 트레이 엘리베이터 1 Z 준비 위치
                    pos_no = (int)POSDF.TRAY_ELEV_READY;
                    axis_no = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;
                    break;
                case 16://  엠티 트레이 엘리베이터 2 Z 준비 위치
                    pos_no = (int)POSDF.TRAY_ELEV_READY;
                    axis_no = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;
                    break;
                case 17://  굿 트레이 스테이지1,2 Y 준비 위치
                    pos_no = (int)POSDF.TRAY_STAGE_READY;
                    if (Tray_stage_select == 0)
                        axis_no = (int)SVDF.AXES.GD_TRAY_STG_1_Y;
                    else
                        axis_no = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
                    break;
                case 18://  햇갈림 방지용 empty case
                    break;
                case 19://  리워크 트레이 스테이지 Y 준비 위치
                    pos_no = (int)POSDF.TRAY_STAGE_READY;
                    axis_no = (int)SVDF.AXES.RW_TRAY_STG_Y;
                    break;
                #endregion
                #region 메뉴얼 동작 위치
                #region 맵 피커 X 동작
                case 20://  스트립 로드 위치
                    pos_no = (int)POSDF.MAP_PICKER_STRIP_LOADING;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 21://  비전 시작 T1 위치
                    pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_START_T1;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 22://  비전 종료 T1 위치
                    pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_END_T1;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 23://  비전 시작 T2 위치
                    pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_START_T2;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 24://  비전 종료 T2 위치
                    pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_END_T2;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 25://  스트립 언로딩  스테이지 1 위치
                    pos_no = (int)POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 26://  스트립 언로딩  스테이지 2 위치
                    pos_no = (int)POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_2;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 27://  맵 스테이지 얼라인 마크 1 위치
                    pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 28://  맵 스테이지 얼라인 마크 2 위치
                    pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_ALIGN_T2;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 29://  에어 드라이 시작 위치
                    pos_no = (int)POSDF.MAP_PICKER_AIR_DRY_START;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 30://  에어 드라이 종료 위치
                    pos_no = (int)POSDF.MAP_PICKER_AIR_DRY_END;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 31://   T1 좌측 탑 유닛 뷰 위치
                    pos_no = (int)POSDF.MAP_PICKER_T1_LEFT_TOP_UNIT_POS;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                case 32://   T2 좌측 탑 유닛 뷰 위치
                    pos_no = (int)POSDF.MAP_PICKER_T2_LEFT_TOP_UNIT_POS;
                    axis_no = (int)SVDF.AXES.MAP_PK_X;
                    break;
                
                #endregion
                #region 맵 피커 Z 동작
                case 33:
                    axis_no = (int)SVDF.AXES.MAP_PK_Z;
                    nAxisNoX = (int)SVDF.AXES.MAP_PK_X;
                    if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_STRIP_LOADING))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_STRIP_LOADING;
                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_MAP_VISION_START_T1))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_START_T1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_MAP_VISION_END_T1))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_END_T1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_MAP_VISION_START_T2))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_START_T2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_MAP_VISION_END_T2))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_END_T2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_2))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_MAP_VISION_ALIGN_T2))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_ALIGN_T2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_AIR_DRY_START))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_AIR_DRY_START;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_AIR_DRY_END))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_AIR_DRY_END;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_T1_LEFT_TOP_UNIT_POS))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_T1_LEFT_TOP_UNIT_POS;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_PICKER_T2_LEFT_TOP_UNIT_POS))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_T2_LEFT_TOP_UNIT_POS;

                    }
                    else
                    {
                        popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
                        if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                        pos_no = (int)POSDF.MAP_PICKER_READY;
                    }
                    break;
                #endregion
                #region 맵 스테이지 Y1,2
                case 34:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_STRIP_LOADING;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_STRIP_LOADING;
                    }
                    break;
                case 35:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_MAP_VISION_START;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_MAP_VISION_START;
                    }
                    break;
                case 36:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_MAP_VISION_END;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_MAP_VISION_END;
                    }
                    break;
                case 37:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P1;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P1;
                    }
                    break;
                case 38:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P2;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P2;
                    }
                    break;
                case 39:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_CLEAN_START;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_CLEAN_START;
                    }
                    break;
                case 40:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_CLEAN_END;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_CLEAN_END;
                    }
                    break;
                case 41:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK1;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK1;
                    }
                    break;
                case 42:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK2;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK2;
                    }
                    break;
                case 43:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_ROATE;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_ROATE;
                    }
                    break;
                case 44:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_PICKUP_MOVE;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_PICKUP_MOVE;
                    }
                    break;
                case 45:
                    if (Map_stage_select == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_Y;
                        pos_no = (int)POSDF.MAP_STAGE_LEFT_TOP_POS;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_Y;
                        pos_no = (int)POSDF.MAP_STAGE_LEFT_TOP_POS;
                    }
                    break;
                #endregion
                #region 맵 스테이지 T1,2
                case 46:

                    if (Map_stage_select_T == 0)
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_1_T;
                        nAxisNoX = (int)SVDF.AXES.MAP_STG_1_Y;
                    }
                    else
                    {
                        axis_no = (int)SVDF.AXES.MAP_STG_2_T;
                        nAxisNoX = (int)SVDF.AXES.MAP_STG_2_Y;
                    }

                    
                    if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_STRIP_LOADING))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_STRIP_LOADING;
                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_MAP_VISION_START))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_MAP_VISION_START;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_MAP_VISION_END))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_MAP_VISION_END;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P1))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P2))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_UNIT_CLEAN_START))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_CLEAN_START;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_UNIT_CLEAN_END))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_UNIT_CLEAN_END;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK1))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK2))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_ROATE))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_ROATE;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_PICKUP_MOVE))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_PICKUP_MOVE;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.MAP_STAGE_LEFT_TOP_POS))
                    {
                        pos_no = (int)POSDF.MAP_STAGE_LEFT_TOP_POS;

                    }
                    else
                    {
                        popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
                        if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                        pos_no = (int)POSDF.MAP_STAGE_READY;
                    }
                    break;
                #endregion
                #region 맵 비전 Z 동작
                case 47:
                    axis_no = (int)SVDF.AXES.MAP_VISION_Z;
                    nAxisNoX = (int)SVDF.AXES.MAP_PK_X;
                    if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, POSDF.MAP_PICKER_MAP_VISION_START_T1))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_START_T1;
                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, POSDF.MAP_PICKER_MAP_VISION_START_T2))
                    {
                        pos_no = (int)POSDF.MAP_PICKER_MAP_VISION_START_T2;

                    }
                    else
                    {
                        popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
                        if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                        pos_no = (int)POSDF.MAP_VISION_READY;
                    }
                    break;
                #endregion
                #region 칩 피커 동작
                case 48:
                    pos_no = (int)POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1;
                    if (Chip_picker_or_pickerPad_select == 0)// 피커 선택
                    {
                        if (Chip_picker_select == 0)
                            axis_no = (int)SVDF.AXES.CHIP_PK_1_X;
                        else
                            axis_no = (int)SVDF.AXES.CHIP_PK_2_X;
                    }
                    else //피커 패드 선택
                    {
                        if (Chip_picker_select == 0)
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_8;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_8;
                                    break;
                            }
                        }
                    }
                    break;
                case 49:
                    pos_no = (int)POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2;
                    if (Chip_picker_or_pickerPad_select == 0)// 피커 선택
                    {
                        if (Chip_picker_select == 0)
                            axis_no = (int)SVDF.AXES.CHIP_PK_1_X;
                        else
                            axis_no = (int)SVDF.AXES.CHIP_PK_2_X;
                    }
                    else //피커 패드 선택
                    {
                        if (Chip_picker_select == 0)
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_8;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_8;
                                    break;
                            }
                        }
                    }
                    break;
                case 50:
                    pos_no = (int)POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF;
                    if (Chip_picker_or_pickerPad_select == 0)// 피커 선택
                    {
                        if (Chip_picker_select == 0)
                            axis_no = (int)SVDF.AXES.CHIP_PK_1_X;
                        else
                            axis_no = (int)SVDF.AXES.CHIP_PK_2_X;
                    }
                    else //피커 패드 선택
                    {
                        if (Chip_picker_select == 0)
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_8;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_8;
                                    break;
                            }
                        }
                    }
                    break;
                case 51:
                    pos_no = (int)POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1;
                    if (Chip_picker_or_pickerPad_select == 0)// 피커 선택
                    {
                        if (Chip_picker_select == 0)
                            axis_no = (int)SVDF.AXES.CHIP_PK_1_X;
                        else
                            axis_no = (int)SVDF.AXES.CHIP_PK_2_X;
                    }
                    else //피커 패드 선택
                    {
                        if (Chip_picker_select == 0)
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_8;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_8;
                                    break;
                            }
                        }
                    }
                    break;
                case 52:
                    pos_no = (int)POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2;
                    if (Chip_picker_or_pickerPad_select == 0)// 피커 선택
                    {
                        if (Chip_picker_select == 0)
                            axis_no = (int)SVDF.AXES.CHIP_PK_1_X;
                        else
                            axis_no = (int)SVDF.AXES.CHIP_PK_2_X;
                    }
                    else //피커 패드 선택
                    {
                        if (Chip_picker_select == 0)
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_8;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_8;
                                    break;
                            }
                        }
                    }
                    break;
                case 53:
                    pos_no = (int)POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF;
                    if (Chip_picker_or_pickerPad_select == 0)// 피커 선택
                    {
                        if (Chip_picker_select == 0)
                            axis_no = (int)SVDF.AXES.CHIP_PK_1_X;
                        else
                            axis_no = (int)SVDF.AXES.CHIP_PK_2_X;
                    }
                    else //피커 패드 선택
                    {
                        if (Chip_picker_select == 0)
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_8;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_8;
                                    break;
                            }
                        }
                    }
                    break;
                case 54:
                    pos_no = (int)POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN;
                    if (Chip_picker_or_pickerPad_select == 0)// 피커 선택
                    {
                        if (Chip_picker_select == 0)
                            axis_no = (int)SVDF.AXES.CHIP_PK_1_X;
                        else
                            axis_no = (int)SVDF.AXES.CHIP_PK_2_X;
                    }
                    else //피커 패드 선택
                    {
                        if (Chip_picker_select == 0)
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_1_Z_8;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Chip_picker_pad_select)
                            {
                                case 0:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_1;
                                    break;
                                case 1:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_2;
                                    break;
                                case 2:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_3;
                                    break;
                                case 3:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_4;
                                    break;
                                case 4:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_5;
                                    break;
                                case 5:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_6;
                                    break;
                                case 6:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_7;
                                    break;
                                case 7:
                                    axis_no = (int)SVDF.AXES.CHIP_PK_2_Z_8;
                                    break;
                            }
                        }
                    }
                    break;
                #endregion
                #region 볼비전 Y 동작
                case 55:
                    pos_no = (int)POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1;
                    axis_no = (int)SVDF.AXES.BALL_VISION_Y;
                    break;
                case 56:
                    pos_no = (int)POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2;
                    axis_no = (int)SVDF.AXES.BALL_VISION_Y;
                    break;
                #endregion
                #region 볼비전 Z 동작
                case 57:
                    axis_no = (int)SVDF.AXES.BALL_VISION_Z;
                    nAxisNoX = (int)SVDF.AXES.BALL_VISION_Y;
                    if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1))
                    {
                        pos_no = (int)POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1;
                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2))
                    {
                        pos_no = (int)POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2;

                    }
                    else
                    {
                        popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
                        if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                        pos_no = (int)POSDF.BALL_VISION_READY;
                    }
                    break;
                #endregion
                #region 트레이 피커 X,Y 동작
                case 58:
                    pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 59:
                    pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_2;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 60:
                    pos_no = (int)POSDF.TRAY_PICKER_STAGE_GOOD_1;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 61:
                    pos_no = (int)POSDF.TRAY_PICKER_STAGE_GOOD_2;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 62:
                    pos_no = (int)POSDF.TRAY_PICKER_STAGE_REWORK;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 63:
                    pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 64:
                    pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_GOOD_2;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 65:
                    pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_REWORK;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 66:
                    pos_no = (int)POSDF.TRAY_PICKER_TRAY_GOOD_1_INSPECTION;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 67:
                    pos_no = (int)POSDF.TRAY_PICKER_TRAY_GOOD_2_INSPECTION;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                case 68:
                    pos_no = (int)POSDF.TRAY_PICKER_TRAY_REWORK_INSPECTION;
                    if (Tray_picker_XorY_select == 0)
                        axis_no = (int)SVDF.AXES.TRAY_PK_X;
                    else
                        axis_no = (int)SVDF.AXES.TRAY_PK_Y;
                    break;
                #endregion
                #region 트레이 피커 Z 동작
                case 69:
                    axis_no = (int)SVDF.AXES.TRAY_PK_Z;
                    nAxisNoX = (int)SVDF.AXES.TRAY_PK_X;
                    nAxisNoY = (int)SVDF.AXES.TRAY_PK_Y;
                    if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1)&&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1;
                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_2) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_2))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_STAGE_GOOD_1) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_STAGE_GOOD_1))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_STAGE_GOOD_1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_STAGE_GOOD_2) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_STAGE_GOOD_2))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_STAGE_GOOD_2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_STAGE_REWORK) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_STAGE_REWORK))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_STAGE_REWORK;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_TRAY_ELV_GOOD_2) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_TRAY_ELV_GOOD_2))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_GOOD_2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_TRAY_ELV_REWORK) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_TRAY_ELV_REWORK))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_TRAY_ELV_REWORK;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_TRAY_GOOD_1_INSPECTION) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_TRAY_GOOD_1_INSPECTION))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_TRAY_GOOD_1_INSPECTION;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_TRAY_GOOD_2_INSPECTION) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_TRAY_GOOD_2_INSPECTION))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_TRAY_GOOD_2_INSPECTION;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.TRAY_PICKER_TRAY_REWORK_INSPECTION) &&
                        SafetyMgr.Inst.IsInPosNo(nAxisNoY, (int)POSDF.TRAY_PICKER_TRAY_REWORK_INSPECTION))
                    {
                        pos_no = (int)POSDF.TRAY_PICKER_TRAY_REWORK_INSPECTION;

                    }
                    else
                    {
                        popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
                        if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                        pos_no = (int)POSDF.BALL_VISION_READY;
                    }
                    break;
                #endregion
                #region 엘리베이터 동작(굿1,굿2,리워크,엠티1,엠티2)
                case 70:
                    pos_no = (int)POSDF.TRAY_ELEV_TRAY_LOADING;
                    switch (Tray_ELV_select)
                    {
                        case 0:
                            axis_no = (int)SVDF.AXES.GD_TRAY_1_ELV_Z;
                            break;
                        case 1:
                            axis_no = (int)SVDF.AXES.GD_TRAY_2_ELV_Z;
                            break;
                        case 2:
                            axis_no = (int)SVDF.AXES.RW_TRAY_ELV_Z;
                            break;
                        case 3:
                            axis_no = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;
                            break;
                        case 4:
                            axis_no = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;
                            break;
                    }
                    break;
                case 71:
                    pos_no = (int)POSDF.TRAY_ELEV_TRAY_OHT_LIFT_IN;
                    switch (Tray_ELV_select)
                    {
                        case 0:
                            axis_no = (int)SVDF.AXES.GD_TRAY_1_ELV_Z;
                            break;
                        case 1:
                            axis_no = (int)SVDF.AXES.GD_TRAY_2_ELV_Z;
                            break;
                        case 2:
                            axis_no = (int)SVDF.AXES.RW_TRAY_ELV_Z;
                            break;
                        case 3:
                            axis_no = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;
                            break;
                        case 4:
                            axis_no = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;
                            break;
                    }
                    break;
                case 72:
                    pos_no = (int)POSDF.TRAY_ELEV_TRAY_OHT_LIFT_OUT;
                    switch (Tray_ELV_select)
                    {
                        case 0:
                            axis_no = (int)SVDF.AXES.GD_TRAY_1_ELV_Z;
                            break;
                        case 1:
                            axis_no = (int)SVDF.AXES.GD_TRAY_2_ELV_Z;
                            break;
                        case 2:
                            axis_no = (int)SVDF.AXES.RW_TRAY_ELV_Z;
                            break;
                        case 3:
                            axis_no = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;
                            break;
                        case 4:
                            axis_no = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;
                            break;
                    }
                    break;
                case 73:
                    pos_no = (int)POSDF.TRAY_ELEV_TRAY_UNLOADING;
                    switch (Tray_ELV_select)
                    {
                        case 0:
                            axis_no = (int)SVDF.AXES.GD_TRAY_1_ELV_Z;
                            break;
                        case 1:
                            axis_no = (int)SVDF.AXES.GD_TRAY_2_ELV_Z;
                            break;
                        case 2:
                            axis_no = (int)SVDF.AXES.RW_TRAY_ELV_Z;
                            break;
                        case 3:
                            axis_no = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;
                            break;
                        case 4:
                            axis_no = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;
                            break;
                    }
                    break;
                #endregion
                #region 트레이 스테이지 Y 동작(굿1,2 ,리워크)
                /*
            case 74:
                pos_no = (int)POSDF.TRAY_STAGE_TRAY_LOADING;
                switch (Tray_stage_select)
                {
                    case 0:
                        axis_no = (int)SVDF.AXES.GD_TRAY_STG_1_Y;
                        break;
                    case 1:
                        axis_no = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
                        break;
                    case 2:
                        axis_no = (int)SVDF.AXES.RW_TRAY_STG_Y;
                        break;
                }
                break;
                */
                case 75:
                    pos_no = (int)POSDF.TRAY_STAGE_TRAY_WORKING_P1;
                    switch (Tray_stage_select)
                    {
                        case 0:
                            axis_no = (int)SVDF.AXES.GD_TRAY_STG_1_Y;
                            break;
                        case 1:
                            axis_no = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
                            break;
                        case 2:
                            axis_no = (int)SVDF.AXES.RW_TRAY_STG_Y;
                            break;
                    }
                    break;
                case 76:
                    pos_no = (int)POSDF.TRAY_STAGE_TRAY_WORKING_P2;
                    switch (Tray_stage_select)
                    {
                        case 0:
                            axis_no = (int)SVDF.AXES.GD_TRAY_STG_1_Y;
                            break;
                        case 1:
                            axis_no = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
                            break;
                        case 2:
                            axis_no = (int)SVDF.AXES.RW_TRAY_STG_Y;
                            break;
                    }
                    break;
                case 77:
                    pos_no = (int)POSDF.TRAY_STAGE_TRAY_UNLOADING;
                    switch (Tray_stage_select)
                    {
                        case 0:
                            axis_no = (int)SVDF.AXES.GD_TRAY_STG_1_Y;
                            break;
                        case 1:
                            axis_no = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
                            break;
                        case 2:
                            axis_no = (int)SVDF.AXES.RW_TRAY_STG_Y;
                            break;
                    }
                    break;
                case 78:
                    pos_no = (int)POSDF.TRAY_STAGE_TRAY_INSPECTION_START;
                    switch (Tray_stage_select)
                    {
                        case 0:
                            axis_no = (int)SVDF.AXES.GD_TRAY_STG_1_Y;
                            break;
                        case 1:
                            axis_no = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
                            break;
                        case 2:
                            axis_no = (int)SVDF.AXES.RW_TRAY_STG_Y;
                            break;
                    }
                    break;
                #endregion
                #endregion
                default:
                    break;
            }
            MovePos(axis_no, pos_no);
        }
        void MovePos(int nAxisNo, int nPosNo)
        {
            // MOVE
            double dTargetPos = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dPos[nPosNo];
            double dSpeed = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dVel[nPosNo];
            double dAcc = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dAcc[nPosNo];
            double dDec = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dDec[nPosNo];
            string strTitle = FormTextLangMgr.FindKey("축 이동");
            string strMsg = string.Format("{0}, {1} {2}\n {3}{4} {5}{6} {7}{8} ",
                FormTextLangMgr.FindKey(SVDF.GetAxisName(nAxisNo))
                , dTargetPos.ToString("F3")
                , FormTextLangMgr.FindKey("으로 이동하시겠습니까?")
                , FormTextLangMgr.FindKey("속도:")
                , dSpeed.ToString("0.0")
                , FormTextLangMgr.FindKey("가속도:")
                , dAcc.ToString("0.0")
                , FormTextLangMgr.FindKey("감속도:")
                , dDec.ToString("0.0")
                );
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo };
            m_dPosArray = new double[] { dTargetPos };
            m_nOrderArray = new uint[] { 0 };
            m_dSpeedArray = new double[] { dSpeed };
            m_dAccArray = new double[] { dAcc };
            m_dDecArray = new double[] { dDec };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }
        void MovePos(int nAxisNo, double dTargetPos, double dSpeed = 40, double dAcc = 400, double dDec = 400)
        {
            string strTitle = FormTextLangMgr.FindKey("축 이동");
            string strMsg = string.Format("{0}, {1} {2}\n {3}{4} {5}{6} {7}{8} ",
                FormTextLangMgr.FindKey(SVDF.GetAxisName(nAxisNo))
                , dTargetPos.ToString("F3")
                , FormTextLangMgr.FindKey("으로 이동하시겠습니까?")
                , FormTextLangMgr.FindKey("속도:")
                , dSpeed.ToString("0.0")
                , FormTextLangMgr.FindKey("가속도:")
                , dAcc.ToString("0.0")
                , FormTextLangMgr.FindKey("감속도:")
                , dDec.ToString("0.0")
                );
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo };
            m_dPosArray = new double[] { dTargetPos };
            m_nOrderArray = new uint[] { 0 };
            m_dSpeedArray = new double[] { dSpeed };
            m_dAccArray = new double[] { dAcc };
            m_dDecArray = new double[] { dDec };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }
        protected void OnFinishSeq(int nResult)
        {
            //System.Diagnostics.Debug.WriteLine("Finished");

            if (this.InvokeRequired)
            {
                BeginInvoke(new CommonEvent.FinishSeqEvent(OnFinishSeq), nResult);
                return;
            }

            if (m_popManualRun != null && !m_popManualRun.IsDisposed)
            {
                if (nResult == 0) m_popManualRun.DialogResult = System.Windows.Forms.DialogResult.OK;
                m_popManualRun.Close();
            }
        }

        protected void popManual_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_popManualRun.FormClosed -= popManual_FormClosed;
            if (m_popManualRun != null)
            {
                m_popManualRun.Dispose();
            }
            m_popManualRun = null;
        }

        protected void ShowDlgPopManualRunBlock()
        {
            m_popManualRun = new POP.popManualRun();
            m_popManualRun.FormClosed += popManual_FormClosed;
            if (m_popManualRun.ShowDialog(Application.OpenForms[0]) == System.Windows.Forms.DialogResult.Cancel)
                GbSeq.manualRun.LeaveCycle(-1);
        }
    }
}

