namespace NSS_3330S.POP
{
    partial class popNeonJog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(popNeonJog));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lbJogTitleBar = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rdbPicker2 = new System.Windows.Forms.RadioButton();
            this.rdbPicker1 = new System.Windows.Forms.RadioButton();
            this.rdbSorter = new System.Windows.Forms.RadioButton();
            this.rdoSaw = new System.Windows.Forms.RadioButton();
            this.rdoLoader = new System.Windows.Forms.RadioButton();
            this.tabJogMode = new System.Windows.Forms.TabControl();
            this.tabPage22 = new System.Windows.Forms.TabPage();
            this.rbJogSpeedHigh = new System.Windows.Forms.RadioButton();
            this.rbJogSpeedLow = new System.Windows.Forms.RadioButton();
            this.rbJogSpeedMiddle = new System.Windows.Forms.RadioButton();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.lbSpeedChk = new System.Windows.Forms.Label();
            this.tbxIncMove = new System.Windows.Forms.TextBox();
            this.buttonInc1um = new System.Windows.Forms.Button();
            this.buttonInc1000um = new System.Windows.Forms.Button();
            this.buttonInc100um = new System.Windows.Forms.Button();
            this.buttonInc10um = new System.Windows.Forms.Button();
            this.label54 = new System.Windows.Forms.Label();
            this.tabPage16 = new System.Windows.Forms.TabPage();
            this.Chk_Q = new System.Windows.Forms.CheckBox();
            this.rdbVelHigh = new System.Windows.Forms.RadioButton();
            this.rdbVelLow = new System.Windows.Forms.RadioButton();
            this.rdbVelMid = new System.Windows.Forms.RadioButton();
            this.tbxAbsMove = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnMotionMovingStop = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.gridSelect = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.panel3 = new System.Windows.Forms.Panel();
            this.label97 = new System.Windows.Forms.Label();
            this.lbPlus = new System.Windows.Forms.Label();
            this.lbMinus = new System.Windows.Forms.Label();
            this.lbHome = new System.Windows.Forms.Label();
            this.lbBusy = new System.Windows.Forms.Label();
            this.lbHomeComplete = new System.Windows.Forms.Label();
            this.lbError = new System.Windows.Forms.Label();
            this.btnHome = new System.Windows.Forms.Button();
            this.btnServoOn = new System.Windows.Forms.Button();
            this.btnServoReset = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnLatch = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbUNIT_0 = new System.Windows.Forms.Label();
            this.labelExJogPosLatch = new NSS_3330S.UC.LabelExJogPos();
            this.label2 = new System.Windows.Forms.Label();
            this.lbGAP = new System.Windows.Forms.Label();
            this.lbENC = new System.Windows.Forms.Label();
            this.lbCMD = new System.Windows.Forms.Label();
            this.btnJogCCW = new System.Windows.Forms.Button();
            this.btnJogCW = new System.Windows.Forms.Button();
            this.labelExJogPosSpeed = new NSS_3330S.UC.LabelExJogPos();
            this.labelExJogPosCmd = new NSS_3330S.UC.LabelExJogPos();
            this.labelExJogPos = new NSS_3330S.UC.LabelExJogPos();
            this.panel1.SuspendLayout();
            this.tabJogMode.SuspendLayout();
            this.tabPage22.SuspendLayout();
            this.tabPage8.SuspendLayout();
            this.tabPage16.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSelect)).BeginInit();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbJogTitleBar
            // 
            this.lbJogTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.lbJogTitleBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbJogTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbJogTitleBar.Font = new System.Drawing.Font("맑은 고딕", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbJogTitleBar.ForeColor = System.Drawing.Color.OrangeRed;
            this.lbJogTitleBar.Location = new System.Drawing.Point(0, 0);
            this.lbJogTitleBar.Name = "lbJogTitleBar";
            this.lbJogTitleBar.Padding = new System.Windows.Forms.Padding(15, 6, 0, 0);
            this.lbJogTitleBar.Size = new System.Drawing.Size(498, 40);
            this.lbJogTitleBar.TabIndex = 1069;
            this.lbJogTitleBar.Text = "JOG Control";
            this.lbJogTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbJogTitleBar_MouseDown);
            this.lbJogTitleBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbJogTitleBar_MouseMove);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rdbPicker2);
            this.panel1.Controls.Add(this.rdbPicker1);
            this.panel1.Controls.Add(this.rdbSorter);
            this.panel1.Controls.Add(this.rdoSaw);
            this.panel1.Controls.Add(this.rdoLoader);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(211, 390);
            this.panel1.TabIndex = 1073;
            // 
            // rdbPicker2
            // 
            this.rdbPicker2.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbPicker2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.rdbPicker2.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdbPicker2.ForeColor = System.Drawing.Color.Black;
            this.rdbPicker2.Image = ((System.Drawing.Image)(resources.GetObject("rdbPicker2.Image")));
            this.rdbPicker2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rdbPicker2.Location = new System.Drawing.Point(0, 260);
            this.rdbPicker2.Name = "rdbPicker2";
            this.rdbPicker2.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.rdbPicker2.Size = new System.Drawing.Size(211, 65);
            this.rdbPicker2.TabIndex = 1077;
            this.rdbPicker2.Tag = "5";
            this.rdbPicker2.Text = "칩 피커 #2";
            this.rdbPicker2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbPicker2.UseVisualStyleBackColor = true;
            this.rdbPicker2.CheckedChanged += new System.EventHandler(this.rdoLoader_CheckedChanged);
            // 
            // rdbPicker1
            // 
            this.rdbPicker1.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbPicker1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.rdbPicker1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdbPicker1.ForeColor = System.Drawing.Color.Black;
            this.rdbPicker1.Image = ((System.Drawing.Image)(resources.GetObject("rdbPicker1.Image")));
            this.rdbPicker1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rdbPicker1.Location = new System.Drawing.Point(0, 195);
            this.rdbPicker1.Name = "rdbPicker1";
            this.rdbPicker1.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.rdbPicker1.Size = new System.Drawing.Size(211, 65);
            this.rdbPicker1.TabIndex = 1076;
            this.rdbPicker1.Tag = "4";
            this.rdbPicker1.Text = "칩 피커 #1";
            this.rdbPicker1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbPicker1.UseVisualStyleBackColor = true;
            this.rdbPicker1.CheckedChanged += new System.EventHandler(this.rdoLoader_CheckedChanged);
            // 
            // rdbSorter
            // 
            this.rdbSorter.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbSorter.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.rdbSorter.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdbSorter.ForeColor = System.Drawing.Color.Black;
            this.rdbSorter.Image = ((System.Drawing.Image)(resources.GetObject("rdbSorter.Image")));
            this.rdbSorter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rdbSorter.Location = new System.Drawing.Point(0, 130);
            this.rdbSorter.Name = "rdbSorter";
            this.rdbSorter.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.rdbSorter.Size = new System.Drawing.Size(211, 65);
            this.rdbSorter.TabIndex = 1075;
            this.rdbSorter.Tag = "3";
            this.rdbSorter.Text = "소터";
            this.rdbSorter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbSorter.UseVisualStyleBackColor = true;
            this.rdbSorter.CheckedChanged += new System.EventHandler(this.rdoLoader_CheckedChanged);
            // 
            // rdoSaw
            // 
            this.rdoSaw.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoSaw.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.rdoSaw.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdoSaw.ForeColor = System.Drawing.Color.Black;
            this.rdoSaw.Image = ((System.Drawing.Image)(resources.GetObject("rdoSaw.Image")));
            this.rdoSaw.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rdoSaw.Location = new System.Drawing.Point(0, 65);
            this.rdoSaw.Name = "rdoSaw";
            this.rdoSaw.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.rdoSaw.Size = new System.Drawing.Size(211, 65);
            this.rdoSaw.TabIndex = 1;
            this.rdoSaw.Tag = "1";
            this.rdoSaw.Text = "쏘우";
            this.rdoSaw.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoSaw.UseVisualStyleBackColor = true;
            this.rdoSaw.CheckedChanged += new System.EventHandler(this.rdoLoader_CheckedChanged);
            // 
            // rdoLoader
            // 
            this.rdoLoader.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoLoader.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.rdoLoader.Checked = true;
            this.rdoLoader.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdoLoader.ForeColor = System.Drawing.Color.Black;
            this.rdoLoader.Image = ((System.Drawing.Image)(resources.GetObject("rdoLoader.Image")));
            this.rdoLoader.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rdoLoader.Location = new System.Drawing.Point(0, 0);
            this.rdoLoader.Name = "rdoLoader";
            this.rdoLoader.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.rdoLoader.Size = new System.Drawing.Size(211, 65);
            this.rdoLoader.TabIndex = 0;
            this.rdoLoader.TabStop = true;
            this.rdoLoader.Tag = "0";
            this.rdoLoader.Text = "로더";
            this.rdoLoader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoLoader.UseVisualStyleBackColor = true;
            this.rdoLoader.CheckedChanged += new System.EventHandler(this.rdoLoader_CheckedChanged);
            // 
            // tabJogMode
            // 
            this.tabJogMode.Controls.Add(this.tabPage22);
            this.tabJogMode.Controls.Add(this.tabPage8);
            this.tabJogMode.Controls.Add(this.tabPage16);
            this.tabJogMode.ItemSize = new System.Drawing.Size(66, 35);
            this.tabJogMode.Location = new System.Drawing.Point(164, 74);
            this.tabJogMode.Name = "tabJogMode";
            this.tabJogMode.SelectedIndex = 0;
            this.tabJogMode.Size = new System.Drawing.Size(205, 121);
            this.tabJogMode.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabJogMode.TabIndex = 4;
            // 
            // tabPage22
            // 
            this.tabPage22.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabPage22.Controls.Add(this.rbJogSpeedHigh);
            this.tabPage22.Controls.Add(this.rbJogSpeedLow);
            this.tabPage22.Controls.Add(this.rbJogSpeedMiddle);
            this.tabPage22.Location = new System.Drawing.Point(4, 39);
            this.tabPage22.Name = "tabPage22";
            this.tabPage22.Size = new System.Drawing.Size(197, 78);
            this.tabPage22.TabIndex = 2;
            this.tabPage22.Text = "JOG";
            // 
            // rbJogSpeedHigh
            // 
            this.rbJogSpeedHigh.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbJogSpeedHigh.ForeColor = System.Drawing.Color.Black;
            this.rbJogSpeedHigh.Location = new System.Drawing.Point(131, 13);
            this.rbJogSpeedHigh.Name = "rbJogSpeedHigh";
            this.rbJogSpeedHigh.Size = new System.Drawing.Size(64, 55);
            this.rbJogSpeedHigh.TabIndex = 1007;
            this.rbJogSpeedHigh.Tag = "2";
            this.rbJogSpeedHigh.Text = "고속";
            this.rbJogSpeedHigh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbJogSpeedHigh.UseVisualStyleBackColor = true;
            // 
            // rbJogSpeedLow
            // 
            this.rbJogSpeedLow.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbJogSpeedLow.Checked = true;
            this.rbJogSpeedLow.ForeColor = System.Drawing.Color.Black;
            this.rbJogSpeedLow.Location = new System.Drawing.Point(3, 13);
            this.rbJogSpeedLow.Name = "rbJogSpeedLow";
            this.rbJogSpeedLow.Size = new System.Drawing.Size(64, 55);
            this.rbJogSpeedLow.TabIndex = 1005;
            this.rbJogSpeedLow.TabStop = true;
            this.rbJogSpeedLow.Tag = "0";
            this.rbJogSpeedLow.Text = "저속";
            this.rbJogSpeedLow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbJogSpeedLow.UseVisualStyleBackColor = true;
            // 
            // rbJogSpeedMiddle
            // 
            this.rbJogSpeedMiddle.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbJogSpeedMiddle.ForeColor = System.Drawing.Color.Black;
            this.rbJogSpeedMiddle.Location = new System.Drawing.Point(67, 13);
            this.rbJogSpeedMiddle.Name = "rbJogSpeedMiddle";
            this.rbJogSpeedMiddle.Size = new System.Drawing.Size(64, 55);
            this.rbJogSpeedMiddle.TabIndex = 1006;
            this.rbJogSpeedMiddle.Tag = "1";
            this.rbJogSpeedMiddle.Text = "중속";
            this.rbJogSpeedMiddle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbJogSpeedMiddle.UseVisualStyleBackColor = true;
            // 
            // tabPage8
            // 
            this.tabPage8.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage8.Controls.Add(this.lbSpeedChk);
            this.tabPage8.Controls.Add(this.tbxIncMove);
            this.tabPage8.Controls.Add(this.buttonInc1um);
            this.tabPage8.Controls.Add(this.buttonInc1000um);
            this.tabPage8.Controls.Add(this.buttonInc100um);
            this.tabPage8.Controls.Add(this.buttonInc10um);
            this.tabPage8.Controls.Add(this.label54);
            this.tabPage8.Location = new System.Drawing.Point(4, 39);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(197, 78);
            this.tabPage8.TabIndex = 0;
            this.tabPage8.Text = "INC";
            // 
            // lbSpeedChk
            // 
            this.lbSpeedChk.AutoSize = true;
            this.lbSpeedChk.Location = new System.Drawing.Point(143, 15);
            this.lbSpeedChk.Name = "lbSpeedChk";
            this.lbSpeedChk.Size = new System.Drawing.Size(39, 15);
            this.lbSpeedChk.TabIndex = 1084;
            this.lbSpeedChk.Text = "[저속]";
            // 
            // tbxIncMove
            // 
            this.tbxIncMove.Location = new System.Drawing.Point(13, 10);
            this.tbxIncMove.Name = "tbxIncMove";
            this.tbxIncMove.Size = new System.Drawing.Size(72, 23);
            this.tbxIncMove.TabIndex = 1211;
            this.tbxIncMove.Text = "0.1";
            this.tbxIncMove.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonInc1um
            // 
            this.buttonInc1um.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInc1um.ForeColor = System.Drawing.Color.Black;
            this.buttonInc1um.Location = new System.Drawing.Point(4, 42);
            this.buttonInc1um.Name = "buttonInc1um";
            this.buttonInc1um.Size = new System.Drawing.Size(49, 34);
            this.buttonInc1um.TabIndex = 0;
            this.buttonInc1um.Tag = "0";
            this.buttonInc1um.Text = "0.001";
            this.buttonInc1um.UseVisualStyleBackColor = true;
            this.buttonInc1um.Click += new System.EventHandler(this.OnIncValChangeButtonClick);
            // 
            // buttonInc1000um
            // 
            this.buttonInc1000um.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInc1000um.ForeColor = System.Drawing.Color.Black;
            this.buttonInc1000um.Location = new System.Drawing.Point(146, 42);
            this.buttonInc1000um.Name = "buttonInc1000um";
            this.buttonInc1000um.Size = new System.Drawing.Size(46, 34);
            this.buttonInc1000um.TabIndex = 3;
            this.buttonInc1000um.Tag = "3";
            this.buttonInc1000um.Text = "1";
            this.buttonInc1000um.UseVisualStyleBackColor = true;
            this.buttonInc1000um.Click += new System.EventHandler(this.OnIncValChangeButtonClick);
            // 
            // buttonInc100um
            // 
            this.buttonInc100um.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInc100um.ForeColor = System.Drawing.Color.Black;
            this.buttonInc100um.Location = new System.Drawing.Point(100, 42);
            this.buttonInc100um.Name = "buttonInc100um";
            this.buttonInc100um.Size = new System.Drawing.Size(46, 34);
            this.buttonInc100um.TabIndex = 2;
            this.buttonInc100um.Tag = "2";
            this.buttonInc100um.Text = "0.1";
            this.buttonInc100um.UseVisualStyleBackColor = true;
            this.buttonInc100um.Click += new System.EventHandler(this.OnIncValChangeButtonClick);
            // 
            // buttonInc10um
            // 
            this.buttonInc10um.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInc10um.ForeColor = System.Drawing.Color.Black;
            this.buttonInc10um.Location = new System.Drawing.Point(52, 42);
            this.buttonInc10um.Name = "buttonInc10um";
            this.buttonInc10um.Size = new System.Drawing.Size(49, 34);
            this.buttonInc10um.TabIndex = 1;
            this.buttonInc10um.Tag = "1";
            this.buttonInc10um.Text = "0.01";
            this.buttonInc10um.UseVisualStyleBackColor = true;
            this.buttonInc10um.Click += new System.EventHandler(this.OnIncValChangeButtonClick);
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.ForeColor = System.Drawing.Color.Black;
            this.label54.Location = new System.Drawing.Point(91, 15);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(29, 15);
            this.label54.TabIndex = 15;
            this.label54.Text = "mm";
            // 
            // tabPage16
            // 
            this.tabPage16.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage16.Controls.Add(this.Chk_Q);
            this.tabPage16.Controls.Add(this.rdbVelHigh);
            this.tabPage16.Controls.Add(this.rdbVelLow);
            this.tabPage16.Controls.Add(this.rdbVelMid);
            this.tabPage16.Controls.Add(this.tbxAbsMove);
            this.tabPage16.Controls.Add(this.label5);
            this.tabPage16.Location = new System.Drawing.Point(4, 39);
            this.tabPage16.Name = "tabPage16";
            this.tabPage16.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage16.Size = new System.Drawing.Size(197, 78);
            this.tabPage16.TabIndex = 3;
            this.tabPage16.Text = "ABS";
            // 
            // Chk_Q
            // 
            this.Chk_Q.AutoSize = true;
            this.Chk_Q.Checked = true;
            this.Chk_Q.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Chk_Q.Location = new System.Drawing.Point(133, 3);
            this.Chk_Q.Name = "Chk_Q";
            this.Chk_Q.Size = new System.Drawing.Size(61, 19);
            this.Chk_Q.TabIndex = 1011;
            this.Chk_Q.Text = "Chk_Q";
            this.Chk_Q.UseVisualStyleBackColor = true;
            this.Chk_Q.Visible = false;
            // 
            // rdbVelHigh
            // 
            this.rdbVelHigh.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbVelHigh.ForeColor = System.Drawing.Color.Black;
            this.rdbVelHigh.Location = new System.Drawing.Point(130, 40);
            this.rdbVelHigh.Name = "rdbVelHigh";
            this.rdbVelHigh.Size = new System.Drawing.Size(64, 38);
            this.rdbVelHigh.TabIndex = 1010;
            this.rdbVelHigh.Tag = "2";
            this.rdbVelHigh.Text = "고속";
            this.rdbVelHigh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbVelHigh.UseVisualStyleBackColor = true;
            // 
            // rdbVelLow
            // 
            this.rdbVelLow.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbVelLow.Checked = true;
            this.rdbVelLow.ForeColor = System.Drawing.Color.Black;
            this.rdbVelLow.Location = new System.Drawing.Point(2, 40);
            this.rdbVelLow.Name = "rdbVelLow";
            this.rdbVelLow.Size = new System.Drawing.Size(64, 38);
            this.rdbVelLow.TabIndex = 1008;
            this.rdbVelLow.TabStop = true;
            this.rdbVelLow.Tag = "0";
            this.rdbVelLow.Text = "저속";
            this.rdbVelLow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbVelLow.UseVisualStyleBackColor = true;
            // 
            // rdbVelMid
            // 
            this.rdbVelMid.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbVelMid.ForeColor = System.Drawing.Color.Black;
            this.rdbVelMid.Location = new System.Drawing.Point(66, 40);
            this.rdbVelMid.Name = "rdbVelMid";
            this.rdbVelMid.Size = new System.Drawing.Size(64, 38);
            this.rdbVelMid.TabIndex = 1009;
            this.rdbVelMid.Tag = "1";
            this.rdbVelMid.Text = "중속";
            this.rdbVelMid.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbVelMid.UseVisualStyleBackColor = true;
            // 
            // tbxAbsMove
            // 
            this.tbxAbsMove.Location = new System.Drawing.Point(13, 10);
            this.tbxAbsMove.Name = "tbxAbsMove";
            this.tbxAbsMove.Size = new System.Drawing.Size(72, 23);
            this.tbxAbsMove.TabIndex = 20;
            this.tbxAbsMove.Text = "0.1";
            this.tbxAbsMove.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(91, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 15);
            this.label5.TabIndex = 17;
            this.label5.Text = "mm";
            // 
            // btnMotionMovingStop
            // 
            this.btnMotionMovingStop.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnMotionMovingStop.Image = global::NSS_3330S.Properties.Resources.round_minus_icon_24;
            this.btnMotionMovingStop.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMotionMovingStop.Location = new System.Drawing.Point(4, 144);
            this.btnMotionMovingStop.Name = "btnMotionMovingStop";
            this.btnMotionMovingStop.Padding = new System.Windows.Forms.Padding(25, 0, 0, 0);
            this.btnMotionMovingStop.Size = new System.Drawing.Size(160, 48);
            this.btnMotionMovingStop.TabIndex = 1074;
            this.btnMotionMovingStop.Text = "이동 정지";
            this.btnMotionMovingStop.UseVisualStyleBackColor = true;
            this.btnMotionMovingStop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnMotionMovingStop_MouseDown);
            this.btnMotionMovingStop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnMotionMovingStop_MouseUp);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(47)))), ((int)(((byte)(62)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(452, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(36, 28);
            this.btnClose.TabIndex = 1076;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.gridSelect);
            this.panel2.Location = new System.Drawing.Point(212, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(286, 390);
            this.panel2.TabIndex = 1078;
            // 
            // gridSelect
            // 
            this.gridSelect.AllowUserToAddRows = false;
            this.gridSelect.AllowUserToDeleteRows = false;
            this.gridSelect.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridSelect.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridSelect.ColumnHeadersHeight = 30;
            this.gridSelect.ColumnHeadersVisible = false;
            this.gridSelect.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            this.gridSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSelect.GridColor = System.Drawing.Color.Silver;
            this.gridSelect.Location = new System.Drawing.Point(0, 0);
            this.gridSelect.Name = "gridSelect";
            this.gridSelect.ReadOnly = true;
            this.gridSelect.RowHeadersVisible = false;
            this.gridSelect.RowHeadersWidth = 35;
            this.gridSelect.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            this.gridSelect.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.gridSelect.RowTemplate.Height = 23;
            this.gridSelect.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.gridSelect.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridSelect.Size = new System.Drawing.Size(284, 388);
            this.gridSelect.TabIndex = 827;
            this.gridSelect.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridSelect_CellClick);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "MOTOR";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 350;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 50;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.Control;
            this.panel3.Controls.Add(this.panel1);
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Location = new System.Drawing.Point(0, 241);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(498, 397);
            this.panel3.TabIndex = 1079;
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.ForeColor = System.Drawing.Color.Orange;
            this.label97.Location = new System.Drawing.Point(0, 652);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(66, 15);
            this.label97.TabIndex = 1080;
            this.label97.Text = "Message ...";
            // 
            // lbPlus
            // 
            this.lbPlus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbPlus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbPlus.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbPlus.ForeColor = System.Drawing.Color.White;
            this.lbPlus.Location = new System.Drawing.Point(209, 2);
            this.lbPlus.Name = "lbPlus";
            this.lbPlus.Size = new System.Drawing.Size(31, 31);
            this.lbPlus.TabIndex = 1104;
            this.lbPlus.Text = "L +";
            this.lbPlus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMinus
            // 
            this.lbMinus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbMinus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMinus.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbMinus.ForeColor = System.Drawing.Color.White;
            this.lbMinus.Location = new System.Drawing.Point(168, 2);
            this.lbMinus.Name = "lbMinus";
            this.lbMinus.Size = new System.Drawing.Size(31, 31);
            this.lbMinus.TabIndex = 1103;
            this.lbMinus.Text = "L -";
            this.lbMinus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbHome
            // 
            this.lbHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lbHome.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbHome.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbHome.ForeColor = System.Drawing.Color.White;
            this.lbHome.Location = new System.Drawing.Point(127, 2);
            this.lbHome.Name = "lbHome";
            this.lbHome.Size = new System.Drawing.Size(31, 31);
            this.lbHome.TabIndex = 1102;
            this.lbHome.Text = "H";
            this.lbHome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbBusy
            // 
            this.lbBusy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lbBusy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbBusy.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbBusy.ForeColor = System.Drawing.Color.White;
            this.lbBusy.Location = new System.Drawing.Point(86, 2);
            this.lbBusy.Name = "lbBusy";
            this.lbBusy.Size = new System.Drawing.Size(31, 31);
            this.lbBusy.TabIndex = 1101;
            this.lbBusy.Text = "B";
            this.lbBusy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbHomeComplete
            // 
            this.lbHomeComplete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lbHomeComplete.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbHomeComplete.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbHomeComplete.ForeColor = System.Drawing.Color.White;
            this.lbHomeComplete.Location = new System.Drawing.Point(45, 2);
            this.lbHomeComplete.Name = "lbHomeComplete";
            this.lbHomeComplete.Size = new System.Drawing.Size(31, 31);
            this.lbHomeComplete.TabIndex = 1100;
            this.lbHomeComplete.Text = "HC";
            this.lbHomeComplete.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbError
            // 
            this.lbError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbError.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbError.ForeColor = System.Drawing.Color.White;
            this.lbError.Location = new System.Drawing.Point(4, 2);
            this.lbError.Name = "lbError";
            this.lbError.Size = new System.Drawing.Size(31, 31);
            this.lbError.TabIndex = 1099;
            this.lbError.Text = "E";
            this.lbError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnHome
            // 
            this.btnHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnHome.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHome.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnHome.ForeColor = System.Drawing.Color.White;
            this.btnHome.Location = new System.Drawing.Point(161, 36);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(79, 35);
            this.btnHome.TabIndex = 1523;
            this.btnHome.Tag = "5";
            this.btnHome.Text = "HOME";
            this.btnHome.UseVisualStyleBackColor = false;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // btnServoOn
            // 
            this.btnServoOn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnServoOn.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnServoOn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnServoOn.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnServoOn.ForeColor = System.Drawing.Color.White;
            this.btnServoOn.Location = new System.Drawing.Point(5, 36);
            this.btnServoOn.Name = "btnServoOn";
            this.btnServoOn.Size = new System.Drawing.Size(79, 35);
            this.btnServoOn.TabIndex = 1521;
            this.btnServoOn.Tag = "5";
            this.btnServoOn.Text = "SERVO ON";
            this.btnServoOn.UseVisualStyleBackColor = false;
            this.btnServoOn.Click += new System.EventHandler(this.btnServoOn_Click);
            // 
            // btnServoReset
            // 
            this.btnServoReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnServoReset.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnServoReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnServoReset.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnServoReset.ForeColor = System.Drawing.Color.White;
            this.btnServoReset.Location = new System.Drawing.Point(83, 36);
            this.btnServoReset.Name = "btnServoReset";
            this.btnServoReset.Size = new System.Drawing.Size(79, 35);
            this.btnServoReset.TabIndex = 1522;
            this.btnServoReset.Tag = "5";
            this.btnServoReset.Text = "RESET";
            this.btnServoReset.UseVisualStyleBackColor = false;
            this.btnServoReset.Click += new System.EventHandler(this.btnServoReset_Click);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.SystemColors.Control;
            this.panel4.Controls.Add(this.btnLatch);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Controls.Add(this.lbUNIT_0);
            this.panel4.Controls.Add(this.labelExJogPosLatch);
            this.panel4.Controls.Add(this.label2);
            this.panel4.Controls.Add(this.lbGAP);
            this.panel4.Controls.Add(this.lbENC);
            this.panel4.Controls.Add(this.lbCMD);
            this.panel4.Controls.Add(this.btnHome);
            this.panel4.Controls.Add(this.lbError);
            this.panel4.Controls.Add(this.lbHomeComplete);
            this.panel4.Controls.Add(this.tabJogMode);
            this.panel4.Controls.Add(this.lbBusy);
            this.panel4.Controls.Add(this.btnJogCCW);
            this.panel4.Controls.Add(this.btnServoOn);
            this.panel4.Controls.Add(this.lbHome);
            this.panel4.Controls.Add(this.btnJogCW);
            this.panel4.Controls.Add(this.btnServoReset);
            this.panel4.Controls.Add(this.btnMotionMovingStop);
            this.panel4.Controls.Add(this.labelExJogPosSpeed);
            this.panel4.Controls.Add(this.labelExJogPosCmd);
            this.panel4.Controls.Add(this.labelExJogPos);
            this.panel4.Controls.Add(this.lbPlus);
            this.panel4.Controls.Add(this.lbMinus);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 40);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(498, 204);
            this.panel4.TabIndex = 1081;
            // 
            // btnLatch
            // 
            this.btnLatch.BackColor = System.Drawing.Color.LightGray;
            this.btnLatch.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnLatch.Location = new System.Drawing.Point(425, 3);
            this.btnLatch.Name = "btnLatch";
            this.btnLatch.Size = new System.Drawing.Size(63, 68);
            this.btnLatch.TabIndex = 1079;
            this.btnLatch.Text = "LATCH";
            this.btnLatch.UseVisualStyleBackColor = false;
            this.btnLatch.Click += new System.EventHandler(this.btnLatch_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(456, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 1532;
            this.label4.Text = "mm";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(456, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 13);
            this.label3.TabIndex = 1531;
            this.label3.Text = "mm";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(398, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 1530;
            this.label1.Text = "mm";
            // 
            // lbUNIT_0
            // 
            this.lbUNIT_0.AutoSize = true;
            this.lbUNIT_0.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbUNIT_0.Location = new System.Drawing.Point(309, 40);
            this.lbUNIT_0.Name = "lbUNIT_0";
            this.lbUNIT_0.Size = new System.Drawing.Size(23, 13);
            this.lbUNIT_0.TabIndex = 1529;
            this.lbUNIT_0.Text = "mm";
            // 
            // labelExJogPosLatch
            // 
            this.labelExJogPosLatch.AXIS = NSS_3330S.SVDF.AXES.MAGAZINE_ELV_Z;
            this.labelExJogPosLatch.BackColor = System.Drawing.SystemColors.Control;
            this.labelExJogPosLatch.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelExJogPosLatch.ForeColor = System.Drawing.Color.Blue;
            this.labelExJogPosLatch.Location = new System.Drawing.Point(332, 33);
            this.labelExJogPosLatch.Name = "labelExJogPosLatch";
            this.labelExJogPosLatch.Size = new System.Drawing.Size(72, 27);
            this.labelExJogPosLatch.TabIndex = 1528;
            this.labelExJogPosLatch.Text = "000.0000";
            this.labelExJogPosLatch.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(422, 141);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 1527;
            this.label2.Text = "SPD";
            // 
            // lbGAP
            // 
            this.lbGAP.AutoSize = true;
            this.lbGAP.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbGAP.Location = new System.Drawing.Point(357, 20);
            this.lbGAP.Name = "lbGAP";
            this.lbGAP.Size = new System.Drawing.Size(39, 13);
            this.lbGAP.TabIndex = 1526;
            this.lbGAP.Text = "LATCH";
            // 
            // lbENC
            // 
            this.lbENC.AutoSize = true;
            this.lbENC.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbENC.Location = new System.Drawing.Point(272, 20);
            this.lbENC.Name = "lbENC";
            this.lbENC.Size = new System.Drawing.Size(27, 13);
            this.lbENC.TabIndex = 1525;
            this.lbENC.Text = "ENC";
            // 
            // lbCMD
            // 
            this.lbCMD.AutoSize = true;
            this.lbCMD.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbCMD.Location = new System.Drawing.Point(422, 87);
            this.lbCMD.Name = "lbCMD";
            this.lbCMD.Size = new System.Drawing.Size(29, 13);
            this.lbCMD.TabIndex = 1524;
            this.lbCMD.Text = "CMD";
            // 
            // btnJogCCW
            // 
            this.btnJogCCW.BackColor = System.Drawing.Color.White;
            this.btnJogCCW.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnJogCCW.ForeColor = System.Drawing.Color.Black;
            this.btnJogCCW.Image = ((System.Drawing.Image)(resources.GetObject("btnJogCCW.Image")));
            this.btnJogCCW.Location = new System.Drawing.Point(4, 83);
            this.btnJogCCW.Name = "btnJogCCW";
            this.btnJogCCW.Size = new System.Drawing.Size(80, 55);
            this.btnJogCCW.TabIndex = 868;
            this.btnJogCCW.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnJogCCW.UseVisualStyleBackColor = false;
            this.btnJogCCW.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnJogCCW_MouseDown);
            this.btnJogCCW.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnJogCCW_MouseUp);
            // 
            // btnJogCW
            // 
            this.btnJogCW.BackColor = System.Drawing.Color.White;
            this.btnJogCW.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnJogCW.ForeColor = System.Drawing.Color.Black;
            this.btnJogCW.Image = ((System.Drawing.Image)(resources.GetObject("btnJogCW.Image")));
            this.btnJogCW.Location = new System.Drawing.Point(84, 83);
            this.btnJogCW.Name = "btnJogCW";
            this.btnJogCW.Size = new System.Drawing.Size(80, 55);
            this.btnJogCW.TabIndex = 867;
            this.btnJogCW.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnJogCW.UseVisualStyleBackColor = false;
            this.btnJogCW.Click += new System.EventHandler(this.btnJogCW_Click);
            this.btnJogCW.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnJogCW_MouseDown);
            this.btnJogCW.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnJogCW_MouseUp);
            // 
            // labelExJogPosSpeed
            // 
            this.labelExJogPosSpeed.AXIS = NSS_3330S.SVDF.AXES.LD_RAIL_T;
            this.labelExJogPosSpeed.BackColor = System.Drawing.SystemColors.Control;
            this.labelExJogPosSpeed.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelExJogPosSpeed.Location = new System.Drawing.Point(387, 154);
            this.labelExJogPosSpeed.Name = "labelExJogPosSpeed";
            this.labelExJogPosSpeed.Size = new System.Drawing.Size(72, 27);
            this.labelExJogPosSpeed.TabIndex = 1098;
            this.labelExJogPosSpeed.Text = "000.0000";
            this.labelExJogPosSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelExJogPosCmd
            // 
            this.labelExJogPosCmd.AXIS = NSS_3330S.SVDF.AXES.LD_RAIL_T;
            this.labelExJogPosCmd.BackColor = System.Drawing.SystemColors.Control;
            this.labelExJogPosCmd.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelExJogPosCmd.Location = new System.Drawing.Point(387, 100);
            this.labelExJogPosCmd.Name = "labelExJogPosCmd";
            this.labelExJogPosCmd.Size = new System.Drawing.Size(72, 27);
            this.labelExJogPosCmd.TabIndex = 1098;
            this.labelExJogPosCmd.Text = "000.0000";
            this.labelExJogPosCmd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelExJogPos
            // 
            this.labelExJogPos.AXIS = NSS_3330S.SVDF.AXES.LD_RAIL_T;
            this.labelExJogPos.BackColor = System.Drawing.SystemColors.Control;
            this.labelExJogPos.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelExJogPos.Location = new System.Drawing.Point(243, 33);
            this.labelExJogPos.Name = "labelExJogPos";
            this.labelExJogPos.Size = new System.Drawing.Size(72, 27);
            this.labelExJogPos.TabIndex = 1098;
            this.labelExJogPos.Text = "000.0000";
            this.labelExJogPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // popNeonJog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(33)))), ((int)(((byte)(43)))));
            this.ClientSize = new System.Drawing.Size(498, 672);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.label97);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lbJogTitleBar);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "popNeonJog";
            this.Text = "popNeonJog";
            this.Load += new System.EventHandler(this.popNeonJog_Load);
            this.VisibleChanged += new System.EventHandler(this.popNeonJog_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.tabJogMode.ResumeLayout(false);
            this.tabPage22.ResumeLayout(false);
            this.tabPage8.ResumeLayout(false);
            this.tabPage8.PerformLayout();
            this.tabPage16.ResumeLayout(false);
            this.tabPage16.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridSelect)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbJogTitleBar;
        private System.Windows.Forms.RadioButton rdoLoader;
        private System.Windows.Forms.RadioButton rdoSaw;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnMotionMovingStop;
        private System.Windows.Forms.TabControl tabJogMode;
        private System.Windows.Forms.TabPage tabPage22;
        private System.Windows.Forms.RadioButton rbJogSpeedHigh;
        private System.Windows.Forms.RadioButton rbJogSpeedLow;
        private System.Windows.Forms.RadioButton rbJogSpeedMiddle;
        private System.Windows.Forms.TabPage tabPage8;
        private System.Windows.Forms.TextBox tbxIncMove;
        private System.Windows.Forms.Button buttonInc1um;
        private System.Windows.Forms.Button buttonInc1000um;
        private System.Windows.Forms.Button buttonInc100um;
        private System.Windows.Forms.Button buttonInc10um;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.TabPage tabPage16;
        private System.Windows.Forms.TextBox tbxAbsMove;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.RadioButton rdbVelHigh;
        private System.Windows.Forms.RadioButton rdbVelLow;
        private System.Windows.Forms.RadioButton rdbVelMid;
        private System.Windows.Forms.Label lbSpeedChk;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label97;
        private System.Windows.Forms.RadioButton rdbSorter;
        private UC.LabelExJogPos labelExJogPos;
        private System.Windows.Forms.CheckBox Chk_Q;
        internal System.Windows.Forms.DataGridView gridSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.Label lbPlus;
        private System.Windows.Forms.Label lbMinus;
        private System.Windows.Forms.Label lbHome;
        private System.Windows.Forms.Label lbBusy;
        private System.Windows.Forms.Label lbHomeComplete;
        private System.Windows.Forms.Label lbError;
        private System.Windows.Forms.Button btnHome;
        private System.Windows.Forms.Button btnServoOn;
        private System.Windows.Forms.Button btnServoReset;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbGAP;
        private System.Windows.Forms.Label lbENC;
        private System.Windows.Forms.Label lbCMD;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbUNIT_0;
        private UC.LabelExJogPos labelExJogPosLatch;
        private UC.LabelExJogPos labelExJogPosSpeed;
        private UC.LabelExJogPos labelExJogPosCmd;
        private System.Windows.Forms.Button btnLatch;
        private System.Windows.Forms.RadioButton rdbPicker2;
        private System.Windows.Forms.RadioButton rdbPicker1;
        private System.Windows.Forms.Button btnJogCCW;
        private System.Windows.Forms.Button btnJogCW;
    }
}