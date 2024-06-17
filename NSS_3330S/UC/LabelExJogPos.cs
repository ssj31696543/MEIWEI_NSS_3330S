using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.MOTION;

namespace NSS_3330S.UC
{
    [DefaultProperty("AXIS")]
    public class LabelExJogPos : Label
    {
        public SVDF.AXES AXIS
        {
            get;
            set;
        }

        public LabelExJogPos()
        {

        }

        public void RefreshUI()
        {
            this.Text = MotionMgr.Inst[AXIS].GetRealPos().ToString("F3");
        }
    }
}
