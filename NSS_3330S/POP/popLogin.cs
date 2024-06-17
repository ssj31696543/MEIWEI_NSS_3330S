using log4net.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml.Serialization;

namespace NSS_3330S.POP
{
    public partial class popLogin : Form
    {
        bool m_bFlagTmrShake = false;
        bool m_bStartShake = false;
        int m_nShakeCnt = 0;
        int m_nPosOrg = 0;
        private List<LoginInfo> m_loginInfos = new List<LoginInfo>();
        private List<UserColorInfo> m_usercolorInfos = new List<UserColorInfo>();

        public popLogin()
        {
            InitializeComponent();

            m_nPosOrg = txtPass.Left;
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        bool Login()
        {
            if (txtPass.Text.ToUpper() == "NEON1234")
            {
                return true;
            }

            txtPass.Text = "";

            return false;
        }

        private void popLogin_Load(object sender, EventArgs e)
        {
            txtPass.Focus();
            tmrShake.Start();
        }

        private void popLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (!Login())
                {
                    txtPass.Focus();
                    m_nShakeCnt = 0;
                    m_bStartShake = true;
                    e.Cancel = true;
                    return;
                }
            }

            tmrShake.Stop();
        }

        private void tmrShake_Tick(object sender, EventArgs e)
        {
            if (m_bFlagTmrShake) return;
            m_bFlagTmrShake = true;

            try
            {
                if (m_bStartShake)
                {
                    if (m_nShakeCnt % 2 == 0)
                        txtPass.Left = m_nPosOrg - 1;
                    else
                        txtPass.Left = m_nPosOrg + 1;

                    m_nShakeCnt++;

                    if (m_nShakeCnt > 10)
                    {
                        m_bStartShake = false;
                        m_nShakeCnt = 0;

                        txtPass.Left = m_nPosOrg;
                    }
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
            finally
            {
                m_bFlagTmrShake = false;
            }
        }


        private void btnPasswordOK_Click(object sender, EventArgs e)
        {
            if (GbVar.CurLoginInfo.PASSWORD == txtPass.Text)
            {
                if(txtVeriPass.TextLength == 0 )
                {
                    string strChange = "请输入新密码";
                    if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
                    {
                        strChange = "请输入新密码";
                    }
                    else
                    {
                        strChange = "Please enter a new password";
                    }
                              // 실패
                    MessageBox.Show(strChange);
                    return;
                }

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

                int nFind = m_loginInfos.FindIndex(x => x.USER_ID == GbVar.CurLoginInfo.USER_ID && x.PASSWORD == GbVar.CurLoginInfo.PASSWORD && x.USER_LEVEL == GbVar.CurLoginInfo.USER_LEVEL);
                m_loginInfos[nFind].PASSWORD = txtVeriPass.Text;

                GbFunc.WriteEventLog("PASSWORD CHANGED", string.Format("LEVEL : {1}, ID : {0}, CHANGED SUCCESS", m_loginInfos[nFind].USER_ID, m_loginInfos[nFind].USER_LEVEL == 0 ? "OPERAOTER" : m_loginInfos[nFind].USER_LEVEL == 1 ? "ENGINEER" : "MAKER"));

                GbVar.loginInfos = m_loginInfos;
                GbForm.fLogin.Save();
                string strSuccess = FormTextLangMgr.FindKey("SAVE SUCCESS");
                popMessageBox msg = new popMessageBox(strSuccess, "");
                msg.ShowDialog();
            }
            else
            {
                string strChange = "旧密码不正确";
                if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
                {
                    strChange = "旧密码不正确";
                }
                else
                {
                    strChange = "Old password is incorrect";
                }
                        // 실패
                MessageBox.Show(strChange);
                //return;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            
            //Close();
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

        private void popLogin_VisibleChanged(object sender, EventArgs e)
        {
            string strLoginPass = "旧密码";
            string strChangePass = "新密码";
            string strChange = "CHANGE";
            string strCancel = "CANCEL";
            string strLogin = "LOGIN";
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
            {
                strLoginPass = "旧密码";
                strChangePass = "新密码";
                strChange = "变更";
                strCancel = "取消";
                strLogin = "登录";
            }
            else
            {
                strLoginPass = "OLD LOGIN PASSWORD";
                strChangePass = "VERIFY NEW PASSWORD";
                strChange = "CHANGE";
                strCancel = "CANCEL";
                strLogin = "登录";
            }
            if (this.Visible)
            {
                lblModuleNameTitle.Text = strLoginPass;
                label1.Text = strChangePass;
                btnPasswordOK.Text = strChange;
                button1.Text = strCancel;
                this.Text = strLogin;
            }
        }
    }
}
