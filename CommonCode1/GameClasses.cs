using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Drawing;

namespace RealTimeProject
{
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2(Point p)
        {
            X = p.X;
            Y = p.Y;
        }

        public Vector2(Size z)
        {
            X = z.Width;
            Y = z.Height;
        }

        public bool IsEqual(Vector2 other)
        {
            if (X == other.X && Y == other.Y)
                return false;
            return true;
        }

        public void Add(Vector2 other)
        {
            X += other.X;
            Y += other.Y;
        }

        public void Scale(float f)
        {
            X = (int)(X * f);
            Y = (int)(Y * f);
        }

        public Size ToSize()
        {
            return new Size((int)X, (int)Y);
        }

        public Point ToPoint()
        {
            return new Point((int)X, (int)Y);
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[8];
            byte[] xB = new byte[4];
            BinaryPrimitives.WriteSingleBigEndian(xB, X);
            byte[] yB = new byte[4];
            BinaryPrimitives.WriteSingleBigEndian(yB, Y);
            xB.CopyTo(bytes, 0);
            yB.CopyTo(bytes, 4);
            return bytes;
        }

        public static Vector2 FromBytes(byte[] bytes)
        {
            return new Vector2(BinaryPrimitives.ReadSingleBigEndian(bytes[..4]), BinaryPrimitives.ReadSingleBigEndian(bytes[4..]));
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }

    public struct AnimHitbox
    {
        public RectangleF hitbox;
        public int endF;

        public AnimHitbox(RectangleF hitbox, int endF)
        {
            this.hitbox = hitbox;
            this.endF = endF;
        }

    }

    public class Attack
    {
        public AnimHitbox[] Animation { get; }
        public Vector2 Knockback { get; }
        public byte StunF { get; }
        public int StartupF { get; }
        public int RecoveryF { get; }

        public Attack(AnimHitbox[] animation, Vector2 knockback, byte stunF, int startupF, int recoveryF)
        {
            Animation = animation;
            Knockback = knockback;
            StunF = stunF;
            StartupF = startupF;
            RecoveryF = recoveryF;
        }
    }

    public enum AttackName : byte
    {
        None = 0, NLight, SLight, ULight, NAir, NHeavy, SHeavy, UHeavy
    }

    [Flags]
    public enum Input : byte
    {
        None = 0,
        Right = 0b_0000_0001,
        Left = 0b_0000_0010,
        Up = 0b_0000_0100,
        Jump = 0b_0000_1000,
        LAttack = 0b_0001_0000,
        HAttack = 0b_0010_0000,
        Block = 0b_0100_0000
    }

