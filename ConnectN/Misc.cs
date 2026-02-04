using System;

namespace ConnectN
{
    //Miscellaneous enums, delegates and simple classes/structs
    public enum State { Empty, X, O }

    public enum AILevel { off, rand, complete, extend, central, minimax }

    public delegate sbyte Move(Board board);

    public struct Position
    {
        public sbyte col { get; } //the column its in = c -> x value
        public sbyte row { get; } //the row its in = r -> y value
        public static Position Sentinel { get; } = new Position(127, 127);

        public static Position up { get; } = new Position(-1, 0);
        public static Position down { get; } = new Position(1, 0);
        public static Position left { get; } = new Position(0, -1);
        public static Position right { get; } = new Position(0, 1);

        public static Position operator +(Position a, Position b)
        { return new Position(a.row + b.row, a.col + b.col); }
        public static Position operator *(int b, Position a)
        { return new Position(a.row * b, a.col * b); }

        public Position(int row, int col)
        {
            this.row = (sbyte)row;
            this.col = (sbyte)col;
        }

        public Position(sbyte col, Board board)
        {
            this.col = (sbyte)col;
            this.row = (sbyte)board.FindRow(col);
        }

        //0-based, higher col = more right (0 to numCols-1); higher row = more down (0 at top, numRows-1 at bottom) 
        public override string ToString() { return $"[{col}, {row}]"; }
    }

    public class FullColumnException : Exception
    {
        public FullColumnException() : base("This column is full") { } //normal exception but message is preset
    }
}
