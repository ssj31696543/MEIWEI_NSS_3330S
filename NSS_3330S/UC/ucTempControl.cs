using NSSU_3400.DEVICE;
using System.Windows.Forms;

namespace NSSU_3400.UC
{
    public partial class ucTempControl : UserControl
    {
        public string TARGET_TEMP { get; set; }
        public string WARN_HIGH_TEMP { get; set; }
        public string ALARM_HIGH_TEMP { get; set; }
        public string WARN_LOW_TEMP { get; set; }
        public string ALARM_LOW_TEMP { get; set; }
        public string WATER_TANK_INNER_TEMP { get; set; }
        public string WATER_RETURN_TEMP { get; set; }
        public string FLOW { get; set; }
        public string PID_CONTROL { get; set; }
        public string SYS_OP_STATUS { get; set; }
        public string SYS_ERROR_STATUS { get; set; }
        public string SYS_ERROR_STRING { get; set; }

        public ucTempControl()
        {
            InitializeComponent();
        }

        private void ucTempControl_Load(object sender, System.EventArgs e)
        {
            //cmbUltrasonicComm.SelectedIndex = 0;
            //cmb_setFreq.SelectedIndex = 0;
            //int nIdx = cmbUltrasonicComm.SelectedIndex;
        }

        private void cmbUltrasonicComm_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            //int nIdx = cmbUltrasonicComm.SelectedIndex;
            //GbDev.ultrasonic[nIdx].ID = nIdx;
            //GbDev.currentUltra = GbDev.ultrasonic[nIdx];
        }

        private void tmrRefresh_Tick(object sender, System.EventArgs e)
        {
#if !_NOTEBOOK
            if (this.Visible)
            {
                if (lbTargetTemp.Text != TARGET_TEMP)
                    lbTargetTemp.Text = TARGET_TEMP;
                if (lbWaterTankInnerTemp.Text != WATER_TANK_INNER_TEMP)
                    lbWaterTankInnerTemp.Text = WATER_TANK_INNER_TEMP;
                if (lbWaterReturnTemp.Text != WATER_RETURN_TEMP)
                    lbWaterReturnTemp.Text = WATER_RETURN_TEMP;

                int nValue = 0;
                int.TryParse(SYS_OP_STATUS, out nValue);
            
                if (lbStatus.Text != ((DSH_20_B7_HT.SYSTEM_OPERATION_STATUS)nValue).ToString())
                    lbStatus.Text = ((DSH_20_B7_HT.SYSTEM_OPERATION_STATUS)nValue).ToString();

                for (int nIdx = 0; nIdx < (int)DSH_20_B7_HT.ERROR_CODE.MAX; nIdx++)
                {

                }
                if (lbErrorCode.Text != SYS_ERROR_STATUS)
                    lbErrorCode.Text = SYS_ERROR_STATUS;
            }
#endif
        }

        private void btn_setFreq_Click(object sender, System.EventArgs e)
        {
            //if (GbDev.currentUltra == null) return;
            //int nChannel = 0;
            //nChannel = cmb_setFreq.SelectedIndex + 1;
            //GbDev.currentUltra.SetFreqChannel(nChannel);
        }


        private void ucTempControl_VisibleChanged(object sender, System.EventArgs e)
        {
            //if (GbDev.currentUltra == null) return;
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
