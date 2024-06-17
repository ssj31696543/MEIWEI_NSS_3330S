namespace NSS_3330S.FORM
{
    partial class frmPM
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPM));
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dTxtElap = new Owf.Controls.DigitalDisplayControl();
            this.lbCurrentTime = new System.Windows.Forms.Label();
            this.tmrPM = new System.Windows.Forms.Timer(this.components);
            this.txtPass = new System.Windows.Forms.TextBox();
            this.tmrShake = new System.Windows.Forms.Timer(this.components);
            this.btnClose = new Glass.GlassButton();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Tahoma", 17F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label2.Location = new System.Drawing.Point(424, 340);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(433, 28);
            this.label2.TabIndex = 60;
            this.label2.Text = "Press the close button to close the screen";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Tahoma", 17F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label1.Location = new System.Drawing.Point(475, 293);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(330, 28);
            this.label1.TabIndex = 59;
            this.label1.Text = "Lock the screen for your safety!";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dTxtElap
            // 
            this.dTxtElap.BackColor = System.Drawing.Color.Transparent;
            this.dTxtElap.DigitColor = System.Drawing.Color.LightGray;
            this.dTxtElap.DigitText = "0000:00:00";
            this.dTxtElap.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dTxtElap.Location = new System.Drawing.Point(283, 411);
            this.dTxtElap.Name = "dTxtElap";
            this.dTxtElap.Size = new System.Drawing.Size(714, 109);
            this.dTxtElap.TabIndex = 63;
            // 
            // lbCurrentTime
            // 
            this.lbCurrentTime.Font = new System.Drawing.Font("새굴림", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbCurrentTime.ForeColor = System.Drawing.Color.Silver;
            this.lbCurrentTime.Location = new System.Drawing.Point(318, 547);
            this.lbCurrentTime.Name = "lbCurrentTime";
            this.lbCurrentTime.Size = new System.Drawing.Size(644, 39);
            this.lbCurrentTime.TabIndex = 62;
            this.lbCurrentTime.Text = "00:00:00";
            this.lbCurrentTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tmrPM
            // 
            this.tmrPM.Interval = 500;
            this.tmrPM.Tick += new System.EventHandler(this.tmrPM_Tick);
            // 
            // txtPass
            // 
            this.txtPass.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.txtPass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPass.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPass.ForeColor = System.Drawing.Color.Black;
            this.txtPass.Location = new System.Drawing.Point(357, 672);
            this.txtPass.Margin = new System.Windows.Forms.Padding(3, 30, 3, 3);
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '●';
            this.txtPass.Size = new System.Drawing.Size(352, 40);
            this.txtPass.TabIndex = 64;
            this.txtPass.Text = "11";
            this.txtPass.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tmrShake
            // 
            this.tmrShake.Interval = 50;
            this.tmrShake.Tick += new System.EventHandler(this.tmrShake_Tick);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(731, 654);
            this.btnClose.Name = "btnClose";
            this.btnClose.OuterBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnClose.Padding = new System.Windows.Forms.Padding(15, 0, 20, 0);
            this.btnClose.Size = new System.Drawing.Size(187, 78);
            this.btnClose.TabIndex = 61;
            this.btnClose.Text = "CLOSE";
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmPM
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(33)))), ((int)(((byte)(43)))));
            this.ClientSize = new System.Drawing.Size(1280, 1024);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dTxtElap);
            this.Controls.Add(this.lbCurrentTime);
            this.Controls.Add(this.txtPass);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmPM";
            this.Text = "frmPM";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmPM_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Glass.GlassButton btnClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private Owf.Controls.DigitalDisplayControl dTxtElap;
        private System.Windows.Forms.Label lbCurrentTime;
        private System.Windows.Forms.Timer tmrPM;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.Timer tmrShake;
    }
}