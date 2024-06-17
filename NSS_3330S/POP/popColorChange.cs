using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NSS_3330S.POP
{
    public partial class popColorChange : Form
    {
        int UserColorindex = 0;
        private List<UserColorInfo> m_usercolorInfos = new List<UserColorInfo>();
        public popColorChange(int n)
        {
            UserColorindex = n;
            InitializeComponent();
        }

        private void popColorChange_Load(object sender, EventArgs e)
        {
            ArrayList ColorList = new ArrayList();
            Type ColorType = typeof(Color);
            PropertyInfo[] propInfoList = ColorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            LoadAccountData();
            foreach(PropertyInfo c in propInfoList)
            {
                this.cmb_ColorListbox.Items.Add(c.Name);
            }
            switch(UserColorindex)
            {
                case 1:
                    {
                        label1.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_GoodColor);
                    }
                    break;
                case 2:
                    {
                        label1.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_VisionNgColor);
                    }
                    break;
                case 3:
                    {
                        label1.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_UnitEmptyColor);
                    }
                    break;
                case 4:
                    {
                        label1.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_ITSColor);
                    }
                    break;
                case 5:
                    {
                        label1.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_InvaildColor);
                    }
                    break;
                default:
                    break;
            }

        }

        private void cmb_ColorListbox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = new Font("Arial", 9, FontStyle.Regular);
                Color c = Color.FromName(n);
                Brush b = new SolidBrush(c);
                g.DrawString(n, f, Brushes.Black, rect.X, rect.Top);
                g.FillRectangle(b, rect.X + 110, rect.Y + 3, rect.Width - 10, rect.Height - 3);
            }
        }

        //private void cmb_ColorListbox_SelectionChangeCommitted(object sender, EventArgs e)
        //{
        //    label1.Text = cmb_ColorListbox.Text;
        //    label1.BackColor = Color.FromName(cmb_ColorListbox.Text);
        //}

        private void btn_save_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < m_usercolorInfos.Count; i++)
            {
                if (m_usercolorInfos[i].USER_ID == GbVar.CurUserColorInfo.USER_ID)
                {
                    switch (UserColorindex)
                    {
                        case 1:
                            {
                                GbVar.CurUserColorInfo.USER_GoodColor = cmb_ColorListbox.Text;
                                m_usercolorInfos[i].USER_GoodColor = cmb_ColorListbox.Text;
                            }
                            break;
                        case 2:
                            {
                                GbVar.CurUserColorInfo.USER_VisionNgColor = cmb_ColorListbox.Text;
                                m_usercolorInfos[i].USER_VisionNgColor = cmb_ColorListbox.Text;
                            }
                            break;
                        case 3:
                            {
                                GbVar.CurUserColorInfo.USER_UnitEmptyColor = cmb_ColorListbox.Text;
                                m_usercolorInfos[i].USER_UnitEmptyColor = cmb_ColorListbox.Text;
                            }
                            break;
                        case 4:
                            {
                                GbVar.CurUserColorInfo.USER_ITSColor = cmb_ColorListbox.Text;
                                m_usercolorInfos[i].USER_ITSColor = cmb_ColorListbox.Text;
                            }
                            break;
                        case 5:
                            {
                                GbVar.CurUserColorInfo.USER_InvaildColor = cmb_ColorListbox.Text;
                                m_usercolorInfos[i].USER_InvaildColor = cmb_ColorListbox.Text;
                            }
                            break;
                        default:
                            break;
                    }
                    SaveAccountInfo();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        public void SaveAccountInfo()
        {
            GbVar.UserColorInfos = m_usercolorInfos;

            popMessageBox msg;

            if (!Serialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO, new UserColorInfoList(GbVar.UserColorInfos)))
            {
                msg = new popMessageBox(FormTextLangMgr.FindKey("FAIL TO SAVE!!"), "ACCOUNT");
                msg.ShowDialog();
                // 실패
            }
            else
            {
                LoadAccountData();
                msg = new popMessageBox(FormTextLangMgr.FindKey("SAVE COMPLETE"), "ACCOUNT");
                msg.ShowDialog();
            }
        }
        public void LoadAccountData()
        {
            UserColorInfoList obj = Deserialize(PathMgr.Inst.PATH_PRODUCT_USER_COLORINFO);
            if (obj == null)
            {
                obj = new UserColorInfoList();
                for (int i = 0; i < 3; i++)
                {
                    UserColorInfo colorInfo = new UserColorInfo();
                    obj.m_listColorInfo.Add(colorInfo);
                }
                m_usercolorInfos = obj.m_listColorInfo;
            }
            m_usercolorInfos = obj.m_listColorInfo;
        }
        protected UserColorInfoList Deserialize(string path)
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

        private void cmb_ColorListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = cmb_ColorListbox.Text;
            label1.BackColor = Color.FromName(cmb_ColorListbox.Text);
        }
    }
}
