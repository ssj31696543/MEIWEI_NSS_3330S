
namespace NSS_3330S.POP
{
    partial class ManualBladeIdKeyIn
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
            this.txbBladeId1 = new System.Windows.Forms.TextBox();
            this.txbBladeId2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBladeIdChange = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txbBladeId1
            // 
            this.txbBladeId1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txbBladeId1.Location = new System.Drawing.Point(83, 19);
            this.txbBladeId1.Name = "txbBladeId1";
            this.txbBladeId1.Size = new System.Drawing.Size(100, 25);
            this.txbBladeId1.TabIndex = 0;
            // 
            // txbBladeId2
            // 
            this.txbBladeId2.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txbBladeId2.Location = new System.Drawing.Point(83, 46);
            this.txbBladeId2.Name = "txbBladeId2";
            this.txbBladeId2.Size = new System.Drawing.Size(100, 25);
            this.txbBladeId2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Blade Id1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(12, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Blade Id2";
            // 
            // btnBladeIdChange
            // 
            this.btnBladeIdChange.Location = new System.Drawing.Point(15, 77);
            this.btnBladeIdChange.Name = "btnBladeIdChange";
            this.btnBladeIdChange.Size = new System.Drawing.Size(168, 34);
            this.btnBladeIdChange.TabIndex = 2;
            this.btnBladeIdChange.Text = "Change";
            this.btnBladeIdChange.UseVisualStyleBackColor = true;
            this.btnBladeIdChange.Click += new System.EventHandler(this.btnBladeIdChange_Click);
            // 
            // ManualBladeIdKeyIn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(209, 132);
            this.Controls.Add(this.btnBladeIdChange);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txbBladeId2);
            this.Controls.Add(this.txbBladeId1);
            this.Name = "ManualBladeIdKeyIn";
            this.Text = "ManualBladeIdKeyIn";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txbBladeId1;
        private System.Windows.Forms.TextBox txbBladeId2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBladeIdChange;
    }
}