namespace RealTimeProject
{
    partial class Server
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.StopGameButton = new System.Windows.Forms.Button();
            this.ResetGameButton = new System.Windows.Forms.Button();
            this.StartGameButton = new System.Windows.Forms.Button();
            this.PlayerListLabel = new System.Windows.Forms.Label();
            this.InfoTextLabel = new System.Windows.Forms.Label();
            this.GameLoopTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.StopGameButton);
            this.panel1.Controls.Add(this.ResetGameButton);
            this.panel1.Controls.Add(this.StartGameButton);
            this.panel1.Controls.Add(this.PlayerListLabel);
            this.panel1.Controls.Add(this.InfoTextLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 450);
            this.panel1.TabIndex = 0;
            // 
            // StopGameButton
            // 
            this.StopGameButton.Enabled = false;
            this.StopGameButton.Location = new System.Drawing.Point(480, 356);
            this.StopGameButton.Name = "StopGameButton";
            this.StopGameButton.Size = new System.Drawing.Size(133, 32);
            this.StopGameButton.TabIndex = 4;
            this.StopGameButton.Text = "Stop Game";
            this.StopGameButton.UseVisualStyleBackColor = true;
            this.StopGameButton.Click += new System.EventHandler(this.StopGameButton_Click);
            // 
            // ResetGameButton
            // 
            this.ResetGameButton.Enabled = false;
            this.ResetGameButton.Location = new System.Drawing.Point(341, 356);
            this.ResetGameButton.Name = "ResetGameButton";
            this.ResetGameButton.Size = new System.Drawing.Size(133, 32);
            this.ResetGameButton.TabIndex = 3;
            this.ResetGameButton.Text = "Reset Game";
            this.ResetGameButton.UseVisualStyleBackColor = true;
            this.ResetGameButton.Click += new System.EventHandler(this.ResetGameButton_Click);
            // 
            // StartGameButton
            // 
            this.StartGameButton.Enabled = false;
            this.StartGameButton.Location = new System.Drawing.Point(202, 356);
            this.StartGameButton.Name = "StartGameButton";
            this.StartGameButton.Size = new System.Drawing.Size(133, 32);
            this.StartGameButton.TabIndex = 2;
            this.StartGameButton.Text = "Start Game";
            this.StartGameButton.UseVisualStyleBackColor = true;
            this.StartGameButton.Click += new System.EventHandler(this.StartGameButton_Click);
            // 
            // PlayerListLabel
            // 
            this.PlayerListLabel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.PlayerListLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.PlayerListLabel.Location = new System.Drawing.Point(83, 119);
            this.PlayerListLabel.Name = "PlayerListLabel";
            this.PlayerListLabel.Size = new System.Drawing.Size(636, 223);
            this.PlayerListLabel.TabIndex = 1;
            this.PlayerListLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // InfoTextLabel
            // 
            this.InfoTextLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoTextLabel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.InfoTextLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.InfoTextLabel.Location = new System.Drawing.Point(219, 26);
            this.InfoTextLabel.Name = "InfoTextLabel";
            this.InfoTextLabel.Size = new System.Drawing.Size(352, 78);
            this.InfoTextLabel.TabIndex = 0;
            this.InfoTextLabel.Text = "Info Text";
            this.InfoTextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GameLoopTimer
            // 
            this.GameLoopTimer.Interval = 15;
            this.GameLoopTimer.Tick += new System.EventHandler(this.GameLoopTimer_Tick);
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Name = "Server";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panel1;
        private Label InfoTextLabel;
        private Label PlayerListLabel;
        private Button StartGameButton;
        private Button StopGameButton;
        private Button ResetGameButton;
        private System.Windows.Forms.Timer GameLoopTimer;
    }
}