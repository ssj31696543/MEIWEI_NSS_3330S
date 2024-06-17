namespace NSS_3330S.POP
{
    partial class popCheckPassword
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(popCheckPassword));
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbxPassword = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnCANCEL = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lbNotCorrect = new System.Windows.Forms.Label();
            this.lbCurrentUserId = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.tmrShake = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.tbxPassword);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.btnCANCEL);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.lbNotCorrect);
            this.panel1.Controls.Add(this.lbCurrentUserId);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(5, 18);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(566, 238);
            this.panel1.TabIndex = 0;
            // 
            // tbxPassword
            // 
            this.tbxPassword.BackColor = System.Drawing.SystemColors.Control;
            this.tbxPassword.ForeColor = System.Drawing.SystemColors.Highlight;
            this.tbxPassword.Location = new System.Drawing.Point(138, 103);
            this.tbxPassword.Name = "tbxPassword";
            this.tbxPassword.PasswordChar = '●';
            this.tbxPassword.Size = new System.Drawing.Size(316, 33);
            this.tbxPassword.TabIndex = 3;
            this.tbxPassword.Text = "llllllll";
            this.tbxPassword.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tbxPassword_MouseClick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.ErrorImage = null;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(457, 3);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // btnCANCEL
            // 
            this.btnCANCEL.BackColor = System.Drawing.SystemColors.Control;
            this.btnCANCEL.ForeColor = System.Drawing.Color.Black;
            this.btnCANCEL.Location = new System.Drawing.Point(286, 189);
            this.btnCANCEL.Name = "btnCANCEL";
            this.btnCANCEL.Size = new System.Drawing.Size(120, 45);
            this.btnCANCEL.TabIndex = 1;
            this.btnCANCEL.Text = "CANCEL";
            this.btnCANCEL.UseVisualStyleBackColor = false;
            this.btnCANCEL.Click += new System.EventHandler(this.btnCANCEL_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.SystemColors.Control;
            this.btnOK.ForeColor = System.Drawing.Color.Black;
            this.btnOK.Location = new System.Drawing.Point(160, 189);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(120, 45);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(9, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 25);
            this.label4.TabIndex = 0;
            this.label4.Text = "PASSWORD :";
            // 
            // lbNotCorrect
            // 
            this.lbNotCorrect.AutoSize = true;
            this.lbNotCorrect.ForeColor = System.Drawing.Color.Tomato;
            this.lbNotCorrect.Location = new System.Drawing.Point(154, 152);
            this.lbNotCorrect.Name = "lbNotCorrect";
            this.lbNotCorrect.Size = new System.Drawing.Size(262, 25);
            this.lbNotCorrect.TabIndex = 0;
            this.lbNotCorrect.Text = "PASSWORD NOT CORRECT!!";
            this.lbNotCorrect.Visible = false;
            // 
            // lbCurrentUserId
            // 
            this.lbCurrentUserId.AutoSize = true;
            this.lbCurrentUserId.ForeColor = System.Drawing.Color.Black;
            this.lbCurrentUserId.Location = new System.Drawing.Point(144, 57);
            this.lbCurrentUserId.Name = "lbCurrentUserId";
            this.lbCurrentUserId.Size = new System.Drawing.Size(117, 25);
            this.lbCurrentUserId.TabIndex = 0;
            this.lbCurrentUserId.Text = "OOOOOOO";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(9, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 25);
            this.label3.TabIndex = 0;
            this.label3.Text = "USER NAME :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "  PASSWORD CHECK";
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.Black;
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(577, 17);
            this.pnlTop.TabIndex = 1;
            this.pnlTop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTop_MouseDown);
            // 
            // tmrShake
            // 
            this.tmrShake.Interval = 50;
            this.tmrShake.Tick += new System.EventHandler(this.tmrShake_Tick);
            // 
            // popCheckPassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(577, 262);
            this.ControlBox = false;
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "popCheckPassword";
            this.Text = "PASSWORD CHECK";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.popCheckPassword_FormClosing);
            this.Load += new System.EventHandler(this.popCheckPassword_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCANCEL;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox tbxPassword;
        private System.Windows.Forms.Label lbCurrentUserId;
        private System.Windows.Forms.Label lbNotCorrect;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Timer tmrShake;
    }
}