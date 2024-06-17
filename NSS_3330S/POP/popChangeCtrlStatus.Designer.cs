namespace NSS_3330S.POP
{
    partial class popChangeCtrlStatus
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
            this.rdnOnlineRemote = new System.Windows.Forms.RadioButton();
            this.btnCtrlChange = new Glass.GlassButton();
            this.rdnOnlineLocal = new System.Windows.Forms.RadioButton();
            this.rdnOffline = new System.Windows.Forms.RadioButton();
            this.lbControlStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tmrStatus = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // rdnOnlineRemote
            // 
            this.rdnOnlineRemote.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdnOnlineRemote.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.rdnOnlineRemote.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.rdnOnlineRemote.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdnOnlineRemote.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rdnOnlineRemote.Location = new System.Drawing.Point(225, 74);
            this.rdnOnlineRemote.Name = "rdnOnlineRemote";
            this.rdnOnlineRemote.Size = new System.Drawing.Size(91, 51);
            this.rdnOnlineRemote.TabIndex = 1723;
            this.rdnOnlineRemote.Text = "Online\r\nRemote";
            this.rdnOnlineRemote.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdnOnlineRemote.UseVisualStyleBackColor = false;
            // 
            // btnCtrlChange
            // 
            this.btnCtrlChange.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCtrlChange.FadeOnFocus = true;
            this.btnCtrlChange.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCtrlChange.ForeColor = System.Drawing.Color.Black;
            this.btnCtrlChange.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnCtrlChange.Location = new System.Drawing.Point(128, 143);
            this.btnCtrlChange.Name = "btnCtrlChange";
            this.btnCtrlChange.OuterBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(82)))), ((int)(((byte)(143)))));
            this.btnCtrlChange.ShineColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCtrlChange.Size = new System.Drawing.Size(89, 38);
            this.btnCtrlChange.TabIndex = 1726;
            this.btnCtrlChange.Tag = "10";
            this.btnCtrlChange.Text = "Change";
            this.btnCtrlChange.Click += new System.EventHandler(this.btnCtrlChange_Click);
            // 
            // rdnOnlineLocal
            // 
            this.rdnOnlineLocal.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdnOnlineLocal.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.rdnOnlineLocal.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.rdnOnlineLocal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdnOnlineLocal.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rdnOnlineLocal.Location = new System.Drawing.Point(128, 74);
            this.rdnOnlineLocal.Name = "rdnOnlineLocal";
            this.rdnOnlineLocal.Size = new System.Drawing.Size(91, 51);
            this.rdnOnlineLocal.TabIndex = 1724;
            this.rdnOnlineLocal.Text = "Online\r\nLocal";
            this.rdnOnlineLocal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdnOnlineLocal.UseVisualStyleBackColor = false;
            // 
            // rdnOffline
            // 
            this.rdnOffline.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdnOffline.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.rdnOffline.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.rdnOffline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdnOffline.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rdnOffline.Location = new System.Drawing.Point(32, 74);
            this.rdnOffline.Name = "rdnOffline";
            this.rdnOffline.Size = new System.Drawing.Size(90, 51);
            this.rdnOffline.TabIndex = 1725;
            this.rdnOffline.Text = "Offline";
            this.rdnOffline.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdnOffline.UseVisualStyleBackColor = false;
            // 
            // lbControlStatus
            // 
            this.lbControlStatus.AutoSize = true;
            this.lbControlStatus.BackColor = System.Drawing.Color.Transparent;
            this.lbControlStatus.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbControlStatus.ForeColor = System.Drawing.Color.White;
            this.lbControlStatus.Location = new System.Drawing.Point(161, 26);
            this.lbControlStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbControlStatus.Name = "lbControlStatus";
            this.lbControlStatus.Size = new System.Drawing.Size(58, 21);
            this.lbControlStatus.TabIndex = 1721;
            this.lbControlStatus.Text = "Offline";
            this.lbControlStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(28, 24);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 24);
            this.label4.TabIndex = 1722;
            this.label4.Text = "Control Status : ";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tmrStatus
            // 
            this.tmrStatus.Tick += new System.EventHandler(this.tmrStatus_Tick);
            // 
            // popChangeCtrlStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.ClientSize = new System.Drawing.Size(344, 204);
            this.ControlBox = false;
            this.Controls.Add(this.rdnOnlineRemote);
            this.Controls.Add(this.btnCtrlChange);
            this.Controls.Add(this.rdnOnlineLocal);
            this.Controls.Add(this.rdnOffline);
            this.Controls.Add(this.lbControlStatus);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popChangeCtrlStatus";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "popChangeCtrlStatus";
            this.VisibleChanged += new System.EventHandler(this.popChangeCtrlStatus_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rdnOnlineRemote;
        private Glass.GlassButton btnCtrlChange;
        private System.Windows.Forms.RadioButton rdnOffline;
        private System.Windows.Forms.Label lbControlStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Timer tmrStatus;
        private System.Windows.Forms.RadioButton rdnOnlineLocal;

    }
}