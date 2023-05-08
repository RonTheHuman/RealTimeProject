using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;
using System.Text.Json;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;

namespace RealTimeProject
{
    public partial class ClientUI : Form
    {
        byte frameMS = 15;
        bool fullSim = true, clientSim = true, enemySim = false;
        Color[] playerColors = new Color[4] { Color.MediumTurquoise, Color.Coral, Color.FromArgb(255, 255, 90), Color.MediumPurple };
        Input curInput = new Input();
        Label[]? playerLabels, blockLabels, attackLabels, platformLabels;

        public ClientUI()
        {
            InitializeComponent();
        }

        private void Client_Load(object sender, EventArgs e)
        {
            GameLoopTimer.Interval = frameMS;
            ClientFuncs.InitClient();
            ClientFuncs.UI = this;
            ClientFuncs.OnJoinLobby = OnJoinLobby;
            ClientFuncs.OnJoinFail = OnJoinFail;
            ClientFuncs.OnEndGame = OnEndGame;
            ClientFuncs.timer = GameLoopTimer;
            LoadStartupPanel();
        }

        private void LoadStartupPanel()
        {
            UNameTextBox.Text = "";
            PassTextBox.Text = "";
            DisablePanels();
            StartupPanel.Enabled = true;
            StartupPanel.Visible = true;
        }

        private void LoadMainMenuPanel()
        {
            DisablePanels();
            MenuTextLabel.Text = "Welcome " + ClientFuncs.uName + "!";
            MainMenuPanel.Enabled = true;
            MainMenuPanel.Visible = true;
        }

        private void LoadTransitionPanel(string labelText)
        {
            DisablePanels();
            TransitionPanel.Enabled = true;
            TransitionPanel.Visible = true;
            TransitionTextLabel.Text = labelText;

        }

        private void LoadGamePanel()
        {
            DisablePanels();
            GamePanel.Enabled = true;
            GamePanel.Visible = true;

            Player2Label.Visible = false;
            Player3Label.Visible = false;
            Player4Label.Visible = false;
            BlockLabel2.Visible = false;
            BlockLabel3.Visible = false;
            BlockLabel4.Visible = false;
            AttackLabel2.Visible = false;
            AttackLabel3.Visible = false;
            AttackLabel4.Visible = false;

            int pCount = ClientFuncs.pCount;
            playerLabels = new Label[pCount];
            attackLabels = new Label[pCount];
            blockLabels = new Label[pCount];
            playerLabels[0] = Player1Label;
            attackLabels[0] = AttackLabel1;
            blockLabels[0] = BlockLabel1;
            if (pCount >= 2)
            {
                Player2Label.Visible = true;
                playerLabels[1] = Player2Label;
                attackLabels[1] = AttackLabel2;
                blockLabels[1] = BlockLabel2;
            }
            if (pCount >= 3)
            {
                Player3Label.Visible = true;
                playerLabels[2] = Player3Label;
                attackLabels[2] = AttackLabel3;
                blockLabels[2] = BlockLabel3;
            }
            if (pCount >= 4)
            {
                Player4Label.Visible = true;
                playerLabels[3] = Player4Label;
                attackLabels[3] = AttackLabel4;
                blockLabels[3] = BlockLabel4;
            }

            Rectangle[] platformLayout = GameLogic.GameVariables.PlatformLayouts[ClientFuncs.levelLayout];
            if (platformLabels != null)
            {
                foreach (Label l in platformLabels)
                {
                    GamePanel.Controls.Remove(l);
                }
            }
            platformLabels = new Label[platformLayout.Length];
            for (int i = 0; i < platformLayout.Length; i++)
            {
                platformLabels[i] = new Label();
                platformLabels[i].Text = "";
                platformLabels[i].BackColor = Color.OliveDrab;
                platformLabels[i].Location = platformLayout[i].Location;
                platformLabels[i].Size = platformLayout[i].Size;
                GamePanel.Controls.Add(platformLabels[i]);
            }
        }

        public void DisablePanels()
        {
            MainMenuPanel.Enabled = false;
            MainMenuPanel.Visible = false;
            StartupPanel.Enabled = false;
            StartupPanel.Visible = false;
            GameHistoryPanel.Enabled = false;
            GameHistoryPanel.Visible = false;
            GamePanel.Enabled = false;
            GamePanel.Visible = false;
            TransitionPanel.Enabled = false;
            TransitionPanel.Visible = false;
        }

