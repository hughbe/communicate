using System.ComponentModel;
using System.Windows.Forms;

namespace Demo.Bonjour
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
            this.sendTextButton = new System.Windows.Forms.Button();
            this.sendFileButton = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.serializationTextBox = new System.Windows.Forms.TextBox();
            this.serializationCheckBox = new System.Windows.Forms.CheckBox();
            this.serializationGroupBox = new System.Windows.Forms.GroupBox();
            this.sendArrayCheckBox = new System.Windows.Forms.CheckBox();
            this.sendJsonButton = new System.Windows.Forms.Button();
            this.sendXmlButton = new System.Windows.Forms.Button();
            this.sendSoapButton = new System.Windows.Forms.Button();
            this.serializationArrayCount = new System.Windows.Forms.Label();
            this.sendBinaryButton = new System.Windows.Forms.Button();
            this.addToArrayButton = new System.Windows.Forms.Button();
            this.serializationNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.sendGroupBox = new System.Windows.Forms.GroupBox();
            this.connectingGroupBox = new System.Windows.Forms.GroupBox();
            this.txtRecordsGroupBox = new System.Windows.Forms.GroupBox();
            this.txtRecordsListView = new System.Windows.Forms.ListView();
            this.keyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.valueHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.manuallyConnectTextBox = new System.Windows.Forms.TextBox();
            this.manuallyConnectNumbericUpDown = new System.Windows.Forms.NumericUpDown();
            this.manuallyConnectButton = new System.Windows.Forms.Button();
            this.ipAddressLabel = new System.Windows.Forms.Label();
            this.manualGroupBox = new System.Windows.Forms.GroupBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.ipLabel = new System.Windows.Forms.Label();
            this.informationGroupBox = new System.Windows.Forms.GroupBox();
            this.informationListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.serializationGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.serializationNumericUpDown)).BeginInit();
            this.sendGroupBox.SuspendLayout();
            this.connectingGroupBox.SuspendLayout();
            this.txtRecordsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.manuallyConnectNumbericUpDown)).BeginInit();
            this.manualGroupBox.SuspendLayout();
            this.informationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // devicesListBox
            // 
            this.devicesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.devicesListBox.FormattingEnabled = true;
            this.devicesListBox.ItemHeight = 16;
            this.devicesListBox.Location = new System.Drawing.Point(3, 20);
            this.devicesListBox.Name = "devicesListBox";
            this.devicesListBox.Size = new System.Drawing.Size(534, 118);
            this.devicesListBox.TabIndex = 0;
            this.devicesListBox.SelectedIndexChanged += new System.EventHandler(this.devicesListBox_SelectedIndexChanged);
            // 
            // sendTextButton
            // 
            this.sendTextButton.AutoSize = true;
            this.sendTextButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sendTextButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendTextButton.Location = new System.Drawing.Point(10, 56);
            this.sendTextButton.Name = "sendTextButton";
            this.sendTextButton.Size = new System.Drawing.Size(79, 26);
            this.sendTextButton.TabIndex = 1;
            this.sendTextButton.Text = "Send Text";
            this.sendTextButton.UseVisualStyleBackColor = true;
            this.sendTextButton.Click += new System.EventHandler(this.sendTextButton_Click);
            // 
            // sendFileButton
            // 
            this.sendFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.sendFileButton.AutoSize = true;
            this.sendFileButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sendFileButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendFileButton.Location = new System.Drawing.Point(719, 56);
            this.sendFileButton.Name = "sendFileButton";
            this.sendFileButton.Size = new System.Drawing.Size(75, 26);
            this.sendFileButton.TabIndex = 2;
            this.sendFileButton.Text = "Send File";
            this.sendFileButton.UseVisualStyleBackColor = true;
            this.sendFileButton.Click += new System.EventHandler(this.sendFileButton_Click);
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox.Location = new System.Drawing.Point(9, 28);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(784, 22);
            this.textBox.TabIndex = 3;
            this.textBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyUp);
            // 
            // serializationTextBox
            // 
            this.serializationTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serializationTextBox.Location = new System.Drawing.Point(6, 23);
            this.serializationTextBox.Name = "serializationTextBox";
            this.serializationTextBox.Size = new System.Drawing.Size(380, 22);
            this.serializationTextBox.TabIndex = 4;
            // 
            // serializationCheckBox
            // 
            this.serializationCheckBox.AutoSize = true;
            this.serializationCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serializationCheckBox.Location = new System.Drawing.Point(392, 29);
            this.serializationCheckBox.Name = "serializationCheckBox";
            this.serializationCheckBox.Size = new System.Drawing.Size(15, 14);
            this.serializationCheckBox.TabIndex = 5;
            this.serializationCheckBox.UseVisualStyleBackColor = true;
            // 
            // serializationGroupBox
            // 
            this.serializationGroupBox.Controls.Add(this.sendArrayCheckBox);
            this.serializationGroupBox.Controls.Add(this.sendJsonButton);
            this.serializationGroupBox.Controls.Add(this.sendXmlButton);
            this.serializationGroupBox.Controls.Add(this.sendSoapButton);
            this.serializationGroupBox.Controls.Add(this.serializationArrayCount);
            this.serializationGroupBox.Controls.Add(this.sendBinaryButton);
            this.serializationGroupBox.Controls.Add(this.addToArrayButton);
            this.serializationGroupBox.Controls.Add(this.serializationNumericUpDown);
            this.serializationGroupBox.Controls.Add(this.serializationTextBox);
            this.serializationGroupBox.Controls.Add(this.serializationCheckBox);
            this.serializationGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serializationGroupBox.Location = new System.Drawing.Point(11, 410);
            this.serializationGroupBox.Name = "serializationGroupBox";
            this.serializationGroupBox.Size = new System.Drawing.Size(799, 141);
            this.serializationGroupBox.TabIndex = 6;
            this.serializationGroupBox.TabStop = false;
            this.serializationGroupBox.Text = "Serialization";
            // 
            // sendArrayCheckBox
            // 
            this.sendArrayCheckBox.AutoSize = true;
            this.sendArrayCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendArrayCheckBox.Location = new System.Drawing.Point(652, 107);
            this.sendArrayCheckBox.Name = "sendArrayCheckBox";
            this.sendArrayCheckBox.Size = new System.Drawing.Size(113, 20);
            this.sendArrayCheckBox.TabIndex = 15;
            this.sendArrayCheckBox.Text = "Send As Array";
            this.sendArrayCheckBox.UseVisualStyleBackColor = true;
            this.sendArrayCheckBox.CheckedChanged += new System.EventHandler(this.sendArrayCheckBox_CheckedChanged);
            // 
            // sendJsonButton
            // 
            this.sendJsonButton.AutoSize = true;
            this.sendJsonButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendJsonButton.Location = new System.Drawing.Point(7, 101);
            this.sendJsonButton.Name = "sendJsonButton";
            this.sendJsonButton.Size = new System.Drawing.Size(155, 34);
            this.sendJsonButton.TabIndex = 7;
            this.sendJsonButton.Text = "Send JSON";
            this.sendJsonButton.UseVisualStyleBackColor = true;
            this.sendJsonButton.Click += new System.EventHandler(this.sendJsonButton_Click);
            // 
            // sendXmlButton
            // 
            this.sendXmlButton.AutoSize = true;
            this.sendXmlButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendXmlButton.Location = new System.Drawing.Point(169, 101);
            this.sendXmlButton.Name = "sendXmlButton";
            this.sendXmlButton.Size = new System.Drawing.Size(155, 34);
            this.sendXmlButton.TabIndex = 8;
            this.sendXmlButton.Text = "Send XML";
            this.sendXmlButton.UseVisualStyleBackColor = true;
            this.sendXmlButton.Click += new System.EventHandler(this.sendXmlButton_Click);
            // 
            // sendSoapButton
            // 
            this.sendSoapButton.AutoSize = true;
            this.sendSoapButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendSoapButton.Location = new System.Drawing.Point(330, 101);
            this.sendSoapButton.Name = "sendSoapButton";
            this.sendSoapButton.Size = new System.Drawing.Size(155, 34);
            this.sendSoapButton.TabIndex = 9;
            this.sendSoapButton.Text = "Send SOAP";
            this.sendSoapButton.UseVisualStyleBackColor = true;
            this.sendSoapButton.Click += new System.EventHandler(this.sendSoapButton_Click);
            // 
            // serializationArrayCount
            // 
            this.serializationArrayCount.AutoSize = true;
            this.serializationArrayCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serializationArrayCount.Location = new System.Drawing.Point(168, 60);
            this.serializationArrayCount.Name = "serializationArrayCount";
            this.serializationArrayCount.Size = new System.Drawing.Size(75, 16);
            this.serializationArrayCount.TabIndex = 13;
            this.serializationArrayCount.Text = "No Objects";
            // 
            // sendBinaryButton
            // 
            this.sendBinaryButton.AutoSize = true;
            this.sendBinaryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendBinaryButton.Location = new System.Drawing.Point(491, 101);
            this.sendBinaryButton.Name = "sendBinaryButton";
            this.sendBinaryButton.Size = new System.Drawing.Size(155, 34);
            this.sendBinaryButton.TabIndex = 10;
            this.sendBinaryButton.Text = "Send Binary";
            this.sendBinaryButton.UseVisualStyleBackColor = true;
            this.sendBinaryButton.Click += new System.EventHandler(this.sendBinaryButton_Click);
            // 
            // addToArrayButton
            // 
            this.addToArrayButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.addToArrayButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addToArrayButton.Location = new System.Drawing.Point(7, 53);
            this.addToArrayButton.Name = "addToArrayButton";
            this.addToArrayButton.Size = new System.Drawing.Size(155, 34);
            this.addToArrayButton.TabIndex = 13;
            this.addToArrayButton.Text = "Add to Array";
            this.addToArrayButton.UseVisualStyleBackColor = true;
            this.addToArrayButton.Click += new System.EventHandler(this.addToArrayButton_Click);
            // 
            // serializationNumericUpDown
            // 
            this.serializationNumericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serializationNumericUpDown.Location = new System.Drawing.Point(413, 24);
            this.serializationNumericUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.serializationNumericUpDown.Name = "serializationNumericUpDown";
            this.serializationNumericUpDown.Size = new System.Drawing.Size(380, 22);
            this.serializationNumericUpDown.TabIndex = 6;
            this.serializationNumericUpDown.Value = new decimal(new int[] {
            12345,
            0,
            0,
            0});
            // 
            // sendGroupBox
            // 
            this.sendGroupBox.Controls.Add(this.textBox);
            this.sendGroupBox.Controls.Add(this.sendTextButton);
            this.sendGroupBox.Controls.Add(this.sendFileButton);
            this.sendGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendGroupBox.Location = new System.Drawing.Point(11, 312);
            this.sendGroupBox.Name = "sendGroupBox";
            this.sendGroupBox.Size = new System.Drawing.Size(799, 92);
            this.sendGroupBox.TabIndex = 11;
            this.sendGroupBox.TabStop = false;
            this.sendGroupBox.Text = "Sending";
            // 
            // connectingGroupBox
            // 
            this.connectingGroupBox.Controls.Add(this.devicesListBox);
            this.connectingGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connectingGroupBox.Location = new System.Drawing.Point(12, 12);
            this.connectingGroupBox.Name = "connectingGroupBox";
            this.connectingGroupBox.Size = new System.Drawing.Size(540, 141);
            this.connectingGroupBox.TabIndex = 12;
            this.connectingGroupBox.TabStop = false;
            this.connectingGroupBox.Text = "Connect to Service";
            // 
            // txtRecordsGroupBox
            // 
            this.txtRecordsGroupBox.Controls.Add(this.txtRecordsListView);
            this.txtRecordsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRecordsGroupBox.Location = new System.Drawing.Point(558, 12);
            this.txtRecordsGroupBox.Name = "txtRecordsGroupBox";
            this.txtRecordsGroupBox.Size = new System.Drawing.Size(253, 141);
            this.txtRecordsGroupBox.TabIndex = 13;
            this.txtRecordsGroupBox.TabStop = false;
            this.txtRecordsGroupBox.Text = "Txt Records";
            // 
            // txtRecordsListView
            // 
            this.txtRecordsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.keyHeader,
            this.valueHeader});
            this.txtRecordsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRecordsListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRecordsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.txtRecordsListView.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtRecordsListView.Location = new System.Drawing.Point(3, 20);
            this.txtRecordsListView.Name = "txtRecordsListView";
            this.txtRecordsListView.Size = new System.Drawing.Size(247, 118);
            this.txtRecordsListView.TabIndex = 0;
            this.txtRecordsListView.UseCompatibleStateImageBehavior = false;
            this.txtRecordsListView.View = System.Windows.Forms.View.Details;
            // 
            // keyHeader
            // 
            this.keyHeader.Text = "Key";
            this.keyHeader.Width = 70;
            // 
            // valueHeader
            // 
            this.valueHeader.Text = "Value";
            this.valueHeader.Width = 122;
            // 
            // manuallyConnectTextBox
            // 
            this.manuallyConnectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.manuallyConnectTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manuallyConnectTextBox.Location = new System.Drawing.Point(76, 30);
            this.manuallyConnectTextBox.Name = "manuallyConnectTextBox";
            this.manuallyConnectTextBox.Size = new System.Drawing.Size(458, 22);
            this.manuallyConnectTextBox.TabIndex = 4;
            // 
            // manuallyConnectNumbericUpDown
            // 
            this.manuallyConnectNumbericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manuallyConnectNumbericUpDown.Location = new System.Drawing.Point(76, 60);
            this.manuallyConnectNumbericUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.manuallyConnectNumbericUpDown.Name = "manuallyConnectNumbericUpDown";
            this.manuallyConnectNumbericUpDown.Size = new System.Drawing.Size(458, 22);
            this.manuallyConnectNumbericUpDown.TabIndex = 16;
            this.manuallyConnectNumbericUpDown.Value = new decimal(new int[] {
            12345,
            0,
            0,
            0});
            // 
            // manuallyConnectButton
            // 
            this.manuallyConnectButton.AutoSize = true;
            this.manuallyConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manuallyConnectButton.Location = new System.Drawing.Point(76, 88);
            this.manuallyConnectButton.Name = "manuallyConnectButton";
            this.manuallyConnectButton.Size = new System.Drawing.Size(458, 28);
            this.manuallyConnectButton.TabIndex = 17;
            this.manuallyConnectButton.Text = "Manually Connect";
            this.manuallyConnectButton.UseVisualStyleBackColor = true;
            this.manuallyConnectButton.Click += new System.EventHandler(this.manuallyConnectButton_Click);
            // 
            // ipAddressLabel
            // 
            this.ipAddressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ipAddressLabel.Location = new System.Drawing.Point(78, 121);
            this.ipAddressLabel.Name = "ipAddressLabel";
            this.ipAddressLabel.Size = new System.Drawing.Size(456, 19);
            this.ipAddressLabel.TabIndex = 16;
            this.ipAddressLabel.Text = "No Network Connection";
            this.ipAddressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // manualGroupBox
            // 
            this.manualGroupBox.Controls.Add(this.portLabel);
            this.manualGroupBox.Controls.Add(this.ipLabel);
            this.manualGroupBox.Controls.Add(this.ipAddressLabel);
            this.manualGroupBox.Controls.Add(this.manuallyConnectTextBox);
            this.manualGroupBox.Controls.Add(this.manuallyConnectButton);
            this.manualGroupBox.Controls.Add(this.manuallyConnectNumbericUpDown);
            this.manualGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manualGroupBox.Location = new System.Drawing.Point(12, 159);
            this.manualGroupBox.Name = "manualGroupBox";
            this.manualGroupBox.Size = new System.Drawing.Size(540, 147);
            this.manualGroupBox.TabIndex = 18;
            this.manualGroupBox.TabStop = false;
            this.manualGroupBox.Text = "Manually Connect";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.portLabel.Location = new System.Drawing.Point(30, 62);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(35, 16);
            this.portLabel.TabIndex = 19;
            this.portLabel.Text = "Port:";
            this.portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ipLabel
            // 
            this.ipLabel.AutoSize = true;
            this.ipLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ipLabel.Location = new System.Drawing.Point(45, 33);
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.Size = new System.Drawing.Size(23, 16);
            this.ipLabel.TabIndex = 18;
            this.ipLabel.Text = "IP:";
            this.ipLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // informationGroupBox
            // 
            this.informationGroupBox.Controls.Add(this.informationListView);
            this.informationGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.informationGroupBox.Location = new System.Drawing.Point(561, 165);
            this.informationGroupBox.Name = "informationGroupBox";
            this.informationGroupBox.Size = new System.Drawing.Size(253, 141);
            this.informationGroupBox.TabIndex = 14;
            this.informationGroupBox.TabStop = false;
            this.informationGroupBox.Text = "Information";
            // 
            // informationListView
            // 
            this.informationListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.informationListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.informationListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.informationListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.informationListView.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.informationListView.Location = new System.Drawing.Point(3, 20);
            this.informationListView.Name = "informationListView";
            this.informationListView.Size = new System.Drawing.Size(247, 118);
            this.informationListView.TabIndex = 0;
            this.informationListView.UseCompatibleStateImageBehavior = false;
            this.informationListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Key";
            this.columnHeader1.Width = 70;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 122;
            // 
            // DemoClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 556);
            this.Controls.Add(this.informationGroupBox);
            this.Controls.Add(this.manualGroupBox);
            this.Controls.Add(this.txtRecordsGroupBox);
            this.Controls.Add(this.connectingGroupBox);
            this.Controls.Add(this.sendGroupBox);
            this.Controls.Add(this.serializationGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DemoClient";
            this.Text = "DemoClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DemoClient_FormClosing);
            this.Load += new System.EventHandler(this.DemoClient_Load);
            this.serializationGroupBox.ResumeLayout(false);
            this.serializationGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.serializationNumericUpDown)).EndInit();
            this.sendGroupBox.ResumeLayout(false);
            this.sendGroupBox.PerformLayout();
            this.connectingGroupBox.ResumeLayout(false);
            this.txtRecordsGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.manuallyConnectNumbericUpDown)).EndInit();
            this.manualGroupBox.ResumeLayout(false);
            this.manualGroupBox.PerformLayout();
            this.informationGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox devicesListBox;
        private Button sendTextButton;
        private Button sendFileButton;
        private TextBox textBox;
        private TextBox serializationTextBox;
        private CheckBox serializationCheckBox;
        private GroupBox serializationGroupBox;
        private NumericUpDown serializationNumericUpDown;
        private Button sendSoapButton;
        private Button sendXmlButton;
        private Button sendJsonButton;
        private Button sendBinaryButton;
        private GroupBox sendGroupBox;
        private GroupBox connectingGroupBox;
        private Button addToArrayButton;
        private Label serializationArrayCount;
        private GroupBox txtRecordsGroupBox;
        private ListView txtRecordsListView;
        private ColumnHeader keyHeader;
        private ColumnHeader valueHeader;
        private CheckBox sendArrayCheckBox;
        private Button manuallyConnectButton;
        private NumericUpDown manuallyConnectNumbericUpDown;
        private TextBox manuallyConnectTextBox;
        private Label ipAddressLabel;
        private GroupBox manualGroupBox;
        private Label portLabel;
        private Label ipLabel;
        private GroupBox informationGroupBox;
        private ListView informationListView;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
    }
}