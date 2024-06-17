namespace NSS_3330S.UC
{
    partial class ucUserSelectButton
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnUserSelectNum = new Glass.GlassButton();
            this.btnCycleStart = new Glass.GlassButton();
            this.SuspendLayout();
            // 
            // btnUserSelectNum
            // 
            this.btnUserSelectNum.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnUserSelectNum.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnUserSelectNum.FadeOnFocus = true;
            this.btnUserSelectNum.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnUserSelectNum.ForeColor = System.Drawing.Color.Black;
            this.btnUserSelectNum.GlowColor = System.Drawing.Color.Lime;
            this.btnUserSelectNum.Location = new System.Drawing.Point(206, 0);
            this.btnUserSelectNum.Name = "btnUserSelectNum";
            this.btnUserSelectNum.Size = new System.Drawing.Size(30, 66);
            this.btnUserSelectNum.TabIndex = 1723;
            this.btnUserSelectNum.Tag = "10";
            this.btnUserSelectNum.Text = "▼";
            this.btnUserSelectNum.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnUserSelectNum.Click += new System.EventHandler(this.btnUserSelectNum_Click);
            // 
            // btnCycleStart
            // 
            this.btnCycleStart.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCycleStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCycleStart.FadeOnFocus = true;
            this.btnCycleStart.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCycleStart.ForeColor = System.Drawing.Color.Black;
            this.btnCycleStart.GlowColor = System.Drawing.Color.Lime;
            this.btnCycleStart.Location = new System.Drawing.Point(0, 0);
            this.btnCycleStart.Name = "btnCycleStart";
            this.btnCycleStart.Size = new System.Drawing.Size(206, 66);
            this.btnCycleStart.TabIndex = 1724;
            this.btnCycleStart.Tag = "10";
            this.btnCycleStart.Text = "LOADING";
            this.btnCycleStart.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // ucUserSelectButton
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.btnCycleStart);
            this.Controls.Add(this.btnUserSelectNum);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "ucUserSelectButton";
            this.Size = new System.Drawing.Size(236, 66);
            this.Load += new System.EventHandler(this.ucUserSelectButton_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Glass.GlassButton btnUserSelectNum;
        private Glass.GlassButton btnCycleStart;


    }
}
