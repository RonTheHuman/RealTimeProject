using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeProject
{
    public class GameState
    {
        static int speed = 5, blockCD = 5, blockDur = 40;
        public int[] positions;
        public int[] points;
        public int[] blockFrames;
        public char[] dirs;
        public int[] attacks;

        public GameState(int[] positions, int[] points, int[] blockFrames, char[] dirs, int[] attacks)
        {
            this.positions = positions;
            this.points = points;
            this.blockFrames = blockFrames;
            this.dirs = dirs;
            this.attacks = attacks;
        }

        public GameState(GameState gs)
        {
            int players = gs.positions.Length;
            positions = new int[players];
            points = new int[players];
            blockFrames = new int[players];
            dirs = new char[players];
            attacks = new int[players];
            gs.positions.CopyTo(positions, 0);
            gs.points.CopyTo(points, 0);
            gs.blockFrames.CopyTo(blockFrames, 0);
            gs.dirs.CopyTo(dirs, 0);
            gs.attacks.CopyTo(attacks, 0);
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < positions.Length; i++)
            {
                s += "p" + (i + 1) + ": (";
                s += "x = " + positions[i];
                s += ", points = " + points[i];
                s += ", bframes = " + blockFrames[i];
                s += ", dir = " + dirs[i];
                s += ", attack  = " + attacks[i];
                s += "), ";
            }
            s.Remove(s.Length - 2);
            return s;
        }

        public static GameState NextState(GameState state, string[] inputs, bool grid)
        {
            if (grid) speed = 50;
            var nextState = new GameState(state);
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i][0] == '1')    //right
                {
                    nextState.positions[i] += speed;
                    nextState.dirs[i] = 'r';
                }
                if (inputs[i][1] == '1')    //left
                {
                    nextState.positions[i] -= speed;
                    nextState.dirs[i] = 'l';
                }
                if (inputs[i][2] == '1')    //block
                {
                    if (state.blockFrames[i] == -blockCD)
                    {
                        nextState.blockFrames[i] = blockDur;
                    }
                }
                if (state.blockFrames[i] > -blockCD)
                {
                    nextState.blockFrames[i] -= 1;
                }
                if (inputs[i][3] == '1')    //attack
                {
                    nextState.attacks[i] = 1;
                    if (state.attacks[i] == 0)
                    {
                        if (nextState.dirs[i] == 'r')
                        {
                            for (int j = 0; j < inputs.Length; j++)
                            {
                                if (j != i && state.blockFrames[j] <= 0)
                                {
                                    if (state.positions[i] + 50 < state.positions[j] && state.positions[j] < state.positions[i] + 150)
                                    {
                                        nextState.points[i] += 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < inputs.Length; j++)
                            {
                                if (j != i && state.blockFrames[j] <= 0)
                                {
                                    if (state.positions[i] - 100 < state.positions[j] + 50 && state.positions[j] + 50 < state.positions[i])
                                    {
                                        nextState.points[i] += 1;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    nextState.attacks[i] = 0;
                }
            }
            return nextState;
        }

        public static GameState InitialState(int pCount)
        {
            if (pCount == 1)
                return new GameState(new int[] { 0 }, new int[] { 0 }, new int[] { -blockCD }, new char[] { 'r' }, new int[] { 0 });
            else
                return new GameState(new int[] { 0, 100 }, new int[] { 0, 0 }, new int[] { -blockCD, -blockCD }, new char[] { 'r', 'l' }, new int[] { 0, 0 })));
        }
    }

    public class Frame
    {
        public DateTime startTime;
        public string[] inputs;
        public GameState state;

        public Frame(DateTime startTime, string[] inputs, GameState state)
        {
            this.startTime = startTime;
            this.inputs = inputs;
            this.state = state;
        }


        public override string ToString()
        {
            string s = "";
            s += "time: " + startTime.ToString("mm.ss.fff") + ", ";
            s += "inputs: [";
            foreach (string input in inputs)
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

        public ServerPacket(DateTime timeStamp, Frame frame)
        {
            this.timeStamp = timeStamp;
            this.frame = frame;
        }

        public ServerPacket Deserialize(string packet)
        {
            System.Text. JsonElement[] recvData = JsonSerializer.Deserialize<JsonElement[]>(packet);
            DateTime recvTimeStamp = new DateTime(BinaryPrimitives.ReadInt64BigEndian(recvData[0].Deserialize<byte[]>()));
            string[] recvInputs = recvData[1].Deserialize<string[]>();
            int[] recvPos = recvData[2].Deserialize<int[]>();
            int[] recvPoints = recvData[3].Deserialize<int[]>();
            int[] recvBFrames = recvData[4].Deserialize<int[]>();
            char[] recvDirs = recvData[5].Deserialize<char[]>();
            int[] recvAttacks = recvData[6].Deserialize<int[]>();
        }
    }
}
