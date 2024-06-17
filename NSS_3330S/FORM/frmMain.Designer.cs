namespace NSS_3330S.FORM
{
    partial class frmMain
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.LogOut_panel = new Owf.Controls.A1Panel();
            this.button_Logout = new System.Windows.Forms.Button();
            this.btnDoorLock = new Glass.GlassButton();
            this.btnExit = new System.Windows.Forms.Button();
            this.dtxTime = new Owf.Controls.DigitalDisplayControl();
            this.a1Panel6 = new Owf.Controls.A1Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dtxDay = new Owf.Controls.DigitalDisplayControl();
            this.a1Panel3 = new Owf.Controls.A1Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnManagRcp = new System.Windows.Forms.Button();
            this.lblRecipeName = new System.Windows.Forms.Label();
            this.lbProjectName = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lbVersion = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panClient = new System.Windows.Forms.Panel();
            this.panToolMenu = new System.Windows.Forms.Panel();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btnToolFunc = new System.Windows.Forms.Button();
            this.btnJog = new System.Windows.Forms.Button();
            this.btnToolLog = new System.Windows.Forms.Button();
            this.btnToolAlarm = new System.Windows.Forms.Button();
            this.btnToolDio = new System.Windows.Forms.Button();
            this.btnToolConfig = new System.Windows.Forms.Button();
            this.btnToolRecipe = new System.Windows.Forms.Button();
            this.btnToolManual = new System.Windows.Forms.Button();
            this.btnToolMain = new System.Windows.Forms.Button();
            this.tmrStatus = new System.Windows.Forms.Timer(this.components);
            this.tmrFlicker = new System.Windows.Forms.Timer(this.components);
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.LogOut_panel.SuspendLayout();
            this.a1Panel6.SuspendLayout();
            this.a1Panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panToolMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.panel1.Controls.Add(this.LogOut_panel);
            this.panel1.Controls.Add(this.btnDoorLock);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Controls.Add(this.dtxTime);
            this.panel1.Controls.Add(this.a1Panel6);
            this.panel1.Controls.Add(this.dtxDay);
            this.panel1.Controls.Add(this.a1Panel3);
            this.panel1.Controls.Add(this.lbProjectName);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1280, 74);
            this.panel1.TabIndex = 1;
            // 
            // LogOut_panel
            // 
            this.LogOut_panel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LogOut_panel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(82)))), ((int)(((byte)(143)))));
            this.LogOut_panel.BorderWidth = 2;
            this.LogOut_panel.Controls.Add(this.button_Logout);
            this.LogOut_panel.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.LogOut_panel.GradientStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.LogOut_panel.Image = null;
            this.LogOut_panel.ImageLocation = new System.Drawing.Point(4, 4);
            this.LogOut_panel.Location = new System.Drawing.Point(142, 0);
            this.LogOut_panel.Margin = new System.Windows.Forms.Padding(2);
            this.LogOut_panel.Name = "LogOut_panel";
            this.LogOut_panel.RoundCornerRadius = 1;
            this.LogOut_panel.ShadowOffSet = 0;
            this.LogOut_panel.Size = new System.Drawing.Size(84, 74);
            this.LogOut_panel.TabIndex = 1723;
            this.LogOut_panel.Visible = false;
            // 
            // button_Logout
            // 
            this.button_Logout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.button_Logout.FlatAppearance.BorderSize = 0;
            this.button_Logout.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.button_Logout.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.button_Logout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Logout.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button_Logout.ForeColor = System.Drawing.Color.OrangeRed;
            this.button_Logout.Image = global::NSS_3330S.Properties.Resources.user_icon_48;
            this.button_Logout.Location = new System.Drawing.Point(3, 0);
            this.button_Logout.Name = "button_Logout";
            this.button_Logout.Size = new System.Drawing.Size(80, 71);
            this.button_Logout.TabIndex = 1723;
            this.button_Logout.Tag = "0";
            this.button_Logout.Text = "LogOut";
            this.button_Logout.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button_Logout.UseVisualStyleBackColor = false;
            this.button_Logout.Visible = false;
            this.button_Logout.Click += new System.EventHandler(this.button_Logout_Click);
            // 
            // btnDoorLock
            // 
            this.btnDoorLock.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDoorLock.FadeOnFocus = true;
            this.btnDoorLock.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDoorLock.ForeColor = System.Drawing.Color.Black;
            this.btnDoorLock.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnDoorLock.Location = new System.Drawing.Point(231, 8);
            this.btnDoorLock.Name = "btnDoorLock";
            this.btnDoorLock.OuterBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(82)))), ((int)(((byte)(143)))));
            this.btnDoorLock.ShineColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDoorLock.Size = new System.Drawing.Size(89, 60);
            this.btnDoorLock.TabIndex = 1722;
            this.btnDoorLock.Tag = "10";
            this.btnDoorLock.Text = "DOOR\r\nLOCK";
            this.btnDoorLock.Click += new System.EventHandler(this.btnDoorLock_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Image = global::NSS_3330S.Properties.Resources.on_off_icon_48;
            this.btnExit.Location = new System.Drawing.Point(1192, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(80, 68);
            this.btnExit.TabIndex = 1056;
            this.btnExit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dtxTime
            // 
            this.dtxTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.dtxTime.DigitColor = System.Drawing.Color.White;
            this.dtxTime.DigitText = "15:20:27";
            this.dtxTime.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dtxTime.ForeColor = System.Drawing.Color.Black;
            this.dtxTime.Location = new System.Drawing.Point(1026, 42);
            this.dtxTime.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dtxTime.Name = "dtxTime";
            this.dtxTime.Size = new System.Drawing.Size(131, 22);
            this.dtxTime.TabIndex = 1058;
            // 
            // a1Panel6
            // 
            this.a1Panel6.BackColor = System.Drawing.SystemColors.ControlDark;
            this.a1Panel6.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(82)))), ((int)(((byte)(143)))));
            this.a1Panel6.BorderWidth = 2;
            this.a1Panel6.Controls.Add(this.label3);
            this.a1Panel6.Controls.Add(this.label5);
            this.a1Panel6.Controls.Add(this.label6);
            this.a1Panel6.Controls.Add(this.label7);
            this.a1Panel6.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.a1Panel6.GradientStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.a1Panel6.Image = null;
            this.a1Panel6.ImageLocation = new System.Drawing.Point(4, 4);
            this.a1Panel6.Location = new System.Drawing.Point(325, 5);
            this.a1Panel6.Margin = new System.Windows.Forms.Padding(2);
            this.a1Panel6.Name = "a1Panel6";
            this.a1Panel6.RoundCornerRadius = 1;
            this.a1Panel6.ShadowOffSet = 0;
            this.a1Panel6.Size = new System.Drawing.Size(228, 65);
            this.a1Panel6.TabIndex = 1056;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(115, 12);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 17);
            this.label3.TabIndex = 939;
            this.label3.Text = "No.1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(13, 12);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(107, 17);
            this.label5.TabIndex = 938;
            this.label5.Text = "MACHINE NO : ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(81, 36);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 17);
            this.label6.TabIndex = 937;
            this.label6.Text = "IDLE";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(13, 36);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 17);
            this.label7.TabIndex = 936;
            this.label7.Text = "STATUS : ";
            // 
            // dtxDay
            // 
            this.dtxDay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.dtxDay.DigitColor = System.Drawing.Color.White;
            this.dtxDay.DigitText = "2021-08-13";
            this.dtxDay.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dtxDay.ForeColor = System.Drawing.Color.Black;
            this.dtxDay.Location = new System.Drawing.Point(1026, 11);
            this.dtxDay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dtxDay.Name = "dtxDay";
            this.dtxDay.Size = new System.Drawing.Size(131, 18);
            this.dtxDay.TabIndex = 1057;
            // 
            // a1Panel3
            // 
            this.a1Panel3.BackColor = System.Drawing.SystemColors.ControlDark;
            this.a1Panel3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(82)))), ((int)(((byte)(143)))));
            this.a1Panel3.BorderWidth = 2;
            this.a1Panel3.Controls.Add(this.label1);
            this.a1Panel3.Controls.Add(this.btnManagRcp);
            this.a1Panel3.Controls.Add(this.lblRecipeName);
            this.a1Panel3.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.a1Panel3.GradientStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.a1Panel3.Image = null;
            this.a1Panel3.ImageLocation = new System.Drawing.Point(4, 4);
            this.a1Panel3.Location = new System.Drawing.Point(556, 5);
            this.a1Panel3.Margin = new System.Windows.Forms.Padding(2);
            this.a1Panel3.Name = "a1Panel3";
            this.a1Panel3.RoundCornerRadius = 1;
            this.a1Panel3.ShadowOffSet = 0;
            this.a1Panel3.Size = new System.Drawing.Size(464, 65);
            this.a1Panel3.TabIndex = 1054;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(2, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(303, 24);
            this.label1.TabIndex = 1052;
            this.label1.Text = "RECIPE NAME : ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnManagRcp
            // 
            this.btnManagRcp.BackColor = System.Drawing.Color.Transparent;
            this.btnManagRcp.FlatAppearance.BorderSize = 0;
            this.btnManagRcp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnManagRcp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnManagRcp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManagRcp.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnManagRcp.ForeColor = System.Drawing.Color.White;
            this.btnManagRcp.Image = ((System.Drawing.Image)(resources.GetObject("btnManagRcp.Image")));
            this.btnManagRcp.Location = new System.Drawing.Point(397, 5);
            this.btnManagRcp.Name = "btnManagRcp";
            this.btnManagRcp.Size = new System.Drawing.Size(63, 55);
            this.btnManagRcp.TabIndex = 1051;
            this.btnManagRcp.Tag = "0";
            this.btnManagRcp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnManagRcp.UseVisualStyleBackColor = false;
            this.btnManagRcp.Visible = false;
            this.btnManagRcp.Click += new System.EventHandler(this.btnManagRcp_Click);
            // 
            // lblRecipeName
            // 
            this.lblRecipeName.BackColor = System.Drawing.Color.Transparent;
            this.lblRecipeName.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblRecipeName.ForeColor = System.Drawing.Color.White;
            this.lblRecipeName.Location = new System.Drawing.Point(2, 30);
            this.lblRecipeName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRecipeName.Name = "lblRecipeName";
            this.lblRecipeName.Size = new System.Drawing.Size(303, 24);
            this.lblRecipeName.TabIndex = 1050;
            this.lblRecipeName.Text = "NOTESTAB EQ-OFFL";
            this.lblRecipeName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbProjectName
            // 
            this.lbProjectName.AutoSize = true;
            this.lbProjectName.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbProjectName.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbProjectName.ForeColor = System.Drawing.Color.White;
            this.lbProjectName.Location = new System.Drawing.Point(9, 19);
            this.lbProjectName.Name = "lbProjectName";
            this.lbProjectName.Size = new System.Drawing.Size(111, 30);
            this.lbProjectName.TabIndex = 0;
            this.lbProjectName.Text = "NSS_3330";
            this.lbProjectName.Click += new System.EventHandler(this.lbProjectName_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.label13);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.lbVersion);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 985);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1280, 20);
            this.panel2.TabIndex = 2;
            // 
            // label12
            // 
            this.label12.BackColor = System.Drawing.Color.Lime;
            this.label12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label12.Location = new System.Drawing.Point(86, 3);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(21, 13);
            this.label12.TabIndex = 4;
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // label13
            // 
            this.label13.BackColor = System.Drawing.Color.Lime;
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label13.Location = new System.Drawing.Point(59, 3);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(21, 13);
            this.label13.TabIndex = 3;
            // 
            // label11
            // 
            this.label11.BackColor = System.Drawing.Color.Lime;
            this.label11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label11.Location = new System.Drawing.Point(32, 3);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(21, 13);
            this.label11.TabIndex = 2;
            // 
            // label10
            // 
            this.label10.BackColor = System.Drawing.Color.Lime;
            this.label10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label10.Location = new System.Drawing.Point(5, 3);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(21, 13);
            this.label10.TabIndex = 1;
            // 
            // lbVersion
            // 
            this.lbVersion.AutoSize = true;
            this.lbVersion.ForeColor = System.Drawing.Color.White;
            this.lbVersion.Location = new System.Drawing.Point(1141, 3);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(124, 15);
            this.lbVersion.TabIndex = 0;
            this.lbVersion.Text = "Copyright ver. 1.1.0.5";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.Control;
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 74);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1280, 911);
            this.panel3.TabIndex = 3;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.panClient);
            this.panel4.Controls.Add(this.panToolMenu);
            this.panel4.Location = new System.Drawing.Point(3, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1274, 924);
            this.panel4.TabIndex = 0;
            // 
            // panClient
            // 
            this.panClient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panClient.Location = new System.Drawing.Point(121, 0);
            this.panClient.Name = "panClient";
            this.panClient.Size = new System.Drawing.Size(1151, 922);
            this.panClient.TabIndex = 8;
            // 
            // panToolMenu
            // 
            this.panToolMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(33)))), ((int)(((byte)(43)))));
            this.panToolMenu.Controls.Add(this.checkBox1);
            this.panToolMenu.Controls.Add(this.btnToolFunc);
            this.panToolMenu.Controls.Add(this.btnJog);
            this.panToolMenu.Controls.Add(this.btnToolLog);
            this.panToolMenu.Controls.Add(this.btnToolAlarm);
            this.panToolMenu.Controls.Add(this.btnToolDio);
            this.panToolMenu.Controls.Add(this.btnToolConfig);
            this.panToolMenu.Controls.Add(this.btnToolRecipe);
            this.panToolMenu.Controls.Add(this.btnToolManual);
            this.panToolMenu.Controls.Add(this.btnToolMain);
            this.panToolMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panToolMenu.Location = new System.Drawing.Point(0, 0);
            this.panToolMenu.Name = "panToolMenu";
            this.panToolMenu.Size = new System.Drawing.Size(121, 922);
            this.panToolMenu.TabIndex = 6;
            this.panToolMenu.Visible = false;
            // 
            // checkBox1
            // 
            this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox1.AutoEllipsis = true;
            this.checkBox1.BackColor = System.Drawing.Color.DimGray;
            this.checkBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.checkBox1.ForeColor = System.Drawing.Color.White;
            this.checkBox1.Location = new System.Drawing.Point(0, 762);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(121, 70);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "Manual Move Reconfirm Popup Release";
            this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.Visible = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // btnToolFunc
            // 
            this.btnToolFunc.BackColor = System.Drawing.Color.Transparent;
            this.btnToolFunc.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnToolFunc.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToolFunc.FlatAppearance.BorderSize = 0;
            this.btnToolFunc.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnToolFunc.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolFunc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolFunc.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToolFunc.ForeColor = System.Drawing.Color.White;
            this.btnToolFunc.Image = ((System.Drawing.Image)(resources.GetObject("btnToolFunc.Image")));
            this.btnToolFunc.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnToolFunc.Location = new System.Drawing.Point(0, 630);
            this.btnToolFunc.Name = "btnToolFunc";
            this.btnToolFunc.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnToolFunc.Size = new System.Drawing.Size(121, 90);
            this.btnToolFunc.TabIndex = 10;
            this.btnToolFunc.Tag = "8";
            this.btnToolFunc.Text = "TEACHING";
            this.btnToolFunc.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnToolFunc.UseVisualStyleBackColor = false;
            this.btnToolFunc.Click += new System.EventHandler(this.OnMainToolButtonClick);
            // 
            // btnJog
            // 
            this.btnJog.BackColor = System.Drawing.Color.Transparent;
            this.btnJog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnJog.FlatAppearance.BorderSize = 0;
            this.btnJog.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnJog.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnJog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnJog.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnJog.ForeColor = System.Drawing.Color.White;
            this.btnJog.Image = global::NSS_3330S.Properties.Resources.game_pad_icon_322;
            this.btnJog.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnJog.Location = new System.Drawing.Point(0, 832);
            this.btnJog.Name = "btnJog";
            this.btnJog.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnJog.Size = new System.Drawing.Size(121, 90);
            this.btnJog.TabIndex = 9;
            this.btnJog.Tag = "7";
            this.btnJog.Text = "JOG";
            this.btnJog.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnJog.UseVisualStyleBackColor = false;
            this.btnJog.Click += new System.EventHandler(this.btnJog_Click);
            // 
            // btnToolLog
            // 
            this.btnToolLog.BackColor = System.Drawing.Color.Transparent;
            this.btnToolLog.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToolLog.FlatAppearance.BorderSize = 0;
            this.btnToolLog.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnToolLog.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolLog.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToolLog.ForeColor = System.Drawing.Color.White;
            this.btnToolLog.Image = global::NSS_3330S.Properties.Resources.doc_edit_icon_32;
            this.btnToolLog.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnToolLog.Location = new System.Drawing.Point(0, 540);
            this.btnToolLog.Name = "btnToolLog";
            this.btnToolLog.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnToolLog.Size = new System.Drawing.Size(121, 90);
            this.btnToolLog.TabIndex = 6;
            this.btnToolLog.Tag = "7";
            this.btnToolLog.Text = "LOG";
            this.btnToolLog.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnToolLog.UseVisualStyleBackColor = false;
            this.btnToolLog.Click += new System.EventHandler(this.OnMainToolButtonClick);
            // 
            // btnToolAlarm
            // 
            this.btnToolAlarm.BackColor = System.Drawing.Color.Transparent;
            this.btnToolAlarm.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToolAlarm.FlatAppearance.BorderSize = 0;
            this.btnToolAlarm.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnToolAlarm.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolAlarm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolAlarm.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToolAlarm.ForeColor = System.Drawing.Color.White;
            this.btnToolAlarm.Image = global::NSS_3330S.Properties.Resources.stop_watch_icon_32;
            this.btnToolAlarm.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnToolAlarm.Location = new System.Drawing.Point(0, 450);
            this.btnToolAlarm.Name = "btnToolAlarm";
            this.btnToolAlarm.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnToolAlarm.Size = new System.Drawing.Size(121, 90);
            this.btnToolAlarm.TabIndex = 5;
            this.btnToolAlarm.Tag = "6";
            this.btnToolAlarm.Text = "ALARM";
            this.btnToolAlarm.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnToolAlarm.UseVisualStyleBackColor = false;
            this.btnToolAlarm.Click += new System.EventHandler(this.OnMainToolButtonClick);
            // 
            // btnToolDio
            // 
            this.btnToolDio.BackColor = System.Drawing.Color.Transparent;
            this.btnToolDio.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToolDio.FlatAppearance.BorderSize = 0;
            this.btnToolDio.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnToolDio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolDio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolDio.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToolDio.ForeColor = System.Drawing.Color.White;
            this.btnToolDio.Image = global::NSS_3330S.Properties.Resources.list_bullets_icon_32;
            this.btnToolDio.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnToolDio.Location = new System.Drawing.Point(0, 360);
            this.btnToolDio.Name = "btnToolDio";
            this.btnToolDio.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnToolDio.Size = new System.Drawing.Size(121, 90);
            this.btnToolDio.TabIndex = 4;
            this.btnToolDio.Tag = "5";
            this.btnToolDio.Text = "DIO";
            this.btnToolDio.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnToolDio.UseVisualStyleBackColor = false;
            this.btnToolDio.Click += new System.EventHandler(this.OnMainToolButtonClick);
            // 
            // btnToolConfig
            // 
            this.btnToolConfig.BackColor = System.Drawing.Color.Transparent;
            this.btnToolConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToolConfig.FlatAppearance.BorderSize = 0;
            this.btnToolConfig.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnToolConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolConfig.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToolConfig.ForeColor = System.Drawing.Color.White;
            this.btnToolConfig.Image = global::NSS_3330S.Properties.Resources.cogs_icon_32;
            this.btnToolConfig.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnToolConfig.Location = new System.Drawing.Point(0, 270);
            this.btnToolConfig.Name = "btnToolConfig";
            this.btnToolConfig.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnToolConfig.Size = new System.Drawing.Size(121, 90);
            this.btnToolConfig.TabIndex = 3;
            this.btnToolConfig.Tag = "4";
            this.btnToolConfig.Text = "CONFIG";
            this.btnToolConfig.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnToolConfig.UseVisualStyleBackColor = false;
            this.btnToolConfig.Click += new System.EventHandler(this.OnMainToolButtonClick);
            // 
            // btnToolRecipe
            // 
            this.btnToolRecipe.BackColor = System.Drawing.Color.Transparent;
            this.btnToolRecipe.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToolRecipe.FlatAppearance.BorderSize = 0;
            this.btnToolRecipe.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnToolRecipe.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolRecipe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolRecipe.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToolRecipe.ForeColor = System.Drawing.Color.White;
            this.btnToolRecipe.Image = global::NSS_3330S.Properties.Resources.clipboard_past_icon_32;
            this.btnToolRecipe.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnToolRecipe.Location = new System.Drawing.Point(0, 180);
            this.btnToolRecipe.Name = "btnToolRecipe";
            this.btnToolRecipe.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnToolRecipe.Size = new System.Drawing.Size(121, 90);
            this.btnToolRecipe.TabIndex = 2;
            this.btnToolRecipe.Tag = "3";
            this.btnToolRecipe.Text = "RECIPE";
            this.btnToolRecipe.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnToolRecipe.UseVisualStyleBackColor = false;
            this.btnToolRecipe.Click += new System.EventHandler(this.OnMainToolButtonClick);
            // 
            // btnToolManual
            // 
            this.btnToolManual.BackColor = System.Drawing.Color.Transparent;
            this.btnToolManual.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToolManual.FlatAppearance.BorderSize = 0;
            this.btnToolManual.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnToolManual.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolManual.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToolManual.ForeColor = System.Drawing.Color.White;
            this.btnToolManual.Image = global::NSS_3330S.Properties.Resources.hand_2_icon_32;
            this.btnToolManual.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnToolManual.Location = new System.Drawing.Point(0, 90);
            this.btnToolManual.Name = "btnToolManual";
            this.btnToolManual.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnToolManual.Size = new System.Drawing.Size(121, 90);
            this.btnToolManual.TabIndex = 1;
            this.btnToolManual.Tag = "2";
            this.btnToolManual.Text = "MANUAL";
            this.btnToolManual.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnToolManual.UseVisualStyleBackColor = false;
            this.btnToolManual.Click += new System.EventHandler(this.OnMainToolButtonClick);
            // 
            // btnToolMain
            // 
            this.btnToolMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToolMain.FlatAppearance.BorderSize = 0;
            this.btnToolMain.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(160)))), ((int)(((byte)(183)))));
            this.btnToolMain.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(97)))), ((int)(((byte)(128)))));
            this.btnToolMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolMain.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToolMain.ForeColor = System.Drawing.Color.White;
            this.btnToolMain.Image = global::NSS_3330S.Properties.Resources.monitor_icon_32;
            this.btnToolMain.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnToolMain.Location = new System.Drawing.Point(0, 0);
            this.btnToolMain.Name = "btnToolMain";
            this.btnToolMain.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.btnToolMain.Size = new System.Drawing.Size(121, 90);
            this.btnToolMain.TabIndex = 0;
            this.btnToolMain.Tag = "1";
            this.btnToolMain.Text = "AUTO";
            this.btnToolMain.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnToolMain.UseVisualStyleBackColor = false;
            this.btnToolMain.Click += new System.EventHandler(this.OnMainToolButtonClick);
            // 
            // tmrStatus
            // 
            this.tmrStatus.Tick += new System.EventHandler(this.tmrStatus_Tick);
            // 
            // tmrFlicker
            // 
            this.tmrFlicker.Interval = 500;
            this.tmrFlicker.Tick += new System.EventHandler(this.tmrFlicker_Tick);
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1280, 1005);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.IsMdiContainer = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NSK8000S";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.LogOut_panel.ResumeLayout(false);
            this.a1Panel6.ResumeLayout(false);
            this.a1Panel6.PerformLayout();
            this.a1Panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panToolMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lbVersion;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panToolMenu;
        private System.Windows.Forms.Button btnToolLog;
        private System.Windows.Forms.Button btnToolAlarm;
        private System.Windows.Forms.Button btnToolDio;
        private System.Windows.Forms.Button btnToolConfig;
        private System.Windows.Forms.Button btnToolRecipe;
        private System.Windows.Forms.Button btnToolManual;
        private System.Windows.Forms.Button btnToolMain;
        private System.Windows.Forms.Button btnExit;
        private Owf.Controls.DigitalDisplayControl dtxTime;
        private Owf.Controls.A1Panel a1Panel6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private Owf.Controls.DigitalDisplayControl dtxDay;
        private Owf.Controls.A1Panel a1Panel3;
        private System.Windows.Forms.Button btnManagRcp;
        private System.Windows.Forms.Label lblRecipeName;
        private System.Windows.Forms.Label lbProjectName;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panClient;
        private System.Windows.Forms.Timer tmrStatus;
        private System.Windows.Forms.Timer tmrFlicker;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Button btnJog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnToolFunc;
        private Glass.GlassButton btnDoorLock;
        private System.Windows.Forms.CheckBox checkBox1;
        private Owf.Controls.A1Panel LogOut_panel;
        private System.Windows.Forms.Button button_Logout;
    }
}

