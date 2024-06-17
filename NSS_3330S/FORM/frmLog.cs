using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using DionesTool.UTIL;
using SQLiteDB;
using NSS_3330S.POP;

namespace NSS_3330S.FORM
{
    public partial class frmLog : Form
    {
        int m_nLogSeqId = (int)DBDF.LOG_TYPE.SEQ_LD_MZ_LD_CONV;
        int m_nLogIFId = (int)DBDF.LOG_TYPE.IF_SAW;

        LogProperties[] m_DB_LOG_SEQ_INFO = null;
        LogProperties[] m_DB_LOG_IF_INFO = null;
        EventProperties[] m_DB_LOG_EVENT_INFO = null;
        TactTimeProperties[] m_DB_LOG_TACTTIME_INFO = null;
        MccInfo[] m_DB_LOG_MCC_INFO = null;

        BindingList<LogProperties> m_blistSeqLogItem = new BindingList<LogProperties>();
        BindingList<EventProperties> m_blistEventLogItem = new BindingList<EventProperties>();
        BindingList<LogProperties> m_blistIfLogItem = new BindingList<LogProperties>();
        BindingList<LotProperties> m_blistLot = new BindingList<LotProperties>();
        BindingList<StripProperties> m_blistStrip = new BindingList<StripProperties>();

        BindingList<TactTimeProperties> m_blistTactTimeLogItem = new BindingList<TactTimeProperties>();
        BindingList<MccInfo> m_blistMCCLogItem = new BindingList<MccInfo>();
        BindingList<ITS_LOG> m_blistITSLogItem = new BindingList<ITS_LOG>();

        DateTime dtStartTime = DateTime.Now;

        Random rd = new Random();


        Timer timer = new Timer();

        int m_nLogTypeNum = 0;
        bool m_bIsLogSave = false;
        string m_sFileSavePath = "";
        string m_sFileSaveDir = "";

        public frmLog()
        {
            InitializeComponent();
            Initdgv();
            tabLog.TabPages.Remove(tabTracking);
            for (DBDF.LOG_TYPE i = 0; i < DBDF.LOG_TYPE.MAX; i++)
            {
                if (i.ToString().Contains("SEQ"))
                    cbbSeqList.Items.Add(i.ToString());
                if (i.ToString().Contains("IF"))
                    cbbIFList.Items.Add(i.ToString());
            }

            dtpSeqStartDate.Value = DateTime.Now.AddDays(-1);
            dtpSeqEndDate.Value = DateTime.Now;
            dtpIFStartDate.Value = DateTime.Now.AddDays(-1);
            dtpIFEndDate.Value = DateTime.Now;
            dtpEventStartDate.Value = DateTime.Now.AddDays(-1);
            dtpEventEndDate.Value = DateTime.Now;
            dtpTrkStartDate.Value = DateTime.Now.AddDays(-1);
            dtpTrkEndDate.Value = DateTime.Now;
        }

