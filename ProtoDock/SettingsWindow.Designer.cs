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
            this.Category = new System.Windows.Forms.ListBox();
            this.Content = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // Category
            // 
            this.Category.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Category.FormattingEnabled = true;
            this.Category.IntegralHeight = false;
            this.Category.ItemHeight = 15;
            this.Category.Location = new System.Drawing.Point(0, 0);
            this.Category.Name = "Category";
            this.Category.ScrollAlwaysVisible = true;
            this.Category.Size = new System.Drawing.Size(228, 454);
            this.Category.TabIndex = 0;
            this.Category.SelectedIndexChanged += new System.EventHandler(this.Category_SelectedIndexChanged);
            // 
            // Content
            // 
            this.Content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Content.AutoScroll = true;
            this.Content.Location = new System.Drawing.Point(224, 0);
            this.Content.Name = "Content";
            this.Content.Size = new System.Drawing.Size(570, 454);
            this.Content.TabIndex = 1;
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 454);
            this.Controls.Add(this.Content);
            this.Controls.Add(this.Category);
            this.Name = "SettingsWindow";
            this.Text = "SettingsWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox Category;
        private System.Windows.Forms.Panel Content;
    }
}