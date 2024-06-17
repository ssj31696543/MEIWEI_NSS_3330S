namespace NSS_3330S.POP
{
    partial class popWarnningMsg
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
            this.lbWarnningMsg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbWarnningMsg
            // 
            this.lbWarnningMsg.BackColor = System.Drawing.Color.Transparent;
            this.lbWarnningMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbWarnningMsg.Font = new System.Drawing.Font("맑은 고딕", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbWarnningMsg.ForeColor = System.Drawing.Color.Gold;
            this.lbWarnningMsg.Location = new System.Drawing.Point(0, 0);
            this.lbWarnningMsg.Name = "lbWarnningMsg";
            this.lbWarnningMsg.Size = new System.Drawing.Size(1940, 200);
            this.lbWarnningMsg.TabIndex = 1;
            this.lbWarnningMsg.Text = "Warnning Message !!!";
            this.lbWarnningMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // popWarnningMsg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Red;
            this.ClientSize = new System.Drawing.Size(1940, 200);
            this.Controls.Add(this.lbWarnningMsg);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popWarnningMsg";
            this.Opacity = 0.4D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "popWarnningMsg";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbWarnningMsg;
    }
}