        void Initdgv()
        {
            #region SEQ
            DataGridView dgvSeq = dgvSeqLog;
            dgvSeq.AutoGenerateColumns = false;
            dgvSeq.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvSeq.Columns.Clear();

            dgvSeq.AllowUserToAddRows = false;

            dgvSeq.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvSeq.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvSeq.EnableHeadersVisualStyles = false;

            DataGridViewTextBoxColumn dgvcText = null;
            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "TIME";
            dgvcText.HeaderText = "TIME";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 150;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Frozen = false;
            dgvSeq.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "MODEL";
            dgvcText.HeaderText = "MODEL";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 130;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvSeq.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "LEVEL";
            dgvcText.HeaderText = "LEVEL";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 100;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvSeq.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "USER_ID";
            dgvcText.HeaderText = "ID";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 70;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvSeq.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "STRIP_ID";
            dgvcText.HeaderText = "STRIP ID";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 1;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvSeq.Columns.Add(dgvcText);


            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "LOG";
            dgvcText.HeaderText = "LOG";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvSeq.Columns.Add(dgvcText);

            dgvSeq.Font = new Font("맑은 고딕", 9);
            dgvSeq.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSeq.DataSource = m_blistSeqLogItem;
            #endregion

            #region IF
            DataGridView dgvIF = dgvIFLog;
            dgvIF.AutoGenerateColumns = false;
            dgvIF.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvIF.Columns.Clear();

            dgvIF.AllowUserToAddRows = false;

            dgvIF.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvIF.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvIF.EnableHeadersVisualStyles = false;

            dgvcText = null;
            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "TIME";
            dgvcText.HeaderText = "TIME";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 150;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Frozen = false;
            dgvIF.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "MODEL";
            dgvcText.HeaderText = "MODEL";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 130;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvIF.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "LEVEL";
            dgvcText.HeaderText = "LEVEL";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 100;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvIF.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "USER_ID";
            dgvcText.HeaderText = "USER_ID";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 70;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvIF.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "CELL_ID";
            dgvcText.HeaderText = "CELL_ID";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 1;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvIF.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "LOG";
            dgvcText.HeaderText = "LOG";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvIF.Columns.Add(dgvcText);

            dgvIF.Font = new Font("맑은 고딕", 9);
            dgvIF.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvIF.DataSource = m_blistIfLogItem;
            #endregion

            #region EVENT
            DataGridView dgvEvnet = dgvEventLog;
            dgvEvnet.AutoGenerateColumns = false;
            dgvEvnet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvEvnet.Columns.Clear();

            dgvEvnet.AllowUserToAddRows = false;

            dgvEvnet.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvEvnet.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvEvnet.EnableHeadersVisualStyles = false;

            dgvcText = null;
            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "TIME";
            dgvcText.HeaderText = "TIME";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 150;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Frozen = false;
            dgvEvnet.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "SECTION";
            dgvcText.HeaderText = "SECTION";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 250;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvEvnet.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "DETAIL";
            dgvcText.HeaderText = "DETAIL";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvEvnet.Columns.Add(dgvcText);

            dgvEvnet.Font = new Font("맑은 고딕", 9);
            dgvEvnet.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvEvnet.DataSource = m_blistEventLogItem;
            #endregion

            #region LOTLIST
            DataGridView dgvLot = dgvLotList;
            dgvLot.AutoGenerateColumns = false;
            dgvLot.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvLot.Columns.Clear();

            dgvLot.AllowUserToAddRows = false;

            dgvLot.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvLot.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvLot.EnableHeadersVisualStyles = false;

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Move_in_time.ToString();
            dgvcText.HeaderText = "TIME";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 200;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Frozen = false;
            dgvLot.Columns.Add(dgvcText);

            #region Visiable area
            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Move_out_time.ToString();
            dgvcText.HeaderText = "QTY";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Lot_id.ToString();
            dgvcText.HeaderText = "ID";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Cycle_time.ToString();
            dgvcText.HeaderText = "CYCLE TIME";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 180;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvLot.Columns.Add(dgvcText);
            #endregion

            #region Unvisiable area
            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Carrier_id.ToString();
            dgvcText.HeaderText = "QTY";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Pkg_prod_count.ToString();
            dgvcText.HeaderText = "QTY";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Strip_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Eqp_sq_chip_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Eqp_nq_chip_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Eqp_rj_chip_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Eqp_sq_tray_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Eqp_nq_tray_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Sat_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Total_chip_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Mark_vision_err_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Ball_vision_err_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.X_out_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Host_sq_chip_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Host_nq_chip_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Host_rj_chip_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Process_err_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Strip_cutting_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = LotProperties.ColName.Unit_picker_place_count.ToString();
            dgvcText.HeaderText = "수량";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Visible = false;
            dgvLot.Columns.Add(dgvcText);
            #endregion

            dgvLot.Font = new Font("맑은 고딕", 10);
            dgvLot.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvLot.DataSource = m_blistLot;
            #endregion

            #region LOTINFO
            DataGridView dgvlotInfo = dgvLotInfo;
            dgvlotInfo.AutoGenerateColumns = false;
            dgvlotInfo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvlotInfo.Columns.Clear();

            dgvlotInfo.AllowUserToAddRows = false;

            dgvlotInfo.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvlotInfo.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvlotInfo.EnableHeadersVisualStyles = false;

            dgvcText = null;
            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "Items";
            dgvcText.HeaderText = "Items";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 300;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Frozen = false;
            dgvlotInfo.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "Content";
            dgvcText.HeaderText = "Content";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvlotInfo.Columns.Add(dgvcText);

            #region ItemList
            dgvlotInfo.Rows.Add("MOVE IN TIME");
            dgvlotInfo.Rows.Add("MOVE OUT TIME");
            dgvlotInfo.Rows.Add("LOT ID");
            dgvlotInfo.Rows.Add("CYCLE TIME");
            dgvlotInfo.Rows.Add("CARRIER ID");
            dgvlotInfo.Rows.Add("PROCESSEDPACKAGE QTY(LOT)");
            dgvlotInfo.Rows.Add("STRIP QTY(HOST INFO)");
            dgvlotInfo.Rows.Add("GD MAT QTY");
            dgvlotInfo.Rows.Add("RW MAT QTY");
            dgvlotInfo.Rows.Add("NG MAT QTY");
            dgvlotInfo.Rows.Add("GD TRAY QTY");
            dgvlotInfo.Rows.Add("RW TRAY QTY");
            dgvlotInfo.Rows.Add("SAT QTY");
            dgvlotInfo.Rows.Add("TOTAL MAT QTY(LOT)");
            dgvlotInfo.Rows.Add("TOP VISION NG");
            dgvlotInfo.Rows.Add("BTM VISION NG");
            dgvlotInfo.Rows.Add("X-OUT");
            dgvlotInfo.Rows.Add("HOST SQ MAT QTY");
            dgvlotInfo.Rows.Add("HOST NQ MAT QTY");
            dgvlotInfo.Rows.Add("HOST RJ MAT QTY");
            dgvlotInfo.Rows.Add("PROCESSED UNIT QTY");
            dgvlotInfo.Rows.Add("CUT STRIP QTY");
            dgvlotInfo.Rows.Add("PLACED UNIT QTY");
            #endregion

            dgvlotInfo.Font = new Font("맑은 고딕", 10);
            dgvlotInfo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;



            #endregion

            #region STRIPLIST
            DataGridView dgvStrip = dgvStripList;
            dgvStrip.AutoGenerateColumns = false;
            dgvStrip.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvStrip.Columns.Clear();

            dgvStrip.AllowUserToAddRows = false;

            dgvStrip.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvStrip.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvStrip.EnableHeadersVisualStyles = false;

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = StripProperties.ColName.Time.ToString();
            dgvcText.HeaderText = "TIME";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 150;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Frozen = false;
            dgvStrip.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = StripProperties.ColName.LotId.ToString();
            dgvcText.HeaderText = "LOT ID";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 130;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvcText.Frozen = false;
            dgvStrip.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = StripProperties.ColName.StripId.ToString();
            dgvcText.HeaderText = "ID";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvStrip.Columns.Add(dgvcText);

            dgvStrip.Font = new Font("맑은 고딕", 10);
            dgvStrip.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvStrip.DataSource = m_blistStrip;
            #endregion

        }

