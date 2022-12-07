using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameState = System.Collections.Generic.Dictionary<string, int>;

namespace RealTimeProject
{
    struct HistoryData 
    {
        public DateTime time;
        public string command;
        public GameState state;

        public HistoryData(DateTime time, string action, GameState state)
        {
            this.time = time;
            this.command = action;
            this.state = state;
        }
    }
    internal class ServerUtils
    {
    }
}
