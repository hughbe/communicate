using System.ComponentModel;
using System.Windows.Forms;

namespace ScreenCaptureExample
{
    partial class DemoClient
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
                if (Client != null) { Client.Dispose(); Client = null; }
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
            this.devicesListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // devicesListBox
            // 
            this.devicesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesListBox.FormattingEnabled = true;
            this.devicesListBox.Location = new System.Drawing.Point(0, 0);
            this.devicesListBox.Name = "devicesListBox";
            this.devicesListBox.Size = new System.Drawing.Size(266, 98);
            this.devicesListBox.TabIndex = 0;
            this.devicesListBox.SelectedIndexChanged += new System.EventHandler(this.DevicesListBox_SelectedIndexChanged);
            // 
            // DemoClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 98);
            this.Controls.Add(this.devicesListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "DemoClient";
            this.ShowIcon = false;
            this.Text = "Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DemoClient_Closing);
            this.Load += new System.EventHandler(this.DemoClient_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox devicesListBox;

    }
}

