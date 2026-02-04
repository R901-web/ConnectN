using System;

namespace ConnectN
{
    public class Board
    {
        //Limit board size to 127x127
        public sbyte numRows { get; } //how tall
        public sbyte numCols { get; } //how fat
        private State[,] board { get; } //the actual board
        public bool compact { get; set; } //the print style

        public State this[Position p]
        {
            get
            {
                if (p.row >= numRows || p.col >= numCols || p.row < 0 || p.col < 0) { return State.Empty; }
                else { return board[p.row, p.col]; }
            }
            set { if (p.row < numRows && p.col < numCols && p.row >= 0 && p.col >= 0) { board[p.row, p.col] = value; } }
        }

        public Board(Position p)
        {
            numRows = p.row;
            numCols = p.col;
            board = new State[numRows, numCols];
            if (numRows > 15 || numCols > 20) { compact = true; } //Large board -> compact print
            else { compact = false; }
        }

        //Clones the board -> create new instance to test moves on
        public Board(Board board)
        {
            numRows = board.numRows;
            numCols = board.numCols;
            this.board = new State[numRows, numCols];
            compact = board.compact;
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols; c++)
                { this.board[r, c] = board[new Position(r, c)]; }
            }
        }

        public void EmptyBoard()
        {
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols; c++)
                { board[r, c] = State.Empty; }
            }
        }

        //Group empty cells together and print together
        //Print X or O individually with colour
        //Print last move in different colour for clarity
        //When game won, highlight winning positions
        //Normal mode: | dividers and ---+---+--- dividers
        //Compact mode: no dividers, just spaces -> perfomance benefit too
        public void printBoard(Position move, Position[] winPos) //Combine normal board and compact board
        {
            string divider = "";
            if (!compact) //Prebuild divider for normal board
            {
                for (int col = 0; col < numCols; col++)
                {
                    divider += "---";
                    if (col < numCols - 1) { divider += "+"; }
                }
            }
            //Segment builder
            string BuildSegment(sbyte length, bool isLast)
            {
                string segString = "";
                for (int i = 0; i < length; i++)
                {
                    if (compact) { segString += "\u00B7 "; }
                    else
                    {
                        segString += "   ";
                        if (i < length - 1) { segString += "|"; }
                    }
                }
                if (!isLast && length > 0 && !compact) { segString += "|"; }
                return segString;
            }

            ConsoleColor defaultColor = Console.ForegroundColor;
            for (int row = 0; row < numRows; row++) //indexer uses sbyte
            {
                sbyte segLength = 0; //number that are same color in a row
                string segString;
                for (int col = 0; col < numCols; col++)
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
                    else
                    {
                        //build and print segment
                        segString = BuildSegment(segLength, false);
                        Console.Write(segString);
                        segLength = 0;
                        //Switch colour
                        if (GameState.CheckWin(this).winner == State.Empty) //game not won -> normal colour choosing
                        {
                            if (move.row == row && move.col == col)
                            //{ Console.ForegroundColor = (s == State.X) ? ConsoleColor.DarkMagenta : ConsoleColor.Cyan; } //Highlight last move
                            { Console.ForegroundColor = ConsoleColor.Green; }
                            else { Console.ForegroundColor = (s == State.X) ? ConsoleColor.Red : ConsoleColor.Blue; } //Normal cell
                        }
                        else //game won -> highlight winning positions
                        {
                            //Check if in winPos
                            bool isWinPos = false;
                            for (int i = 0; i < winPos.Length; i++)
                            {
                                if (winPos[i].row == row && winPos[i].col == col)
                                { isWinPos = true; break; }
                            }
                            if (isWinPos) { Console.ForegroundColor = ConsoleColor.Green; } //Highlight winning position
                            else { Console.ForegroundColor = (s == State.X) ? ConsoleColor.DarkRed : ConsoleColor.DarkBlue; } //Non-winning position
                        }

                        //Print cell
                        Console.Write(compact ? $"{s} " : $" {s} ");
                        Console.ForegroundColor = defaultColor;
                        if (!compact && !isLast) { Console.Write("|"); }
                    }
                }
                Console.WriteLine(); //end row
                if (!compact && row < numRows - 1) { Console.WriteLine(divider); } //new row for the cells
            }
            Console.ForegroundColor = defaultColor;
        }

        public bool ValidMove(sbyte column)
        {
            if (column >= numCols) { return false; } //Out of bounds
            for (int r = numRows - 1; r >= 0; r--) // Count from bottom to top, include row 0
            {
                if (board[r, column] == State.Empty)
                { return true; } // Found empty space
            }
            return false; // No empty space found
        }

        public sbyte FindRow(sbyte column) //Find the lowest empty row in a column
        {
            if (column >= numCols) { return 127; } //Out of bounds
            for (int r = numRows - 1; r >= 0; r--) // Count from bottom to top, include row 0
            {
                if (board[r, column] == State.Empty)
                { return (sbyte)r; } // Found empty space 
            }
            return 127; // No empty space found
        }
    }
}
