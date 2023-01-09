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
