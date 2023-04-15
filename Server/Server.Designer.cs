﻿namespace RealTimeProject
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
            this.GameLoopTimer = new System.Windows.Forms.Timer(this.components);
            this.StartGameButton = new System.Windows.Forms.Button();
            this.ResetGameButton = new System.Windows.Forms.Button();
            this.StopGameButton = new System.Windows.Forms.Button();
            this.PlayerListLabel = new System.Windows.Forms.Label();
            this.InfoTextLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // GameLoopTimer
            // 
            this.GameLoopTimer.Interval = 15;
            this.GameLoopTimer.Tick += new System.EventHandler(this.GameLoopTimer_Tick);
            // 
            // StartGameButton
            // 
            this.StartGameButton.Enabled = false;
            this.StartGameButton.Location = new System.Drawing.Point(297, 539);
            this.StartGameButton.Name = "StartGameButton";
            this.StartGameButton.Size = new System.Drawing.Size(129, 106);
            this.StartGameButton.TabIndex = 2;
            this.StartGameButton.Text = "Start Game";
            this.StartGameButton.UseVisualStyleBackColor = true;
            this.StartGameButton.Click += new System.EventHandler(this.StartGameButton_Click);
            // 
            // ResetGameButton
            // 
            this.ResetGameButton.Enabled = false;
            this.ResetGameButton.Location = new System.Drawing.Point(442, 539);
            this.ResetGameButton.Name = "ResetGameButton";
            this.ResetGameButton.Size = new System.Drawing.Size(129, 106);
            this.ResetGameButton.TabIndex = 3;
            this.ResetGameButton.Text = "Reset Game";
            this.ResetGameButton.UseVisualStyleBackColor = true;
            this.ResetGameButton.Click += new System.EventHandler(this.ResetGameButton_Click);
            // 
            // StopGameButton
            // 
            this.StopGameButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.StopGameButton.Enabled = false;
            this.StopGameButton.Location = new System.Drawing.Point(590, 539);
            this.StopGameButton.Name = "StopGameButton";
            this.StopGameButton.Size = new System.Drawing.Size(129, 105);
            this.StopGameButton.TabIndex = 4;
            this.StopGameButton.Text = "Stop Game";
            this.StopGameButton.UseVisualStyleBackColor = true;
            this.StopGameButton.Click += new System.EventHandler(this.StopGameButton_Click);
            // 
            // PlayerListLabel
            // 
            this.PlayerListLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayerListLabel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.PlayerListLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.PlayerListLabel.Location = new System.Drawing.Point(58, 153);
            this.PlayerListLabel.Name = "PlayerListLabel";
            this.PlayerListLabel.Size = new System.Drawing.Size(883, 360);
            this.PlayerListLabel.TabIndex = 1;
            // 
            // InfoTextLabel
            // 
            this.InfoTextLabel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.InfoTextLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.InfoTextLabel.Location = new System.Drawing.Point(297, 33);
            this.InfoTextLabel.Name = "InfoTextLabel";
            this.InfoTextLabel.Size = new System.Drawing.Size(401, 106);
            this.InfoTextLabel.TabIndex = 0;
            this.InfoTextLabel.Text = "Info Text";
            this.InfoTextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(989, 668);
            this.Controls.Add(this.ResetGameButton);
            this.Controls.Add(this.StartGameButton);
            this.Controls.Add(this.StopGameButton);
            this.Controls.Add(this.PlayerListLabel);
            this.Controls.Add(this.InfoTextLabel);
            this.Name = "Server";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer GameLoopTimer;
        private Button StartGameButton;
        private Button ResetGameButton;
        private Button StopGameButton;
        private Label PlayerListLabel;
        private Label InfoTextLabel;
    }
}