        private void LoadGameHistoryPanel()
        {
            SuspendLayout();
            DisablePanels();
            GameHistoryPanel.Enabled = true;
            GameHistoryPanel.Visible = true;
            HistoryTableLayoutPanel.RowCount = 2;
            List<Match> MatchHistory = ClientSockFuncs.GetMatchesWithUser(ClientFuncs.uName);
            Console.WriteLine("Recieved " + MatchHistory.Count() + " matches");
            foreach (Match match in MatchHistory)
            {
                AddMatchToTable(match);
            }
            ResumeLayout();
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

        private void Draw(GameState state)
        {
            string ScoreText = "";
            for (int i = 0; i < ClientFuncs.pCount; i++)
            {
                PlayerState playerI = state.PStates[i];
                if (i == 0)
                    ScoreText += "Blue: " + playerI.Stocks + " | ";
                else if (i == 1)
                    ScoreText += "Orange: " + playerI.Stocks + " | ";
                else if (i == 2)
                    ScoreText += "Yellow: " + playerI.Stocks + " | ";
                else if (i == 3)
                    ScoreText += "Purple: " + playerI.Stocks;
                if (playerI.Stocks > 0)
                {
                    playerLabels[i].Visible = true;
                    attackLabels[i].Visible = true;
                    playerLabels[i].BackColor = playerColors[i];
                    playerLabels[i].BorderStyle = BorderStyle.None;
                    Point intPos = playerI.Pos.ToPoint();
                    playerLabels[i].Location = intPos;
                    if (playerI.FacingLeft)
                        playerLabels[i].TextAlign = ContentAlignment.MiddleLeft;
                    else
                        playerLabels[i].TextAlign = ContentAlignment.MiddleRight;

                    if (playerI.AttackFrame > 0)
                    {
                        Attack attack = GameLogic.GameVariables.AttackDict[playerI.AttackName];
                        AnimHitbox[] anim = attack.Animation;
                        AnimHitbox ah = new AnimHitbox();
                        if (playerI.AttackFrame > attack.StartupF && playerI.AttackFrame < attack.StartupF + anim.Last().endF)
                        {
                            for (int j = 0; j < anim.Length; j++)
                            {
                                if (playerI.AttackFrame - attack.StartupF < anim[j].endF)
                                {
                                    ah = anim[j];
                                    break;
                                }
                            }
                            if (!playerI.FacingLeft)
                            {
                                attackLabels[i].Location = new Point(intPos.X + 25 + (int)ah.hitbox.X, intPos.Y + 25 + (int)ah.hitbox.Y);
                            }
                            else
                            {
                                attackLabels[i].Location = new Point(intPos.X + 25 - (int)ah.hitbox.X - (int)ah.hitbox.Width, intPos.Y + 25 + (int)ah.hitbox.Y);
                            }
                            attackLabels[i].Size = ah.hitbox.Size.ToSize();
                            attackLabels[i].Visible = true;
                        }
                        else
                        {
                            attackLabels[i].Visible = false;
                        }
                        playerLabels[i].BackColor = Color.FromArgb(playerLabels[i].BackColor.R - 50, playerLabels[i].BackColor.G - 50, playerLabels[i].BackColor.B - 50);
                    }
                    else
                    {
                        attackLabels[i].Visible = false;
                    }
                    if (playerI.BFrame == 0)
                    {
                        blockLabels[i].Location = new Point(intPos.X + 19, intPos.Y - 11 - 7);
                        blockLabels[i].Visible = true;
                    }
                    else if (playerI.BFrame < GameLogic.GameVariables.BlockDur)
                    {
                        playerLabels[i].BackColor = Color.Gray;
                        blockLabels[i].Visible = false;
                    }
                    if (playerI.StunFrame > 0)
                    {
                        playerLabels[i].BackColor = Color.Honeydew;
                        playerLabels[i].BorderStyle = BorderStyle.FixedSingle;
                    }
                    playerLabels[i].Text = playerI.KBPercent + "";
                }
                else
                {
                    playerLabels[i].Visible = false;
                    attackLabels[i].Visible = false;
                    blockLabels[i].Visible = false;
                }
            }
            ScoreLabel.Text = ScoreText;
        }

        private void JoinLobby()
        {
            ClientFuncs.JoinLobby();
            LoadTransitionPanel("Waiting for server...");
        }

        private void SignInButton_Click(object sender, EventArgs e)
        { 
            if (ClientSockFuncs.SignIn(UNameTextBox.Text, PassTextBox.Text))
            {
                ClientFuncs.uName = UNameTextBox.Text;
                LoadMainMenuPanel();
            }
            else
                ResponseLabel.Text = "User name or password is incorrect";
        }

        private void JoinLobbyButton_Click(object sender, EventArgs e)
        {
            JoinLobby();
        }

        private void OnJoinLobby()
        {
            GameLoopTimer.Interval = frameMS;
            GameLoopTimer.Enabled = true;
            LoadGamePanel();
        }

        private void OnJoinFail()
        {
            TransitionTextLabel.Text = "Game is already running.";
        }

        private void OnEndGame(string winner)
        {
            GameLoopTimer.Enabled = false;
            if (winner == "Tie")
            {
                LoadTransitionPanel("Game Over\nIt was a tie! (or the server ended early)");
            }
            else
            {
                LoadTransitionPanel("Game Over\n" + winner + " Won!");
            }
        }

        private void SignUpButton_Click(object sender, EventArgs e)
        {
            if (UNameTextBox.Text.Length == 0)
            {
                ResponseLabel.Text = "No user name entered";
            }
            else if (UNameTextBox.Text == "guest")
            {
                ResponseLabel.Text = "User name can't be 'guest'";
            }
            else if (PassTextBox.Text.Length < 8)
            {
                ResponseLabel.Text = "Password needs to be at least 8 characters long";
            }
            else
            {
                if (ClientSockFuncs.SignUp(UNameTextBox.Text, PassTextBox.Text))
                    ResponseLabel.Text = "Signed up successfully";
                else
                    ResponseLabel.Text = "User name already exists";
            }

        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            LoadStartupPanel();
        }

        private void ViewGameHistoryButton_Click(object sender, EventArgs e)
        {
            LoadGameHistoryPanel();
        }

        private void BackButtonGH_Click(object sender, EventArgs e)
        {
            LoadMainMenuPanel();
        }

        private void TransitionButton_Click(object sender, EventArgs e)
        {
            if (ClientFuncs.inLobby)
            {
                ClientSockFuncs.clientSockTcp.Send(new byte[1] { (byte)ClientMessageType.LeaveLobby });
            }
            if (ClientFuncs.uName == "guest")
            {
                LoadStartupPanel();
            }
            else
            {
                LoadMainMenuPanel();
            }
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            GameState toDraw = ClientFuncs.OnTimerTick(curInput, fullSim, clientSim, enemySim);
            if (toDraw != null)
                Draw(toDraw);
            if (fullSim)
                Text = "Fully simulating";
            else
            {
                Text = "Simulating seperatly: ";
                if (clientSim)
                    Text += "client simulation ";
                if (enemySim)
                    Text += "enemy extrapolation ";
                if (!clientSim && !enemySim)
                    Text += "nothing";

            }
        }

        private void Graphics_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    curInput |= Input.Right;
                    break;
                case Keys.A:
                    curInput |= Input.Left;
                    break;
                case Keys.W:
                    curInput |= Input.Up;
                    break;
                case Keys.S:
                    curInput |= Input.Down;
                    break;
                case Keys.Space:
                    curInput |= Input.Jump;
                    break;
                case Keys.L:
                    curInput |= Input.Block;
                    break;
                case Keys.J: 
                case Keys.H:
                    curInput |= Input.LAttack;
                    break;
                case Keys.K:
                    curInput |= Input.HAttack;
                    break;
                case Keys.I:
                    fullSim = !fullSim;
                    break;
                case Keys.O:
                    clientSim = !clientSim;
                    break;
                case Keys.P:
                    enemySim = !enemySim;
                    break;
            }
        }

        private void Graphics_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    curInput &= ~Input.Right;
                    break;
                case Keys.A:
                    curInput &= ~Input.Left;
                    break;
                case Keys.W:
                    curInput &= ~Input.Up;
                    break;
                case Keys.S:
                    curInput &= ~Input.Down;
                    break;
                case Keys.Space:
                    curInput &= ~Input.Jump;
                    break;
                case Keys.L:
                    curInput &= ~Input.Block;
                    break;
                case Keys.J:
                case Keys.H:
                    curInput &= ~Input.LAttack;
                    break;
                case Keys.K:
                    curInput &= ~Input.HAttack;
                    break;
            }
        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClientSockFuncs.clientSockTcp.Close();
        }

        private void TimeTimer_Tick(object sender, EventArgs e)
        {
            //TimeLabel.Text = "Time: " + DateTime.Now.ToString("mm.ss.fff") + ", Frame: " + curFNum;
        }
    }
}