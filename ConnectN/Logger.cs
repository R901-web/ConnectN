using System;
using System.Diagnostics;
using System.IO;

namespace ConnectN
{
    internal class Logger
    {
        Stopwatch clock = new Stopwatch();
        Log append;
        Log write;
        Log emptyLine;

        private string GetTime()
        {
            TimeSpan time = clock.Elapsed;
            double milliseconds = time.TotalMilliseconds; //get time
            time = TimeSpan.FromMilliseconds(Math.Round(milliseconds)); //round to nearest ms
            string timeString = $"[LOG {time.ToString(@"hh\:mm\:ss\.fff")}] ";
            return timeString;
        }

        //6 chars before ' : '
        public Logger()
        {
            append = delegate (string s) { File.AppendAllText("log.txt", $"\n{ GetTime() + s }"); };
            write = delegate (string s) { File.WriteAllText("log.txt", s); };
            emptyLine = delegate { File.AppendAllText("log.txt", "\n"); };
            //Add fancy header thing
            write("==========[ ConnectN v1.2.0 ]========== ");
            clock.Start();
            emptyLine("");
            append("Logger : Log initiated");
        }

        //For announcements/new sections
        public void Log(string s) { emptyLine(""); append($"Logger : {s} "); }
        public void Log(string[] s) 
        { 
            emptyLine(""); 
            foreach (string line in s) { append($"Logger : {line} "); }
        }

        public void LogInput(string s) { append($"Input  : User inputted '{s}' "); } 

        public void LogError(Exception e) //exception is base class so can all
        {
            if (e is OverflowException) { append($"Error  : Input not in range"); }
            else if (e is FormatException) { append($"Error  : Input in wrong format"); }
            else if (e is FullColumnException) { append($"Error  : Input column is full"); }
            else { append($"Error  : An unexpected error occured"); }
        }

        public void LogMove(State player, Position pos)
        { append($"Move   : {player} placed counter at [{pos.col}, {pos.row}]"); }

        public void LogGameStart(int numGames, State starter)
        { 
            emptyLine(""); 
            append($"----------| Game {numGames} |----------"); 
            append($"Game   : Player {starter} starts"); 
        }

        public void LogGameEnd(State winner, int numMoves)
        {
            if (winner == State.Empty)
            { append($"Game   : Draw after {numMoves} moves"); }
            else
            { append($"Game   : {winner} wins after {numMoves} moves"); }
        }

        public void LogEnd(int[] wins)
        {
            //Add statistics
            emptyLine("");
            append("Logger : Statistics ");
            append($"Logger : Wins by X = {wins[0]}");
            append($"Logger : Wins by O = {wins[1]}");
            append($"Logger : Draws = {wins[2]}");
            emptyLine("");
            append("Logger : Log ended ");
            clock.Stop();
        }
    }
}
