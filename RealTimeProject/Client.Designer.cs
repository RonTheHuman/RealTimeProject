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
            this.label1 = new System.Windows.Forms.Label();
            this.StartupPanel = new System.Windows.Forms.Panel();
            this.ResponseLabel = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.SignUpButton = new System.Windows.Forms.Button();
            this.SignInButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PassTextBox = new System.Windows.Forms.TextBox();
            this.UNameTextBox = new System.Windows.Forms.TextBox();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.MainMenuPanel = new System.Windows.Forms.Panel();
            this.MenuTextLabel = new System.Windows.Forms.Label();
            this.BackButton = new System.Windows.Forms.Button();
            this.ViewGameHistoryButton = new System.Windows.Forms.Button();
            this.EnterLobbyButton = new System.Windows.Forms.Button();
            this.GamePanel = new System.Windows.Forms.Panel();
            this.GameHistoryPanel = new System.Windows.Forms.Panel();
            this.BackButtonGH = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.HistoryTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.StartupPanel.SuspendLayout();
            this.MainMenuPanel.SuspendLayout();
            this.GamePanel.SuspendLayout();
            this.GameHistoryPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.HistoryTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // GameLoopTimer
            // 
            this.GameLoopTimer.Interval = 15;
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
            this.ScoreLabel.Text = "Waiting For Server...";
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
            this.FloorLabel.Size = new System.Drawing.Size(500, 365);
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
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.OliveDrab;
            this.label1.Location = new System.Drawing.Point(300, 365);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(386, 13);
            this.label1.TabIndex = 15;
            // 
            // StartupPanel
            // 
            this.StartupPanel.Controls.Add(this.ResponseLabel);
            this.StartupPanel.Controls.Add(this.button3);
            this.StartupPanel.Controls.Add(this.SignUpButton);
            this.StartupPanel.Controls.Add(this.SignInButton);
            this.StartupPanel.Controls.Add(this.label3);
            this.StartupPanel.Controls.Add(this.label2);
            this.StartupPanel.Controls.Add(this.PassTextBox);
            this.StartupPanel.Controls.Add(this.UNameTextBox);
            this.StartupPanel.Controls.Add(this.TitleLabel);
            this.StartupPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StartupPanel.Location = new System.Drawing.Point(0, 0);
            this.StartupPanel.Name = "StartupPanel";
            this.StartupPanel.Size = new System.Drawing.Size(970, 692);
            this.StartupPanel.TabIndex = 16;
            // 
            // ResponseLabel
            // 
            this.ResponseLabel.Location = new System.Drawing.Point(648, 321);
            this.ResponseLabel.Name = "ResponseLabel";
            this.ResponseLabel.Size = new System.Drawing.Size(187, 57);
            this.ResponseLabel.TabIndex = 8;
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button3.Location = new System.Drawing.Point(422, 508);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(142, 49);
            this.button3.TabIndex = 7;
            this.button3.Text = "Enter as Guest (Quickplay)";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.EnterLobbyButton_Click);
            // 
            // SignUpButton
            // 
            this.SignUpButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.SignUpButton.Location = new System.Drawing.Point(422, 457);
            this.SignUpButton.Name = "SignUpButton";
            this.SignUpButton.Size = new System.Drawing.Size(142, 42);
            this.SignUpButton.TabIndex = 6;
            this.SignUpButton.Text = "Sign Up";
            this.SignUpButton.UseVisualStyleBackColor = true;
            this.SignUpButton.Click += new System.EventHandler(this.SignUpButton_Click);
            // 
            // SignInButton
            // 
            this.SignInButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.SignInButton.Location = new System.Drawing.Point(422, 409);
            this.SignInButton.Name = "SignInButton";
            this.SignInButton.Size = new System.Drawing.Size(142, 42);
            this.SignInButton.TabIndex = 5;
            this.SignInButton.Text = "Sign In";
            this.SignInButton.UseVisualStyleBackColor = true;
            this.SignInButton.Click += new System.EventHandler(this.SignInButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(258, 361);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Password:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(258, 322);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "User Name:";
            // 
            // PassTextBox
            // 
            this.PassTextBox.Location = new System.Drawing.Point(372, 361);
            this.PassTextBox.Name = "PassTextBox";
            this.PassTextBox.Size = new System.Drawing.Size(241, 23);
            this.PassTextBox.TabIndex = 2;
            // 
            // UNameTextBox
            // 
            this.UNameTextBox.Location = new System.Drawing.Point(372, 321);
            this.UNameTextBox.Name = "UNameTextBox";
            this.UNameTextBox.Size = new System.Drawing.Size(241, 23);
            this.UNameTextBox.TabIndex = 1;
            // 
            // TitleLabel
            // 
            this.TitleLabel.Font = new System.Drawing.Font("Yu Gothic", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.TitleLabel.Location = new System.Drawing.Point(126, 143);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(717, 138);
            this.TitleLabel.TabIndex = 0;
            this.TitleLabel.Text = "Fight^2";
            this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainMenuPanel
            // 
            this.MainMenuPanel.Controls.Add(this.MenuTextLabel);
            this.MainMenuPanel.Controls.Add(this.BackButton);
            this.MainMenuPanel.Controls.Add(this.ViewGameHistoryButton);
            this.MainMenuPanel.Controls.Add(this.EnterLobbyButton);
            this.MainMenuPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainMenuPanel.Location = new System.Drawing.Point(0, 0);
            this.MainMenuPanel.Name = "MainMenuPanel";
            this.MainMenuPanel.Size = new System.Drawing.Size(970, 692);
            this.MainMenuPanel.TabIndex = 8;
            // 
            // MenuTextLabel
            // 
            this.MenuTextLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.MenuTextLabel.Location = new System.Drawing.Point(347, 215);
            this.MenuTextLabel.Name = "MenuTextLabel";
            this.MenuTextLabel.Size = new System.Drawing.Size(291, 52);
            this.MenuTextLabel.TabIndex = 3;
            this.MenuTextLabel.Text = "Welcome User!";
            this.MenuTextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BackButton
            // 
            this.BackButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.BackButton.Location = new System.Drawing.Point(407, 418);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(178, 47);
            this.BackButton.TabIndex = 2;
            this.BackButton.Text = "Back";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // ViewGameHistoryButton
            // 
            this.ViewGameHistoryButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ViewGameHistoryButton.Location = new System.Drawing.Point(407, 365);
            this.ViewGameHistoryButton.Name = "ViewGameHistoryButton";
            this.ViewGameHistoryButton.Size = new System.Drawing.Size(178, 47);
            this.ViewGameHistoryButton.TabIndex = 1;
            this.ViewGameHistoryButton.Text = "View Game History";
            this.ViewGameHistoryButton.UseVisualStyleBackColor = true;
            this.ViewGameHistoryButton.Click += new System.EventHandler(this.ViewGameHistoryButton_Click);
            // 
            // EnterLobbyButton
            // 
            this.EnterLobbyButton.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.EnterLobbyButton.Location = new System.Drawing.Point(347, 280);
            this.EnterLobbyButton.Name = "EnterLobbyButton";
            this.EnterLobbyButton.Size = new System.Drawing.Size(291, 69);
            this.EnterLobbyButton.TabIndex = 0;
            this.EnterLobbyButton.Text = "Enter Lobby";
            this.EnterLobbyButton.UseVisualStyleBackColor = true;
            this.EnterLobbyButton.Click += new System.EventHandler(this.EnterLobbyButton_Click);
            // 
            // GamePanel
            // 
            this.GamePanel.Controls.Add(this.label1);
            this.GamePanel.Controls.Add(this.BlockLabel4);
            this.GamePanel.Controls.Add(this.BlockLabel3);
            this.GamePanel.Controls.Add(this.BlockLabel2);
            this.GamePanel.Controls.Add(this.BlockLabel1);
            this.GamePanel.Controls.Add(this.Player4Label);
            this.GamePanel.Controls.Add(this.Player2Label);
            this.GamePanel.Controls.Add(this.Player1Label);
            this.GamePanel.Controls.Add(this.Player3Label);
            this.GamePanel.Controls.Add(this.AttackLabel4);
            this.GamePanel.Controls.Add(this.AttackLabel3);
            this.GamePanel.Controls.Add(this.ScoreLabel);
            this.GamePanel.Controls.Add(this.FloorLabel);
            this.GamePanel.Controls.Add(this.TimeLabel);
            this.GamePanel.Controls.Add(this.AttackLabel2);
            this.GamePanel.Controls.Add(this.AttackLabel1);
            this.GamePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GamePanel.Location = new System.Drawing.Point(0, 0);
            this.GamePanel.Name = "GamePanel";
            this.GamePanel.Size = new System.Drawing.Size(970, 692);
            this.GamePanel.TabIndex = 8;
            // 
            // GameHistoryPanel
            // 
            this.GameHistoryPanel.Controls.Add(this.BackButtonGH);
            this.GameHistoryPanel.Controls.Add(this.label8);
            this.GameHistoryPanel.Controls.Add(this.panel1);
            this.GameHistoryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameHistoryPanel.Location = new System.Drawing.Point(0, 0);
            this.GameHistoryPanel.Name = "GameHistoryPanel";
            this.GameHistoryPanel.Size = new System.Drawing.Size(970, 692);
            this.GameHistoryPanel.TabIndex = 4;
            // 
            // BackButtonGH
            // 
            this.BackButtonGH.Location = new System.Drawing.Point(390, 619);
            this.BackButtonGH.Name = "BackButtonGH";
            this.BackButtonGH.Size = new System.Drawing.Size(187, 50);
            this.BackButtonGH.TabIndex = 2;
            this.BackButtonGH.Text = "Back";
            this.BackButtonGH.UseVisualStyleBackColor = true;
            this.BackButtonGH.Click += new System.EventHandler(this.BackButtonGH_Click);
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(390, 26);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(210, 43);
            this.label8.TabIndex = 1;
            this.label8.Text = "Game History:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.HistoryTableLayoutPanel);
            this.panel1.Location = new System.Drawing.Point(109, 80);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(757, 521);
            this.panel1.TabIndex = 0;
            // 
            // HistoryTableLayoutPanel
            // 
            this.HistoryTableLayoutPanel.AutoSize = true;
            this.HistoryTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.HistoryTableLayoutPanel.ColumnCount = 4;
            this.HistoryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.HistoryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.HistoryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.HistoryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.HistoryTableLayoutPanel.Controls.Add(this.label7, 3, 0);
            this.HistoryTableLayoutPanel.Controls.Add(this.label6, 2, 0);
            this.HistoryTableLayoutPanel.Controls.Add(this.label5, 1, 0);
            this.HistoryTableLayoutPanel.Controls.Add(this.label4, 0, 0);
            this.HistoryTableLayoutPanel.Controls.Add(this.label9, 0, 1);
            this.HistoryTableLayoutPanel.Location = new System.Drawing.Point(21, 19);
            this.HistoryTableLayoutPanel.Name = "HistoryTableLayoutPanel";
            this.HistoryTableLayoutPanel.RowCount = 2;
            this.HistoryTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.HistoryTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.HistoryTableLayoutPanel.Size = new System.Drawing.Size(715, 52);
            this.HistoryTableLayoutPanel.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(626, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 49);
            this.label7.TabIndex = 5;
            this.label7.Text = "Length";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(419, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(201, 49);
            this.label6.TabIndex = 4;
            this.label6.Text = "Winner";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(153, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(260, 49);
            this.label5.TabIndex = 3;
            this.label5.Text = "Players";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(144, 49);
            this.label4.TabIndex = 2;
            this.label4.Text = "Date";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.HistoryTableLayoutPanel.SetColumnSpan(this.label9, 4);
            this.label9.Location = new System.Drawing.Point(3, 49);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(709, 3);
            this.label9.TabIndex = 6;
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 692);
            this.Controls.Add(this.GameHistoryPanel);
            this.Controls.Add(this.MainMenuPanel);
            this.Controls.Add(this.StartupPanel);
            this.Controls.Add(this.GamePanel);
            this.Name = "Client";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Client_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Graphics_KeyUp);
            this.StartupPanel.ResumeLayout(false);
            this.StartupPanel.PerformLayout();
            this.MainMenuPanel.ResumeLayout(false);
            this.GamePanel.ResumeLayout(false);
            this.GamePanel.PerformLayout();
            this.GameHistoryPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.HistoryTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private Label label1;
        private Panel StartupPanel;
        private Label TitleLabel;
        private Button SignUpButton;
        private Button SignInButton;
        private Label label3;
        private Label label2;
        private TextBox PassTextBox;
        private TextBox UNameTextBox;
        private Button button3;
        private Panel MainMenuPanel;
        private Button ViewGameHistoryButton;
        private Button EnterLobbyButton;
        private Panel GamePanel;
        private Label ResponseLabel;
        private Button BackButton;
        private Label MenuTextLabel;
        private Panel GameHistoryPanel;
        private Panel panel1;
        private TableLayoutPanel HistoryTableLayoutPanel;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Button BackButtonGH;
        private Label label8;
        private Label label9;
    }
}