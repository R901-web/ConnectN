namespace ConnectN
{
    public static class GameState
    {
        private static sbyte toWin = 0;

        public static sbyte ToWin
        {
            get { return toWin; }
            set { if (toWin == 0) { toWin = value; } } //Set only if not already set
        }
        //higher col, go down; higher row, go right
        public static (State winner, Position[] positions) CheckWin(Board board) //Can leave out numToCheck
        {
            State[] values = new State[toWin];
            Position[] positions = new Position[toWin];
            //Check horizontal
            for (int r = 0; r < board.numRows; r++)
            {
                for (int c = 0; c < board.numCols; c++) //Can out of bounds because indexer returns State.Empty
                {
                    Position currentCell = new Position(r, c);
                    for (int i = 0; i < toWin; i++)
                    { positions[i] = currentCell + i * Position.right; values[i] = board[positions[i]]; }
                    if (AllEqual(values) && NoEmpty(values)) { return (values[0], positions); }
                }
            }
            //Check vertical
            for (int r = 0; r < board.numRows; r++)
            {
                for (int c = 0; c < board.numCols; c++)
                {
                    Position currentCell = new Position(r, c);
                    for (int i = 0; i < toWin; i++)
                    { positions[i] = currentCell + i * Position.down; values[i] = board[positions[i]]; }
                    if (AllEqual(values) && NoEmpty(values)) { return (values[0], positions); }
                }
            }
            if (toWin <= board.numRows && toWin <= board.numCols) //for optimization
            {
                //Check \ diagonal
                for (int r = 0; r < board.numRows; r++)
                {
                    for (int c = 0; c < board.numCols; c++)
                    {
                        Position currentCell = new Position(r, c);
                        for (int i = 0; i < toWin; i++)
                        { positions[i] = currentCell + i * (Position.right + Position.down); values[i] = board[positions[i]]; }
                        if (AllEqual(values) && NoEmpty(values)) { return (values[0], positions); }
                    }
                }
                //Check / diagonal
                for (int r = 0; r < board.numRows; r++)
                {
                    for (int c = 0; c < board.numCols; c++)
                    {
                        Position currentCell = new Position(r, c);
                        for (int i = 0; i < toWin; i++)
                        { positions[i] = currentCell + i * (Position.right + Position.up); values[i] = board[positions[i]]; }
                        if (AllEqual(values) && NoEmpty(values)) { return (values[0], positions); }
                    }
                }
            }
            return (State.Empty, new Position[0]); //If no winner
        }



        public static bool AllFull(Board board)
        {
            for (int r = 0; r < board.numRows; r++)
            {
                for (int c = 0; c < board.numCols; c++)
                { if (board[new Position(r, c)] == State.Empty) { return false; } }
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
