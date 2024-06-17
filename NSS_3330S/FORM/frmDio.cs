using NSS_3330S.UC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using NSS_3330S.POP;
using NSS_3330S.MOTION;


namespace NSS_3330S.FORM
{
    public partial class frmDio : Form
    {
        AGauge[] m_AGg = new AGauge[(int)IODF.A_INPUT.MAX];
        Panel[] m_VacPanel = new Panel[(int)IODF.A_INPUT.MAX];
        Button[] m_VacButton = new Button[(int)IODF.A_INPUT.MAX];
        int nSelectedIndex = 33;
        int nLastSelectedIndex = 33;

        Color colSelected = Color.Gold;
        Color colUnSelected = Color.Gainsboro;

        public const int IO_VIEW_CNT = 32;
        private int InputCount = System.Enum.GetValues(typeof(NSS_3330S.IODF.INPUT)).Length - 2;
        private int OutputCount = System.Enum.GetValues(typeof(NSS_3330S.IODF.OUTPUT)).Length - 2;
        public static bool CylinderLoop_SW = true;

        private int nChIn = 0;
        private int nChOut = 0;

        private string[] strInDefine;
        private string[] strOutDefine;
        private string[] strInVarAddr;
        private string[] strOutVarAddr;
        private string[] strInDevNo;
        private string[] strOutDevNo;
        private string[] strSearchDefine = new string[IODF.IN_TOTAL_CNT + IO_VIEW_CNT];

        private RadioButton[] rdbInputPages = new RadioButton[15];
        private RadioButton[] rdbOutputPages = new RadioButton[12];
        UcVacuumAirView ucVacAir = null;

        int CurLanguage = -1;

        public frmDio()
        {
            InitializeComponent();

            // 탭컨트롤 아이템사이즈 변경
            foreach (TabControl item in new TabControl[] { tabVacuum })
            {
                item.ItemSize = new Size(0, 1);
            }

            FindVacButton(tabVacuum.Controls);
            FindVacPanel(tabVacuum.Controls);
            SortByTag();

            rdbInputPages = new RadioButton[11] { 
                rdbInputPage1, rdbInputPage2, rdbInputPage3, rdbInputPage4, rdbInputPage5, rdbInputPage6, 
                rdbInputPage7, rdbInputPage8, rdbInputPage9, rdbInputPage10, rdbInputPage11};

            rdbOutputPages = new RadioButton[9] {
                rdbOutputPage1, rdbOutputPage2, rdbOutputPage3, rdbOutputPage4, rdbOutputPage5,
                rdbOutputPage6, rdbOutputPage7, rdbOutputPage8, rdbOutputPage9};

            strInDefine = new string[InputCount + IO_VIEW_CNT];
            strOutDefine = new string[OutputCount + IO_VIEW_CNT];
            strInVarAddr = new string[InputCount + IO_VIEW_CNT];
            strOutVarAddr = new string[OutputCount + IO_VIEW_CNT];
            strInDevNo = new string[InputCount + IO_VIEW_CNT];
            strOutDevNo = new string[OutputCount + IO_VIEW_CNT];

            if (DesignMode) return;

            loadIoDefine();

            for (int i = 0; i < (int)IODF.A_INPUT.MAX; i++)
            {
                m_AGg[i] = new AGauge();

                m_AGg[i].BackColor = System.Drawing.Color.White;
                m_AGg[i].BaseArcColor = System.Drawing.Color.Gray;
                m_AGg[i].BaseArcRadius = 40;
                m_AGg[i].BaseArcStart = 180;
                m_AGg[i].BaseArcSweep = 180;
                m_AGg[i].BaseArcWidth = 2;
                m_AGg[i].Cap_Idx = ((byte)(1));
                m_AGg[i].CapColors = new System.Drawing.Color[] {
                                                                System.Drawing.Color.Black,
                                                                System.Drawing.Color.Black,
                                                                System.Drawing.Color.Black,
                                                                System.Drawing.Color.Black,
                                                                System.Drawing.Color.Black};
                m_AGg[i].CapPosition = new System.Drawing.Point(10, 10);
                m_AGg[i].CapsPosition = new System.Drawing.Point[] {
                                                                    new System.Drawing.Point(10, 10),
                                                                    new System.Drawing.Point(10, 10),
                                                                    new System.Drawing.Point(10, 10),
                                                                    new System.Drawing.Point(10, 10),
                                                                    new System.Drawing.Point(10, 10)};
                m_AGg[i].CapsText = new string[] {
                                        "",
                                        "",
                                        "",
                                        "",
                                        ""};
                m_AGg[i].CapText = "";
                m_AGg[i].Center = new System.Drawing.Point(80, 80);
                m_AGg[i].Location = new System.Drawing.Point(160, 15);
                m_AGg[i].MaxValue = 100F;
                m_AGg[i].MinValue = -100F;
                m_AGg[i].Name = "aGg" + i.ToString();
                m_AGg[i].NeedleColor1 = NSS_3330S.UC.AGauge.NeedleColorEnum.Gray;
                m_AGg[i].NeedleColor2 = System.Drawing.Color.DimGray;
                m_AGg[i].NeedleRadius = 40;
                m_AGg[i].NeedleType = 0;
                m_AGg[i].NeedleWidth = 2;
                m_AGg[i].RangesColor = new System.Drawing.Color[] {
                                                                    System.Drawing.Color.Lime,
                                                                    System.Drawing.Color.Gold,
                                                                    System.Drawing.Color.DeepSkyBlue,
                                                                    System.Drawing.SystemColors.Control,
                                                                    System.Drawing.SystemColors.Control};
                m_AGg[i].RangesEnabled = new bool[] {
                                                    true,
                                                    true,
                                                    true,
                                                    false,
                                                    false};
                m_AGg[i].RangesEndValue = new float[] {
                                                     -50F,
                                                     50F,
                                                     100F,
                                                     0F,
                                                     0F};
                m_AGg[i].RangesInnerRadius = new int[] {
                                                        40,
                                                        40,
                                                        40,
                                                        70,
                                                        70};
                m_AGg[i].RangesOuterRadius = new int[] {
                                                        50,
                                                        50,
                                                        50,
                                                        80,
                                                        80};
                m_AGg[i].RangesStartValue = new float[] {
                                                        -100F,
                                                        -50F,
                                                        50F,
                                                        0F,
                                                        0F};
                m_AGg[i].ScaleLinesInterColor = System.Drawing.Color.Black;
                m_AGg[i].ScaleLinesInterInnerRadius = 42;
                m_AGg[i].ScaleLinesInterOuterRadius = 50;
                m_AGg[i].ScaleLinesInterWidth = 1;
                m_AGg[i].ScaleLinesMajorColor = System.Drawing.Color.Black;
                m_AGg[i].ScaleLinesMajorInnerRadius = 40;
                m_AGg[i].ScaleLinesMajorOuterRadius = 50;
                m_AGg[i].ScaleLinesMajorStepValue = 50F;
                m_AGg[i].ScaleLinesMajorWidth = 2;
                m_AGg[i].ScaleLinesMinorColor = System.Drawing.Color.Gray;
                m_AGg[i].ScaleLinesMinorInnerRadius = 43;
                m_AGg[i].ScaleLinesMinorNumOf = 9;
                m_AGg[i].ScaleLinesMinorOuterRadius = 50;
                m_AGg[i].ScaleLinesMinorWidth = 1;
                m_AGg[i].ScaleNumbersColor = System.Drawing.Color.Black;
                m_AGg[i].ScaleNumbersFormat = null;
                m_AGg[i].ScaleNumbersRadius = 65;
                m_AGg[i].ScaleNumbersRotation = 0;
                m_AGg[i].ScaleNumbersStartScaleLine = 1;
                m_AGg[i].ScaleNumbersStepScaleLines = 2;
                m_AGg[i].Size = new System.Drawing.Size(160, 88);
                m_AGg[i].TabIndex = 0;
                m_AGg[i].Text = "aGg" + i.ToString();
                m_AGg[i].Value = 10F;

                m_AGg[i].Dock = DockStyle.Fill;
                m_AGg[i].Tag = i.ToString();
                if (m_VacPanel[i] != null)
                {
                    m_VacPanel[i].Controls.Add(m_AGg[i]);
                }
            }
            SelectButton();
        }
        //private string[] strInDefine = new string[System.Enum.GetValues(typeof(NSS_3330S.IODF.INPUT)).Length -2 + IO_VIEW_CNT];

