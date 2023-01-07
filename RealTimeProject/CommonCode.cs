using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeProject
{
    internal class CommonCode
    {
        public static GameState NextState(GameState state, string[] inputs, bool grid)
        {
            int speed = 5, blockDur = 20, blockCooldown = 300;
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
                    if (state.blockFrames[i] == -blockCooldown)
                    {
                        nextState.blockFrames[i] = blockDur;
                    }
                }
                if (state.blockFrames[i] > -blockCooldown)
                {
                    nextState.blockFrames[i] -= 1;
                }
                if (inputs[i][3] == '1')    //attack
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
                                if (state.positions[i] - 100 < state.positions[j] && state.positions[j] < state.positions[i])
                                {
                                    nextState.points[i] += 1;
                                }
                            }
                        }
                    }
                }
            }
            return nextState;
        }
    }

    public class GameState {
        public int[] positions;
        public int[] points;
        public int[] blockFrames;
        public char[] dirs;
        
        public GameState(int[] positions, int[] points, int[] blockFrames, char[] dirs)
        {
            this.positions = positions;
            this.points = points;
            this.blockFrames = blockFrames;
            this.dirs = dirs;
        }

        public GameState(GameState gs)
        {
            int players = gs.positions.Length;
            positions = new int[players];
            points = new int[players];
            blockFrames = new int[players];
            dirs = new char[players];
            gs.positions.CopyTo(positions, 0);
            gs.points.CopyTo(points, 0);
            gs.blockFrames.CopyTo(blockFrames, 0);
            gs.dirs.CopyTo(dirs, 0);
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
                s += "), ";
            }
            s.Remove(s.Length - 2);
            return s;
        }
    }

    public class Frame {
        public string[] inputs;
        public GameState state;
        
        public Frame(string[] inputs, GameState state)
        {
            this.inputs = inputs;
            this.state = state;
        }


        public override string ToString()
        {
            string s = "";
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
