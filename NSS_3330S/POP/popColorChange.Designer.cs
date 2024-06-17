namespace NSS_3330S.POP
{
    partial class popColorChange
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
            this.cmb_ColorListbox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmb_ColorListbox
            // 
            this.cmb_ColorListbox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmb_ColorListbox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_ColorListbox.FormattingEnabled = true;
            this.cmb_ColorListbox.Location = new System.Drawing.Point(13, 25);
            this.cmb_ColorListbox.Name = "cmb_ColorListbox";
            this.cmb_ColorListbox.Size = new System.Drawing.Size(262, 22);
            this.cmb_ColorListbox.TabIndex = 0;
            this.cmb_ColorListbox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmb_ColorListbox_DrawItem);
            this.cmb_ColorListbox.SelectedIndexChanged += new System.EventHandler(this.cmb_ColorListbox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(262, 75);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(14, 126);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(262, 68);
            this.btn_save.TabIndex = 2;
            this.btn_save.Text = "SAVE";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // popColorChange
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 211);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmb_ColorListbox);
            this.Name = "popColorChange";
            this.Text = "popColorChange";
            this.Load += new System.EventHandler(this.popColorChange_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmb_ColorListbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_save;
    }
}