        private void SortByTag()
        {
            Panel[] buf_VacPanel = new Panel[(int)IODF.A_INPUT.MAX];
            Button[] buf_VacButton = new Button[(int)IODF.A_INPUT.MAX];

            foreach (Panel item in m_VacPanel)
            {
                if (item == null) continue;
                int nTag = Convert.ToInt16(item.Tag.ToString());
                buf_VacPanel[nTag] = item;
            }

            foreach (Button item in m_VacButton)
            {
                if (item == null) continue;
                int nTag = Convert.ToInt16(item.Tag.ToString());
                buf_VacButton[nTag] = item;
            }

            m_VacPanel = buf_VacPanel;
            m_VacButton = buf_VacButton;
        }

        public int TOTAL_INPUT_PAGE_COUNT
        {
            get
            {
                int nTotalCnt = (InputCount / IO_VIEW_CNT);
                if (InputCount % IO_VIEW_CNT > 0) nTotalCnt++;
                return nTotalCnt;
            }
        }

        public int TOTAL_OUTPUT_PAGE_COUNT
        {
            get
            {
                int nTotalCnt = (OutputCount / IO_VIEW_CNT);
                if (OutputCount % IO_VIEW_CNT > 0) nTotalCnt++;
                return nTotalCnt;
            }
        }

        private void frmDIO_Load(object sender, EventArgs e)
        {
            nChIn = 0;
            nChOut = 0;

            dgvInput.RowCount = IO_VIEW_CNT;
            foreach (DataGridViewRow row in dgvInput.Rows) { row.Height = (dgvInput.Height - dgvInput.ColumnHeadersHeight) / IO_VIEW_CNT; }

            dgvOutput.RowCount = IO_VIEW_CNT;
            foreach (DataGridViewRow row in dgvOutput.Rows) { row.Height = (dgvOutput.Height - dgvOutput.ColumnHeadersHeight) / IO_VIEW_CNT; }

            //btnInPrev.Enabled = false;
            //btnOutPrev.Enabled = false;

            inputUpdate(0);
            outputUpdate(0);

            GbFunc.SetDoubleBuffered(dgvInput);
            GbFunc.SetDoubleBuffered(dgvOutput);

            ucVacAir = new UcVacuumAirView();
            ucVacAir.Dock = DockStyle.Fill;
            pnlVacBlowDelay.Controls.Add(ucVacAir);
        }

