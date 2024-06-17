namespace NSS_3330S.UC
{
    partial class ucSvManualJog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucSvManualJog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabJog = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnExJogXn = new NSS_3330S.UC.ButtonExJog();
            this.btnExJogXp = new NSS_3330S.UC.ButtonExJog();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnExJogYn = new NSS_3330S.UC.ButtonExJog();
            this.btnExJogYp = new NSS_3330S.UC.ButtonExJog();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnExJogZn = new NSS_3330S.UC.ButtonExJog();
            this.btnExJogZp = new NSS_3330S.UC.ButtonExJog();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnExJogCCW = new NSS_3330S.UC.ButtonExJog();
            this.btnExJogCW = new NSS_3330S.UC.ButtonExJog();
            this.label9 = new System.Windows.Forms.Label();
            this.lbCurPos = new NSS_3330S.UC.LabelExJogPos();
            this.tabJogMode = new System.Windows.Forms.TabControl();
            this.tabPage22 = new System.Windows.Forms.TabPage();
            this.rbJogSpeedHigh = new System.Windows.Forms.RadioButton();
            this.rbJogSpeedLow = new System.Windows.Forms.RadioButton();
            this.rbJogSpeedMiddle = new System.Windows.Forms.RadioButton();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.tbxIncMove = new System.Windows.Forms.TextBox();
            this.buttonInc1um = new System.Windows.Forms.Button();
            this.buttonInc1000um = new System.Windows.Forms.Button();
            this.buttonInc100um = new System.Windows.Forms.Button();
            this.buttonInc10um = new System.Windows.Forms.Button();
            this.label54 = new System.Windows.Forms.Label();
            this.tabPage16 = new System.Windows.Forms.TabPage();
            this.tbxAbsMove = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lbError = new System.Windows.Forms.Label();
            this.lbHomeComplete = new System.Windows.Forms.Label();
            this.lbHome = new System.Windows.Forms.Label();
            this.lbBusy = new System.Windows.Forms.Label();
            this.lbPlus = new System.Windows.Forms.Label();
            this.lbMinus = new System.Windows.Forms.Label();
            this.cbPadNo = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.lbIncSpeed = new System.Windows.Forms.Label();
            this.rdbAbsVelHigh = new System.Windows.Forms.RadioButton();
            this.rdbAbsVelLow = new System.Windows.Forms.RadioButton();
            this.rdbAbsVelMid = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.tabJog.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabJogMode.SuspendLayout();
            this.tabPage22.SuspendLayout();
            this.tabPage8.SuspendLayout();
            this.tabPage16.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabJog);
            this.panel1.Location = new System.Drawing.Point(219, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(261, 182);
            this.panel1.TabIndex = 1064;
            // 
            // tabJog
            // 
            this.tabJog.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabJog.Controls.Add(this.tabPage1);
            this.tabJog.Controls.Add(this.tabPage2);
            this.tabJog.Controls.Add(this.tabPage3);
            this.tabJog.Controls.Add(this.tabPage4);
            this.tabJog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabJog.ItemSize = new System.Drawing.Size(50, 12);
            this.tabJog.Location = new System.Drawing.Point(0, 0);
            this.tabJog.Name = "tabJog";
            this.tabJog.SelectedIndex = 0;
            this.tabJog.Size = new System.Drawing.Size(261, 182);
            this.tabJog.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabJog.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnExJogXn);
            this.tabPage1.Controls.Add(this.btnExJogXp);
            this.tabPage1.Location = new System.Drawing.Point(4, 16);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(253, 162);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "X";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnExJogXn
            // 
            this.btnExJogXn.AUTO_TEXT_ALIGN = true;
            this.btnExJogXn.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.btnExJogXn.BackColor = System.Drawing.SystemColors.Control;
            this.btnExJogXn.DIR = NSS_3330S.UC.ButtonExJog.JOG_DIR.X;
            this.btnExJogXn.FlatAppearance.BorderColor = System.Drawing.Color.Chocolate;
            this.btnExJogXn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExJogXn.ForeColor = System.Drawing.Color.Black;
            this.btnExJogXn.Image = ((System.Drawing.Image)(resources.GetObject("btnExJogXn.Image")));
            this.btnExJogXn.JOG_DIR_REVERSE = true;
            this.btnExJogXn.Location = new System.Drawing.Point(53, 63);
            this.btnExJogXn.MOTOR_DIR_REVERSE = true;
            this.btnExJogXn.Name = "btnExJogXn";
            this.btnExJogXn.Size = new System.Drawing.Size(72, 67);
            this.btnExJogXn.TabIndex = 1087;
            this.btnExJogXn.Text = "─";
            this.btnExJogXn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExJogXn.UseVisualStyleBackColor = false;
            // 
            // btnExJogXp
            // 
            this.btnExJogXp.AUTO_TEXT_ALIGN = true;
            this.btnExJogXp.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.btnExJogXp.BackColor = System.Drawing.SystemColors.Control;
            this.btnExJogXp.DIR = NSS_3330S.UC.ButtonExJog.JOG_DIR.X;
            this.btnExJogXp.FlatAppearance.BorderColor = System.Drawing.Color.Chocolate;
            this.btnExJogXp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExJogXp.ForeColor = System.Drawing.Color.Black;
            this.btnExJogXp.Image = ((System.Drawing.Image)(resources.GetObject("btnExJogXp.Image")));
            this.btnExJogXp.JOG_DIR_REVERSE = false;
            this.btnExJogXp.Location = new System.Drawing.Point(131, 63);
            this.btnExJogXp.MOTOR_DIR_REVERSE = false;
            this.btnExJogXp.Name = "btnExJogXp";
            this.btnExJogXp.Size = new System.Drawing.Size(72, 67);
            this.btnExJogXp.TabIndex = 1088;
            this.btnExJogXp.Text = "┼";
            this.btnExJogXp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExJogXp.UseVisualStyleBackColor = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnExJogYn);
            this.tabPage2.Controls.Add(this.btnExJogYp);
            this.tabPage2.Location = new System.Drawing.Point(4, 16);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(253, 162);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Y";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnExJogYn
            // 
            this.btnExJogYn.AUTO_TEXT_ALIGN = true;
            this.btnExJogYn.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.btnExJogYn.BackColor = System.Drawing.SystemColors.Control;
            this.btnExJogYn.DIR = NSS_3330S.UC.ButtonExJog.JOG_DIR.Y;
            this.btnExJogYn.FlatAppearance.BorderColor = System.Drawing.Color.Chocolate;
            this.btnExJogYn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExJogYn.ForeColor = System.Drawing.Color.Black;
            this.btnExJogYn.Image = ((System.Drawing.Image)(resources.GetObject("btnExJogYn.Image")));
            this.btnExJogYn.JOG_DIR_REVERSE = true;
            this.btnExJogYn.Location = new System.Drawing.Point(92, 89);
            this.btnExJogYn.MOTOR_DIR_REVERSE = true;
            this.btnExJogYn.Name = "btnExJogYn";
            this.btnExJogYn.Size = new System.Drawing.Size(72, 67);
            this.btnExJogYn.TabIndex = 1090;
            this.btnExJogYn.Text = "─";
            this.btnExJogYn.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExJogYn.UseVisualStyleBackColor = false;
            // 
            // btnExJogYp
            // 
            this.btnExJogYp.AUTO_TEXT_ALIGN = true;
            this.btnExJogYp.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.btnExJogYp.BackColor = System.Drawing.SystemColors.Control;
            this.btnExJogYp.DIR = NSS_3330S.UC.ButtonExJog.JOG_DIR.Y;
            this.btnExJogYp.FlatAppearance.BorderColor = System.Drawing.Color.Chocolate;
            this.btnExJogYp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExJogYp.ForeColor = System.Drawing.Color.Black;
            this.btnExJogYp.Image = ((System.Drawing.Image)(resources.GetObject("btnExJogYp.Image")));
            this.btnExJogYp.JOG_DIR_REVERSE = false;
            this.btnExJogYp.Location = new System.Drawing.Point(92, 16);
            this.btnExJogYp.MOTOR_DIR_REVERSE = false;
            this.btnExJogYp.Name = "btnExJogYp";
            this.btnExJogYp.Size = new System.Drawing.Size(72, 67);
            this.btnExJogYp.TabIndex = 1091;
            this.btnExJogYp.Text = "┼";
            this.btnExJogYp.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnExJogYp.UseVisualStyleBackColor = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnExJogZn);
            this.tabPage3.Controls.Add(this.btnExJogZp);
            this.tabPage3.Location = new System.Drawing.Point(4, 16);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(253, 162);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Z";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnExJogZn
            // 
            this.btnExJogZn.AUTO_TEXT_ALIGN = true;
            this.btnExJogZn.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.btnExJogZn.BackColor = System.Drawing.SystemColors.Control;
            this.btnExJogZn.DIR = NSS_3330S.UC.ButtonExJog.JOG_DIR.Z;
            this.btnExJogZn.FlatAppearance.BorderColor = System.Drawing.Color.MidnightBlue;
            this.btnExJogZn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExJogZn.ForeColor = System.Drawing.Color.Black;
            this.btnExJogZn.Image = ((System.Drawing.Image)(resources.GetObject("btnExJogZn.Image")));
            this.btnExJogZn.JOG_DIR_REVERSE = true;
            this.btnExJogZn.Location = new System.Drawing.Point(92, 89);
            this.btnExJogZn.MOTOR_DIR_REVERSE = true;
            this.btnExJogZn.Name = "btnExJogZn";
            this.btnExJogZn.Size = new System.Drawing.Size(72, 67);
            this.btnExJogZn.TabIndex = 1090;
            this.btnExJogZn.Text = "─";
            this.btnExJogZn.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExJogZn.UseVisualStyleBackColor = false;
            // 
            // btnExJogZp
            // 
            this.btnExJogZp.AUTO_TEXT_ALIGN = true;
            this.btnExJogZp.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.btnExJogZp.BackColor = System.Drawing.SystemColors.Control;
            this.btnExJogZp.DIR = NSS_3330S.UC.ButtonExJog.JOG_DIR.Z;
            this.btnExJogZp.FlatAppearance.BorderColor = System.Drawing.Color.MidnightBlue;
            this.btnExJogZp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExJogZp.ForeColor = System.Drawing.Color.Black;
            this.btnExJogZp.Image = ((System.Drawing.Image)(resources.GetObject("btnExJogZp.Image")));
            this.btnExJogZp.JOG_DIR_REVERSE = false;
            this.btnExJogZp.Location = new System.Drawing.Point(92, 16);
            this.btnExJogZp.MOTOR_DIR_REVERSE = false;
            this.btnExJogZp.Name = "btnExJogZp";
            this.btnExJogZp.Size = new System.Drawing.Size(72, 67);
            this.btnExJogZp.TabIndex = 1091;
            this.btnExJogZp.Text = "┼";
            this.btnExJogZp.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnExJogZp.UseVisualStyleBackColor = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnExJogCCW);
            this.tabPage4.Controls.Add(this.btnExJogCW);
            this.tabPage4.Location = new System.Drawing.Point(4, 16);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(253, 162);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "R";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnExJogCCW
            // 
            this.btnExJogCCW.AUTO_TEXT_ALIGN = true;
            this.btnExJogCCW.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.btnExJogCCW.BackColor = System.Drawing.SystemColors.Control;
            this.btnExJogCCW.DIR = NSS_3330S.UC.ButtonExJog.JOG_DIR.R;
            this.btnExJogCCW.FlatAppearance.BorderColor = System.Drawing.Color.MidnightBlue;
            this.btnExJogCCW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExJogCCW.ForeColor = System.Drawing.Color.Black;
            this.btnExJogCCW.Image = ((System.Drawing.Image)(resources.GetObject("btnExJogCCW.Image")));
            this.btnExJogCCW.JOG_DIR_REVERSE = true;
            this.btnExJogCCW.Location = new System.Drawing.Point(53, 63);
            this.btnExJogCCW.MOTOR_DIR_REVERSE = true;
            this.btnExJogCCW.Name = "btnExJogCCW";
            this.btnExJogCCW.Size = new System.Drawing.Size(72, 67);
            this.btnExJogCCW.TabIndex = 1091;
            this.btnExJogCCW.Text = "─";
            this.btnExJogCCW.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExJogCCW.UseVisualStyleBackColor = false;
            // 
            // btnExJogCW
            // 
            this.btnExJogCW.AUTO_TEXT_ALIGN = true;
            this.btnExJogCW.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.btnExJogCW.BackColor = System.Drawing.SystemColors.Control;
            this.btnExJogCW.DIR = NSS_3330S.UC.ButtonExJog.JOG_DIR.R;
            this.btnExJogCW.FlatAppearance.BorderColor = System.Drawing.Color.MidnightBlue;
            this.btnExJogCW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExJogCW.ForeColor = System.Drawing.Color.Black;
            this.btnExJogCW.Image = ((System.Drawing.Image)(resources.GetObject("btnExJogCW.Image")));
            this.btnExJogCW.JOG_DIR_REVERSE = false;
            this.btnExJogCW.Location = new System.Drawing.Point(131, 63);
            this.btnExJogCW.MOTOR_DIR_REVERSE = false;
            this.btnExJogCW.Name = "btnExJogCW";
            this.btnExJogCW.Size = new System.Drawing.Size(72, 67);
            this.btnExJogCW.TabIndex = 1092;
            this.btnExJogCW.Text = "┼";
            this.btnExJogCW.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnExJogCW.UseVisualStyleBackColor = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold);
            this.label9.Location = new System.Drawing.Point(226, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 17);
            this.label9.TabIndex = 1091;
            this.label9.Text = "현재값:";
            // 
            // lbCurPos
            // 
            this.lbCurPos.AXIS = NSS_3330S.SVDF.AXES.NONE;
            this.lbCurPos.BackColor = System.Drawing.SystemColors.Control;
            this.lbCurPos.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbCurPos.Location = new System.Drawing.Point(226, 41);
            this.lbCurPos.Name = "lbCurPos";
            this.lbCurPos.Size = new System.Drawing.Size(72, 27);
            this.lbCurPos.TabIndex = 1090;
            this.lbCurPos.Text = "000.0000";
            this.lbCurPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabJogMode
            // 
            this.tabJogMode.Controls.Add(this.tabPage22);
            this.tabJogMode.Controls.Add(this.tabPage8);
            this.tabJogMode.Controls.Add(this.tabPage16);
            this.tabJogMode.ItemSize = new System.Drawing.Size(66, 35);
            this.tabJogMode.Location = new System.Drawing.Point(11, 45);
            this.tabJogMode.Name = "tabJogMode";
            this.tabJogMode.SelectedIndex = 0;
            this.tabJogMode.Size = new System.Drawing.Size(205, 138);
            this.tabJogMode.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabJogMode.TabIndex = 1065;
            // 
            // tabPage22
            // 
            this.tabPage22.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabPage22.Controls.Add(this.rbJogSpeedHigh);
            this.tabPage22.Controls.Add(this.rbJogSpeedLow);
            this.tabPage22.Controls.Add(this.rbJogSpeedMiddle);
            this.tabPage22.Location = new System.Drawing.Point(4, 39);
            this.tabPage22.Name = "tabPage22";
            this.tabPage22.Size = new System.Drawing.Size(197, 95);
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
            this.tabPage8.Controls.Add(this.lbIncSpeed);
            this.tabPage8.Controls.Add(this.tbxIncMove);
            this.tabPage8.Controls.Add(this.buttonInc1um);
            this.tabPage8.Controls.Add(this.buttonInc1000um);
            this.tabPage8.Controls.Add(this.buttonInc100um);
            this.tabPage8.Controls.Add(this.buttonInc10um);
            this.tabPage8.Controls.Add(this.label54);
            this.tabPage8.Location = new System.Drawing.Point(4, 39);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(197, 95);
            this.tabPage8.TabIndex = 0;
            this.tabPage8.Text = "INC";
            // 
            // tbxIncMove
            // 
            this.tbxIncMove.Location = new System.Drawing.Point(13, 19);
            this.tbxIncMove.Name = "tbxIncMove";
            this.tbxIncMove.Size = new System.Drawing.Size(72, 21);
            this.tbxIncMove.TabIndex = 1211;
            this.tbxIncMove.Text = "0.1";
            this.tbxIncMove.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonInc1um
            // 
            this.buttonInc1um.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInc1um.ForeColor = System.Drawing.Color.Black;
            this.buttonInc1um.Location = new System.Drawing.Point(4, 50);
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
            this.buttonInc1000um.Location = new System.Drawing.Point(146, 50);
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
            this.buttonInc100um.Location = new System.Drawing.Point(100, 50);
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
            this.buttonInc10um.Location = new System.Drawing.Point(52, 50);
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
            this.label54.Location = new System.Drawing.Point(91, 24);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(27, 12);
            this.label54.TabIndex = 15;
            this.label54.Text = "mm";
            // 
            // tabPage16
            // 
            this.tabPage16.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage16.Controls.Add(this.rdbAbsVelHigh);
            this.tabPage16.Controls.Add(this.rdbAbsVelLow);
            this.tabPage16.Controls.Add(this.rdbAbsVelMid);
            this.tabPage16.Controls.Add(this.tbxAbsMove);
            this.tabPage16.Controls.Add(this.label5);
            this.tabPage16.Location = new System.Drawing.Point(4, 39);
            this.tabPage16.Name = "tabPage16";
            this.tabPage16.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage16.Size = new System.Drawing.Size(197, 95);
            this.tabPage16.TabIndex = 3;
            this.tabPage16.Text = "ABS";
            // 
            // tbxAbsMove
            // 
            this.tbxAbsMove.Location = new System.Drawing.Point(13, 19);
            this.tbxAbsMove.Name = "tbxAbsMove";
            this.tbxAbsMove.Size = new System.Drawing.Size(72, 21);
            this.tbxAbsMove.TabIndex = 20;
            this.tbxAbsMove.Text = "0.1";
            this.tbxAbsMove.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(91, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 12);
            this.label5.TabIndex = 17;
            this.label5.Text = "mm";
            // 
            // lbError
            // 
            this.lbError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbError.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbError.ForeColor = System.Drawing.Color.White;
            this.lbError.Location = new System.Drawing.Point(11, 5);
            this.lbError.Name = "lbError";
            this.lbError.Size = new System.Drawing.Size(31, 31);
            this.lbError.TabIndex = 1066;
            this.lbError.Text = "E";
            this.lbError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbHomeComplete
            // 
            this.lbHomeComplete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lbHomeComplete.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbHomeComplete.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbHomeComplete.ForeColor = System.Drawing.Color.White;
            this.lbHomeComplete.Location = new System.Drawing.Point(44, 5);
            this.lbHomeComplete.Name = "lbHomeComplete";
            this.lbHomeComplete.Size = new System.Drawing.Size(31, 31);
            this.lbHomeComplete.TabIndex = 1067;
            this.lbHomeComplete.Text = "C";
            this.lbHomeComplete.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbHome
            // 
            this.lbHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lbHome.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbHome.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbHome.ForeColor = System.Drawing.Color.White;
            this.lbHome.Location = new System.Drawing.Point(110, 5);
            this.lbHome.Name = "lbHome";
            this.lbHome.Size = new System.Drawing.Size(31, 31);
            this.lbHome.TabIndex = 1069;
            this.lbHome.Text = "H";
            this.lbHome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbBusy
            // 
            this.lbBusy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lbBusy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbBusy.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbBusy.ForeColor = System.Drawing.Color.White;
            this.lbBusy.Location = new System.Drawing.Point(77, 5);
            this.lbBusy.Name = "lbBusy";
            this.lbBusy.Size = new System.Drawing.Size(31, 31);
            this.lbBusy.TabIndex = 1068;
            this.lbBusy.Text = "B";
            this.lbBusy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbPlus
            // 
            this.lbPlus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbPlus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbPlus.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbPlus.ForeColor = System.Drawing.Color.White;
            this.lbPlus.Location = new System.Drawing.Point(176, 5);
            this.lbPlus.Name = "lbPlus";
            this.lbPlus.Size = new System.Drawing.Size(31, 31);
            this.lbPlus.TabIndex = 1071;
            this.lbPlus.Text = "L +";
            this.lbPlus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMinus
            // 
            this.lbMinus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbMinus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMinus.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbMinus.ForeColor = System.Drawing.Color.White;
            this.lbMinus.Location = new System.Drawing.Point(143, 5);
            this.lbMinus.Name = "lbMinus";
            this.lbMinus.Size = new System.Drawing.Size(31, 31);
            this.lbMinus.TabIndex = 1070;
            this.lbMinus.Text = "L -";
            this.lbMinus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbPadNo
            // 
            this.cbPadNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPadNo.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cbPadNo.FormattingEnabled = true;
            this.cbPadNo.ItemHeight = 21;
            this.cbPadNo.Items.AddRange(new object[] {
            "PAD1",
            "PAD2",
            "PAD3",
            "PAD4",
            "PAD5",
            "PAD6",
            "PAD7",
            "PAD8",
            "PAD9",
            "PAD10"});
            this.cbPadNo.Location = new System.Drawing.Point(448, 8);
            this.cbPadNo.Name = "cbPadNo";
            this.cbPadNo.Size = new System.Drawing.Size(83, 29);
            this.cbPadNo.TabIndex = 1727;
            this.cbPadNo.SelectedIndexChanged += new System.EventHandler(this.cbPadNo_SelectedIndexChanged);
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 50;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // lbIncSpeed
            // 
            this.lbIncSpeed.AutoSize = true;
            this.lbIncSpeed.Location = new System.Drawing.Point(144, 24);
            this.lbIncSpeed.Name = "lbIncSpeed";
            this.lbIncSpeed.Size = new System.Drawing.Size(29, 12);
            this.lbIncSpeed.TabIndex = 1728;
            this.lbIncSpeed.Text = "저속";
            // 
            // rdbAbsVelHigh
            // 
            this.rdbAbsVelHigh.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbAbsVelHigh.ForeColor = System.Drawing.Color.Black;
            this.rdbAbsVelHigh.Location = new System.Drawing.Point(131, 50);
            this.rdbAbsVelHigh.Name = "rdbAbsVelHigh";
            this.rdbAbsVelHigh.Size = new System.Drawing.Size(62, 40);
            this.rdbAbsVelHigh.TabIndex = 1010;
            this.rdbAbsVelHigh.Tag = "2";
            this.rdbAbsVelHigh.Text = "고속";
            this.rdbAbsVelHigh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbAbsVelHigh.UseVisualStyleBackColor = true;
            // 
            // rdbAbsVelLow
            // 
            this.rdbAbsVelLow.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbAbsVelLow.Checked = true;
            this.rdbAbsVelLow.ForeColor = System.Drawing.Color.Black;
            this.rdbAbsVelLow.Location = new System.Drawing.Point(3, 50);
            this.rdbAbsVelLow.Name = "rdbAbsVelLow";
            this.rdbAbsVelLow.Size = new System.Drawing.Size(62, 40);
            this.rdbAbsVelLow.TabIndex = 1008;
            this.rdbAbsVelLow.TabStop = true;
            this.rdbAbsVelLow.Tag = "0";
            this.rdbAbsVelLow.Text = "저속";
            this.rdbAbsVelLow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbAbsVelLow.UseVisualStyleBackColor = true;
            // 
            // rdbAbsVelMid
            // 
            this.rdbAbsVelMid.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdbAbsVelMid.ForeColor = System.Drawing.Color.Black;
            this.rdbAbsVelMid.Location = new System.Drawing.Point(67, 50);
            this.rdbAbsVelMid.Name = "rdbAbsVelMid";
            this.rdbAbsVelMid.Size = new System.Drawing.Size(62, 40);
            this.rdbAbsVelMid.TabIndex = 1009;
            this.rdbAbsVelMid.Tag = "1";
            this.rdbAbsVelMid.Text = "중속";
            this.rdbAbsVelMid.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdbAbsVelMid.UseVisualStyleBackColor = true;
            // 
            // ucSvManualJog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label9);
            this.Controls.Add(this.lbCurPos);
            this.Controls.Add(this.cbPadNo);
            this.Controls.Add(this.lbPlus);
            this.Controls.Add(this.lbMinus);
            this.Controls.Add(this.lbHome);
            this.Controls.Add(this.lbBusy);
            this.Controls.Add(this.lbHomeComplete);
            this.Controls.Add(this.lbError);
            this.Controls.Add(this.tabJogMode);
            this.Controls.Add(this.panel1);
            this.Name = "ucSvManualJog";
            this.Size = new System.Drawing.Size(540, 190);
            this.Load += new System.EventHandler(this.ucSvManualJog_Load);
            this.panel1.ResumeLayout(false);
            this.tabJog.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabJogMode.ResumeLayout(false);
            this.tabPage22.ResumeLayout(false);
            this.tabPage8.ResumeLayout(false);
            this.tabPage8.PerformLayout();
            this.tabPage16.ResumeLayout(false);
            this.tabPage16.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabJog;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
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
        private System.Windows.Forms.Label lbError;
        private System.Windows.Forms.Label lbHomeComplete;
        private System.Windows.Forms.Label lbHome;
        private System.Windows.Forms.Label lbBusy;
        private System.Windows.Forms.Label lbPlus;
        private System.Windows.Forms.Label lbMinus;
        private System.Windows.Forms.ComboBox cbPadNo;
        private LabelExJogPos lbCurPos;
        private ButtonExJog btnExJogXn;
        private ButtonExJog btnExJogXp;
        private ButtonExJog btnExJogYn;
        private ButtonExJog btnExJogYp;
        private ButtonExJog btnExJogZn;
        private ButtonExJog btnExJogZp;
        private ButtonExJog btnExJogCCW;
        private ButtonExJog btnExJogCW;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Label lbIncSpeed;
        private System.Windows.Forms.RadioButton rdbAbsVelHigh;
        private System.Windows.Forms.RadioButton rdbAbsVelLow;
        private System.Windows.Forms.RadioButton rdbAbsVelMid;
    }
}
