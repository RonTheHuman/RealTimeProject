namespace RealTimeProject
{
    partial class Client
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
            this.Player1Label = new System.Windows.Forms.Label();
            this.Player2Label = new System.Windows.Forms.Label();
            this.AttackLabel1 = new System.Windows.Forms.Label();
            this.AttackLabel2 = new System.Windows.Forms.Label();
            this.ScoreLabel = new System.Windows.Forms.Label();
            this.TimeLabel = new System.Windows.Forms.Label();
            this.TimeTimer = new System.Windows.Forms.Timer(this.components);
            this.FloorLabel = new System.Windows.Forms.Label();
            this.AttackLabel3 = new System.Windows.Forms.Label();
            this.AttackLabel4 = new System.Windows.Forms.Label();
            this.BlockLabel1 = new System.Windows.Forms.Label();
            this.BlockLabel2 = new System.Windows.Forms.Label();
            this.BlockLabel3 = new System.Windows.Forms.Label();
            this.BlockLabel4 = new System.Windows.Forms.Label();
            this.Player3Label = new System.Windows.Forms.Label();
            this.Player4Label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // GameLoopTimer
            // 
            this.GameLoopTimer.Interval = 17;
            this.GameLoopTimer.Tick += new System.EventHandler(this.GameLoopTimer_Tick);
            // 
            // Player1Label
            // 
            this.Player1Label.BackColor = System.Drawing.Color.MediumTurquoise;
            this.Player1Label.Location = new System.Drawing.Point(220, 395);
            this.Player1Label.Name = "Player1Label";
            this.Player1Label.Size = new System.Drawing.Size(50, 50);
            this.Player1Label.TabIndex = 0;
            this.Player1Label.Text = "O";
            this.Player1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Player2Label
            // 
            this.Player2Label.BackColor = System.Drawing.Color.Coral;
            this.Player2Label.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Player2Label.Location = new System.Drawing.Point(591, 395);
            this.Player2Label.Name = "Player2Label";
            this.Player2Label.Size = new System.Drawing.Size(50, 50);
            this.Player2Label.TabIndex = 1;
            this.Player2Label.Text = "O";
            this.Player2Label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Player2Label.Visible = false;
            // 
            // AttackLabel1
            // 
            this.AttackLabel1.BackColor = System.Drawing.Color.Teal;
            this.AttackLabel1.Location = new System.Drawing.Point(12, 10);
            this.AttackLabel1.Name = "AttackLabel1";
            this.AttackLabel1.Size = new System.Drawing.Size(14, 14);
            this.AttackLabel1.TabIndex = 2;
            this.AttackLabel1.Visible = false;
            // 
            // AttackLabel2
            // 
            this.AttackLabel2.BackColor = System.Drawing.Color.Brown;
            this.AttackLabel2.Location = new System.Drawing.Point(32, 10);
            this.AttackLabel2.Name = "AttackLabel2";
            this.AttackLabel2.Size = new System.Drawing.Size(14, 14);
            this.AttackLabel2.TabIndex = 3;
            this.AttackLabel2.Visible = false;
            // 
            // ScoreLabel
            // 
            this.ScoreLabel.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.ScoreLabel.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ScoreLabel.ForeColor = System.Drawing.Color.Black;
            this.ScoreLabel.Location = new System.Drawing.Point(279, 486);
            this.ScoreLabel.Name = "ScoreLabel";
            this.ScoreLabel.Size = new System.Drawing.Size(428, 53);
            this.ScoreLabel.TabIndex = 4;
            this.ScoreLabel.Text = "Blue: 3 | Orange: 3 | Yellow: 3 | Purple: 3";
            this.ScoreLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TimeLabel
            // 
            this.TimeLabel.AutoSize = true;
            this.TimeLabel.Location = new System.Drawing.Point(435, 9);
            this.TimeLabel.Name = "TimeLabel";
            this.TimeLabel.Size = new System.Drawing.Size(36, 15);
            this.TimeLabel.TabIndex = 5;
            this.TimeLabel.Text = "Time:";
            // 
            // TimeTimer
            // 
            this.TimeTimer.Enabled = true;
            this.TimeTimer.Interval = 1;
            this.TimeTimer.Tick += new System.EventHandler(this.TimeTimer_Tick);
            // 
            // FloorLabel
            // 
            this.FloorLabel.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.FloorLabel.Location = new System.Drawing.Point(243, 457);
            this.FloorLabel.Name = "FloorLabel";
            this.FloorLabel.Size = new System.Drawing.Size(500, 108);
            this.FloorLabel.TabIndex = 6;
            // 
            // AttackLabel3
            // 
            this.AttackLabel3.BackColor = System.Drawing.Color.DarkGoldenrod;
            this.AttackLabel3.Location = new System.Drawing.Point(52, 10);
            this.AttackLabel3.Name = "AttackLabel3";
            this.AttackLabel3.Size = new System.Drawing.Size(14, 14);
            this.AttackLabel3.TabIndex = 7;
            this.AttackLabel3.Visible = false;
            // 
            // AttackLabel4
            // 
            this.AttackLabel4.BackColor = System.Drawing.Color.DarkSlateBlue;
            this.AttackLabel4.Location = new System.Drawing.Point(72, 10);
            this.AttackLabel4.Name = "AttackLabel4";
            this.AttackLabel4.Size = new System.Drawing.Size(14, 14);
            this.AttackLabel4.TabIndex = 8;
            this.AttackLabel4.Visible = false;
            // 
            // BlockLabel1
            // 
            this.BlockLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.BlockLabel1.Location = new System.Drawing.Point(92, 10);
            this.BlockLabel1.Name = "BlockLabel1";
            this.BlockLabel1.Size = new System.Drawing.Size(11, 11);
            this.BlockLabel1.TabIndex = 9;
            this.BlockLabel1.Visible = false;
            // 
            // BlockLabel2
            // 
            this.BlockLabel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.BlockLabel2.Location = new System.Drawing.Point(109, 10);
            this.BlockLabel2.Name = "BlockLabel2";
            this.BlockLabel2.Size = new System.Drawing.Size(11, 11);
            this.BlockLabel2.TabIndex = 10;
            this.BlockLabel2.Visible = false;
            // 
            // BlockLabel3
            // 
            this.BlockLabel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.BlockLabel3.Location = new System.Drawing.Point(126, 10);
            this.BlockLabel3.Name = "BlockLabel3";
            this.BlockLabel3.Size = new System.Drawing.Size(11, 11);
            this.BlockLabel3.TabIndex = 11;
            this.BlockLabel3.Visible = false;
            // 
            // BlockLabel4
            // 
            this.BlockLabel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.BlockLabel4.Location = new System.Drawing.Point(143, 10);
            this.BlockLabel4.Name = "BlockLabel4";
            this.BlockLabel4.Size = new System.Drawing.Size(11, 11);
            this.BlockLabel4.TabIndex = 12;
            this.BlockLabel4.Visible = false;
            // 
            // Player3Label
            // 
            this.Player3Label.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(90)))));
            this.Player3Label.Location = new System.Drawing.Point(109, 395);
            this.Player3Label.Name = "Player3Label";
            this.Player3Label.Size = new System.Drawing.Size(50, 50);
            this.Player3Label.TabIndex = 13;
            this.Player3Label.Text = "O";
            this.Player3Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Player3Label.Visible = false;
            // 
            // Player4Label
            // 
            this.Player4Label.BackColor = System.Drawing.Color.MediumPurple;
            this.Player4Label.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Player4Label.Location = new System.Drawing.Point(713, 395);
            this.Player4Label.Name = "Player4Label";
            this.Player4Label.Size = new System.Drawing.Size(50, 50);
            this.Player4Label.TabIndex = 14;
            this.Player4Label.Text = "O";
            this.Player4Label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Player4Label.Visible = false;
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 563);
            this.Controls.Add(this.BlockLabel4);
            this.Controls.Add(this.BlockLabel3);
            this.Controls.Add(this.BlockLabel2);
            this.Controls.Add(this.BlockLabel1);
            this.Controls.Add(this.Player4Label);
            this.Controls.Add(this.Player2Label);
            this.Controls.Add(this.Player1Label);
            this.Controls.Add(this.Player3Label);
            this.Controls.Add(this.AttackLabel4);
            this.Controls.Add(this.AttackLabel3);
            this.Controls.Add(this.ScoreLabel);
            this.Controls.Add(this.FloorLabel);
            this.Controls.Add(this.TimeLabel);
            this.Controls.Add(this.AttackLabel2);
            this.Controls.Add(this.AttackLabel1);
            this.Enabled = false;
            this.Name = "Client";
            this.Text = "Form1";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer GameLoopTimer;
        private Label Player1Label;
        private Label Player2Label;
        private Label AttackLabel1;
        private Label AttackLabel2;
        private Label ScoreLabel;
        private Label TimeLabel;
        private System.Windows.Forms.Timer TimeTimer;
        private Label FloorLabel;
        private Label AttackLabel3;
        private Label AttackLabel4;
        private Label BlockLabel1;
        private Label BlockLabel2;
        private Label BlockLabel3;
        private Label BlockLabel4;
        private Label Player3Label;
        private Label Player4Label;
    }
}