namespace NSS_3330S.POP
{
    partial class popInitializeSet
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
            this.btnSorter = new Glass.GlassButton();
            this.btnSaw = new Glass.GlassButton();
            this.btnLoader = new Glass.GlassButton();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSorter
            // 
            this.btnSorter.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSorter.FadeOnFocus = true;
            this.btnSorter.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSorter.ForeColor = System.Drawing.Color.Black;
            this.btnSorter.GlowColor = System.Drawing.SystemColors.Control;
            this.btnSorter.Image = global::NSS_3330S.Properties.Resources._2x2_grid_icon_48;
            this.btnSorter.Location = new System.Drawing.Point(441, 72);
            this.btnSorter.Name = "btnSorter";
            this.btnSorter.Padding = new System.Windows.Forms.Padding(0, 0, 0, 38);
            this.btnSorter.Size = new System.Drawing.Size(201, 182);
            this.btnSorter.TabIndex = 1074;
            this.btnSorter.Tag = "2";
            this.btnSorter.Text = "Sorter";
            this.btnSorter.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSorter.Click += new System.EventHandler(this.btnLoader_Click);
            // 
            // btnSaw
            // 
            this.btnSaw.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSaw.FadeOnFocus = true;
            this.btnSaw.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSaw.ForeColor = System.Drawing.Color.Black;
            this.btnSaw.GlowColor = System.Drawing.SystemColors.Control;
            this.btnSaw.Image = global::NSS_3330S.Properties.Resources.magic_wand_2_icon_48;
            this.btnSaw.Location = new System.Drawing.Point(229, 72);
            this.btnSaw.Name = "btnSaw";
            this.btnSaw.Padding = new System.Windows.Forms.Padding(0, 0, 0, 38);
            this.btnSaw.Size = new System.Drawing.Size(201, 182);
            this.btnSaw.TabIndex = 1073;
            this.btnSaw.Tag = "1";
            this.btnSaw.Text = "Dicing";
            this.btnSaw.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSaw.Click += new System.EventHandler(this.btnLoader_Click);
            // 
            // btnLoader
            // 
            this.btnLoader.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnLoader.FadeOnFocus = true;
            this.btnLoader.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnLoader.ForeColor = System.Drawing.Color.Black;
            this.btnLoader.GlowColor = System.Drawing.SystemColors.Control;
            this.btnLoader.Image = global::NSS_3330S.Properties.Resources.indent_increase_icon_48;
            this.btnLoader.Location = new System.Drawing.Point(16, 72);
            this.btnLoader.Name = "btnLoader";
            this.btnLoader.Padding = new System.Windows.Forms.Padding(0, 0, 0, 38);
            this.btnLoader.Size = new System.Drawing.Size(201, 182);
            this.btnLoader.TabIndex = 1072;
            this.btnLoader.Tag = "0";
            this.btnLoader.Text = "Loader";
            this.btnLoader.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnLoader.Click += new System.EventHandler(this.btnLoader_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Malgun Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(29, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(364, 37);
            this.label2.TabIndex = 1075;
            this.label2.Text = "Please select Initial moduel";
            // 
            // btnOk
            // 
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOk.Font = new System.Drawing.Font("Malgun Gothic", 20.25F, System.Drawing.FontStyle.Bold);
            this.btnOk.Location = new System.Drawing.Point(171, 284);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(141, 72);
            this.btnOk.TabIndex = 1076;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("Malgun Gothic", 20.25F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(339, 284);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(141, 72);
            this.btnCancel.TabIndex = 1076;
            this.btnCancel.Text = "CANCEL";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // popInitializeSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(657, 380);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSorter);
            this.Controls.Add(this.btnSaw);
            this.Controls.Add(this.btnLoader);
            this.Name = "popInitializeSet";
            this.Text = "popInitializeSet";
            this.Load += new System.EventHandler(this.popInitializeSet_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Glass.GlassButton btnSorter;
        private Glass.GlassButton btnSaw;
        private Glass.GlassButton btnLoader;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}