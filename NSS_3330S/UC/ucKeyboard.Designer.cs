namespace NSS_3330S.UC
{
    partial class ucKeyboard
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
            this.virtualKeyboard = new Keyboard.VirtualKeyboard();
            this.SuspendLayout();
            // 
            // virtualKeyboard
            // 
            this.virtualKeyboard.AltGrState = false;
            this.virtualKeyboard.AltState = false;
            this.virtualKeyboard.BackColor = System.Drawing.Color.Transparent;
            this.virtualKeyboard.BackgroundColor = System.Drawing.Color.White;
            this.virtualKeyboard.BeginGradientColor = System.Drawing.Color.Gray;
            this.virtualKeyboard.BorderColor = System.Drawing.Color.Black;
            this.virtualKeyboard.ButtonBorderColor = System.Drawing.Color.Black;
            this.virtualKeyboard.ButtonRectRadius = 5;
            this.virtualKeyboard.CapsLockState = false;
            this.virtualKeyboard.ColorPressedState = System.Drawing.Color.Bisque;
            this.virtualKeyboard.CtrlState = false;
            this.virtualKeyboard.DecimalSeparator = '.';
            this.virtualKeyboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.virtualKeyboard.EndGradientColor = System.Drawing.Color.Black;
            this.virtualKeyboard.FontColor = System.Drawing.Color.White;
            this.virtualKeyboard.FontColorShiftDisabled = System.Drawing.Color.Silver;
            this.virtualKeyboard.FontColorSpecialKey = System.Drawing.Color.White;
            this.virtualKeyboard.IsNumeric = false;
            this.virtualKeyboard.LabelFont = new System.Drawing.Font("Tahoma", 12.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.virtualKeyboard.LabelFontShiftDisabled = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.virtualKeyboard.LabelFontSpecialKey = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.virtualKeyboard.LanguageButtonBottomText = "";
            this.virtualKeyboard.LanguageButtonImage = null;
            this.virtualKeyboard.LanguageButtonTopText = "Eng";
            this.virtualKeyboard.LayoutSettings = null;
            this.virtualKeyboard.Location = new System.Drawing.Point(0, 0);
            this.virtualKeyboard.Name = "virtualKeyboard";
            this.virtualKeyboard.NumLockState = false;
            this.virtualKeyboard.ShadowColor = System.Drawing.Color.DarkGray;
            this.virtualKeyboard.ShadowShift = 0;
            this.virtualKeyboard.ShiftState = false;
            this.virtualKeyboard.ShowBackground = true;
            this.virtualKeyboard.ShowFunctionButtons = false;
            this.virtualKeyboard.ShowLanguageButton = false;
            this.virtualKeyboard.ShowNumericButtons = false;
            this.virtualKeyboard.Size = new System.Drawing.Size(898, 271);
            this.virtualKeyboard.TabIndex = 3;
            this.virtualKeyboard.Text = "virtualKeyboard1";
            // 
            // ucKeyboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 271);
            this.Controls.Add(this.virtualKeyboard);
            this.Name = "ucKeyboard";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ucKeyboard_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Keyboard.VirtualKeyboard virtualKeyboard;
    }
}
