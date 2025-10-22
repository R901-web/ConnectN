using System;
using System.Diagnostics;
using System.IO;

namespace ConnectN
{
    internal class Logger
    {
        Stopwatch time = new Stopwatch();
        public Logger()
        {
            time.Start();
        }

        public void Log(string message) //Generic Log funcition
        {

        }

        public void LogMove(State player, Position pos) //Replace with Position
        {
            //Add player placed what at where + time
        }
    }
}
