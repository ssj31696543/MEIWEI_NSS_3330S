namespace NSS_3330S.POP
{
    partial class popTODO
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.NO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ITEM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DETAIL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PART = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.damdang = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.STARTTIME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ENDTIME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DONE = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NO,
            this.ITEM,
            this.DETAIL,
            this.PART,
            this.damdang,
            this.STARTTIME,
            this.ENDTIME,
            this.DONE});
            this.dataGridView1.Location = new System.Drawing.Point(38, 25);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(1014, 396);
            this.dataGridView1.TabIndex = 0;
            // 
            // NO
            // 
            this.NO.HeaderText = "NO";
            this.NO.Name = "NO";
            // 
            // ITEM
            // 
            this.ITEM.HeaderText = "ITEM";
            this.ITEM.Name = "ITEM";
            // 
            // DETAIL
            // 
            this.DETAIL.HeaderText = "DETAIL";
            this.DETAIL.Name = "DETAIL";
            // 
            // PART
            // 
            this.PART.HeaderText = "PART";
            this.PART.Name = "PART";
            // 
            // damdang
            // 
            this.damdang.HeaderText = "담당";
            this.damdang.Name = "damdang";
            // 
            // STARTTIME
            // 
            this.STARTTIME.HeaderText = "STARTTIME";
            this.STARTTIME.Name = "STARTTIME";
            // 
            // ENDTIME
            // 
            this.ENDTIME.HeaderText = "ENDTIME";
            this.ENDTIME.Name = "ENDTIME";
            // 
            // DONE
            // 
            this.DONE.HeaderText = "DONE";
            this.DONE.Name = "DONE";
            // 
            // popTODO
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1064, 531);
            this.Controls.Add(this.dataGridView1);
            this.Name = "popTODO";
            this.Text = "popTODO";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn NO;
        private System.Windows.Forms.DataGridViewTextBoxColumn ITEM;
        private System.Windows.Forms.DataGridViewTextBoxColumn DETAIL;
        private System.Windows.Forms.DataGridViewTextBoxColumn PART;
        private System.Windows.Forms.DataGridViewTextBoxColumn damdang;
        private System.Windows.Forms.DataGridViewTextBoxColumn STARTTIME;
        private System.Windows.Forms.DataGridViewTextBoxColumn ENDTIME;
        private System.Windows.Forms.DataGridViewCheckBoxColumn DONE;
    }
}