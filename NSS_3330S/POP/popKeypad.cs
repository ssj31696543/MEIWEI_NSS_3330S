using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DionesTool.UTIL;

namespace NSS_3330S.POP
{
    public partial class popKeypad : Form
    {
        #region Move Window Form Without Tilte Bar (드래그로 창 움직이기)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        private bool m_bIsInt = false;
        private double m_dValue = 0.0;
        private int m_nValue = 0;
        private int m_nDecimalPlace = 3;

        GlobalKeyboardHook ghKeyboard = new GlobalKeyboardHook();

        public popKeypad(string strInit, int nDecimalP = 3, bool bIsInt = false)
        {
            InitializeComponent();
            this.CenterToScreen();

            lbOrgValue.Text = strInit;
            m_bIsInt = bIsInt;
            m_nDecimalPlace = nDecimalP;

            //if (bIsInt == true)
            //{
            //    lbChangedValue.Text = "0";
            //}
            //else
            //{
            //    lbChangedValue.Text = "0.000";
            //}

            button10.Visible = !bIsInt;
        }

        private void btrnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        public string GET_VALUE
        {
            get
            {
                if (m_bIsInt == true)
                {
                    return m_nValue.ToString();
                }
                else
                {
                    string strRtnForamt = string.Format("F{0}", m_nDecimalPlace);
                    return m_dValue.ToString(strRtnForamt);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Button btnGet = sender as Button;
            string sTag = btnGet.Tag as string;

            int Idx = 0;
            if (int.TryParse(sTag, out Idx) == false) return;

            PressNumber(Idx);
        }

        private void PressNumber(int Idx)
        {
            if (Idx != 1000)
            {
                string strChkNum = lbChangedValue.Text;

                if (lbChangedValue.Text.Contains("+"))
                {
                    int nIndex = lbChangedValue.Text.LastIndexOf("+");
                    strChkNum = lbChangedValue.Text.Substring(nIndex, lbChangedValue.Text.Length - nIndex);
                    if (strChkNum.Contains("."))
                    {
                        nIndex = strChkNum.IndexOf(".");
                        strChkNum = strChkNum.Substring(nIndex, strChkNum.Length - nIndex);
                    }
                    if (strChkNum.Length > 7) return;
                }
                else if (lbChangedValue.Text.Contains("-"))
                {
                    int nIndex = lbChangedValue.Text.LastIndexOf("-");
                    strChkNum = lbChangedValue.Text.Substring(nIndex, lbChangedValue.Text.Length - nIndex);
                    if (strChkNum.Contains("."))
                    {
                        nIndex = strChkNum.IndexOf(".");
                        strChkNum = strChkNum.Substring(nIndex, strChkNum.Length - nIndex);
                    }
                    if (strChkNum.Length > 7) return;
                }
                else
                {
                    if (strChkNum.Contains("."))
                    {
                        int nIndex = strChkNum.IndexOf(".");
                        strChkNum = strChkNum.Substring(nIndex, strChkNum.Length - nIndex);
                    }

                    //1000번은 전체 클리어기 때문에 
                    if (strChkNum.Length > 7) return;
                }
            }

            if (Idx != 100 && Idx != 1000)
            {
                if (lbChangedValue.Text == "0" || lbChangedValue.Text == "-0" || lbChangedValue.Text == "+0")
                {
                    return;
                }
            }


            if (Idx == 100)
            {
                // 첫번째 일 경우 예외처리
                if (lbChangedValue.Text.Length == 0) return;

                if (lbChangedValue.Text.Length == 1)
                {
                    if (lbChangedValue.Text == "-" || lbChangedValue.Text == "+") return;
                }

                ////태그 100번이 . 표시임, 중복 방지 
                //string strContains = lbChangedValue.Text;
                //if (strContains.Contains(".")) return;
            }

            if (Idx > 10)
            {
                if (Idx == 100)
                {
                    if (lbChangedValue.Text.Substring(lbChangedValue.Text.Length - 1, 1) != ".")
                    {
                        lbChangedValue.Text += ".";
                    }
                }
                if (Idx == 200)
                {
                    //연산 시도
                    TryCalc();

                    lbChangedValue.Text += "+";
                }
                if (Idx == 300)
                {
                    //연산 시도
                    TryCalc();

                    lbChangedValue.Text += "-";
                }
                if (Idx == 1000) lbChangedValue.Text = "";
                return;
            }

            if (Idx == 0)
            {

                //                 if (lbValue.Text.Length == 1)
                //                 {
                //                     return;
                //                 }
            }

            lbChangedValue.Text += Idx;
            btnEnter.Focus();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Calc();
        }

        private void Calc()
        {
            int nValue = 0;
            double dvalue = 0.0;

            //연산 시도
            TryCalc();

            if (m_bIsInt == true)
            {
                if (int.TryParse(lbChangedValue.Text, out nValue) == false)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.No;
                    return;
                }
            }
            else
            {
                if (double.TryParse(lbChangedValue.Text, out dvalue) == false)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.No;
                    return;
                }
            }


            if (lbChangedValue.Text.Length == 0)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.No;
                return;
            }

            if (lbChangedValue.Text.Length == 0)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.No;
                return;
            }


            m_nValue = nValue;
            m_dValue = dvalue;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void popKeypad_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                ghKeyboard = new GlobalKeyboardHook();
                ghKeyboard.unhook();
                ghKeyboard.hook();
                ghKeyboard.HookedKeys.Clear();

