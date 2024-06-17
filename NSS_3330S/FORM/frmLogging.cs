using NSS_3330S.POP;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NSS_3330S.FORM
{
    public partial class frmLogging : Form
    {
        public delegate void UserLogin(int nScrNo);
        public event UserLogin userLogin = null;
        popAccount pAccount = null;
        int _SelectedLevel = 0;
        popLogin pChange = null;

        public frmLogging()
        {
            InitializeComponent();
        }

        int SelectedUserLevel
        {
            get
            {
                return _SelectedLevel;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            bool Color_ck = true;
            //if (userLogin != null) userLogin(1);
            LoadFile();
            UserColorInfoList colorInfoList = C_Deserialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO);
            if (colorInfoList == null)
            {
                colorInfoList = new UserColorInfoList();
                for (int i = 0; i < GbVar.loginInfos.Count; i++)
                {
                    UserColorInfo userColor = new UserColorInfo(GbVar.loginInfos[i].USER_ID);
                    colorInfoList.m_listColorInfo.Add(userColor);
                    GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                }
                if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO, new UserColorInfoList(GbVar.UserColorInfos)))
                { }
                GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
            }

            GbVar.strLoggingUerID = tbxUserID.Text;
            popMessageBox msg = new popMessageBox("-", "LOGIN");

            for (int i = 0; i < GbVar.loginInfos.Count; i++)
            {
                // 접속하려는 ID와 레벨이 맞으면 패스워드 검사를 한다.
                if (GbVar.loginInfos[i].USER_ID == tbxUserID.Text &&
                    GbVar.loginInfos[i].USER_LEVEL == SelectedUserLevel ||
                    (GbVar.loginInfos[i].USER_LEVEL == MCDF.LEVEL_MAK &&
                    GbVar.loginInfos[i].USER_LEVEL == SelectedUserLevel))
                {
                    // 패스워드가 맞으면 로그인을 한다.
                    if (GbVar.loginInfos[i].PASSWORD == txtPass.Text)
                    {
                        // 로그인 계정의 색상 정보를 불러온다 0423KTH
                        for (int j = 0; j < GbVar.UserColorInfos.Count; j++)
                        {
                            if (GbVar.UserColorInfos[j].USER_ID == tbxUserID.Text)
                            {
                                GbVar.CurUserColorInfo = GbVar.UserColorInfos[j];
                                Color_ck = false;
                            }
                        }
                        if (Color_ck)
                        {
                            UserColorInfo userColor = new UserColorInfo(GbVar.loginInfos[i].USER_ID);
                            colorInfoList.m_listColorInfo.Add(userColor);
                            GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                            if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO, new UserColorInfoList(GbVar.UserColorInfos)))
                            { }
                            GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                            for (int j = 0; j < GbVar.UserColorInfos.Count; j++)
                            {
                                if (GbVar.UserColorInfos[j].USER_ID == tbxUserID.Text)
                                {
                                    GbVar.CurUserColorInfo = GbVar.UserColorInfos[j];
                                }
                            }
                        }

                        GbVar.nLoggingUserLevel = SelectedUserLevel;

                        userLogin(1);

                        // 로그인 된 LoginInfo를 저장
                        GbVar.CurLoginInfo = GbVar.loginInfos[i];

                        //User ID 초기화
                        tbxUserID.Text = "";
                        // 비밀번호 초기화
                        txtPass.Text = "";

                        // 로그인 이벤트로그 기록
                        GbFunc.WriteEventLog("LOGIN SUCCESS", string.Format("LEVEL : {1}, ID : {0}, LOGIN SUCCESS", GbVar.CurLoginInfo.USER_ID, GbVar.CurLoginInfo.USER_LEVEL == 0 ? "OPERAOTER" : GbVar.CurLoginInfo.USER_LEVEL == 1 ? "ENGINEER" : "MAKER"));
                        return;
                    }
                    else
                    {
                        // 로그인 이벤트로그 기록
                        GbFunc.WriteEventLog("LOGIN FAIL", "PASSWORD NOT CORRECT");

                        string strChange = "输入密码失败";
                        if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
                        {
                            strChange = "输入密码失败";
                        }
                        else
                        {
                            strChange = "Password Fail";
                        }

                        // 패스워드 틀림.
                        msg = new popMessageBox(strChange, "");
                        msg.ShowDialog();
                        return;
                    }
                }
            }

            // 로그인 이벤트로그 기록
            GbFunc.WriteEventLog("LOGIN FAIL", "WRONG ID AND LEVEL");

            string strText = "ID或级别不匹配";
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
            {
                strText = "ID或级别不匹配";
            }
            else
            {
                strText = "ID or LEVEL Fail";
            }
            // 아이디와 레벨이 맞는게 없었다.
            msg = new popMessageBox(strText, "");
            msg.ShowDialog();
        }

        public void LoadFile()
        {
            LoginInfoList obj = Deserialize(PathMgr.Inst.PATH_PRODUCT_USERINFO);
            if (obj == null)
            {
                obj = new LoginInfoList();
                for (int i = 0; i < 3; i++)
                {
                    LoginInfo login = new LoginInfo("Default", "0000", i);
                    obj.m_listLogInfo.Add(login);
                }
                GbVar.loginInfos = obj.m_listLogInfo;
                Save();
            }
            GbVar.loginInfos = obj.m_listLogInfo;
        }

        public void Save()
        {
            if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USERINFO, new LoginInfoList(GbVar.loginInfos)))
            {
                // 실패
            }
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
                //if (deleException != null)
                //    deleException(ex.ToString());
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
                //if (deleException != null)
                //    deleException(ex.ToString());
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

        private void rdbOP_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton[] rdbs = { rdbOP, rdbENG, rdbMAK };
            RadioButton rdb = sender as RadioButton;
            _SelectedLevel = Convert.ToInt16(rdb.Tag.ToString());
            btnAccountModify.Visible = true;

            if (rdbMAK.Checked)
            {
                tbxUserID.Visible = false;

            }
            else
            {

                tbxUserID.Visible = true;
                if (rdbOP.Checked)
                {
                    btnAccountModify.Visible = false;
                }
            }
        }

        private void btnAccountModify_Click(object sender, EventArgs e)
        {

            bool Color_ck = true;
            //if (userLogin != null) userLogin(1);
            LoadFile();
            UserColorInfoList colorInfoList = C_Deserialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO);
            if (colorInfoList == null)
            {
                colorInfoList = new UserColorInfoList();
                for (int i = 0; i < GbVar.loginInfos.Count; i++)
                {
                    UserColorInfo userColor = new UserColorInfo(GbVar.loginInfos[i].USER_ID);
                    colorInfoList.m_listColorInfo.Add(userColor);
                    GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                }
                if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO, new UserColorInfoList(GbVar.UserColorInfos)))
                { }
                GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
            }

            GbVar.strLoggingUerID = tbxUserID.Text;
            popMessageBox msg = new popMessageBox("-", "LOGIN");

            for (int i = 0; i < GbVar.loginInfos.Count; i++)
            {
                // 접속하려는 ID와 레벨이 맞으면 패스워드 검사를 한다.
                if (GbVar.loginInfos[i].USER_ID == tbxUserID.Text &&
                    GbVar.loginInfos[i].USER_LEVEL == SelectedUserLevel ||
                    (GbVar.loginInfos[i].USER_LEVEL == MCDF.LEVEL_MAK &&
                    GbVar.loginInfos[i].USER_LEVEL == SelectedUserLevel))
                {
                    // 패스워드가 맞으면 로그인을 한다.
                    if (GbVar.loginInfos[i].PASSWORD == txtPass.Text)
                    {
                        // 로그인 계정의 색상 정보를 불러온다 0423KTH
                        for (int j = 0; j < GbVar.UserColorInfos.Count; j++)
                        {
                            if (GbVar.UserColorInfos[j].USER_ID == tbxUserID.Text)
                            {
                                GbVar.CurUserColorInfo = GbVar.UserColorInfos[j];
                                Color_ck = false;
                            }
                        }
                        if (Color_ck)
                        {
                            UserColorInfo userColor = new UserColorInfo(GbVar.loginInfos[i].USER_ID);
                            colorInfoList.m_listColorInfo.Add(userColor);
                            GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                            if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO, new UserColorInfoList(GbVar.UserColorInfos)))
                            { }
                            GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                            for (int j = 0; j < GbVar.UserColorInfos.Count; j++)
                            {
                                if (GbVar.UserColorInfos[j].USER_ID == tbxUserID.Text)
                                {
                                    GbVar.CurUserColorInfo = GbVar.UserColorInfos[j];
                                }
                            }
                        }

                        GbVar.nLoggingUserLevel = SelectedUserLevel;

                        // 로그인 된 LoginInfo를 저장
                        GbVar.CurLoginInfo = GbVar.loginInfos[i];

                        // 비밀번호 초기화
                        txtPass.Text = "";

                        if (pAccount == null)
                        {
                            pAccount = new popAccount();
                            pAccount.Show();
                        }
                        else
                        {
                            pAccount.Dispose();
                            pAccount = new popAccount();
                            pAccount.Show();
                        }
                        return;
                    }
                    else
                    {
                        // 로그인 이벤트로그 기록
                        GbFunc.WriteEventLog("LOGIN FAIL", "PASSWORD NOT CORRECT");

                        string strChange = "输入密码失败";
                        if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
                        {
                            strChange = "输入密码失败";
                        }
                        else
                        {
                            strChange = "Password Fail";
                        }

                        // 패스워드 틀림.
                        msg = new popMessageBox(strChange, "");
                        msg.ShowDialog();
                        return;
                    }
                }
            }

            // 로그인 이벤트로그 기록
            GbFunc.WriteEventLog("LOGIN FAIL", "WRONG ID AND LEVEL");


            string strText = "ID或级别不匹配";
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
            {
                strText = "ID或级别不匹配";
            }
            else
            {
                strText = "ID or LEVEL Fail";
            }
            // 아이디와 레벨이 맞는게 없었다.
            msg = new popMessageBox(strText, "");
            msg.ShowDialog();

        
           
        }

        private void btnUserAdd_Click(object sender, EventArgs e)
        {
            bool Color_ck = true;
            //if (userLogin != null) userLogin(1);
            LoadFile();
            UserColorInfoList colorInfoList = C_Deserialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO);
            if (colorInfoList == null)
            {
                colorInfoList = new UserColorInfoList();
                for (int i = 0; i < GbVar.loginInfos.Count; i++)
                {
                    UserColorInfo userColor = new UserColorInfo(GbVar.loginInfos[i].USER_ID);
                    colorInfoList.m_listColorInfo.Add(userColor);
                    GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                }
                if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO, new UserColorInfoList(GbVar.UserColorInfos)))
                { }
                GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
            }

            GbVar.strLoggingUerID = tbxUserID.Text;
            popMessageBox msg = new popMessageBox("-", "LOGIN");

            for (int i = 0; i < GbVar.loginInfos.Count; i++)
            {
                // 접속하려는 ID와 레벨이 맞으면 패스워드 검사를 한다.
                if (GbVar.loginInfos[i].USER_ID == tbxUserID.Text &&
                    GbVar.loginInfos[i].USER_LEVEL == SelectedUserLevel ||
                    (GbVar.loginInfos[i].USER_LEVEL == MCDF.LEVEL_MAK &&
                    GbVar.loginInfos[i].USER_LEVEL == SelectedUserLevel))
                {
                               // 패스워드가 맞으면 로그인을 한다.
                    if (GbVar.loginInfos[i].PASSWORD == txtPass.Text)
                    {
                                    // 로그인 계정의 색상 정보를 불러온다 0423KTH
                        for (int j = 0; j < GbVar.UserColorInfos.Count; j++)
                        {
                            if (GbVar.UserColorInfos[j].USER_ID == tbxUserID.Text)
                            {
                                GbVar.CurUserColorInfo = GbVar.UserColorInfos[j];
                                Color_ck = false;
                            }
                        }
                        if (Color_ck)
                        {
                            UserColorInfo userColor = new UserColorInfo(GbVar.loginInfos[i].USER_ID);
                            colorInfoList.m_listColorInfo.Add(userColor);
                            GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                            if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO, new UserColorInfoList(GbVar.UserColorInfos)))
                            { }
                            GbVar.UserColorInfos = colorInfoList.m_listColorInfo;
                            for (int j = 0; j < GbVar.UserColorInfos.Count; j++)
                            {
                                if (GbVar.UserColorInfos[j].USER_ID == tbxUserID.Text)
                                {
                                    GbVar.CurUserColorInfo = GbVar.UserColorInfos[j];
                                }
                            }
                        }

                        GbVar.nLoggingUserLevel = SelectedUserLevel;

                                    // 로그인 된 LoginInfo를 저장
                        GbVar.CurLoginInfo = GbVar.loginInfos[i];

                                     // 비밀번호 초기화
                        txtPass.Text = "";

                        pChange = new popLogin();
                        if (pChange.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                        {
                            //popLogin.Height = 186;
                            return;
                        }
                        
                        return;
                    }
                    else
                    {
                                     // 로그인 이벤트로그 기록
                        GbFunc.WriteEventLog("LOGIN FAIL", "PASSWORD NOT CORRECT");

                        string strChange = "输入密码失败";
                        if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
                        {
                            strChange = "输入密码失败";
                        }
                        else
                        {
                            strChange = "Password Fail";
                        }

                        // 패스워드 틀림.
                        msg = new popMessageBox(strChange, "");
                        msg.ShowDialog();
                        return;
                    }
                }
            }

            // 로그인 이벤트로그 기록
            GbFunc.WriteEventLog("LOGIN FAIL", "WRONG ID AND LEVEL");


            string strText = "ID或级别不匹配";
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
            {
                strText = "ID或级别不匹配";
            }
            else
            {
                strText = "ID or LEVEL Fail";
            }
            // 아이디와 레벨이 맞는게 없었다.
            msg = new popMessageBox(strText, "");
            msg.ShowDialog();

        }

        private void frmLogging_VisibleChanged(object sender, EventArgs e)
        {
            string strChange = "变更 密码";
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
            {
                strChange = "变更 密码";
            }
            else
            {
                strChange = "CHANGE PASSWORD";
            }

            if (this.Visible)
            {
                btnUserAdd.Text = strChange;
            }
        }
    }
}
