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
    public partial class ucKeyboard : Form
    {
        const uint WS_EX_NOACTIVATE = 0x08000000;
        const uint WS_EX_TOPMOST = 0x00000008;

        public ucKeyboard()
        {
            InitializeComponent();
        }

        private void ucKeyboard_Load(object sender, EventArgs e)
        {
            //virtualKeyboard.BeginGradientColor = Color.Red; //버튼 색상
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;
                baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOPMOST);
                return baseParams;
            }
        }
    }
}