                ghKeyboard.HookedKeys.Add(Keys.D0);
                ghKeyboard.HookedKeys.Add(Keys.D1);
                ghKeyboard.HookedKeys.Add(Keys.D2);
                ghKeyboard.HookedKeys.Add(Keys.D3);
                ghKeyboard.HookedKeys.Add(Keys.D4);
                ghKeyboard.HookedKeys.Add(Keys.D5);
                ghKeyboard.HookedKeys.Add(Keys.D6);
                ghKeyboard.HookedKeys.Add(Keys.D7);
                ghKeyboard.HookedKeys.Add(Keys.D8);
                ghKeyboard.HookedKeys.Add(Keys.D9);

                ghKeyboard.HookedKeys.Add(Keys.NumPad0);
                ghKeyboard.HookedKeys.Add(Keys.NumPad1);
                ghKeyboard.HookedKeys.Add(Keys.NumPad2);
                ghKeyboard.HookedKeys.Add(Keys.NumPad3);
                ghKeyboard.HookedKeys.Add(Keys.NumPad4);
                ghKeyboard.HookedKeys.Add(Keys.NumPad5);
                ghKeyboard.HookedKeys.Add(Keys.NumPad6);
                ghKeyboard.HookedKeys.Add(Keys.NumPad7);
                ghKeyboard.HookedKeys.Add(Keys.NumPad8);
                ghKeyboard.HookedKeys.Add(Keys.NumPad9);

                ghKeyboard.HookedKeys.Add(Keys.OemPeriod);
                ghKeyboard.HookedKeys.Add(Keys.Decimal);
                ghKeyboard.HookedKeys.Add(Keys.Oemplus);
                ghKeyboard.HookedKeys.Add(Keys.Add);
                ghKeyboard.HookedKeys.Add(Keys.OemMinus);
                ghKeyboard.HookedKeys.Add(Keys.Subtract);

                ghKeyboard.HookedKeys.Add(Keys.Delete);
                ghKeyboard.HookedKeys.Add(Keys.Back);

                ghKeyboard.KeyDown += new KeyEventHandler(gkh_KeyDown);
                ghKeyboard.KeyUp += new KeyEventHandler(gkh_KeyUp);
            }
            else
            {
                ghKeyboard.unhook();
                ghKeyboard.KeyDown -= new KeyEventHandler(gkh_KeyDown);
                ghKeyboard.KeyUp -= new KeyEventHandler(gkh_KeyUp);

                ghKeyboard.HookedKeys.Clear();
            }
        }

        void gkh_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                case Keys.NumPad0:
                    {
                        PressNumber(0);
                    }
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    {
                        PressNumber(1);
                    }
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    {
                        PressNumber(2);
                    }
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    {
                        PressNumber(3);
                    }
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    {
                        PressNumber(4);
                    }
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    {
                        PressNumber(5);
                    }
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    {
                        PressNumber(6);
                    }
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    {
                        PressNumber(7);
                    }
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    {
                        PressNumber(8);
                    }
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    {
                        PressNumber(9);
                    }
                    break;
                case Keys.OemPeriod:
                case Keys.Decimal:
                    {
                        PressNumber(100);
                    }
                    break;
                case Keys.Oemplus:
                case Keys.Add:
                    {
                        PressNumber(200);
                    }
                    break;
                case Keys.OemMinus:
                case Keys.Subtract:
                    {
                        PressNumber(300);
                    }
                    break;
                case Keys.Delete:
                    {
                        PressNumber(1000);
                    }
                    break;
                case Keys.Back:
                    {
                        if (lbChangedValue.Text.Length <= 0) return;
                        lbChangedValue.Text = lbChangedValue.Text.Substring(0, lbChangedValue.Text.Length - 1);
                    }
                    break;
                default:
                    break;
            }
            e.Handled = true;
        }

        public double CalcPlus(string strOrg)
        {
            int nDivideIndex = strOrg.IndexOf("+");
            double dNum1 = 0.0;
            double dNum2 = 0.0;
            double.TryParse(strOrg.Substring(0, nDivideIndex), out dNum1);
            double.TryParse(strOrg.Substring(nDivideIndex, strOrg.Length - nDivideIndex), out dNum2);

            return Math.Round(dNum1 + dNum2, 4);
        }

        public double CalcMinus(string strOrg)
        {
            int nDivideIndex = strOrg.LastIndexOf("-");
            double dNum1 = 0.0;
            double dNum2 = 0.0;
            double.TryParse(strOrg.Substring(0, nDivideIndex), out dNum1);
            double.TryParse(strOrg.Substring(nDivideIndex, strOrg.Length - nDivideIndex), out dNum2);

            return Math.Round(dNum1 + dNum2, 4);
        }

        public void TryCalc()
        {
            // 연산이 가능하다면 연산한다.
            if (lbChangedValue.Text.Contains("+"))
            {
                lbChangedValue.Text = CalcPlus(lbChangedValue.Text).ToString();
            }

            // 연산이 가능하다면 연산한다.
            if (lbChangedValue.Text.Contains("-"))
            {
                lbChangedValue.Text = CalcMinus(lbChangedValue.Text).ToString();
            }
        }

        private void lbOrgValue_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void popKeypad_Load(object sender, EventArgs e)
        {

        }
    }
}
