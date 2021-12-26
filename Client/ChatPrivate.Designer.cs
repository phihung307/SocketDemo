namespace Client {
    partial class ChatPrivate {
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
            this.listViewMsg = new System.Windows.Forms.ListView();
            this.textBoxChat = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listViewMsg
            // 
            this.listViewMsg.HideSelection = false;
            this.listViewMsg.Location = new System.Drawing.Point(164, 89);
            this.listViewMsg.Name = "listViewMsg";
            this.listViewMsg.Size = new System.Drawing.Size(473, 231);
            this.listViewMsg.TabIndex = 10;
            this.listViewMsg.UseCompatibleStateImageBehavior = false;
            this.listViewMsg.View = System.Windows.Forms.View.List;
            // 
            // textBoxChat
            // 
            this.textBoxChat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxChat.Location = new System.Drawing.Point(164, 326);
            this.textBoxChat.Multiline = true;
            this.textBoxChat.Name = "textBoxChat";
            this.textBoxChat.Size = new System.Drawing.Size(382, 35);
            this.textBoxChat.TabIndex = 9;
            // 
            // buttonSend
            // 
            this.buttonSend.Location = new System.Drawing.Point(562, 326);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(75, 35);
            this.buttonSend.TabIndex = 8;
            this.buttonSend.Text = "Send";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // ChatPrivate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listViewMsg);
            this.Controls.Add(this.textBoxChat);
            this.Controls.Add(this.buttonSend);
            this.Name = "ChatPrivate";
            this.Text = "ChatPrivate";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewMsg;
        private System.Windows.Forms.TextBox textBoxChat;
        private System.Windows.Forms.Button buttonSend;
    }
}