    public class GameVariables
    {
        public static Rectangle Bounds { get; set; }
        public static int FloorY { get; set; }
        public static float Gravity { get; set; }
        public static Size PlayerSize { get; set; }
        public static float BaseMS { get; set; }
        public static float Friction { get; set; }
        public static float MaxMS { get; set; }
        public static float BaseJS { get; set; }
        public static int BlockCD { get; set; }
        public static int BlockDur { get; set; }
        public static Dictionary<AttackName, Attack> AttackDict { get; }
        static GameVariables()
        {
            Bounds = new Rectangle(-50, -50, 1086, 702);
            FloorY = 457;
            Gravity = 0.8f;
            PlayerSize = new Size(50, 50);
            BaseMS = 5;
            Friction = 0.5f;
            MaxMS = 5;
            BaseJS = 16;
            BlockCD = 40;
            BlockDur = 10;
            AttackDict = new Dictionary<AttackName, Attack>();
            AttackDict[AttackName.NLight] = new Attack(
                new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(25, -9), new Size(15, 18)), 8),
                                   new AnimHitbox(new Rectangle(new Point(25, -9), new Size(20, 18)), 12),
                                   new AnimHitbox(new Rectangle(new Point(25, -9), new Size(50, 18)), 20)}, new Vector2(-6, -20), 20, 5, 0);
            AttackDict[AttackName.SLight] = new Attack(
                new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(25, -7), new Size(25, 14)), 5),
                                   new AnimHitbox(new Rectangle(new Point(25, -7), new Size(50, 14)), 10),
                                   new AnimHitbox(new Rectangle(new Point(25, -7), new Size(70, 14)), 15),
                                   new AnimHitbox(new Rectangle(new Point(25, -7), new Size(80, 14)), 20)}, new Vector2(10, -8), 20, 10, 0);
            AttackDict[AttackName.ULight] = new Attack(
                new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(-9, -25 - 15), new Size(18, 15)), 8),
                                   new AnimHitbox(new Rectangle(new Point(-9, -25 - 20), new Size(18, 20)), 12),
                                   new AnimHitbox(new Rectangle(new Point(-9, -25 - 60), new Size(18, 60)), 23)}, new Vector2(0, -20), 20, 10, 0);
            AttackDict[AttackName.NAir] = new Attack(
                new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(-30, -30), new Size(60, 60)), 5),
                                   new AnimHitbox(new Rectangle(new Point(-35, -35), new Size(70, 70)), 15)}, new Vector2(0, -10), 20, 10, 10);
            AttackDict[AttackName.SHeavy] = new Attack(
                new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(25, -10), new Size(20, 20)), 5),
                                   new AnimHitbox(new Rectangle(new Point(50, -50), new Size(40, 40)), 15),
                                   new AnimHitbox(new Rectangle(new Point(20, -120), new Size(60, 60)), 25),
                                   new AnimHitbox(new Rectangle(new Point(-60, -170), new Size(80, 80)), 35)}, new Vector2(-20, -20), 20, 30, 20);
            AttackDict[AttackName.NHeavy] = new Attack(
                new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(-50, -37), new Size(100, 62)), 5),
                                   new AnimHitbox(new Rectangle(new Point(-100, -60), new Size(200, 85)), 20)}, new Vector2(20, -20), 20, 30, 20);
            AttackDict[AttackName.UHeavy] = new Attack(
                new AnimHitbox[] { new AnimHitbox(new Rectangle(new Point(-7, -25 - 600), new Size(14, 600)), 7),
                                   new AnimHitbox(new Rectangle(new Point(-30, -25 - 600), new Size(60, 600)), 18),
                                   new AnimHitbox(new Rectangle(new Point(-40, -25 - 600), new Size(80, 600)), 25)}, new Vector2(5, -50), 20, 30, 20);
        }
    }

    public class PlayerState
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public bool FacingLeft { get; set; }
        public byte BFrame { get; set; }
        public AttackName AttackName { get; set; }
        public ushort AttackFrame { get; set; }
        public byte StunFrame { get; set; }
        public byte KBPercent { get; set; }
        public byte Jumps { get; set; }
        public byte Stocks { get; set; }

        public PlayerState(Vector2 pos, bool facingLeft)
        {
            Pos = pos;
            FacingLeft = facingLeft;

            Vel = new Vector2(0, 0);
            BFrame = 0;
            AttackName = AttackName.None;
            AttackFrame = 0;
            StunFrame = 0;
            KBPercent = 50;
            Jumps = 2;
            Stocks = 1;
        }

        public PlayerState(PlayerState other)
        {
            Pos = other.Pos;
            Vel = other.Vel;
            FacingLeft = other.FacingLeft;
            BFrame = other.BFrame;
            AttackName = other.AttackName;
            AttackFrame = other.AttackFrame;
            StunFrame = other.StunFrame;
            KBPercent = other.KBPercent;
            Jumps = other.Jumps;
            Stocks = other.Stocks;
        }

        public bool IsEqual(PlayerState other)
        {
            bool equal = true;
            equal = equal && Pos.IsEqual(other.Pos);
            equal = equal && Vel.IsEqual(other.Pos);
            equal = equal && FacingLeft == other.FacingLeft;
            equal = equal && BFrame == other.BFrame;
            equal = equal && AttackName == other.AttackName;
            equal = equal && AttackFrame == other.AttackFrame;
            equal = equal && StunFrame == other.StunFrame;
            equal = equal && KBPercent == other.KBPercent;
            equal = equal && Jumps == other.Jumps;
            equal = equal && Stocks == other.Stocks;
            return equal;
        }
        
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[25];
            byte[] tempB = Pos.ToBytes();
            tempB.CopyTo(bytes, 0);
            tempB = Vel.ToBytes();
            tempB.CopyTo(bytes, 8);
            if (FacingLeft)
                bytes[16] = 0x00;
            else
                bytes[16] = 0x01;
            bytes[17] = BFrame;
            bytes[18] = (byte)AttackName;
            tempB = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(tempB, AttackFrame);
            bytes[21] = StunFrame;
            bytes[22] = KBPercent;
            bytes[23] = Jumps;
            bytes[24] = Stocks;
            return bytes;
        }

        public static PlayerState FromBytes(byte[] bytes)
        {
            PlayerState pState = new PlayerState(new Vector2(0, 0), false);
            pState.Pos = Vector2.FromBytes(bytes[0..8]);
            pState.Vel = Vector2.FromBytes(bytes[8..16]);
            if (bytes[16] == 0x01)
                pState.FacingLeft = true;
            pState.BFrame = bytes[17];
            pState.AttackName = (AttackName)bytes[18];
            pState.AttackFrame = BinaryPrimitives.ReadUInt16BigEndian(bytes[19..21]);
            pState.StunFrame = bytes[21];
            pState.KBPercent = bytes[22];
            pState.Jumps = bytes[23];
            pState.Stocks = bytes[24];
            return pState;
        }
        public override string ToString()
        {
            string s = "[";
            s += "Pos = " + Pos;
            s += ", Vel = " + Vel;
            s += ", Left = " + FacingLeft;
            s += ", BFrame = " + BFrame;
            s += ", Atk = " + AttackName;
            s += ", AFrame = " + AttackFrame;
            s += ", KBp = " + KBPercent;
            s += ", Jumps = " + Jumps;
            s += ", Stocks = " + Stocks + "]";
            return s;
        }
    }

    public class GameState
    {
        public PlayerState[] PStates { get; }

        public GameState(PlayerState[] pStates)
        {
            PStates = pStates;
        }

        public GameState(GameState other)
        {
            PlayerState[] pStates = new PlayerState[other.PStates.Length];
            for (int i = 0; i < other.PStates.Length; i++)
            {
                pStates[i] = new PlayerState(other.PStates[i]);
            }
            PStates = pStates;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < PStates.Length; i++)
            {
                s += "p" + (i + 1) + ": " + PStates[i] + ", ";
            }
            s.Remove(s.Length - 2);
            return s;
        }

        public static GameState NextState(Input[] prevInputs, Input[] curInputs, GameState state)
        {
            GameState nextState = new GameState(state);
            int pCount = prevInputs.Length;
            Vector2[] accArr = new Vector2[pCount];
            for (int i = 0; i < pCount; i++)
            {
                accArr[i] = new Vector2(0, 0);
            }
            //block handling
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = nextState.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                if ((curInputs[i] & Input.Block) != 0 && (prevInputs[i] & Input.Block) == 0 && playerI.BFrame == 0)
                {
                    if (playerI.StunFrame == 0)
                    {
                        playerI.BFrame++;
                    }
                }
                else if (playerI.BFrame > 0)
                {
                    playerI.BFrame++;
                }
                if (playerI.BFrame == GameVariables.BlockDur + GameVariables.BlockCD + 1)
                {
                    playerI.BFrame = 0;
                }
            }
            //attack handling
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = nextState.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                bool isOnFloor = false;
                if (playerI.Pos.Y == GameVariables.FloorY - GameVariables.PlayerSize.Height)
                {
                    isOnFloor = true;
                }
                if ((curInputs[i] & Input.LAttack) != 0 && (prevInputs[i] & Input.LAttack) == 0 && playerI.AttackFrame == 0)
                {
                    if ((curInputs[i] & Input.Up) != 0)
                        playerI.AttackName = AttackName.ULight;
                    else if ((curInputs[i] & (Input.Right | Input.Left)) != 0)
                        playerI.AttackName = AttackName.SLight;
                    else
                    {
                        if (isOnFloor)
                            playerI.AttackName = AttackName.NLight;
                        else
                            playerI.AttackName = AttackName.NAir;
                    }
                }
                else if ((curInputs[i] & Input.HAttack) != 0 && (prevInputs[i] & Input.HAttack) == 0 && playerI.AttackFrame == 0)
                {
                    if (isOnFloor)
                    {
                        if ((curInputs[i] & Input.Up) != 0)
                            playerI.AttackName = AttackName.UHeavy;
                        else if ((curInputs[i] & (Input.Right | Input.Left)) != 0)
                            playerI.AttackName = AttackName.SHeavy;
                        else
                            playerI.AttackName = AttackName.NHeavy;
                    }
                }
                if (playerI.AttackName != AttackName.None)
                {
                    Attack attack = GameVariables.AttackDict[playerI.AttackName];
                    AnimHitbox[] anim = attack.Animation;
                    AnimHitbox ah = new AnimHitbox();
                    playerI.AttackFrame++;
                    if (attack.StartupF < playerI.AttackFrame && playerI.AttackFrame <= attack.StartupF + anim.Last().endF)
                    {
                        for (int j = 0; j < pCount; j++)
                        {
                            PlayerState playerJ = nextState.PStates[j];
                            if (j != i)
                            {
                                for (int k = 0; k < anim.Length; k++)
                                {
                                    if (playerI.AttackFrame - attack.StartupF <= anim[k].endF)
                                    {
                                        ah = anim[k];
                                        break;
                                    }
                                }
                                RectangleF actualHitbox;
                                if (!playerI.FacingLeft)
                                {
                                    actualHitbox = new RectangleF(new PointF(playerI.Pos.X + 25 + ah.hitbox.X, playerI.Pos.Y + 25 + ah.hitbox.Y),
                                                                    ah.hitbox.Size);
                                }
                                else
                                {
                                    actualHitbox = new RectangleF(new PointF(playerI.Pos.X + 25 - ah.hitbox.X - ah.hitbox.Width,
                                        playerI.Pos.Y + 25 + ah.hitbox.Y), ah.hitbox.Size);
                                }
                                if (new RectangleF(playerJ.Pos.ToPoint(), GameVariables.PlayerSize).IntersectsWith(actualHitbox))
                                {
                                    if (playerJ.BFrame > 0 && playerJ.BFrame < GameVariables.BlockDur + 1)
                                    {
                                        playerI.AttackFrame = 0;
                                        playerI.StunFrame = attack.StunF;
                                        playerI.AttackName = AttackName.None;
                                    }
                                    else
                                    {
                                        if (playerJ.StunFrame == 0)
                                        {
                                            playerJ.StunFrame = attack.StunF;
                                            playerJ.KBPercent += 2;
                                        }
                                        if (!playerI.FacingLeft)
                                        {
                                            accArr[j].Add(attack.Knockback);
                                        }
                                        else
                                        {
                                            accArr[j].Add(new Vector2(-attack.Knockback.X, attack.Knockback.Y));
                                        }
                                        accArr[j].Scale((float)playerJ.KBPercent / 100);
                                    }
                                }
                            }
                        }
                    }
                    if (playerI.AttackFrame > 0)
                    {
                        if (playerI.AttackFrame == anim.Last().endF + attack.StartupF + attack.RecoveryF)
                        {
                            playerI.AttackName = AttackName.None;
                            playerI.AttackFrame = 0;
                        }
                    }
                }
            }
            //movement handeling
            for (int i = 0; i < pCount; i++)
            {
                PlayerState playerI = nextState.PStates[i];
                if (playerI.Stocks == 0)
                    continue;
                Vector2 playerV = playerI.Vel;
                float WalkV = 0f;
                bool isOnFloor = false;
                if (playerI.Pos.Y == GameVariables.FloorY - GameVariables.PlayerSize.Height)
                {
                    isOnFloor = true;
                }
                if (playerI.StunFrame == 0)
                {
                    if (playerI.BFrame == 0 || playerI.BFrame > GameVariables.BlockDur)
                    {
                        if (playerI.AttackFrame == 0)
                        {
                            if ((curInputs[i] & Input.Right) != 0)
                            {
                                WalkV = GameVariables.BaseMS;
                                playerI.FacingLeft = false;
                            }
                            else if ((curInputs[i] & Input.Left) != 0)
                            {
                                WalkV = -GameVariables.BaseMS;
                                playerI.FacingLeft = true;
                            }

                            if ((curInputs[i] & Input.Jump) != 0 && (prevInputs[i] & Input.Jump) == 0 && playerI.Jumps != 0)
                            {
                                accArr[i].Y = -GameVariables.BaseJS;
                                playerI.Jumps -= 1;
                            }
                            else if ((curInputs[i] & Input.Jump) == 0 && (prevInputs[i] & Input.Jump) != 0 && playerI.Vel.Y < 0)
                            {
                                playerI.Vel.Y = playerI.Vel.Y * 0.4f;
                            }
                        }
                        else
                        {
                            if (playerI.Pos.Y < GameVariables.FloorY - GameVariables.PlayerSize.Height)
                            {
                                WalkV = playerI.Vel.X;
                            }
                        }
                    }
                }
                else
                {
                    playerI.StunFrame--;
                }
                if (accArr[i].X != 0 || accArr[i].Y != 0)
                {
                    playerI.Vel = accArr[i];
                }
                playerI.Vel.Y += GameVariables.Gravity;

                playerI.Pos.Add(playerV);
                if (playerI.Pos.Y >= GameVariables.FloorY - GameVariables.PlayerSize.Height && playerI.Vel.Y > 0)
                {
                    playerI.Pos = new Vector2(playerI.Pos.X, GameVariables.FloorY - GameVariables.PlayerSize.Height);
                    playerI.Vel.Y = 0;
                    playerI.Jumps = 2;
                }
                if (playerV.X > GameVariables.BaseMS || playerV.X < -GameVariables.BaseMS || playerI.StunFrame > 0)
                {
                    if (playerI.Pos.Y == GameVariables.FloorY - GameVariables.PlayerSize.Height)
                    {
                        playerV.X = (int)(playerV.X * GameVariables.Friction);
                    }
                }
                else
                {
                    playerV.X = WalkV;
                }
                if (!GameVariables.Bounds.IntersectsWith(new Rectangle(playerI.Pos.ToPoint(), GameVariables.PlayerSize)))
                {
                    playerI.Stocks -= 1;
                    playerI.Pos = new Vector2(400, 250);
                    playerI.Vel = new Vector2(0, 0);
                }
                Console.WriteLine("Player " + (i + 1) + ": " + playerI.Pos + ", " + playerI.Vel);
            }
            return nextState;
        }

        public static GameState InitialState(int pCount)
        {
            PlayerState[] pStates = new PlayerState[pCount];
            for (int i = 0; i < pCount; i++)
            {
                if (i == 1)
                    pStates[i] = new PlayerState(new Vector2(304, 407), false);
                else if (i == 2)
                    pStates[i] = new PlayerState(new Vector2(604, 407), true);
                else if (i == 3)
                    pStates[i] = new PlayerState(new Vector2(180, 407), false);
                else if (i == 4)
                    pStates[i] = new PlayerState(new Vector2(710, 407), true);
            }
            return new GameState(pStates);
        }
    }

    public class Frame
    {
        public DateTime startTime;
        public Input[] inputs;
        public GameState state;

        public Frame(DateTime startTime, Input[] inputs, GameState state)
        {
            this.startTime = startTime;
            this.inputs = inputs;
            this.state = state;
        }


        public Frame(Frame other)
        {
            startTime = other.startTime;
            inputs = new Input[other.inputs.Length];
            other.inputs.CopyTo(inputs, 0);
            state = new GameState(other.state);
        }


        public bool IsEqual(Frame other)
        {
            bool equal = true;
            equal = equal && Enumerable.SequenceEqual(inputs, other.inputs);
            for (int i = 0; i < state.PStates.Length; i++)
            {
                equal = equal && state.PStates[i].IsEqual(other.state.PStates[i]);
            }
            return equal;
        }


        public override string ToString()
        {
            string s = "";
            s += "time: " + startTime.ToString("mm.ss.fff") + ", ";
            s += "inputs: [";
            foreach (Input input in inputs)
            {
                s += input + ", ";
            }
            s = s.Remove(s.Length - 2);
            s += "], ";
            s += "state: " + state.ToString();
            return s;
        }
    }

    public class ServerPacket
    {
        public DateTime timeStamp;
        public Frame frame;
        public string[][] enemyInputs;

        public ServerPacket(DateTime timeStamp, Frame frame, string[][] enemyInputs)
        {
            this.timeStamp = timeStamp;
            this.frame = frame;
            this.enemyInputs = enemyInputs;
        }

        public static ServerPacket Deserialize(string packet, int pCount)
        {
            JsonElement[] recvData = JsonSerializer.Deserialize<JsonElement[]>(packet);
            DateTime recvTimeStamp = new DateTime(BinaryPrimitives.ReadInt64BigEndian(recvData[0].Deserialize<byte[]>()));
            DateTime recvStartTime = new DateTime(BinaryPrimitives.ReadInt64BigEndian(recvData[1].Deserialize<byte[]>()));
            string[] recvInputs = recvData[2].Deserialize<string[]>();
            int[] recvPos = recvData[3].Deserialize<int[]>();
            int[] recvPoints = recvData[4].Deserialize<int[]>();
            int[] recvBFrames = recvData[5].Deserialize<int[]>();
            char[] recvDirs = recvData[6].Deserialize<char[]>();
            int[] recvAttacks = recvData[7].Deserialize<int[]>();
            string[][] enemyInputs = new string[pCount - 1][];
            for (int i = 0; i < pCount - 1; i++)
            {
                enemyInputs[i] = recvData[8 + i].Deserialize<string[]>();
            }
            return new ServerPacket(recvTimeStamp, new Frame(recvStartTime, recvInputs, new GameState(recvPos, recvPoints, recvBFrames, recvDirs, recvAttacks)), enemyInputs);
        }

        static byte[] Serialize(byte[] timeStamp, Frame state, Input[][] enemyInputs)
        {

            byte[] sendData = new byte[25 * pCount];
            object[] sendData = new object[7 + pCount];
            sendData[0] = timeStamp;
            byte[] frameTime = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(frameTime, state.startTime.Ticks);
            sendData[1] = frameTime;
            sendData[2] = state.inputs;
            sendData[3] = state.state.positions;
            sendData[4] = state.state.points;
            sendData[5] = state.state.blockFrames;
            sendData[6] = state.state.dirs;
            sendData[7] = state.state.attacks;
            for (int i = 0; i < pCount - 1; i++)
            {
                sendData[8 + i] = enemyInputs[i];
            }
            string jsonString = JsonSerializer.Serialize(sendData);
            NBConsole.WriteLine(jsonString);
            return Encoding.Latin1.GetBytes(jsonString);
        }
    }
}
