namespace NSS_3330S
{
    partial class popTerminalMsg
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
            this.lbTerminalMsg = new System.Windows.Forms.Label();
            this.lsvMsg = new System.Windows.Forms.ListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(337, 298);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(197, 65);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "CLOSE";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lbTerminalMsg
            // 
            this.lbTerminalMsg.BackColor = System.Drawing.Color.Gold;
            this.lbTerminalMsg.Font = new System.Drawing.Font("경기천년제목 Bold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbTerminalMsg.Location = new System.Drawing.Point(0, 0);
            this.lbTerminalMsg.Name = "lbTerminalMsg";
            this.lbTerminalMsg.Size = new System.Drawing.Size(905, 63);
            this.lbTerminalMsg.TabIndex = 1;
            this.lbTerminalMsg.Text = "TREMINAL MESSAGE";
            this.lbTerminalMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lsvMsg
            // 
            this.lsvMsg.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.lsvMsg.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lsvMsg.BackColor = System.Drawing.Color.Gainsboro;
            this.lsvMsg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lsvMsg.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6,
            this.columnHeader7});
            this.lsvMsg.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsvMsg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.lsvMsg.FullRowSelect = true;
            this.lsvMsg.GridLines = true;
            this.lsvMsg.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lsvMsg.Location = new System.Drawing.Point(12, 76);
            this.lsvMsg.MultiSelect = false;
            this.lsvMsg.Name = "lsvMsg";
            this.lsvMsg.Size = new System.Drawing.Size(881, 213);
            this.lsvMsg.TabIndex = 1235;
            this.lsvMsg.UseCompatibleStateImageBehavior = false;
            this.lsvMsg.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "TIME";
            this.columnHeader6.Width = 200;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "MESSAGE";
            this.columnHeader7.Width = 700;
            // 
            // popTerminalMsg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(905, 372);
            this.ControlBox = false;
            this.Controls.Add(this.lsvMsg);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lbTerminalMsg);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "popTerminalMsg";
            this.Text = "TREMINAL MESSAGE";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lbTerminalMsg;
        private System.Windows.Forms.ListView lsvMsg;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
    }
}