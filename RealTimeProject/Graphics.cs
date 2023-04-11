using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;
using System.Text.Json;
using System.Buffers.Binary;
using System.Collections.Concurrent;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        int curFNum = 1, recvFNum = 0, frameMS = 15, thisPlayer, pCount;
        bool fullSim = true, clientSim = true, enemySim = false;
        Color[] playerColors = new Color[4] { Color.MediumTurquoise, Color.Coral, Color.FromArgb(255, 255, 90), Color.MediumPurple };

        Input curInput = new Input();
        List<Input> unackedInputs = new List<Input>(20);
        Label[] playerLabels, blockLabels, attackLabels;
        static List<Frame> simHistory = new List<Frame>();
        ServerGamePacket lastServerPacket;
        Socket clientSock = new Socket(SocketType.Dgram, ProtocolType.Udp);
        EndPoint serverEP;
         
        byte[] buffer = new byte[1024];
        public static class NBConsole
        {
            private static BlockingCollection<string> m_Queue = new BlockingCollection<string>();

            static NBConsole()
            {
                var thread = new Thread(
                  () =>
                  {
                      while (true) Console.WriteLine(m_Queue.Take());
                  });
                thread.IsBackground = true;
                thread.Start();
            }

            public static void WriteLine(string value)
            {
                m_Queue.Add(value);
            }
        }

        public Graphics()
        {
            InitializeComponent();
            InitializeConnection();
            InitializeGame();
        }

        private void InitializeConnection()
        {
            IPAddress autoAdress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            List<string> adresses = new List<string> { "172.16.2.167", "10.100.102.20", "192.168.68.112", "172.16.94.163", "172.16.5.133", "172.16.149.199" };
            int sPort = 12345;
            NBConsole.WriteLine("Enter port for client: ");
            int cPort = int.Parse(Console.ReadLine());
            var sAddress = IPAddress.Parse(adresses[1]);
            var cAddress = IPAddress.Parse(adresses[1]);
            EndPoint clientEP = new IPEndPoint(cAddress, cPort);
            serverEP = new IPEndPoint(sAddress, sPort);

            clientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            clientSock.Bind(clientEP);
            clientSock.SendTo(new byte[] { (byte)'a' }, serverEP);
            NBConsole.WriteLine("Waiting server reply");
            EndPoint recieveEP = new IPEndPoint(IPAddress.Any, 0);
            clientSock.ReceiveFrom(buffer, ref recieveEP);
            thisPlayer = int.Parse(Encoding.Latin1.GetString(buffer).TrimEnd('\0')[0] + "");
            pCount = int.Parse(Encoding.Latin1.GetString(buffer).TrimEnd('\0')[1] + "");
            NBConsole.WriteLine("You are player " + thisPlayer);
            
            GameLoopTimer.Interval = frameMS;
            Thread.Sleep(200);
            GameLoopTimer.Enabled = true;

            if (fullSim)
                Text = "Simulating";
            else
                Text = "Not Simulating";
        }

        private void InitializeGame()
        {
            GameLogic.GameVariables.Bounds = new Rectangle(Bounds.X - 50, Bounds.Y - 50, Bounds.Width + 100, Bounds.Height + 100);
            GameLogic.GameVariables.Gravity = 0.8f;
            GameLogic.GameVariables.FloorY = FloorLabel.Location.Y;

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
            if (pCount == 1)
                simHistory.Add(new Frame(DateTime.MinValue, new Input[] { Input.None }, GameLogic.InitialState(1)));
            else if (pCount == 2)
                simHistory.Add(new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None }, GameLogic.InitialState(2)));
            else if (pCount == 3)
                simHistory.Add(new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None, Input.None }, GameLogic.InitialState(3)));
            else if (pCount == 4)
                simHistory.Add(new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None, Input.None, Input.None }, GameLogic.InitialState(4)));
        }

        private void Draw(GameState state)
        {
            string ScoreText = "";
            for (int i = 0; i < pCount; i++)
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


        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            DateTime frameStart = DateTime.Now;
            NBConsole.WriteLine("Current input: [" + curInput + "]" + " current frame num: " + curFNum + " frameStart: " + frameStart.ToString("mm.ss.fff"));
            byte[] timeStamp = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(timeStamp, frameStart.Ticks);
            byte[] sendData = new byte[8 + 1];
            sendData[0] = (byte)curInput;
            timeStamp.CopyTo(sendData, 1);
            clientSock.SendTo(sendData, SocketFlags.None, serverEP);
            //NBConsole.WriteLine(inputBytes.Length + " | " + Convert.ToHexString(inputBytes) + " | " + Convert.ToHexString(timeStamp) + ", " + new DateTime(BinaryPrimitives.ReadInt64BigEndian(timeStamp)).ToString("mm.ss.fff") + " | " + Convert.ToHexString(sendData));

            //NBConsole.WriteLine("Getting packets from server");
            List<byte[]> packets = new List<byte[]>(); // get packets from server
            while (clientSock.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1024];
                EndPoint recieveEP = new IPEndPoint(IPAddress.Any, 0);
                int packetLen = 0;
                try
                {
                    packetLen = clientSock.ReceiveFrom(buffer, ref recieveEP);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("Server closed");
                    GameLoopTimer.Enabled = false;
                    return;
                }
                packets.Add(buffer[..packetLen]);
                //NBConsole.WriteLine("Recieved " + packets.Last());
            }
            if (packets.Count == 0) { NBConsole.WriteLine("no server data recieved"); }
            else { NBConsole.WriteLine("got " + packets.Count + " packets"); }

            //resimulation
            Input[] simInputs = new Input[pCount]; // create simulated frame
            simHistory.Last().Inputs.CopyTo(simInputs, 0);
            simInputs[thisPlayer - 1] = curInput;
            simHistory.Add(new Frame(frameStart, simInputs, GameLogic.NextState(simHistory.Last().Inputs, simInputs, simHistory.Last().State)));
            curFNum++;

            if (packets.Count() > 0) // deserialize packets and apply to simulated history
            {
                bool foundFrame = false;
                foreach (byte[] packet in packets)
                {
                    if (packet.Length == 1)
                    {
                        NBConsole.WriteLine("Server Closed");
                        GameLoopTimer.Enabled = false;
                        return;
                    }
                }
                ServerGamePacket servPacket = ServerGamePacket.Deserialize(packets.Last(), pCount); // pick the latest frame because can arrive out of order
                for (int i = 0; i < packets.Count - 1; i++)
                {
                    ServerGamePacket temp = ServerGamePacket.Deserialize(packets[i], pCount);
                    if (temp.TimeStamp > servPacket.TimeStamp)
                    {
                        servPacket = temp;
                    }
                }
                lastServerPacket = servPacket;
                unackedInputs.Clear();
                NBConsole.WriteLine("applying data from " + servPacket.TimeStamp.ToString("mm.ss.fff") + ". data:\n" + servPacket.ToString());
                //client simulation and lagcomp on enemies
                for (int i = simHistory.Count() - 1; i > 0; i--)
                {
                    if (simHistory[i - 1].StartTime <= servPacket.Frame.StartTime)
                    {
                        foundFrame = true;
                        NBConsole.WriteLine("resimulation start: " + i + " history end: " + (simHistory.Count - 1) + ", count: " + (simHistory.Count - 1 - i));
                        if (pCount > 1)
                            NBConsole.WriteLine(" server sent " + servPacket.EnemyInputs[0].Length + " eInputs");
                        simHistory[i] = servPacket.Frame;
                        for (int j = i + 1; j < simHistory.Count(); j++)
                        {
                            Input[] correctInputs = new Input[pCount];
                            for (int k = 0; k < pCount; k++)
                            {
                                if (k == thisPlayer - 1)
                                {
                                    correctInputs[k] = simHistory[j].Inputs[k];
                                    unackedInputs.Add(correctInputs[k]);
                                }
                                else
                                {
                                    int offset = 0;
                                    if (k > thisPlayer - 1)
                                        offset = -1;
                                    if (servPacket.EnemyInputs[k + offset].Length > j - i - 1)
                                    {
                                        correctInputs[k] = servPacket.EnemyInputs[k + offset][j - i - 1];
                                        //NBConsole.WriteLine(correctInputs[k].ToString());
                                    }
                                    else
                                    {
                                        correctInputs[k] = simHistory[j - 1].Inputs[k];
                                        //NBConsole.WriteLine(correctInputs[k].ToString());
                                    }
                                }
                            }
                            simHistory[j].State = GameLogic.NextState(simHistory[j - 1].Inputs, correctInputs, simHistory[j - 1].State);
                            simHistory[j].Inputs = correctInputs;
                            //NBConsole.WriteLine("P" + thisPlayer + ": pInput= (" + simHistory[j - 1].Inputs[thisPlayer - 1] + "), cInput= (" + correctInputs[thisPlayer - 1] + "), " +
                            //    "start state = " + simHistory[j - 1].State + ", after: Pos= " + simHistory[j].State.PStates[thisPlayer - 1].Pos + ", Vel= " + simHistory[j].State.PStates[thisPlayer - 1].Vel);
                        }
                        break;
                    }
                }
                if (!foundFrame)
                {
                    NBConsole.WriteLine("didn't find :(");
                    throw new Exception();
                }
            }
            else
            {
                unackedInputs.Add(curInput);
            }

            if (fullSim)
            {
                // show the final frame from the simulated history (with causality)
                NBConsole.WriteLine("updated state: " + simHistory.Last().State.ToString());
                Draw(simHistory.Last().State);
                NBConsole.WriteLine("Took " + (DateTime.Now - frameStart).Milliseconds);
            }
            else
            {
                // show the frame after the last player action recieved from server, no extra simulation
                if (!clientSim && !enemySim)
                {
                    if (packets.Count() > 0) // deserialize packets and apply to simulated history
                    {
                        byte[] latestPacket = packets.Last(); // later pick the highest frame because can arrive out of order
                        ServerGamePacket servPacket = ServerGamePacket.Deserialize(latestPacket, pCount);
                        NBConsole.WriteLine("applying data from " + servPacket.TimeStamp.ToString("mm.ss.fff") + 
                            " during frame that started at " + frameStart.ToString("mm.ss.fff"));
                        Draw(servPacket.Frame.State);
                    }
                }
                else
                {
                    if (lastServerPacket != null)
                    {
                        GameState stateToDraw = new GameState(lastServerPacket.Frame.State);
                        // simulate the catch-up frames for the enemies sent by the server
                        for (int i = 0; i < pCount; i++)
                        {
                            if (i != thisPlayer - 1)
                            {
                                int offset = 0;
                                if (i > thisPlayer - 1)
                                {
                                    offset = -1;
                                }
                                Input[] enemyInputs  = new Input[lastServerPacket.EnemyInputs[i + offset].Length + 1];
                                enemyInputs[0] = lastServerPacket.Frame.Inputs[i];
                                lastServerPacket.EnemyInputs[i + offset].CopyTo(enemyInputs, 1);
                                stateToDraw.PStates[i] = GameLogic.SimulatePlayerState(stateToDraw.PStates[i], enemyInputs);
                            }
                        }
                        //simulate client inputs that didn't reach the server
                        if (clientSim)
                        {
                            stateToDraw.PStates[thisPlayer - 1] = GameLogic.SimulatePlayerState(stateToDraw.PStates[thisPlayer - 1], unackedInputs.ToArray());
                        }
                        if (enemySim)
                        {
                            for (int i = 0; i < pCount; i++)
                            {
                                if (i != thisPlayer - 1)
                                {
                                    int offset = 0;
                                    if (i > thisPlayer - 1)
                                    {
                                        offset = -1;
                                    }
                                    Input[] enemyInputs = new Input[unackedInputs.Count - lastServerPacket.EnemyInputs.Length + 1];
                                    for (int j = 0; j < enemyInputs.Length; j++)
                                    {
                                        enemyInputs[j] = lastServerPacket.EnemyInputs[i + offset][^1];
                                    }
                                    stateToDraw.PStates[i] = GameLogic.SimulatePlayerState(stateToDraw.PStates[i], enemyInputs);
                                }
                            }
                        }
                        Draw(stateToDraw);
                    }
                }
            }

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
            //old history deletion, maybe needed?
            //if (simHistory.Count >= 200)
            //{
            //    simHistory.RemoveRange(0, 100);
            //}
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
                case Keys.Space:
                    curInput |= Input.Jump;
                    break;
                case Keys.L:
                    curInput |= Input.Block;
                    break;
                case Keys.J:
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
                case Keys.Space:
                    curInput &= ~Input.Jump;
                    break;
                case Keys.L:
                    curInput &= ~Input.Block;
                    break;
                case Keys.J:
                    curInput &= ~Input.LAttack;
                    break;
                case Keys.K:
                    curInput &= ~Input.HAttack;
                    break;
            }
        }

        private void TimeTimer_Tick(object sender, EventArgs e)
        {
            TimeLabel.Text = "Time: " + DateTime.Now.ToString("mm.ss.fff") + ", Frame: " + curFNum;
        }
    }
}