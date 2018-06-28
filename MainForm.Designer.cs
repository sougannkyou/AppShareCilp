namespace AppShareClip
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                ChangeClipboardChain(this.Handle, nextClipboardViewer);
            }
            base.Dispose(disposing);
        }

        #region Code Windows Form

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripEntryLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.goUpButton = new System.Windows.Forms.Button();
            this.goDownButton = new System.Windows.Forms.Button();
            this.copyButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.clipboardHistoryList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.clearHistoryButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusLabel1,
            this.toolStripEntryLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 422);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(492, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.BackColor = System.Drawing.Color.White;
            this.toolStripStatusLabel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 2);
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(44, 17);
            this.toolStripStatusLabel.Text = "Ready";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(309, 17);
            this.toolStripStatusLabel1.Spring = true;
            // 
            // toolStripEntryLabel
            // 
            this.toolStripEntryLabel.Margin = new System.Windows.Forms.Padding(0, 3, 10, 2);
            this.toolStripEntryLabel.Name = "toolStripEntryLabel";
            this.toolStripEntryLabel.Size = new System.Drawing.Size(111, 17);
            this.toolStripEntryLabel.Text = "No entry selected";
            this.toolStripEntryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStripEntryLabel.ToolTipText = "Indicates which clipboard entry is selected in the list";
            // 
            // goUpButton
            // 
            this.goUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.goUpButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.goUpButton.Location = new System.Drawing.Point(430, 24);
            this.goUpButton.Name = "goUpButton";
            this.goUpButton.Size = new System.Drawing.Size(50, 46);
            this.goUpButton.TabIndex = 2;
            this.goUpButton.Text = "↑\r\n";
            this.goUpButton.UseVisualStyleBackColor = true;
            this.goUpButton.Click += new System.EventHandler(this.GoUpButton_Click);
            // 
            // goDownButton
            // 
            this.goDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.goDownButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.goDownButton.Location = new System.Drawing.Point(430, 102);
            this.goDownButton.Name = "goDownButton";
            this.goDownButton.Size = new System.Drawing.Size(50, 46);
            this.goDownButton.TabIndex = 3;
            this.goDownButton.Text = "↓";
            this.goDownButton.UseVisualStyleBackColor = true;
            this.goDownButton.Click += new System.EventHandler(this.GoDownButton_Click);
            // 
            // copyButton
            // 
            this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.copyButton.Location = new System.Drawing.Point(430, 76);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(50, 21);
            this.copyButton.TabIndex = 4;
            this.copyButton.Text = "Copy";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // clearButton
            // 
            this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.clearButton.Location = new System.Drawing.Point(430, 154);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(50, 21);
            this.clearButton.TabIndex = 5;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.GoDownButton_Click);
            // 
            // clipboardHistoryList
            // 
            this.clipboardHistoryList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clipboardHistoryList.FormattingEnabled = true;
            this.clipboardHistoryList.ItemHeight = 12;
            this.clipboardHistoryList.Location = new System.Drawing.Point(13, 20);
            this.clipboardHistoryList.Name = "clipboardHistoryList";
            this.clipboardHistoryList.Size = new System.Drawing.Size(401, 400);
            this.clipboardHistoryList.TabIndex = 6;
            this.clipboardHistoryList.Tag = "";
            this.clipboardHistoryList.SelectedIndexChanged += new System.EventHandler(this.ClipboardHistoryList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "Clipboard History";
            // 
            // clearHistoryButton
            // 
            this.clearHistoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.clearHistoryButton.Location = new System.Drawing.Point(430, 238);
            this.clearHistoryButton.Name = "clearHistoryButton";
            this.clearHistoryButton.Size = new System.Drawing.Size(50, 42);
            this.clearHistoryButton.TabIndex = 9;
            this.clearHistoryButton.Text = "Delete History";
            this.clearHistoryButton.UseVisualStyleBackColor = true;
            this.clearHistoryButton.Click += new System.EventHandler(this.ClearHistoryButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Enabled = false;
            this.deleteButton.Location = new System.Drawing.Point(430, 213);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(50, 19);
            this.deleteButton.TabIndex = 10;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(492, 444);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.clearHistoryButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.clipboardHistoryList);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.goDownButton);
            this.Controls.Add(this.goUpButton);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(200, 278);
            this.Name = "MainForm";
            this.Text = "NiceClip a0.0.1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Button goUpButton;
        private System.Windows.Forms.Button goDownButton;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.ListBox clipboardHistoryList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button clearHistoryButton;
        private System.Windows.Forms.ToolStripStatusLabel toolStripEntryLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button deleteButton;
    }
}

