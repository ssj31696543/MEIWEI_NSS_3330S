namespace NSS_3330S.UC
{
    partial class UcVacuumAirView
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
            this.dgvVacAir = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVacAir)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvVacAir
            // 
            this.dgvVacAir.AllowUserToAddRows = false;
            this.dgvVacAir.AllowUserToDeleteRows = false;
            this.dgvVacAir.AllowUserToResizeColumns = false;
            this.dgvVacAir.AllowUserToResizeRows = false;
            this.dgvVacAir.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvVacAir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVacAir.Location = new System.Drawing.Point(0, 0);
            this.dgvVacAir.MultiSelect = false;
            this.dgvVacAir.Name = "dgvVacAir";
            this.dgvVacAir.RowHeadersVisible = false;
            this.dgvVacAir.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvVacAir.RowTemplate.Height = 23;
            this.dgvVacAir.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dgvVacAir.Size = new System.Drawing.Size(904, 652);
            this.dgvVacAir.TabIndex = 1082;
            this.dgvVacAir.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVacAir_CellDoubleClick);
            this.dgvVacAir.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVacAir_CellValueChanged);
            // 
            // UcVacuumAirView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvVacAir);
            this.Name = "UcVacuumAirView";
            this.Size = new System.Drawing.Size(904, 652);
            this.VisibleChanged += new System.EventHandler(this.UcVacuumAirView_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVacAir)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvVacAir;
    }
}
