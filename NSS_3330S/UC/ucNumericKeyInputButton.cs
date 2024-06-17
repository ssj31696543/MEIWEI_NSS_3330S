using NSS_3330S.POP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public class ucNumericKeyInputButtonEx : Button
    {
        int nDecimalPlace = 3;
        bool bInt = false;
        string strVal;

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public bool INTEGER
        {
            get { return bInt; }
            set { bInt = value; }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public int DECIMAL_PLACE
        {
            get { return nDecimalPlace; }
            set { nDecimalPlace = value; }
        }

        public ucNumericKeyInputButtonEx()
        {
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            if (bInt)
            {
                this.Text = "0";
            }
            else
            {
                this.Text = "0.000";
            }
            
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.Click += EditValueControl_Click;
            this.TextChanged += EditValueControl_TextChanged;
        }

        void EditValueControl_Click(object sender, EventArgs e)
        {
            ucNumericKeyInputButtonEx btn = sender as ucNumericKeyInputButtonEx;

            popKeypad key = new popKeypad(btn.Text, DECIMAL_PLACE, INTEGER);
            if (key.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            btn.Text = key.GET_VALUE.ToString();
        }

        private void EditValueControl_TextChanged(object sender, EventArgs e)
        {
            //
        }

        public double ToDouble()
        {
            double dVal = 0.0;

            double.TryParse(this.Text, out dVal);

            return dVal;
        }

        public int ToInt()
        {
            int nVal = 0;

            int.TryParse(this.Text, out nVal);

            return nVal;
        }
    }
}
