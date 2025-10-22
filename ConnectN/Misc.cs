using System;

namespace ConnectN
{
    //Miscellaneous enums, delegates and simple classes/structs
    enum State { Empty, X, O }

    internal struct Position
    {
        public byte x { get; } //the column its in
        public byte y { get; } //the row its in

        public Position(byte x, byte y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
