namespace SpaceWarsView
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.Disconnect = new System.Windows.Forms.Button();
            this.Connect = new System.Windows.Forms.Button();
            this.serverAddr = new System.Windows.Forms.TextBox();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.NameLabel = new System.Windows.Forms.Label();
            this.spacewarspicture = new System.Windows.Forms.PictureBox();
            this.ScoreBoardPicture = new System.Windows.Forms.PictureBox();
            this.HelpButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spacewarspicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScoreBoardPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // Disconnect
            // 
            this.Disconnect.Location = new System.Drawing.Point(12, 5);
            this.Disconnect.Name = "Disconnect";
            this.Disconnect.Size = new System.Drawing.Size(75, 23);
            this.Disconnect.TabIndex = 0;
            this.Disconnect.Text = "Disconnect";
            this.Disconnect.UseVisualStyleBackColor = true;
            this.Disconnect.Click += new System.EventHandler(this.Disconnect_Click);
            // 
            // Connect
            // 
            this.Connect.Location = new System.Drawing.Point(485, 126);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(75, 23);
            this.Connect.TabIndex = 1;
            this.Connect.Text = "Connect";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // serverAddr
            // 
            this.serverAddr.Location = new System.Drawing.Point(69, 127);
            this.serverAddr.Name = "serverAddr";
            this.serverAddr.Size = new System.Drawing.Size(138, 20);
            this.serverAddr.TabIndex = 2;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(306, 127);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(138, 20);
            this.nameTextBox.TabIndex = 3;
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ServerLabel.Location = new System.Drawing.Point(22, 130);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(41, 13);
            this.ServerLabel.TabIndex = 4;
            this.ServerLabel.Text = "Server:";
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(262, 130);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(38, 13);
            this.NameLabel.TabIndex = 5;
            this.NameLabel.Text = "Name:";
            // 
            // spacewarspicture
            // 
            this.spacewarspicture.Image = global::SpaceWarsView.Properties.Resources.SpaceWarsLogo;
            this.spacewarspicture.Location = new System.Drawing.Point(12, 28);
            this.spacewarspicture.Name = "spacewarspicture";
            this.spacewarspicture.Size = new System.Drawing.Size(577, 92);
            this.spacewarspicture.TabIndex = 6;
            this.spacewarspicture.TabStop = false;
            // 
            // ScoreBoardPicture
            // 
            this.ScoreBoardPicture.Image = ((System.Drawing.Image)(resources.GetObject("ScoreBoardPicture.Image")));
            this.ScoreBoardPicture.Location = new System.Drawing.Point(435, 28);
            this.ScoreBoardPicture.Name = "ScoreBoardPicture";
            this.ScoreBoardPicture.Size = new System.Drawing.Size(187, 74);
            this.ScoreBoardPicture.TabIndex = 7;
            this.ScoreBoardPicture.TabStop = false;
            this.ScoreBoardPicture.Visible = false;
            // 
            // HelpButton
            // 
            this.HelpButton.Location = new System.Drawing.Point(103, 5);
            this.HelpButton.Name = "HelpButton";
            this.HelpButton.Size = new System.Drawing.Size(75, 23);
            this.HelpButton.TabIndex = 8;
            this.HelpButton.Text = "Help";
            this.HelpButton.UseVisualStyleBackColor = true;
            this.HelpButton.Click += new System.EventHandler(this.HelpButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 161);
            this.Controls.Add(this.HelpButton);
            this.Controls.Add(this.ScoreBoardPicture);
            this.Controls.Add(this.spacewarspicture);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.ServerLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.serverAddr);
            this.Controls.Add(this.Connect);
            this.Controls.Add(this.Disconnect);
            this.Name = "Form1";
            this.Text = "Form1";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.spacewarspicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScoreBoardPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Disconnect;
        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.TextBox serverAddr;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label ServerLabel;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.PictureBox spacewarspicture;
        private System.Windows.Forms.PictureBox ScoreBoardPicture;
        private System.Windows.Forms.Button HelpButton;
    }
}

