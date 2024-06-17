using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public class StripmapImpl : Stripmap
    {
        public override void dieEntered(int x, int y, int bincode)
        {
            // Do nothing
        }

        public override void dieClicked(int x, int y, int bincode, System.Windows.Forms.MouseButtons btn)
        {
            // Cast
            if (btn == MouseButtons.Left)
            {
                // Update dataset
                //Dataset[x, y] = 2;
                // Show the changes
                //updateDie(x, y, 2);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // StripmapImpl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.Name = "StripmapImpl";
            this.Size = new System.Drawing.Size(1490, 600);
            this.ResumeLayout(false);

        }

        public void Clear()
        {
            for (int nColCnt = 0; nColCnt < Dataset.Length; nColCnt++)
            {
                for (int nRowCnt = 0; nRowCnt < 13; nRowCnt++)
                {
                    Dataset[0, nRowCnt] = 0;
                }
            }
        }
    }
}