        ///// <summary> 
        ///// 사용 중인 모든 리소스를 정리합니다.
        ///// </summary>
        ///// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        private void inputUpdate(int nCh)
        {
            int nInCurCount = 0;
            for (int nInCount = 0; nInCount < IO_VIEW_CNT; nInCount++)
            {
                try
                {
                    string strNum = "X_" + string.Format("{0:000}", int.Parse(strInVarAddr[nInCount + (nCh * IO_VIEW_CNT)]));

                    int i = 0;
                    dgvInput.Rows[nInCount].Cells[i++].Value = strNum;
                    dgvInput.Rows[nInCount].Cells[i++].Value = strInDefine[nInCount + (nCh * IO_VIEW_CNT)];
                    //dgvInput.Rows[nInCount].Cells[i++].Value = "N/O";
                    //dgvInput.Rows[nInCount].Cells[i++].Value = GbVar.IO[(IODF.INPUT)Enum.ToObject(typeof(IODF.INPUT), nInCount)];

                    nInCurCount = nInCount + (nCh * IO_VIEW_CNT);
                    if (Enum.IsDefined(typeof(IODF.INPUT), nInCurCount))
                        dgvInput.Rows[nInCount].Cells[0].Tag = (IODF.INPUT)nInCurCount;
                    else
                        dgvInput.Rows[nInCount].Cells[0].Tag = IODF.INPUT.NONE;

                    if (dgvInput.Rows[nInCount].Cells[0].Tag == null)
                        dgvInput.Rows[nInCount].Cells[0].Tag = IODF.INPUT.NONE;
                }
                catch (Exception ex)
                {

                }
                
            }
        }

        private void outputUpdate(int nCh)
        {
            int nOutCurCount = 0;
            for (int nOutCount = 0; nOutCount < IO_VIEW_CNT; nOutCount++)
            {
                string strNum = "Y_" + string.Format("{0:000}", int.Parse(strOutVarAddr[nOutCount + (nCh * IO_VIEW_CNT)]));

                int i = 0;
                dgvOutput.Rows[nOutCount].Cells[i++].Value = strNum;
                dgvOutput.Rows[nOutCount].Cells[i++].Value = strOutDefine[nOutCount + (nCh * IO_VIEW_CNT)];
                //dgvOutput.Rows[nOutCount].Cells[i++].Value = GbVar.IO[(IODF.OUTPUT)Enum.ToObject(typeof(IODF.OUTPUT), nOutCount)];

                nOutCurCount = nOutCount + (nCh * IO_VIEW_CNT);
                if (Enum.IsDefined(typeof(IODF.OUTPUT), nOutCurCount))
                    dgvOutput.Rows[nOutCount].Cells[0].Tag = (IODF.OUTPUT)nOutCurCount;
                else
                    dgvOutput.Rows[nOutCount].Cells[0].Tag = IODF.OUTPUT.NONE;

                if (dgvOutput.Rows[nOutCount].Cells[0].Tag == null)
                    dgvOutput.Rows[nOutCount].Cells[0].Tag = IODF.OUTPUT.NONE;
            }
        }

        private void dgvInput_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 2 && e.RowIndex >= 0 && (e.PaintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
            {
                IODF.INPUT input = (IODF.INPUT)dgvInput.Rows[e.RowIndex].Cells[0].Tag;
                if (input > IODF.INPUT.NONE && input < IODF.INPUT.MAX && GbVar.GB_INPUT[(int)input] == 1)
                {
                    using (Brush gridBrush = new SolidBrush(this.dgvInput.GridColor))
                    {
                        using (Brush backColorBrush = new SolidBrush(Color.Lime))
                        {
                            using (Pen gridLinePen = new Pen(gridBrush))
                            {
                                // Clear cell
                                e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
                                //force paint of content
                                e.PaintContent(e.ClipBounds);
                                e.Handled = true;
                            }
                        }
                    }
                }
                else
                {
                    using (Brush gridBrush = new SolidBrush(this.dgvInput.GridColor))
                    {
                        using (Brush backColorBrush = new SolidBrush(Color.FromArgb(210, 210, 210)))
                        {
                            using (Pen gridLinePen = new Pen(gridBrush))
                            {
                                // Clear cell
                                e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
                                //force paint of content
                                e.PaintContent(e.ClipBounds);
                                e.Handled = true;
                            }
                        }
                    }
                }
                DataGridViewPaintParts paintParts = e.PaintParts & ~DataGridViewPaintParts.Background;
                e.Paint(e.ClipBounds, paintParts);
                e.Handled = true;
            }
        }

        private void dgvOutput_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 2 && e.RowIndex >= 0 && (e.PaintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
            {
                IODF.OUTPUT output = (IODF.OUTPUT)dgvOutput.Rows[e.RowIndex].Cells[0].Tag;
                if (output > IODF.OUTPUT.NONE && output < IODF.OUTPUT.MAX && GbVar.GB_OUTPUT[(int)output] == 1)
                {
                    using (Brush gridBrush = new SolidBrush(this.dgvOutput.GridColor))
                    {
                        using (Brush backColorBrush = new SolidBrush(Color.Orange))
                        {
                            using (Pen gridLinePen = new Pen(gridBrush))
                            {
                                // Clear cell
                                e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
                                //force paint of content
                                e.PaintContent(e.ClipBounds);
                                e.Handled = true;
                            }
                        }
                    }
                }
                else
                {
                    using (Brush gridBrush = new SolidBrush(this.dgvOutput.GridColor))
                    {
                        using (Brush backColorBrush = new SolidBrush(Color.FromArgb(210, 210, 210)))
                        {
                            using (Pen gridLinePen = new Pen(gridBrush))
                            {
                                // Clear cell
                                e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
                                //force paint of content
                                e.PaintContent(e.ClipBounds);
                                e.Handled = true;
                            }
                        }
                    }
                }
                DataGridViewPaintParts paintParts = e.PaintParts & ~DataGridViewPaintParts.Background;
                e.Paint(e.ClipBounds, paintParts);
                e.Handled = true;
            }
        }

