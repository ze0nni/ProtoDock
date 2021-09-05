namespace ProtoDock
{
    partial class SettingsWindow
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
            this.SkinCombo = new System.Windows.Forms.ComboBox();
            this.SkinLabel = new System.Windows.Forms.Label();
            this.IconSize = new System.Windows.Forms.NumericUpDown();
            this.IconSizeLabel = new System.Windows.Forms.Label();
            this.IconSpace = new System.Windows.Forms.NumericUpDown();
            this.IconSpaceLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.IconSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IconSpace)).BeginInit();
            this.SuspendLayout();
            // 
            // SkinCombo
            // 
            this.SkinCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SkinCombo.FormattingEnabled = true;
            this.SkinCombo.Location = new System.Drawing.Point(12, 27);
            this.SkinCombo.Name = "SkinCombo";
            this.SkinCombo.Size = new System.Drawing.Size(192, 23);
            this.SkinCombo.TabIndex = 0;
            this.SkinCombo.SelectedIndexChanged += new System.EventHandler(this.SkinCombo_SelectedIndexChanged);
            // 
            // SkinLabel
            // 
            this.SkinLabel.AutoSize = true;
            this.SkinLabel.Location = new System.Drawing.Point(12, 9);
            this.SkinLabel.Name = "SkinLabel";
            this.SkinLabel.Size = new System.Drawing.Size(29, 15);
            this.SkinLabel.TabIndex = 1;
            this.SkinLabel.Text = "Skin";
            // 
            // IconSize
            // 
            this.IconSize.Location = new System.Drawing.Point(12, 71);
            this.IconSize.Name = "IconSize";
            this.IconSize.Size = new System.Drawing.Size(122, 23);
            this.IconSize.TabIndex = 2;
            // 
            // IconSizeLabel
            // 
            this.IconSizeLabel.AutoSize = true;
            this.IconSizeLabel.Location = new System.Drawing.Point(12, 53);
            this.IconSizeLabel.Name = "IconSizeLabel";
            this.IconSizeLabel.Size = new System.Drawing.Size(52, 15);
            this.IconSizeLabel.TabIndex = 1;
            this.IconSizeLabel.Text = "Icon size";
            // 
            // IconSpace
            // 
            this.IconSpace.Location = new System.Drawing.Point(140, 71);
            this.IconSpace.Name = "IconSpace";
            this.IconSpace.Size = new System.Drawing.Size(122, 23);
            this.IconSpace.TabIndex = 2;
            // 
            // IconSpaceLabel
            // 
            this.IconSpaceLabel.AutoSize = true;
            this.IconSpaceLabel.Location = new System.Drawing.Point(140, 53);
            this.IconSpaceLabel.Name = "IconSpaceLabel";
            this.IconSpaceLabel.Size = new System.Drawing.Size(63, 15);
            this.IconSpaceLabel.TabIndex = 1;
            this.IconSpaceLabel.Text = "Icon space";
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.IconSpaceLabel);
            this.Controls.Add(this.IconSpace);
            this.Controls.Add(this.IconSize);
            this.Controls.Add(this.IconSizeLabel);
            this.Controls.Add(this.SkinCombo);
            this.Controls.Add(this.SkinLabel);
            this.Name = "SettingsWindow";
            this.Text = "SettingsWindow";
            ((System.ComponentModel.ISupportInitialize)(this.IconSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IconSpace)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox SkinCombo;
        private System.Windows.Forms.Label SkinLabel;
        private System.Windows.Forms.NumericUpDown IconSize;
        private System.Windows.Forms.Label IconSizeLabel;
        private System.Windows.Forms.NumericUpDown IconSpace;
        private System.Windows.Forms.Label IconSpaceLabel;
    }
}