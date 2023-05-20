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
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace RealTimeProject
{
    public static class NBConsole
    {
        private static BlockingCollection<string> m_Queue = new BlockingCollection<string>();

        static NBConsole()
        {
            var thread = new Thread(
              () =>
              {
                  //while (true) Console.WriteLine(m_Queue.Take());
              });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void WriteLine(string value)
        {
            //m_Queue.Add(value);
        }
    }

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
        public Vector2? knockback;

        public AnimHitbox(RectangleF hitbox, int endF, Vector2? knockback = null)
        {
            this.hitbox = hitbox;
            this.endF = endF;
            this.knockback = knockback;
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
        None = 0, NLight, SLight, ULight, DLight, NAir, SAir, UAir, DAir, NHeavy, SHeavy, UHeavy, DHeavy
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
        Block = 0b_0100_0000,
        Down = 0b_1000_0000
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
        public byte DownHoldFrame { get; set; }
        public const int sizeInBytes = 26;

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
            Stocks = 3;
            DownHoldFrame = 0;
        }

        public PlayerState(PlayerState other)
        {
            Pos = new Vector2(other.Pos.X, other.Pos.Y);
            Vel = new Vector2(other.Vel.X, other.Vel.Y);
            FacingLeft = other.FacingLeft;
            BFrame = other.BFrame;
            AttackName = other.AttackName;
            AttackFrame = other.AttackFrame;
            StunFrame = other.StunFrame;
            KBPercent = other.KBPercent;
            Jumps = other.Jumps;
            Stocks = other.Stocks;
            DownHoldFrame = other.DownHoldFrame;
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
            equal = equal && DownHoldFrame == other.DownHoldFrame;
            return equal;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeInBytes];
            byte[] tempB = Pos.ToBytes();
            tempB.CopyTo(bytes, 0);
            tempB = Vel.ToBytes();
            tempB.CopyTo(bytes, 8);
            if (FacingLeft)
                bytes[16] = 0x01;
            else
                bytes[16] = 0x00;
            bytes[17] = BFrame;
            bytes[18] = (byte)AttackName;
            tempB = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(tempB, AttackFrame);
            tempB.CopyTo(bytes, 19);
            bytes[21] = StunFrame;
            bytes[22] = KBPercent;
            bytes[23] = Jumps;
            bytes[24] = Stocks;
            bytes[25] = DownHoldFrame;
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
            pState.DownHoldFrame = bytes[25];
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
            s += ", Stocks = " + Stocks;
            s += ", DHold = " + DownHoldFrame + "]";
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

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[SizeInBytes(PStates.Length)];
            for (int i = 0; i < PStates.Length; i++)
            {
                PStates[i].ToBytes().CopyTo(bytes, i * PlayerState.sizeInBytes);
            }
            return bytes;
        }

        public static GameState FromBytes(byte[] bytes, int pCount)
        {
            PlayerState[] pStates = new PlayerState[pCount];
            for (int i = 0; i < pCount; i++)
            {
                pStates[i] = PlayerState.FromBytes(bytes[(i * PlayerState.sizeInBytes)..((i + 1) * PlayerState.sizeInBytes)]);
            }
            return new GameState(pStates);
        }

        public static int SizeInBytes(int pCount)
        {
            return PlayerState.sizeInBytes * pCount;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < PStates.Length; i++)
            {
                s += "p" + (i + 1) + ": " + PStates[i] + ", ";
            }
            s = s.Remove(s.Length - 2);
            return s;
        }
    }

    public class Frame
    {
        public DateTime StartTime { get; set; }
        public Input[] Inputs { get; set; }
        public GameState State { get; set; }

        public Frame(DateTime startTime, Input[] inputs, GameState state)
        {
            StartTime = startTime;
            Inputs = inputs;
            State = state;
        }


        public Frame(Frame other)
        {
            StartTime = other.StartTime;
            Inputs = new Input[other.Inputs.Length];
            other.Inputs.CopyTo(Inputs, 0);
            State = new GameState(other.State);
        }


        public bool IsEqual(Frame other)
        {
            bool equal = true;
            equal = equal && Enumerable.SequenceEqual(Inputs, other.Inputs);
            for (int i = 0; i < State.PStates.Length; i++)
            {
                equal = equal && State.PStates[i].IsEqual(other.State.PStates[i]);
            }
            return equal;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[SizeInBytes(Inputs.Length)];
            BinaryPrimitives.WriteInt64BigEndian(bytes, StartTime.Ticks);
            for (int i = 0; i < Inputs.Length; i++)
            {
                bytes[8 + i] = (byte)Inputs[i];
            }
            byte[] gsBytes = State.ToBytes();
            gsBytes.CopyTo(bytes, 8 + Inputs.Length);
            return bytes;
        }

        public static Frame FromBytes(byte[] bytes, int pCount)
        {
            DateTime startTime = new DateTime(BinaryPrimitives.ReadInt64BigEndian(bytes));
            Input[] inputs = new Input[pCount];
            for (int i = 0; i < pCount; i++)
            {
                inputs[i] = (Input)bytes[8 + i];
            }
            GameState state = GameState.FromBytes(bytes[(8 + pCount)..], pCount);
            return new Frame(startTime, inputs, state);
        }

        public override string ToString()
        {
            string s = "";
            s += "time: " + StartTime.ToString("mm.ss.fff") + ", ";
            s += "inputs: [";
            foreach (Input input in Inputs)
            {
                s += "(";
                s += input + "), ";
            }
            s = s.Remove(s.Length - 2);
            s += "], ";
            s += "state: " + State.ToString();
            return s;
        }

        public static int SizeInBytes(int pCount)
        {
            return 8 + pCount + GameState.SizeInBytes(pCount);
        }
    }

    public class ServerGamePacket
    {
        public DateTime TimeStamp { get; set; }
        public Frame Frame { get; set; }
        public Input[][] EnemyInputs { get; set; }
        public byte FrameMS { get; set; }

        public ServerGamePacket(DateTime timeStamp, Frame frame, Input[][] enemyInputs, byte frameMS)
        {
            TimeStamp = timeStamp;
            Frame = frame;
            EnemyInputs = enemyInputs;
            FrameMS = frameMS;
        }

        public override string ToString()
        {
            string s = "";
            s += "stamp: " + TimeStamp.ToString("mm.ss.fff") + ", ";
            s += "frame: " + Frame.ToString() + ", ";
            s += "inputs: [";
            for (int i = 0; i < EnemyInputs.Length; i++)
            {
                s += "[";
                for (int j = 0; j < EnemyInputs[i].Length; j++)
                {
                    s += "(" + EnemyInputs[i][j].ToString() + "),";
                }
                s += "], ";
            }
            s += "]";
            return s;
        }

        public byte[] Serialize(int pCount)
        {
            int eICount;
            if (EnemyInputs.Length == 0)
            {
                eICount = 0;
            }
            else
            {
                eICount = EnemyInputs[0].Length;
            }
            byte[] bytes = new byte[SizeInBytes(pCount, eICount)];
            BinaryPrimitives.WriteInt64BigEndian(bytes, TimeStamp.Ticks);
            Frame.ToBytes().CopyTo(bytes, 8);
            int eIStartI = 8 + Frame.SizeInBytes(pCount);
            for (int i = 0; i < pCount - 1; i++)
            {
                for (int j = 0; j < eICount; j++)
                {
                    bytes[eIStartI + i * eICount + j] = (byte)EnemyInputs[i][j];
                }
            }
            bytes[^1] = FrameMS;
            return bytes;
        }

        public static ServerGamePacket Deserialize(byte[] packet, int pCount)
        {
            DateTime timeStamp = new DateTime(BinaryPrimitives.ReadInt64BigEndian(packet[..8]));
            int frameEndIndex = 8 + Frame.SizeInBytes(pCount); // actually one after frame ends
            Frame frame = Frame.FromBytes(packet[8..frameEndIndex], pCount);
            Input[][] enemyInputs = new Input[pCount - 1][];
            byte[] eIBytes = packet[frameEndIndex..^1];
            int eICount = 0;
            if (pCount > 1)
                eICount = (packet.Length - frameEndIndex - 1) / (pCount - 1);
            for (int i = 0; i < pCount - 1; i++)
            {
                Input[] oneEnemyInput = new Input[eICount];
                for (int j = 0; j < eICount; j++)
                {
                    oneEnemyInput[j] = (Input)eIBytes[i * eICount + j];
                }
                enemyInputs[i] = oneEnemyInput;
            }
            byte frameMS = packet[^1];
            return new ServerGamePacket(timeStamp, frame, enemyInputs, frameMS);
        }

        public static int SizeInBytes(int pCount, int enemyInputCount)
        {
            return 8 + Frame.SizeInBytes(pCount) + ((pCount - 1) * enemyInputCount) + 1;
        }

    }
    public class ClientPacket
    {
        public byte[] Data { get; set; }
        public int Player { get; set; }

        public ClientPacket(int player, byte[] data)
        {
            Data = data;
            Player = player;
        }
    }

    public enum ClientMessageType : byte { GetMatchesWithUser, SignUp, CheckSignIn, JoinLobby, LeaveLobby, LeaveGame }
    public enum ServerMessageType : byte { None = 0, Success, Failure, GameEnd }

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public User(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

    }

    public class Match
    {
        public int Id { get; set; }
        public string StartTime { get; set; }
        public string Players { get; set; }
        public string Winner { get; set; }
        public string Length { get; set; }

        public Match(string startTime, string players, string winner, string length)
        {
            StartTime = startTime;
            Players = players;
            Winner = winner;
            Length = length;
        }
        public string[] GetProperyArray()
        {
            return new string[3] { StartTime, Players, Winner };
        }
    }

    public class LobbyPlayer
    {
        public string UName { get; set; }
        public int Number { get; set; }
        public Socket Sock { get; set; }
        public bool Disconnected { get; set; }

        public LobbyPlayer(string uName, int number, Socket sock)
        {
            UName = uName;
            Number = number;
            Sock = sock;
            Disconnected = false;
        }
    }
}
