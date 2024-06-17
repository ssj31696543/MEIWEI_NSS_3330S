namespace NSS_3330S.POP
{
    partial class popLOTInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbl_LOTID = new System.Windows.Forms.Label();
            this.tb_LOTID = new System.Windows.Forms.TextBox();
            this.lbl_LOTKind = new System.Windows.Forms.Label();
            this.panel_LotKind = new System.Windows.Forms.Panel();
            this.Rbtn_LOTKind5 = new System.Windows.Forms.RadioButton();
            this.Rbtn_LOTKind4 = new System.Windows.Forms.RadioButton();
            this.Rbtn_LOTKind3 = new System.Windows.Forms.RadioButton();
            this.Rbtn_LOTKind2 = new System.Windows.Forms.RadioButton();
            this.Rbtn_LOTKind1 = new System.Windows.Forms.RadioButton();
            this.lbl_STRIPCnt = new System.Windows.Forms.Label();
            this.NUD_STRIPCnt = new System.Windows.Forms.NumericUpDown();
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancle = new System.Windows.Forms.Button();
            this.btn_EXIT = new System.Windows.Forms.Button();
            this.lbTitle = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel_LotKind.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_STRIPCnt)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_LOTID
            // 
            this.lbl_LOTID.AutoSize = true;
            this.lbl_LOTID.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbl_LOTID.Location = new System.Drawing.Point(35, 25);
            this.lbl_LOTID.Name = "lbl_LOTID";
            this.lbl_LOTID.Size = new System.Drawing.Size(60, 17);
            this.lbl_LOTID.TabIndex = 0;
            this.lbl_LOTID.Text = "LOT ID :";
            // 
            // tb_LOTID
            // 
            this.tb_LOTID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_LOTID.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tb_LOTID.Location = new System.Drawing.Point(104, 21);
            this.tb_LOTID.Name = "tb_LOTID";
            this.tb_LOTID.Size = new System.Drawing.Size(517, 25);
            this.tb_LOTID.TabIndex = 2;
            // 
            // lbl_LOTKind
            // 
            this.lbl_LOTKind.AutoSize = true;
            this.lbl_LOTKind.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbl_LOTKind.Location = new System.Drawing.Point(23, 100);
            this.lbl_LOTKind.Name = "lbl_LOTKind";
            this.lbl_LOTKind.Size = new System.Drawing.Size(72, 17);
            this.lbl_LOTKind.TabIndex = 4;
            this.lbl_LOTKind.Text = "LOT 종류 :";
            this.lbl_LOTKind.Visible = false;
            // 
            // panel_LotKind
            // 
            this.panel_LotKind.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_LotKind.Controls.Add(this.Rbtn_LOTKind5);
            this.panel_LotKind.Controls.Add(this.Rbtn_LOTKind4);
            this.panel_LotKind.Controls.Add(this.Rbtn_LOTKind3);
            this.panel_LotKind.Controls.Add(this.Rbtn_LOTKind2);
            this.panel_LotKind.Controls.Add(this.Rbtn_LOTKind1);
            this.panel_LotKind.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.panel_LotKind.Location = new System.Drawing.Point(104, 87);
            this.panel_LotKind.Name = "panel_LotKind";
            this.panel_LotKind.Size = new System.Drawing.Size(517, 45);
            this.panel_LotKind.TabIndex = 5;
            this.panel_LotKind.Visible = false;
            // 
            // Rbtn_LOTKind5
            // 
            this.Rbtn_LOTKind5.Appearance = System.Windows.Forms.Appearance.Button;
            this.Rbtn_LOTKind5.AutoSize = true;
            this.Rbtn_LOTKind5.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(141)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Rbtn_LOTKind5.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Rbtn_LOTKind5.Location = new System.Drawing.Point(428, 5);
            this.Rbtn_LOTKind5.Name = "Rbtn_LOTKind5";
            this.Rbtn_LOTKind5.Size = new System.Drawing.Size(66, 32);
            this.Rbtn_LOTKind5.TabIndex = 5;
            this.Rbtn_LOTKind5.TabStop = true;
            this.Rbtn_LOTKind5.Text = "재작업";
            this.Rbtn_LOTKind5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Rbtn_LOTKind5.UseVisualStyleBackColor = true;
            this.Rbtn_LOTKind5.CheckedChanged += new System.EventHandler(this.Rbtn_CheckedChanged);
            // 
            // Rbtn_LOTKind4
            // 
            this.Rbtn_LOTKind4.Appearance = System.Windows.Forms.Appearance.Button;
            this.Rbtn_LOTKind4.AutoSize = true;
            this.Rbtn_LOTKind4.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(141)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Rbtn_LOTKind4.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Rbtn_LOTKind4.Location = new System.Drawing.Point(220, 5);
            this.Rbtn_LOTKind4.Name = "Rbtn_LOTKind4";
            this.Rbtn_LOTKind4.Size = new System.Drawing.Size(66, 32);
            this.Rbtn_LOTKind4.TabIndex = 4;
            this.Rbtn_LOTKind4.TabStop = true;
            this.Rbtn_LOTKind4.Text = "재초도";
            this.Rbtn_LOTKind4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Rbtn_LOTKind4.UseVisualStyleBackColor = true;
            this.Rbtn_LOTKind4.CheckedChanged += new System.EventHandler(this.Rbtn_CheckedChanged);
            // 
            // Rbtn_LOTKind3
            // 
            this.Rbtn_LOTKind3.Appearance = System.Windows.Forms.Appearance.Button;
            this.Rbtn_LOTKind3.AutoSize = true;
            this.Rbtn_LOTKind3.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(141)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Rbtn_LOTKind3.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Rbtn_LOTKind3.Location = new System.Drawing.Point(151, 5);
            this.Rbtn_LOTKind3.Name = "Rbtn_LOTKind3";
            this.Rbtn_LOTKind3.Size = new System.Drawing.Size(51, 32);
            this.Rbtn_LOTKind3.TabIndex = 3;
            this.Rbtn_LOTKind3.TabStop = true;
            this.Rbtn_LOTKind3.Text = "더미";
            this.Rbtn_LOTKind3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Rbtn_LOTKind3.UseVisualStyleBackColor = true;
            this.Rbtn_LOTKind3.CheckedChanged += new System.EventHandler(this.Rbtn_CheckedChanged);
            // 
            // Rbtn_LOTKind2
            // 
            this.Rbtn_LOTKind2.Appearance = System.Windows.Forms.Appearance.Button;
            this.Rbtn_LOTKind2.AutoSize = true;
            this.Rbtn_LOTKind2.Checked = true;
            this.Rbtn_LOTKind2.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(141)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Rbtn_LOTKind2.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Rbtn_LOTKind2.Location = new System.Drawing.Point(82, 5);
            this.Rbtn_LOTKind2.Name = "Rbtn_LOTKind2";
            this.Rbtn_LOTKind2.Size = new System.Drawing.Size(51, 32);
            this.Rbtn_LOTKind2.TabIndex = 2;
            this.Rbtn_LOTKind2.TabStop = true;
            this.Rbtn_LOTKind2.Text = "본낫";
            this.Rbtn_LOTKind2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Rbtn_LOTKind2.UseVisualStyleBackColor = true;
            this.Rbtn_LOTKind2.CheckedChanged += new System.EventHandler(this.Rbtn_CheckedChanged);
            // 
            // Rbtn_LOTKind1
            // 
            this.Rbtn_LOTKind1.Appearance = System.Windows.Forms.Appearance.Button;
            this.Rbtn_LOTKind1.AutoSize = true;
            this.Rbtn_LOTKind1.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(141)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.Rbtn_LOTKind1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Rbtn_LOTKind1.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Rbtn_LOTKind1.Location = new System.Drawing.Point(13, 5);
            this.Rbtn_LOTKind1.Name = "Rbtn_LOTKind1";
            this.Rbtn_LOTKind1.Size = new System.Drawing.Size(51, 32);
            this.Rbtn_LOTKind1.TabIndex = 1;
            this.Rbtn_LOTKind1.TabStop = true;
            this.Rbtn_LOTKind1.Text = "초도";
            this.Rbtn_LOTKind1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Rbtn_LOTKind1.UseVisualStyleBackColor = true;
            this.Rbtn_LOTKind1.CheckedChanged += new System.EventHandler(this.Rbtn_CheckedChanged);
            // 
            // lbl_STRIPCnt
            // 
            this.lbl_STRIPCnt.AutoSize = true;
            this.lbl_STRIPCnt.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbl_STRIPCnt.Location = new System.Drawing.Point(-1, 158);
            this.lbl_STRIPCnt.Name = "lbl_STRIPCnt";
            this.lbl_STRIPCnt.Size = new System.Drawing.Size(102, 17);
            this.lbl_STRIPCnt.TabIndex = 6;
            this.lbl_STRIPCnt.Text = "STRIP COUNT :";
            // 
            // NUD_STRIPCnt
            // 
            this.NUD_STRIPCnt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NUD_STRIPCnt.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.NUD_STRIPCnt.Location = new System.Drawing.Point(103, 154);
            this.NUD_STRIPCnt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.NUD_STRIPCnt.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUD_STRIPCnt.Name = "NUD_STRIPCnt";
            this.NUD_STRIPCnt.Size = new System.Drawing.Size(110, 25);
            this.NUD_STRIPCnt.TabIndex = 7;
            this.NUD_STRIPCnt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NUD_STRIPCnt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btn_OK
            // 
            this.btn_OK.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_OK.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_OK.Location = new System.Drawing.Point(229, 138);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(129, 57);
            this.btn_OK.TabIndex = 8;
            this.btn_OK.Text = "REGISTER";
            this.btn_OK.UseVisualStyleBackColor = false;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Cancle
            // 
            this.btn_Cancle.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_Cancle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_Cancle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_Cancle.Location = new System.Drawing.Point(361, 138);
            this.btn_Cancle.Name = "btn_Cancle";
            this.btn_Cancle.Size = new System.Drawing.Size(129, 57);
            this.btn_Cancle.TabIndex = 9;
            this.btn_Cancle.Text = "CANCEL";
            this.btn_Cancle.UseVisualStyleBackColor = false;
            this.btn_Cancle.Visible = false;
            // 
            // btn_EXIT
            // 
            this.btn_EXIT.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_EXIT.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_EXIT.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_EXIT.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_EXIT.Location = new System.Drawing.Point(493, 138);
            this.btn_EXIT.Name = "btn_EXIT";
            this.btn_EXIT.Size = new System.Drawing.Size(129, 57);
            this.btn_EXIT.TabIndex = 10;
            this.btn_EXIT.Text = "EXIT";
            this.btn_EXIT.UseVisualStyleBackColor = false;
            this.btn_EXIT.Click += new System.EventHandler(this.btn_EXIT_Click);
            // 
            // lbTitle
            // 
            this.lbTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(34)))));
            this.lbTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbTitle.ForeColor = System.Drawing.Color.LightGray;
            this.lbTitle.Location = new System.Drawing.Point(0, 0);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lbTitle.Size = new System.Drawing.Size(636, 41);
            this.lbTitle.TabIndex = 11;
            this.lbTitle.Text = "LOT REGISTRATION";
            this.lbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbTitle_MouseDown);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lbl_LOTID);
            this.panel1.Controls.Add(this.btn_EXIT);
            this.panel1.Controls.Add(this.tb_LOTID);
            this.panel1.Controls.Add(this.btn_Cancle);
            this.panel1.Controls.Add(this.btn_OK);
            this.panel1.Controls.Add(this.lbl_LOTKind);
            this.panel1.Controls.Add(this.NUD_STRIPCnt);
            this.panel1.Controls.Add(this.panel_LotKind);
            this.panel1.Controls.Add(this.lbl_STRIPCnt);
            this.panel1.Location = new System.Drawing.Point(1, 40);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(634, 216);
            this.panel1.TabIndex = 12;
            this.panel1.VisibleChanged += new System.EventHandler(this.panel1_VisibleChanged);
            // 
            // popLOTInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 257);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lbTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popLOTInfo";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LOT INFO";
            this.Load += new System.EventHandler(this.popLOTInfo_Load);
            this.panel_LotKind.ResumeLayout(false);
            this.panel_LotKind.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_STRIPCnt)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbl_LOTID;
        private System.Windows.Forms.TextBox tb_LOTID;
        private System.Windows.Forms.Label lbl_LOTKind;
        private System.Windows.Forms.Panel panel_LotKind;
        private System.Windows.Forms.RadioButton Rbtn_LOTKind5;
        private System.Windows.Forms.RadioButton Rbtn_LOTKind4;
        private System.Windows.Forms.RadioButton Rbtn_LOTKind3;
        private System.Windows.Forms.RadioButton Rbtn_LOTKind2;
        private System.Windows.Forms.RadioButton Rbtn_LOTKind1;
        private System.Windows.Forms.Label lbl_STRIPCnt;
        private System.Windows.Forms.NumericUpDown NUD_STRIPCnt;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancle;
        private System.Windows.Forms.Button btn_EXIT;
        private System.Windows.Forms.Label lbTitle;
        private System.Windows.Forms.Panel panel1;
    }
}