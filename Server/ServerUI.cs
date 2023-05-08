using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RealTimeProject
{
    public partial class ServerUI : Form
    {
        static ServerUI staticThis;

        public ServerUI()
        {
            InitializeComponent();
            staticThis = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ServerFuncs.UI = staticThis;
            ServerFuncs.OnLobbyUpdate = OnLobbyUpdate;
            ServerFuncs.OnInitGame = OnInitGame;
            ServerFuncs.OnEndGame = OnEndGame;
            ServerFuncs.frameMS = (byte)numericUpDown1.Value;
            ServerFuncs.levelLayout = 0;
            ServerFuncs.InitServer();
            InfoTextLabel.Text = "Opened lobby, waiting for players. Starts automatically at max or with button";
            PlayerListLabel.Text = "";
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            ServerFuncs.InitGame();
        }

        private void ResetGameButton_Click(object sender, EventArgs e)
        {
            ServerFuncs.ResetGame();
        }

        private void StopGameButton_Click(object sender, EventArgs e)
        {
            ServerFuncs.EndGame(0);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            GameLoopTimer.Interval = (byte)numericUpDown1.Value;
            ServerFuncs.frameMS = (byte)numericUpDown1.Value;
        }

        private void LevelLayoutComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ServerFuncs.levelLayout = LevelLayoutComboBox.SelectedIndex;
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            ServerFuncs.OnTimerTick();
        }

        private void OnLobbyUpdate(string msg)
        {
            StartGameButton.Enabled = !(ServerSockFuncs.lobbyPlayerDict.Count() == 0);
            PlayerListLabel.Text = msg;
        }

        private void OnInitGame()
        {
            InfoTextLabel.Text = "Game Running";
            StartGameButton.Enabled = false;
            ResetGameButton.Enabled = true;
            StopGameButton.Enabled = true;

            // Should be a timer inside of the server code but oh well
            GameLoopTimer.Enabled = true;
        }

        private void OnEndGame()
        {
            GameLoopTimer.Enabled = false;

            PlayerListLabel.Text = "";
            ResetGameButton.Enabled = false;
            StopGameButton.Enabled = false;
        }

        private void DisablePanels()
        {
            MainPanel.Enabled = false;
            MainPanel.Visible = false;
            UserViewPanel.Enabled = false;
            UserViewPanel.Visible = false;
            MatchViewPanel.Enabled = false;
            MatchViewPanel.Visible = false;
        }

        private void UserListButton_Click(object sender, EventArgs e)
        {
            DisablePanels();
            UserViewPanel.Enabled = true;
            UserViewPanel.Visible = true;
            UserListTextBox.Text = string.Join("\r\n", DatabaseAccess.GetUserNames());
        }

        private void MatchHistoryButton_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            HistoryTableLayoutPanel.RowCount = 2;
            List<Match> MatchHistory = DatabaseAccess.GetAllMatches();
            Console.WriteLine("Recieved " + MatchHistory.Count() + " matches");
            foreach (Match match in MatchHistory)
            {
                AddMatchToTable(match);
            }
            ResumeLayout();
            DisablePanels();
            MatchViewPanel.Enabled = true;
            MatchViewPanel.Visible = true;

        }

        public void AddMatchToTable(Match match)
        {
            int row = HistoryTableLayoutPanel.RowCount;
            HistoryTableLayoutPanel.RowCount++;
            Console.WriteLine("Adding Match To Table at row " + row);
            HistoryTableLayoutPanel.Controls.Add(CreateTableLabel(match.StartTime, StartTimeHeaderLabel.Width), 0, row);
            HistoryTableLayoutPanel.Controls.Add(CreateTableLabel(match.Players, PlayersHeaderLabel.Width), 1, row);
            HistoryTableLayoutPanel.Controls.Add(CreateTableLabel(match.Winner, WinnerHeaderLabel.Width), 2, row);
            HistoryTableLayoutPanel.Controls.Add(CreateTableLabel(match.Length, LengthHeaderLabel.Width), 3, row);
        }

        public Label CreateTableLabel(string text, int width)
        {
            Label outLabel = new Label();
            outLabel.Text = text;
            outLabel.AutoSize = false;
            outLabel.Width = width;
            outLabel.Font = StartTimeHeaderLabel.Font;
            return outLabel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DisablePanels();
            MainPanel.Enabled = false;
            MainPanel.Visible = false;
        }
    }
}