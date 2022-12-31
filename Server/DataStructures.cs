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
            positions = gs.positions;
            points = gs.points;
            blockFrames = gs.blockFrames;
            dirs = gs.dirs;
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
    }
}
