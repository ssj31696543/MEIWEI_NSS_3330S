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
    [DefaultProperty("A_INPUT")]
    public partial class ucVacInfo : UserControl
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
        public string VAC_NAME
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
        public string VAC_VAL
        {
            get { return lbTEXT.Text; }
            set { lbTEXT.Text = value; }
        }
        public ucVacInfo()
        {
            InitializeComponent();
        }
        public void RefreshUI()
        {

        }
    }
}
