using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeProject
{
    internal class DataStructures
    {
        
    }
    //server
    public class GameState
    {
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
            int speed = 5, blockCD = 5, blockDur = 40;
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
}
