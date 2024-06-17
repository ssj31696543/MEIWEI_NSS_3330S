using DionesTool.UTIL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.POP
{
    public partial class popMgrRecipe : Form
    {
        public delegate void EqRcpChange(MESDF.ePPChangeCode mode, string rcpName, bool isHostBody = false);
        public event EqRcpChange eqRcpChange = null;

        protected POP.popRmsRun m_popRmsRun = null;


        public popMgrRecipe()
        {
            InitializeComponent();
            this.CenterToScreen();
        }

        private void popMgrRecipe_Load(object sender, EventArgs e)
        {
            GbFunc.SetDoubleBuffered(gridModel);

            GbVar.bChangeRecipe_Main = false;
            ShowModelFileList();
            txtRcpSoruce.Text = gridModel.Rows[0].Cells[1].Value.ToString();
        }

        //정수형 Col로 소팅하기위해 추가
        //http://stackoverflow.com/questions/2674670/how-to-sort-string-as-number-in-datagridview-in-winforms
        private void customSortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Index == 0) //recipe id만 숫자로 소팅하기위해
            {
                int a = int.Parse(e.CellValue1.ToString()), b = int.Parse(e.CellValue2.ToString());
                // If the cell value is already an integer, just cast it instead of parsing
                e.SortResult = a.CompareTo(b);
                e.Handled = true;
            }
        }

        private void ShowModelFileList()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(PathMgr.Inst.PATH_RECIPE);

                cmbTargetRecipeIdx.Items.Clear();
                gridModel.Rows.Clear();

                for (int nRcpCnt = 1; nRcpCnt <= 100; nRcpCnt++)
                {
                    cmbTargetRecipeIdx.Items.Add(nRcpCnt);
                    gridModel.Rows.Add(nRcpCnt, "", "", "", "");
                }

                foreach (FileInfo f in dir.GetFiles("*.mdl"))
                {
                    string strPath = RecipeMgr.Inst.GetRecipePath(f.Name);
                    Recipe rcp = Recipe.Deserialize(strPath);

                    string[] arr = new string[5];
                    arr[0] = rcp.strNo;
                    arr[1] = f.Name.Replace(".mdl", "");
                    arr[2] = rcp.strRecipeDescription;
                    arr[3] = Convert.ToString(f.CreationTime);
                    arr[4] = Convert.ToString(f.LastWriteTime);

                    //if (!IsAcceptTargetRecipeId(arr[0])) continue;
                    try
                    {
                        gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[0].Value = arr[0];
                        gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[1].Value = arr[1];
                        gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[2].Value = arr[2];
                        gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[3].Value = arr[3];
                        gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[4].Value = arr[4];

                        cmbTargetRecipeIdx.Items.Remove(int.Parse(rcp.strNo));
                    }
                    catch { }
                }

                cmbTargetRecipeIdx.SelectedIndex = 0;
            }
            catch (Exception)
            {

            }
        }

        private int GetRegRecipeCount()
        {
            DirectoryInfo dir = new DirectoryInfo(PathMgr.Inst.PATH_RECIPE);
            int nCnt = 0;
            foreach (FileInfo f in dir.GetFiles("*.mdl"))
            {
                nCnt++;
            }
            return nCnt;
        }

        private void gridModel_CurrentCellChanged(object sender, EventArgs e)
        {
            if (gridModel.CurrentCell == null) return;
            if (gridModel.Rows[gridModel.CurrentCell.RowIndex].Cells[1].Value == null) return;
            txtRcpSoruce.Text = gridModel.Rows[gridModel.CurrentCell.RowIndex].Cells[1].Value.ToString();
        }

        public bool FindFileName(string strName)
        {
            string pathExeFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\WorkFile";
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(pathExeFile);

            foreach (System.IO.FileInfo f in di.GetFiles())
            {
                if (strName == f.Name)
                    return false;
            }

            return true;
        }

        private void btnRcpCopy_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(FormTextLangMgr.FindKey("Do you want Copy File ?"), "Confirm!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            //string targetModelName = tbxRcpTarget.Text;
            string targetModelName = tbxTargetRecipeID.Text + ".mdl";

            if (!FindFileName(targetModelName))
            {
                MessageBox.Show("Already Model.");
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnRcpCopy_Click" + ", Model Copy [" + txtRcpSoruce.Text + ".mdl]" + " --> [" + targetModelName + "]");
                return;
            }

            if (!RecipeMgr.Inst.CopyToModel(cmbTargetRecipeIdx.SelectedItem.ToString(), tbxTargetRecipeID.Text + ".mdl"))
            {
                MessageBox.Show("Failed to copy model!!!");
            }

            ShowModelFileList();
        }

        private void btnRcpDelete_Click(object sender, EventArgs e)
        {
            try
            {
                string nDeletePPid = txtRcpSoruce.Text;
                string strDelRecipe = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\WorkFile\\" + txtRcpSoruce.Text + ".mdl";

                if (!File.Exists(strDelRecipe))
                {
                    MessageBox.Show(txtRcpSoruce.Text + FormTextLangMgr.FindKey(" Delete recipe file not found!"));
                    return;
                }

                DialogResult result = MessageBox.Show(string.Format(FormTextLangMgr.FindKey("Do you want to delete") + " [{0}]" + FormTextLangMgr.FindKey("recipe file?"), txtRcpSoruce.Text),
                                                      "Confirm!",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question,
                                                      MessageBoxDefaultButton.Button2);
                if (result != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                if (txtRcpSoruce.Text == RecipeMgr.Inst.LAST_MODEL_NAME.Replace(".mdl", ""))
                {
                    MessageBox.Show("Cannot delete. It's Current Model.");
                    return;
                }

                if (gridModel.Rows.Count <= 1)
                {
                    MessageBox.Show("Cannot delete. It's Last Model.");
                    return;
                }

                RecipeMgr.Inst.DeleteModelFile(txtRcpSoruce.Text);
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnRcpDelete_Click" + ", [Deleted Model Name: " + txtRcpSoruce.Text + ".mdl]");

                ShowModelFileList();
                MessageBox.Show("Model deleted.");
            }
            catch (System.Exception ex)
            {
                //
            }
        }

        private void btnRcpChange_Click(object sender, EventArgs e)
        {
            try
            {
                if (gridModel.CurrentCell == null) return;
                if (gridModel.Rows[gridModel.CurrentCell.RowIndex].Cells[1].Value == null) return;
                string strChangeId = gridModel.Rows[gridModel.CurrentCell.RowIndex].Cells[1].Value.ToString();


                DialogResult result = MessageBox.Show("Do you want to change recipe? [" + strChangeId + "]",
                                                      "Confirm!",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question,
                                                      MessageBoxDefaultButton.Button2);
                if (result != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnRcpChange_Click" + ", Model Changed: [" + RecipeMgr.Inst.LAST_MODEL_NAME + " --> " + strChangeId + ".mdl]");
                txtRcpSoruce.Text = strChangeId;
                RecipeMgr.Inst.ChangeModelFile(txtRcpSoruce.Text + ".mdl");

                ShowModelFileList();

                GbVar.bChangeRecipe_Main = true;
                GbVar.bChangeRecipe_Recipe = true;

                popMessageBox pMsg = new popMessageBox("RECIPE CHANGE SUCCSEE!", "RECIPE CHANGE");
                pMsg.ShowDialog();
            }
            catch (System.Exception ex)
            {
                //
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GbVar.bChangeRecipe_Main = true;
            GbVar.bChangeRecipe_Recipe = true;
            Close();
        }

        /// <summary>
        /// 1~100사이에 ID인가
        /// </summary>
        private bool IsAcceptTargetRecipeId(string strId)
        {
            //             if (strId == "" || strId == null) return false;
            //             int nId = 0;
            //             if (!int.TryParse(strId, out nId)) return false;
            //             if (nId < 1 || nId > 100) return false;
            //             return true;

            if (strId == "" || strId == null) return false;
            if (strId.Length > 32) return false;
            char[] chInvalidArray = Path.GetInvalidFileNameChars();

            foreach (char ch in chInvalidArray)
            {
                if (strId.Contains(ch))
                    return false;
            }

            if (strId.Contains(' '))
                return false;

            return true;

        }

        private void gridModel_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (gridModel.CurrentCell == null) return;
            int nRow = gridModel.CurrentCell.RowIndex;      //Y
            int nCol = gridModel.CurrentCell.ColumnIndex;   //X

            // Model Name Change
            if (nCol == 2)
            {
                if (!RecipeMgr.Inst.ModelDescriptionChange(gridModel.Rows[nRow].Cells[1].Value.ToString() + ".mdl", gridModel.Rows[nRow].Cells[2].Value.ToString()))
                {
                    MessageBox.Show("Failed to change model name!!!");
                }
            }
            
        }

        private void gridModel_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && (e.PaintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
            //{
            //    string[] arr = RecipeMgr.Inst.LAST_MODEL_NAME.Split('.');
            //    int nCurrRcpNo = int.Parse(arr[0]);

            //    if (e.RowIndex == nCurrRcpNo - 1)
            //    {
            //        using (Brush gridBrush = new SolidBrush(this.gridModel.GridColor))
            //        {
            //            using (Brush backColorBrush = new SolidBrush(Color.DarkOrange))
            //            {
            //                using (Pen gridLinePen = new Pen(gridBrush))
            //                {
            //                    // Clear cell
            //                    e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
            //                    //force paint of content
            //                    e.PaintContent(e.ClipBounds);
            //                    e.Handled = true;
            //                }
            //            }
            //        }
            //    }
            //    else if (e.RowIndex == gridModel.CurrentCell.RowIndex)
            //    {
            //        using (Brush gridBrush = new SolidBrush(this.gridModel.GridColor))
            //        {
            //            using (Brush backColorBrush = new SolidBrush(Color.FromArgb(192, 255, 255)))
            //            {
            //                using (Pen gridLinePen = new Pen(gridBrush))
            //                {
            //                    // Clear cell
            //                    e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
            //                    //force paint of content
            //                    e.PaintContent(e.ClipBounds);
            //                    e.Handled = true;
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        using (Brush gridBrush = new SolidBrush(this.gridModel.GridColor))
            //        {
            //            using (Brush backColorBrush = new SolidBrush(Color.FromArgb(42, 42, 42)))
            //            {
            //                using (Pen gridLinePen = new Pen(gridBrush))
            //                {
            //                    // Clear cell
            //                    e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
            //                    //force paint of content
            //                    e.PaintContent(e.ClipBounds);
            //                    e.Handled = true;
            //                }
            //            }
            //        }
            //    }
            //    DataGridViewPaintParts paintParts = e.PaintParts & ~DataGridViewPaintParts.Background;
            //    e.Paint(e.ClipBounds, paintParts);
            //    e.Handled = true;
            //}
        }

        private void btnRcpUpload_Click(object sender, EventArgs e)
        {
            //물어 보는 팝업창 띄우기
            DialogResult result = MessageBox.Show("Do you want to Upload recipe? [" + txtRcpSoruce.Text + "]",
                                                    "Confirm!",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question,
                                                    MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }


            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
            {
                MessageBox.Show("GEM SKIP MODE!");
                return;
            }

            //설비 운영 중에는 해당 시나리오 사용 할 수 없음.
            for (int nCnt = 0; nCnt < GbVar.mcState.isCycleRunReq.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                if (GbVar.mcState.isCycleRunReq[nCnt] == true || GbVar.mcState.isCycleRun[nCnt] == true)
                {
                    MessageBox.Show("설비 구동 중에는 레시피 업로드를 할 수 없습니다.");
                    return;
                }
            }

            ShowDigPopRmsRunBlock("Recipe Parameter Uploading");
        }
		
		void ShowPreview()
        {
            string strPath = RecipeMgr.Inst.GetRecipePath(txtRcpSoruce.Text + ".mdl");


            Recipe m_Rcp = Recipe.Deserialize(strPath);

            if (m_Rcp == null) return;
            //MGZ INFO
            lbPrevMgzFileName.Text = string.Format("규격: {0}", m_Rcp.MgzInfo.strMgzFileName.Replace(".xml", ""));
            lbPrevMgzSlotPitch.Text = string.Format("슬롯 간격: {0} mm", m_Rcp.MgzInfo.dSlotPitch);
            lbPrevMgzSlotCount.Text = string.Format("슬롯 수량: {0} ea", m_Rcp.MgzInfo.nSlotCount);

            //CLEANING INFO
            //20211117 bhoh ToDo
            //lbPrevCleanUnitSpgWat.Text = string.Format("스펀지: {0}회, 이동속도: {1:0.000} mm/s", m_Rcp.cleaning.nUnitPkSpongeCount, m_Rcp.cleaning.dUnitPkSpongeVel);
            //lbPrevCleanUnitAirBlw.Text = string.Format("워터 클리닝: {0}회, 드라이 에어: {1} 회, 드라이 이동속도: {2:0.000} mm/s", m_Rcp.cleaning.nFirstCleanWaterCount, m_Rcp.cleaning.nUnitPkDryBlowCount, m_Rcp.cleaning.dUnitPkDryBlowVel);
            //lbPrevCleanTableWatAir.Text = string.Format("워터: {0}회, 에어: {1} 회, 이동속도: {2:0000} mm/s", m_Rcp.cleaning.nCleanTableWaterCount, m_Rcp.cleaning.nDryBlockAirCount, m_Rcp.cleaning.dDryBlockAirVel);
            lbPrevCleanDryBlw.Text = string.Format("드라이 테이블 블로우 언로딩 속도: {0:0.000}mm/s", m_Rcp.cleaning.dStageBlockDryBlowVel);

            //VISION INFO
            lbPrevVisionFileName.Text = string.Format("규격: {0}", m_Rcp.MapTbInfo.strMapTbFileName.Replace(".xml", ""));
            lbPrevVisionChipPitch.Text = string.Format("칩 간격: {0} * {1} mm", m_Rcp.MapTbInfo.dUnitPitchX, m_Rcp.MapTbInfo.dUnitPitchY);
            lbPrevVisionChipCount.Text = string.Format("칩 수량: {0} * {1}", m_Rcp.MapTbInfo.nUnitCountX, m_Rcp.MapTbInfo.nUnitCountY);
            lbPrevVisionChipCount.Text = string.Format("칩 검사 수량: {0} * {1}", m_Rcp.MapTbInfo.nInspChipViewCountX, m_Rcp.MapTbInfo.nInspChipViewCountY);

            //TRAY INFO
            lbPrevTrayFileName.Text = string.Format("규격: {0}", m_Rcp.TrayInfo.strTrayFileName.Replace(".xml", ""));
            lbPrevTrayChipPitch.Text = string.Format("칩 간격: {0} * {1} mm", m_Rcp.TrayInfo.dTrayPitchX, m_Rcp.TrayInfo.dTrayPitchY);
            lbPrevTrayChipCount.Text = string.Format("칩 수량: {0} * {1}", m_Rcp.TrayInfo.nTrayCountX, m_Rcp.TrayInfo.nTrayCountY);
            lbPrevTrayThickness.Text = string.Format("트레이 두께: {0} mm", m_Rcp.TrayInfo.dTrayThickness);
        }

        private void txtRcpSoruce_TextChanged(object sender, EventArgs e)
        {
            ShowPreview();
        }

        protected void popRms_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_popRmsRun.FormClosing -= popRms_FormClosing;
            if (m_popRmsRun != null)
            {
                m_popRmsRun.Dispose();
            }
            m_popRmsRun = null;
        }
        
        protected void ShowDigPopRmsRunBlock(string strTitle)
        {
            m_popRmsRun = new POP.popRmsRun();
            m_popRmsRun.FormClosing += popRms_FormClosing;
            m_popRmsRun.SetLabelTextChange(strTitle);
            if (m_popRmsRun.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                m_popRmsRun.Close();
            }
        }

        protected void CloseDigPopRmsRun()
        {
            if (m_popRmsRun != null)
            {
                m_popRmsRun.Close();
            }
        }

        public void deleRecipeUploadComp(bool bSuccess, string strmsg = "")
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    CloseDigPopRmsRun();

                    //if (bSuccess == true)
                    //{
                    //    popMessageBox pMsg = new popMessageBox("RECIPE UPLOAD SUCCESS!", "RECIPE UPLOAD");
                    //    pMsg.ShowDialog();
                    //}
                    //else
                    //{
                    //    popMessageBox pMsg = new popMessageBox("RECIPE UPLOAD FAIL!", "RECIPE UPLOAD");
                    //    pMsg.ShowDialog();

                    //}
                });
            }
            else
            {
                CloseDigPopRmsRun();

                //if (bSuccess == true)
                //{
                //    popMessageBox pMsg = new popMessageBox("RECIPE UPLOAD SUCCESS!", "RECIPE UPLOAD");
                //    pMsg.ShowDialog();
                //}
                //else
                //{
                //    popMessageBox pMsg = new popMessageBox("RECIPE UPLOAD FAIL!", "RECIPE UPLOAD");
                //    pMsg.ShowDialog();

                //}
            }
        }


        public void deleRecipeChangeComp(bool bSuccess, string strmsg = "")
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    CloseDigPopRmsRun();
                    //if (bSuccess == true)
                    //{
                    //    popMessageBox pMsg = new popMessageBox("RECIPE CHANGE SUCCESS!", "RECIPE CHANGE");
                    //    pMsg.ShowDialog();
                    //}
                    //else
                    //{
                    //    popMessageBox pMsg = new popMessageBox("RECIPE CHANGE FAIL!", "RECIPE CHANGE");
                    //    pMsg.ShowDialog();
                    //}
                });
            }
            else
            {
                CloseDigPopRmsRun();
                //if (bSuccess == true)
                //{
                //    popMessageBox pMsg = new popMessageBox("RECIPE CHANGE SUCCESS!", "RECIPE CHANGE");
                //    pMsg.ShowDialog();
                //}
                //else
                //{
                //    popMessageBox pMsg = new popMessageBox("RECIPE CHANGE FAIL!", "RECIPE CHANGE");
                //    pMsg.ShowDialog();
                //}
            }
        }
    }
}
