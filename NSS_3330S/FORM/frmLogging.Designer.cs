namespace NSS_3330S.FORM
{
    partial class frmLogging
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogging));
            this.pnlLoginInfo = new System.Windows.Forms.Panel();
            this.btnUserAdd = new System.Windows.Forms.Button();
            this.btnAccountModify = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.rdbMAK = new System.Windows.Forms.RadioButton();
            this.rdbENG = new System.Windows.Forms.RadioButton();
            this.rdbOP = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxUserID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.pnlLoginInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLoginInfo
            // 
            this.pnlLoginInfo.BackColor = System.Drawing.Color.Transparent;
            this.pnlLoginInfo.Controls.Add(this.btnUserAdd);
            this.pnlLoginInfo.Controls.Add(this.btnAccountModify);
            this.pnlLoginInfo.Controls.Add(this.btnLogin);
            this.pnlLoginInfo.Controls.Add(this.rdbMAK);
            this.pnlLoginInfo.Controls.Add(this.rdbENG);
            this.pnlLoginInfo.Controls.Add(this.rdbOP);
            this.pnlLoginInfo.Controls.Add(this.label2);
            this.pnlLoginInfo.Controls.Add(this.tbxUserID);
            this.pnlLoginInfo.Controls.Add(this.label1);
            this.pnlLoginInfo.Controls.Add(this.txtPass);
            this.pnlLoginInfo.Location = new System.Drawing.Point(398, 262);
            this.pnlLoginInfo.Name = "pnlLoginInfo";
            this.pnlLoginInfo.Size = new System.Drawing.Size(479, 394);
            this.pnlLoginInfo.TabIndex = 39;
            // 
            // btnUserAdd
            // 
            this.btnUserAdd.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnUserAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUserAdd.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnUserAdd.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnUserAdd.ForeColor = System.Drawing.Color.Black;
            this.btnUserAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnUserAdd.Image")));
            this.btnUserAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUserAdd.Location = new System.Drawing.Point(165, 300);
            this.btnUserAdd.Name = "btnUserAdd";
            this.btnUserAdd.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnUserAdd.Size = new System.Drawing.Size(150, 68);
            this.btnUserAdd.TabIndex = 47;
            this.btnUserAdd.Text = "CHANGE PASSWORD";
            this.btnUserAdd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnUserAdd.UseVisualStyleBackColor = false;
            this.btnUserAdd.Click += new System.EventHandler(this.btnUserAdd_Click);
            // 
            // btnAccountModify
            // 
            this.btnAccountModify.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnAccountModify.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAccountModify.FlatAppearance.BorderSize = 0;
            this.btnAccountModify.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnAccountModify.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnAccountModify.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnAccountModify.ForeColor = System.Drawing.Color.Black;
            this.btnAccountModify.Image = global::NSS_3330S.Properties.Resources.doc_lines_icon_48;
            this.btnAccountModify.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAccountModify.Location = new System.Drawing.Point(318, 300);
            this.btnAccountModify.Name = "btnAccountModify";
            this.btnAccountModify.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnAccountModify.Size = new System.Drawing.Size(150, 68);
            this.btnAccountModify.TabIndex = 46;
            this.btnAccountModify.Text = "ACCOUNT\r\nMODIFY";
            this.btnAccountModify.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAccountModify.UseVisualStyleBackColor = false;
            this.btnAccountModify.Click += new System.EventHandler(this.btnAccountModify_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnLogin.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnLogin.ForeColor = System.Drawing.Color.Black;
            this.btnLogin.Image = global::NSS_3330S.Properties.Resources.key_icon_32;
            this.btnLogin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogin.Location = new System.Drawing.Point(9, 300);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Padding = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.btnLogin.Size = new System.Drawing.Size(153, 68);
            this.btnLogin.TabIndex = 46;
            this.btnLogin.Text = "LOG-IN";
            this.btnLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // rdbMAK
            // 
            this.rdbMAK.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbMAK.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbMAK.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdbMAK.Image = ((System.Drawing.Image)(resources.GetObject("rdbMAK.Image")));
            this.rdbMAK.Location = new System.Drawing.Point(319, 15);
            this.rdbMAK.Name = "rdbMAK";
            this.rdbMAK.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.rdbMAK.Size = new System.Drawing.Size(150, 133);
            this.rdbMAK.TabIndex = 45;
            this.rdbMAK.Tag = "2";
            this.rdbMAK.Text = "MAKER";
            this.rdbMAK.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.rdbMAK.UseVisualStyleBackColor = true;
            this.rdbMAK.CheckedChanged += new System.EventHandler(this.rdbOP_CheckedChanged);
            // 
            // rdbENG
            // 
            this.rdbENG.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbENG.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbENG.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdbENG.Image = ((System.Drawing.Image)(resources.GetObject("rdbENG.Image")));
            this.rdbENG.Location = new System.Drawing.Point(164, 15);
            this.rdbENG.Name = "rdbENG";
            this.rdbENG.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.rdbENG.Size = new System.Drawing.Size(150, 133);
            this.rdbENG.TabIndex = 44;
            this.rdbENG.Tag = "1";
            this.rdbENG.Text = "ENGINEER";
            this.rdbENG.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.rdbENG.UseVisualStyleBackColor = true;
            this.rdbENG.CheckedChanged += new System.EventHandler(this.rdbOP_CheckedChanged);
            // 
            // rdbOP
            // 
            this.rdbOP.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbOP.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbOP.Checked = true;
            this.rdbOP.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdbOP.Image = ((System.Drawing.Image)(resources.GetObject("rdbOP.Image")));
            this.rdbOP.Location = new System.Drawing.Point(9, 15);
            this.rdbOP.Name = "rdbOP";
            this.rdbOP.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.rdbOP.Size = new System.Drawing.Size(150, 133);
            this.rdbOP.TabIndex = 43;
            this.rdbOP.TabStop = true;
            this.rdbOP.Tag = "0";
            this.rdbOP.Text = "OPERATOR";
            this.rdbOP.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.rdbOP.UseVisualStyleBackColor = true;
            this.rdbOP.CheckedChanged += new System.EventHandler(this.rdbOP_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(11, 199);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 37;
            this.label2.Text = "USER ID";
            // 
            // tbxUserID
            // 
            this.tbxUserID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tbxUserID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbxUserID.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tbxUserID.ForeColor = System.Drawing.Color.Black;
            this.tbxUserID.Location = new System.Drawing.Point(164, 195);
            this.tbxUserID.Name = "tbxUserID";
            this.tbxUserID.Size = new System.Drawing.Size(305, 22);
            this.tbxUserID.TabIndex = 33;
            this.tbxUserID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(11, 233);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 36;
            this.label1.Text = "PASSWORD";
            // 
            // txtPass
            // 
            this.txtPass.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.txtPass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPass.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtPass.ForeColor = System.Drawing.Color.Black;
            this.txtPass.Location = new System.Drawing.Point(164, 231);
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '●';
            this.txtPass.Size = new System.Drawing.Size(305, 22);
            this.txtPass.TabIndex = 34;
            this.txtPass.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // frmLogging
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1274, 924);
            this.Controls.Add(this.pnlLoginInfo);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmLogging";
            this.Text = "frmLogging";
            this.VisibleChanged += new System.EventHandler(this.frmLogging_VisibleChanged);
            this.pnlLoginInfo.ResumeLayout(false);
            this.pnlLoginInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLoginInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbxUserID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.RadioButton rdbMAK;
        private System.Windows.Forms.RadioButton rdbENG;
        private System.Windows.Forms.RadioButton rdbOP;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnAccountModify;
        private System.Windows.Forms.Button btnUserAdd;
    }
}