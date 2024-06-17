namespace NSS_3330S.POP
{
    partial class popAccount
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(popAccount));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbWarningMsg = new System.Windows.Forms.Label();
            this.pnlTitlebar = new System.Windows.Forms.Panel();
            this.pbxSave = new System.Windows.Forms.PictureBox();
            this.pbxClose = new System.Windows.Forms.PictureBox();
            this.pbxPw2 = new System.Windows.Forms.PictureBox();
            this.btnDeleteId = new System.Windows.Forms.Button();
            this.pbxPw = new System.Windows.Forms.PictureBox();
            this.btnCreateId = new System.Windows.Forms.Button();
            this.pbxId = new System.Windows.Forms.PictureBox();
            this.pnlPw2 = new System.Windows.Forms.Panel();
            this.tbxPw2 = new System.Windows.Forms.TextBox();
            this.pnlPw = new System.Windows.Forms.Panel();
            this.tbxPw = new System.Windows.Forms.TextBox();
            this.pnlId = new System.Windows.Forms.Panel();
            this.tbxId = new System.Windows.Forms.TextBox();
            this.btnMaker = new System.Windows.Forms.Button();
            this.btnEng = new System.Windows.Forms.Button();
            this.btnOp = new System.Windows.Forms.Button();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.pnlTitlebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPw2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPw)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxId)).BeginInit();
            this.pnlPw2.SuspendLayout();
            this.pnlPw.SuspendLayout();
            this.pnlId.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel1.Controls.Add(this.lbWarningMsg);
            this.panel1.Controls.Add(this.pnlTitlebar);
            this.panel1.Controls.Add(this.pbxPw2);
            this.panel1.Controls.Add(this.btnDeleteId);
            this.panel1.Controls.Add(this.pbxPw);
            this.panel1.Controls.Add(this.btnCreateId);
            this.panel1.Controls.Add(this.pbxId);
            this.panel1.Controls.Add(this.pnlPw2);
            this.panel1.Controls.Add(this.pnlPw);
            this.panel1.Controls.Add(this.pnlId);
            this.panel1.Controls.Add(this.btnMaker);
            this.panel1.Controls.Add(this.btnEng);
            this.panel1.Controls.Add(this.btnOp);
            this.panel1.Location = new System.Drawing.Point(2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(502, 558);
            this.panel1.TabIndex = 0;
            // 
            // lbWarningMsg
            // 
            this.lbWarningMsg.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lbWarningMsg.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbWarningMsg.ForeColor = System.Drawing.Color.Tomato;
            this.lbWarningMsg.Location = new System.Drawing.Point(0, 534);
            this.lbWarningMsg.Name = "lbWarningMsg";
            this.lbWarningMsg.Size = new System.Drawing.Size(502, 24);
            this.lbWarningMsg.TabIndex = 38;
            this.lbWarningMsg.Text = "Password Is Not Same!!";
            this.lbWarningMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbWarningMsg.Visible = false;
            // 
            // pnlTitlebar
            // 
            this.pnlTitlebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(34)))));
            this.pnlTitlebar.Controls.Add(this.pbxSave);
            this.pnlTitlebar.Controls.Add(this.pbxClose);
            this.pnlTitlebar.Location = new System.Drawing.Point(0, 0);
            this.pnlTitlebar.Name = "pnlTitlebar";
            this.pnlTitlebar.Size = new System.Drawing.Size(502, 50);
            this.pnlTitlebar.TabIndex = 2;
            this.pnlTitlebar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            // 
            // pbxSave
            // 
            this.pbxSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbxSave.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbxSave.Image = global::NSS_3330S.Properties.Resources.Save_Gray_100;
            this.pbxSave.Location = new System.Drawing.Point(402, 0);
            this.pbxSave.Name = "pbxSave";
            this.pbxSave.Size = new System.Drawing.Size(50, 50);
            this.pbxSave.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxSave.TabIndex = 39;
            this.pbxSave.TabStop = false;
            this.pbxSave.Click += new System.EventHandler(this.pbxSave_Click);
            // 
            // pbxClose
            // 
            this.pbxClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbxClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbxClose.Image = global::NSS_3330S.Properties.Resources.Close_Gray_50;
            this.pbxClose.Location = new System.Drawing.Point(452, 0);
            this.pbxClose.Name = "pbxClose";
            this.pbxClose.Size = new System.Drawing.Size(50, 50);
            this.pbxClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxClose.TabIndex = 0;
            this.pbxClose.TabStop = false;
            this.pbxClose.Click += new System.EventHandler(this.pbxClose_Click);
            // 
            // pbxPw2
            // 
            this.pbxPw2.Image = ((System.Drawing.Image)(resources.GetObject("pbxPw2.Image")));
            this.pbxPw2.Location = new System.Drawing.Point(60, 386);
            this.pbxPw2.Name = "pbxPw2";
            this.pbxPw2.Size = new System.Drawing.Size(50, 50);
            this.pbxPw2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxPw2.TabIndex = 37;
            this.pbxPw2.TabStop = false;
            // 
            // btnDeleteId
            // 
            this.btnDeleteId.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteId.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(33)))), ((int)(((byte)(43)))));
            this.btnDeleteId.FlatAppearance.BorderSize = 0;
            this.btnDeleteId.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteId.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnDeleteId.ForeColor = System.Drawing.Color.Black;
            this.btnDeleteId.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteId.Image")));
            this.btnDeleteId.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDeleteId.Location = new System.Drawing.Point(254, 467);
            this.btnDeleteId.Margin = new System.Windows.Forms.Padding(0);
            this.btnDeleteId.Name = "btnDeleteId";
            this.btnDeleteId.Padding = new System.Windows.Forms.Padding(12);
            this.btnDeleteId.Size = new System.Drawing.Size(164, 50);
            this.btnDeleteId.TabIndex = 6;
            this.btnDeleteId.Text = "DELETE";
            this.btnDeleteId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDeleteId.UseVisualStyleBackColor = true;
            this.btnDeleteId.Click += new System.EventHandler(this.btnDeleteId_Click);
            this.btnDeleteId.Paint += new System.Windows.Forms.PaintEventHandler(this.btnDeleteId_Paint);
            // 
            // pbxPw
            // 
            this.pbxPw.Image = ((System.Drawing.Image)(resources.GetObject("pbxPw.Image")));
            this.pbxPw.Location = new System.Drawing.Point(60, 330);
            this.pbxPw.Name = "pbxPw";
            this.pbxPw.Size = new System.Drawing.Size(50, 50);
            this.pbxPw.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxPw.TabIndex = 37;
            this.pbxPw.TabStop = false;
            // 
            // btnCreateId
            // 
            this.btnCreateId.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreateId.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(33)))), ((int)(((byte)(43)))));
            this.btnCreateId.FlatAppearance.BorderSize = 0;
            this.btnCreateId.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreateId.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnCreateId.ForeColor = System.Drawing.Color.Black;
            this.btnCreateId.Image = global::NSS_3330S.Properties.Resources.round_plus_icon_48;
            this.btnCreateId.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCreateId.Location = new System.Drawing.Point(75, 467);
            this.btnCreateId.Margin = new System.Windows.Forms.Padding(0);
            this.btnCreateId.Name = "btnCreateId";
            this.btnCreateId.Padding = new System.Windows.Forms.Padding(12);
            this.btnCreateId.Size = new System.Drawing.Size(164, 50);
            this.btnCreateId.TabIndex = 5;
            this.btnCreateId.Text = "CREATE";
            this.btnCreateId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCreateId.UseVisualStyleBackColor = true;
            this.btnCreateId.Click += new System.EventHandler(this.btnCreateId_Click);
            this.btnCreateId.Paint += new System.Windows.Forms.PaintEventHandler(this.btnDeleteId_Paint);
            // 
            // pbxId
            // 
            this.pbxId.Image = global::NSS_3330S.Properties.Resources.user_icon_481;
            this.pbxId.Location = new System.Drawing.Point(60, 274);
            this.pbxId.Name = "pbxId";
            this.pbxId.Size = new System.Drawing.Size(50, 50);
            this.pbxId.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxId.TabIndex = 37;
            this.pbxId.TabStop = false;
            // 
            // pnlPw2
            // 
            this.pnlPw2.Controls.Add(this.tbxPw2);
            this.pnlPw2.Location = new System.Drawing.Point(116, 386);
            this.pnlPw2.Name = "pnlPw2";
            this.pnlPw2.Size = new System.Drawing.Size(319, 50);
            this.pnlPw2.TabIndex = 3;
            // 
            // tbxPw2
            // 
            this.tbxPw2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tbxPw2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbxPw2.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.tbxPw2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tbxPw2.Location = new System.Drawing.Point(7, 9);
            this.tbxPw2.Name = "tbxPw2";
            this.tbxPw2.Size = new System.Drawing.Size(304, 32);
            this.tbxPw2.TabIndex = 3;
            this.tbxPw2.Text = "PASSWORD";
            // 
            // pnlPw
            // 
            this.pnlPw.Controls.Add(this.tbxPw);
            this.pnlPw.Location = new System.Drawing.Point(116, 330);
            this.pnlPw.Name = "pnlPw";
            this.pnlPw.Size = new System.Drawing.Size(319, 50);
            this.pnlPw.TabIndex = 2;
            // 
            // tbxPw
            // 
            this.tbxPw.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tbxPw.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbxPw.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.tbxPw.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tbxPw.Location = new System.Drawing.Point(7, 9);
            this.tbxPw.Name = "tbxPw";
            this.tbxPw.Size = new System.Drawing.Size(304, 32);
            this.tbxPw.TabIndex = 2;
            this.tbxPw.Text = "PASSWORD";
            // 
            // pnlId
            // 
            this.pnlId.Controls.Add(this.tbxId);
            this.pnlId.Location = new System.Drawing.Point(116, 274);
            this.pnlId.Name = "pnlId";
            this.pnlId.Size = new System.Drawing.Size(319, 50);
            this.pnlId.TabIndex = 1;
            // 
            // tbxId
            // 
            this.tbxId.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tbxId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbxId.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.tbxId.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tbxId.Location = new System.Drawing.Point(7, 9);
            this.tbxId.Name = "tbxId";
            this.tbxId.Size = new System.Drawing.Size(304, 32);
            this.tbxId.TabIndex = 0;
            this.tbxId.Text = "ID";
            this.tbxId.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tbxId_MouseClick);
            // 
            // btnMaker
            // 
            this.btnMaker.BackColor = System.Drawing.SystemColors.Control;
            this.btnMaker.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMaker.FlatAppearance.BorderSize = 0;
            this.btnMaker.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMaker.ForeColor = System.Drawing.Color.Black;
            this.btnMaker.Image = ((System.Drawing.Image)(resources.GetObject("btnMaker.Image")));
            this.btnMaker.Location = new System.Drawing.Point(332, 74);
            this.btnMaker.Name = "btnMaker";
            this.btnMaker.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.btnMaker.Size = new System.Drawing.Size(150, 133);
            this.btnMaker.TabIndex = 35;
            this.btnMaker.Tag = "2";
            this.btnMaker.Text = "MAKER";
            this.btnMaker.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnMaker.UseVisualStyleBackColor = false;
            this.btnMaker.Visible = false;
            this.btnMaker.Click += new System.EventHandler(this.btnMaker_Click);
            // 
            // btnEng
            // 
            this.btnEng.BackColor = System.Drawing.SystemColors.Control;
            this.btnEng.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEng.FlatAppearance.BorderSize = 0;
            this.btnEng.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEng.ForeColor = System.Drawing.Color.Black;
            this.btnEng.Image = ((System.Drawing.Image)(resources.GetObject("btnEng.Image")));
            this.btnEng.Location = new System.Drawing.Point(177, 74);
            this.btnEng.Name = "btnEng";
            this.btnEng.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.btnEng.Size = new System.Drawing.Size(150, 133);
            this.btnEng.TabIndex = 34;
            this.btnEng.Tag = "1";
            this.btnEng.Text = "ENGINEER";
            this.btnEng.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnEng.UseVisualStyleBackColor = false;
            this.btnEng.Click += new System.EventHandler(this.btnEng_Click);
            // 
            // btnOp
            // 
            this.btnOp.BackColor = System.Drawing.Color.SkyBlue;
            this.btnOp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOp.FlatAppearance.BorderSize = 0;
            this.btnOp.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOp.ForeColor = System.Drawing.Color.Black;
            this.btnOp.Image = ((System.Drawing.Image)(resources.GetObject("btnOp.Image")));
            this.btnOp.Location = new System.Drawing.Point(22, 74);
            this.btnOp.Name = "btnOp";
            this.btnOp.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.btnOp.Size = new System.Drawing.Size(150, 133);
            this.btnOp.TabIndex = 33;
            this.btnOp.Tag = "0";
            this.btnOp.Text = "OPERATOR";
            this.btnOp.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnOp.UseVisualStyleBackColor = false;
            this.btnOp.Click += new System.EventHandler(this.btnOp_Click);
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Enabled = true;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // popAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(506, 562);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popAccount";
            this.Text = "Account Manager";
            this.Load += new System.EventHandler(this.popAccount_Load);
            this.VisibleChanged += new System.EventHandler(this.popAccount_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.pnlTitlebar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPw2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPw)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxId)).EndInit();
            this.pnlPw2.ResumeLayout(false);
            this.pnlPw2.PerformLayout();
            this.pnlPw.ResumeLayout(false);
            this.pnlPw.PerformLayout();
            this.pnlId.ResumeLayout(false);
            this.pnlId.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnMaker;
        private System.Windows.Forms.Button btnEng;
        private System.Windows.Forms.Button btnOp;
        private System.Windows.Forms.Panel pnlTitlebar;
        private System.Windows.Forms.PictureBox pbxClose;
        private System.Windows.Forms.Panel pnlId;
        private System.Windows.Forms.PictureBox pbxPw2;
        private System.Windows.Forms.PictureBox pbxPw;
        private System.Windows.Forms.PictureBox pbxId;
        private System.Windows.Forms.Button btnDeleteId;
        private System.Windows.Forms.Button btnCreateId;
        private System.Windows.Forms.PictureBox pbxSave;
        private System.Windows.Forms.TextBox tbxId;
        private System.Windows.Forms.Panel pnlPw2;
        private System.Windows.Forms.TextBox tbxPw2;
        private System.Windows.Forms.Panel pnlPw;
        private System.Windows.Forms.TextBox tbxPw;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Label lbWarningMsg;
    }
}