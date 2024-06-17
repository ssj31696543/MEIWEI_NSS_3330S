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
    public partial class PopLogKeyWordSetting : Form
    {
        public PopLogKeyWordSetting()
        {
            InitializeComponent();
            this.CenterToScreen();

            initGrid();
        }
        private void PopLogKeyWordSetting_Load(object sender, EventArgs e)
        {
            LogKeyWordMgr.KeyWordDic = LogKeyWordMgr.Read(LogKeyWordMgr.LogPaths);
            UpdateData();
        }

        /// <summary>
        /// datagridview init
        /// </summary>
        private void initGrid()
        {
            DataGridViewTextBoxColumn dgvCol1 = new DataGridViewTextBoxColumn();
            dgvCol1.HeaderText = "키워드";
            dgvCol1.Width = 200;
            dgvList.Columns.Add(dgvCol1);

            DataGridViewTextBoxColumn dgvCol2 = new DataGridViewTextBoxColumn();
            dgvCol2.HeaderText = "색깔";
            dgvCol2.Width = 80;
            dgvCol2.ReadOnly = true;
            dgvList.Columns.Add(dgvCol2);

            //DataGridViewCheckBoxColumn dgvCheckCol1 = new DataGridViewCheckBoxColumn();
            //dgvCheckCol1.HeaderText = "FIND";
            //dgvCheckCol1.Width = 50;
            //dgvList.Columns.Add(dgvCheckCol1);

            DataGridViewCheckBoxColumn dgvCheckCol2 = new DataGridViewCheckBoxColumn();
            dgvCheckCol2.HeaderText = "사용여부";
            dgvCheckCol2.Width = 50;
            dgvList.Columns.Add(dgvCheckCol2);

            dgvList.Font = new Font("맑은 고딕", 9); 
        }

        public void UpdateData()
        {
            try
            {
                if (LogKeyWordMgr.KeyWordDic.Count > 0)
                {
                    foreach (var item in LogKeyWordMgr.KeyWordDic)
                    {
                        //dgvList.Rows.Add(item.Key, "", item.Value.Find, item.Value.Usage);
                        dgvList.Rows.Add(item.Key, "", item.Value.Usage);
                        dgvList.Rows[dgvList.RowCount - 1].Cells[1].Style.BackColor = Color.FromArgb(item.Value.Color);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                //dgvList.Rows.Add("", "", false, true);
                dgvList.Rows.Add("", "", true);
            }
            catch (Exception)
            {
            }   
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvList.SelectedCells.Count > 0)
                {
                    int nIndex = dgvList.SelectedCells[0].RowIndex;
                    dgvList.Rows.RemoveAt(nIndex);
                }
            }
            catch (Exception)
            {
            }  
        }

        private void dgvColorList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvList.SelectedCells.Count > 0)
                {
                    int nIndex;
                    nIndex = dgvList.SelectedCells[0].ColumnIndex;

                    if (nIndex == 1)
                    {
                        ColorDialog cd = new ColorDialog();
                        if (DialogResult.OK == cd.ShowDialog())
                        {
                            Color color = cd.Color;
                            dgvList.SelectedCells[0].Style.BackColor = color;
                        }
                        else
                        {
                            dgvList.SelectedCells[0].Style.BackColor = Color.Transparent;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {

                LogKeyWordMgr.KeyWordDic.Clear();
                for (int i = 0; i < dgvList.Rows.Count; i++)
                {
                    //LogKeyWordMgr.KeyWordDic.Add
                    //(
                    //    dgvList.Rows[i].Cells[0].Value.ToString(),
                    //    new Property(dgvList.Rows[i].Cells[1].Style.BackColor.ToArgb(),
                    //                    (Boolean)dgvList.Rows[i].Cells[2].Value,
                    //                    (Boolean)dgvList.Rows[i].Cells[3].Value)
                    //);
                    LogKeyWordMgr.KeyWordDic.Add
                    (
                        dgvList.Rows[i].Cells[0].Value.ToString(),
                        new Property(
                            dgvList.Rows[i].Cells[1].Style.BackColor.ToArgb(),
                            (Boolean)dgvList.Rows[i].Cells[2].Value
                                     )
                    );
                }
                LogKeyWordMgr.Write(LogKeyWordMgr.LogPaths);
            }
            catch (Exception)
            {
            }
        }
    }
}
