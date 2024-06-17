using NSS_3330S;
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
    [DefaultProperty("Input")]
    public class InputPanel : Panel
    {
        IODF.INPUT _Input = IODF.INPUT.NONE;

        Color _InputColorOn = Color.Lime;
        Color _InputColorOff = Color.Transparent;

        bool m_bOn = false;

        public IODF.INPUT Input
        {
            get { return _Input; }
            set { _Input = value; }
        }

        public Color InputColorOn
        {
            get { return _InputColorOn; }
            set { _InputColorOn = value; }
        }

        public Color InputColorOff
        {
            get { return _InputColorOff; }
            set { _InputColorOff = value; }
        }

        public bool On
        {
            get { return m_bOn; }
            set
            {
                if (m_bOn != value)
                {
                    m_bOn = value;

                    this.BackColor = m_bOn ? _InputColorOn : _InputColorOff;
                }
            }
        }

        public int Mode
        {
            get;
            set;
        }

        public InputPanel()
        {
            this.Size = new Size(11, 13);
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        }
    }
}
