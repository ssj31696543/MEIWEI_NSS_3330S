using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public partial class ucVacList : UserControl
    {
        private UC.ucVacInfo[] m_VacInfo = new UC.ucVacInfo[37];
        public ucVacList()
        {
            InitializeComponent();
            SetVacUc();
        }
        private void SetVacUc()
        {
            m_VacInfo = new UC.ucVacInfo[]
            {
                ucVacInfo1,ucVacInfo2,ucVacInfo3,ucVacInfo4,ucVacInfo5,
                ucVacInfo9,ucVacInfo10,ucVacInfo11,ucVacInfo12,
                ucVacInfo13,ucVacInfo14,ucVacInfo15,ucVacInfo16,ucVacInfo17,ucVacInfo18,
                ucVacInfo19,ucVacInfo20,ucVacInfo21,ucVacInfo22,ucVacInfo23,ucVacInfo24,
                ucVacInfo25,ucVacInfo26,ucVacInfo27,ucVacInfo28,ucVacInfo29,ucVacInfo30,
                ucVacInfo31,ucVacInfo32,ucVacInfo33,
            };
        }
        public void refresh_VacInfo()
        {
            for (int i = 0; i < m_VacInfo.Length; i++)
            {
                if (m_VacInfo[i] == null) continue;

                int nIoIdx = (int)m_VacInfo[i].A_INPUT;
                double dVacValue = (GbVar.GB_AINPUT[nIoIdx] - ConfigMgr.Inst.Cfg.Vac[nIoIdx].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nIoIdx].dRatio;

                m_VacInfo[i].VAC_VAL = dVacValue.ToString("F1");

                if (dVacValue < ConfigMgr.Inst.Cfg.Vac[nIoIdx].dVacLevelLow)
                {
                    m_VacInfo[i].ON = true;
                }
                else
                {
                    m_VacInfo[i].ON = false;
                }
            }
        }
    }
}
