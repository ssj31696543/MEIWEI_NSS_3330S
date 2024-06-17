using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

namespace NSS_3330S.POP
{
    public partial class popAccount : Form
    {
        private List<LoginInfo> m_loginInfos = new List<LoginInfo>();
        private List<UserColorInfo> m_usercolorInfos = new List<UserColorInfo>();
        int nLevel = 0;
        bool bIsEnable = false;
        bool bIsFirstOpen = false;

        #region Move Window Form Without Tilte Bar (드래그로 창 움직이기)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void pnlTitleBar_MouseDown(object sender,
        System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        public popAccount()
        {
            InitializeComponent();
        }

        private void popAccount_Load(object sender, EventArgs e)
        {
            nLevel = 0;
            bIsEnable = false;
            bIsFirstOpen = true;
            LoadAccountData();
            this.CenterToScreen();
        }

        public void LoadAccountData()
        {
            LoginInfoList obj = Deserialize(PathMgr.Inst.PATH_PRODUCT_USERINFO);
            if (obj == null)
            {
                obj = new LoginInfoList();
                for (int i = 0; i < 3; i++)
                {
                    LoginInfo login = new LoginInfo();
                    obj.m_listLogInfo.Add(login);
                }
                m_loginInfos = obj.m_listLogInfo;
            }
            m_loginInfos = obj.m_listLogInfo;

           UserColorInfoList c_obj = C_Deserialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO);
            if (c_obj == null)
            {
                c_obj = new UserColorInfoList();
                for (int i = 0; i < 3; i++)
                {
                    UserColorInfo c_login = new UserColorInfo();
                    c_obj.m_listColorInfo.Add(c_login);
                }
                m_usercolorInfos = c_obj.m_listColorInfo;
            }
            m_usercolorInfos = c_obj.m_listColorInfo;
        }

