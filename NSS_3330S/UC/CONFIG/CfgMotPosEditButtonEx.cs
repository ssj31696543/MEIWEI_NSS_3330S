using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC.CONFIG
{
    public class CfgMotPosEditButtonEx : Button
    {
        SVDF.AXES m_axis = SVDF.AXES.NONE;
        int m_nPosNo = 0;
        int m_nCategory = 0;
        private ToolTip toolTip1 = new System.Windows.Forms.ToolTip();

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public SVDF.AXES Axis
        {
            get { return m_axis; }
            set { m_axis = value; }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public int PosNo
        {
            get { return m_nPosNo; }
            set { m_nPosNo = value; }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public int Category
        {
            get { return m_nCategory; }
            set { m_nCategory = value; }
        }

        [Category("Advanced")]
        public string PosName
        {
            get
            {
                string name = "";

                #region Position Name
                name = POSDF.GetPosName((POSDF.CFG_POS_MODE)m_nCategory, m_nPosNo);
                #endregion

                return name;
            }
        }

        public CfgMotPosEditButtonEx()
        {
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            
            this.Text = "0.000";
            this.TextAlign = ContentAlignment.MiddleCenter;

            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //this.FlatAppearance.BorderColor
        }
        [Category("Advanced")]
        // Public and designer access to the property.
        public string ButtonToolTip
        {
            get
            {
                return toolTip1.GetToolTip(this);
            }
            set
            {
                toolTip1.SetToolTip(this, value);
            }
        }
        public double ToDouble()
        {
            double dPos = 0.0;

            double.TryParse(this.Text, out dPos);

            return dPos;
        }
    }
}
