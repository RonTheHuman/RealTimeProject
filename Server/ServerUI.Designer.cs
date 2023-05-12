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
            this.button2 = new System.Windows.Forms.Button();
            this.MatchListPanel = new System.Windows.Forms.Panel();
            this.HistoryTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LengthHeaderLabel = new System.Windows.Forms.Label();
            this.WinnerHeaderLabel = new System.Windows.Forms.Label();
            this.PlayersHeaderLabel = new System.Windows.Forms.Label();
            this.StartTimeHeaderLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.UserViewPanel = new System.Windows.Forms.Panel();
            this.UserListPanel = new System.Windows.Forms.Panel();
            this.UserListLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.MainPanel.SuspendLayout();
            this.MatchViewPanel.SuspendLayout();
            this.MatchListPanel.SuspendLayout();
            this.HistoryTableLayoutPanel.SuspendLayout();
            this.UserViewPanel.SuspendLayout();
            this.UserListPanel.SuspendLayout();
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
            this.StartGameButton.Location = new System.Drawing.Point(57, 378);
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
            this.ResetGameButton.Location = new System.Drawing.Point(176, 378);
            this.ResetGameButton.Name = "ResetGameButton";
            this.ResetGameButton.Size = new System.Drawing.Size(92, 36);
            this.ResetGameButton.TabIndex = 3;
            this.ResetGameButton.Text = "Reset Game";
            this.ResetGameButton.UseVisualStyleBackColor = true;
            this.ResetGameButton.Click += new System.EventHandler(this.ResetGameButton_Click);
            // 
            // StopGameButton
            // 
            this.StopGameButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.StopGameButton.Enabled = false;
            this.StopGameButton.Location = new System.Drawing.Point(295, 377);
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
            this.PlayerListLabel.Location = new System.Drawing.Point(57, 133);
            this.PlayerListLabel.Name = "PlayerListLabel";
            this.PlayerListLabel.Size = new System.Drawing.Size(330, 209);
            this.PlayerListLabel.TabIndex = 1;
            // 
            // InfoTextLabel
            // 
            this.InfoTextLabel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.InfoTextLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.InfoTextLabel.Location = new System.Drawing.Point(37, 50);
            this.InfoTextLabel.Name = "InfoTextLabel";
            this.InfoTextLabel.Size = new System.Drawing.Size(370, 65);
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
            this.numericUpDown1.Location = new System.Drawing.Point(89, 557);
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
            this.label1.Location = new System.Drawing.Point(58, 529);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Milliseconds per Frame";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(285, 529);
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
            this.LevelLayoutComboBox.Location = new System.Drawing.Point(258, 557);
            this.LevelLayoutComboBox.Name = "LevelLayoutComboBox";
            this.LevelLayoutComboBox.Size = new System.Drawing.Size(129, 23);
            this.LevelLayoutComboBox.TabIndex = 9;
            this.LevelLayoutComboBox.Text = "Single Platform";
            this.LevelLayoutComboBox.SelectedIndexChanged += new System.EventHandler(this.LevelLayoutComboBox_SelectedIndexChanged);
            // 
            // MainPanel
            // 
            this.MainPanel.BackColor = System.Drawing.SystemColors.Control;
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
            this.MainPanel.Size = new System.Drawing.Size(444, 625);
            this.MainPanel.TabIndex = 10;
            // 
            // MatchHistoryButton
            // 
            this.MatchHistoryButton.Location = new System.Drawing.Point(248, 444);
            this.MatchHistoryButton.Name = "MatchHistoryButton";
            this.MatchHistoryButton.Size = new System.Drawing.Size(139, 36);
            this.MatchHistoryButton.TabIndex = 11;
            this.MatchHistoryButton.Text = "View Match History";
            this.MatchHistoryButton.UseVisualStyleBackColor = true;
            this.MatchHistoryButton.Click += new System.EventHandler(this.MatchHistoryButton_Click);
            // 
            // UserListButton
            // 
            this.UserListButton.Location = new System.Drawing.Point(57, 444);
            this.UserListButton.Name = "UserListButton";
            this.UserListButton.Size = new System.Drawing.Size(130, 36);
            this.UserListButton.TabIndex = 10;
            this.UserListButton.Text = "View User List";
            this.UserListButton.UseVisualStyleBackColor = true;
            this.UserListButton.Click += new System.EventHandler(this.UserListButton_Click);
            // 
            // MatchViewPanel
            // 
            this.MatchViewPanel.Controls.Add(this.button2);
            this.MatchViewPanel.Controls.Add(this.MatchListPanel);
            this.MatchViewPanel.Controls.Add(this.label4);
            this.MatchViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MatchViewPanel.Location = new System.Drawing.Point(0, 0);
            this.MatchViewPanel.Name = "MatchViewPanel";
            this.MatchViewPanel.Size = new System.Drawing.Size(444, 625);
            this.MatchViewPanel.TabIndex = 10;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(151, 567);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(142, 37);
            this.button2.TabIndex = 5;
            this.button2.Text = "Return";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button1_Click);
            // 
            // MatchListPanel
            // 
            this.MatchListPanel.AutoScroll = true;
            this.MatchListPanel.Controls.Add(this.HistoryTableLayoutPanel);
            this.MatchListPanel.Location = new System.Drawing.Point(21, 129);
            this.MatchListPanel.Name = "MatchListPanel";
            this.MatchListPanel.Size = new System.Drawing.Size(402, 422);
            this.MatchListPanel.TabIndex = 3;
            // 
            // HistoryTableLayoutPanel
            // 
            this.HistoryTableLayoutPanel.AutoSize = true;
            this.HistoryTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.HistoryTableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.HistoryTableLayoutPanel.ColumnCount = 4;
            this.HistoryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.HistoryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.HistoryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.HistoryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.HistoryTableLayoutPanel.Controls.Add(this.LengthHeaderLabel, 3, 0);
            this.HistoryTableLayoutPanel.Controls.Add(this.WinnerHeaderLabel, 2, 0);
            this.HistoryTableLayoutPanel.Controls.Add(this.PlayersHeaderLabel, 1, 0);
            this.HistoryTableLayoutPanel.Controls.Add(this.StartTimeHeaderLabel, 0, 0);
            this.HistoryTableLayoutPanel.Controls.Add(this.label9, 0, 1);
            this.HistoryTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.HistoryTableLayoutPanel.Name = "HistoryTableLayoutPanel";
            this.HistoryTableLayoutPanel.RowCount = 2;
            this.HistoryTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.HistoryTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.HistoryTableLayoutPanel.Size = new System.Drawing.Size(394, 55);
            this.HistoryTableLayoutPanel.TabIndex = 4;
            // 
            // LengthHeaderLabel
            // 
            this.LengthHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LengthHeaderLabel.Location = new System.Drawing.Point(301, 1);
            this.LengthHeaderLabel.Name = "LengthHeaderLabel";
            this.LengthHeaderLabel.Size = new System.Drawing.Size(89, 49);
            this.LengthHeaderLabel.TabIndex = 5;
            this.LengthHeaderLabel.Text = "Length";
            this.LengthHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // WinnerHeaderLabel
            // 
            this.WinnerHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WinnerHeaderLabel.Location = new System.Drawing.Point(193, 1);
            this.WinnerHeaderLabel.Name = "WinnerHeaderLabel";
            this.WinnerHeaderLabel.Size = new System.Drawing.Size(101, 49);
            this.WinnerHeaderLabel.TabIndex = 4;
            this.WinnerHeaderLabel.Text = "Winner";
            this.WinnerHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PlayersHeaderLabel
            // 
            this.PlayersHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayersHeaderLabel.Location = new System.Drawing.Point(90, 1);
            this.PlayersHeaderLabel.Name = "PlayersHeaderLabel";
            this.PlayersHeaderLabel.Size = new System.Drawing.Size(96, 49);
            this.PlayersHeaderLabel.TabIndex = 3;
            this.PlayersHeaderLabel.Text = "Players";
            this.PlayersHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StartTimeHeaderLabel
            // 
            this.StartTimeHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StartTimeHeaderLabel.Location = new System.Drawing.Point(4, 1);
            this.StartTimeHeaderLabel.Name = "StartTimeHeaderLabel";
            this.StartTimeHeaderLabel.Size = new System.Drawing.Size(79, 49);
            this.StartTimeHeaderLabel.TabIndex = 2;
            this.StartTimeHeaderLabel.Text = "Start Time";
            this.StartTimeHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.HistoryTableLayoutPanel.SetColumnSpan(this.label9, 4);
            this.label9.Location = new System.Drawing.Point(4, 51);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(386, 3);
            this.label9.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(37, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(370, 65);
            this.label4.TabIndex = 2;
            this.label4.Text = "Match History:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UserViewPanel
            // 
            this.UserViewPanel.Controls.Add(this.UserListPanel);
            this.UserViewPanel.Controls.Add(this.button1);
            this.UserViewPanel.Controls.Add(this.label3);
            this.UserViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UserViewPanel.Location = new System.Drawing.Point(0, 0);
            this.UserViewPanel.Name = "UserViewPanel";
            this.UserViewPanel.Size = new System.Drawing.Size(444, 625);
            this.UserViewPanel.TabIndex = 0;
            // 
            // UserListPanel
            // 
            this.UserListPanel.AutoScroll = true;
            this.UserListPanel.Controls.Add(this.UserListLabel);
            this.UserListPanel.Location = new System.Drawing.Point(55, 136);
            this.UserListPanel.Name = "UserListPanel";
            this.UserListPanel.Size = new System.Drawing.Size(333, 415);
            this.UserListPanel.TabIndex = 4;
            // 
            // UserListLabel
            // 
            this.UserListLabel.AutoSize = true;
            this.UserListLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.UserListLabel.Location = new System.Drawing.Point(96, 6);
            this.UserListLabel.Name = "UserListLabel";
            this.UserListLabel.Size = new System.Drawing.Size(0, 21);
            this.UserListLabel.TabIndex = 0;
            this.UserListLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(151, 567);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(142, 37);
            this.button1.TabIndex = 3;
            this.button1.Text = "Return";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(37, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(370, 65);
            this.label3.TabIndex = 1;
            this.label3.Text = "All Users:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ServerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 625);
            this.Controls.Add(this.MatchViewPanel);
            this.Controls.Add(this.UserViewPanel);
            this.Controls.Add(this.MainPanel);
            this.Name = "ServerUI";
            this.Text = "Fight^2 Server";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.MatchViewPanel.ResumeLayout(false);
            this.MatchListPanel.ResumeLayout(false);
            this.MatchListPanel.PerformLayout();
            this.HistoryTableLayoutPanel.ResumeLayout(false);
            this.UserViewPanel.ResumeLayout(false);
            this.UserListPanel.ResumeLayout(false);
            this.UserListPanel.PerformLayout();
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
        private Button button1;
        private Panel MatchListPanel;
        private Label label4;
        private TableLayoutPanel HistoryTableLayoutPanel;
        private Label LengthHeaderLabel;
        private Label WinnerHeaderLabel;
        private Label PlayersHeaderLabel;
        private Label StartTimeHeaderLabel;
        private Label label9;
        private Panel UserListPanel;
        private Label UserListLabel;
        private Button button2;
    }
}