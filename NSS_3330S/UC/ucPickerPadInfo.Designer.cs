namespace NSS_3330S.UC
{
    partial class ucPickerPadInfo
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
            this.a1Panel4 = new Owf.Controls.A1Panel();
            this.lbTEXT = new System.Windows.Forms.Label();
            this.lbAxisName = new System.Windows.Forms.Label();
            this.lbBtmInspResult = new System.Windows.Forms.Label();
            this.lbTopInspResult = new System.Windows.Forms.Label();
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
            this.a1Panel4.Location = new System.Drawing.Point(1, 17);
            this.a1Panel4.Margin = new System.Windows.Forms.Padding(2);
            this.a1Panel4.Name = "a1Panel4";
            this.a1Panel4.RoundCornerRadius = 1;
            this.a1Panel4.ShadowOffSet = 0;
            this.a1Panel4.Size = new System.Drawing.Size(57, 27);
            this.a1Panel4.TabIndex = 1058;
            // 
            // lbTEXT
            // 
            this.lbTEXT.BackColor = System.Drawing.SystemColors.Control;
            this.lbTEXT.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbTEXT.Location = new System.Drawing.Point(7, 6);
            this.lbTEXT.Name = "lbTEXT";
            this.lbTEXT.Size = new System.Drawing.Size(45, 17);
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
            this.lbAxisName.Size = new System.Drawing.Size(57, 15);
            this.lbAxisName.TabIndex = 1059;
            this.lbAxisName.Text = "Z1";
            this.lbAxisName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbBtmInspResult
            // 
            this.lbBtmInspResult.BackColor = System.Drawing.Color.Gold;
            this.lbBtmInspResult.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lbBtmInspResult.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbBtmInspResult.Location = new System.Drawing.Point(0, 64);
            this.lbBtmInspResult.Name = "lbBtmInspResult";
            this.lbBtmInspResult.Size = new System.Drawing.Size(60, 16);
            this.lbBtmInspResult.TabIndex = 1060;
            this.lbBtmInspResult.Text = "WAIT";
            this.lbBtmInspResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbTopInspResult
            // 
            this.lbTopInspResult.BackColor = System.Drawing.Color.Gold;
            this.lbTopInspResult.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lbTopInspResult.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbTopInspResult.Location = new System.Drawing.Point(0, 48);
            this.lbTopInspResult.Name = "lbTopInspResult";
            this.lbTopInspResult.Size = new System.Drawing.Size(60, 16);
            this.lbTopInspResult.TabIndex = 1061;
            this.lbTopInspResult.Text = "WAIT";
            this.lbTopInspResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucPickerPadInfo
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.lbTopInspResult);
            this.Controls.Add(this.lbBtmInspResult);
            this.Controls.Add(this.a1Panel4);
            this.Controls.Add(this.lbAxisName);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "ucPickerPadInfo";
            this.Size = new System.Drawing.Size(60, 80);
            this.a1Panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Owf.Controls.A1Panel a1Panel4;
        private System.Windows.Forms.Label lbTEXT;
        private System.Windows.Forms.Label lbAxisName;
        private System.Windows.Forms.Label lbBtmInspResult;
        private System.Windows.Forms.Label lbTopInspResult;
    }
}
