namespace NSS_3330S.UC
{
    partial class ucTrayInOutWarning
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
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucTrayInOutWarning));
            this.lbText = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbPicture = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbText
            // 
            resources.ApplyResources(this.lbText, "lbText");
            this.lbText.ForeColor = System.Drawing.Color.DarkRed;
            this.lbText.Name = "lbText";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.lbPicture);
            this.panel1.Name = "panel1";
            // 
            // lbPicture
            // 
            resources.ApplyResources(this.lbPicture, "lbPicture");
            this.lbPicture.ImageList = this.imageList1;
            this.lbPicture.Name = "lbPicture";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Login Rounded Up_50px.png");
            this.imageList1.Images.SetKeyName(1, "Login Rounded Up_50px_gray.png");
            this.imageList1.Images.SetKeyName(2, "Logout Rounded Down_50px.png");
            this.imageList1.Images.SetKeyName(3, "Logout Rounded Down_50px_gray.png");
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 500;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // ucTrayInOutWarning
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lbText);
            this.Name = "ucTrayInOutWarning";
            this.VisibleChanged += new System.EventHandler(this.ucTrayInOutWarning_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbText;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label lbPicture;
        private System.Windows.Forms.Timer tmrRefresh;
    }
}
