namespace uk.andyjohnson.HardView2
{
    partial class ImagePreviewForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImagePreviewForm));
            this.SuspendLayout();
            // 
            // ImagePreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(633, 525);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ImagePreviewForm";
            this.Text = "HardView2";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.ImagePreviewForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ImagePreviewForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImagePreviewForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ImagePreviewForm_MouseDown);
            this.MouseEnter += new System.EventHandler(this.ImagePreviewForm_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ImagePreviewForm_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImagePreviewForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ImagePreviewForm_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

