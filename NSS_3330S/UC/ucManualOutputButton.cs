using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Design;
using NSS_3330S;
using NSS_3330S.MOTION;

namespace NSS_3330S.UC
{
    [DefaultProperty("Output")]
    public partial class ucManualOutputButton : UserControl
    {
        IODF.OUTPUT _Output = IODF.OUTPUT.NONE;

        Color _OutputColorOn = Color.Gold;
        Color _OutputColorOff = SystemColors.Control;

        // Serialized property.
        private ToolTip toolTip1 = new System.Windows.Forms.ToolTip();

        bool m_bOn = false;

        public IODF.OUTPUT Output
        {
            get { return _Output; }
            set { _Output = value; }
        }

        public bool On
        {
            get { return m_bOn; }
            set
            {
                if (m_bOn != value)
                {
                    m_bOn = value;

                    btnOutput.BackColor = m_bOn ? _OutputColorOn : _OutputColorOff;
                }
            }
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ButtonText
        {
            get { return btnOutput.Text; }
            set { btnOutput.Text = value; }
        }

        public override string Text
        {
            get { return btnOutput.Text; }
            set { btnOutput.Text = value; }
        }

        public ucManualOutputButton()
        {
            InitializeComponent();
        }

        private void UcManualOutputButtonUmac_Load(object sender, EventArgs e)
        {

        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {

            if (_Output == IODF.OUTPUT.NONE || _Output == IODF.OUTPUT.MAX)
            {
                return;
            }
            bool bCurrentOn = GbVar.GB_OUTPUT[(int)_Output] == 1;
            // 인터락 추가해야 함
            int nSafety = SafetyMgr.Inst.GetDioBeforeOnOff(_Output, !bCurrentOn);
            if (FNC.IsErr(nSafety))
            {
                return;
            }



            GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("Output Button {0}_Click", _Output.ToString()));

            SafetyMgr.Inst.DioOffReverse(_Output, !bCurrentOn);

            MotionMgr.Inst.SetOutput(_Output, !bCurrentOn);
            CheckCombinatedOutput(_Output, !bCurrentOn);
        }

        // Public and designer access to the property.
        public string ButtonToolTip
        {
            get
            {
                return toolTip1.GetToolTip(btnOutput);
            }
            set
            {
                toolTip1.SetToolTip(btnOutput, value);
            }
        }
        private void CheckCombinatedOutput(IODF.OUTPUT Out, bool bIsOn)
        {
            switch (Out)
            {
                case IODF.OUTPUT.NONE:
                    break;
                case IODF.OUTPUT.TEN_KEY_1:
                    break;
                case IODF.OUTPUT.TEN_KEY_2:
                    break;
                case IODF.OUTPUT.TEN_KEY_3:
                    break;
                case IODF.OUTPUT.TEN_KEY_4:
                    break;
                case IODF.OUTPUT.TEN_KEY_5:
                    break;
                case IODF.OUTPUT.TEN_KEY_6:
                    break;
                case IODF.OUTPUT.TEN_KEY_7:
                    break;
                case IODF.OUTPUT.TEN_KEY_8:
                    break;
                case IODF.OUTPUT.TEN_KEY_9:
                    break;
                case IODF.OUTPUT.HPC_POWER_SWITCH_LED:
                    break;
                case IODF.OUTPUT.VPC_POWER_SWITCH_LED:
                    break;
                case IODF.OUTPUT.START_SWITCH_LED:
                    break;
                case IODF.OUTPUT.STOP_SWITCH_LED:
                    break;
                case IODF.OUTPUT.RESET_SWITCH_LED:
                    break;
                case IODF.OUTPUT.DOOR_LOCK_SIGNAL:
                    break;
                case IODF.OUTPUT.FLUORESCENT_LIGHT_ONOFF:
                    break;
                case IODF.OUTPUT.TOWER_LAMP_RED_LED:
                    break;
                case IODF.OUTPUT.TOWER_LAMP_YELLOW_LED:
                    break;
                case IODF.OUTPUT.TOWER_LAMP_GREEN_LED:
                    break;
                case IODF.OUTPUT.ERROR_BUZZER:
                    break;
                case IODF.OUTPUT.END_BUZZER:
                    break;
                case IODF.OUTPUT.ION_BLOWER_FAN_RUN_1:
                    break;
                case IODF.OUTPUT.ION_BLOWER_FAN_TIP_CLEANING_1:
                    break;
                case IODF.OUTPUT.ION_BLOWER_FAN_RUN_2:
                    break;
                case IODF.OUTPUT.ION_BLOWER_FAN_TIP_CLEANING_2:
                    break;
                case IODF.OUTPUT.ION_BAR_RUN_1:
                    break;
                case IODF.OUTPUT.SERVO_POWER_OFF:
                    break;
                case IODF.OUTPUT.SPARE_026:
                    break;
                case IODF.OUTPUT.IONIZER_AIR:
                    break;
                case IODF.OUTPUT.BTM_VISION_ALIGN_FWD:
                    GbFunc.SetOut(IODF.OUTPUT.BTM_VISION_ALIGN_BWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.BTM_VISION_ALIGN_FWD, bIsOn);
                    break;
                case IODF.OUTPUT.BTM_VISION_ALIGN_BWD:
                    GbFunc.SetOut(IODF.OUTPUT.BTM_VISION_ALIGN_FWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.BTM_VISION_ALIGN_BWD, bIsOn);
                    break;
                case IODF.OUTPUT.GD_TRAY_1_STG_UP:
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_1_STG_DOWN, false);
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_1_STG_UP, bIsOn);
                    break;
                case IODF.OUTPUT.GD_TRAY_1_STG_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_1_STG_UP, false);
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_1_STG_DOWN, bIsOn);
                    break;
                case IODF.OUTPUT.GD_TRAY_2_STG_UP:
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_2_STG_DOWN, false);
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_2_STG_UP, bIsOn);
                    break;
                case IODF.OUTPUT.GD_TRAY_2_STG_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_2_STG_UP, false);
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_2_STG_DOWN, bIsOn);
                    break;
                case IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP:
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP, bIsOn);
                    break;
                case IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP:
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP, bIsOn);
                    break;
                case IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP:
                    GbFunc.SetOut(IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP, bIsOn);
                    break;
                case IODF.OUTPUT.GD_TRAY_1_STG_GRIP:
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_1_STG_GRIP, bIsOn);
                    break;
                case IODF.OUTPUT.GD_TRAY_2_STG_GRIP:
                    GbFunc.SetOut(IODF.OUTPUT.GD_TRAY_2_STG_GRIP, bIsOn);
                    break;
                case IODF.OUTPUT.RW_TRAY_STG_GRIP:
                    GbFunc.SetOut(IODF.OUTPUT.RW_TRAY_STG_GRIP, bIsOn);
                    break;
                case IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_UP:
                    GbFunc.SetOut(IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN, !bIsOn);
                    break;
                case IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_UP, !bIsOn);
                    break;
                case IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP:
                    GbFunc.SetOut(IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN, !bIsOn);

                    break;
                case IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP, !bIsOn);

                    break;
                case IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP:
                    GbFunc.SetOut(IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN, !bIsOn);

                    break;
                case IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP, !bIsOn);

                    break;
                case IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP:
                    GbFunc.SetOut(IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN, !bIsOn);

                    break;
                case IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP, !bIsOn);

                    break;
                case IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP:
                    GbFunc.SetOut(IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN, !bIsOn);

                    break;
                case IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP, !bIsOn);
                    break;
                case IODF.OUTPUT.SPARE_119:
                    break;
                case IODF.OUTPUT.SPARE_120:
                    break;
                case IODF.OUTPUT.SPARE_121:
                    break;
                case IODF.OUTPUT.TRAY_PK_ATC_CLAMP:
                    GbFunc.SetOut(IODF.OUTPUT.TRAY_PK_ATC_CLAMP, bIsOn);
                    break;
                case IODF.OUTPUT.SPARE_123:
                    break;
                case IODF.OUTPUT.SPARE_124:
                    break;
                case IODF.OUTPUT.SPARE_125:
                    break;
                case IODF.OUTPUT.SPARE_126:
                    break;
                case IODF.OUTPUT.SPARE_127:
                    break;
                case IODF.OUTPUT.SPARE_128:
                    break;
                case IODF.OUTPUT.DRY_ST_AIR_KNIFE:
                    GbFunc.SetOut(IODF.OUTPUT.DRY_ST_AIR_KNIFE, bIsOn);
                    break;
                case IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT:
                    GbFunc.SetOut(IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, bIsOn);
                    break;
                case IODF.OUTPUT.SPARE_131:
                    //GbFunc.SetOut(IODF.OUTPUT.SPARE_131, bIsOn);
                    break;
                case IODF.OUTPUT.GD1_CONV_STOP:
                    break;
                case IODF.OUTPUT.GD1_CONV_RESET:
                    break;
                case IODF.OUTPUT.GD2_CONV_STOP:
                    break;
                case IODF.OUTPUT.GD2_CONV_RESET:
                    break;
                case IODF.OUTPUT.RW_CONV_STOP:
                    break;
                case IODF.OUTPUT.RW_CONV_RESET:
                    break;
                case IODF.OUTPUT.EMPTY_CONV_STOP:
                    break;
                case IODF.OUTPUT.EMPTY_CONV_RESET:
                    break;
                case IODF.OUTPUT.SPARE_208:
                    break;
                case IODF.OUTPUT.MARK_VISION_BLOW:
                    break;
                case IODF.OUTPUT.MAP_PK_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.MAP_PK_VAC_ON:
                    break;
                case IODF.OUTPUT.MAP_PK_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.MAP_PK_BLOW:
                    break;
                case IODF.OUTPUT.DRY_BLOCK_VAC_PUMP:
                    break;
                case IODF.OUTPUT.DRY_BLOCK_VAC_ON:
                    break;
                case IODF.OUTPUT.SPARE_216:
                    break;
                case IODF.OUTPUT.DRY_BLOCK_BLOW:
                    break;
                case IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.MAP_ST_1_VAC_ON:
                    break;
                case IODF.OUTPUT.MAP_ST_1_BLOW:
                    break;
                case IODF.OUTPUT.SPARE_221:
                    break;
                case IODF.OUTPUT.SPARE_222:
                    break;
                case IODF.OUTPUT.SPARE_223:
                    break;
                case IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.MAP_ST_2_VAC_ON:
                    break;
                case IODF.OUTPUT.MAP_ST_2_BLOW:
                    break;
                case IODF.OUTPUT.SPARE_227:
                    break;
                case IODF.OUTPUT.SPARE_228:
                    break;
                case IODF.OUTPUT.SPARE_229:
                    break;
                case IODF.OUTPUT.SPARE_230:
                    break;
                case IODF.OUTPUT.SPARE_231:
                    break;
                case IODF.OUTPUT.GD_1_ELV_STOPPER_FWD:
                    GbFunc.SetOut(IODF.OUTPUT.GD_1_ELV_STOPPER_BWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.GD_1_ELV_STOPPER_FWD, bIsOn);
                    break;
                case IODF.OUTPUT.GD_1_ELV_STOPPER_BWD:
                    GbFunc.SetOut(IODF.OUTPUT.GD_1_ELV_STOPPER_FWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.GD_1_ELV_STOPPER_BWD, bIsOn);
                    break;
                case IODF.OUTPUT.GD_2_ELV_STOPPER_FWD:
                    GbFunc.SetOut(IODF.OUTPUT.GD_2_ELV_STOPPER_BWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.GD_2_ELV_STOPPER_FWD, bIsOn);
                    break;
                case IODF.OUTPUT.GD_2_ELV_STOPPER_BWD:
                    GbFunc.SetOut(IODF.OUTPUT.GD_2_ELV_STOPPER_FWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.GD_2_ELV_STOPPER_BWD, bIsOn);
                    break;
                case IODF.OUTPUT.RW_ELV_STOPPER_FWD:
                    GbFunc.SetOut(IODF.OUTPUT.RW_ELV_STOPPER_BWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.RW_ELV_STOPPER_FWD, bIsOn);
                    break;
                case IODF.OUTPUT.RW_ELV_STOPPER_BWD:
                    GbFunc.SetOut(IODF.OUTPUT.RW_ELV_STOPPER_FWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.RW_ELV_STOPPER_BWD, bIsOn);
                    break;
                case IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD:
                    GbFunc.SetOut(IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD, bIsOn);
                    break;
                case IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD:
                    GbFunc.SetOut(IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD, bIsOn);
                    break;
                case IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD:
                    GbFunc.SetOut(IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD, bIsOn);
                    break;
                case IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD:
                    GbFunc.SetOut(IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD, false);
                    GbFunc.SetOut(IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD, bIsOn);
                    break;
                case IODF.OUTPUT.GD2_CONV_CCW:
                    break;
                case IODF.OUTPUT.RW_CONV_CW:
                    break;
                case IODF.OUTPUT.RW_CONV_CCW:
                    break;
                case IODF.OUTPUT.SPARE_313:
                    break;
                case IODF.OUTPUT.SPARE_314:
                    break;
                case IODF.OUTPUT.SPARE_315:
                    break;
                case IODF.OUTPUT.SPARE_316:
                    break;
                case IODF.OUTPUT.SPARE_317:
                    break;
                case IODF.OUTPUT.SPARE_318:
                    break;
                case IODF.OUTPUT.SPARE_319:
                    break;
                case IODF.OUTPUT.GD1_CONV_CW:
                    break;
                case IODF.OUTPUT.GD1_CONV_CCW:
                    break;
                case IODF.OUTPUT.GD2_CONV_CW:
                    break;
                case IODF.OUTPUT.MAP_ST_AIR_KNIFE_1:
                    break;
                case IODF.OUTPUT.MAP_ST_AIR_KNIFE_2:
                    break;
                case IODF.OUTPUT.MAP_PK_AIR_KNIFE:
                    break;
                case IODF.OUTPUT.EMPTY1_CONV_CW:
                    break;
                case IODF.OUTPUT.EMPTY1_CONV_CCW:
                    break;
                case IODF.OUTPUT.EMPTY2_CONV_CW:
                    break;
                case IODF.OUTPUT.EMPTY2_CONV_CCW:
                    break;
                case IODF.OUTPUT.EMPTY2_CONV_STOP:
                    break;
                case IODF.OUTPUT.EMPTY2_CONV_RESET:
                    break;
                case IODF.OUTPUT.MAP_STAGE_NO:
                    break;
                case IODF.OUTPUT.PRE_INSP_COMP:
                    break;
                case IODF.OUTPUT.PRE_INSP_GRAB_REQ:
                    break;
                case IODF.OUTPUT.TOP_ALIGN_CNT_RESET:
                    break;
                case IODF.OUTPUT.TOP_ALIGN_COMP:
                    break;
                case IODF.OUTPUT.MAP_CNT_RESET:
                    break;
                case IODF.OUTPUT.MAP_INSP_COMP:
                    break;
                case IODF.OUTPUT.BALL_HEAD_NO:
                    break;
                case IODF.OUTPUT.BALL_CNT_RESET:
                    break;
                case IODF.OUTPUT.BALL_INSP_COMP:
                    break;
                case IODF.OUTPUT.PICKER_CAL_RESET:
                    break;
                case IODF.OUTPUT.PICKER_CAL_COMP:
                    break;
                case IODF.OUTPUT.PICKER_CAL_LED_ON:
                    break;
                case IODF.OUTPUT.PICKER_CAL_LED_OFF:
                    break;
                case IODF.OUTPUT.SPARE_414:
                    break;
                case IODF.OUTPUT.SPARE_415:
                    break;
                case IODF.OUTPUT.SPARE_416:
                    break;
                case IODF.OUTPUT.SPARE_417:
                    break;
                case IODF.OUTPUT.SPARE_418:
                    break;
                case IODF.OUTPUT.SPARE_419:
                    break;
                case IODF.OUTPUT.SPARE_420:
                    break;
                case IODF.OUTPUT.SPARE_421:
                    break;
                case IODF.OUTPUT.SPARE_422:
                    break;
                case IODF.OUTPUT.SPARE_423:
                    break;
                case IODF.OUTPUT.SPARE_424:
                    break;
                case IODF.OUTPUT.SPARE_425:
                    break;
                case IODF.OUTPUT.SPARE_426:
                    break;
                case IODF.OUTPUT.SPARE_427:
                    break;
                case IODF.OUTPUT.SPARE_428:
                    break;
                case IODF.OUTPUT.SPARE_429:
                    break;
                case IODF.OUTPUT.SPARE_430:
                    break;
                case IODF.OUTPUT.SPARE_431:
                    break;

                case IODF.OUTPUT.IN_LET_TABLE_UP:
                    GbFunc.SetOut(IODF.OUTPUT.IN_LET_TABLE_DOWN, false);
                    GbFunc.SetOut(IODF.OUTPUT.IN_LET_TABLE_UP, bIsOn);
                    break;
                case IODF.OUTPUT.IN_LET_TABLE_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.IN_LET_TABLE_UP, false);
                    GbFunc.SetOut(IODF.OUTPUT.IN_LET_TABLE_DOWN, bIsOn);
                    break;
                case IODF.OUTPUT.IN_LET_TABLE_VAC:
                    GbFunc.SetOut(IODF.OUTPUT.IN_LET_TABLE_BLOW, false);
                    GbFunc.SetOut(IODF.OUTPUT.IN_LET_TABLE_VAC, bIsOn); // !없는게 최신(2022.04.23)
                    break;
                case IODF.OUTPUT.IN_LET_TABLE_BLOW:
                    GbFunc.SetOut(IODF.OUTPUT.IN_LET_TABLE_VAC, false);
                    GbFunc.SetOut(IODF.OUTPUT.IN_LET_TABLE_BLOW, bIsOn); // !없는게 최신(2022.04.23)
                    break;
                case IODF.OUTPUT.STRIP_PK_AIR_BLOW:
                    break;
                case IODF.OUTPUT.LD_X_GRIP_UP:
                    GbFunc.SetOut(IODF.OUTPUT.LD_X_GRIP_DOWN, false);
                    GbFunc.SetOut(IODF.OUTPUT.LD_X_GRIP_UP, bIsOn); // !없는게 최신(2022.04.23)
                    break;
                case IODF.OUTPUT.LD_X_GRIP_DOWN:
                    GbFunc.SetOut(IODF.OUTPUT.LD_X_GRIP_UP, false);
                    GbFunc.SetOut(IODF.OUTPUT.LD_X_GRIP_DOWN, bIsOn); // !없는게 최신(2022.04.23)
                    break;
                case IODF.OUTPUT.LD_X_UNGRIP:
                    break;
                case IODF.OUTPUT.LD_X_GRIP:
                    GbFunc.SetOut(IODF.OUTPUT.LD_X_GRIP, bIsOn);
                    GbFunc.SetOut(IODF.OUTPUT.LD_X_UNGRIP, !bIsOn);
                    break;
                case IODF.OUTPUT.INLET_TABLE_MATERIAL_IN_STOPPER_UP:
                    break;
                case IODF.OUTPUT.INLET_TABLE_MATERIAL_IN_STOPPER_DOWN:
                    break;
                case IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD:
                    break;
                case IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.STRIP_PK_BLOW:
                    break;
                case IODF.OUTPUT.SPARE_2015:
                    break;
                case IODF.OUTPUT.SPARE_2016:
                    break;
                case IODF.OUTPUT.SPARE_2017:
                    break;
                case IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_BLOW:
                    break;
                case IODF.OUTPUT.STRIP_PK_VAC_ON:
                    break;
                case IODF.OUTPUT.UNIT_PK_VAC_ON:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON:
                    break;
                case IODF.OUTPUT.SPARE_2031:
                    break;
                case IODF.OUTPUT.SPARE_2100:
                    break;
                case IODF.OUTPUT.SPARE_2101:
                    break;
                case IODF.OUTPUT.SPARE_2102:
                    break;
                case IODF.OUTPUT.SPARE_2103:
                    break;
                case IODF.OUTPUT.SPARE_2104:
                    break;
                case IODF.OUTPUT.SPARE_2105:
                    break;
                case IODF.OUTPUT.SPARE_2106:
                    break;
                case IODF.OUTPUT.SPARE_2107:
                    break;
                case IODF.OUTPUT.SORTER_PROGRAM_ON:
                    break;
                case IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_X:
                    break;
                case IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_Z:
                    break;
                case IODF.OUTPUT.SORTER_LD_OK_R:
                    break;
                case IODF.OUTPUT.SORTER_BLOW_OFF_R:
                    break;
                case IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_X:
                    break;
                case IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_Z:
                    break;
                case IODF.OUTPUT.SORTER_LD_OK:
                    break;
                case IODF.OUTPUT.SORTER_BLOW_OFF:
                    break;
                case IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_X:
                    break;
                case IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_Z:
                    break;
                case IODF.OUTPUT.SORTER_ULD_OK_R:
                    break;
                case IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_X:
                    break;
                case IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_Z:
                    break;
                case IODF.OUTPUT.SORTER_ULD_OK:
                    break;
                case IODF.OUTPUT.DICING_COMM_8:
                    break;
                case IODF.OUTPUT.DICING_COMM_9:
                    break;
                case IODF.OUTPUT.DICING_COMM_10:
                    break;
                case IODF.OUTPUT.SORTER_HBC_MDL_MOVE_PERMIT:
                    break;
                case IODF.OUTPUT.DICING_COMM_12:
                    break;
                case IODF.OUTPUT.SORTER_RCP_CHANGE:
                    break;
                case IODF.OUTPUT.SORTER_ERROR:
                    break;
                case IODF.OUTPUT.SORTER_PK_INTERLOCK_R:
                    break;
                case IODF.OUTPUT.SORTER_PK_INTERLOCK:
                    break;
                case IODF.OUTPUT._1F_CONVEYOR_INPUT_SWITCH_LED:
                    break;
                case IODF.OUTPUT._1F_CONVEYOR_OUTPUT_SWITCH_LED:
                    break;
                case IODF.OUTPUT._2F_CONVEYOR_INPUT_SWITCH_LED:
                    break;
                case IODF.OUTPUT._2F_CONVEYOR_OUTPUT_SWITCH_LED:
                    break;
                case IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD:
                    break;
                case IODF.OUTPUT.SERVO_CONTROL_POWER_OFF_SIGNAL:
                    break;
                case IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT._1F_MZ_CONVEYOR_RESET_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_CONVEYOR_RESET_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT._2F_MZ_CONVEYOR_RESET_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_CONVEYOR_RESET_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT.ME_CONVEYOR_RESET_SIGNAL:
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_CONVEYOR_RESET_SIGNAL, bIsOn);
                    break;
                case IODF.OUTPUT.ME_MZ_CLAMP_SOL:
                    GbFunc.SetOut(IODF.OUTPUT.ME_MZ_UNCLAMP_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_MZ_CLAMP_SOL, bIsOn);
                    break;
                case IODF.OUTPUT.ME_MZ_UNCLAMP_SOL:
                    GbFunc.SetOut(IODF.OUTPUT.ME_MZ_CLAMP_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT.ME_MZ_UNCLAMP_SOL, bIsOn);
                    break;
                case IODF.OUTPUT.MZ_DOOR_OPEN_SOL:
                    GbFunc.SetOut(IODF.OUTPUT.MZ_DOOR_CLOSE_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT.MZ_DOOR_OPEN_SOL, bIsOn);
                    break;
                case IODF.OUTPUT.MZ_DOOR_CLOSE_SOL:
                    GbFunc.SetOut(IODF.OUTPUT.MZ_DOOR_OPEN_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT.MZ_DOOR_CLOSE_SOL, bIsOn);
                    break;
                case IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL:
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_STOPPER_DOWN_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL, bIsOn);
                    break;
                case IODF.OUTPUT._1F_MZ_STOPPER_DOWN_SOL:
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_MZ_STOPPER_DOWN_SOL, bIsOn);
                    break;
                case IODF.OUTPUT._2F_MZ_STOPPER_UP_SOL:
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_STOPPER_DOWN_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_STOPPER_UP_SOL, bIsOn);
                    break;
                case IODF.OUTPUT._2F_MZ_STOPPER_DOWN_SOL:
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_STOPPER_UP_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT._2F_MZ_STOPPER_DOWN_SOL, bIsOn);
                    break;
                case IODF.OUTPUT._1F_INPUT_STOPPER_UP_SOL:
                    GbFunc.SetOut(IODF.OUTPUT._1F_INPUT_STOPPER_DOWN_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_INPUT_STOPPER_UP_SOL, bIsOn);
                    break;
                case IODF.OUTPUT._1F_INPUT_STOPPER_DOWN_SOL:
                    GbFunc.SetOut(IODF.OUTPUT._1F_INPUT_STOPPER_UP_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT._1F_INPUT_STOPPER_DOWN_SOL, bIsOn);
                    break;
                case IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL:
                    GbFunc.SetOut(IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL, bIsOn);
                    break;
                case IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL:
                    GbFunc.SetOut(IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL, false);
                    GbFunc.SetOut(IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL, bIsOn);
                    break;
                case IODF.OUTPUT.SPARE_3030:
                    break;
                case IODF.OUTPUT.SPARE_3031:
                    break;
                case IODF.OUTPUT.SPARE_3100:
                    break;
                case IODF.OUTPUT.SPARE_3101:
                    break;
                case IODF.OUTPUT.SPARE_3102:
                    break;
                case IODF.OUTPUT.SPARE_3103:
                    break;
                case IODF.OUTPUT.SPARE_3104:
                    break;
                case IODF.OUTPUT.SPARE_3105:
                    break;
                case IODF.OUTPUT.SPARE_3106:
                    break;
                case IODF.OUTPUT.SPARE_3107:
                    break;
                case IODF.OUTPUT.SPARE_3108:
                    break;
                case IODF.OUTPUT.SPARE_3109:
                    break;
                case IODF.OUTPUT.SPARE_3110:
                    break;
                case IODF.OUTPUT.SPARE_3111:
                    break;
                case IODF.OUTPUT.SPARE_3112:
                    break;
                case IODF.OUTPUT.SPARE_3113:
                    break;
                case IODF.OUTPUT.SPARE_3114:
                    break;
                case IODF.OUTPUT.SPARE_3115:
                    break;
                case IODF.OUTPUT.SPARE_3116:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_1_WATER:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_WATER_1_WATER, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_WATER_1_AIR:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_WATER_1_AIR, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_WATER_2_WATER:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_WATER_2_WATER, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_WATER_2_AIR:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_WATER_2_AIR, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_WATER_3_WATER:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_WATER_3_WATER, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_WATER_3_AIR:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_WATER_3_AIR, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_WATER_4_WATER:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_WATER_4_WATER, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_WATER_4_AIR:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_WATER_4_AIR, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_BRUSH_WATER:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_BRUSH_WATER, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_AIR_KNIFE_1:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_AIR_KNIFE_1, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_AIR_KNIFE_2:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_AIR_KNIFE_2, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, bIsOn);
                    break;
                case IODF.OUTPUT.CLEAN_SWING_FWD:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_SWING_FWD, bIsOn);
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_SWING_BWD, false);
                    break;
                case IODF.OUTPUT.CLEAN_SWING_BWD:
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_SWING_BWD, bIsOn);
                    GbFunc.SetOut(IODF.OUTPUT.CLEAN_SWING_FWD, false);
                    break;
                case IODF.OUTPUT.MAX:
                    break;
                default:
                    break;
            }
        }
    }
}
