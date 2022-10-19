namespace RealTimeProject
{
    partial class Graphics
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
            this.SocketTimer = new System.Windows.Forms.Timer(this.components);
            this.Player1Label = new System.Windows.Forms.Label();
            this.Player2Label = new System.Windows.Forms.Label();
            this.Bullet1Label = new System.Windows.Forms.Label();
            this.Bullet2Label = new System.Windows.Forms.Label();
            this.ScoreLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SocketTimer
            // 
            this.SocketTimer.Enabled = true;
            this.SocketTimer.Interval = 17;
            this.SocketTimer.Tick += new System.EventHandler(this.SocketTimer_Tick);
            // 
            // Player1Label
            // 
            this.Player1Label.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Player1Label.Location = new System.Drawing.Point(50, 267);
            this.Player1Label.Name = "Player1Label";
            this.Player1Label.Size = new System.Drawing.Size(50, 50);
            this.Player1Label.TabIndex = 0;
            this.Player1Label.Text = "O";
            this.Player1Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Player2Label
            // 
            this.Player2Label.BackColor = System.Drawing.SystemColors.Desktop;
            this.Player2Label.Location = new System.Drawing.Point(700, 141);
            this.Player2Label.Name = "Player2Label";
            this.Player2Label.Size = new System.Drawing.Size(50, 50);
            this.Player2Label.TabIndex = 1;
            this.Player2Label.Text = "O";
            this.Player2Label.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // Bullet1Label
            // 
            this.Bullet1Label.BackColor = System.Drawing.SystemColors.ControlText;
            this.Bullet1Label.Location = new System.Drawing.Point(378, 165);
            this.Bullet1Label.Name = "Bullet1Label";
            this.Bullet1Label.Size = new System.Drawing.Size(14, 14);
            this.Bullet1Label.TabIndex = 2;
            this.Bullet1Label.Visible = false;
            // 
            // Bullet2Label
            // 
            this.Bullet2Label.BackColor = System.Drawing.SystemColors.ControlText;
            this.Bullet2Label.Location = new System.Drawing.Point(378, 277);
            this.Bullet2Label.Name = "Bullet2Label";
            this.Bullet2Label.Size = new System.Drawing.Size(14, 14);
            this.Bullet2Label.TabIndex = 3;
            this.Bullet2Label.Visible = false;
            // 
            // ScoreLabel
            // 
            this.ScoreLabel.AutoSize = true;
            this.ScoreLabel.Location = new System.Drawing.Point(348, 9);
            this.ScoreLabel.Name = "ScoreLabel";
            this.ScoreLabel.Size = new System.Drawing.Size(73, 30);
            this.ScoreLabel.TabIndex = 4;
            this.ScoreLabel.Text = "Blue score: 0\r\nRed score: 0";
            // 
            // Graphics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 450);
            this.Controls.Add(this.ScoreLabel);
            this.Controls.Add(this.Bullet2Label);
            this.Controls.Add(this.Bullet1Label);
            this.Controls.Add(this.Player2Label);
            this.Controls.Add(this.Player1Label);
            this.Enabled = false;
            this.Name = "Graphics";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Graphics_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer SocketTimer;
        private Label Player1Label;
        private Label Player2Label;
        private Label Bullet1Label;
        private Label Bullet2Label;
        private Label ScoreLabel;
    }
}