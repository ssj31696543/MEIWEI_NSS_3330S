namespace NSS_3330S.UC
{
    partial class ucVacList
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
            this.LOADER_GB = new System.Windows.Forms.GroupBox();
            this.ucVacInfo2 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo1 = new NSS_3330S.UC.ucVacInfo();
            this.CLEANER_GB = new System.Windows.Forms.GroupBox();
            this.ucVacInfo5 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo4 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo3 = new NSS_3330S.UC.ucVacInfo();
            this.SORTER_GB = new System.Windows.Forms.GroupBox();
            this.ucVacInfo25 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo26 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo23 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo22 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo21 = new NSS_3330S.UC.ucVacInfo();
            this.PICKER2_GB = new System.Windows.Forms.GroupBox();
            this.ucVacInfo15 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo16 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo17 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo18 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo19 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo20 = new NSS_3330S.UC.ucVacInfo();
            this.PICKER1_GB = new System.Windows.Forms.GroupBox();
            this.ucVacInfo13 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo14 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo11 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo12 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo9 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo10 = new NSS_3330S.UC.ucVacInfo();
            this.DEVICE_GB = new System.Windows.Forms.GroupBox();
            this.ucVacInfo33 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo32 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo24 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo27 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo28 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo29 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo30 = new NSS_3330S.UC.ucVacInfo();
            this.ucVacInfo31 = new NSS_3330S.UC.ucVacInfo();
            this.LOADER_GB.SuspendLayout();
            this.CLEANER_GB.SuspendLayout();
            this.SORTER_GB.SuspendLayout();
            this.PICKER2_GB.SuspendLayout();
            this.PICKER1_GB.SuspendLayout();
            this.DEVICE_GB.SuspendLayout();
            this.SuspendLayout();
            // 
            // LOADER_GB
            // 
            this.LOADER_GB.Controls.Add(this.ucVacInfo2);
            this.LOADER_GB.Controls.Add(this.ucVacInfo1);
            this.LOADER_GB.Location = new System.Drawing.Point(17, 13);
            this.LOADER_GB.Name = "LOADER_GB";
            this.LOADER_GB.Size = new System.Drawing.Size(292, 151);
            this.LOADER_GB.TabIndex = 0;
            this.LOADER_GB.TabStop = false;
            this.LOADER_GB.Text = "로더부 진공";
            // 
            // ucVacInfo2
            // 
            this.ucVacInfo2.A_INPUT = NSS_3330S.IODF.A_INPUT.STRIP_PK_VAC;
            this.ucVacInfo2.Location = new System.Drawing.Point(148, 52);
            this.ucVacInfo2.Name = "ucVacInfo2";
            this.ucVacInfo2.ON = false;
            this.ucVacInfo2.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo2.TabIndex = 1;
            this.ucVacInfo2.VAC_NAME = "스트립 피커";
            this.ucVacInfo2.VAC_VAL = "-10";
            // 
            // ucVacInfo1
            // 
            this.ucVacInfo1.A_INPUT = NSS_3330S.IODF.A_INPUT.LD_RAIL_VAC;
            this.ucVacInfo1.Location = new System.Drawing.Point(6, 52);
            this.ucVacInfo1.Name = "ucVacInfo1";
            this.ucVacInfo1.ON = false;
            this.ucVacInfo1.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo1.TabIndex = 0;
            this.ucVacInfo1.VAC_NAME = "로딩 레일";
            this.ucVacInfo1.VAC_VAL = "-10";
            // 
            // CLEANER_GB
            // 
            this.CLEANER_GB.Controls.Add(this.ucVacInfo5);
            this.CLEANER_GB.Controls.Add(this.ucVacInfo4);
            this.CLEANER_GB.Controls.Add(this.ucVacInfo3);
            this.CLEANER_GB.Location = new System.Drawing.Point(17, 163);
            this.CLEANER_GB.Name = "CLEANER_GB";
            this.CLEANER_GB.Size = new System.Drawing.Size(292, 263);
            this.CLEANER_GB.TabIndex = 1;
            this.CLEANER_GB.TabStop = false;
            this.CLEANER_GB.Text = "수세기부 진공";
            // 
            // ucVacInfo5
            // 
            this.ucVacInfo5.A_INPUT = NSS_3330S.IODF.A_INPUT.UNIT_PK_VAC;
            this.ucVacInfo5.Location = new System.Drawing.Point(6, 100);
            this.ucVacInfo5.Name = "ucVacInfo5";
            this.ucVacInfo5.ON = false;
            this.ucVacInfo5.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo5.TabIndex = 4;
            this.ucVacInfo5.VAC_NAME = "유닛 피커";
            this.ucVacInfo5.VAC_VAL = "-10";
            // 
            // ucVacInfo4
            // 
            this.ucVacInfo4.A_INPUT = NSS_3330S.IODF.A_INPUT.SCRAP_VAC_2;
            this.ucVacInfo4.Location = new System.Drawing.Point(148, 20);
            this.ucVacInfo4.Name = "ucVacInfo4";
            this.ucVacInfo4.ON = false;
            this.ucVacInfo4.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo4.TabIndex = 3;
            this.ucVacInfo4.VAC_NAME = "유닛 피커 스크랩 2";
            this.ucVacInfo4.VAC_VAL = "-10";
            // 
            // ucVacInfo3
            // 
            this.ucVacInfo3.A_INPUT = NSS_3330S.IODF.A_INPUT.SCRAP_VAC_1;
            this.ucVacInfo3.Location = new System.Drawing.Point(6, 20);
            this.ucVacInfo3.Name = "ucVacInfo3";
            this.ucVacInfo3.ON = false;
            this.ucVacInfo3.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo3.TabIndex = 2;
            this.ucVacInfo3.VAC_NAME = "유닛 피커 스크랩 1";
            this.ucVacInfo3.VAC_VAL = "-10";
            // 
            // SORTER_GB
            // 
            this.SORTER_GB.Controls.Add(this.ucVacInfo25);
            this.SORTER_GB.Controls.Add(this.ucVacInfo26);
            this.SORTER_GB.Controls.Add(this.ucVacInfo23);
            this.SORTER_GB.Controls.Add(this.ucVacInfo22);
            this.SORTER_GB.Controls.Add(this.ucVacInfo21);
            this.SORTER_GB.Controls.Add(this.PICKER2_GB);
            this.SORTER_GB.Controls.Add(this.PICKER1_GB);
            this.SORTER_GB.Location = new System.Drawing.Point(315, 13);
            this.SORTER_GB.Name = "SORTER_GB";
            this.SORTER_GB.Size = new System.Drawing.Size(448, 413);
            this.SORTER_GB.TabIndex = 2;
            this.SORTER_GB.TabStop = false;
            this.SORTER_GB.Text = "소터부 진공";
            // 
            // ucVacInfo25
            // 
            this.ucVacInfo25.A_INPUT = NSS_3330S.IODF.A_INPUT.MAP_STG_2_WORK_VAC;
            this.ucVacInfo25.Location = new System.Drawing.Point(296, 352);
            this.ucVacInfo25.Name = "ucVacInfo25";
            this.ucVacInfo25.ON = false;
            this.ucVacInfo25.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo25.TabIndex = 12;
            this.ucVacInfo25.VAC_NAME = "맵 스테이지 2 워크";
            this.ucVacInfo25.VAC_VAL = "-10";
            // 
            // ucVacInfo26
            // 
            this.ucVacInfo26.A_INPUT = NSS_3330S.IODF.A_INPUT.MAP_STG_1_WORK_VAC;
            this.ucVacInfo26.Location = new System.Drawing.Point(154, 352);
            this.ucVacInfo26.Name = "ucVacInfo26";
            this.ucVacInfo26.ON = false;
            this.ucVacInfo26.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo26.TabIndex = 11;
            this.ucVacInfo26.VAC_NAME = "맵 스테이지 1 워크";
            this.ucVacInfo26.VAC_VAL = "-10";
            // 
            // ucVacInfo23
            // 
            this.ucVacInfo23.A_INPUT = NSS_3330S.IODF.A_INPUT.MAP_PK_CHUCK_VAC;
            this.ucVacInfo23.Location = new System.Drawing.Point(296, 294);
            this.ucVacInfo23.Name = "ucVacInfo23";
            this.ucVacInfo23.ON = false;
            this.ucVacInfo23.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo23.TabIndex = 10;
            this.ucVacInfo23.VAC_NAME = "맵 피커 척";
            this.ucVacInfo23.VAC_VAL = "-10";
            // 
            // ucVacInfo22
            // 
            this.ucVacInfo22.A_INPUT = NSS_3330S.IODF.A_INPUT.MAP_PK_WORK_VAC;
            this.ucVacInfo22.Location = new System.Drawing.Point(154, 294);
            this.ucVacInfo22.Name = "ucVacInfo22";
            this.ucVacInfo22.ON = false;
            this.ucVacInfo22.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo22.TabIndex = 9;
            this.ucVacInfo22.VAC_NAME = "맵 피커 워크";
            this.ucVacInfo22.VAC_VAL = "-10";
            // 
            // ucVacInfo21
            // 
            this.ucVacInfo21.A_INPUT = NSS_3330S.IODF.A_INPUT.DRY_BLOCK_WORK_VAC;
            this.ucVacInfo21.Location = new System.Drawing.Point(12, 294);
            this.ucVacInfo21.Name = "ucVacInfo21";
            this.ucVacInfo21.ON = false;
            this.ucVacInfo21.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo21.TabIndex = 8;
            this.ucVacInfo21.VAC_NAME = "드라이 블럭";
            this.ucVacInfo21.VAC_VAL = "-10";
            // 
            // PICKER2_GB
            // 
            this.PICKER2_GB.Controls.Add(this.ucVacInfo15);
            this.PICKER2_GB.Controls.Add(this.ucVacInfo16);
            this.PICKER2_GB.Controls.Add(this.ucVacInfo17);
            this.PICKER2_GB.Controls.Add(this.ucVacInfo18);
            this.PICKER2_GB.Controls.Add(this.ucVacInfo19);
            this.PICKER2_GB.Controls.Add(this.ucVacInfo20);
            this.PICKER2_GB.Location = new System.Drawing.Point(6, 157);
            this.PICKER2_GB.Name = "PICKER2_GB";
            this.PICKER2_GB.Size = new System.Drawing.Size(437, 131);
            this.PICKER2_GB.TabIndex = 6;
            this.PICKER2_GB.TabStop = false;
            this.PICKER2_GB.Text = "칩 피커2 진공";
            // 
            // ucVacInfo15
            // 
            this.ucVacInfo15.A_INPUT = NSS_3330S.IODF.A_INPUT.X2_AXIS_6_PICKER_VACUUM;
            this.ucVacInfo15.Location = new System.Drawing.Point(290, 72);
            this.ucVacInfo15.Name = "ucVacInfo15";
            this.ucVacInfo15.ON = false;
            this.ucVacInfo15.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo15.TabIndex = 5;
            this.ucVacInfo15.VAC_NAME = "피커 패드 6";
            this.ucVacInfo15.VAC_VAL = "-10";
            // 
            // ucVacInfo16
            // 
            this.ucVacInfo16.A_INPUT = NSS_3330S.IODF.A_INPUT.X2_AXIS_5_PICKER_VACUUM;
            this.ucVacInfo16.Location = new System.Drawing.Point(148, 72);
            this.ucVacInfo16.Name = "ucVacInfo16";
            this.ucVacInfo16.ON = false;
            this.ucVacInfo16.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo16.TabIndex = 4;
            this.ucVacInfo16.VAC_NAME = "피커 패드 5";
            this.ucVacInfo16.VAC_VAL = "-10";
            // 
            // ucVacInfo17
            // 
            this.ucVacInfo17.A_INPUT = NSS_3330S.IODF.A_INPUT.X2_AXIS_4_PICKER_VACUUM;
            this.ucVacInfo17.Location = new System.Drawing.Point(6, 72);
            this.ucVacInfo17.Name = "ucVacInfo17";
            this.ucVacInfo17.ON = false;
            this.ucVacInfo17.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo17.TabIndex = 3;
            this.ucVacInfo17.VAC_NAME = "피커 패드 4";
            this.ucVacInfo17.VAC_VAL = "-10";
            // 
            // ucVacInfo18
            // 
            this.ucVacInfo18.A_INPUT = NSS_3330S.IODF.A_INPUT.X2_AXIS_3_PICKER_VACUUM;
            this.ucVacInfo18.Location = new System.Drawing.Point(290, 20);
            this.ucVacInfo18.Name = "ucVacInfo18";
            this.ucVacInfo18.ON = false;
            this.ucVacInfo18.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo18.TabIndex = 2;
            this.ucVacInfo18.VAC_NAME = "피커 패드 3";
            this.ucVacInfo18.VAC_VAL = "-10";
            // 
            // ucVacInfo19
            // 
            this.ucVacInfo19.A_INPUT = NSS_3330S.IODF.A_INPUT.X2_AXIS_2_PICKER_VACUUM;
            this.ucVacInfo19.Location = new System.Drawing.Point(148, 20);
            this.ucVacInfo19.Name = "ucVacInfo19";
            this.ucVacInfo19.ON = false;
            this.ucVacInfo19.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo19.TabIndex = 1;
            this.ucVacInfo19.VAC_NAME = "피커 패드 2";
            this.ucVacInfo19.VAC_VAL = "-10";
            // 
            // ucVacInfo20
            // 
            this.ucVacInfo20.A_INPUT = NSS_3330S.IODF.A_INPUT.X2_AXIS_1_PICKER_VACUUM;
            this.ucVacInfo20.Location = new System.Drawing.Point(6, 20);
            this.ucVacInfo20.Name = "ucVacInfo20";
            this.ucVacInfo20.ON = false;
            this.ucVacInfo20.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo20.TabIndex = 0;
            this.ucVacInfo20.VAC_NAME = "피커 패드 1";
            this.ucVacInfo20.VAC_VAL = "-10";
            // 
            // PICKER1_GB
            // 
            this.PICKER1_GB.Controls.Add(this.ucVacInfo13);
            this.PICKER1_GB.Controls.Add(this.ucVacInfo14);
            this.PICKER1_GB.Controls.Add(this.ucVacInfo11);
            this.PICKER1_GB.Controls.Add(this.ucVacInfo12);
            this.PICKER1_GB.Controls.Add(this.ucVacInfo9);
            this.PICKER1_GB.Controls.Add(this.ucVacInfo10);
            this.PICKER1_GB.Location = new System.Drawing.Point(6, 20);
            this.PICKER1_GB.Name = "PICKER1_GB";
            this.PICKER1_GB.Size = new System.Drawing.Size(437, 131);
            this.PICKER1_GB.TabIndex = 2;
            this.PICKER1_GB.TabStop = false;
            this.PICKER1_GB.Text = "칩 피커1 진공";
            // 
            // ucVacInfo13
            // 
            this.ucVacInfo13.A_INPUT = NSS_3330S.IODF.A_INPUT.X1_AXIS_6_PICKER_VACUUM;
            this.ucVacInfo13.Location = new System.Drawing.Point(290, 72);
            this.ucVacInfo13.Name = "ucVacInfo13";
            this.ucVacInfo13.ON = false;
            this.ucVacInfo13.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo13.TabIndex = 5;
            this.ucVacInfo13.VAC_NAME = "피커 패드 6";
            this.ucVacInfo13.VAC_VAL = "-10";
            // 
            // ucVacInfo14
            // 
            this.ucVacInfo14.A_INPUT = NSS_3330S.IODF.A_INPUT.X1_AXIS_5_PICKER_VACUUM;
            this.ucVacInfo14.Location = new System.Drawing.Point(148, 72);
            this.ucVacInfo14.Name = "ucVacInfo14";
            this.ucVacInfo14.ON = false;
            this.ucVacInfo14.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo14.TabIndex = 4;
            this.ucVacInfo14.VAC_NAME = "피커 패드 5";
            this.ucVacInfo14.VAC_VAL = "-10";
            // 
            // ucVacInfo11
            // 
            this.ucVacInfo11.A_INPUT = NSS_3330S.IODF.A_INPUT.X1_AXIS_4_PICKER_VACUUM;
            this.ucVacInfo11.Location = new System.Drawing.Point(6, 72);
            this.ucVacInfo11.Name = "ucVacInfo11";
            this.ucVacInfo11.ON = false;
            this.ucVacInfo11.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo11.TabIndex = 3;
            this.ucVacInfo11.VAC_NAME = "피커 패드 4";
            this.ucVacInfo11.VAC_VAL = "-10";
            // 
            // ucVacInfo12
            // 
            this.ucVacInfo12.A_INPUT = NSS_3330S.IODF.A_INPUT.X1_AXIS_3_PICKER_VACUUM;
            this.ucVacInfo12.Location = new System.Drawing.Point(290, 20);
            this.ucVacInfo12.Name = "ucVacInfo12";
            this.ucVacInfo12.ON = false;
            this.ucVacInfo12.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo12.TabIndex = 2;
            this.ucVacInfo12.VAC_NAME = "피커 패드 3";
            this.ucVacInfo12.VAC_VAL = "-10";
            // 
            // ucVacInfo9
            // 
            this.ucVacInfo9.A_INPUT = NSS_3330S.IODF.A_INPUT.X1_AXIS_2_PICKER_VACUUM;
            this.ucVacInfo9.Location = new System.Drawing.Point(148, 20);
            this.ucVacInfo9.Name = "ucVacInfo9";
            this.ucVacInfo9.ON = false;
            this.ucVacInfo9.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo9.TabIndex = 1;
            this.ucVacInfo9.VAC_NAME = "피커 패드 2";
            this.ucVacInfo9.VAC_VAL = "-10";
            // 
            // ucVacInfo10
            // 
            this.ucVacInfo10.A_INPUT = NSS_3330S.IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM;
            this.ucVacInfo10.Location = new System.Drawing.Point(6, 20);
            this.ucVacInfo10.Name = "ucVacInfo10";
            this.ucVacInfo10.ON = false;
            this.ucVacInfo10.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo10.TabIndex = 0;
            this.ucVacInfo10.VAC_NAME = "피커 패드 1";
            this.ucVacInfo10.VAC_VAL = "-10";
            // 
            // DEVICE_GB
            // 
            this.DEVICE_GB.Controls.Add(this.ucVacInfo33);
            this.DEVICE_GB.Controls.Add(this.ucVacInfo32);
            this.DEVICE_GB.Controls.Add(this.ucVacInfo24);
            this.DEVICE_GB.Controls.Add(this.ucVacInfo27);
            this.DEVICE_GB.Controls.Add(this.ucVacInfo28);
            this.DEVICE_GB.Controls.Add(this.ucVacInfo29);
            this.DEVICE_GB.Controls.Add(this.ucVacInfo30);
            this.DEVICE_GB.Controls.Add(this.ucVacInfo31);
            this.DEVICE_GB.Location = new System.Drawing.Point(769, 13);
            this.DEVICE_GB.Name = "DEVICE_GB";
            this.DEVICE_GB.Size = new System.Drawing.Size(292, 413);
            this.DEVICE_GB.TabIndex = 8;
            this.DEVICE_GB.TabStop = false;
            this.DEVICE_GB.Text = "장비 진공";
            // 
            // ucVacInfo33
            // 
            this.ucVacInfo33.A_INPUT = NSS_3330S.IODF.A_INPUT.CHIP_PK_AIR;
            this.ucVacInfo33.Location = new System.Drawing.Point(148, 207);
            this.ucVacInfo33.Name = "ucVacInfo33";
            this.ucVacInfo33.ON = false;
            this.ucVacInfo33.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo33.TabIndex = 9;
            this.ucVacInfo33.VAC_NAME = "칩 피커 에어";
            this.ucVacInfo33.VAC_VAL = "-10";
            // 
            // ucVacInfo32
            // 
            this.ucVacInfo32.A_INPUT = NSS_3330S.IODF.A_INPUT.MAP_PK_AIR;
            this.ucVacInfo32.Location = new System.Drawing.Point(6, 207);
            this.ucVacInfo32.Name = "ucVacInfo32";
            this.ucVacInfo32.ON = false;
            this.ucVacInfo32.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo32.TabIndex = 8;
            this.ucVacInfo32.VAC_NAME = "맵 피커 에어";
            this.ucVacInfo32.VAC_VAL = "-10";
            // 
            // ucVacInfo24
            // 
            this.ucVacInfo24.A_INPUT = NSS_3330S.IODF.A_INPUT.MAP_STG_AIR;
            this.ucVacInfo24.Location = new System.Drawing.Point(148, 146);
            this.ucVacInfo24.Name = "ucVacInfo24";
            this.ucVacInfo24.ON = false;
            this.ucVacInfo24.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo24.TabIndex = 7;
            this.ucVacInfo24.VAC_NAME = "맵 스테이지 에어";
            this.ucVacInfo24.VAC_VAL = "-10";
            // 
            // ucVacInfo27
            // 
            this.ucVacInfo27.A_INPUT = NSS_3330S.IODF.A_INPUT.BLOW_AIR;
            this.ucVacInfo27.Location = new System.Drawing.Point(6, 146);
            this.ucVacInfo27.Name = "ucVacInfo27";
            this.ucVacInfo27.ON = false;
            this.ucVacInfo27.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo27.TabIndex = 6;
            this.ucVacInfo27.VAC_NAME = "블로우 에어";
            this.ucVacInfo27.VAC_VAL = "-10";
            // 
            // ucVacInfo28
            // 
            this.ucVacInfo28.A_INPUT = NSS_3330S.IODF.A_INPUT.DRY_STG_AIR;
            this.ucVacInfo28.Location = new System.Drawing.Point(148, 84);
            this.ucVacInfo28.Name = "ucVacInfo28";
            this.ucVacInfo28.ON = false;
            this.ucVacInfo28.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo28.TabIndex = 5;
            this.ucVacInfo28.VAC_NAME = "드라이 스테이지 에어";
            this.ucVacInfo28.VAC_VAL = "-10";
            // 
            // ucVacInfo29
            // 
            this.ucVacInfo29.A_INPUT = NSS_3330S.IODF.A_INPUT.DRIVE_AIR;
            this.ucVacInfo29.Location = new System.Drawing.Point(6, 84);
            this.ucVacInfo29.Name = "ucVacInfo29";
            this.ucVacInfo29.ON = false;
            this.ucVacInfo29.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo29.TabIndex = 4;
            this.ucVacInfo29.VAC_NAME = "드라이브 에어";
            this.ucVacInfo29.VAC_VAL = "-10";
            // 
            // ucVacInfo30
            // 
            this.ucVacInfo30.A_INPUT = NSS_3330S.IODF.A_INPUT.PUMP_AIR_2;
            this.ucVacInfo30.Location = new System.Drawing.Point(148, 20);
            this.ucVacInfo30.Name = "ucVacInfo30";
            this.ucVacInfo30.ON = false;
            this.ucVacInfo30.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo30.TabIndex = 3;
            this.ucVacInfo30.VAC_NAME = "펌프 에어 2";
            this.ucVacInfo30.VAC_VAL = "-10";
            // 
            // ucVacInfo31
            // 
            this.ucVacInfo31.A_INPUT = NSS_3330S.IODF.A_INPUT.PUMP_AIR_1;
            this.ucVacInfo31.Location = new System.Drawing.Point(6, 20);
            this.ucVacInfo31.Name = "ucVacInfo31";
            this.ucVacInfo31.ON = false;
            this.ucVacInfo31.Size = new System.Drawing.Size(136, 52);
            this.ucVacInfo31.TabIndex = 2;
            this.ucVacInfo31.VAC_NAME = "펌프 에어 1";
            this.ucVacInfo31.VAC_VAL = "-10";
            // 
            // ucVacList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DEVICE_GB);
            this.Controls.Add(this.SORTER_GB);
            this.Controls.Add(this.CLEANER_GB);
            this.Controls.Add(this.LOADER_GB);
            this.Name = "ucVacList";
            this.Size = new System.Drawing.Size(1071, 436);
            this.LOADER_GB.ResumeLayout(false);
            this.CLEANER_GB.ResumeLayout(false);
            this.SORTER_GB.ResumeLayout(false);
            this.PICKER2_GB.ResumeLayout(false);
            this.PICKER1_GB.ResumeLayout(false);
            this.DEVICE_GB.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox LOADER_GB;
        private System.Windows.Forms.GroupBox CLEANER_GB;
        private System.Windows.Forms.GroupBox SORTER_GB;
        private ucVacInfo ucVacInfo2;
        private ucVacInfo ucVacInfo1;
        private ucVacInfo ucVacInfo5;
        private ucVacInfo ucVacInfo4;
        private ucVacInfo ucVacInfo3;
        private System.Windows.Forms.GroupBox PICKER1_GB;
        private ucVacInfo ucVacInfo9;
        private ucVacInfo ucVacInfo10;
        private System.Windows.Forms.GroupBox PICKER2_GB;
        private ucVacInfo ucVacInfo15;
        private ucVacInfo ucVacInfo16;
        private ucVacInfo ucVacInfo17;
        private ucVacInfo ucVacInfo18;
        private ucVacInfo ucVacInfo19;
        private ucVacInfo ucVacInfo20;
        private ucVacInfo ucVacInfo13;
        private ucVacInfo ucVacInfo14;
        private ucVacInfo ucVacInfo11;
        private ucVacInfo ucVacInfo12;
        private ucVacInfo ucVacInfo25;
        private ucVacInfo ucVacInfo26;
        private ucVacInfo ucVacInfo23;
        private ucVacInfo ucVacInfo22;
        private ucVacInfo ucVacInfo21;
        private System.Windows.Forms.GroupBox DEVICE_GB;
        private ucVacInfo ucVacInfo24;
        private ucVacInfo ucVacInfo27;
        private ucVacInfo ucVacInfo28;
        private ucVacInfo ucVacInfo29;
        private ucVacInfo ucVacInfo30;
        private ucVacInfo ucVacInfo31;
        private ucVacInfo ucVacInfo33;
        private ucVacInfo ucVacInfo32;
    }
}
