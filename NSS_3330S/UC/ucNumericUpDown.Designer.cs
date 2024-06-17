namespace NSS_3330S.UC
{
    partial class ucNumericUpDown
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
            this.btnBrightDec = new System.Windows.Forms.Button();
            this.btnBrightInc = new System.Windows.Forms.Button();
            this.cmbStepValue = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnBrightDec
            // 
            this.btnBrightDec.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBrightDec.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnBrightDec.Location = new System.Drawing.Point(0, 0);
            this.btnBrightDec.Name = "btnBrightDec";
            this.btnBrightDec.Size = new System.Drawing.Size(60, 31);
            this.btnBrightDec.TabIndex = 0;
            this.btnBrightDec.Text = "◀";
            this.btnBrightDec.UseVisualStyleBackColor = true;
            this.btnBrightDec.Click += new System.EventHandler(this.btnBrightDec_Click);
            // 
            // btnBrightInc
            // 
            this.btnBrightInc.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnBrightInc.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnBrightInc.Location = new System.Drawing.Point(131, 0);
            this.btnBrightInc.Name = "btnBrightInc";
            this.btnBrightInc.Size = new System.Drawing.Size(60, 31);
            this.btnBrightInc.TabIndex = 1;
            this.btnBrightInc.Text = "▶";
            this.btnBrightInc.UseVisualStyleBackColor = true;
            this.btnBrightInc.Click += new System.EventHandler(this.btnBrightInc_Click);
            // 
            // cmbStepValue
            // 
            this.cmbStepValue.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cmbStepValue.FormattingEnabled = true;
            this.cmbStepValue.Items.AddRange(new object[] {
            "1",
            "5",
            "10"});
            this.cmbStepValue.Location = new System.Drawing.Point(61, 1);
            this.cmbStepValue.Name = "cmbStepValue";
            this.cmbStepValue.Size = new System.Drawing.Size(69, 29);
            this.cmbStepValue.TabIndex = 2;
            this.cmbStepValue.Text = "5";
            // 
            // ucNumericUpDown
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.cmbStepValue);
            this.Controls.Add(this.btnBrightInc);
            this.Controls.Add(this.btnBrightDec);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "ucNumericUpDown";
            this.Size = new System.Drawing.Size(191, 31);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnBrightDec;
        private System.Windows.Forms.Button btnBrightInc;
        private System.Windows.Forms.ComboBox cmbStepValue;
    }
}
