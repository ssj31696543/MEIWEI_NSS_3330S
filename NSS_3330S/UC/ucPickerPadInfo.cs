using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    [DefaultProperty("A_INPUT")]
    public partial class ucPickerPadInfo : UserControl
    {
        bool bVacOn = false;
        int nJudge = 0;

        public IODF.A_INPUT A_INPUT
        {
            get;
            set;
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public string AXIS_NAME
        {
            get { return lbAxisName.Text; }
            set
            {
                lbAxisName.Text = value;
            }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public bool ON
        {
            get { return bVacOn; }
            set
            {
                bVacOn = value;
                if (bVacOn && a1Panel4.BorderColor != Color.DarkGreen)
                {
                    a1Panel4.BorderColor = Color.DarkGreen;
                }
                else if (!bVacOn && a1Panel4.BorderColor != Color.FromArgb(32, 32, 32))
                {
                    a1Panel4.BorderColor = Color.FromArgb(32, 32, 32);
                }
            }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public string TOP_JUDGE_TEXT
        {
            get { return lbTopInspResult.Text; }
            set
            {
                lbTopInspResult.Text = value;
                if (lbTopInspResult.Text == "WAIT")
                {
                    lbTopInspResult.BackColor = Color.Gold;
                }
                else if (lbTopInspResult.Text == "OK")
                {
                    lbTopInspResult.BackColor = Color.SkyBlue;
                }
                else if (lbTopInspResult.Text == "RW")
                {
                    lbTopInspResult.BackColor = Color.OrangeRed;
                }
            }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public string BTM_JUDGE_TEXT
        {
            get { return lbBtmInspResult.Text; }
            set 
            { 
                lbBtmInspResult.Text = value;
                if (lbBtmInspResult.Text == "WAIT")
                {
                    lbBtmInspResult.BackColor = Color.Gold;
                }
                else if (lbBtmInspResult.Text == "OK")
                {
                    lbBtmInspResult.BackColor = Color.SkyBlue;
                }
                else if (lbBtmInspResult.Text == "RW")
                {
                    lbBtmInspResult.BackColor = Color.OrangeRed;
                }
            }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public string VAC_VAL
        {
            get { return lbTEXT.Text; }
            set { lbTEXT.Text = value; }
        }

        public ucPickerPadInfo()
        {
            InitializeComponent();
        }

        public void RefreshUI()
        {

        }
    }
}
