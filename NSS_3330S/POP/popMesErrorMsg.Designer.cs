namespace NSS_3330S.POP
{
    partial class popMesErrorMsg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(popMesErrorMsg));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.buttonClose_EMsg = new System.Windows.Forms.Button();
            this.lblAction = new System.Windows.Forms.Label();
            this.pnlCompBtnPnl = new System.Windows.Forms.Panel();
            this.btnBuzzerStop = new System.Windows.Forms.Button();
            this.lblCause = new System.Windows.Forms.Label();
            this.tmrErr = new System.Windows.Forms.Timer(this.components);
            this.lblTime = new Owf.Controls.DigitalDisplayControl();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnExpComp = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Scroll Up_25px.png");
            this.imageList1.Images.SetKeyName(1, "Scroll Down_25px.png");
            // 
            // buttonClose_EMsg
            // 
            this.buttonClose_EMsg.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClose_EMsg.Location = new System.Drawing.Point(87, 167);
            this.buttonClose_EMsg.Name = "buttonClose_EMsg";
            this.buttonClose_EMsg.Size = new System.Drawing.Size(202, 47);
            this.buttonClose_EMsg.TabIndex = 909;
            this.buttonClose_EMsg.Text = "Close";
            this.buttonClose_EMsg.UseVisualStyleBackColor = true;
            this.buttonClose_EMsg.Click += new System.EventHandler(this.buttonClose_EMsg_Click);
            // 
            // lblAction
            // 
            this.lblAction.BackColor = System.Drawing.Color.White;
            this.lblAction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblAction.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblAction.ForeColor = System.Drawing.Color.Black;
            this.lblAction.Location = new System.Drawing.Point(12, 172);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(560, 153);
            this.lblAction.TabIndex = 914;
            this.lblAction.Text = "Details";
            this.lblAction.Visible = false;
            // 
            // pnlCompBtnPnl
            // 
            this.pnlCompBtnPnl.BackColor = System.Drawing.Color.Transparent;
            this.pnlCompBtnPnl.Location = new System.Drawing.Point(271, 472);
            this.pnlCompBtnPnl.Name = "pnlCompBtnPnl";
            this.pnlCompBtnPnl.Size = new System.Drawing.Size(32, 32);
            this.pnlCompBtnPnl.TabIndex = 922;
            // 
            // btnBuzzerStop
            // 
            this.btnBuzzerStop.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuzzerStop.Location = new System.Drawing.Point(368, 167);
            this.btnBuzzerStop.Name = "btnBuzzerStop";
            this.btnBuzzerStop.Size = new System.Drawing.Size(202, 47);
            this.btnBuzzerStop.TabIndex = 915;
            this.btnBuzzerStop.Text = "Buzzer Stop";
            this.btnBuzzerStop.UseVisualStyleBackColor = true;
            this.btnBuzzerStop.Click += new System.EventHandler(this.btnBuzzerStop_Click);
            // 
            // lblCause
            // 
            this.lblCause.BackColor = System.Drawing.Color.Transparent;
            this.lblCause.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCause.ForeColor = System.Drawing.Color.Black;
            this.lblCause.Location = new System.Drawing.Point(105, 51);
            this.lblCause.Name = "lblCause";
            this.lblCause.Size = new System.Drawing.Size(467, 66);
            this.lblCause.TabIndex = 912;
            this.lblCause.Text = "Please enter the detail";
            // 
            // tmrErr
            // 
            this.tmrErr.Enabled = true;
            this.tmrErr.Interval = 500;
            // 
            // lblTime
            // 
            this.lblTime.BackColor = System.Drawing.Color.Transparent;
            this.lblTime.DigitColor = System.Drawing.Color.Gray;
            this.lblTime.DigitText = "00:00:00";
            this.lblTime.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTime.ForeColor = System.Drawing.SystemColors.Control;
            this.lblTime.Location = new System.Drawing.Point(252, 123);
            this.lblTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(162, 30);
            this.lblTime.TabIndex = 918;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.ForeColor = System.Drawing.Color.Black;
            this.lblTitle.Location = new System.Drawing.Point(105, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(467, 32);
            this.lblTitle.TabIndex = 919;
            this.lblTitle.Tag = "10";
            this.lblTitle.Text = "MES_TIMEOUT";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnExpComp
            // 
            this.btnExpComp.BackColor = System.Drawing.Color.Transparent;
            this.btnExpComp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnExpComp.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnExpComp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpComp.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnExpComp.ForeColor = System.Drawing.Color.OrangeRed;
            this.btnExpComp.ImageIndex = 1;
            this.btnExpComp.ImageList = this.imageList1;
            this.btnExpComp.Location = new System.Drawing.Point(26, 175);
            this.btnExpComp.Name = "btnExpComp";
            this.btnExpComp.Size = new System.Drawing.Size(30, 31);
            this.btnExpComp.TabIndex = 916;
            this.btnExpComp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExpComp.UseVisualStyleBackColor = false;
            this.btnExpComp.Click += new System.EventHandler(this.btnExpComp_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(4, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(95, 93);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 921;
            this.pictureBox1.TabStop = false;
            // 
            // popMesErrorMsg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 227);
            this.ControlBox = false;
            this.Controls.Add(this.buttonClose_EMsg);
            this.Controls.Add(this.btnExpComp);
            this.Controls.Add(this.btnBuzzerStop);
            this.Controls.Add(this.lblAction);
            this.Controls.Add(this.pnlCompBtnPnl);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.lblCause);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pictureBox1);
            this.Name = "popMesErrorMsg";
            this.Text = "MES ALARM";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btnExpComp;
        private System.Windows.Forms.Button buttonClose_EMsg;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.Panel pnlCompBtnPnl;
        private System.Windows.Forms.Button btnBuzzerStop;
        private System.Windows.Forms.Label lblCause;
        private System.Windows.Forms.Timer tmrErr;
        public Owf.Controls.DigitalDisplayControl lblTime;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}