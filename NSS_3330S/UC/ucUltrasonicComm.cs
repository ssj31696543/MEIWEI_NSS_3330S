using NSSU_3400.DEVICE;
using System.Windows.Forms;

namespace NSSU_3400.UC
{
    public partial class ucUltrasonicComm : UserControl
    {
        public ucUltrasonicComm()
        {
            InitializeComponent();
        }

        private void ucUltrasonicComm_Load(object sender, System.EventArgs e)
        {
            cmbUltrasonicComm.SelectedIndex = 0;
            cmb_setFreq.SelectedIndex = 0;
        }

        private void cmbUltrasonicComm_SelectedIndexChanged(object sender, System.EventArgs e)
        {
#if !_NOTEBOOK
            int nIdx = cmbUltrasonicComm.SelectedIndex;
            GbDev.ultrasonic[nIdx].ID = nIdx;
            GbDev.currentUltra = GbDev.ultrasonic[nIdx];
#endif
        }

        private void tmrRefresh_Tick(object sender, System.EventArgs e)
        {
#if !_NOTEBOOK
            if (this.Visible)
            {
                switch (GbDev.currentUltra.Setting_Freq_Channel)
                {
                    case 1:
                        if (lbl_Setting_freq_val.Text != "40")
                            lbl_Setting_freq_val.Text = "40";
                        break;
                    case 2:
                        if (lbl_Setting_freq_val.Text != "80")
                            lbl_Setting_freq_val.Text = "80";
                        break;
                    case 3:
                        if (lbl_Setting_freq_val.Text != "120")
                            lbl_Setting_freq_val.Text = "120";
                        break;
                    default:
                        break;
                }
                
                if (lbl_Setting_Out_val.Text != GbDev.currentUltra.Setting_Output.ToString())
                    lbl_Setting_Out_val.Text = GbDev.currentUltra.Setting_Output.ToString();
                if (lbl_freq_val.Text != GbDev.currentUltra.Current_Freq.ToString())
                    lbl_freq_val.Text = GbDev.currentUltra.Current_Freq.ToString();
                if (lbl_output_val.Text != GbDev.currentUltra.Current_Output.ToString())
                    lbl_output_val.Text = GbDev.currentUltra.Current_Output.ToString();
                if (lbl_osill_val.Text != GbDev.currentUltra.Oscillaotr_State.ToString())
                    lbl_osill_val.Text = GbDev.currentUltra.Oscillaotr_State.ToString();
                if (lbl_alarm_val.Text != GbDev.currentUltra.Alarm_State.ToString())
                    lbl_alarm_val.Text = GbDev.currentUltra.Alarm_State.ToString();
            }
#endif
        }

        private void btn_setFreq_Click(object sender, System.EventArgs e)
        {
            int nChannel = 0;
            nChannel = cmb_setFreq.SelectedIndex + 1;
            GbDev.currentUltra.SetFreqChannel(nChannel);
        }

        private void btn_OperationOutput_Click(object sender, System.EventArgs e)
        {
            int nValue = 0;
            int.TryParse(tb_OperationOutput.Text, out nValue);
            if (UltraSonicComm.MAX_POWER < nValue) nValue = UltraSonicComm.MAX_POWER;
            GbDev.currentUltra.SetOperationOutput(nValue);
        }

        private void btn_OsillatorOn_Click(object sender, System.EventArgs e)
        {
            GbDev.currentUltra.SetOsillatorOnOff(true);
        }
        private void btn_OsillatorOff_Click(object sender, System.EventArgs e)
        {
            GbDev.currentUltra.SetOsillatorOnOff(false);
        }
        private void btn_AlarmClear_Click(object sender, System.EventArgs e)
        {
            GbDev.currentUltra.SetAlarmClear();
        }

        private void ucUltrasonicComm_VisibleChanged(object sender, System.EventArgs e)
        {
            if (this.Visible)
            {
                tmrRefresh.Enabled = true;
            }
            else
            {
                tmrRefresh.Enabled = false;
            }
        }
    }
}