        private void btnSeqSearch_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dt1 = new DateTime(dtpSeqStartDate.Value.Year, dtpSeqStartDate.Value.Month, dtpSeqStartDate.Value.Day, 00, 00, 0, 0);
                DateTime dt2 = new DateTime(dtpSeqEndDate.Value.Year, dtpSeqEndDate.Value.Month, dtpSeqEndDate.Value.Day, 23, 59, 59, 999);

                if (dt1.Date > DateTime.Now || dt2.Date > DateTime.Now)
                {
                    MessageBox.Show("미래날짜의 로그는 조회하실 수 없습니다.");
                    return;
                }
                if ((dt2.Date - dt1.Date).Days < 0)
                {
                    MessageBox.Show("조회 기간의 시작 날짜는 끝 날짜보다 클 수 없습니다.");
                    return;
                }

                //m_DB_LOG_SEQ_INFO = GbVar.dbLogData[m_nLogSeqId].SelectDataInRange(dt1, dt2);
                dgvSeqLog.DataSource = GbVar.dbLogData[m_nLogSeqId].DataTableSelectDataInRange(dt1, dt2);
                GbVar.g_bUpdateLogData[m_nLogSeqId] = true;
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }


        private void btnIFSearch_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dt1 = new DateTime(dtpIFStartDate.Value.Year, dtpIFStartDate.Value.Month, dtpIFStartDate.Value.Day, 0, 0, 0, 0);
                DateTime dt2 = new DateTime(dtpIFEndDate.Value.Year, dtpIFEndDate.Value.Month, dtpIFEndDate.Value.Day, 23, 59, 59, 999);

                if (dt1.Date > DateTime.Now || dt2.Date > DateTime.Now)
                {
                    MessageBox.Show("미래날짜의 로그는 조회하실 수 없습니다.");
                    return;
                }
                if ((dt2.Date - dt1.Date).Days < 0)
                {
                    MessageBox.Show("조회 기간의 시작 날짜는 끝 날짜보다 클 수 없습니다.");
                    return;
                }

                //m_DB_LOG_IF_INFO = GbVar.dbLogData[m_nLogIFId].SelectDataInRange(dt1, dt2);
                dgvIFLog.DataSource = GbVar.dbLogData[m_nLogIFId].DataTableSelectDataInRange(dt1, dt2);

