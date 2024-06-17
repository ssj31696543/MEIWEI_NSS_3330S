using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.POP
{
    public partial class popChangeCtrlStatus : Form
    {
        public popChangeCtrlStatus()
        {
            InitializeComponent();
        }

        private void btnCtrlChange_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void tmrStatus_Tick(object sender, EventArgs e)
        {
            lbControlStatus.Text = ((MESDF.eControlStatus)GbVar.mcState.nControlStatus).ToString();
        }

        private void popChangeCtrlStatus_VisibleChanged(object sender, EventArgs e)
        {
            if (GbVar.mcState.nControlStatus == (int)MESDF.eControlStatus.OFFLINE)
            {
                rdnOffline.Checked = true;
            }
            else if (GbVar.mcState.nControlStatus == (int)MESDF.eControlStatus.ONLINE_LOCAL)
            {
                rdnOnlineLocal.Checked = true;
            }
            else
            {
                rdnOnlineRemote.Checked = true;
            }

            tmrStatus.Enabled = this.Visible;
        }
    }
}
