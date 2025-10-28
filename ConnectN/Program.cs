using System;

namespace ConnectN
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connect N v1.2.0\n"); //MAKE SURE TO UPDATE
            Random rand = new Random();
            Logger logger = new Logger();

            //Create board
            Position size = new Position(0, 0);
            Console.WriteLine("Enter the dimensions of the board (rows, columns) ");
            logger.Log("Board Dimensions");
            while (size.row < 1 || size.col < 1)
            {
                try 
                {
                    string input = Console.ReadLine();
                    logger.LogInput(input);
                    string[] sizeString = input.Split(',');
                    if (sizeString.Length != 2) { throw new FormatException(); }
                    byte[] sizeByte = new byte[2] { Convert.ToByte(sizeString[0]), Convert.ToByte(sizeString[1]) };
                    size = new Position(sizeByte[0], sizeByte[1]);
                    if (size.row == 0 || size.col == 0) { throw new OverflowException(); }
                    if (size.row > 20 || size.col > 20) { size = new Position(0, 0); throw new OverflowException(); }
                }
                catch (OverflowException e) { Console.WriteLine("Please enter numbers in range (1-20) "); logger.LogError(e); }
                catch (FormatException e) { Console.WriteLine("Please enter two integers separated by a comma"); logger.LogError(e); }
                catch (Exception e) { Console.WriteLine("An unexpected error occurred. Please try again"); logger.LogError(e); }
            }
            Board board = new Board(size);

            //Set number to connect
            byte toWin = 0;
            Console.WriteLine("Enter the number in a row to win");
            logger.Log("Pieces to connect");
            while (toWin < 1)
            {
                try
                {
                    string input = Console.ReadLine();
                    logger.LogInput(input);
                    toWin = Convert.ToByte(input);
                    if (toWin == 0) { throw new OverflowException(); } //3x9 board can have 9 in a row 
                    if (toWin > board.numCols && toWin > board.numRows) { toWin = 0; throw new OverflowException(); }
                }
                catch (OverflowException e) 
                { Console.WriteLine($"Please enter a number in range (1-{Math.Max(board.numRows, board.numCols)}) "); logger.LogError(e); }
                catch (FormatException e) { Console.WriteLine("Please enter an integer"); logger.LogError(e); }
                catch (Exception e) { Console.WriteLine("An unexpected error occurred. Please try again"); logger.LogError(e); }
            }
            GameState.ToWin = toWin;

            //Announce stuff to user
            Console.WriteLine("");
            board.PrintBoard();
            Console.WriteLine("");
            Console.WriteLine($"The board is {board.numRows} rows tall and {board.numCols} columns wide");
            Console.WriteLine($"Get {GameState.ToWin} to win \n"); //Use class values instead of local variables
            logger.Log(new string[] { $"Board size = {board.numRows} x {board.numCols}", $"Pieces to connect = {GameState.ToWin}" });

            //Tournament loop
            string again = null;
            int[] wins = new int[3] { 0, 0, 0 }; //xWins, oWins, draws
            int numGames = 0;
            State starter = State.X;
            while (again == "yes" || again == null) 
            {
                //Game loop -> 1 game only
                numGames++;
                State winner = State.Empty;
                int numMoves = 0;
                State nowPlayer = starter;
                logger.LogGameStart(numGames, starter);
                while (winner == State.Empty)
                {
                    numMoves++;
                    Console.WriteLine($"Player {nowPlayer}'s turn");
                    //Get column and check if valid move
                    Console.WriteLine($"Enter the column (1-{board.numCols})");
                    byte column = 0;
                    while (column < 1)
                    {
                        try
                        {
                            string input = Console.ReadLine();
                            logger.LogInput(input);
                            column = Convert.ToByte(input);
                            if (column == 0) { throw new OverflowException(); }
                            if (column > board.numCols) { column = 0; throw new OverflowException(); }
                            if (!board.ValidMove((byte)(column - 1))) { column = 0; throw new FullColumnException(); }
                        }
                        catch (OverflowException e) { Console.WriteLine($"Please enter a number in range (1-{board.numCols}) "); logger.LogError(e); }
                        catch (FormatException e) { Console.WriteLine("Please enter an integer"); logger.LogError(e); }
                        catch (FullColumnException e) { Console.WriteLine("Please choose a column with empty cells"); logger.LogError(e); }
                        catch (Exception e) { Console.WriteLine("An unexpected error occured. Please try again"); logger.LogError(e); }
                    }
                    column -= 1; //arrays use 0-based indexing
                    logger.LogMove(nowPlayer, new Position(column, board)); //Before setting square to prevent FindRow() bugs
                    board[new Position(column, board)] = nowPlayer; //Set square
                    board.PrintBoard();
                    Console.WriteLine();
                    //Check if win
                    winner = GameState.CheckWin(board);
                    if (winner == State.Empty && GameState.AllFull(board)) { break; } //Draw
                    //Switch player
                    nowPlayer = (nowPlayer == State.X) ? State.O : State.X;
                }
                //Winner/Draw logic
                if (winner == State.Empty) { Console.WriteLine("The game ended in a draw"); wins[2]++; }
                else
                {
                    if (winner == State.X) { Console.ForegroundColor = ConsoleColor.Red; wins[0]++; }
                    else if (winner == State.O) { Console.ForegroundColor = ConsoleColor.Blue; wins[1]++; }
                    Console.WriteLine($"Player {winner} wins the game!");
                    Console.ResetColor();
                }
                logger.LogGameEnd(winner, numMoves);
                //Play again
                again = null; //reset for loop
                Console.WriteLine("Play again? (yes/no) ");
                while (again == null)
                {
                    try
                    {
                        again = Console.ReadLine();
                        logger.LogInput(again);
                        if (again == null) { throw new FormatException(); }
                        if (again != "yes" && again != "no") { again = null; throw new FormatException(); }
                    }
                    catch (FormatException e) { Console.WriteLine("Please enter yes or no "); logger.LogError(e); }
                    catch (Exception e) { Console.WriteLine("An unexpected error occured. Please try again"); logger.LogError(e); }
                }
                Console.WriteLine();
                if (again == "yes") //Reset board for next game
                {
                    board.EmptyBoard(); board.PrintBoard(); Console.WriteLine();
                    if (winner == State.Empty) { starter = (rand.Next(0, 2) == 0) ? State.X : State.O; } //random start
                    else { starter = (winner == State.X) ? State.O : State.X; } //loser starts
                }
            }

            //Announce statistics
            Console.WriteLine("Statistics: ");
            Console.WriteLine($"Wins by X: {wins[0]}");
            Console.WriteLine($"Wins by O: {wins[1]}");
            Console.WriteLine($"Draws: {wins[2]} ");
            Console.WriteLine($"Total games played: {numGames} \n");
            if (wins[0] == wins[1]) { Console.WriteLine("The tournament ended in a draw"); }
            else if (wins[0] > wins[1])
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Player X wins the tournament!");
            }
            else if (wins[1] > wins[0])
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Player O wins the tournament!");
            }
            Console.ResetColor();

            logger.LogEnd(wins);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
