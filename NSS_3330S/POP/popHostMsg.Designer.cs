namespace NSS_3330S
{
    partial class popHostMsg
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
            this.btnClose = new System.Windows.Forms.Button();
            this.lbMsg = new System.Windows.Forms.Label();
            this.lblRcmdMsg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(342, 214);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(197, 50);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "CLOSE";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lbMsg
            // 
            this.lbMsg.BackColor = System.Drawing.Color.White;
            this.lbMsg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMsg.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbMsg.Location = new System.Drawing.Point(12, 55);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(881, 141);
            this.lbMsg.TabIndex = 2;
            this.lbMsg.Text = "Message";
            this.lbMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRcmdMsg
            // 
            this.lblRcmdMsg.BackColor = System.Drawing.Color.OrangeRed;
            this.lblRcmdMsg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblRcmdMsg.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblRcmdMsg.ForeColor = System.Drawing.Color.Yellow;
            this.lblRcmdMsg.Location = new System.Drawing.Point(0, 0);
            this.lblRcmdMsg.Name = "lblRcmdMsg";
            this.lblRcmdMsg.Size = new System.Drawing.Size(905, 44);
            this.lblRcmdMsg.TabIndex = 12;
            this.lblRcmdMsg.Text = "HOST ERROR MESSAGE";
            this.lblRcmdMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // popHostMsg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(905, 276);
            this.ControlBox = false;
            this.Controls.Add(this.lbMsg);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblRcmdMsg);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "popHostMsg";
            this.Text = "MESSAGE";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Label lblRcmdMsg;
    }
}