namespace NSS_3330S.UC
{
    partial class ucServoStatus
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.label44 = new System.Windows.Forms.Label();
            this.btnServoOn = new System.Windows.Forms.Button();
            this.panel80 = new System.Windows.Forms.Panel();
            this.ledBusy = new NSS_3330S.UC.LedBulb();
            this.panel88 = new System.Windows.Forms.Panel();
            this.ledInPo = new NSS_3330S.UC.LedBulb();
            this.panel96 = new System.Windows.Forms.Panel();
            this.ledAlarm = new NSS_3330S.UC.LedBulb();
            this.btnServoReset = new System.Windows.Forms.Button();
            this.lblPos = new System.Windows.Forms.Label();
            this.panel56 = new System.Windows.Forms.Panel();
            this.ledFls = new NSS_3330S.UC.LedBulb();
            this.panel72 = new System.Windows.Forms.Panel();
            this.ledHome = new NSS_3330S.UC.LedBulb();
            this.panel64 = new System.Windows.Forms.Panel();
            this.ledRls = new NSS_3330S.UC.LedBulb();
            this.lblAxisName = new System.Windows.Forms.Label();
            this.btnHome = new System.Windows.Forms.Button();
            this.panel80.SuspendLayout();
            this.panel88.SuspendLayout();
            this.panel96.SuspendLayout();
            this.panel56.SuspendLayout();
            this.panel72.SuspendLayout();
            this.panel64.SuspendLayout();
            this.SuspendLayout();
            // 
            // label44
            // 
            this.label44.BackColor = System.Drawing.SystemColors.Control;
            this.label44.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label44.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label44.ForeColor = System.Drawing.Color.White;
            this.label44.Location = new System.Drawing.Point(0, 0);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(276, 35);
            this.label44.TabIndex = 1508;
            this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnServoOn
            // 
            this.btnServoOn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnServoOn.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnServoOn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnServoOn.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnServoOn.ForeColor = System.Drawing.Color.White;
            this.btnServoOn.Location = new System.Drawing.Point(754, 0);
            this.btnServoOn.Name = "btnServoOn";
            this.btnServoOn.Size = new System.Drawing.Size(79, 35);
            this.btnServoOn.TabIndex = 1516;
            this.btnServoOn.Tag = "5";
            this.btnServoOn.Text = "ON";
            this.btnServoOn.UseVisualStyleBackColor = false;
            this.btnServoOn.Click += new System.EventHandler(this.btnServoOn_Click);
            // 
            // panel80
            // 
            this.panel80.BackColor = System.Drawing.SystemColors.Control;
            this.panel80.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel80.Controls.Add(this.ledBusy);
            this.panel80.Location = new System.Drawing.Point(361, 0);
            this.panel80.Name = "panel80";
            this.panel80.Size = new System.Drawing.Size(44, 35);
            this.panel80.TabIndex = 1512;
            // 
            // ledBusy
            // 
            this.ledBusy.DarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.ledBusy.DarkDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledBusy.Location = new System.Drawing.Point(13, 9);
            this.ledBusy.Name = "ledBusy";
            this.ledBusy.On = true;
            this.ledBusy.Size = new System.Drawing.Size(17, 17);
            this.ledBusy.TabIndex = 1475;
            this.ledBusy.Text = "ledBulb6";
            // 
            // panel88
            // 
            this.panel88.BackColor = System.Drawing.SystemColors.Control;
            this.panel88.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel88.Controls.Add(this.ledInPo);
            this.panel88.Location = new System.Drawing.Point(318, 0);
            this.panel88.Name = "panel88";
            this.panel88.Size = new System.Drawing.Size(44, 35);
            this.panel88.TabIndex = 1511;
            // 
            // ledInPo
            // 
            this.ledInPo.DarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.ledInPo.DarkDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledInPo.Location = new System.Drawing.Point(13, 9);
            this.ledInPo.Name = "ledInPo";
            this.ledInPo.On = true;
            this.ledInPo.Size = new System.Drawing.Size(17, 17);
            this.ledInPo.TabIndex = 1475;
            this.ledInPo.Text = "ledBulb8";
            // 
            // panel96
            // 
            this.panel96.BackColor = System.Drawing.SystemColors.Control;
            this.panel96.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel96.Controls.Add(this.ledAlarm);
            this.panel96.Location = new System.Drawing.Point(275, 0);
            this.panel96.Name = "panel96";
            this.panel96.Size = new System.Drawing.Size(44, 35);
            this.panel96.TabIndex = 1510;
            // 
            // ledAlarm
            // 
            this.ledAlarm.Color = System.Drawing.Color.Red;
            this.ledAlarm.DarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledAlarm.DarkDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledAlarm.Location = new System.Drawing.Point(13, 9);
            this.ledAlarm.Name = "ledAlarm";
            this.ledAlarm.On = true;
            this.ledAlarm.Size = new System.Drawing.Size(17, 17);
            this.ledAlarm.TabIndex = 1475;
            this.ledAlarm.Text = "ledBulb10";
            // 
            // btnServoReset
            // 
            this.btnServoReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnServoReset.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnServoReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnServoReset.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnServoReset.ForeColor = System.Drawing.Color.White;
            this.btnServoReset.Location = new System.Drawing.Point(832, 0);
            this.btnServoReset.Name = "btnServoReset";
            this.btnServoReset.Size = new System.Drawing.Size(79, 35);
            this.btnServoReset.TabIndex = 1518;
            this.btnServoReset.Tag = "5";
            this.btnServoReset.Text = "RESET";
            this.btnServoReset.UseVisualStyleBackColor = false;
            this.btnServoReset.Click += new System.EventHandler(this.btnServoReset_Click);
            // 
            // lblPos
            // 
            this.lblPos.BackColor = System.Drawing.SystemColors.Control;
            this.lblPos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPos.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblPos.ForeColor = System.Drawing.Color.Black;
            this.lblPos.Location = new System.Drawing.Point(533, 0);
            this.lblPos.Name = "lblPos";
            this.lblPos.Size = new System.Drawing.Size(222, 35);
            this.lblPos.TabIndex = 1509;
            this.lblPos.Text = "0.000";
            this.lblPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel56
            // 
            this.panel56.BackColor = System.Drawing.SystemColors.Control;
            this.panel56.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel56.Controls.Add(this.ledFls);
            this.panel56.Location = new System.Drawing.Point(490, 0);
            this.panel56.Name = "panel56";
            this.panel56.Size = new System.Drawing.Size(44, 35);
            this.panel56.TabIndex = 1515;
            // 
            // ledFls
            // 
            this.ledFls.Color = System.Drawing.Color.Red;
            this.ledFls.DarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledFls.DarkDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledFls.Location = new System.Drawing.Point(13, 9);
            this.ledFls.Name = "ledFls";
            this.ledFls.On = true;
            this.ledFls.Size = new System.Drawing.Size(17, 17);
            this.ledFls.TabIndex = 1475;
            this.ledFls.Text = "ledBulb1";
            // 
            // panel72
            // 
            this.panel72.BackColor = System.Drawing.SystemColors.Control;
            this.panel72.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel72.Controls.Add(this.ledHome);
            this.panel72.Location = new System.Drawing.Point(404, 0);
            this.panel72.Name = "panel72";
            this.panel72.Size = new System.Drawing.Size(44, 35);
            this.panel72.TabIndex = 1513;
            // 
            // ledHome
            // 
            this.ledHome.DarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.ledHome.DarkDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledHome.Location = new System.Drawing.Point(13, 9);
            this.ledHome.Name = "ledHome";
            this.ledHome.On = true;
            this.ledHome.Size = new System.Drawing.Size(17, 17);
            this.ledHome.TabIndex = 1475;
            this.ledHome.Text = "ledBulb4";
            // 
            // panel64
            // 
            this.panel64.BackColor = System.Drawing.SystemColors.Control;
            this.panel64.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel64.Controls.Add(this.ledRls);
            this.panel64.Location = new System.Drawing.Point(447, 0);
            this.panel64.Name = "panel64";
            this.panel64.Size = new System.Drawing.Size(44, 35);
            this.panel64.TabIndex = 1514;
            // 
            // ledRls
            // 
            this.ledRls.Color = System.Drawing.Color.Red;
            this.ledRls.DarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledRls.DarkDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ledRls.Location = new System.Drawing.Point(13, 9);
            this.ledRls.Name = "ledRls";
            this.ledRls.On = true;
            this.ledRls.Size = new System.Drawing.Size(17, 17);
            this.ledRls.TabIndex = 1475;
            this.ledRls.Text = "ledBulb2";
            // 
            // lblAxisName
            // 
            this.lblAxisName.AutoSize = true;
            this.lblAxisName.BackColor = System.Drawing.SystemColors.Control;
            this.lblAxisName.Font = new System.Drawing.Font("Ebrima", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAxisName.ForeColor = System.Drawing.Color.Black;
            this.lblAxisName.Location = new System.Drawing.Point(21, 9);
            this.lblAxisName.Name = "lblAxisName";
            this.lblAxisName.Size = new System.Drawing.Size(45, 17);
            this.lblAxisName.TabIndex = 1519;
            this.lblAxisName.Text = "label1";
            // 
            // btnHome
            // 
            this.btnHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnHome.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHome.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnHome.ForeColor = System.Drawing.Color.White;
            this.btnHome.Location = new System.Drawing.Point(910, 0);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(79, 35);
            this.btnHome.TabIndex = 1520;
            this.btnHome.Tag = "5";
            this.btnHome.Text = "HOME";
            this.btnHome.UseVisualStyleBackColor = false;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // ucServoStatus
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.btnHome);
            this.Controls.Add(this.lblAxisName);
            this.Controls.Add(this.label44);
            this.Controls.Add(this.btnServoOn);
            this.Controls.Add(this.panel80);
            this.Controls.Add(this.panel88);
            this.Controls.Add(this.panel96);
            this.Controls.Add(this.btnServoReset);
            this.Controls.Add(this.lblPos);
            this.Controls.Add(this.panel56);
            this.Controls.Add(this.panel72);
            this.Controls.Add(this.panel64);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ucServoStatus";
            this.Size = new System.Drawing.Size(989, 35);
            this.Load += new System.EventHandler(this.ucServoStatus_Load);
            this.panel80.ResumeLayout(false);
            this.panel88.ResumeLayout(false);
            this.panel96.ResumeLayout(false);
            this.panel56.ResumeLayout(false);
            this.panel72.ResumeLayout(false);
            this.panel64.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.Button btnServoOn;
        private System.Windows.Forms.Panel panel80;
        private LedBulb ledBusy;
        private System.Windows.Forms.Panel panel88;
        private LedBulb ledInPo;
        private System.Windows.Forms.Panel panel96;
        private LedBulb ledAlarm;
        private System.Windows.Forms.Button btnServoReset;
        private System.Windows.Forms.Label lblPos;
        private System.Windows.Forms.Panel panel56;
        private LedBulb ledFls;
        private System.Windows.Forms.Panel panel72;
        private LedBulb ledHome;
        private System.Windows.Forms.Panel panel64;
        private LedBulb ledRls;
        private System.Windows.Forms.Label lblAxisName;
        private System.Windows.Forms.Button btnHome;
    }
}
