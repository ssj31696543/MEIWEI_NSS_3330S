namespace NSS_3330S.POP
{
    partial class popLotEnd
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(popLotEnd));
            this.lblLotEnd = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBuzzerStop = new System.Windows.Forms.Button();
            this.lblTime = new Owf.Controls.DigitalDisplayControl();
            this.btnYes = new System.Windows.Forms.Button();
            this.lbTitle = new System.Windows.Forms.Label();
            this.tmrWaitTime = new System.Windows.Forms.Timer(this.components);
            this.lbLotId = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblLotEnd
            // 
            this.lblLotEnd.BackColor = System.Drawing.Color.Transparent;
            this.lblLotEnd.Font = new System.Drawing.Font("맑은 고딕", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblLotEnd.ForeColor = System.Drawing.Color.Black;
            this.lblLotEnd.Location = new System.Drawing.Point(179, 23);
            this.lblLotEnd.Name = "lblLotEnd";
            this.lblLotEnd.Size = new System.Drawing.Size(274, 101);
            this.lblLotEnd.TabIndex = 6;
            this.lblLotEnd.Text = "LOT END!";
            this.lblLotEnd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.lbLotId);
            this.panel1.Controls.Add(this.btnBuzzerStop);
            this.panel1.Controls.Add(this.lblTime);
            this.panel1.Controls.Add(this.lblLotEnd);
            this.panel1.Controls.Add(this.btnYes);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 62);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(563, 407);
            this.panel1.TabIndex = 14;
            // 
            // btnBuzzerStop
            // 
            this.btnBuzzerStop.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnBuzzerStop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnBuzzerStop.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnBuzzerStop.ForeColor = System.Drawing.Color.Black;
            this.btnBuzzerStop.Location = new System.Drawing.Point(80, 280);
            this.btnBuzzerStop.Name = "btnBuzzerStop";
            this.btnBuzzerStop.Size = new System.Drawing.Size(93, 102);
            this.btnBuzzerStop.TabIndex = 916;
            this.btnBuzzerStop.Text = "부저\r\n끄기";
            this.btnBuzzerStop.UseVisualStyleBackColor = false;
            this.btnBuzzerStop.Click += new System.EventHandler(this.btnBuzzerStop_Click);
            // 
            // lblTime
            // 
            this.lblTime.BackColor = System.Drawing.Color.Transparent;
            this.lblTime.DigitColor = System.Drawing.Color.Gray;
            this.lblTime.DigitText = "00:00:00";
            this.lblTime.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTime.ForeColor = System.Drawing.SystemColors.Control;
            this.lblTime.Location = new System.Drawing.Point(142, 170);
            this.lblTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(311, 28);
            this.lblTime.TabIndex = 907;
            // 
            // btnYes
            // 
            this.btnYes.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnYes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnYes.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnYes.ForeColor = System.Drawing.Color.Black;
            this.btnYes.Location = new System.Drawing.Point(179, 280);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(289, 102);
            this.btnYes.TabIndex = 7;
            this.btnYes.Text = "OK";
            this.btnYes.UseVisualStyleBackColor = false;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // lbTitle
            // 
            this.lbTitle.BackColor = System.Drawing.Color.Violet;
            this.lbTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbTitle.ForeColor = System.Drawing.Color.LightGray;
            this.lbTitle.Location = new System.Drawing.Point(0, 0);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lbTitle.Size = new System.Drawing.Size(563, 62);
            this.lbTitle.TabIndex = 13;
            this.lbTitle.Text = "LOT END";
            this.lbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbTitle_MouseDown);
            this.lbTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbTitle_MouseMove);
            // 
            // tmrWaitTime
            // 
            this.tmrWaitTime.Interval = 500;
            this.tmrWaitTime.Tick += new System.EventHandler(this.tmrWaitTimeTick);
            // 
            // lbLotItsId
            // 
            this.lbLotId.BackColor = System.Drawing.Color.Transparent;
            this.lbLotId.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbLotId.ForeColor = System.Drawing.Color.Black;
            this.lbLotId.Location = new System.Drawing.Point(55, 127);
            this.lbLotId.Name = "lbLotItsId";
            this.lbLotId.Size = new System.Drawing.Size(463, 39);
            this.lbLotId.TabIndex = 917;
            this.lbLotId.Text = "LOT ID : 123456789123, ITS ID : 123456789123 0";
            this.lbLotId.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(78, 22);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 106);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 918;
            this.pictureBox1.TabStop = false;
            // 
            // popLotEnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 469);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lbTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popLotEnd";
            this.Text = "popLotEnd";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblLotEnd;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnBuzzerStop;
        public Owf.Controls.DigitalDisplayControl lblTime;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Label lbTitle;
        private System.Windows.Forms.Timer tmrWaitTime;
        private System.Windows.Forms.Label lbLotId;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}