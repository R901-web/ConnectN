using System;

namespace ConnectN
{
    //Miscellaneous enums, delegates and simple classes/structs
    enum State { Empty, X, O }

    public delegate void Log(string s);

    internal struct Position 
    {
        public byte col { get; } //the column its in = c
        public byte row { get; } //the row its in = r

        public Position(int row, int col)
        {
            this.row = (byte)row;
            this.col = (byte)col;
        }

        public Position(byte col, Board board)
        {
            this.col = col;
            this.row = board.FindRow(col);
        }
    }
    internal class FullColumnException : Exception
    {
        public FullColumnException() : base("This column is full") { } //normal exception but message is preset
    }
}
