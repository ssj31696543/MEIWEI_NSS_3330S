namespace NSSU_3400.UC
{
    partial class ucUltrasonicComm
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_setFreq = new System.Windows.Forms.Button();
            this.btn_OperationOutput = new System.Windows.Forms.Button();
            this.btn_OsillatorOff = new System.Windows.Forms.Button();
            this.btn_OsillatorOn = new System.Windows.Forms.Button();
            this.tb_OperationOutput = new System.Windows.Forms.TextBox();
            this.btn_AlarmClear = new System.Windows.Forms.Button();
            this.cmb_setFreq = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbl_freq_name = new System.Windows.Forms.Label();
            this.lbl_freq_val = new System.Windows.Forms.Label();
            this.lbl_output_name = new System.Windows.Forms.Label();
            this.lbl_alarm_val = new System.Windows.Forms.Label();
            this.lbl_output_val = new System.Windows.Forms.Label();
            this.lbl_alarm_name = new System.Windows.Forms.Label();
            this.lbl_osill_name = new System.Windows.Forms.Label();
            this.lbl_osill_val = new System.Windows.Forms.Label();
            this.cmbUltrasonicComm = new System.Windows.Forms.ComboBox();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.lbl_Setting_Out_val = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbl_Setting_freq_val = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.cmbUltrasonicComm);
            this.panel1.Location = new System.Drawing.Point(3, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(476, 343);
            this.panel1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btn_setFreq);
            this.groupBox2.Controls.Add(this.btn_OperationOutput);
            this.groupBox2.Controls.Add(this.btn_OsillatorOff);
            this.groupBox2.Controls.Add(this.btn_OsillatorOn);
            this.groupBox2.Controls.Add(this.tb_OperationOutput);
            this.groupBox2.Controls.Add(this.btn_AlarmClear);
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 15);
            this.label3.TabIndex = 22;
            this.label3.Text = "출력 : ";
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 15);
            this.label2.TabIndex = 16;
            this.label2.Text = "채널 : ";
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
            // btn_OperationOutput
            // 
            this.btn_OperationOutput.Location = new System.Drawing.Point(144, 94);
            this.btn_OperationOutput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_OperationOutput.Name = "btn_OperationOutput";
            this.btn_OperationOutput.Size = new System.Drawing.Size(114, 71);
            this.btn_OperationOutput.TabIndex = 19;
            this.btn_OperationOutput.Text = "동작출력 설정";
            this.btn_OperationOutput.UseVisualStyleBackColor = true;
            this.btn_OperationOutput.Click += new System.EventHandler(this.btn_OperationOutput_Click);
            // 
            // btn_OsillatorOff
            // 
            this.btn_OsillatorOff.Location = new System.Drawing.Point(144, 172);
            this.btn_OsillatorOff.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_OsillatorOff.Name = "btn_OsillatorOff";
            this.btn_OsillatorOff.Size = new System.Drawing.Size(114, 71);
            this.btn_OsillatorOff.TabIndex = 20;
            this.btn_OsillatorOff.Text = "발진기 OFF";
            this.btn_OsillatorOff.UseVisualStyleBackColor = true;
            this.btn_OsillatorOff.Click += new System.EventHandler(this.btn_OsillatorOff_Click);
            // 
            // btn_OsillatorOn
            // 
            this.btn_OsillatorOn.Location = new System.Drawing.Point(10, 172);
            this.btn_OsillatorOn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_OsillatorOn.Name = "btn_OsillatorOn";
            this.btn_OsillatorOn.Size = new System.Drawing.Size(114, 71);
            this.btn_OsillatorOn.TabIndex = 20;
            this.btn_OsillatorOn.Text = "발진기 ON";
            this.btn_OsillatorOn.UseVisualStyleBackColor = true;
            this.btn_OsillatorOn.Click += new System.EventHandler(this.btn_OsillatorOn_Click);
            // 
            // tb_OperationOutput
            // 
            this.tb_OperationOutput.Font = new System.Drawing.Font("굴림", 20F);
            this.tb_OperationOutput.Location = new System.Drawing.Point(51, 105);
            this.tb_OperationOutput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tb_OperationOutput.Name = "tb_OperationOutput";
            this.tb_OperationOutput.Size = new System.Drawing.Size(68, 38);
            this.tb_OperationOutput.TabIndex = 18;
            // 
            // btn_AlarmClear
            // 
            this.btn_AlarmClear.Location = new System.Drawing.Point(144, 251);
            this.btn_AlarmClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_AlarmClear.Name = "btn_AlarmClear";
            this.btn_AlarmClear.Size = new System.Drawing.Size(114, 71);
            this.btn_AlarmClear.TabIndex = 21;
            this.btn_AlarmClear.Text = "알람해제";
            this.btn_AlarmClear.UseVisualStyleBackColor = true;
            this.btn_AlarmClear.Click += new System.EventHandler(this.btn_AlarmClear_Click);
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
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.lbl_Setting_freq_val);
            this.groupBox1.Controls.Add(this.lbl_freq_name);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lbl_freq_val);
            this.groupBox1.Controls.Add(this.lbl_output_name);
            this.groupBox1.Controls.Add(this.lbl_Setting_Out_val);
            this.groupBox1.Controls.Add(this.lbl_alarm_val);
            this.groupBox1.Controls.Add(this.lbl_output_val);
            this.groupBox1.Controls.Add(this.lbl_alarm_name);
            this.groupBox1.Controls.Add(this.lbl_osill_name);
            this.groupBox1.Controls.Add(this.lbl_osill_val);
            this.groupBox1.Location = new System.Drawing.Point(3, 62);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(200, 274);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "읽기";
            // 
            // lbl_freq_name
            // 
            this.lbl_freq_name.AutoSize = true;
            this.lbl_freq_name.Location = new System.Drawing.Point(16, 83);
            this.lbl_freq_name.Name = "lbl_freq_name";
            this.lbl_freq_name.Size = new System.Drawing.Size(79, 15);
            this.lbl_freq_name.TabIndex = 16;
            this.lbl_freq_name.Text = "주파수 채널 : ";
            // 
            // lbl_freq_val
            // 
            this.lbl_freq_val.AutoSize = true;
            this.lbl_freq_val.Location = new System.Drawing.Point(144, 83);
            this.lbl_freq_val.Name = "lbl_freq_val";
            this.lbl_freq_val.Size = new System.Drawing.Size(19, 15);
            this.lbl_freq_val.TabIndex = 17;
            this.lbl_freq_val.Text = "값";
            // 
            // lbl_output_name
            // 
            this.lbl_output_name.AutoSize = true;
            this.lbl_output_name.Location = new System.Drawing.Point(32, 115);
            this.lbl_output_name.Name = "lbl_output_name";
            this.lbl_output_name.Size = new System.Drawing.Size(64, 15);
            this.lbl_output_name.TabIndex = 22;
            this.lbl_output_name.Text = "동작출력 : ";
            // 
            // lbl_alarm_val
            // 
            this.lbl_alarm_val.AutoSize = true;
            this.lbl_alarm_val.Location = new System.Drawing.Point(144, 185);
            this.lbl_alarm_val.Name = "lbl_alarm_val";
            this.lbl_alarm_val.Size = new System.Drawing.Size(19, 15);
            this.lbl_alarm_val.TabIndex = 27;
            this.lbl_alarm_val.Text = "값";
            // 
            // lbl_output_val
            // 
            this.lbl_output_val.AutoSize = true;
            this.lbl_output_val.Location = new System.Drawing.Point(144, 115);
            this.lbl_output_val.Name = "lbl_output_val";
            this.lbl_output_val.Size = new System.Drawing.Size(19, 15);
            this.lbl_output_val.TabIndex = 23;
            this.lbl_output_val.Text = "값";
            // 
            // lbl_alarm_name
            // 
            this.lbl_alarm_name.AutoSize = true;
            this.lbl_alarm_name.Location = new System.Drawing.Point(56, 185);
            this.lbl_alarm_name.Name = "lbl_alarm_name";
            this.lbl_alarm_name.Size = new System.Drawing.Size(40, 15);
            this.lbl_alarm_name.TabIndex = 26;
            this.lbl_alarm_name.Text = "알람 : ";
            // 
            // lbl_osill_name
            // 
            this.lbl_osill_name.AutoSize = true;
            this.lbl_osill_name.Location = new System.Drawing.Point(44, 149);
            this.lbl_osill_name.Name = "lbl_osill_name";
            this.lbl_osill_name.Size = new System.Drawing.Size(52, 15);
            this.lbl_osill_name.TabIndex = 24;
            this.lbl_osill_name.Text = "발진기 : ";
            // 
            // lbl_osill_val
            // 
            this.lbl_osill_val.AutoSize = true;
            this.lbl_osill_val.Location = new System.Drawing.Point(144, 149);
            this.lbl_osill_val.Name = "lbl_osill_val";
            this.lbl_osill_val.Size = new System.Drawing.Size(19, 15);
            this.lbl_osill_val.TabIndex = 25;
            this.lbl_osill_val.Text = "값";
            // 
            // cmbUltrasonicComm
            // 
            this.cmbUltrasonicComm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUltrasonicComm.Font = new System.Drawing.Font("굴림", 20F);
            this.cmbUltrasonicComm.FormattingEnabled = true;
            this.cmbUltrasonicComm.Items.AddRange(new object[] {
            "1번 초음파",
            "2번 초음파",
            "3번 초음파",
            "4번 초음파"});
            this.cmbUltrasonicComm.Location = new System.Drawing.Point(12, 19);
            this.cmbUltrasonicComm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbUltrasonicComm.Name = "cmbUltrasonicComm";
            this.cmbUltrasonicComm.Size = new System.Drawing.Size(191, 35);
            this.cmbUltrasonicComm.TabIndex = 28;
            this.cmbUltrasonicComm.SelectedIndexChanged += new System.EventHandler(this.cmbUltrasonicComm_SelectedIndexChanged);
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // lbl_Setting_Out_val
            // 
            this.lbl_Setting_Out_val.AutoSize = true;
            this.lbl_Setting_Out_val.Location = new System.Drawing.Point(143, 50);
            this.lbl_Setting_Out_val.Name = "lbl_Setting_Out_val";
            this.lbl_Setting_Out_val.Size = new System.Drawing.Size(19, 15);
            this.lbl_Setting_Out_val.TabIndex = 23;
            this.lbl_Setting_Out_val.Text = "값";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(31, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 15);
            this.label5.TabIndex = 22;
            this.label5.Text = "세팅출력 : ";
            // 
            // lbl_Setting_freq_val
            // 
            this.lbl_Setting_freq_val.AutoSize = true;
            this.lbl_Setting_freq_val.Location = new System.Drawing.Point(143, 18);
            this.lbl_Setting_freq_val.Name = "lbl_Setting_freq_val";
            this.lbl_Setting_freq_val.Size = new System.Drawing.Size(19, 15);
            this.lbl_Setting_freq_val.TabIndex = 17;
            this.lbl_Setting_freq_val.Text = "값";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 15);
            this.label7.TabIndex = 16;
            this.label7.Text = "세팅 주파수 :";
            // 
            // ucUltrasonicComm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ucUltrasonicComm";
            this.Size = new System.Drawing.Size(482, 351);
            this.Load += new System.EventHandler(this.ucUltrasonicComm_Load);
            this.VisibleChanged += new System.EventHandler(this.ucUltrasonicComm_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbl_alarm_val;
        private System.Windows.Forms.Label lbl_alarm_name;
        private System.Windows.Forms.Label lbl_osill_val;
        private System.Windows.Forms.Label lbl_osill_name;
        private System.Windows.Forms.Label lbl_output_val;
        private System.Windows.Forms.Label lbl_output_name;
        private System.Windows.Forms.Button btn_AlarmClear;
        private System.Windows.Forms.Button btn_OsillatorOn;
        private System.Windows.Forms.Button btn_OperationOutput;
        private System.Windows.Forms.TextBox tb_OperationOutput;
        private System.Windows.Forms.Label lbl_freq_val;
        private System.Windows.Forms.Label lbl_freq_name;
        private System.Windows.Forms.Button btn_setFreq;
        private System.Windows.Forms.ComboBox cmb_setFreq;
        private System.Windows.Forms.ComboBox cmbUltrasonicComm;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Button btn_OsillatorOff;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbl_Setting_freq_val;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lbl_Setting_Out_val;
    }
}
