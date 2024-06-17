using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S
{
    /// <summary>
    /// UI 추가 기능 클래스 (KMLee 2018.07.13)
    /// </summary>
    public static class UIExtension
    {
        /// <summary>
        /// ListView의 해당 Row로 Focus를 맞춘다
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="nRow"></param>
        public static void FocusRow(this ListView lv, int nRow)
        {
            if (nRow < 0 || nRow >= lv.Items.Count) return;

            //lv.Items[nRow].Selected = true;
            //lv.Items[nRow].Focused = true;
            //lv.Focus();
            lv.EnsureVisible(nRow);
        }

        /// <summary>
        /// ListView의 마지막 Row로 Focus를 맞춘다
        /// </summary>
        /// <param name="lv"></param>
        public static void FocusLast(this ListView lv)
        {
            FocusRow(lv, lv.Items.Count - 1);
        }

        /// <summary>
        /// DataGridView의 해당 Row로 Focus를 맞춘다
        /// 검증 전
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="nRow"></param>
        public static void FocusRow(this DataGridView dgv, int nRow)
        {
            if (nRow < 0 || nRow > dgv.Rows.Count - 1) return;

            // 선택하게 할 때 다른 선택된 cell을 해제
            foreach (DataGridViewCell cell in dgv.SelectedCells)
                cell.Selected = false;

            //dgv.FirstDisplayedCell = dgv.Rows[nRow].Cells[0];
            //dgv.CurrentCell = dgv.Rows[nRow].Cells[0];
            dgv.Rows[nRow].Selected = true;
            dgv.FirstDisplayedScrollingRowIndex = nRow;
        }

        /// <summary>
        /// DataGridView의 마지막 Row로 Focus를 맞춘다
        /// 검증 전
        /// </summary>
        /// <param name="dgv"></param>
        public static void FocusLast(this DataGridView dgv)
        {
            FocusRow(dgv, dgv.Rows.Count - 1);
        }

        /// <summary>
        /// Button의 Tag 값을 int로 가져온다
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int GetTag(this Button btn)
        {
            int nTag = -1;

            if (btn.Tag == null) return -1;
            string tag = (string)btn.Tag;

            if (!int.TryParse(tag, out nTag)) return -1;

            return nTag;
        }

        public static int GetTag(this TextBox tbx)
        {
            int nTag = -1;

            if (tbx.Tag == null) return -1;
            string tag = (string)tbx.Tag;

            if (!int.TryParse(tag, out nTag)) return -1;

            return nTag;
        }

        public static int GetTag(this NumericUpDown num)
        {
            int nTag = -1;

            if (num.Tag == null) return -1;
            string tag = (string)num.Tag;

            if (!int.TryParse(tag, out nTag)) return -1;

            return nTag;
        }

        public static int GetTag(this CheckBox cbx)
        {
            int nTag = -1;

            if (cbx.Tag == null) return -1;
            string tag = (string)cbx.Tag;

            if (!int.TryParse(tag, out nTag)) return -1;

            return nTag;
        }

        public static int GetTag(this ComboBox cbbx)
        {
            int nTag = -1;

            if (cbbx.Tag == null) return -1;
            string tag = (string)cbbx.Tag;

            if (!int.TryParse(tag, out nTag)) return -1;

            return nTag;
        }

        public static int GetTag(this DateTimePicker dtp)
        {
            int nTag = -1;

            if (dtp.Tag == null) return -1;
            string tag = (string)dtp.Tag;

            if (!int.TryParse(tag, out nTag)) return -1;

            return nTag;
        }

        public static int GetTag(this RadioButton rdo)
        {
            int nTag = -1;

            if (rdo.Tag == null) return -1;
            string tag = (string)rdo.Tag;

            if (!int.TryParse(tag, out nTag)) return -1;

            return nTag;
        }

        public static int GetTag(this Label lbl)
        {
            int nTag = -1;

            if (lbl.Tag == null) return -1;
            string tag = (string)lbl.Tag;

            if (!int.TryParse(tag, out nTag)) return -1;

            return nTag;
        }
    }
}