                GbVar.g_bUpdateLogData[m_nLogIFId] = true;
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }


        }

        private void btnEventSearch_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dt1 = new DateTime(dtpEventStartDate.Value.Year, dtpEventStartDate.Value.Month, dtpEventStartDate.Value.Day, 0, 0, 0, 0);
                DateTime dt2 = new DateTime(dtpEventEndDate.Value.Year, dtpEventEndDate.Value.Month, dtpEventEndDate.Value.Day, 23, 59, 59, 999);

                if (dt1.Date > DateTime.Now || dt2.Date > DateTime.Now)
                {
                    MessageBox.Show("미래날짜의 로그는 조회하실 수 없습니다.");
                    return;
                }
                if ((dt2.Date - dt1.Date).Days < 0)
                {
                    MessageBox.Show("조회 기간의 시작 날짜는 끝 날짜보다 클 수 없습니다.");
                    return;
                }

                //m_DB_LOG_EVENT_INFO = GbVar.dbEventData.SelectEventDataInRange(dt1, dt2); DataTableSelectEventDataInRange
                dgvEventLog.DataSource = GbVar.dbEventLog2.DataTableSelectDataInRange(dt1, dt2); 
                GbVar.g_bUpdateEventData = true;
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void btnTrkSearch_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dt1 = new DateTime(dtpTrkStartDate.Value.Year, dtpTrkStartDate.Value.Month, dtpTrkStartDate.Value.Day, 0, 0, 0, 0);
                DateTime dt2 = new DateTime(dtpTrkEndDate.Value.Year, dtpTrkEndDate.Value.Month, dtpTrkEndDate.Value.Day, 23, 59, 59, 999);

                if (dt1.Date > DateTime.Now || dt2.Date > DateTime.Now)
                {
                    MessageBox.Show("미래날짜의 로그는 조회하실 수 없습니다.");
                    return;
                }
                if ((dt2.Date - dt1.Date).Days < 0)
                {
                    MessageBox.Show("조회 기간의 시작 날짜는 끝 날짜보다 클 수 없습니다.");
                    return;
                }
                string strLotId = tbxLotID.Text;

                dgvLotList.DataSource = GbVar.dbLotLog.DataTableSelectDataInRange(dt1, dt2, strLotId);
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (GbVar.g_bUpdateLogData[m_nLogSeqId] == true)
            {
                GbVar.g_bUpdateLogData[m_nLogSeqId] = false;
                SeqUpdateData();
            }

            if (GbVar.g_bUpdateLogData[m_nLogIFId] == true)
            {
                GbVar.g_bUpdateLogData[m_nLogIFId] = false;
                IFUpdateData();
            }

            if (GbVar.g_bUpdateEventData == true)
            {
                GbVar.g_bUpdateEventData = false;
                EventUpdateData();
            }
        }

        private void btnKeyWordSetting_Click(object sender, EventArgs e)
        {
            PopLogKeyWordSetting popup = new PopLogKeyWordSetting();
            if (popup.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GbVar.g_bUpdateLogData[m_nLogSeqId] = true;
                GbVar.g_bUpdateLogData[m_nLogIFId] = true;
                GbVar.g_bUpdateEventData = true;

                popup.Close();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView[] dgv = { dgvSeqLog, dgvIFLog};
                BindingList<LogProperties>[] m_Log = { m_blistSeqLogItem, m_blistIfLogItem };
                Button btn = sender as Button;
                int nTag = 0;

                string strFilePath = "log";
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = @"D:\LOG\USER_SAVE";   // 기본 폴더
                dialog.Filter = "csv Files | *.csv";
                dialog.CheckPathExists = true;   // 폴더 존재여부확인

                if (!Directory.Exists(dialog.InitialDirectory))
                {
                    Directory.CreateDirectory(dialog.InitialDirectory);
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    strFilePath = dialog.FileName;
                    DataTable dt = new DataTable();
                    dt = Util.ConvertCSVtoDataTable(strFilePath);

                    LogProperties[] colInfo = new LogProperties[dt.Rows.Count];

                    int nCnt = 0;
                    if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;

                    m_Log[nTag].Clear();

                    foreach (DataRow item in dt.Rows)
                    {
                        colInfo[nCnt] = new LogProperties();
                        colInfo[nCnt].Time = item[0].ToString().Replace("\"", "");
                        colInfo[nCnt].Model = item[1].ToString().Replace("\"", "");
                        colInfo[nCnt].Level = item[2].ToString().Replace("\"", "");
                        colInfo[nCnt].User_Id = item[3].ToString().Replace("\"", "");
                        colInfo[nCnt].Cell_Id = item[4].ToString().Replace("\"", "");
                        colInfo[nCnt].Log = item[5].ToString().Replace("\"", "");

                        m_Log[nTag].Add(colInfo[nCnt++]);
                    }

                    if (nTag == 0) UpdateKeyWord(dgv[nTag]);
                    dgv[nTag].Invalidate();
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                DataGridView[] dgv = { dgvSeqLog, dgvEventLog, dgvIFLog };
                Button btn = sender as Button;
                int nTag = 0;

                string today = DateTime.Now.ToString("yyyyMMdd");
                dialog.InitialDirectory = @"D:\LOG\USER_SAVE\" + today;   // 기본 폴더
                dialog.Filter = // 필터설정
                    "csv Files | *.csv";
                dialog.FileName = "LOG_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"); // 초기 파일명
                //dialog.CheckFileExists = true;   // 파일 존재여부확인
                dialog.CheckPathExists = true;   // 폴더 존재여부확인

                if (!Directory.Exists(dialog.InitialDirectory))
                {
                    Directory.CreateDirectory(dialog.InitialDirectory);
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string strPath = dialog.FileName;
                    if (!int.TryParse(btn.Tag.ToString(), out nTag)) nTag = 0;
                    for (int i = 0; i < dgv[nTag].RowCount; i++)
                    {
                        if (dgv[nTag].Rows[i].Cells.Count>5)
                        {
                            if (dgv[nTag].Rows[i].Cells[4].Value == null || dgv[nTag].Rows[i].Cells[4].Value.ToString() == "")
                                dgv[nTag].Rows[i].Cells[4].Value = "-";
                        }
                    }
                    

                    if (Util.DataGridViewToCSV(dgv[nTag], strPath, true))
                        System.Diagnostics.Process.Start(strPath);

                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void cbbList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox control = sender as ComboBox;

                string text = control.SelectedItem.ToString();
                var Value = Enum.Parse(typeof(DBDF.LOG_TYPE), text);

                if (Value != null)
                {
                    if (text.ToString().Contains("SEQ"))
                    {
                        m_nLogSeqId = (int)Value;
                    }
                    else
                    {
                        m_nLogIFId = (int)Value;
                    }
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());

            }

        }

        private void UpdateKeyWord(DataGridView dgv)
        {
            try
            {
                foreach (var item in m_blistSeqLogItem.Select((data, index) => new { data, index }))
                {
                    foreach (var Dic in LogKeyWordMgr.KeyWordDic)
                    {
                        if (Dic.Value.Usage == false)
                            continue;
                        if (item.data.Log.Contains(Dic.Key))
                            dgv.Rows[item.index].Cells["Log"].Style.BackColor = Color.FromArgb(Dic.Value.Color);
                    }
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void SeqUpdateData()
        {
            try
            {
                //if (m_DB_LOG_SEQ_INFO == null) return;

                //m_blistSeqLogItem.Clear();
                //string strtime = "";
                //for (int i = 0; i < m_DB_LOG_SEQ_INFO.Length; i++)
                //{
                //    LogProperties Target = new LogProperties();
                //    strtime = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                //        m_DB_LOG_SEQ_INFO[i].Time.Substring(0, 4),
                //        m_DB_LOG_SEQ_INFO[i].Time.Substring(4, 2),
                //        m_DB_LOG_SEQ_INFO[i].Time.Substring(6, 2),
                //        m_DB_LOG_SEQ_INFO[i].Time.Substring(8, 2),
                //        m_DB_LOG_SEQ_INFO[i].Time.Substring(10, 2),
                //        m_DB_LOG_SEQ_INFO[i].Time.Substring(12, 2),
                //        m_DB_LOG_SEQ_INFO[i].Time.Substring(14));

                //    Target.Time = strtime;
                //    Target.Model = m_DB_LOG_SEQ_INFO[i].Model;
                //    Target.Level = m_DB_LOG_SEQ_INFO[i].Level;
                //    Target.Cell_Id = m_DB_LOG_SEQ_INFO[i].Cell_Id;
                //    Target.User_Id = m_DB_LOG_SEQ_INFO[i].User_Id;
                //    Target.Log = m_DB_LOG_SEQ_INFO[i].Log;

                //    m_blistSeqLogItem.Add(Target);
                //}

                UpdateKeyWord(dgvSeqLog);
                dgvSeqLog.Invalidate();
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }

        }

        private void IFUpdateData()
        {
            try
            {
                //if (m_DB_LOG_IF_INFO == null) return;

                //m_blistIfLogItem.Clear();
                //string strtime = "";
                //for (int i = 0; i < m_DB_LOG_IF_INFO.Length; i++)
                //{
                //    LogProperties Target = new LogProperties();
                //    strtime = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                //            m_DB_LOG_IF_INFO[i].Time.Substring(0, 4),
                //            m_DB_LOG_IF_INFO[i].Time.Substring(4, 2),
                //            m_DB_LOG_IF_INFO[i].Time.Substring(6, 2),
                //            m_DB_LOG_IF_INFO[i].Time.Substring(8, 2),
                //            m_DB_LOG_IF_INFO[i].Time.Substring(10, 2),
                //            m_DB_LOG_IF_INFO[i].Time.Substring(12, 2),
                //            m_DB_LOG_IF_INFO[i].Time.Substring(14));
                //    Target.Time = strtime;
                //    Target.Model = m_DB_LOG_IF_INFO[i].Model;
                //    Target.Level = m_DB_LOG_IF_INFO[i].Level;
                //    Target.Cell_Id = m_DB_LOG_IF_INFO[i].Cell_Id;
                //    Target.User_Id = m_DB_LOG_IF_INFO[i].User_Id;
                //    Target.Log = m_DB_LOG_IF_INFO[i].Log;

                //    m_blistIfLogItem.Add(Target);
                //}

                dgvIFLog.Invalidate();
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }

        }

        private void EventUpdateData()
        {
            try
            {
                //if (m_DB_LOG_EVENT_INFO == null) return;

                //m_blistEventLogItem.Clear();
                //string strtime = "";
                //for (int i = 0; i < m_DB_LOG_EVENT_INFO.Length; i++)
                //{
                //    EventProperties Target = new EventProperties();
                //    strtime = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                //            m_DB_LOG_EVENT_INFO[i].Time.Substring(0, 4),
                //            m_DB_LOG_EVENT_INFO[i].Time.Substring(4, 2),
                //            m_DB_LOG_EVENT_INFO[i].Time.Substring(6, 2),
                //            m_DB_LOG_EVENT_INFO[i].Time.Substring(8, 2),
                //            m_DB_LOG_EVENT_INFO[i].Time.Substring(10, 2),
                //            m_DB_LOG_EVENT_INFO[i].Time.Substring(12, 2),
                //            m_DB_LOG_EVENT_INFO[i].Time.Substring(14));
                //    Target.Time = strtime;
                //    Target.Section = m_DB_LOG_EVENT_INFO[i].Section;
                //    Target.Detail = m_DB_LOG_EVENT_INFO[i].Detail;

                //    m_blistEventLogItem.Add(Target);
                //}

                dgvEventLog.Invalidate();
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            LogProperties m_colInfo = new LogProperties();


            int m_nSeqID = 2;
            int nCurrentSeqNo = 1;
            int m_nSeqNo = 1;
            string strNow = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string strHistory = "TEST LOG";
            for (int nIdx = 0; nIdx < 10000; nIdx++)
            {
                GbFunc.WriteProcLog(m_nSeqID, nCurrentSeqNo, m_nSeqNo, strNow, "", strHistory);
            }

            //int nRandom = rd.Next(1000000, 10000000);
            //string[] strMsg = { "servo ", "complete ", "moving ", "receive ", "prepass ", "3223123 ", "1234 " };
            //string[] strCellID = { "qwerty ", "asdfgh ", "zxcvbn ", "poiuyt ", "lkjhgf ", "mnbvcx " };
            //string MODEL_NAME = "HSR_DFGH1";
            //string LEVEL = "MASTER";
            //string USER_ID = "USER_ID";
            //string[] LOG = { " NG", " OK", " REWORK", "" };

            //SQLiteBase.LogProperties m_colInfo = new SQLiteBase.LogProperties();
            ////m_dbInfo.LOG_TIME = DateTime.Now.ToString();

            //for (DBDF.LOG_TYPE i = 0; i < DBDF.LOG_TYPE.MAX; i++)
            //{
            //    for (int j = 0; j < 100; j++)
            //    {
            //        m_colInfo.Time = string.Format("2021{0}{1}{2}{3}{4}{5}", rd.Next(1, 12).ToString("00"), rd.Next(1, 30).ToString("00"), rd.Next(24).ToString("00"), rd.Next(59).ToString("00"), rd.Next(59).ToString("00"), rd.Next(999).ToString("000"));
            //        m_colInfo.Model = MODEL_NAME;
            //        m_colInfo.Cell_Id = strCellID[rd.Next(0, 5)];
            //        m_colInfo.Level = LEVEL;
            //        m_colInfo.User_Id = USER_ID;
            //        m_colInfo.Log = nRandom + strMsg[rd.Next(4)] + strMsg[rd.Next(4)] + strMsg[rd.Next(4)] + LOG[rd.Next(4)];

            //        GbVar.dbLogData[(int)i].Enqueue_InsertDB(DBDF.LogType[(int)i], m_colInfo);
            //    }
            //}
        }


        private void tabLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tabCtr = sender as TabControl;

            foreach (TabPage tabpage in tabCtr.TabPages)
            {
                tabpage.ImageIndex = 0;
            }

            tabCtr.SelectedTab.ImageIndex = 1;
        }

        private void dgvSeqLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex != this.dgvSeqLog.NewRowIndex)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        string strDate = e.Value.ToString();
                        DateTime dateTime;
                        try
                        {
                            dateTime = DateTime.ParseExact(strDate, "yyyyMMddHHmmssfff", null);
                            e.Value = dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        }
                        catch
                        {
                            e.Value = strDate;
                        }
                        break;

                    case 5:
                        try
                        {
                            foreach (var Dic in LogKeyWordMgr.KeyWordDic)
                            {
                                if (Dic.Value.Usage == false)
                                    continue;
                                if (e.Value.ToString().Contains(Dic.Key))
                                    e.CellStyle.BackColor = Color.FromArgb(Dic.Value.Color);
                            }
                        }
                        catch
                        {
                            e.CellStyle.BackColor = Color.White;
                        }

                        break;
                }
            }
        }

        private void dgvEventLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex != this.dgvSeqLog.NewRowIndex)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        string strDate = e.Value.ToString();
                        DateTime dateTime;
                        try
                        {
                            dateTime = DateTime.ParseExact(strDate, "yyyyMMddHHmmssfff", null);
                            e.Value = dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        }
                        catch
                        {
                            e.Value = strDate;
                        }
                        break;
                    case 2:
                        try
                        {
                            foreach (var Dic in LogKeyWordMgr.KeyWordDic)
                            {
                                if (Dic.Value.Usage == false)
                                    continue;
                                if (e.Value.ToString().Contains(Dic.Key))
                                    e.CellStyle.BackColor = Color.FromArgb(Dic.Value.Color);
                            }
                        }
                        catch
                        {
                            e.CellStyle.BackColor = Color.White;
                        }

                        break;
                }
            }
        }

        private void dgvIFLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex != this.dgvSeqLog.NewRowIndex)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        string strDate = e.Value.ToString();
                        DateTime dateTime;
                        try
                        {
                            dateTime = DateTime.ParseExact(strDate, "yyyyMMddHHmmssfff", null);
                            e.Value = dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        }
                        catch
                        {
                            e.Value = strDate;
                        }
                        break;
                    case 5:
                        try
                        {
                            foreach (var Dic in LogKeyWordMgr.KeyWordDic)
                            {
                                if (Dic.Value.Usage == false)
                                    continue;
                                if (e.Value.ToString().Contains(Dic.Key))
                                    e.CellStyle.BackColor = Color.FromArgb(Dic.Value.Color);
                            }
                        }
                        catch
                        {
                            e.CellStyle.BackColor = Color.White;
                        }

                        break;
                }
            }
        }

        private void dgvMccLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex != this.dgvSeqLog.NewRowIndex)
            {
                switch (e.ColumnIndex)
                {
                    case 1:
                        if (e.Value == null) break;
                        string strDate = e.Value.ToString();
                        DateTime dateTime;
                        try
                        {
                            dateTime = DateTime.ParseExact(strDate, "yyyyMMddHHmmssff", null);
                            e.Value = dateTime.ToString("yyyy/MM/dd HH:mm:ss.ff");
                        }
                        catch
                        {
                            e.Value = strDate;
                        }
                        break;
                }
            }
        }

        private void dgvLotList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex != this.dgvSeqLog.NewRowIndex)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        if (e.Value == null) break;
                        string strDate = e.Value.ToString();
                        DateTime dateTime;
                        try
                        {
                            dateTime = DateTime.ParseExact(strDate, "yyyyMMddHHmmssff", null);
                            e.Value = dateTime.ToString("yyyy년MM월dd일 HH시mm분ss초");
                        }
                        catch
                        {
                            e.Value = strDate;
                        }
                        break;
                }
            }
        }

        private void dgvStripList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex != this.dgvSeqLog.NewRowIndex)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        if (e.Value == null) break;
                        string strDate = e.Value.ToString();
                        DateTime dateTime;
                        try
                        {
                            dateTime = DateTime.ParseExact(strDate, "yyyyMMddHHmmssff", null);
                            e.Value = dateTime.ToString("yyyy-MM-dd HH:mm:ss.ff");
                        }
                        catch
                        {
                            e.Value = strDate;
                        }
                        break;
                }
            }
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            tabLot.SelectedIndex = 0;
        }

        private void dgvLotList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            tabLot.SelectedIndex = 1;

            DataGridView dgv = sender as DataGridView;

            if (dgv.SelectedRows[0] == null) return;

            if (dgv.SelectedRows[0].Cells.Count != dgvLotInfo.Rows.Count) return;

            for (int i = 0; i < dgv.SelectedRows[0].Cells.Count; i++)
            {
                dgvLotInfo.Rows[i].Cells[1].Value = dgv.SelectedRows[0].Cells[i].Value;
            }
        }

        private void dgvLotList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv.SelectedRows[0] == null) return;

            string strLotId = dgv.SelectedRows[0].Cells[(int)LotProperties.ColName.Lot_id].Value.ToString();
            if (strLotId == null) return;

            dgvStripList.DataSource = GbVar.dbStripLog.SelectLotStrip(strLotId);
        }

        private void dgvItsLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex != this.dgvSeqLog.NewRowIndex)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        if (e.Value == null) break;
                        string strDate = e.Value.ToString();
                        DateTime dateTime;
                        try
                        {
                            dateTime = DateTime.ParseExact(strDate, "yyyyMMddHHmmss", null);
                            e.Value = dateTime.ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        catch
                        {
                            e.Value = strDate;
                        }
                        break;
                }
            }
        }

        private void btnDebugLog_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            DataGridView[] dgv = { dgvSeqLog, dgvEventLog, dgvIFLog };
            Button btn = sender as Button;
            int nTag = 0;

            string today = DateTime.Now.ToString("yyyyMMdd");
            dialog.InitialDirectory = @"D:\LOG\USER_SAVE\" + today;   // 기본 폴더
            dialog.Filter = // 필터설정
                "csv Files | *.csv";
            dialog.FileName = "LOG_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"); // 초기 파일명
                                                                                 //dialog.CheckFileExists = true;   // 파일 존재여부확인
            dialog.CheckPathExists = true;   // 폴더 존재여부확인

            if (!Directory.Exists(dialog.InitialDirectory))
            {
                Directory.CreateDirectory(dialog.InitialDirectory);
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                m_nLogTypeNum = 0;
                m_sFileSavePath = dialog.FileName.Substring(0, dialog.FileName.Length - 4);

                tmrDebugLogSearch.Enabled = true;
                MessageBox.Show("SEQUENCE LOG SAVE START", "SEQUENCE LOG SAVE START", MessageBoxButtons.OK);
            }
        }

        private void tmrDebugLogSave_Tick(object sender, EventArgs e)
        {
            string strPath = m_sFileSavePath + string.Format("_{0}.csv", m_nLogTypeNum);
            for (int i = 0; i < dgvSeqLog.RowCount; i++)
            {
                if (dgvSeqLog.Rows[i].Cells.Count > 5)
                {
                    if (dgvSeqLog.Rows[i].Cells[4].Value == null || dgvSeqLog.Rows[i].Cells[4].Value.ToString() == "")
                        dgvSeqLog.Rows[i].Cells[4].Value = "-";
                }
            }
            Util.DataGridViewToCSV(dgvSeqLog, strPath, true);

            m_nLogTypeNum++;
            tmrDebugLogSave.Enabled = false;
            tmrDebugLogSearch.Enabled = true;
        }

        private void tmrDebugLogSearch_Tick(object sender, EventArgs e)
        {
            if (m_nLogTypeNum >= GbVar.dbLogData.Length)
            {
                tmrDebugLogSave.Enabled = false;
                tmrDebugLogSearch.Enabled = false;
                MessageBox.Show("SEQUENCE LOG SAVE FINISH", "SEQUENCE LOG SAVE FINISH", MessageBoxButtons.OK);
                return;
            }

            try
            {
                DateTime dt1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour - 2, 00, 0, 0);

                dgvSeqLog.DataSource = GbVar.dbLogData[m_nLogTypeNum].DataTableSelectDataInRange(dt1, DateTime.Now);
                GbVar.g_bUpdateLogData[m_nLogTypeNum] = true;
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
            tmrDebugLogSearch.Enabled = false;
            tmrDebugLogSave.Enabled = true;
        }
    }
}
