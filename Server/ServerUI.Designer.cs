namespace RealTimeProject
{
    partial class ServerUI
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
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LevelLayoutComboBox = new System.Windows.Forms.ComboBox();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.MatchHistoryButton = new System.Windows.Forms.Button();
            this.UserListButton = new System.Windows.Forms.Button();
            this.MatchViewPanel = new System.Windows.Forms.Panel();
            this.UserViewPanel = new System.Windows.Forms.Panel();
            this.UserListTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.MainPanel.SuspendLayout();
            this.MatchViewPanel.SuspendLayout();
            this.UserViewPanel.SuspendLayout();
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
            this.StartGameButton.Location = new System.Drawing.Point(68, 378);
            this.StartGameButton.Name = "StartGameButton";
            this.StartGameButton.Size = new System.Drawing.Size(92, 36);
            this.StartGameButton.TabIndex = 2;
            this.StartGameButton.Text = "Start Game";
            this.StartGameButton.UseVisualStyleBackColor = true;
            this.StartGameButton.Click += new System.EventHandler(this.StartGameButton_Click);
            // 
            // ResetGameButton
            // 
            this.ResetGameButton.Enabled = false;
            this.ResetGameButton.Location = new System.Drawing.Point(186, 378);
            this.ResetGameButton.Name = "ResetGameButton";
            this.ResetGameButton.Size = new System.Drawing.Size(95, 36);
            this.ResetGameButton.TabIndex = 3;
            this.ResetGameButton.Text = "Reset Game";
            this.ResetGameButton.UseVisualStyleBackColor = true;
            this.ResetGameButton.Click += new System.EventHandler(this.ResetGameButton_Click);
            // 
            // StopGameButton
            // 
            this.StopGameButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.StopGameButton.Enabled = false;
            this.StopGameButton.Location = new System.Drawing.Point(306, 378);
            this.StopGameButton.Name = "StopGameButton";
            this.StopGameButton.Size = new System.Drawing.Size(92, 37);
            this.StopGameButton.TabIndex = 4;
            this.StopGameButton.Text = "Stop Game";
            this.StopGameButton.UseVisualStyleBackColor = true;
            this.StopGameButton.Click += new System.EventHandler(this.StopGameButton_Click);
            // 
            // PlayerListLabel
            // 
            this.PlayerListLabel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.PlayerListLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.PlayerListLabel.Location = new System.Drawing.Point(68, 133);
            this.PlayerListLabel.Name = "PlayerListLabel";
            this.PlayerListLabel.Size = new System.Drawing.Size(330, 209);
            this.PlayerListLabel.TabIndex = 1;
            // 
            // InfoTextLabel
            // 
            this.InfoTextLabel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.InfoTextLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.InfoTextLabel.Location = new System.Drawing.Point(49, 51);
            this.InfoTextLabel.Name = "InfoTextLabel";
            this.InfoTextLabel.Size = new System.Drawing.Size(374, 66);
            this.InfoTextLabel.TabIndex = 0;
            this.InfoTextLabel.Text = "Info Text";
            this.InfoTextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(105, 558);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(71, 23);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(74, 529);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Milliseconds per Frame";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(272, 529);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Level Layout";
            // 
            // LevelLayoutComboBox
            // 
            this.LevelLayoutComboBox.FormattingEnabled = true;
            this.LevelLayoutComboBox.Items.AddRange(new object[] {
            "Single Platform",
            "Two Small Platforms",
            "Platform Tower"});
            this.LevelLayoutComboBox.Location = new System.Drawing.Point(243, 558);
            this.LevelLayoutComboBox.Name = "LevelLayoutComboBox";
            this.LevelLayoutComboBox.Size = new System.Drawing.Size(129, 23);
            this.LevelLayoutComboBox.TabIndex = 9;
            this.LevelLayoutComboBox.Text = "Single Platform";
            this.LevelLayoutComboBox.SelectedIndexChanged += new System.EventHandler(this.LevelLayoutComboBox_SelectedIndexChanged);
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.MatchHistoryButton);
            this.MainPanel.Controls.Add(this.UserListButton);
            this.MainPanel.Controls.Add(this.InfoTextLabel);
            this.MainPanel.Controls.Add(this.PlayerListLabel);
            this.MainPanel.Controls.Add(this.StopGameButton);
            this.MainPanel.Controls.Add(this.StartGameButton);
            this.MainPanel.Controls.Add(this.ResetGameButton);
            this.MainPanel.Controls.Add(this.numericUpDown1);
            this.MainPanel.Controls.Add(this.LevelLayoutComboBox);
            this.MainPanel.Controls.Add(this.label1);
            this.MainPanel.Controls.Add(this.label2);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(464, 668);
            this.MainPanel.TabIndex = 10;
            // 
            // MatchHistoryButton
            // 
            this.MatchHistoryButton.Location = new System.Drawing.Point(259, 444);
            this.MatchHistoryButton.Name = "MatchHistoryButton";
            this.MatchHistoryButton.Size = new System.Drawing.Size(139, 36);
            this.MatchHistoryButton.TabIndex = 11;
            this.MatchHistoryButton.Text = "View Match History";
            this.MatchHistoryButton.UseVisualStyleBackColor = true;
            this.MatchHistoryButton.Click += new System.EventHandler(this.MatchHistoryButton_Click);
            // 
            // UserListButton
            // 
            this.UserListButton.Location = new System.Drawing.Point(68, 444);
            this.UserListButton.Name = "UserListButton";
            this.UserListButton.Size = new System.Drawing.Size(130, 36);
            this.UserListButton.TabIndex = 10;
            this.UserListButton.Text = "View User List";
            this.UserListButton.UseVisualStyleBackColor = true;
            this.UserListButton.Click += new System.EventHandler(this.UserListButton_Click);
            // 
            // MatchViewPanel
            // 
            this.MatchViewPanel.Controls.Add(this.panel1);
            this.MatchViewPanel.Controls.Add(this.label4);
            this.MatchViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MatchViewPanel.Location = new System.Drawing.Point(0, 0);
            this.MatchViewPanel.Name = "MatchViewPanel";
            this.MatchViewPanel.Size = new System.Drawing.Size(464, 668);
            this.MatchViewPanel.TabIndex = 10;
            // 
            // UserViewPanel
            // 
            this.UserViewPanel.Controls.Add(this.button1);
            this.UserViewPanel.Controls.Add(this.UserListTextBox);
            this.UserViewPanel.Controls.Add(this.label3);
            this.UserViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UserViewPanel.Location = new System.Drawing.Point(0, 0);
            this.UserViewPanel.Name = "UserViewPanel";
            this.UserViewPanel.Size = new System.Drawing.Size(464, 668);
            this.UserViewPanel.TabIndex = 0;
            // 
            // UserListTextBox
            // 
            this.UserListTextBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.UserListTextBox.Enabled = false;
            this.UserListTextBox.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.UserListTextBox.Location = new System.Drawing.Point(57, 134);
            this.UserListTextBox.Multiline = true;
            this.UserListTextBox.Name = "UserListTextBox";
            this.UserListTextBox.ReadOnly = true;
            this.UserListTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.UserListTextBox.Size = new System.Drawing.Size(333, 447);
            this.UserListTextBox.TabIndex = 2;
            this.UserListTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(37, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(374, 66);
            this.label3.TabIndex = 1;
            this.label3.Text = "All Users:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(154, 600);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(142, 37);
            this.button1.TabIndex = 3;
            this.button1.Text = "Return";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(37, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(374, 66);
            this.label4.TabIndex = 2;
            this.label4.Text = "Match History:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(30, 129);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(393, 461);
            this.panel1.TabIndex = 3;
            // 
            // ServerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 668);
            this.Controls.Add(this.MatchViewPanel);
            this.Controls.Add(this.UserViewPanel);
            this.Controls.Add(this.MainPanel);
            this.Name = "ServerUI";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.MatchViewPanel.ResumeLayout(false);
            this.UserViewPanel.ResumeLayout(false);
            this.UserViewPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer GameLoopTimer;
        private Button StartGameButton;
        private Button ResetGameButton;
        private Button StopGameButton;
        private Label PlayerListLabel;
        private Label InfoTextLabel;
        private NumericUpDown numericUpDown1;
        private Label label1;
        private Label label2;
        private ComboBox LevelLayoutComboBox;
        private Panel MainPanel;
        private Panel MatchViewPanel;
        private Panel UserViewPanel;
        private Button MatchHistoryButton;
        private Button UserListButton;
        private Label label3;
        private TextBox UserListTextBox;
        private Button button1;
        private Panel panel1;
        private Label label4;
    }
}