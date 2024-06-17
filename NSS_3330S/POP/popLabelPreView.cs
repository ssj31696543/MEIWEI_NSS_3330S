using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text.pdf;

namespace NSK_8000S.POP
{
    public partial class popLabelPreView : Form
    {
        string strPath = @"D:\barcode.png";

        public popLabelPreView()
        {
            InitializeComponent();
        }

        private void CreateBarcode(string strLotId)
        {
            Barcode39 barcode39 = new Barcode39();
            barcode39.Code = strLotId;
            barcode39.BarHeight = 50f;

            if (File.Exists(strPath))
            {
                File.Delete(strPath);
            }

            barcode39.CreateDrawingImage(Color.Black, Color.White).Save(strPath);
        }

        private void popLabelPreView_Load(object sender, EventArgs e)
        {
            string strBarcode = GbVar.TU01Label.LOT_ID;
            CreateBarcode(strBarcode);

            pbxBarcode1.Load(strPath);
            pbxBarcode1.Show();

            pbxBarcode2.Load(strPath);
            pbxBarcode2.Show();

            #region TU01
            lbCjTest1.Text = GbVar.TU01Label.LABEL_TITLE;
            lbLotId1.Text = GbVar.TU01Label.LOT_ID;
            lbProd1.Text = GbVar.TU01Label.PRODUCT;
            lbPkgType1.Text = GbVar.TU01Label.PKG_TYPE1_LEAD;
            lbDevice1.Text = GbVar.TU01Label.DEVICE;
            lbSize1.Text = GbVar.TU01Label.PKG_SIZE;
            lbFab1.Text = GbVar.TU01Label.FAB;
            lbGrade1.Text = GbVar.TU01Label.GRADE;
            lbOwner1.Text = GbVar.TU01Label.OWNER;
            lbNotice1.Text = "-";

            Label[] lbOperlit1 = new Label[6] { lbOper1_1, lbOper1_2, lbOper1_3, lbOper1_4, lbOper1_5, lbOper1_6 };
            Label[] lbDesclit1 = new Label[6] { lbDesc1_1, lbDesc1_2, lbDesc1_3, lbDesc1_4, lbDesc1_5, lbDesc1_6 };
            Label[] lbPiNolit1 = new Label[6] { lbPiNo1_1, lbPiNo1_2, lbPiNo1_3, lbPiNo1_4, lbPiNo1_5, lbPiNo1_6 };
            Label[] lbQtylit1 = new Label[6] { lbQty1_1, lbQty1_2, lbQty1_3, lbQty1_4, lbQty1_5, lbOper1_6 };
            Label[] lbCommentlit1 = new Label[6] { lbComment1_1, lbComment1_2, lbComment1_3, lbComment1_4, lbComment1_5, lbComment1_6 };


            for (int i = 0; i < GbVar.TU01Label.OPER_CODE.Count; i++)
            {
                lbOperlit1[i].Text = GbVar.TU01Label.OPER_CODE[i];
            }
            for (int i = 0; i < GbVar.TU01Label.OPER_DESC.Count; i++)
            {
                lbDesclit1[i].Text = GbVar.TU01Label.OPER_DESC[i];
            }
            for (int i = 0; i < GbVar.TU01Label.PI_NO.Count; i++)
            {
                lbPiNolit1[i].Text = GbVar.TU01Label.PI_NO[i];
            }
            for (int i = 0; i < GbVar.TU01Label.UNIT_QTY.Count; i++)
            {
                lbQtylit1[i].Text = GbVar.TU01Label.UNIT_QTY[i].ToString();
            }
            for (int i = 0; i < GbVar.TU01Label.BIN_SORTING.Count; i++)
            {
                lbCommentlit1[i].Text = GbVar.TU01Label.BIN_SORTING[i];
            }
            #endregion

            #region TU02
            lbCjTest2.Text = GbVar.TU02Label.LABEL_TITLE;
            lbLotId2.Text = GbVar.TU02Label.LOT_ID;
            lbProd2.Text = GbVar.TU02Label.PRODUCT;
            lbPkgType2.Text = GbVar.TU02Label.PKG_TYPE1_LEAD;
            lbDevice2.Text = GbVar.TU02Label.DEVICE;
            lbSize2.Text = GbVar.TU02Label.PKG_SIZE;
            lbFab2.Text = GbVar.TU02Label.FAB;
            lbGrade2.Text = GbVar.TU02Label.GRADE;
            lbOwner2.Text = GbVar.TU02Label.OWNER;
            lbNotice1.Text = "-";

            Label[] lbOperlit2 = new Label[6] { lbOper2_1, lbOper2_2, lbOper2_3, lbOper2_4, lbOper2_5, lbOper2_6 };
            Label[] lbDesclit2 = new Label[6] { lbDesc2_1, lbDesc2_2, lbDesc2_3, lbDesc2_4, lbDesc2_5, lbDesc2_6 };
            Label[] lbPiNolit2 = new Label[6] { lbPiNo2_1, lbPiNo2_2, lbPiNo2_3, lbPiNo2_4, lbPiNo2_5, lbPiNo2_6 };
            Label[] lbQtylit2 = new Label[6] { lbQty2_1, lbQty2_2, lbQty2_3, lbQty2_4, lbQty2_5, lbOper2_6 };
            Label[] lbCommentlit2 = new Label[6] { lbComment2_1, lbComment2_2, lbComment2_3, lbComment2_4, lbComment2_5, lbComment2_6 };


            for (int i = 0; i < GbVar.TU02Label.OPER_CODE.Count; i++)
            {
                lbOperlit2[i].Text = GbVar.TU02Label.OPER_CODE[i];
            }
            for (int i = 0; i < GbVar.TU02Label.OPER_DESC.Count; i++)
            {
                lbDesclit2[i].Text = GbVar.TU02Label.OPER_DESC[i];
            }
            for (int i = 0; i < GbVar.TU02Label.PI_NO.Count; i++)
            {
                lbPiNolit2[i].Text = GbVar.TU02Label.PI_NO[i];
            }
            for (int i = 0; i < GbVar.TU02Label.UNIT_QTY.Count; i++)
            {
                lbQtylit2[i].Text = GbVar.TU02Label.UNIT_QTY[i].ToString();
            }
            for (int i = 0; i < GbVar.TU02Label.BIN_SORTING.Count; i++)
            {
                lbCommentlit2[i].Text = GbVar.TU02Label.BIN_SORTING[i];
            }
            #endregion
        }


    }
}
