namespace Gothic_3_Quest_and_Dialog_Editor
{
    partial class FormCreateNewFile
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCreateNewFile));
            this.cbx_NewFileExtension = new System.Windows.Forms.ComboBox();
            this.btn_NewFileConfirm = new System.Windows.Forms.Button();
            this.lbl_NewFileName = new System.Windows.Forms.Label();
            this.tbx_NewFileName = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // cbx_NewFileExtension
            // 
            this.cbx_NewFileExtension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbx_NewFileExtension.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.cbx_NewFileExtension.FormattingEnabled = true;
            this.cbx_NewFileExtension.Location = new System.Drawing.Point(376, 10);
            this.cbx_NewFileExtension.Name = "cbx_NewFileExtension";
            this.cbx_NewFileExtension.Size = new System.Drawing.Size(149, 21);
            this.cbx_NewFileExtension.TabIndex = 138;
            // 
            // btn_NewFileConfirm
            // 
            this.btn_NewFileConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btn_NewFileConfirm.Location = new System.Drawing.Point(531, 7);
            this.btn_NewFileConfirm.Name = "btn_NewFileConfirm";
            this.btn_NewFileConfirm.Size = new System.Drawing.Size(67, 23);
            this.btn_NewFileConfirm.TabIndex = 139;
            this.btn_NewFileConfirm.Text = "OK";
            this.btn_NewFileConfirm.UseVisualStyleBackColor = true;
            // 
            // lbl_NewFileName
            // 
            this.lbl_NewFileName.AutoSize = true;
            this.lbl_NewFileName.Font = new System.Drawing.Font("Monotype Corsiva", 13F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_NewFileName.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbl_NewFileName.Location = new System.Drawing.Point(12, 9);
            this.lbl_NewFileName.Name = "lbl_NewFileName";
            this.lbl_NewFileName.Size = new System.Drawing.Size(87, 21);
            this.lbl_NewFileName.TabIndex = 136;
            this.lbl_NewFileName.Text = "File Name";
            // 
            // tbx_NewFileName
            // 
            this.tbx_NewFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tbx_NewFileName.Location = new System.Drawing.Point(105, 10);
            this.tbx_NewFileName.Name = "tbx_NewFileName";
            this.tbx_NewFileName.Size = new System.Drawing.Size(260, 21);
            this.tbx_NewFileName.TabIndex = 137;
            this.tbx_NewFileName.Text = "";
            this.tbx_NewFileName.WordWrap = false;
            // 
            // FormCreateNewFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(610, 41);
            this.Controls.Add(this.cbx_NewFileExtension);
            this.Controls.Add(this.btn_NewFileConfirm);
            this.Controls.Add(this.lbl_NewFileName);
            this.Controls.Add(this.tbx_NewFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCreateNewFile";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Create New File";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ComboBox cbx_NewFileExtension;
        public System.Windows.Forms.Button btn_NewFileConfirm;
        public System.Windows.Forms.Label lbl_NewFileName;
        public System.Windows.Forms.RichTextBox tbx_NewFileName;
    }
}