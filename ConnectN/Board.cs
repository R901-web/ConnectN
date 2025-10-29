using System;

namespace ConnectN
{
    internal class Board
    {
        //Limit board size to 255x255
        public byte numRows { get; } //how tall
        public byte numCols { get; } //how fat
        State[,] board { get; } //the actual board

        public State this[Position p]
        {
            get 
            {
                if (p.row >= numRows || p.col >= numCols) { return State.Empty; } //Is byte -> no need check if < 0
                else { return board[p.row, p.col]; }
            }
            set { if (p.row < numRows && p.col < numCols) { board[p.row, p.col] = value; } }
        }

        public Board(Position p)
        {
            numRows = p.row;
            numCols = p.col;
            board = new State[numRows, numCols];
            EmptyBoard();
        }

        public void EmptyBoard()
        {
            for (byte r = 0; r < numRows; r++)
            {
                for (byte c = 0; c < numCols; c++)
                { board[r, c] = State.Empty; }
            }
        }

        //Group empty cells into segments, print segments all together
        //For X and O, print cells individually so divider is white
        //Last piece placed is printed in a lighter colour
        public void PrintBoard(Position move) //If move not in board, no change
        {
            //Prebuild ---+---+--- divider
            string divider = "";
            for (int col = 0; col < numCols; col++)
            {
                divider += "---";
                if (col < numCols - 1) { divider += "+"; }
            }
            //Create cell builder local function
            string BuildSegment(byte length, bool isLast)
            {
                string segString = "";
                for (int i = 0; i < length; i++)
                {
                    segString += "   ";
                    if (i < length - 1) { segString += "|"; }
                }
                if (!isLast && length > 0) { segString += "|"; } //length > 0 to avoid || when 2 consec. coloured
                return segString;
            }
            ConsoleColor defaultColor = Console.ForegroundColor;
            //Each row
            for (byte row = 0; row < numRows; row++) //indexer uses byte
            {
                byte segLength = 0; //number in a row
                string segString;
                for (byte col = 0; col < numCols; col++)
                {
                    State s = board[row, col];
                    bool isLast = (col == numCols - 1);
                    if (s == State.Empty)
                    {
                        segLength++;
                        if (isLast) //Build segment and print
                        {
                            segString = BuildSegment(segLength, true);
                            Console.Write(segString);
                            segLength = 0;
                        }
                    }
                    //Not empty cell
                    else
                    {
                        //build and print segment
                        segString = BuildSegment(segLength, false);
                        Console.Write(segString);
                        segLength = 0;
                        //Switch colour
                        if (move.row == row && move.col == col) //Last move
                        { Console.ForegroundColor = (s == State.X) ? ConsoleColor.Red : ConsoleColor.Blue ; } //Lighter colour
                        else { Console.ForegroundColor = (s == State.X) ? ConsoleColor.DarkRed : ConsoleColor.DarkBlue; } //Normal cell
                        //Print cell
                        Console.Write($" {s} ");
                        Console.ForegroundColor = defaultColor;
                        if (!isLast) { Console.Write("|"); }
                    }
                }
                Console.WriteLine(); //end row of  X |   | O |   | X | O 
                if (row < numRows - 1) { Console.WriteLine(divider); } //new row for the cells
            }
            Console.ForegroundColor = defaultColor;
        }

        public bool ValidMove(byte column)
        {
            if (column >= numCols) { return false; } //Out of bounds
            for (int r = numRows - 1; r >= 0; r--) // Count from bottom to top, include row 0
            {
                if (board[r, column] == State.Empty)
                { return true; } // Found empty space
            }
            return false; // No empty space found
        }

        public byte FindRow(byte column) //Find the lowest empty row in a column
        {
            if (column >= numCols) { return 255; } //Out of bounds
            for (int r = numRows - 1; r >= 0; r--) // Count from bottom to top, include row 0
            {
                if (board[r, column] == State.Empty)
                { return (byte)r; } // Found empty space 
            }
            return 255; // No empty space found
        }
    }
}