        void FindVacButton(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(Button))
                    {
                        int nTagNo = Convert.ToInt16(ctr.Tag.ToString());
                        m_VacButton[nTagNo] = (Button)ctr;
                        ctr.Click += btnVac_Click;
                    }

                    if (ctr.Controls.Count > 0)
                        FindVacButton(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                throw ex;
            }
        }

        void FindVacPanel(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(Panel))
                    {
                        int nTagNo = Convert.ToInt16(ctr.Tag.ToString());
                        m_VacPanel[nTagNo] = (Panel)ctr;
                    }

                    if (ctr.Controls.Count > 0)
                        FindVacPanel(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                throw ex;
            }
        }

        private void dgvOutput_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (GbVar.mcState.IsRun()) return;
            if (GbVar.CurLoginInfo.USER_LEVEL == MCDF.LEVEL_OP) return;
            string logmsg = "";
            if (e.ColumnIndex != 2) return;
            if (e.RowIndex < 0) return;

            int nIndex = e.RowIndex + (nChOut * IO_VIEW_CNT);

            if (dgvOutput.Rows[e.RowIndex].Cells[0].Tag == null)
                return;

            IODF.OUTPUT output = (IODF.OUTPUT)dgvOutput.Rows[e.RowIndex].Cells[0].Tag;
            if (output == IODF.OUTPUT.NONE)
                return;

            bool bAct = false;
            bAct = Convert.ToBoolean(GbVar.GB_OUTPUT[(int)output]);
            MotionMgr.Inst.SetOutput(output, !bAct);
            //TODO: 위에 주석 풀어야함

            //20181231 pjw.WRITE EVENT LOG ADD??
            //logmsg = string.Format("I/O page Clicked -> {0} : {1}", output.ToString(), !bAct);
            //GbFunc.WriteEventLog(this.GetType().Name.ToString(), logmsg);

        }

        private void btnInPrev_Click(object sender, EventArgs e)
        {
            if (nChIn <= 0)
            {
                //btnInPrev.Enabled = false;
                return;
            }
            nChIn--;
            //btnInNext.Enabled = true;

            int nPage = Convert.ToInt32(nChIn.ToString()) + 1;
            rdbInputPages[nChIn].Checked = true;

            //lblInputPage.Text = string.Format("{0}/{1}", nPage.ToString(), TOTAL_INPUT_PAGE_COUNT);

            inputUpdate(nChIn);
            dgvInput.Invalidate();
            RefreshUI();
        }

        private void btnInNext_Click(object sender, EventArgs e)
        {
            if (nChIn + 1 >= TOTAL_INPUT_PAGE_COUNT) return;

            nChIn++;

            if (nChIn == 2)
            {
                //btnInNext.Enabled = false;
            }
            //btnInPrev.Enabled = true;

            int nPage = Convert.ToInt32(nChIn.ToString()) + 1;
            rdbInputPages[nChIn].Checked = true;

            //lblInputPage.Text = string.Format("{0}/{1}", nPage.ToString(), TOTAL_INPUT_PAGE_COUNT);

            inputUpdate(nChIn);
            dgvInput.Invalidate();
            RefreshUI();
        }

        private void btnOutPrev_Click(object sender, EventArgs e)
        {
            if (nChOut <= 0)
            {
                //btnOutPrev.Enabled = false;
                return;
            }
            nChOut--;

            //btnOutNext.Enabled = true;

            int nPage = Convert.ToInt32(nChOut.ToString()) + 1;
            rdbOutputPages[nChOut].Checked = true;

            //lblOutputPage.Text = string.Format("{0}/{1}", nPage.ToString(), TOTAL_OUTPUT_PAGE_COUNT);

            outputUpdate(nChOut);
            dgvOutput.Invalidate();
            RefreshUI();
        }

        private void btnOutNext_Click(object sender, EventArgs e)
        {
            if (nChOut + 1 >= TOTAL_OUTPUT_PAGE_COUNT) return;

            nChOut++;

            if (nChOut == (TOTAL_OUTPUT_PAGE_COUNT))
            {
                //btnOutNext.Enabled = false;
            }
            //btnOutPrev.Enabled = true;

            int nPage = Convert.ToInt32(nChOut.ToString()) + 1;
            rdbOutputPages[nChOut].Checked = true;

            //lblOutputPage.Text = string.Format("{0}/{1}", nPage.ToString(), TOTAL_OUTPUT_PAGE_COUNT);

            outputUpdate(nChOut);
            dgvOutput.Invalidate();
            RefreshUI();
        }

        public void RefreshUI()
        {
            try
            {
                for (int nCnt = 0; nCnt < dgvInput.Rows.Count; nCnt++)
                {
                    IODF.INPUT input = (IODF.INPUT)dgvInput.Rows[nCnt].Cells[0].Tag;

                    if (input == IODF.INPUT.NONE || input == IODF.INPUT.MAX) continue;

                    //if (Enum.IsDefined(typeof(IODF.INPUT), input))
                    //    dgvInput.Rows[nCnt].Cells[2].Value = GbVar.GB_INPUT[(int)input];
                }

                for (int nCnt = 0; nCnt < dgvOutput.Rows.Count; nCnt++)
                {
                    IODF.OUTPUT output = (IODF.OUTPUT)dgvOutput.Rows[nCnt].Cells[0].Tag;

                    if (output == IODF.OUTPUT.NONE || output == IODF.OUTPUT.MAX) continue;

                    //if (Enum.IsDefined(typeof(IODF.OUTPUT), output))
                    //    dgvOutput.Rows[nCnt].Cells[2].Value = GbVar.GB_OUTPUT[(int)output];
                }

                dgvInput.Invalidate();
                dgvOutput.Invalidate();
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void loadIoDefine()
        {
            int nInCount = 0;
            int n1cnt = 0;
            int n100cnt = 0;
            bool nSSwitch = true;
            bool nLSwitch = true;
            bool nCSwitch = true;
            foreach (IODF.INPUT input in Enum.GetValues(typeof(IODF.INPUT)))
            {
                if (input == IODF.INPUT.NONE || input == IODF.INPUT.MAX)
                    continue;

                strInDefine[nInCount] = string.Format("{0}", input.ToString().Replace("_", " "));
                if(n1cnt < 32)
                {
                    strInVarAddr[nInCount] = string.Format("{0}", n1cnt + n100cnt);
                }
                else
                {
                    n100cnt += 100;
                    n1cnt = 0;
                    n100cnt = IncheckN100cnt(n100cnt, ref nSSwitch, ref nLSwitch, ref nCSwitch);
                    strInVarAddr[nInCount] = string.Format("{0}", n1cnt + n100cnt);
                }
                strInDevNo[nInCount] = string.Format("{0}", "1");

                n1cnt++;
                nInCount++;
            }

            int nOutCount = 0;
            n1cnt = 0;
            n100cnt = 0;
            nSSwitch = true;
            nLSwitch = true;
            nCSwitch = true;
            foreach (IODF.OUTPUT output in Enum.GetValues(typeof(IODF.OUTPUT)))
            {
                if (output == IODF.OUTPUT.NONE || output == IODF.OUTPUT.MAX)
                    continue;

                strOutDefine[nOutCount] = string.Format("{0}", output.ToString().Replace("_", " "));
                if (n1cnt < 32)
                {
                    strOutVarAddr[nOutCount] = string.Format("{0}", n1cnt + n100cnt);
                }
                else
                {
                    n100cnt += 100;
                    n1cnt = 0;
                    n100cnt = OutcheckN100cnt(n100cnt, ref nSSwitch, ref nLSwitch, ref nCSwitch);
                    strOutVarAddr[nOutCount] = string.Format("{0}", n1cnt + n100cnt);
                }

                strOutDevNo[nOutCount] = string.Format("{0}", "1");
                n1cnt++;
                nOutCount++;
            }
        }
        /// <summary>
        /// in 100 단위 페이지 번호 체크
        /// </summary>
        /// <param name="n100cnt"> 확인할 100 단위 수</param>
        /// <param name="nSSwitch"> 소터 스위치 확인 변수</param>
        /// <param name="nLSwitch">로더 스위치 확인 변수</param>
        /// <param name="nCSwitch">수세기 스위치 확인 변수</param>
        /// <returns></returns>
        private int IncheckN100cnt(int n100cnt ,ref bool nSSwitch, ref bool nLSwitch, ref bool nCSwitch)
        {
            int changeSnum = 1200;
            int SskipPage = 1;
            int endSnum = 500;
            int startLnum = 2000;
            int changeLnum = 3000;
            int LskipPage = 3;
            int endLnum = 2200;
            int startCnum = 3000;
            int changeCnum = 0;
            int CskipPage = 0;
            int endCnum = 4500;

            if (n100cnt == changeSnum - SskipPage * 100) n100cnt = changeSnum;
            if (n100cnt > endSnum && nSSwitch)
            {
                n100cnt = startLnum;
                nSSwitch = false;
            }
            if (n100cnt == changeLnum - LskipPage * 100) n100cnt = changeLnum;
            if (n100cnt > endLnum && nLSwitch)
            {
                n100cnt = startCnum;
                nLSwitch = false;
            }
            if (n100cnt == changeCnum - CskipPage * 100) n100cnt = changeCnum;
            if (n100cnt > endCnum && nCSwitch)
            {
                nCSwitch = false;
            }

            return n100cnt;
        }


        /// <summary>
        /// out 100 단위 페이지 번호 체크
        /// </summary>
        /// <param name="n100cnt"> 확인할 100 단위 수</param>
        /// <param name="nSSwitch"> 소터 스위치 확인 변수</param>
        /// <param name="nLSwitch">로더 스위치 확인 변수</param>
        /// <param name="nCSwitch">수세기 스위치 확인 변수</param>
        /// <returns></returns>
        private int OutcheckN100cnt(int n100cnt, ref bool nSSwitch, ref bool nLSwitch, ref bool nCSwitch)
        {
            int changeSnum = 0;
            int SskipPage = 0;
            int endSnum = 400;
            int startLnum = 2000;
            int changeLnum = 3000;
            int LskipPage = 3;
            int endLnum = 2200;
            int startCnum = 3000;
            int changeCnum = 0;
            int CskipPage = 0;
            int endCnum = 4500;

            if (n100cnt == changeSnum - SskipPage * 100) n100cnt = changeSnum;
            if (n100cnt > endSnum && nSSwitch)
            {
                n100cnt = startLnum;
                nSSwitch = false;
            }
            if (n100cnt == changeLnum - LskipPage * 100) n100cnt = changeLnum;
            if (n100cnt > endLnum && nLSwitch)
            {
                n100cnt = startCnum;
                nLSwitch = false;
            }
            if (n100cnt == changeCnum - CskipPage * 100) n100cnt = changeCnum;
            if (n100cnt > endCnum && nCSwitch)
            {
                nCSwitch = false;
            }

            return n100cnt;
        }

        private void radioButtonInput_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            nChIn = nTag;

            inputUpdate(nChIn);
            dgvInput.Invalidate();
            RefreshUI();
        }

        private void radioButtonOutput_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            nChOut = nTag;

            outputUpdate(nChOut);
            dgvOutput.Invalidate();
            RefreshUI();
        }

        private void tabDioPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tbc = sender as TabControl;
            for (int i = 0; i < tbc.TabPages.Count; i++)
            {
                if (tbc.SelectedIndex == i) tbc.TabPages[i].ImageIndex = 1;
                else tbc.TabPages[i].ImageIndex = 0;
            }
        }

        private void rdbVacuum1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabVacuum.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabVacuum.SelectedIndex = nTag;
            }
        }

        private void btnVac_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            nLastSelectedIndex = nSelectedIndex;
            nSelectedIndex = Convert.ToInt16(btn.Tag.ToString());
            SelectButton();
        }

        private void SelectButton(bool bSetLastSelect = true)
        {
            foreach (var item in m_VacButton)
            {
                if (item == null) continue;
                item.BackColor = colUnSelected;
                item.Font = new Font("맑은 고딕", 8, FontStyle.Bold);
            }
            if (m_VacButton[nSelectedIndex] == null) return;
            m_VacButton[nSelectedIndex].BackColor = colSelected;

            {
                int nSaveIdx = bSetLastSelect ? nLastSelectedIndex : nSelectedIndex;
                long lValue = 0;
                if (long.TryParse(tbxVacOnDelay.Text, out lValue))
                    ConfigMgr.Inst.TempCfg.Vac[nSaveIdx].lVacOnDelay = lValue;
                if (long.TryParse(tbxBlowOnDelay.Text, out lValue))
                    ConfigMgr.Inst.TempCfg.Vac[nSaveIdx].lBlowOnDelay = lValue;
                if (long.TryParse(tbxBlowOffDelay.Text, out lValue))
                    ConfigMgr.Inst.TempCfg.Vac[nSaveIdx].lBlowOffDelay = lValue;

                double dValue = 0;
                if (double.TryParse(tbxVacuumLevelLow.Text, out dValue) && dValue >= m_AGg[nSaveIdx].MinValue)
                    ConfigMgr.Inst.TempCfg.Vac[nSaveIdx].dVacLevelLow = dValue;
                if (double.TryParse(tbxVacuumLevelHigh.Text, out dValue) && dValue <= m_AGg[nSaveIdx].MaxValue)
                    ConfigMgr.Inst.TempCfg.Vac[nSaveIdx].dVacLevelHigh = dValue;
                if (double.TryParse(tbxRatio.Text, out dValue))
                    ConfigMgr.Inst.TempCfg.Vac[nSaveIdx].dRatio = dValue;
                if (double.TryParse(tbxDefaultVoltage.Text, out dValue))
                    ConfigMgr.Inst.TempCfg.Vac[nSaveIdx].dDefaultVoltage = dValue;
            }

            // 새로운 값 불러오기
            tbxVacOnDelay.Text = ConfigMgr.Inst.TempCfg.Vac[nSelectedIndex].lVacOnDelay.ToString();
            tbxBlowOnDelay.Text = ConfigMgr.Inst.TempCfg.Vac[nSelectedIndex].lBlowOnDelay.ToString();
            tbxBlowOffDelay.Text = ConfigMgr.Inst.TempCfg.Vac[nSelectedIndex].lBlowOffDelay.ToString();
            tbxVacuumLevelLow.Text = ConfigMgr.Inst.TempCfg.Vac[nSelectedIndex].dVacLevelLow.ToString("F3");
            tbxVacuumLevelHigh.Text = ConfigMgr.Inst.TempCfg.Vac[nSelectedIndex].dVacLevelHigh.ToString("F3");
            tbxRatio.Text = ConfigMgr.Inst.TempCfg.Vac[nSelectedIndex].dRatio.ToString();
            tbxDefaultVoltage.Text = ConfigMgr.Inst.TempCfg.Vac[nSelectedIndex].dDefaultVoltage.ToString();

            foreach (var item in m_AGg)
            {
                if (item == null) continue;
                int nTag = 0;
                if (!int.TryParse(item.Tag.ToString(), out nTag)) continue;

                item.Range_Idx = 0;
                item.RangeEndValue = (float)ConfigMgr.Inst.TempCfg.Vac[nTag].dVacLevelLow;
                item.RangeStartValue = -100F;

                item.Range_Idx = 1;
                item.RangeEndValue = (float)ConfigMgr.Inst.TempCfg.Vac[nTag].dVacLevelHigh;
                item.RangeStartValue = m_AGg[nTag].RangesEndValue[0];

                item.Range_Idx = 2;
                item.RangeEndValue = 100F;
                item.RangeStartValue = m_AGg[nTag].RangesEndValue[1];
            }

            tabIO.Refresh();
        }

        private void tabIO_VisibleChanged(object sender, EventArgs e)
        {
            FindControl(this.Controls);
            btnVacuumSave.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
            btnVacOn.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
            btnVacOff.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
            btnBlowOn.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
            btnBlowOff.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
            button38.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
        }
        static void FindControl(Control.ControlCollection collection)
        {
            foreach (Control ctrl in collection)
            {
                if (ctrl.GetType() == typeof(UcVacuumAirView) ||
                    ctrl.GetType() == typeof(NumericUpDown) ||
                    ctrl.GetType() == typeof(CheckBox) ||
                    ctrl.GetType() == typeof(TextBox))
                {
                    try
                    {
                        ctrl.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
                    }
                    catch (Exception ex)
                    {

                    }

                }

                if (ctrl.Controls.Count > 0)
                    FindControl(ctrl.Controls);
            }
        }
        private void btnVacuumSave_Click(object sender, EventArgs e)
        {
            popCheckPassword chk = new popCheckPassword();
            chk.TopMost = true;
            if (chk.ShowDialog(this) != DialogResult.OK) return;
            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnVacuumSave_Click");

            if (!ConfigMgr.Inst.DataCheckBeforeSave()) return;
            SelectButton(false);

            ConfigMgr.Inst.SaveBackup();
            ConfigMgr.Inst.SaveTempCfg();
            ConfigMgr.Inst.Save();
            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "SaveConfig Complete");
            popMessageBox pMsg = new popMessageBox(FormTextLangMgr.FindKey("SAVE COMPLETE"), "ACCOUNT", MessageBoxButtons.OK);
            if (pMsg.ShowDialog(this) != DialogResult.OK) return;
        }

        private void UpdateData()
        {
            SelectButton(false);
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            dgvInput.Refresh();
            dgvOutput.Refresh();
            setAnalogValue();
        }

        private void frmDio_VisibleChanged(object sender, EventArgs e)
        {
            tmrRefresh.Enabled = this.Visible;
            if (!this.Visible)
                return;

            if (CurLanguage != ConfigMgr.Inst.Cfg.General.nLanguage)
            {
                CurLanguage = ConfigMgr.Inst.Cfg.General.nLanguage;

                dgvInput.Columns[0].HeaderText = FormTextLangMgr.FindKey("NO");
                dgvInput.Columns[1].HeaderText = FormTextLangMgr.FindKey("NAME");
                dgvInput.Columns[2].HeaderText = FormTextLangMgr.FindKey("STATUS");

                dgvOutput.Columns[0].HeaderText = FormTextLangMgr.FindKey("NO");
                dgvOutput.Columns[1].HeaderText = FormTextLangMgr.FindKey("NAME");
                dgvOutput.Columns[2].HeaderText = FormTextLangMgr.FindKey("STATUS");
            }

                if (this.Visible)
            {
                try
                {
                }
                catch (Exception ex)
                {

                }
                
            }
        }

        private void setAnalogValue()
        {
            foreach (AGauge item in m_AGg)
            {
                int nTag = Convert.ToInt16(item.Tag.ToString());

                item.Range_Idx = 0;
                item.RangeEndValue = (float)ConfigMgr.Inst.TempCfg.Vac[nTag].dVacLevelLow;
                item.RangeStartValue = -100F;

                item.Range_Idx = 1;
                item.RangeEndValue = (float)ConfigMgr.Inst.TempCfg.Vac[nTag].dVacLevelHigh;
                item.RangeStartValue = item.RangesEndValue[0];

                item.Range_Idx = 2;
                item.RangeEndValue = 100F;
                item.RangeStartValue = item.RangesEndValue[1];

                item.Value = (float)(((GbVar.GB_AINPUT[nTag] - ConfigMgr.Inst.Cfg.Vac[nTag].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nTag].dRatio));
            }

            lbVoltage.Text = GbVar.GB_AINPUT[nSelectedIndex].ToString("F1");

            lbPressure.Text = (((GbVar.GB_AINPUT[nSelectedIndex] - ConfigMgr.Inst.Cfg.Vac[nSelectedIndex].dDefaultVoltage) * ConfigMgr.Inst.TempCfg.Vac[nSelectedIndex].dRatio)).ToString("F1");
        }

        private void dgvInput_CellClick(object sender, DataGridViewCellEventArgs e)
        {
#if _NOTEBOOK
            if (GbVar.mcState.IsRun()) return;

            string logmsg = "";
            if (e.ColumnIndex != 2) return;
            if (e.RowIndex < 0) return;

            int nIndex = e.RowIndex + (nChIn * IO_VIEW_CNT);

            if (dgvInput.Rows[e.RowIndex].Cells[0].Tag == null)
                return;

            IODF.INPUT input = (IODF.INPUT)dgvInput.Rows[e.RowIndex].Cells[0].Tag;
            if (input == IODF.INPUT.NONE)
                return;

            bool bAct = false;
            bAct = Convert.ToBoolean(GbVar.GB_INPUT[(int)input]);
            GbVar.GB_INPUT[(int)input] = GbVar.GB_INPUT[(int)input] == 1 ? 0 : 1;
            //TODO: 위에 주석 풀어야함

            //20181231 pjw.WRITE EVENT LOG ADD??
            //logmsg = string.Format("I/O page Clicked -> {0} : {1}", output.ToString(), !bAct);
            //GbFunc.WriteEventLog(this.GetType().Name.ToString(), logmsg);
#endif
        }
        private void btnInputSearch_Click(object sender, EventArgs e)
        {
            for (int nIdx = 0; nIdx < strSearchDefine.Length; nIdx++)
            {
                strSearchDefine[nIdx] = "";
            }
            int nInputCnt = 0;
            foreach (IODF.INPUT input in Enum.GetValues(typeof(IODF.INPUT)))
            {
                if (nInputCnt == 32)
                {
                    MessageBox.Show("THERE IS MANY SEARCH COUNT. SEARCH DETAILED!!");
                    break;
                }
                string strText = tbInput.Text.ToString().Replace(" ", "_");
                if (input.ToString().Contains(strText))
                {
                    strSearchDefine[nInputCnt] = string.Format("{0}", input.ToString().Replace("_", " "));

                    dgvInput.Rows[nInputCnt].Cells[0].Value = string.Format("X_{0:000}",ChangeInputAddress(input));
                    dgvInput.Rows[nInputCnt].Cells[1].Value = strSearchDefine[nInputCnt];

                    dgvInput.Rows[nInputCnt].Cells[0].Tag = input;

                    nInputCnt++;
                }
            }
            if (nInputCnt < 32)
            {
                for (int nIdx = nInputCnt; nIdx < 32; nIdx++)
                {
                    dgvInput.Rows[nIdx].Cells[0].Value = "";
                    dgvInput.Rows[nIdx].Cells[1].Value = strSearchDefine[nInputCnt];

                    dgvInput.Rows[nIdx].Cells[0].Tag = IODF.INPUT.NONE;

                }
            }

            dgvInput.Invalidate();
        }
        private void btnOutputSearch_Click(object sender, EventArgs e)
        {
            for (int nIdx = 0; nIdx < strSearchDefine.Length; nIdx++)
            {
                strSearchDefine[nIdx] = "";
            }
            int nOutput = 0;
            foreach (IODF.OUTPUT output in Enum.GetValues(typeof(IODF.OUTPUT)))
            {
                if (nOutput == 32)
                {
                    MessageBox.Show("THERE IS MANY SEARCH COUNT. SEARCH DETAILED!!");
                    break;
                }
                string strText = tbOutput.Text.ToString().Replace(" ", "_");
                if (output.ToString().Contains(strText))
                {
                    strSearchDefine[nOutput] = string.Format("{0}", output.ToString().Replace("_", " "));

                    dgvOutput.Rows[nOutput].Cells[0].Value = string.Format("Y_{0:000}", ChangeOutputAddress(output)); ;

                    dgvOutput.Rows[nOutput].Cells[1].Value = strSearchDefine[nOutput];

                    dgvOutput.Rows[nOutput].Cells[0].Tag = output;

                    nOutput++;
                }
            }
            if (nOutput < 32)
            {
                for (int nIdx = nOutput; nIdx < 32; nIdx++)
                {
                    dgvOutput.Rows[nIdx].Cells[0].Value = "";
                    dgvOutput.Rows[nIdx].Cells[1].Value = strSearchDefine[nOutput];

                    dgvOutput.Rows[nIdx].Cells[0].Tag = IODF.OUTPUT.NONE;

                }
            }

            dgvOutput.Invalidate();
        }
        int ChangeInputAddress(IODF.INPUT nInput)
        {
            int nValue = (int)nInput;
            //0~900
            if (nValue < 160)
            {
                //몫은 quotient
                //나머지는 remainder
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                if (nQuotient != 0)
                {
                    return (nQuotient * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
            //1200~1315
            else if(nValue < 192)
            {
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                int nOffset = 2;
                if (nQuotient != 0)
                {
                    return ((nQuotient + nOffset) * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
            //2000~ 2315
            else if(nValue < 256)
            {
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                int nOffset = 2 + 6;
                if (nQuotient != 0)
                {
                    return ((nQuotient + nOffset) * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
            //3000~ 3515
            else if(nValue < 352)
            {
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                int nOffset = 2 + 6 + 6;
                if (nQuotient != 0)
                {
                    return ((nQuotient + nOffset) * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
            else
            {
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                int nOffset = 2 + 6 + 6 + 4;
                if (nQuotient != 0)
                {
                    return ((nQuotient + nOffset) * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
        }
        int ChangeOutputAddress(IODF.OUTPUT nOutput)
        {
            int nValue = (int)nOutput;
            //0~915
            if (nValue < 160)
            {
                //몫은 quotient
                //나머지는 remainder
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                if (nQuotient != 0)
                {
                    return (nQuotient * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
            //2000~ 2315
            else if (nValue < 224)
            {
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                int nOffset = 10;
                if (nQuotient != 0)
                {
                    return ((nQuotient + nOffset) * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
            //3000~ 3315
            else if(nValue < 288)
            {
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                int nOffset = 10 + 6;
                if (nQuotient != 0)
                {
                    return ((nQuotient + nOffset) * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
            else
            {
                int nQuotient = nValue / 16;
                int nRemainder = nValue % 16;
                int nOffset = 10 + 6 + 6;
                if (nQuotient != 0)
                {
                    return ((nQuotient + nOffset) * 100) + nRemainder;
                }
                else
                {
                    return nValue;
                }
            }
        }

        private void label3_DoubleClick(object sender, EventArgs e)
        {
            if (GbVar.CurLoginInfo.USER_LEVEL == 2 && frmDio.CylinderLoop_SW)
            {
                frmDio.CylinderLoop_SW = false;
                popCylinderLoop cylinderLoop = new popCylinderLoop();
                cylinderLoop.Show();
            }
        }

        private void numHeatingConReg_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;
            int nTag = 0;
            nTag = num.GetTag();
        }
    }
}
