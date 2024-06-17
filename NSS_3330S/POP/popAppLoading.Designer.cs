namespace NSS_3330S.POP
{
    partial class popAppLoading
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
            this.lbLoadingStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panProgressBg = new System.Windows.Forms.Panel();
            this.panProgressBar = new System.Windows.Forms.Panel();
            this.panProgressBg.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbLoadingStatus
            // 
            this.lbLoadingStatus.BackColor = System.Drawing.Color.Transparent;
            this.lbLoadingStatus.ForeColor = System.Drawing.Color.Yellow;
            this.lbLoadingStatus.Location = new System.Drawing.Point(173, 161);
            this.lbLoadingStatus.Name = "lbLoadingStatus";
            this.lbLoadingStatus.Size = new System.Drawing.Size(331, 23);
            this.lbLoadingStatus.TabIndex = 7;
            this.lbLoadingStatus.Text = "GUI Loading && initializing";
            this.lbLoadingStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(382, 54);
            this.label1.TabIndex = 8;
            this.label1.Text = "Application loading";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(15, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(331, 23);
            this.label2.TabIndex = 9;
            this.label2.Text = "Please wait...";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panProgressBg
            // 
            this.panProgressBg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(21)))), ((int)(((byte)(130)))));
            this.panProgressBg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panProgressBg.Controls.Add(this.panProgressBar);
            this.panProgressBg.Location = new System.Drawing.Point(3, 132);
            this.panProgressBg.Name = "panProgressBg";
            this.panProgressBg.Size = new System.Drawing.Size(500, 27);
            this.panProgressBg.TabIndex = 10;
            // 
            // panProgressBar
            // 
            this.panProgressBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(33)))), ((int)(((byte)(192)))));
            this.panProgressBar.Location = new System.Drawing.Point(-1, 1);
            this.panProgressBar.Name = "panProgressBar";
            this.panProgressBar.Size = new System.Drawing.Size(23, 23);
            this.panProgressBar.TabIndex = 11;
            // 
            // popAppLoading
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(506, 210);
            this.Controls.Add(this.panProgressBg);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbLoadingStatus);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popAppLoading";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "popAppLoading";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.popAppLoading_FormClosing);
            this.Load += new System.EventHandler(this.popAppLoading_Load);
            this.panProgressBg.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbLoadingStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panProgressBg;
        private System.Windows.Forms.Panel panProgressBar;
    }
}