namespace NSS_3330S.UC
{
    partial class ucVacInfo
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
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.a1Panel4 = new Owf.Controls.A1Panel();
            this.lbTEXT = new System.Windows.Forms.Label();
            this.lbAxisName = new System.Windows.Forms.Label();
            this.a1Panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // a1Panel4
            // 
            this.a1Panel4.BackColor = System.Drawing.SystemColors.ControlDark;
            this.a1Panel4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.a1Panel4.BorderWidth = 8;
            this.a1Panel4.Controls.Add(this.lbTEXT);
            this.a1Panel4.GradientEndColor = System.Drawing.SystemColors.ButtonFace;
            this.a1Panel4.GradientStartColor = System.Drawing.SystemColors.ButtonFace;
            this.a1Panel4.Image = null;
            this.a1Panel4.ImageLocation = new System.Drawing.Point(4, 4);
            this.a1Panel4.Location = new System.Drawing.Point(1, 18);
            this.a1Panel4.Margin = new System.Windows.Forms.Padding(2);
            this.a1Panel4.Name = "a1Panel4";
            this.a1Panel4.RoundCornerRadius = 1;
            this.a1Panel4.ShadowOffSet = 0;
            this.a1Panel4.Size = new System.Drawing.Size(133, 33);
            this.a1Panel4.TabIndex = 1062;
            // 
            // lbTEXT
            // 
            this.lbTEXT.BackColor = System.Drawing.SystemColors.Control;
            this.lbTEXT.Location = new System.Drawing.Point(11, 6);
            this.lbTEXT.Name = "lbTEXT";
            this.lbTEXT.Size = new System.Drawing.Size(104, 21);
            this.lbTEXT.TabIndex = 0;
            this.lbTEXT.Text = "-10";
            this.lbTEXT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbAxisName
            // 
            this.lbAxisName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbAxisName.ForeColor = System.Drawing.Color.White;
            this.lbAxisName.Location = new System.Drawing.Point(1, 1);
            this.lbAxisName.Name = "lbAxisName";
            this.lbAxisName.Size = new System.Drawing.Size(133, 19);
            this.lbAxisName.TabIndex = 1063;
            this.lbAxisName.Text = "Z1";
            this.lbAxisName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucVacInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.a1Panel4);
            this.Controls.Add(this.lbAxisName);
            this.Name = "ucVacInfo";
            this.Size = new System.Drawing.Size(136, 52);
            this.a1Panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Owf.Controls.A1Panel a1Panel4;
        private System.Windows.Forms.Label lbTEXT;
        private System.Windows.Forms.Label lbAxisName;
    }
}
