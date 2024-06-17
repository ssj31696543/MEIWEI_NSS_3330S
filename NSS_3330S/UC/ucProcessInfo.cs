using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public partial class ucProcessInfo : UserControl
    {
        Label[] m_lbTitle = null;
        Label[] m_lbElapsed = null;
        int[] m_nTactIdx = null;

        int nCnt = 0;

        public ucProcessInfo()
        {
            InitializeComponent();
            m_lbTitle = new Label[]
            {
                lbTitle0,lbTitle1,lbTitle2,lbTitle3,lbTitle4,lbTitle5,lbTitle6,lbTitle7,lbTitle8,lbTitle9,lbTitle10,
                lbTitle11,lbTitle12,lbTitle13,lbTitle14,lbTitle15,lbTitle16,lbTitle17,lbTitle18,
            };
            m_lbElapsed = new Label[]
            {
                lbElapsed0,lbElapsed1,lbElapsed2,lbElapsed3,lbElapsed4,lbElapsed5,lbElapsed6,lbElapsed7,lbElapsed8,lbElapsed9,lbElapsed10,
                lbElapsed11,lbElapsed12,lbElapsed13,lbElapsed14,lbElapsed15,lbElapsed16,lbElapsed17,lbElapsed18,
            };

            //m_nTactIdx = new int[]
            //{
            //    TTDF.MAGAZINE_LOADING,
            //    TTDF.MAGAZINE_STRIP_SUPPLY,
            //    TTDF.MAGAZINE_UNLOADING,

            //    TTDF.STRIP_PUSH_AND_LOADING,
            //    TTDF.STRIP_LOADING_TO_LEFT_TABLE,
            //    TTDF.STRIP_LOADING_TO_RIGHT_TABLE,
            //    TTDF.STRIP_CUTTING_LF,
            //    TTDF.STRIP_CUTTING_RT,
            //    TTDF.STRIP_UNLOADING_TO_CLEAN_TABLE,

            //    TTDF.STRIP_MARK_VISION_LEFT_TABLE,
            //    TTDF.STRIP_MARK_VISION_RIGHT_TABLE,
            //    TTDF.STRIP_PICK_AND_PLACE_LEFT_TABLE,
            //    TTDF.STRIP_PICK_AND_PLACE_RIGHT_TABLE,

            //    TTDF.TRAY_LOADING_GOOD_1,
            //    TTDF.TRAY_LOADING_GOOD_2,
            //    TTDF.TRAY_LOADING_REWORK,
            //    TTDF.TRAY_UNLOADING_GOOD_1,
            //    TTDF.TRAY_UNLOADING_GOOD_2,
            //    TTDF.TRAY_UNLOADING_REWORK,
            //};

        }

        public void UpdateTitleLabel()
        {
            try
            {
                TactTimeProperties[] m_DB_LOG_TACTTIME_INFO = GbVar.dbTactTime.SelectTactTimeAll();

                for (int i = 0; i < m_lbTitle.Length; i++)
                {
                    m_lbTitle[i].Text = m_DB_LOG_TACTTIME_INFO[m_nTactIdx[i]].Process;
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        public void UpdateElapsedLabel()
        {
            try
            {
                TactTimeProperties[] m_DB_LOG_TACTTIME_INFO = GbVar.dbTactTime.SelectTactTimeAll();

                for (int i = 0; i < m_lbElapsed.Length; i++)
                {
                    if (m_lbElapsed[i].Text != m_DB_LOG_TACTTIME_INFO[m_nTactIdx[i]].TactTime)
                        m_lbElapsed[i].Text = m_DB_LOG_TACTTIME_INFO[m_nTactIdx[i]].TactTime;
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        private void label52_DoubleClick(object sender, EventArgs e)
        {
            //더블클릭시 DB초기화
            InitAll();
        }

        private void InitAll()
        {
            GbVar.dbTactTime.Enqueue_DeleteAllDB();

            DateTime dtInit = DateTime.Now;
            for (int i = 0; i < (int)TTDF.CYCLE_NAME.MAX; i++)
            {
                TactTimeProperties m_colInfo = new TactTimeProperties();

                m_colInfo.Process = string.Format("{0}", (TTDF.CYCLE_NAME)i);
                m_colInfo.StartTime = dtInit.ToString("yyyyMMddHHmmssfff");
                m_colInfo.EndTime = dtInit.ToString("yyyyMMddHHmmssfff");
                m_colInfo.TactTime = string.Format("{0}:{1}:{2}.{3}",
                                (dtInit - dtInit).Hours.ToString("00"),
                                (dtInit - dtInit).Minutes.ToString("00"),
                                (dtInit - dtInit).Seconds.ToString("00"),
                                (dtInit - dtInit).Milliseconds.ToString("000"));

                GbVar.dbTactTime.Enqueue_InsertDB(m_colInfo);

                TTDF.bIsStarted[i] = false;
            }
        }
    }
}
