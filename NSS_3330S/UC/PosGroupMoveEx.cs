using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    [DefaultProperty("PosGroup")]
    public class PosGroupMoveEx : Button
    {
        int m_nTableNo = 0;
        int m_nPosNo = 0;
        MANUAL_POS_GROUP m_nPosGroup = 0;

        [Category("Advanced")]
        public int TableNo
        {
            get { return m_nTableNo; }
            set { m_nTableNo = value; }
        }

        [Category("Advanced")]
        public MANUAL_POS_GROUP PosGroup
        {
            get { return m_nPosGroup; }
            set { m_nPosGroup = value; }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public int PosNo
        {
            get { return m_nPosNo; }
            set { m_nPosNo = value; }
        }

        [Category("Advanced")]
        public string PosName
        {
            get
            {
                string name = "";

                #region Position Name
                switch (m_nPosGroup)
                {
                    case MANUAL_POS_GROUP.RCP_LD_PICKER_MOVE:
                        {
                            //name = POSDF.GetPosName(POSDF.RCP_POS_MODE.LD_UVW, m_nPosNo);
                        }
                        break;
                    default:
                        break;
                }
                #endregion

                return name;
            }
        }
    }
}
