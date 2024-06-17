using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public class DataGridViewEx : DataGridView
    {
        List<DataGridViewCell> m_listSelectedCell = new List<DataGridViewCell>();

        // Column Header를 눌러 전체 선택할 때는 SelectionChangedEx 이벤트를 발생시키지 않는다
        bool m_bSuspendSelection = false;

        public event DataGridViewCellEventHandler CellEndEditEx;
        public event EventHandler SelectionChangedEx;

        /// <summary>
        /// 여러 셀을 선택 후 셀의 내용을 편집하면 선택된 모든 셀들을 같은 값으로 적용하는 모드
        /// </summary>
        public bool AUTO_SELECTED_CELL_EDIT_MODE { get; set; }

        public DataGridViewEx()
        {
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AllowUserToOrderColumns = false;
            this.AllowUserToResizeRows = false;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.RowHeadersVisible = false;

            this.CellBeginEdit += DataGridViewEx_CellBeginEdit;
            this.CellEndEdit += DataGridViewEx_CellEndEdit;
            this.ColumnHeaderMouseClick += DataGridViewEx_ColumnHeaderMouseClick;
            this.SelectionChanged += DataGridViewEx_SelectionChanged;

            AUTO_SELECTED_CELL_EDIT_MODE = true;
        }

        ~DataGridViewEx()
        {
            this.CellBeginEdit -= DataGridViewEx_CellBeginEdit;
            this.CellEndEdit -= DataGridViewEx_CellEndEdit;
            this.ColumnHeaderMouseClick -= DataGridViewEx_ColumnHeaderMouseClick;
            this.SelectionChanged -= DataGridViewEx_SelectionChanged;
        }

        void DataGridViewEx_SelectionChanged(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (dgv.SelectedCells.Count <= 0)
                return;

            if (m_bSuspendSelection)
                return;

            if (SelectionChangedEx != null)
                SelectionChangedEx(sender, e);
        }

        void DataGridViewEx_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            try
            {
                if (dgv.Rows.Count > 0)
                {
                    dgv.CurrentCell = dgv.Rows[0].Cells[e.ColumnIndex];
                }

                m_bSuspendSelection = true;
                dgv.ClearSelection();
                for (int i = 0; i < dgv.Rows.Count; ++i)
                {
                    dgv.Rows[i].Cells[e.ColumnIndex].Selected = true;
                }
                m_bSuspendSelection = false;

                this.OnSelectionChanged(new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        void DataGridViewEx_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            object obj = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            Type type = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].ValueType;

            if (AUTO_SELECTED_CELL_EDIT_MODE)
            {
                foreach (DataGridViewCell dgvc in m_listSelectedCell)
                {
                    if (!dgvc.ReadOnly && type == dgvc.ValueType)
                    {
                        dgvc.Value = obj;
                    }
                }
            }

            m_listSelectedCell.Clear();

            if (CellEndEditEx != null)
                CellEndEditEx(sender, e);
        }

        void DataGridViewEx_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            m_listSelectedCell.Clear();
            foreach (DataGridViewCell dgvc in this.SelectedCells)
            {
                m_listSelectedCell.Add(dgvc);
            }
        }
    }
}
