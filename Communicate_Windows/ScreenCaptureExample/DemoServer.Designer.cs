using System.ComponentModel;
using System.Windows.Forms;

namespace ScreenCaptureExample
{
    partial class DemoServer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                if (Server != null) { Server.Dispose(); Server = null; }
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
            this.receivedPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.receivedPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // receivedPictureBox
            // 
            this.receivedPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.receivedPictureBox.Location = new System.Drawing.Point(0, 0);
            this.receivedPictureBox.Name = "receivedPictureBox";
            this.receivedPictureBox.Size = new System.Drawing.Size(284, 261);
            this.receivedPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.receivedPictureBox.TabIndex = 0;
            this.receivedPictureBox.TabStop = false;
            // 
            // DemoServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.receivedPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DemoServer";
            this.Text = "Screen";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Screen_Load);
            ((System.ComponentModel.ISupportInitialize)(this.receivedPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox receivedPictureBox;
    }
}

