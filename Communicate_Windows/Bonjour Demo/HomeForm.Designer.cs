using System.ComponentModel;
using System.Windows.Forms;

namespace Demo.Bonjour
{
    partial class HomeForm
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
            this.showClientButton = new System.Windows.Forms.Button();
            this.showServerButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // showClientButton
            // 
            this.showClientButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showClientButton.Location = new System.Drawing.Point(12, 12);
            this.showClientButton.Name = "showClientButton";
            this.showClientButton.Size = new System.Drawing.Size(237, 237);
            this.showClientButton.TabIndex = 0;
            this.showClientButton.Text = "Client";
            this.showClientButton.UseVisualStyleBackColor = true;
            this.showClientButton.Click += new System.EventHandler(this.showClientButton_Click);
            // 
            // showServerButton
            // 
            this.showServerButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showServerButton.Location = new System.Drawing.Point(255, 12);
            this.showServerButton.Name = "showServerButton";
            this.showServerButton.Size = new System.Drawing.Size(237, 237);
            this.showServerButton.TabIndex = 1;
            this.showServerButton.Text = "Server";
            this.showServerButton.UseVisualStyleBackColor = true;
            this.showServerButton.Click += new System.EventHandler(this.showServerButton_Click);
            // 
            // HomeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 261);
            this.Controls.Add(this.showServerButton);
            this.Controls.Add(this.showClientButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "HomeForm";
            this.Text = "HomeForm";
            this.ResumeLayout(false);

        }

        #endregion

        private Button showClientButton;
        private Button showServerButton;
    }
}