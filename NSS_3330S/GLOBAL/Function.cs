using NSS_3330S;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NSS_3330S.GLOBAL
{
    [Serializable]
    public class Function
    {
        public FuncServo[] dMotPos = new FuncServo[SVDF.SV_CNT];
        public AutoFocusParam fAutoFocus = new AutoFocusParam();

        public Function()
        {
            for (int nMotCount = 0; nMotCount < SVDF.SV_CNT; nMotCount++)
            {
                dMotPos[nMotCount] = new FuncServo();
            }
        }

        #region Serialization
        /// <summary>
        /// 파일에 xml형태로 정보를 저장합니다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>성공 여부</returns>
        public bool Serialize(string path)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Function));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, this);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(new System.Windows.Forms.Form() { TopMost = true }, string.Format("Cannot Serialize Function : {0}", ex));
                return false;
            }

            return true;
        }
        /// <summary>
        /// xml형태의 파일에서 정보를 불러옵니다.
        /// </summary>
        /// <param name="path">파일 경로</param>
        /// <returns>반환 객체</returns>
        public static Function Deserialize(string path)
        {
            Function inst = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Function));
                using (StreamReader rd = new StreamReader(path))
                {
                    inst = (Function)xmlSerializer.Deserialize(rd);
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(new System.Windows.Forms.Form() { TopMost = true }, string.Format("Cannot Deserialize Function : {0}", ex));
            }

            return inst;
        }
        #endregion
    }

    [Serializable]
    public class AutoFocusParam
    {
        public string strFocusPath;
        public int nCamViewIdx = 0;

        public double[] dStartFocus = new double[2] { 0.0, 0.0 };
        public double[] dEndFocus = new double[2] { 0.0, 0.0 };
        public double[] dFocusStep = new double[2] { 0.0, 0.0 };

        public int[] nAreaROIOrgX = new int[2];
        public int[] nAreaROIOrgY = new int[2];
        public int[] nAreaROIWidth = new int[2];
        public int[] nAreaROIHeight = new int[2];

        public int[] nCamBrightVal_Ring = new int[2];
        public int[] nCamBrightVal_Coax = new int[2];

        //[System.Xml.Serialization.XmlIgnore]
        //[NonSerialized]
        //public gigEViewer eViewer;
    }

    [Serializable]
    public class FuncServo
    {
        public double[] dPos = new double[SVDF.POS_CNT];
        //아래 속도와 가감속은 일반적인경우 사용하지 않는다.. 특별히 모델별로 속도조정이 필요한 시퀀스에서만 추가해서 쓸것..
        public double[] dVel = new double[SVDF.POS_CNT];
        public double[] dAcc = new double[SVDF.POS_CNT];
        public double[] dDec = new double[SVDF.POS_CNT];
    }

}
