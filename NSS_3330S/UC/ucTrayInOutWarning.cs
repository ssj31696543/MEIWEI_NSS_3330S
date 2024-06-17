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
    public partial class ucTrayInOutWarning : UserControl
    {
        bool m_bOutWarning = false; 

        /// <summary>
        /// true = OutWaring, false = InWarning
        /// </summary>
        [Category("Advanced")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Description("true = OutWaring, false = InWarning")]
        public bool IsOutWarning 
        {
            get { return m_bOutWarning; }
            set
            {
                if (m_bOutWarning == value) return;

                m_bOutWarning = value;
                this.Invalidate();
            }
        }

        public ucTrayInOutWarning()
        {
            InitializeComponent();

            if (m_bOutWarning == true)
            {
                lbPicture.ImageIndex = 2;
                lbText.Text = "트레이를\r\n제거하십시오.";
            }
            else
            {
                lbPicture.ImageIndex = 0;
                lbText.Text = "트레이를\r\n투입하십시오.";
            }
        }

        private void ucTrayInOutWarning_VisibleChanged(object sender, EventArgs e)
        {
            //tmrRefresh.Enabled = this.Visible;

            if (this.Visible)
            {
                if (m_bOutWarning == true)
                {
                    lbPicture.ImageIndex = 2;
                    lbText.Text = "트레이를\r\n제거하십시오.";
                }
                else
                {
                    lbPicture.ImageIndex = 0;
                    lbText.Text = "트레이를\r\n투입하십시오.";
                }
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if (m_bOutWarning == true)
            {
                if (lbPicture.ImageIndex == 2)
                    lbPicture.ImageIndex = 3;
                else
                    lbPicture.ImageIndex = 2;
            }
            else
            {
                if (lbPicture.ImageIndex == 0)
                    lbPicture.ImageIndex = 1;
                else
                    lbPicture.ImageIndex = 0;
            }
        }
    }
}
