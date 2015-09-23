using System.ComponentModel;
using System.Windows.Forms;

namespace Demo.Bonjour
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
                components = null;
                historyListBox.Dispose();
                historyListBox = null;

                _server.Dispose();
                _server = null;
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
            this.historyListBox = new System.Windows.Forms.ListBox();
            this.textBox = new System.Windows.Forms.TextBox();
            this.sendTextButton = new System.Windows.Forms.Button();
            this.receivedPictureBox = new System.Windows.Forms.PictureBox();
            this.historyGroupBox = new System.Windows.Forms.GroupBox();
            this.imageGroupBox = new System.Windows.Forms.GroupBox();
            this.serializedGroupBox = new System.Windows.Forms.GroupBox();
            this.property3Label = new System.Windows.Forms.Label();
            this.property2Label = new System.Windows.Forms.Label();
            this.property1Label = new System.Windows.Forms.Label();
            this.sendingGroupBox = new System.Windows.Forms.GroupBox();
            this.saveFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.receivedArrayListBox = new System.Windows.Forms.ListBox();
            this.receivingProgressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.receivedPictureBox)).BeginInit();
            this.historyGroupBox.SuspendLayout();
            this.imageGroupBox.SuspendLayout();
            this.serializedGroupBox.SuspendLayout();
            this.sendingGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // historyListBox
            // 
            this.historyListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.historyListBox.FormattingEnabled = true;
            this.historyListBox.ItemHeight = 18;
            this.historyListBox.Location = new System.Drawing.Point(3, 20);
            this.historyListBox.Name = "historyListBox";
            this.historyListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.historyListBox.Size = new System.Drawing.Size(344, 305);
            this.historyListBox.TabIndex = 0;
            // 
            // textBox
            // 
            this.textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox.Location = new System.Drawing.Point(6, 25);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(325, 24);
            this.textBox.TabIndex = 1;
            this.textBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyUp);
            // 
            // sendTextButton
            // 
            this.sendTextButton.AutoSize = true;
            this.sendTextButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sendTextButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendTextButton.Location = new System.Drawing.Point(6, 56);
            this.sendTextButton.Name = "sendTextButton";
            this.sendTextButton.Size = new System.Drawing.Size(52, 28);
            this.sendTextButton.TabIndex = 2;
            this.sendTextButton.Text = "Send";
            this.sendTextButton.UseVisualStyleBackColor = true;
            this.sendTextButton.Click += new System.EventHandler(this.sendTextButton_Clicked);
            // 
            // receivedPictureBox
            // 
            this.receivedPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.receivedPictureBox.Location = new System.Drawing.Point(3, 20);
            this.receivedPictureBox.Name = "receivedPictureBox";
            this.receivedPictureBox.Size = new System.Drawing.Size(325, 308);
            this.receivedPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.receivedPictureBox.TabIndex = 3;
            this.receivedPictureBox.TabStop = false;
            // 
            // historyGroupBox
            // 
            this.historyGroupBox.Controls.Add(this.historyListBox);
            this.historyGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.historyGroupBox.Location = new System.Drawing.Point(13, 12);
            this.historyGroupBox.Name = "historyGroupBox";
            this.historyGroupBox.Size = new System.Drawing.Size(350, 328);
            this.historyGroupBox.TabIndex = 4;
            this.historyGroupBox.TabStop = false;
            this.historyGroupBox.Text = "History";
            // 
            // imageGroupBox
            // 
            this.imageGroupBox.Controls.Add(this.receivedPictureBox);
            this.imageGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imageGroupBox.Location = new System.Drawing.Point(369, 12);
            this.imageGroupBox.Name = "imageGroupBox";
            this.imageGroupBox.Size = new System.Drawing.Size(331, 331);
            this.imageGroupBox.TabIndex = 5;
            this.imageGroupBox.TabStop = false;
            this.imageGroupBox.Text = "Received Image";
            // 
            // serializedGroupBox
            // 
            this.serializedGroupBox.Controls.Add(this.property3Label);
            this.serializedGroupBox.Controls.Add(this.property2Label);
            this.serializedGroupBox.Controls.Add(this.property1Label);
            this.serializedGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serializedGroupBox.Location = new System.Drawing.Point(375, 352);
            this.serializedGroupBox.Name = "serializedGroupBox";
            this.serializedGroupBox.Size = new System.Drawing.Size(325, 121);
            this.serializedGroupBox.TabIndex = 1;
            this.serializedGroupBox.TabStop = false;
            this.serializedGroupBox.Text = "Serialized";
            // 
            // property3Label
            // 
            this.property3Label.AutoSize = true;
            this.property3Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.property3Label.Location = new System.Drawing.Point(7, 86);
            this.property3Label.Name = "property3Label";
            this.property3Label.Size = new System.Drawing.Size(85, 20);
            this.property3Label.TabIndex = 2;
            this.property3Label.Text = "Property 3:";
            // 
            // property2Label
            // 
            this.property2Label.AutoSize = true;
            this.property2Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.property2Label.Location = new System.Drawing.Point(7, 60);
            this.property2Label.Name = "property2Label";
            this.property2Label.Size = new System.Drawing.Size(85, 20);
            this.property2Label.TabIndex = 1;
            this.property2Label.Text = "Property 2:";
            // 
            // property1Label
            // 
            this.property1Label.AutoSize = true;
            this.property1Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.property1Label.Location = new System.Drawing.Point(7, 34);
            this.property1Label.Name = "property1Label";
            this.property1Label.Size = new System.Drawing.Size(85, 20);
            this.property1Label.TabIndex = 0;
            this.property1Label.Text = "Property 1:";
            // 
            // sendingGroupBox
            // 
            this.sendingGroupBox.Controls.Add(this.saveFilesCheckBox);
            this.sendingGroupBox.Controls.Add(this.textBox);
            this.sendingGroupBox.Controls.Add(this.sendTextButton);
            this.sendingGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendingGroupBox.Location = new System.Drawing.Point(19, 353);
            this.sendingGroupBox.Name = "sendingGroupBox";
            this.sendingGroupBox.Size = new System.Drawing.Size(344, 120);
            this.sendingGroupBox.TabIndex = 6;
            this.sendingGroupBox.TabStop = false;
            this.sendingGroupBox.Text = "Send";
            // 
            // saveFilesCheckBox
            // 
            this.saveFilesCheckBox.AutoSize = true;
            this.saveFilesCheckBox.Checked = true;
            this.saveFilesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.saveFilesCheckBox.Location = new System.Drawing.Point(6, 90);
            this.saveFilesCheckBox.Name = "saveFilesCheckBox";
            this.saveFilesCheckBox.Size = new System.Drawing.Size(255, 24);
            this.saveFilesCheckBox.TabIndex = 9;
            this.saveFilesCheckBox.Text = "Save received files automatically";
            this.saveFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.receivedArrayListBox);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(715, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(298, 461);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Received Array";
            // 
            // receivedArrayListBox
            // 
            this.receivedArrayListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.receivedArrayListBox.FormattingEnabled = true;
            this.receivedArrayListBox.ItemHeight = 18;
            this.receivedArrayListBox.Location = new System.Drawing.Point(3, 20);
            this.receivedArrayListBox.Name = "receivedArrayListBox";
            this.receivedArrayListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.receivedArrayListBox.Size = new System.Drawing.Size(292, 438);
            this.receivedArrayListBox.TabIndex = 0;
            // 
            // receivingProgressBar
            // 
            this.receivingProgressBar.Location = new System.Drawing.Point(22, 479);
            this.receivingProgressBar.Name = "receivingProgressBar";
            this.receivingProgressBar.Size = new System.Drawing.Size(988, 23);
            this.receivingProgressBar.TabIndex = 8;
            // 
            // DemoServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 509);
            this.Controls.Add(this.receivingProgressBar);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.sendingGroupBox);
            this.Controls.Add(this.serializedGroupBox);
            this.Controls.Add(this.imageGroupBox);
            this.Controls.Add(this.historyGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DemoServer";
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DemoServer_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.receivedPictureBox)).EndInit();
            this.historyGroupBox.ResumeLayout(false);
            this.imageGroupBox.ResumeLayout(false);
            this.serializedGroupBox.ResumeLayout(false);
            this.serializedGroupBox.PerformLayout();
            this.sendingGroupBox.ResumeLayout(false);
            this.sendingGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox historyListBox;
        private TextBox textBox;
        private Button sendTextButton;
        private PictureBox receivedPictureBox;
        private GroupBox historyGroupBox;
        private GroupBox imageGroupBox;
        private GroupBox serializedGroupBox;
        private Label property3Label;
        private Label property2Label;
        private Label property1Label;
        private GroupBox sendingGroupBox;
        private GroupBox groupBox1;
        private ListBox receivedArrayListBox;
        private ProgressBar receivingProgressBar;
        private CheckBox saveFilesCheckBox;
    }
}

