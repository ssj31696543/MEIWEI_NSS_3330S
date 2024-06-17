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
    public partial class UcVacuumAirView : UserControl
    {
        public UcVacuumAirView()
        {
            InitializeComponent();
        }
        void initDgv()
        {
            #region VAC BLOW LIST
            DataGridView dgvVac = dgvVacAir;
            dgvVac.AutoGenerateColumns = false;
            dgvVac.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvVac.Columns.Clear();

            dgvVac.AllowUserToAddRows = false;

            dgvVac.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvVac.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvVac.EnableHeadersVisualStyles = false;

            DataGridViewTextBoxColumn dgvcText = new DataGridViewTextBoxColumn();
            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "NAME";
            dgvcText.HeaderText = "NAME";
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvcText.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvVac.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "VAC_DELAY";
            dgvcText.HeaderText = "VAC_DELAY";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 120;
            dgvcText.ReadOnly = false;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvcText.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvVac.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "BLOW_DELAY";
            dgvcText.HeaderText = "BLOW_DELAY";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 120;
            dgvcText.ReadOnly = false;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvcText.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvVac.Columns.Add(dgvcText);

            dgvVac.Font = new Font("맑은 고딕", 10);
            dgvVac.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            for (int nIdx = 0; nIdx < (int)IODF.A_INPUT.MAX; nIdx++)
            {
                string strName = Enum.GetName(typeof(IODF.A_INPUT), nIdx);
                if (strName.Contains("VAC"))
                {
                    //strName = strName.Replace("_", " ");
                    //strName = strName.Replace("WORK", "워크");
                    //strName = strName.Replace("CHUCK", "척");
                    //strName = strName.Replace("PICKER", "칩 피커");
                    //strName = strName.Replace("VACUUM", "베큠");
                    //strName = strName.Replace("STG", "스테이지");
                    //strName = strName.Replace("DRY BLOCK", "드라이 블럭");
                    //strName = strName.Replace("MAP", "맵");
                    //strName = strName.Replace("LD", "로드");
                    //strName = strName.Replace("VAC", "베큠");
                    //strName = strName.Replace("PK", "피커");
                    //strName = strName.Replace(" AXIS ", "-");
                    //strName = strName.Replace("SENSOR", "센서");
                    //strName = strName.Replace("SECOND CLEANING ZONE KIT MATERIAL", "2차 세척");
                    //strName = strName.Replace("CLEAN", "클리너");
                    //strName = strName.Replace("SECOND ULTRASONIC ZONE KIT MATERIAL", "2차 초음파");
                    dgvVac.Rows.Add(strName, ConfigMgr.Inst.Cfg.Vac[nIdx].lVacOnDelay.ToString(), ConfigMgr.Inst.Cfg.Vac[nIdx].lBlowOnDelay.ToString());
                }
            }
            dgvVac.Refresh();
            #endregion
        }

        private void UcVacuumAirView_VisibleChanged(object sender, EventArgs e)
        {
            initDgv();
        }

        private void dgvVacAir_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dgvVacAir_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.ColumnIndex != 2 || e.RowIndex < 0) return;

            DataGridView dgv = sender as DataGridView;

            IODF.A_INPUT a_INPUT = 0;
            Enum.TryParse(dgvVacAir.Rows[e.RowIndex].Cells[0].Value.ToString(), out a_INPUT);
            int nItemIdx = (int)a_INPUT;

            object VacValue;
            object BlowValue;

            if (e.ColumnIndex == 1)
            {
                VacValue = dgvVacAir.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            }
            else if (e.ColumnIndex == 2)
            {
                BlowValue = dgvVacAir.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            }
            else
            {
                //잘못 누른듯
                return;
            }


            double dVal = 0.0;
            int nVal = 0;
            long lVal = 0;
            if (long.TryParse(dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out lVal))
            {
                //OptionItem item = ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx];
                if (e.ColumnIndex == 1)
                {
                    VacValue = dgvVacAir.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    ConfigMgr.Inst.TempCfg.Vac[nItemIdx].lVacOnDelay = lVal;
                }
                else if (e.ColumnIndex == 2)
                {
                    BlowValue = dgvVacAir.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    ConfigMgr.Inst.TempCfg.Vac[nItemIdx].lBlowOnDelay = lVal;
                }
                else
                {
                    //잘못 누른듯
                    return;
                }
            }
            else
            {
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].strValue;
            }
        }
    }
}
