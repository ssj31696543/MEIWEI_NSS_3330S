namespace NSSU_3400.UC
{
    partial class ucTempControl
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lbTargetTemp = new System.Windows.Forms.Label();
            this.lbl_freq_name = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbWaterReturnTemp = new System.Windows.Forms.Label();
            this.lbl_output_name = new System.Windows.Forms.Label();
            this.lbWaterTankInnerTemp = new System.Windows.Forms.Label();
            this.lbStatus = new System.Windows.Forms.Label();
            this.lbl_osill_name = new System.Windows.Forms.Label();
            this.lbErrorCode = new System.Windows.Forms.Label();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.cmb_setFreq = new System.Windows.Forms.ComboBox();
            this.btn_setFreq = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Location = new System.Drawing.Point(3, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(476, 343);
            this.panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.lbTargetTemp);
            this.groupBox1.Controls.Add(this.lbl_freq_name);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lbWaterReturnTemp);
            this.groupBox1.Controls.Add(this.lbl_output_name);
            this.groupBox1.Controls.Add(this.lbWaterTankInnerTemp);
            this.groupBox1.Controls.Add(this.lbStatus);
            this.groupBox1.Controls.Add(this.lbl_osill_name);
            this.groupBox1.Controls.Add(this.lbErrorCode);
            this.groupBox1.Location = new System.Drawing.Point(3, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(200, 182);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "읽기";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(63, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(64, 15);
            this.label7.TabIndex = 16;
            this.label7.Text = "설정 온도 :";
            // 
            // lbTargetTemp
            // 
            this.lbTargetTemp.AutoSize = true;
            this.lbTargetTemp.Location = new System.Drawing.Point(143, 18);
            this.lbTargetTemp.Name = "lbTargetTemp";
            this.lbTargetTemp.Size = new System.Drawing.Size(19, 15);
            this.lbTargetTemp.TabIndex = 17;
            this.lbTargetTemp.Text = "값";
            // 
            // lbl_freq_name
            // 
            this.lbl_freq_name.AutoSize = true;
            this.lbl_freq_name.Location = new System.Drawing.Point(33, 82);
            this.lbl_freq_name.Name = "lbl_freq_name";
            this.lbl_freq_name.Size = new System.Drawing.Size(94, 15);
            this.lbl_freq_name.TabIndex = 16;
            this.lbl_freq_name.Text = "워터 리턴 온도 : ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 15);
            this.label5.TabIndex = 22;
            this.label5.Text = "워터 탱크 내부 온도 : ";
            // 
            // lbWaterReturnTemp
            // 
            this.lbWaterReturnTemp.AutoSize = true;
            this.lbWaterReturnTemp.Location = new System.Drawing.Point(144, 82);
            this.lbWaterReturnTemp.Name = "lbWaterReturnTemp";
            this.lbWaterReturnTemp.Size = new System.Drawing.Size(19, 15);
            this.lbWaterReturnTemp.TabIndex = 17;
            this.lbWaterReturnTemp.Text = "값";
            // 
            // lbl_output_name
            // 
            this.lbl_output_name.AutoSize = true;
            this.lbl_output_name.Location = new System.Drawing.Point(87, 114);
            this.lbl_output_name.Name = "lbl_output_name";
            this.lbl_output_name.Size = new System.Drawing.Size(40, 15);
            this.lbl_output_name.TabIndex = 22;
            this.lbl_output_name.Text = "상태 : ";
            // 
            // lbWaterTankInnerTemp
            // 
            this.lbWaterTankInnerTemp.AutoSize = true;
            this.lbWaterTankInnerTemp.Location = new System.Drawing.Point(143, 50);
            this.lbWaterTankInnerTemp.Name = "lbWaterTankInnerTemp";
            this.lbWaterTankInnerTemp.Size = new System.Drawing.Size(19, 15);
            this.lbWaterTankInnerTemp.TabIndex = 23;
            this.lbWaterTankInnerTemp.Text = "값";
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = true;
            this.lbStatus.Location = new System.Drawing.Point(144, 114);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(19, 15);
            this.lbStatus.TabIndex = 23;
            this.lbStatus.Text = "값";
            // 
            // lbl_osill_name
            // 
            this.lbl_osill_name.AutoSize = true;
            this.lbl_osill_name.Location = new System.Drawing.Point(60, 146);
            this.lbl_osill_name.Name = "lbl_osill_name";
            this.lbl_osill_name.Size = new System.Drawing.Size(67, 15);
            this.lbl_osill_name.TabIndex = 24;
            this.lbl_osill_name.Text = "에러 코드 : ";
            // 
            // lbErrorCode
            // 
            this.lbErrorCode.AutoSize = true;
            this.lbErrorCode.Location = new System.Drawing.Point(144, 146);
            this.lbErrorCode.Name = "lbErrorCode";
            this.lbErrorCode.Size = new System.Drawing.Size(19, 15);
            this.lbErrorCode.TabIndex = 25;
            this.lbErrorCode.Text = "값";
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // cmb_setFreq
            // 
            this.cmb_setFreq.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_setFreq.Font = new System.Drawing.Font("굴림", 20F);
            this.cmb_setFreq.FormattingEnabled = true;
            this.cmb_setFreq.Items.AddRange(new object[] {
            "40",
            "80",
            "120"});
            this.cmb_setFreq.Location = new System.Drawing.Point(47, 28);
            this.cmb_setFreq.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmb_setFreq.Name = "cmb_setFreq";
            this.cmb_setFreq.Size = new System.Drawing.Size(68, 35);
            this.cmb_setFreq.TabIndex = 14;
            // 
            // btn_setFreq
            // 
            this.btn_setFreq.Location = new System.Drawing.Point(144, 15);
            this.btn_setFreq.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_setFreq.Name = "btn_setFreq";
            this.btn_setFreq.Size = new System.Drawing.Size(114, 71);
            this.btn_setFreq.TabIndex = 15;
            this.btn_setFreq.TabStop = false;
            this.btn_setFreq.Text = "주파수 채널 설정";
            this.btn_setFreq.UseVisualStyleBackColor = true;
            this.btn_setFreq.Click += new System.EventHandler(this.btn_setFreq_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 15);
            this.label2.TabIndex = 16;
            this.label2.Text = "채널 : ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(116, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 15);
            this.label1.TabIndex = 16;
            this.label1.Text = "kHz";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btn_setFreq);
            this.groupBox2.Controls.Add(this.cmb_setFreq);
            this.groupBox2.Location = new System.Drawing.Point(209, 4);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(264, 332);
            this.groupBox2.TabIndex = 30;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "쓰기";
            // 
            // ucTempControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ucTempControl";
            this.Size = new System.Drawing.Size(208, 194);
            this.Load += new System.EventHandler(this.ucTempControl_Load);
            this.VisibleChanged += new System.EventHandler(this.ucTempControl_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbErrorCode;
        private System.Windows.Forms.Label lbl_osill_name;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Label lbl_output_name;
        private System.Windows.Forms.Label lbWaterReturnTemp;
        private System.Windows.Forms.Label lbl_freq_name;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbTargetTemp;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lbWaterTankInnerTemp;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_setFreq;
        private System.Windows.Forms.ComboBox cmb_setFreq;
    }
}
