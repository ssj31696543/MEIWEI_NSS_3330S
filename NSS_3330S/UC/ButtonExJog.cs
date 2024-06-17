using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.MOTION;

namespace NSS_3330S.UC
{
    [DefaultProperty("AXIS")]
    public class ButtonExJog : Button
    {
        public enum JOG_DIR
        {
            X,
            Y,
            Z,
            R
        }

        JOG_DIR m_dir = JOG_DIR.X;
        bool m_bMotorDirRev = false;
        bool m_bJogDirRev = false;

        public SVDF.AXES AXIS
        {
            get;
            set;
        }

        [RefreshProperties(RefreshProperties.All)]
        public JOG_DIR DIR
        {
            get
            {
                return m_dir;
            }
            set
            {
                m_dir = value;

                RefreshUI();
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public bool MOTOR_DIR_REVERSE
        {
            get
            {
                return m_bMotorDirRev;
            }
            set
            {
                m_bMotorDirRev = value;

                RefreshUI();
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public bool JOG_DIR_REVERSE
        {
            get
            {
                return m_bJogDirRev;
            }
            set
            {
                m_bJogDirRev = value;

                if (this.DIR == JOG_DIR.R)
                {
                    this.Text = m_bJogDirRev ? "CCW" : "CW";
                }
                else
                {
                    this.Text = m_bJogDirRev ? "─" : "┼";
                }

                RefreshUI();
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public bool AUTO_TEXT_ALIGN
        {
            get;
            set;
        }

        public ButtonExJog()
        {
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FlatAppearance.BorderColor = Color.Chocolate;
        }

        void RefreshUI()
        {
            switch (m_dir)
            {
                case JOG_DIR.X:
                    {
                        this.FlatAppearance.BorderColor = Color.Chocolate;
                        if (m_bMotorDirRev)
                        {
                            this.Image = NSS_3330S.Properties.Resources.icons8_left_20; //X_LEFT

                            if (AUTO_TEXT_ALIGN) this.TextAlign = ContentAlignment.MiddleLeft;
                        }
                        else
                        {
                            this.Image = NSS_3330S.Properties.Resources.icons8_right_20; //X_RIGHT
                            if (AUTO_TEXT_ALIGN) this.TextAlign = ContentAlignment.MiddleRight;
                        }
                    }
                    break;
                case JOG_DIR.Y:
                    {
                        this.FlatAppearance.BorderColor = Color.Chocolate;
                        if (m_bMotorDirRev)
                        {
                            this.Image = NSS_3330S.Properties.Resources.icons8_down_20; //Y_DOWN
                            if (AUTO_TEXT_ALIGN) this.TextAlign = ContentAlignment.BottomCenter;
                        }
                        else
                        {
                            this.Image = NSS_3330S.Properties.Resources.icons8_up_20; //Y_UP
                            if (AUTO_TEXT_ALIGN) this.TextAlign = ContentAlignment.TopCenter;
                        }
                    }
                    break;
                case JOG_DIR.Z:
                    {
                        this.FlatAppearance.BorderColor = Color.MidnightBlue;
                        if (m_bMotorDirRev)
                        {
                            this.Image = NSS_3330S.Properties.Resources.icons8_down_20_1; //Z_DOWN
                            if (AUTO_TEXT_ALIGN) this.TextAlign = ContentAlignment.BottomCenter;
                        }
                        else
                        {
                            this.Image = NSS_3330S.Properties.Resources.icons8_up_20_1; //Z_UP
                            if (AUTO_TEXT_ALIGN) this.TextAlign = ContentAlignment.TopCenter;
                        }
                    }
                    break;
                case JOG_DIR.R:
                    {
                        this.FlatAppearance.BorderColor = Color.MidnightBlue;
                        if (m_bMotorDirRev)
                        {
                            this.Image = NSS_3330S.Properties.Resources.icons8_rotate_left_20_1; //CCW
                            if (AUTO_TEXT_ALIGN) this.TextAlign = ContentAlignment.BottomCenter;
                        }
                        else
                        {
                            this.Image = NSS_3330S.Properties.Resources.icons8_rotate_right_20_1; //CW
                            if (AUTO_TEXT_ALIGN) this.TextAlign = ContentAlignment.TopCenter;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
