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

    public struct GameState {
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
                s += "points = " + points[i];
                s += "bframes = " + blockFrames[i];
                s += "dir = " + dirs[i];
                s += "), ";
            }
            s.Remove(s.Length - 2);
            return s;
        }
    }

    public struct Frame {
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
            s.Remove(s.Length - 2);
            s += "] ";
            s += "state: " + state.ToString();
            return s;
        }
    }
}
