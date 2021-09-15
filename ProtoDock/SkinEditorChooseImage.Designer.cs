namespace ProtoDock
{
    partial class SkinEditorChooseImage
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
            this.Right = new System.Windows.Forms.NumericUpDown();
            this.Bottom = new System.Windows.Forms.NumericUpDown();
            this.Left = new System.Windows.Forms.NumericUpDown();
            this.Top = new System.Windows.Forms.NumericUpDown();
            this.Images = new System.Windows.Forms.ComboBox();
            this.Align = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.Right)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Bottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Left)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Top)).BeginInit();
            this.SuspendLayout();
            // 
            // Right
            // 
            this.Right.Location = new System.Drawing.Point(474, 35);
            this.Right.Name = "Right";
            this.Right.Size = new System.Drawing.Size(66, 23);
            this.Right.TabIndex = 0;
            // 
            // Bottom
            // 
            this.Bottom.Location = new System.Drawing.Point(433, 64);
            this.Bottom.Name = "Bottom";
            this.Bottom.Size = new System.Drawing.Size(66, 23);
            this.Bottom.TabIndex = 0;
            // 
            // Left
            // 
            this.Left.Location = new System.Drawing.Point(402, 35);
            this.Left.Name = "Left";
            this.Left.Size = new System.Drawing.Size(66, 23);
            this.Left.TabIndex = 0;
            // 
            // Top
            // 
            this.Top.Location = new System.Drawing.Point(433, 6);
            this.Top.Name = "Top";
            this.Top.Size = new System.Drawing.Size(66, 23);
            this.Top.TabIndex = 0;
            // 
            // Images
            // 
            this.Images.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Images.FormattingEnabled = true;
            this.Images.Location = new System.Drawing.Point(12, 34);
            this.Images.Name = "Images";
            this.Images.Size = new System.Drawing.Size(384, 23);
            this.Images.TabIndex = 1;
            // 
            // Align
            // 
            this.Align.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Align.FormattingEnabled = true;
            this.Align.Location = new System.Drawing.Point(12, 65);
            this.Align.Name = "Align";
            this.Align.Size = new System.Drawing.Size(384, 23);
            this.Align.TabIndex = 1;
            // 
            // SkinEditorChooseImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(552, 100);
            this.Controls.Add(this.Align);
            this.Controls.Add(this.Images);
            this.Controls.Add(this.Top);
            this.Controls.Add(this.Left);
            this.Controls.Add(this.Bottom);
            this.Controls.Add(this.Right);
            this.MinimizeBox = false;
            this.Name = "SkinEditorChooseImage";
            this.Text = "SkinEditorChooseImage";
            ((System.ComponentModel.ISupportInitialize)(this.Right)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Bottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Left)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Top)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown Right;
        private System.Windows.Forms.NumericUpDown Bottom;
        private System.Windows.Forms.NumericUpDown Left;
        private System.Windows.Forms.NumericUpDown Top;
        private System.Windows.Forms.ComboBox Images;
        private System.Windows.Forms.ComboBox Align;
    }
}