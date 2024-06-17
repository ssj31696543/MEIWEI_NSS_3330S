using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.POP;

namespace NSS_3330S.UC
{
    public partial class ucNumericUpDown : UserControl
    {
        public ucNumericUpDown()
        {
            InitializeComponent();
        }

        public delegate void LightValueChangeDele(int nChangeIncVal);
        public event LightValueChangeDele LightValueChagedEvent = null;

        private void btnBrightDec_Click(object sender, EventArgs e)
        {
            if (cmbStepValue.Text == "") return;

            int nChangeVal = 0;
            if (int.TryParse(cmbStepValue.Text, out nChangeVal))
            {
                SetLightInc(-nChangeVal);
            }
        }

        private void btnBrightInc_Click(object sender, EventArgs e)
        {
            if (cmbStepValue.Text == "") return;

            int nChangeVal = 0;
            if (int.TryParse(cmbStepValue.Text, out nChangeVal))
            {
                SetLightInc(nChangeVal);
            }
        }
        protected void SetLightInc(int nChangeIncVal)
        {
            if (LightValueChagedEvent != null)
                LightValueChagedEvent(nChangeIncVal);
        }
    }
}
