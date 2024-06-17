using DionesTool.UTIL;
using NSS_3330S.POP;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NSS_3330S.FORM
{
    public partial class frmAlarm : Form
    {
        int nCurCnt = 1;
        Random rd = new Random();
        ErrorInfo errInfo;

        BindingList<AlarmProperties> m_blistAlarmLog = new BindingList<AlarmProperties>();
        AlarmProperties[] m_DB_LOG_ALARM = null;

        BindingList<AlarmProperties> m_blistAlarmStatus = new BindingList<AlarmProperties>();
        AlarmProperties[] m_AlarmStatus = null;

        BindingList<ErrorInfo> m_listErrorDef = new BindingList<ErrorInfo>();
        //ErrorInfo[] m_ErrorDef = new ErrorInfo[(int)ERDF.ERR_CNT];

        BindingList<AlarmFreqProperties> m_blistAlarmFreq = new BindingList<AlarmFreqProperties>();
        AlarmFreqProperties[] m_AlarmFreq = null;

        AlarmProperties m_colInfo = new AlarmProperties();

        DateTime dt1;
        DateTime dt2;

        public frmAlarm()
        {
            InitializeComponent();

            errInfo = new ErrorInfo();
            ptgErrOption.PropertySort = PropertySort.NoSort;
            ptgErrOption.SelectedObject = errInfo;
            ErrMgr.Inst.Init();

            initcbb();
            Initdgv();

            dtpStartDate.Value = DateTime.Now.AddDays(-1);
            dtpEndDate.Value = DateTime.Now;
        }

        public void initcbb()
        {
            foreach (ErrorInfo.BUZZERLIST item in Enum.GetValues(typeof(ErrorInfo.BUZZERLIST)))
            {
                cbbBuzzer.Items.Add(item);
                cbbBatchBuzzer.Items.Add(item);
            }

            cbbBatchHeavy.Items.Add(FormTextLangMgr.FindKey("경알람"));
            cbbBatchHeavy.Items.Add(FormTextLangMgr.FindKey("중알람"));

            cbbBuzzer.SelectedItem = 0;
            cbbBatchBuzzer.SelectedItem = 0;
            cbbBatchHeavy.SelectedItem = 0;

            cbbBuzzer.MouseWheel += Combobox_MouseWheel;
            cbbBatchBuzzer.MouseWheel += Combobox_MouseWheel;
            cbbBatchHeavy.MouseWheel += Combobox_MouseWheel;
        }

        public void Initdgv()
        {
            #region ERDF LIST
            DataGridView dgvError = dgvErrorList;
            dgvError.AutoGenerateColumns = false;
            dgvError.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvError.Columns.Clear();

            dgvError.AllowUserToAddRows = false;

            dgvError.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvError.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvError.EnableHeadersVisualStyles = false;

            DataGridViewTextBoxColumn dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "NO";
            //dgvcText.HeaderText = "번호";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("번호");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 100;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvcText.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvError.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "NAME";
            //dgvcText.HeaderText = "알람명";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("알람명");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvcText.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvError.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "CAUSE";
            // dgvcText.HeaderText = "원인";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("원인");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvcText.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvcText.Visible = false;
            dgvError.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "SOLUTION";
            //dgvcText.HeaderText = "해결방안";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("해결방안");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 5;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvcText.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvcText.Visible = false;
            dgvError.Columns.Add(dgvcText);

            DataGridViewCheckBoxColumn dgvcCheck = new DataGridViewCheckBoxColumn();
            dgvcCheck.DataPropertyName = "HEAVY";
            //dgvcCheck.HeaderText = "중알람";
            dgvcCheck.HeaderText = FormTextLangMgr.FindKey("중알람");
            dgvcCheck.Name = dgvcText.DataPropertyName;
            dgvcCheck.Width = 70;
            dgvcCheck.ReadOnly = true;
            dgvcCheck.ValueType = typeof(string);
            dgvcCheck.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcCheck.Frozen = false;
            dgvcCheck.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvError.Columns.Add(dgvcCheck);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "BUZZER";
            //dgvcText.HeaderText = "부저";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("부저");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 100;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvcText.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvError.Columns.Add(dgvcText);


            dgvError.Font = new Font("맑은 고딕", 10);
            dgvError.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            m_listErrorDef.Clear();
            for (int i = 0; i < ErrMgr.Inst.errlist.Count; i++)
            {
                m_listErrorDef.Add(ErrMgr.Inst.errlist[i]);
            }

            dgvErrorList.DataSource = m_listErrorDef;
            #endregion

            #region CURR ERROR
            DataGridView dgvLog = dgvAlarmStatus;
            dgvLog.AutoGenerateColumns = false;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvLog.Columns.Clear();

            dgvLog.AllowUserToAddRows = false;

            dgvLog.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvLog.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvLog.EnableHeadersVisualStyles = false;

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "TIME";
            //dgvcText.HeaderText ="시간";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("시간");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 210;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvLog.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "NO";
            //dgvcText.HeaderText = "번호";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("번호");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 100;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvLog.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "NAME";
            //dgvcText.HeaderText = "알람명";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("알람명");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvLog.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "CAUSE";
            //dgvcText.HeaderText = "발생원인";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("발생원인");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvLog.Columns.Add(dgvcText);

            dgvLog.Font = new Font("맑은 고딕", 10);
            dgvLog.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvLog.DataSource = m_blistAlarmStatus;
            #endregion

            #region ERROR LOG
            dgvLog = dgvAlarmLog;
            dgvLog.AutoGenerateColumns = false;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvLog.Columns.Clear();

            dgvLog.AllowUserToAddRows = false;

            dgvLog.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvLog.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvLog.EnableHeadersVisualStyles = false;

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "TIME";
            //dgvcText.HeaderText = "시간";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("시간");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 200;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvLog.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "NO";
            //dgvcText.HeaderText = "번호";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("번호");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 100;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvLog.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "NAME";
            //dgvcText.HeaderText = "알람명";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("알람명");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvLog.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "CAUSE";
            //dgvcText.HeaderText = "발생원인";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("발생원인");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvLog.Columns.Add(dgvcText);

            dgvLog.Font = new Font("맑은 고딕", 10);
            dgvLog.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvLog.DataSource = m_blistAlarmLog;
            #endregion

            #region ERROR FREQ LIST
            DataGridView dgvFreq = dgvAlarmFreq;
            dgvFreq.AutoGenerateColumns = false;
            dgvFreq.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvFreq.Columns.Clear();

            dgvFreq.AllowUserToAddRows = false;

            dgvFreq.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dgvFreq.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvFreq.EnableHeadersVisualStyles = false;

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "RANKING";
            //dgvcText.HeaderText = "순위";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("순위");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 50;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvFreq.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "NAME";
            //dgvcText.HeaderText = "알람명";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("알람명");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 280;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvFreq.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "COUNT";
            //dgvcText.HeaderText = "횟수";
            dgvcText.HeaderText = FormTextLangMgr.FindKey("횟수");
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.Width = 50;
            dgvcText.ReadOnly = true;
            dgvcText.ValueType = typeof(string);
            dgvcText.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvcText.Frozen = false;
            dgvFreq.Columns.Add(dgvcText);

            dgvFreq.Font = new Font("맑은 고딕", 10);
            dgvFreq.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvFreq.DataSource = m_blistAlarmFreq;
            #endregion
        }

        public void addGridCurAlarm(string strDateTime, int nErr, string strAlarm)
        {
            m_colInfo = new AlarmProperties();
            m_colInfo.Time = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                                            strDateTime.Substring(0, 4),
                                            strDateTime.Substring(4, 2),
                                            strDateTime.Substring(6, 2),
                                            strDateTime.Substring(8, 2),
                                            strDateTime.Substring(10, 2),
                                            strDateTime.Substring(12, 2),
                                            strDateTime.Substring(14));
            m_colInfo.No = nErr.ToString();
            m_colInfo.Name = strAlarm;
            m_colInfo.Cause = ErrMgr.Inst.Get(nErr).CAUSE;

            //220505 PJH 알람 이력이 이중으로 기록되어 삭제
            //GbVar.dbAlarmData.Enqueue_InsertDB(m_colInfo);

            m_blistAlarmStatus.Add(m_colInfo);
        }

        private void btnSelectAlarmClear_Click(object sender, EventArgs e)
        {
            try
            {
                popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("선택한 알람을 삭제하시겠습니까?"), "주의");
                msg.TopMost = true;
                if (msg.ShowDialog(this) != DialogResult.OK) return;

                m_blistAlarmStatus.RemoveAt(dgvAlarmStatus.CurrentCell.RowIndex);
                dgvAlarmStatus.Invalidate();
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void btnAllClear_Click(object sender, EventArgs e)
        {
            try
            {
                popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("모든 알람을 삭제하시겠습니까?"), "주의");
                msg.TopMost = true;
                if (msg.ShowDialog(this) != DialogResult.OK) return;

                m_blistAlarmStatus.Clear();
                dgvAlarmStatus.Invalidate();
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }


        private void Combobox_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void ckbHeavy_CheckedChange(object sender, EventArgs e)
        {
            CheckBox Ctr = sender as CheckBox;

            if (Ctr.Checked == true)
                Ctr.ImageIndex = 1;
            else
                Ctr.ImageIndex = 0;

            int nIndex = dgvErrorList.CurrentCell.RowIndex;//선택된 셀의 번호     
            m_listErrorDef[nIndex].HEAVY = ckbHeavy.Checked;
        }


        private void txbCause_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox ctr = sender as TextBox;

            int nIndex = dgvErrorList.CurrentCell.RowIndex;//선택된 셀의 번호     
            if (ctr.Tag == "CAUSE")
            {
                m_listErrorDef[nIndex].CAUSE = ctr.Text;
            }
            else
            {
                m_listErrorDef[nIndex].SOLUTION = ctr.Text;
            }
        }

        private void cbbBuzzer_DropDownClosed(object sender, EventArgs e)
        {
            int nIndex = dgvErrorList.CurrentCell.RowIndex;//선택된 셀의 번호     
            m_listErrorDef[nIndex].BUZZER = cbbBuzzer.Text;
        }

        private void dgvErrorList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int nIndex = dgvErrorList.CurrentCell.RowIndex;//선택된 셀의 번호         
                string strKey = dgvErrorList.Rows[nIndex].Cells[1].Value.ToString();
                lbSelectedErr.Text = string.Format("[{0}] {1}", nIndex, strKey);

                txbCause.Text = m_listErrorDef[nIndex].CAUSE;
                txbSolution.Text = m_listErrorDef[nIndex].SOLUTION;
                ckbHeavy.Checked = m_listErrorDef[nIndex].HEAVY;
                cbbBuzzer.Text = m_listErrorDef[nIndex].BUZZER;
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }


        private void dgvErrorList_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyData.Equals(Keys.Down))
                {
                    int nIndex = dgvErrorList.CurrentCell.RowIndex + 1;//선택된 셀의 번호         
                    if (nIndex >= dgvErrorList.Rows.Count) return;
                    string strKey = dgvErrorList.Rows[nIndex].Cells[1].Value.ToString();
                    lbSelectedErr.Text = string.Format("[{0}] {1}", nIndex, strKey);

                    txbCause.Text = m_listErrorDef[nIndex].CAUSE;
                    txbSolution.Text = m_listErrorDef[nIndex].SOLUTION;
                    ckbHeavy.Checked = m_listErrorDef[nIndex].HEAVY;
                    cbbBuzzer.Text = m_listErrorDef[nIndex].BUZZER;
                }
                else if (e.KeyData.Equals(Keys.Up))
                {
                    int nIndex = dgvErrorList.CurrentCell.RowIndex - 1;//선택된 셀의 번호         
                    if (nIndex < 0) return;
                    string strKey = dgvErrorList.Rows[nIndex].Cells[1].Value.ToString();
                    lbSelectedErr.Text = string.Format("[{0}] {1}", nIndex, strKey);

                    txbCause.Text = m_listErrorDef[nIndex].CAUSE;
                    txbSolution.Text = m_listErrorDef[nIndex].SOLUTION;
                    ckbHeavy.Checked = m_listErrorDef[nIndex].HEAVY;
                    cbbBuzzer.Text = m_listErrorDef[nIndex].BUZZER;
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void ptgErrOption_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {
                int nIndex = dgvErrorList.CurrentCell.RowIndex;//선택된 셀의 번호     

                ErrMgr.Inst.errlist[nIndex].BUZZER = errInfo.BUZZER;
                ErrMgr.Inst.errlist[nIndex].NAME = errInfo.NAME;
                ErrMgr.Inst.errlist[nIndex].CAUSE = errInfo.CAUSE;
                ErrMgr.Inst.errlist[nIndex].SOLUTION = errInfo.SOLUTION;
                ErrMgr.Inst.errlist[nIndex].HEAVY = errInfo.HEAVY;
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Do you want to save the alarm list?"), "SAVE");
            msg.TopMost = true;
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            for (int i = 0; i < ErrMgr.Inst.errlist.Count; i++)
            {
                ErrMgr.Inst.errlist[i] = m_listErrorDef[i];
            }

            ErrMgr.Inst.Save();
            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "Alarm List Save Complete");
        }

        private void btnBuzzerSet_Click(object sender, EventArgs e)
        {
            bool bHeavy = cbbBatchHeavy.SelectedItem.ToString() == "중알람" ? true : false;


            foreach (var item in m_listErrorDef)
            {
                if (item.HEAVY == bHeavy)
                {
                    item.BUZZER = cbbBatchBuzzer.Text;
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                dt1 = new DateTime(dtpStartDate.Value.Year, dtpStartDate.Value.Month, dtpStartDate.Value.Day, 00, 00, 0, 0);
                dt2 = new DateTime(dtpEndDate.Value.Year, dtpEndDate.Value.Month, dtpEndDate.Value.Day, 23, 59, 59, 999);

                if (dt1.Date > DateTime.Now || dt2.Date > DateTime.Now)
                {
                    MessageBox.Show(FormTextLangMgr.FindKey("미래날짜의 로그는 조회하실 수 없습니다."));
                    return;
                }
                if ((dt2.Date - dt1.Date).Days < 0)
                {
                    MessageBox.Show(FormTextLangMgr.FindKey("조회 기간의 시작 날짜는 끝 날짜보다 클 수 없습니다."));
                    return;
                }
                m_DB_LOG_ALARM = GbVar.dbAlarmLog.SelectDataInRange(dt1, dt2);

                GbVar.g_bUpdateAlarmData = true;
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (GbVar.g_bUpdateAlarmData == true)
            {
                GbVar.g_bUpdateAlarmData = false;
                UpdateAlarmHistory();
                UpdateAlarmData();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
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

                    AlarmProperties[] colInfo = new AlarmProperties[dt.Rows.Count];

                    int nCnt = 0;
                    m_blistAlarmLog.Clear();

                    foreach (DataRow item in dt.Rows)
                    {
                        colInfo[nCnt] = new AlarmProperties();
                        colInfo[nCnt].Time = item[0].ToString().Replace("\"", "");
                        colInfo[nCnt].No = item[1].ToString().Replace("\"", "");
                        colInfo[nCnt].Name = item[2].ToString().Replace("\"", "");

                        m_blistAlarmLog.Add(colInfo[nCnt++]);
                    }

                    dgvAlarmLog.Invalidate();
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
                    if (Util.DataGridViewToCSV(dgvAlarmLog, strPath, true))
                        System.Diagnostics.Process.Start(strPath);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }


        private void UpdateAlarmHistory()
        {
            try
            {
                if (m_DB_LOG_ALARM == null) return;

                m_blistAlarmLog.Clear();
                string strtime = "";

                lbAlarmCnt.Text = (m_DB_LOG_ALARM.Length + 1).ToString();

                for (int i = 0; i < m_DB_LOG_ALARM.Length; i++)
                {
                    AlarmProperties Target = new AlarmProperties();
                    strtime = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                        m_DB_LOG_ALARM[i].Time.Substring(0, 4),
                        m_DB_LOG_ALARM[i].Time.Substring(4, 2),
                        m_DB_LOG_ALARM[i].Time.Substring(6, 2),
                        m_DB_LOG_ALARM[i].Time.Substring(8, 2),
                        m_DB_LOG_ALARM[i].Time.Substring(10, 2),
                        m_DB_LOG_ALARM[i].Time.Substring(12, 2),
                        m_DB_LOG_ALARM[i].Time.Substring(14));
                    Target.Time = strtime;
                    Target.No = m_DB_LOG_ALARM[i].No;
                    Target.Name = m_DB_LOG_ALARM[i].Name;
                    Target.Cause = m_DB_LOG_ALARM[i].Cause;

                    m_blistAlarmLog.Add(Target);
                }
                dgvAlarmLog.Invalidate();
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }

        }

        private void UpdateAlarmData()
        {
            try
            {
                #region DAILY CHART
                DataTable dt = new DataTable();
                DataRow dr = null;
                dt = GbVar.dbAlarmLog.GroupByDate(dt1, dt2);
                crtAlarmDaily.Series[0].Points.Clear();
                crtAlarmDaily.Series[1].Points.Clear();
                if (dt.Rows.Count <= 0) return;

                foreach (DataRow item in dt.Rows)
                {
                    if (int.Parse(item[1].ToString()) > 0)
                    {
                        crtAlarmDaily.Series[0].Points.AddXY(item[0].ToString().Substring(0, 8), item[1]);
                        crtAlarmDaily.Series[1].Points.AddXY(item[0].ToString().Substring(0, 8), item[1]);
                    }
                }
                #endregion

                #region FREQ CHART LIST
                dt = new DataTable();
                dt = GbVar.dbAlarmLog.GroupByCol(dt1, dt2, "No");

                crtAlarmFreq.Series[0].Points.Clear();
                m_blistAlarmFreq.Clear();

                if (dt.Rows.Count <= 0) return;
                //m_AlarmFreq = new AlarmFreqProperties[dt.Rows.Count];

                int nErrCnt = 0;
                int nErrNo = 0;
                for (int nCnt = 0; nCnt < dt.Rows.Count; nCnt++)
                {
                    dr = dt.Rows[nCnt];

                    if (!int.TryParse(dr[0].ToString(), out nErrNo)) return;
                    if (!int.TryParse(dr[1].ToString(), out nErrCnt)) return;
                    if (nCnt > 4) break;
                    if (nErrCnt < 1) continue;

                    string strErrName = nErrNo.ToString();
                    if (Enum.IsDefined(typeof(ERDF), nErrNo))
                        strErrName = ((ERDF)nErrNo).ToString();

                    crtAlarmFreq.Series[0].Points.AddXY(nErrNo, nErrCnt);
                    crtAlarmFreq.Series[0].Points[crtAlarmFreq.Series[0].Points.Count - 1].LegendText = strErrName + " | " + nErrCnt + "회";

                    //Freq List 표시 부분...공간이 부족하여 표시 안함....
                    //m_AlarmFreq[nCnt] = new AlarmFreqProperties();
                    //m_AlarmFreq[nCnt].Ranking = nCnt + 1;
                    //m_AlarmFreq[nCnt].Count = nErrCnt;

                    //if (Enum.IsDefined(typeof(ERDF), nErrNo))
                    //{
                    //    m_AlarmFreq[nCnt].Name = ((ERDF)nErrNo).ToString();
                    //}
                    //else
                    //{
                    //    m_AlarmFreq[nCnt].Name = nErrNo.ToString();
                    //}

                    //m_blistAlarmFreq.Add(m_AlarmFreq[nCnt]);
                }

                //dgvAlarmFreq.Invalidate();
                #endregion
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int nRandom = 0;
            AlarmProperties m_colInfo = new AlarmProperties();


            for (int j = 0; j < 1000; j++)
            {
                nRandom = rd.Next(0, (int)ERDF.ERR_CNT);
                if (Enum.IsDefined(typeof(ERDF), nRandom))
                {
                    m_colInfo.Time = string.Format("2021{0}{1}{2}{3}{4}{5}", rd.Next(1, 12).ToString("00"), rd.Next(1, 30).ToString("00"), rd.Next(24).ToString("00"), rd.Next(59).ToString("00"), rd.Next(59).ToString("00"), rd.Next(999).ToString("000"));
                    m_colInfo.No = nRandom.ToString();
                    m_colInfo.Name = ((ERDF)nRandom).ToString();
                    m_colInfo.Cause = ErrMgr.Inst.Get(nRandom).CAUSE;

                    GbVar.dbAlarmLog.Enqueue_InsertDB(m_colInfo);
                }
            }
        }

        private void tabAlarm_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tabCtr = sender as TabControl;

            foreach (TabPage tabpage in tabCtr.TabPages)
            {
                tabpage.ImageIndex = 0;
            }

            tabCtr.SelectedTab.ImageIndex = 1;
        }

        private void cbbBuzzer_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox comboBox1 = sender as ComboBox; // By using sender, one method could handle multiple ComboBoxes.
            if (comboBox1 != null)
            {
                e.DrawBackground(); // Always draw the background.               
                if (e.Index >= 0) // If there are items to be drawn.
                {
                    StringFormat format = new StringFormat(); // Set the string alignment.  Choices are Center, Near and Far.
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Center;
                    // Set the Brush to ComboBox ForeColor to maintain any ComboBox color settings.
                    // Assumes Brush is solid.
                    Brush brush = new SolidBrush(comboBox1.ForeColor);
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) // If drawing highlighted selection, change brush.
                    {
                        brush = SystemBrushes.HighlightText;
                    }
                    e.Graphics.DrawString(comboBox1.Items[e.Index].ToString(), comboBox1.Font, brush, e.Bounds, format); // Draw the string.
                }
            }
        }

        private void frmAlarm_Load(object sender, EventArgs e)
        {
            GbFunc.SetDoubleBuffered(dgvErrorList);
            GbFunc.SetDoubleBuffered(dgvAlarmStatus);
            
            GbFunc.SetDoubleBuffered(dgvAlarmLog);

            Initdgv();
            initcbb();
        }

    }

    class AlarmFreqProperties
    {
        public int Ranking { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }

        public AlarmFreqProperties()
        {
            this.Ranking = 0;
            this.Name = "";
            this.Count = 0;
        }
    }

    class ErrorProperties
    {
        public int No { get; set; }
        public string Name { get; set; }
        public bool Heavy { get; set; }
        public string Buzzer { get; set; }


        public ErrorProperties()
        {
            this.No = 0;
            this.Name = "SPARE";
            this.Heavy = false;
            this.Buzzer = "Buzzer1";
        }
    }
}
