using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.MOTION;

namespace NSS_3330S.UC
{
    [DefaultProperty("AXIS")]
    public partial class ucServoStatus : UserControl
    {
        public delegate void deleChangeServoChecked(ucServoStatus ucSvStatus, SVDF.AXES axis, bool bChecked);
        public event deleChangeServoChecked changeServoChecked;

        public delegate void deleServoHomeClicked(ucServoStatus ucSvStatus, SVDF.AXES axis);
        public event deleServoHomeClicked servoHomeClicked;

        bool m_bDisableHomeCheckbox = false;
        bool m_bDisableServoOn = false;

        public SVDF.AXES AXIS
        {
            get;
            set;
        }

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public string AXIS_NAME
        {
            get
            {
                return lblAxisName.Text;
            }
            set
            {
                lblAxisName.Text = value;
            }
        }

        ///// <summary>
        ///// HOME Checkbox를 비활성화
        ///// </summary>
        ///// [Category("Advanced")]
        //[Browsable(true)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        //[EditorBrowsable(EditorBrowsableState.Always)]
        //public bool DISABLE_HOME_CHECKBOX
        //{
        //    get
        //    {
        //        return m_bDisableHomeCheckbox;
        //    }
        //    set
        //    {
        //        m_bDisableHomeCheckbox = value;

        //        if (cbxAxis.Visible) cbxAxis.Checked = true;
        //        cbxAxis.Visible = !m_bDisableHomeCheckbox;
        //        if (cbxAxis.Visible) cbxAxis.Checked = true;
        //    }
        //}

        ///// <summary>
        ///// HOME Checkbox를 숨김 및 표시
        ///// </summary>
        ///// [Category("Advanced")]
        //[Browsable(true)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        //[EditorBrowsable(EditorBrowsableState.Always)]
        //public bool VISIBLE_HOME_CHECKBOX
        //{
        //    get { return cbxAxis.Visible; }
        //    set
        //    {
        //        if (value)
        //        {
        //            if (m_bDisableHomeCheckbox) return;
        //        }

        //        cbxAxis.Visible = value;
        //    }
        //}

        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public bool VISIBLE_HOME_BUTTON
        {
            get
            {
                btnHome.Visible = true;
                return btnHome.Visible;
            }
            set
            {
                btnHome.Visible = true;
            }
        }

        //[Category("Advanced")]
        //[Browsable(true)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        //[EditorBrowsable(EditorBrowsableState.Always)]
        //public bool CHECKED
        //{
        //    get { return cbxAxis.CheckState == CheckState.Indeterminate; }
        //    set
        //    {
        //        if (value && !m_bDisableHomeCheckbox)
        //        {
        //            cbxAxis.CheckState = CheckState.Indeterminate;
        //        }
        //        else
        //        {
        //            cbxAxis.CheckState = CheckState.Unchecked;
        //        }

        //        if (changeServoChecked != null)
        //            changeServoChecked(this, AXIS, value);
        //    }
        //}

        /// <summary>
        /// Servo On/Off 버튼을 비활성화
        /// </summary>
        /// [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public bool DISABLE_SERVO_ON
        {
            get { return m_bDisableServoOn; }
            set
            {
                m_bDisableServoOn = value;
            }
        }

        public ucServoStatus()
        {
            InitializeComponent();
        }

        private void ucServoStatus_Load(object sender, EventArgs e)
        {

        }

        public void RefreshUI()
        {
            if (ledAlarm != null)
                ledAlarm.On = MotionMgr.Inst[AXIS].IsAlarm();

            if (ledInPo != null)
                ledInPo.On = MotionMgr.Inst[AXIS].IsInPosition();

            if (ledBusy != null)
                ledBusy.On = MotionMgr.Inst[AXIS].IsBusy();

            if (ledHome != null)
                ledHome.On = MotionMgr.Inst[AXIS].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Success;

            if (ledRls != null)
                ledRls.On = MotionMgr.Inst[AXIS].IsMinusLimit();

            if (ledFls != null)
                ledFls.On = MotionMgr.Inst[AXIS].IsPlusLimit();

            if (lblPos != null)
                lblPos.Text = MotionMgr.Inst[AXIS].GetRealPos().ToString("F3");

            if (btnServoOn != null)
            {
                if (MotionMgr.Inst[AXIS].GetServoOnOff())
                {
                    if (btnServoOn.BackColor != Color.FromArgb(153, 255, 54))
                        btnServoOn.BackColor = Color.FromArgb(153, 255, 54);

                    if (btnServoOn.ForeColor != Color.Black)
                        btnServoOn.ForeColor = Color.Black;

                    if (m_bDisableServoOn)
                    {
                        if (btnServoOn.Enabled == false) btnServoOn.Enabled = true;
                    }
                }
                else
                {
                    if (btnServoOn.BackColor != Color.FromArgb(24, 24, 24))
                        btnServoOn.BackColor = Color.FromArgb(24, 24, 24);

                    if (btnServoOn.ForeColor != Color.White)
                        btnServoOn.ForeColor = Color.White;

                    if (m_bDisableServoOn)
                    {
                        if (btnServoOn.Enabled == true) btnServoOn.Enabled = false;
                    }
                }
            }

            if (btnServoReset != null)
            {
                if (MotionMgr.Inst[AXIS].IsAlarm())
                {
                    if (btnServoReset.BackColor != Color.Red)
                        btnServoReset.BackColor = Color.Red;
                }
                else
                {
                    if (btnServoReset.BackColor != Color.FromArgb(24, 24, 24))
                        btnServoReset.BackColor = Color.FromArgb(24, 24, 24);
                }
            }
        }

        private void cbxAxis_Click(object sender, EventArgs e)
        {
            //if (DISABLE_HOME_CHECKBOX) return;

            //CHECKED = !CHECKED;
        }

        private void btnServoOn_Click(object sender, EventArgs e)
        {
            if (m_bDisableServoOn) return;

            bool bSetOnOff = !MotionMgr.Inst[AXIS].GetServoOnOff();

            MotionMgr.Inst[AXIS].SetServoOnOff(bSetOnOff);

            if (bSetOnOff == false)
            {
                GbVar.mcState.isHomeComplete[(int)AXIS] = false;
            }
        }

        private void btnServoReset_Click(object sender, EventArgs e)
        {
            bool bReset = MotionMgr.Inst[AXIS].ResetAlarm();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            string strTitle = "축 원점검색 (홈동작)";
            string strMsg = string.Format("{0} 축의 원점검색을 하시겠습니까?", SVDF.GetAxisName(AXIS));
            POP.popMessageBox pMsgBox = new POP.popMessageBox(strMsg, strTitle);
            if (pMsgBox.ShowDialog() != DialogResult.OK) return;

            if (servoHomeClicked != null)
                servoHomeClicked(this, AXIS);
        }
    }
}