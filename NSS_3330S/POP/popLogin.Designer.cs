namespace NSS_3330S.POP
{
    partial class popLogin
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnPasswordOK = new System.Windows.Forms.Button();
            this.lblModuleNameTitle = new System.Windows.Forms.Label();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.tmrShake = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.txtVeriPass = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(288, 174);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 46);
            this.button1.TabIndex = 15;
            this.button1.Text = "CANCEL";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnPasswordOK
            // 
            this.btnPasswordOK.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnPasswordOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnPasswordOK.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnPasswordOK.ForeColor = System.Drawing.Color.Black;
            this.btnPasswordOK.Location = new System.Drawing.Point(57, 174);
            this.btnPasswordOK.Name = "btnPasswordOK";
            this.btnPasswordOK.Size = new System.Drawing.Size(150, 46);
            this.btnPasswordOK.TabIndex = 14;
            this.btnPasswordOK.Text = "CHANGE";
            this.btnPasswordOK.UseVisualStyleBackColor = false;
            this.btnPasswordOK.Click += new System.EventHandler(this.btnPasswordOK_Click);
            // 
            // lblModuleNameTitle
            // 
            this.lblModuleNameTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModuleNameTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblModuleNameTitle.Location = new System.Drawing.Point(53, 37);
            this.lblModuleNameTitle.Margin = new System.Windows.Forms.Padding(3, 20, 3, 0);
            this.lblModuleNameTitle.Name = "lblModuleNameTitle";
            this.lblModuleNameTitle.Size = new System.Drawing.Size(154, 40);
            this.lblModuleNameTitle.TabIndex = 13;
            this.lblModuleNameTitle.Text = "Login Password : ";
            this.lblModuleNameTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPass
            // 
            this.txtPass.BackColor = System.Drawing.Color.White;
            this.txtPass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPass.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPass.ForeColor = System.Drawing.Color.Black;
            this.txtPass.Location = new System.Drawing.Point(213, 37);
            this.txtPass.Margin = new System.Windows.Forms.Padding(3, 30, 3, 3);
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '●';
            this.txtPass.Size = new System.Drawing.Size(225, 40);
            this.txtPass.TabIndex = 12;
            this.txtPass.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tmrShake
            // 
            this.tmrShake.Interval = 50;
            this.tmrShake.Tick += new System.EventHandler(this.tmrShake_Tick);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(12, 91);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 20, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 40);
            this.label1.TabIndex = 17;
            this.label1.Text = "Verify New Password : ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVeriPass
            // 
            this.txtVeriPass.BackColor = System.Drawing.Color.White;
            this.txtVeriPass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtVeriPass.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVeriPass.ForeColor = System.Drawing.Color.Black;
            this.txtVeriPass.Location = new System.Drawing.Point(213, 91);
            this.txtVeriPass.Margin = new System.Windows.Forms.Padding(3, 30, 3, 3);
            this.txtVeriPass.Name = "txtVeriPass";
            this.txtVeriPass.PasswordChar = '●';
            this.txtVeriPass.Size = new System.Drawing.Size(225, 40);
            this.txtVeriPass.TabIndex = 16;
            this.txtVeriPass.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // popLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 232);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtVeriPass);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnPasswordOK);
            this.Controls.Add(this.lblModuleNameTitle);
            this.Controls.Add(this.txtPass);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "popLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LOGIN";
            this.VisibleChanged += new System.EventHandler(this.popLogin_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnPasswordOK;
        private System.Windows.Forms.Label lblModuleNameTitle;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.Timer tmrShake;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtVeriPass;
    }
}