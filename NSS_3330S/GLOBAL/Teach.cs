using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace NSS_3330S
{
    [Serializable]
    public class Teach
    {
        public TeachServo[] dMotPos = new TeachServo[SVDF.SV_CNT];

        public Teach()
        {
            for (int nMotCount = 0; nMotCount < SVDF.SV_CNT; nMotCount++)
            {
                dMotPos[nMotCount] = new TeachServo();
            }
        }


        public void CopyTo(ref Teach info)
        {
            for (int nMotCount = 0; nMotCount < SVDF.SV_CNT; nMotCount++)
            {
                info.dMotPos[nMotCount] = dMotPos[nMotCount];
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool Serialize(string path)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Teach));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, this);
                }
            }
            catch (Exception ex)
            {
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
                return false;
            }

            return true;
        }

        public static Teach Deserialize(string path)
        {
            Teach inst = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Teach));
                using (StreamReader rd = new StreamReader(path))
                {
                    inst = (Teach)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LogMgr.Inst.LogAdd(MCDF.EXCEPT_LOG, ex);
            }

            return inst;
        }
    }

    #region SERVO
    [Serializable]
    public class TeachServo
    {
        public double[] dPos = new double[SVDF.POS_CNT];
        //아래 속도와 가감속은 일반적인경우 사용하지 않는다.. 특별히 모델별로 속도조정이 필요한 시퀀스에서만 추가해서 쓸것..
        public double[] dVel = new double[SVDF.POS_CNT];
        public double[] dAcc = new double[SVDF.POS_CNT];
        public double[] dDec = new double[SVDF.POS_CNT];
    }
    #endregion
}
