using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public class BaseSubMenu : Form
    {
        public class LightControl
        {
            public NumericUpDown num;
            public TrackBar trackBar;
        }

        protected List<ucManualOutputButton> m_listOuputButton = new List<ucManualOutputButton>();
        protected List<InputPanel> m_listInputPanel = new List<InputPanel>();
        
        //protected List<PosGroupMoveEx> m_listMotPosGroupMoveBtn = new List<PosGroupMoveEx>();

        protected LightControl[] m_LightControlArray = new LightControl[20];

        protected POP.popManualRun m_popManualRun = null;

        /// <summary>
        /// UserControl을 소유하고 있는 Form의 Timer에서 호출할 함수. override하여 사용한다.
        /// </summary>
        public virtual void RefreshUI()
        {
            #region Input Panel, Output Button 갱신
            
            foreach (ucManualOutputButton btn in m_listOuputButton)
            {
                if (btn.Output != IODF.OUTPUT.NONE && btn.Output != IODF.OUTPUT.MAX)
                {
                    btn.On = GbVar.GB_OUTPUT[(int)btn.Output] == 1;
                }
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine(string.Format("Output Button {0} is not defined OUTPUT {1}", btn.Name, btn.ButtonText));
                //}
            }

            foreach (InputPanel item in m_listInputPanel)
            {
                if (item.Input != IODF.INPUT.NONE && item.Input != IODF.INPUT.MAX)
                {
                    item.On = GbVar.GB_INPUT[(int)item.Input] == 1;
                }
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine(string.Format("Input Panel {0} is not defined INPUT", item.Name));
                //}
            }
            #endregion


        }

        protected void FindControls(Control.ControlCollection collection)
        {
            m_LightControlArray.Populate(null);

            LoopFindControls(collection);

            LinkLightControl();
        }

        void LoopFindControls(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    #region INPUT PANEL
                    if (ctr.GetType() == typeof(InputPanel))
                    {
                        m_listInputPanel.Add((InputPanel)ctr);
                    }
                    #endregion

                    #region OUTPUT BUTTON
                    if (ctr.GetType() == typeof(ucManualOutputButton))
                    {
                        m_listOuputButton.Add((ucManualOutputButton)ctr);
                    }
                    #endregion

                    #region Light Control
                    if (ctr.Name.Contains("ledBright"))
                    {
                        string tag;
                        int nIndex;
                        string[] arr;

                        tag = ctr.Tag as string;
                        if (string.IsNullOrEmpty(tag)) continue;

                        arr = ctr.Name.Split(new char[] { '_' });
                        if (arr.Length != 2) continue;

                        if (int.TryParse(arr[1], out nIndex) == false)
                            continue;

                        if (m_LightControlArray[nIndex] == null)
                            m_LightControlArray[nIndex] = new LightControl();

                        if (ctr.GetType() == typeof(NumericUpDown))
                        {
                            m_LightControlArray[nIndex].num = (NumericUpDown)ctr;
                        }
                        else if (ctr.GetType() == typeof(TrackBar))
                        {
                            m_LightControlArray[nIndex].trackBar = (TrackBar)ctr;
                        }
                    }
                    #endregion

                    if (ctr.Controls.Count > 0)
                        LoopFindControls(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        #region 조명 연결 및 이벤트
        public void LinkLightControl()
        {
            foreach (LightControl item in m_LightControlArray)
            {
                if (item == null) continue;
                if (item.num == null || item.trackBar == null) continue;

                item.num.ValueChanged += num_ValueChanged;
                item.trackBar.ValueChanged += trackBar_ValueChanged;
            }
        }

        public void UnLinkLightControl()
        {
            foreach (LightControl item in m_LightControlArray)
            {
                if (item == null) continue;
                if (item.num == null || item.trackBar == null) continue;

                item.num.ValueChanged -= num_ValueChanged;
                item.trackBar.ValueChanged -= trackBar_ValueChanged;
            }
        }

        void trackBar_ValueChanged(object sender, EventArgs e)
        {
            TrackBar trk = sender as TrackBar;

            int nIndex;
            string[] arr;

            // 동일한 이름으로 NumericUpDown을 찾아 값 변경
            // trkledBright_1
            // nmcledBright_1
            // Control의 아이템은 서로 이름을 맞추어 같은 조명을 바꾸는 역할
            // NumericUpDown의 Tag는 실제 조명 컨트롤러의 인덱스
            arr = trk.Name.Split(new char[] { '_' });
            if (arr.Length != 2) return;

            if (int.TryParse(arr[1], out nIndex) == false)
                return;

            m_LightControlArray[nIndex].num.Value = trk.Value;
        }

        void num_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            int nTag = num.GetTag();

            if (nTag < 0) return;

            //GbDevice.ledBright.SetLight(nTag, (int)num.Value);
        } 
        #endregion

        public virtual void UpdateData()
        {

        }

        public virtual void SaveData()
        {

        }

        public virtual void AfterSaveData()
        {

        }

        /// <summary>
        /// 저장하기 전 갱신할 데이터
        /// </summary>
        public virtual void RefreshBeforeSave()
        {

        }

        #region Manual Run 중이라는 표시 팝업 창
        protected virtual void OnFinishSeq(int nResult)
        {
            //System.Diagnostics.Debug.WriteLine("Finished");

            if (this.InvokeRequired)
            {
                BeginInvoke(new CommonEvent.FinishSeqEvent(OnFinishSeq), nResult);
                return;
            }

            if (m_popManualRun != null && !m_popManualRun.IsDisposed)
            {
                if (nResult == 0) m_popManualRun.DialogResult = System.Windows.Forms.DialogResult.OK;
                m_popManualRun.Close();
            }
        }

        protected void popManual_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_popManualRun.FormClosed -= popManual_FormClosed;
            if (m_popManualRun != null)
            {
                m_popManualRun.Dispose();
            }
            m_popManualRun = null;
        }

        protected void ShowDlgPopManualRunBlock()
        {
            m_popManualRun = new POP.popManualRun();
            m_popManualRun.FormClosed += popManual_FormClosed;
            if (m_popManualRun.ShowDialog(Application.OpenForms[0]) == System.Windows.Forms.DialogResult.Cancel)
                GbSeq.manualRun.LeaveCycle(-1);
        } 
        #endregion
    }
}
