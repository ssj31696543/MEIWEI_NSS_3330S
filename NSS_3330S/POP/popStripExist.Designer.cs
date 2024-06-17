namespace NSS_3330S.POP
{
    partial class popStripExist
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(popStripExist));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBuzzerStop = new System.Windows.Forms.Button();
            this.lblTime = new Owf.Controls.DigitalDisplayControl();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblErrNo = new System.Windows.Forms.Label();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.lbTitle = new System.Windows.Forms.Label();
            this.tmrErr = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnBuzzerStop);
            this.panel1.Controls.Add(this.lblTime);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.lblErrNo);
            this.panel1.Controls.Add(this.btnYes);
            this.panel1.Controls.Add(this.btnNo);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 41);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(776, 343);
            this.panel1.TabIndex = 12;
            // 
            // btnBuzzerStop
            // 
            this.btnBuzzerStop.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnBuzzerStop.Location = new System.Drawing.Point(154, 235);
            this.btnBuzzerStop.Name = "btnBuzzerStop";
            this.btnBuzzerStop.Size = new System.Drawing.Size(93, 72);
            this.btnBuzzerStop.TabIndex = 916;
            this.btnBuzzerStop.Text = "BUZZER\r\nOFF";
            this.btnBuzzerStop.UseVisualStyleBackColor = true;
            this.btnBuzzerStop.Click += new System.EventHandler(this.btnBuzzerStop_Click);
            // 
            // lblTime
            // 
            this.lblTime.BackColor = System.Drawing.Color.Transparent;
            this.lblTime.DigitColor = System.Drawing.Color.Gray;
            this.lblTime.DigitText = "00:00:00";
            this.lblTime.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTime.ForeColor = System.Drawing.SystemColors.Control;
            this.lblTime.Location = new System.Drawing.Point(250, 176);
            this.lblTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(311, 52);
            this.lblTime.TabIndex = 907;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(21, 10);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 106);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // lblErrNo
            // 
            this.lblErrNo.BackColor = System.Drawing.Color.Transparent;
            this.lblErrNo.Font = new System.Drawing.Font("맑은 고딕", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblErrNo.ForeColor = System.Drawing.Color.Black;
            this.lblErrNo.Location = new System.Drawing.Point(94, 26);
            this.lblErrNo.Name = "lblErrNo";
            this.lblErrNo.Size = new System.Drawing.Size(647, 145);
            this.lblErrNo.TabIndex = 6;
            this.lblErrNo.Text = "VAC ALARM OCUURED!\r\n ARE THERE ANY MATERIAL?";
            this.lblErrNo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnYes
            // 
            this.btnYes.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnYes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnYes.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnYes.ForeColor = System.Drawing.Color.Black;
            this.btnYes.Location = new System.Drawing.Point(253, 235);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(173, 72);
            this.btnYes.TabIndex = 7;
            this.btnYes.Text = "YES";
            this.btnYes.UseVisualStyleBackColor = false;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // btnNo
            // 
            this.btnNo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btnNo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnNo.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnNo.ForeColor = System.Drawing.Color.Black;
            this.btnNo.Location = new System.Drawing.Point(432, 235);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(173, 72);
            this.btnNo.TabIndex = 8;
            this.btnNo.Text = "NO";
            this.btnNo.UseVisualStyleBackColor = false;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // lbTitle
            // 
            this.lbTitle.BackColor = System.Drawing.Color.Orange;
            this.lbTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbTitle.ForeColor = System.Drawing.Color.LightGray;
            this.lbTitle.Location = new System.Drawing.Point(0, 0);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lbTitle.Size = new System.Drawing.Size(776, 41);
            this.lbTitle.TabIndex = 11;
            this.lbTitle.Text = "ALARM";
            this.lbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbTitle_MouseDown);
            this.lbTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbTitle_MouseMove);
            // 
            // tmrErr
            // 
            this.tmrErr.Interval = 500;
            this.tmrErr.Tick += new System.EventHandler(this.tmrErr_Tick);
            // 
            // popStripExist
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 384);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lbTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popStripExist";
            this.Text = "popStripExist";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblErrNo;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Label lbTitle;
        public Owf.Controls.DigitalDisplayControl lblTime;
        private System.Windows.Forms.Button btnBuzzerStop;
        private System.Windows.Forms.Timer tmrErr;
    }
}