using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Design;

namespace NSS_3330S.UC
{
    public partial class ucUserSelectButton : UserControl
    {
        int nUserSelectCount = 10;
        int nUserSelectNum = 0;
        string strBtnText;
        ContextMenuStrip muTrip = new ContextMenuStrip();

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ButtonText
        {
            get { return strBtnText; }
            set 
            { 
                strBtnText = value;
                btnCycleStart.Text = strBtnText + " : " + (nUserSelectNum + 1).ToString();
            }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public int SEL_NUM
        {
            get { return nUserSelectNum; }
            set 
            { 
                nUserSelectNum = value;
                btnCycleStart.Text = strBtnText + " : " + (nUserSelectNum + 1).ToString();
            }
        }

        [Category("Advanced")]
        [RefreshProperties(RefreshProperties.All)]
        public int SEL_CNT
        {
            get { return nUserSelectCount; }
            set { nUserSelectCount = value; }
        }

        public ucUserSelectButton()
        {
            InitializeComponent();
        }

        private void ucUserSelectButton_Load(object sender, EventArgs e)
        {
            //muTrip.Items.Add("1");
            //muTrip.Items.Add("2");
            //muTrip.Items.Add("3");

            //this.ContextMenuStrip = muTrip;
            muTrip.ItemClicked += new ToolStripItemClickedEventHandler(contexMenu_ItemClicked);
        }

        private void contexMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            (sender as ContextMenuStrip).Close();

            ToolStripItem item = e.ClickedItem;

            int nSelect = 0;

            if (int.TryParse(item.Text, out nSelect))
	        {
                SEL_NUM = nSelect - 1;
                //btnCycleStart.Text = strBtnText + nSelect.ToString();
	        }
        }

        private void btnUserSelectNum_Click(object sender, EventArgs e)
        {
            muTrip.Items.Clear();

            for (int nSelCount = 0; nSelCount < nUserSelectCount; nSelCount++)
            {
                muTrip.Items.Add((nSelCount+1).ToString());
            }

            muTrip.Show(MousePosition);
        }
    }
}
