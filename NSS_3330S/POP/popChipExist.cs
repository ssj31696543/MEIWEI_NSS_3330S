using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
namespace NSS_3330S.POP
{
    public partial class popChipExist : Form
    {
        Point mouseLocation;
        public int m_nErrNo = 0;
        public STRIP_MDL m_eMdl = 0;
        public string strAlarmText = "";
        private Stopwatch swTimeStamp = Stopwatch.StartNew();

        public delegate void ResetDelegate();
        public event ResetDelegate OnResetEvent;
        public delegate void addCurAlarmEvent(string strDateTime, int nErr, string strAlarm);
        public event addCurAlarmEvent addAlarmEvent;

        public bool bFDir;
        public int nMapTableNo;
        public int nHeadNo;
        public int nTablePickUpColCount;

        int nPad = 0;
        public popChipExist()
        {
            InitializeComponent();

            swTimeStamp.Stop();
            this.CenterToScreen();
        }

        private void lbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLocation = new Point(-e.X, -e.Y);
        }

        private void lbTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseLocation.X, mouseLocation.Y);
                Location = mousePos;
            }
        }

        public void SetErrorText(int nErrNo)
        {
            try
            {
                m_nErrNo = nErrNo;
                if (nErrNo < 0)
                {
                    tmrErr.Enabled = true;
                    swTimeStamp.Restart();
                }
                else
                {
                    ReturnPadNo(m_nErrNo, ref nHeadNo, ref nPad);

                    switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
                    {
                        case Language.ENGLISH:
                            lblErrNo.Text = String.Format("PICKER HEAD{0} PAD{1} VAC ALARM!", nHeadNo, nPad);
                            lblErrNo.Text += "\r\nDo you want to retry?";
                            break;
                        case Language.CHINA:
                            lblErrNo.Text = String.Format("分拣手臂#{0} 中 吸嘴#{1} 真空异常报警", nHeadNo, nPad);
                            lblErrNo.Text += "\r\n是否重新运行?";
                            break;
                        case Language.KOREAN:
                            lblErrNo.Text = String.Format("PICKER HEAD{0} PAD{1} VAC알람 발생!", nHeadNo, nPad);
                            lblErrNo.Text += "\r\n픽업 재시도 하시겠습니까?";
                            break;
                    }
                    ErrStruct err = new ErrStruct(nErrNo, 1);
                    ErrMgr.Inst.ErrStatus.Add(err);

                    ErrorInfo eInfo = ErrMgr.Inst.errlist[nErrNo];

                    tmrErr.Enabled = true;
                    swTimeStamp.Restart();

                    strAlarmText = eInfo.NAME;
                    if (addAlarmEvent != null)
                        addAlarmEvent(System.DateTime.Now.ToString("yyyyMMddHHmmssfff"), nErrNo, eInfo.NAME);

                    string strnow = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    GbFunc.WriteErrorLog(eInfo.NAME, strnow, nErrNo);

                    int nBuzzer = 1;
                    //if (int.TryParse(eInfo.BUZZER.Substring(eInfo.BUZZER.Length - 1), out nBuzzer))
                    {
                        MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER + (nBuzzer - 1), true);
                    }

                    SetSkipParam();
                }
            }
            catch (Exception)
            {

            }
        }

        public void ResetError()
        {
            for (int i = 0; i < ErrMgr.Inst.ErrStatus.Count; i++)
            {
                ErrMgr.Inst.ErrStatus[i].Set = 0;
            }

            Thread.Sleep(500);

            //GbVar.mcState.isErr = false;

            strAlarmText = "";
            tmrErr.Enabled = false;
            swTimeStamp.Stop();

            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER, false);
            this.Invoke((MethodInvoker)delegate ()
            {
                Hide();
            });
        }

        private void tmrErr_Tick(object sender, EventArgs e)
        {
            string strElapsedTime = String.Format("{0:00}", swTimeStamp.Elapsed.Hours) + ":";
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Minutes) + ":");
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Seconds));
            lblTime.DigitText = strElapsedTime;
            if (lblErrNo.ForeColor == Color.OrangeRed) { lblErrNo.ForeColor = Color.FromArgb(50, 50, 50); }
            else { lblErrNo.ForeColor = Color.OrangeRed; }
        }

        private void btnBuzzerStop_Click(object sender, EventArgs e)
        {
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER, false);
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            SetSkipUnit();

            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();
        }

        private void ReturnPadNo(int nError, ref int nHeadNo, ref int nPadNo)
        {
            int nOffset = ((nError - (int)ERDF.E_X1_PK_VAC_1_NOT_ON) / 2);

            nHeadNo = (nOffset / 8) + 1;
            nPadNo = (nOffset % 8) + 1;
        }

        private void SetSkipParam()
        {
            this.bFDir = GbVar.padSkipParam.FDir; 
            this.nMapTableNo = GbVar.padSkipParam.MapTableNo; 
            //this.nHeadNo = GbVar.padSkipParam.HeadNo;
        }

        private void SetSkipUnit()
        {
            if (nHeadNo == 0) return;
            
            GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo -1].isExistSkipUnit = true;
            GbVar.Seq.sMapVisionTable[nMapTableNo].IsExistSkipUnit = true;
            //int nRow, nCol;

            //if (bFDir == true)
            //{
            //    nRow = nTablePickUpTotCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            //    nCol = nTablePickUpColCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            //}
            //else
            //{
            //    nRow = nTablePickUpTotCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            //    nCol = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - (nTablePickUpColCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX) - 1;
            //}

            //GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].IS_SKIP_UNIT = true;
        }

    }
}
