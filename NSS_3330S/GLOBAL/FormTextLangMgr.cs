using Newtonsoft.Json.Linq;
using NSS_3330S.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace NSS_3330S
{
    [Serializable]
    public class DictItemControl
    {
        public string Key { get; set; }
        public string EnglishValue { get; set; }
        public string ChinaValue { get; set; }
        public string KoreanValue { get; set; }

        public string FormName { get; set; }
        public string ParentName { get; set; }
        public string ControlName { get; set; }


        public DictItemControl()
        {
            Key = "";
            EnglishValue = "";
            ChinaValue = "";
            KoreanValue = "";
            FormName = "";
            ParentName = "";
            ControlName = "";
        }

        public DictItemControl(string key, string englishValue, string chinaValue, string koreanValue)
        {
            this.Key = key;
            this.EnglishValue = englishValue;
            this.ChinaValue = chinaValue;
            this.KoreanValue = koreanValue;
        }

        public DictItemControl(string formName, string parentName, string ctrName, string englishValue, string chinaValue, string koreanValue, string key)
        {
            // TODO: Complete member initialization
            this.FormName = formName;
            this.ParentName = parentName;
            this.ControlName = ctrName;
            this.EnglishValue = englishValue;
            this.ChinaValue = chinaValue;
            this.KoreanValue = koreanValue;
            this.Key = key;
        }
    }
       
    public static class FormTextLangMgr
    {
        static List<DictItemControl> m_listMsgControl = new List<DictItemControl>();
        static List<DictItemControl> m_listControl = new List<DictItemControl>();

        public static List<DictItemControl> FileOpen(string Path)
        {

            List<DictItemControl> listDic = new List<DictItemControl>();

            try
            {
         
                // 공백, 탭 문자 제거
                char[] chRemove = { ' ', '\t', '\'' };

                string[] strLines = File.ReadAllLines(Path, Encoding.UTF8);


                // 레코드 파일을 삽입
                for (int iLoopLine = 1; iLoopLine < strLines.Length; iLoopLine++)
                {
                    if (strLines[iLoopLine] == "")
                        continue;

                    string[] strRecord = strLines[iLoopLine].Split('|');


                    listDic.Add(new DictItemControl(strRecord[4].Trim(chRemove), strRecord[5].Trim(chRemove), strRecord[6].Trim(chRemove),                                                    
                                                    strRecord[1].Trim(chRemove).Replace("'r", "\r").Replace("'n", "\n"),
                                                    strRecord[2].Trim(chRemove).Replace("'r", "\r").Replace("'n", "\n"),
                                                    strRecord[3].Trim(chRemove).Replace("'r", "\r").Replace("'n", "\n"),
                                                    strRecord[0].Trim(chRemove).Replace("'r", "\r").Replace("'n", "\n")
                                                    ));

                }

                return listDic;

            }
            catch (Exception ex)
            {
                return listDic;
            }

        }
        public static void FileSave(string Path, List<DictItemControl> listDic)
        {

            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Key|EnglishValue|ChinaValue|KoreanValue|FormName|ParentName|ControlName");

            foreach (var item in listDic)
            {
                csvContent.Append($"{item.Key.Replace("\r", " 'r").Replace("\n", "'n ")}|");
                csvContent.Append($"{item.EnglishValue.Replace("\r", " 'r").Replace("\n", "'n ")}|");
                csvContent.Append($"{item.ChinaValue.Replace("\r", " 'r").Replace("\n", "'n ")}|");
                csvContent.Append($"{item.KoreanValue.Replace("\r", " 'r").Replace("\n", "'n ")}|");

                csvContent.AppendLine($"{item.FormName}|{item.ParentName}|{item.ControlName}");
            }

            Encoding utf8 = new UTF8Encoding(false); // BOM 없이 UTF-8 인코딩

            File.WriteAllText(Path, csvContent.ToString(), utf8);
        }


       
        public static string FindKey(string key)
        {
            m_listMsgControl =FileOpen(PathMgr.Inst.PATH_LANGUAGE_MESSAGE);

            if (m_listMsgControl == null)
            {
                m_listMsgControl = new List<DictItemControl>();
            }
            if (m_listMsgControl.Exists(kvp => kvp.Key == key))
            {
                DictItemControl dic = m_listMsgControl.Find(kvp => kvp.Key == key);
                switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
                {
                    case Language.ENGLISH:
                        return dic.EnglishValue;
                    case Language.CHINA:
                        return dic.ChinaValue;
                    case Language.KOREAN:
                        return dic.KoreanValue;
                    default:
                        return dic.EnglishValue;
                }
            }

            //key가 없으면 새로 추가    
            m_listMsgControl.Add(new DictItemControl(key, key, key, key));

            FileSave(PathMgr.Inst.PATH_LANGUAGE_MESSAGE, m_listMsgControl);
            return key;
        }
       
        public static void Load(Form form, string path)
        {

            m_listControl = FileOpen(PathMgr.Inst.FILE_PATH_LANGUAGE_UI);
            SetControl(form.Controls,"");
            FileSave(PathMgr.Inst.FILE_PATH_LANGUAGE_UI, m_listControl);
        }
     
        static void SetControl(Control.ControlCollection collection, string _strFrm)
        {
            DictItemControl value = null;
            
            foreach (Control ctrl in collection)
            {
                string parentName1 = ctrl.Parent.Name;


                Form parentForm = ctrl.FindForm();

                string formName = "";
                if (parentForm != null)
                {
                    formName = parentForm.Name;
                }
                else
                {
                    formName = ctrl.Parent.Parent.Name;
                }

                string strTextCheck = ctrl.Text.Replace("\r", " 'r").Replace("\n", "'n ").Trim();
                if ((ctrl.GetType() == typeof(Owf.Controls.A1Panel) ||
                     ctrl.GetType() == typeof(GroupBox) ||
                    ctrl.GetType() == typeof(TabPage)) &&
                   strTextCheck == "")
                {
                    if (ctrl.Controls.Count > 0)
                        SetControl(ctrl.Controls, formName);
                }

                if (ctrl.GetType() == typeof(Button)
                || ctrl.GetType() == typeof(Label)
                || ctrl.GetType() == typeof(CheckBox)
                || ctrl.GetType() == typeof(Glass.GlassButton)
                || ctrl.GetType() == typeof(GroupBox)
                || ctrl.GetType() == typeof(RadioButton)
                || ctrl.GetType() == typeof(TabPage)
                || ctrl.GetType() == typeof(Owf.Controls.A1Panel)
                || ctrl.GetType() == typeof(UC.ucManualOutputButton)
                || ctrl.GetType() == typeof(UC.RcpMotPosGetButtonEx)
                || ctrl.GetType() == typeof(UC.RcpMotPosMoveButtonEx)
                || ctrl.GetType() == typeof(UC.TeachingMotPosMoveButtonEx)
                || ctrl.GetType() == typeof(UC.TeachingMotPosGetButtonEx)
                )
                {
                    try
                    {
               
                        string parentName = ctrl.Parent.Name;

                        string strTest;
                        strTest = ctrl.Text.Replace("\r", " 'r").Replace("\n", "'n ").Trim();

                        if (strTest == " ")
                            strTest = "";

                        if (strTest == "")
                            continue;


                        if (double.TryParse(strTest, out double dstrCheck))
                        {
                            //입력 받는 값이 숫자만 있으면 언어 변화 할 필요 없음 2024-05-20 sj.shin
                            continue;
                        }
                        var ctlTemp3 = m_listControl.FindAll(d => d.FormName == formName &&
                                                                    d.ParentName == parentName &&
                                                                    d.ControlName == ctrl.Name &&
                                                                    d.Key != "");



                        if (ctlTemp3 != null &&
                            0 < ctlTemp3.Count)
                        {
                            value = (DictItemControl)ctlTemp3[0];
                        }
                        else
                            value = null;



                        if (value != null)
                        {
                            switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
                            {
                                case Language.ENGLISH:
                                    ctrl.Text = value.EnglishValue;
                                    break;
                                case Language.CHINA:
                                    ctrl.Text = value.ChinaValue;
                                    break;
                                case Language.KOREAN:
                                    ctrl.Text = value.KoreanValue;
                                    break;
                                default:
                                    ctrl.Text = value.EnglishValue;
                                    break;
                            }
                        }
                        else
                        {

                            DictItemControl dict = new DictItemControl(formName, ctrl.Parent.Name,
                                      ctrl.Name, strTest, strTest, strTest.Trim(), strTest);
                            if (!m_listControl.Contains(dict))
                            {
                                m_listControl.Add(dict);
                            }
                           
                        }


                    }
                    catch (System.InvalidOperationException ex)
                    {
                        string strText = ctrl.Text.Replace("\r", " 'r").Replace("\n","'n").Trim();


                        if (strText != "")
                        {
                            if (double.TryParse(strText, out double dstrCheck) == false)
                            {
                          
                                DictItemControl dict = new DictItemControl(formName, ctrl.Parent.Name,
                                    ctrl.Name, strText, strText, strText.Trim(), strText);
                                if (!m_listControl.Contains(dict))
                                {
                                    m_listControl.Add(dict);
                                }
                                
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 없으면 그냥 SKIP
                    }
                }

                if (ctrl.Controls.Count > 0)
                {
                   
                    SetControl(ctrl.Controls, formName);
                }
            }
        }

        /// <summary>
        /// 파일에 xml형태로 정보를 저장합니다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>성공 여부</returns>
        static bool Serialize<T>(string path, T info)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, info);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// xml형태의 파일에서 정보를 불러옵니다.
        /// </summary>
        /// <param name="path">파일 경로</param>
        /// <returns>반환 객체</returns>
        static T Deserialize<T>(string path) where T : List<DictItemControl>
        {
            T info = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (StreamReader rd = new StreamReader(path))
                {
                    info = (T)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.ToString());
                return null;
            }

            return info;
        }
    }
}
