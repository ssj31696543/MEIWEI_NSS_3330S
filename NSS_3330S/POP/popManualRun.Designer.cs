namespace NSS_3330S.POP
{
    partial class popManualRun
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblElapsedTime = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblManualModuleName = new System.Windows.Forms.Label();
            this.lblModuleNameTitle = new System.Windows.Forms.Label();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblElapsedTime);
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.lblManualModuleName);
            this.panel1.Controls.Add(this.lblModuleNameTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(574, 218);
            this.panel1.TabIndex = 982;
            // 
            // lblElapsedTime
            // 
            this.lblElapsedTime.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblElapsedTime.ForeColor = System.Drawing.Color.White;
            this.lblElapsedTime.Location = new System.Drawing.Point(222, 120);
            this.lblElapsedTime.Name = "lblElapsedTime";
            this.lblElapsedTime.Size = new System.Drawing.Size(150, 33);
            this.lblElapsedTime.TabIndex = 980;
            this.lblElapsedTime.Text = "00:00:00";
            this.lblElapsedTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("맑은 고딕", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(111, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(372, 64);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Manual Running...";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(207, 165);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(180, 37);
            this.btnCancel.TabIndex = 979;
            this.btnCancel.Tag = "2";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // lblManualModuleName
            // 
            this.lblManualModuleName.AutoSize = true;
            this.lblManualModuleName.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblManualModuleName.ForeColor = System.Drawing.Color.White;
            this.lblManualModuleName.Location = new System.Drawing.Point(299, 92);
            this.lblManualModuleName.Name = "lblManualModuleName";
            this.lblManualModuleName.Size = new System.Drawing.Size(130, 21);
            this.lblManualModuleName.TabIndex = 2;
            this.lblManualModuleName.Text = "MANUAL_MOVE";
            this.lblManualModuleName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblModuleNameTitle
            // 
            this.lblModuleNameTitle.AutoSize = true;
            this.lblModuleNameTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblModuleNameTitle.ForeColor = System.Drawing.Color.White;
            this.lblModuleNameTitle.Location = new System.Drawing.Point(170, 92);
            this.lblModuleNameTitle.Name = "lblModuleNameTitle";
            this.lblModuleNameTitle.Size = new System.Drawing.Size(132, 21);
            this.lblModuleNameTitle.TabIndex = 3;
            this.lblModuleNameTitle.Text = "Module Name : ";
            this.lblModuleNameTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 150;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // popManualRun
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.ClientSize = new System.Drawing.Size(574, 218);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popManualRun";
            this.Text = "popManualRun";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.popManualRun_FormClosing);
            this.Load += new System.EventHandler(this.popManualRun_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblElapsedTime;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblManualModuleName;
        private System.Windows.Forms.Label lblModuleNameTitle;
        private System.Windows.Forms.Timer tmrRefresh;
    }
}