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

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            ServerFuncs.OnTimerTick();
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            ServerFuncs.InitGame();
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

        private void ResetGameButton_Click(object sender, EventArgs e)
        {
            ServerFuncs.ResetGame();
        }

        private void StopGameButton_Click(object sender, EventArgs e)
        {
            ServerFuncs.EndGame(0);
        }

        private void OnLobbyUpdate(string msg)
        {
            StartGameButton.Enabled = !(SocketFuncs.lobbyPlayerDict.Count() == 0);
            PlayerListLabel.Text += msg;
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

            ResetGameButton.Enabled = false;
            StopGameButton.Enabled = false;
        }

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

    }
}