        public void SaveAccountInfo()
        {
            GbVar.loginInfos = m_loginInfos;
            GbVar.UserColorInfos = m_usercolorInfos;

            popMessageBox msg;

            if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USERINFO, new LoginInfoList(GbVar.loginInfos)))
            {
                msg = new popMessageBox(FormTextLangMgr.FindKey("FAIL TO SAVE!!"), "");
                msg.ShowDialog();
                // 실패
            }
            else
            {
                if(!Serialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO, new UserColorInfoList(GbVar.UserColorInfos)))
                {
                    msg = new popMessageBox(FormTextLangMgr.FindKey("FAIL Color TO SAVE!!"), "");
                    msg.ShowDialog();
                    return;
                }
                LoadAccountData();
                msg = new popMessageBox(FormTextLangMgr.FindKey("SAVE COMPLETE"), "");
                msg.ShowDialog();
                pbxSave.BackColor = Color.FromArgb(0, 0, 34);
            }
        }

        protected LoginInfoList Deserialize(string path)
        {
            LoginInfoList obj = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoginInfoList));
                using (StreamReader rd = new StreamReader(path))
                {
                    obj = (LoginInfoList)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }

            return obj;
        }
        protected UserColorInfoList C_Deserialize(string path)
        {
            UserColorInfoList obj = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserColorInfoList));
                using (StreamReader rd = new StreamReader(path))
                {
                    obj = (UserColorInfoList)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }

            return obj;
        }
        protected bool Serialize(string path, LoginInfoList obj)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoginInfoList));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, obj);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }
        protected bool Serialize(string path, UserColorInfoList obj)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserColorInfoList));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, obj);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public bool bIsAccountExist(string strChkID)
        {
            for (int i = 0; i < m_loginInfos.Count; i++)
            {
                if (m_loginInfos[i].USER_ID == strChkID)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddAccountInfo()
        {
            popMessageBox pMsg;

            if (bIsAccountExist(tbxId.Text) == true)
            {
                pMsg = new popMessageBox("ALREADY EXIST ID", "FAIL TO CREATE", MessageBoxButtons.OK);
                return;
            }

            LoginInfo m_Info = new LoginInfo(tbxId.Text, tbxPw.Text, nLevel);
            UserColorInfo m_ColorInfo = new UserColorInfo(tbxId.Text);

            m_loginInfos.Add(m_Info);
            m_usercolorInfos.Add(m_ColorInfo);
            GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("Account Added ID: {0}, Level: {1}", m_Info.USER_ID, nLevel));

            pMsg = new popMessageBox("ACCOUNT CREATED", "SUCCESS", MessageBoxButtons.OK);
            pbxSave.BackColor = Color.Orange;

        }

        public void DeleteAccountInfo()
        {
            popMessageBox pMsg;

            if (bIsAccountExist(tbxId.Text) == false)
            {
                pMsg = new popMessageBox("ID IS NOT EXIST", "FAIL TO DELETE", MessageBoxButtons.OK);
                return;
            }
            if (m_loginInfos.Count <= 1)
            {
                pMsg = new popMessageBox("PASSWORD IS NOT CORRECT", "FAIL TO DELETE", MessageBoxButtons.OK);
                return;
            }
            for (int i = 0; i < m_usercolorInfos.Count; i++)
            {
                if (m_usercolorInfos[i].USER_ID == tbxId.Text)
                {
                    m_usercolorInfos.RemoveAt(i);
                }
            }
            for (int i = 0; i < m_loginInfos.Count; i++)
            {
                if (m_loginInfos[i].USER_ID == tbxId.Text)
                {
                    GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("Account Deleted ID: {0}, Level: {1}", m_loginInfos[i].USER_ID, nLevel));
                    m_loginInfos.RemoveAt(i);
                    pMsg = new popMessageBox("ACCOUNT DELETED", "SUCCESS", MessageBoxButtons.OK);
                    pbxSave.BackColor = Color.Orange;
                }
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if( GbVar.CurLoginInfo.USER_LEVEL == MCDF.LEVEL_MAK)
            {
                if (!btnOp.Visible)
                    btnOp.Visible = true;
                if (!btnEng.Visible)
                    btnEng.Visible = true;
            }
            else
            {
                if (!btnOp.Visible)
                btnOp.Visible = true;
                if (btnEng.Visible)
                btnEng.Visible = false;
            }

            // 버튼 색상 업데이트
            if (nLevel == 0)
            {
                btnOp.BackColor = Color.SkyBlue;
                btnEng.BackColor = SystemColors.ButtonFace;
                btnMaker.BackColor = SystemColors.ButtonFace;
            }
            else if (nLevel == 1)
            {
                btnOp.BackColor = SystemColors.ButtonFace;
                btnEng.BackColor = Color.SkyBlue;
                btnMaker.BackColor = SystemColors.ButtonFace;
            }
            else if (nLevel == 2)
            {
                btnOp.BackColor = SystemColors.ButtonFace;
                btnEng.BackColor = SystemColors.ButtonFace;
                btnMaker.BackColor = Color.SkyBlue;
            }

            // 패스워드 텍스트박스 업데이트
            tbxPw.PasswordChar = tbxPw.Text == "PASSWORD" ? '\0' : '●';
            tbxPw2.PasswordChar = tbxPw2.Text == "PASSWORD" ? '\0' : '●';

            // 버튼 Enable 업데이트
            bIsEnable = false;
            if (!string.IsNullOrEmpty(tbxId.Text))
            {
                if (tbxPw.Text == tbxPw2.Text && tbxPw.Text != "PASSWORD")
                {
                    bIsEnable = true;
                    lbWarningMsg.Visible = false;
                }
                else
                {
                    lbWarningMsg.Visible = true;
                    lbWarningMsg.Text = "Password Is Not Correct!!";
                }
            }
            else
            {
                lbWarningMsg.Visible = true;
                lbWarningMsg.Text = "Password Is Empty!!";
            }
            btnCreateId.Enabled = bIsEnable;
            btnDeleteId.Enabled = bIsEnable;
        }

        private void btnCreateId_Click(object sender, EventArgs e)
        {
            AddAccountInfo();
        }

        private void btnDeleteId_Click(object sender, EventArgs e)
        {
            DeleteAccountInfo();
        }

        private void pbxSave_Click(object sender, EventArgs e)
        {
            SaveAccountInfo();
            Close();
        }

        private void btnOp_Click(object sender, EventArgs e)
        {
            nLevel = 0;
        }

        private void btnEng_Click(object sender, EventArgs e)
        {
            nLevel = 1;
        }

        private void btnMaker_Click(object sender, EventArgs e)
        {
            nLevel = 2;
        }

        private void pbxClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDeleteId_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw Border using color specified in Flat Appearance
            Pen pen = new Pen(btnDeleteId.FlatAppearance.BorderColor, 1);
            Rectangle rectangle = new Rectangle(0, 0, Size.Width - 1, Size.Height - 1);
            e.Graphics.DrawRectangle(pen, rectangle);
        }

        private void tbxId_MouseClick(object sender, MouseEventArgs e)
        {
            if(bIsFirstOpen)
            {
                tbxId.Text = "";
                tbxPw.Text = "";
                tbxPw2.Text = "";
                bIsFirstOpen = false;
            }
        }


        private void popAccount_VisibleChanged(object sender, EventArgs e)
        {
            string strOpText = "OPERATOR";
            string strEngineer = "ENGINEER";
            string strMaker = "MAKER";
            string strCreate = "CREATE";
            string strDelete = "DELETE";
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
            {
                strOpText = "操作人员";
                strEngineer = "工程师";
                strMaker = "开发者";
                strCreate = "创建";
                strDelete = "删除";
            }
            else
            {
                strOpText = "OPERATOR";
                strEngineer = "ENGINEER";
                strMaker = "MAKER";
                strCreate = "CREATE";
                strDelete = "DELETE";
            }
            if (this.Visible)
            {
                btnOp.Text = strOpText;
                btnEng.Text = strEngineer;
                btnMaker.Text = strMaker;
                btnCreateId.Text = strCreate;
                btnDeleteId.Text = strDelete;
            }
        }
    }
}
