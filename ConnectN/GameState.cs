namespace ConnectN_v2
{
    internal static class GameState
    {
        public static byte toWin { get; private set; } = 0; //amount in row to win, is readonly

        public static void SetToWin(byte n)
        {
            if (toWin == 0) { toWin = n; } //Set only if not already set, also cannot set back to 0
        }

        public static State CheckWin(Board board)
        {
            State[] values = new State[toWin];

            //Check horizontal
            for (byte r = 0; r < board.numRows; r++)
            {
                for (byte c = 0; c < board.numCols; c++) //Can out of bounds because indexer returns State.Empty
                {
                    for (byte i = 0; i < toWin; i++)
                    { values[i] = board[r, (byte)(c + i)]; } //Explicit cast bcs implicit when add
                    if (AllEqual(values) && NoEmpty(values)) { return values[0]; }
                }
            }
            //Check vertical
            for (byte r = 0; r < board.numRows; r++)
            {
                for (byte c = 0; c < board.numCols; c++)
                {
                    for (byte i = 0; i < toWin; i++)
                    { values[i] = board[(byte)(r + i), c]; }
                    if (AllEqual(values) && NoEmpty(values)) { return values[0]; }
                }
            }
            if (toWin <= board.numRows && toWin <= board.numCols) //for optimization
            {
                //Check \ diagonal
                for (byte r = 0; r < board.numRows; r++)
                {
                    for (byte c = 0; c < board.numCols; c++)
                    {
                        for (byte i = 0; i < toWin; i++)
                        { values[i] = board[(byte)(r + i), (byte)(c + i)]; }
                        if (AllEqual(values) && NoEmpty(values)) { return values[0]; }
                    }
                }
                //Check / diagonal
                for (byte r = 0; r < board.numRows; r++)
                {
                    for (byte c = 0; c < board.numCols; c++)
                    {
                        for (byte i = 0; i < toWin; i++)
                        { values[i] = board[(byte)(r - i), (byte)(c + i)]; }
                        if (AllEqual(values) && NoEmpty(values)) { return values[0]; }
                    }
                }
            }

            return State.Empty; //If no winner
        }

        public static bool AllFull(Board board)
        {
            for (byte r = 0; r < board.numRows; r++)
            {
                for (byte c = 0; c < board.numCols; c++)
                { if (board[r, c] == State.Empty) { return false; } }
            }
            return true; // No empty cells found, board is full
        }

        private static bool AllEqual(params State[] states)
        {
            State first = states[0];
            foreach (State s in states)
            { if (s != first) { return false; } }
            return true;
        }

        private static bool NoEmpty(params State[] states)
        {
            foreach (State s in states)
            { if (s == State.Empty) { return false; } }
            return true;
        }
    }
}
