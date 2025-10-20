using System;

namespace ConnectN
{
    enum State { Empty, X, O }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connect N \n");
            Random rand = new Random();

            byte toWin = 0;
            //Create board
            byte rows = 0; //limit board size to 255x255
            byte cols = 0;

            Console.WriteLine("Enter the dimensions of the board (rows, columns) ");
            while (rows < 1 || cols < 1)
            {
                try
                {
                    string[] dimensionString = Console.ReadLine().Split(',');
                    if (dimensionString.Length != 2) { throw new FormatException(); }
                    rows = Convert.ToByte(dimensionString[0]);
                    cols = Convert.ToByte(dimensionString[1]);
                    if (rows == 0 || cols == 0) { throw new OverflowException(); }
                    if (rows > 20 || cols > 20)
                    {
                        rows = 0; cols = 0;
                        throw new OverflowException(); //Limit board size to 20x20 
                    }
                }
                catch (OverflowException) { Console.WriteLine("The numbers you entered are not in range (1-20) "); }
                catch (FormatException) { Console.WriteLine("Please enter two integers separated by a comma"); }
                catch (Exception) { Console.WriteLine("An unexpected error occurred. Please try again"); }
            }
            Board board = new Board(rows, cols);

            //Set number to connect
            Console.WriteLine("Enter the number in a row to win");
            while (toWin < 1)
            {
                try
                {
                    toWin = Convert.ToByte(Console.ReadLine());
                    if (toWin == 0) { throw new OverflowException(); }
                    if (toWin > board.numCols && toWin > board.numRows) //3x9 board can have 9 in a row 
                    { toWin = 0; throw new OverflowException(); }
                }
                catch (OverflowException) { Console.WriteLine($"The number you entered is not in range (1-{Math.Max(rows, cols)}) "); }
                catch (FormatException) { Console.WriteLine("Please enter an integer"); }
                catch (Exception) { Console.WriteLine("An unexpected error occurred. Please try again"); }
            }
            GameState.SetToWin(toWin);

            //Check classes and program values same
            if (GameState.toWin != toWin) { Console.WriteLine("An unexpected error occurred. The game will now exit"); return; }
            if (board.numRows != rows || board.numCols != cols)
            { Console.WriteLine("An unexpected error occurred. The game will now exit"); return; }

            //Announce stuff to user
            Console.WriteLine("");
            board.PrintBoard();
            Console.WriteLine("");
            Console.WriteLine($"The board is {board.numRows} rows tall and {board.numCols} columns wide");
            Console.WriteLine($"Get {GameState.toWin} to win \n"); //Use class values instead of local variables

            //Tournament loop
            string again = null;
            int[] wins = new int[3] { 0, 0, 0 }; //xWins, oWins, draws
            State starter = State.X;
            State nowPlayer;
            while (again == "yes" || again == null)
            {
                //Game loop -> 1 game only
                State winner = State.Empty;
                nowPlayer = starter;
                while (winner == State.Empty)
                {
                    Console.WriteLine($"Player {nowPlayer}'s turn");
                    //Get column and check if valid move
                    Console.WriteLine($"Enter the column (1-{board.numCols})");
                    byte column = 0;
                    while (column < 1)
                    {
                        try
                        {
                            column = Convert.ToByte(Console.ReadLine());
                            if (column == 0) { throw new OverflowException(); }
                            if (column > board.numCols) { column = 0; throw new OverflowException(); }
                            if (!board.ValidMove((byte)(column - 1))) { column = 0; throw new Exception("The column is already full"); }
                        }
                        catch (OverflowException) { Console.WriteLine($"The number you entered is not in range (1-{cols}) "); }
                        catch (FormatException) { Console.WriteLine("Please enter an integer"); }
                        catch (Exception e) { Console.WriteLine(e.Message); }
                    }
                    column -= 1; //arrays use 0-based indexing
                    board[board.FindRow(column), column] = nowPlayer; //Set square         
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
                //Play again
                again = null; //reset for loop
                Console.WriteLine("Play again? (yes/no) ");
                while (again == null)
                {
                    try
                    {
                        again = Console.ReadLine();
                        if (again == null) { throw new FormatException(); }
                        if (again != "yes" && again != "no") { again = null; throw new FormatException(); }
                    }
                    catch (FormatException) { Console.WriteLine("Please enter yes or no "); }
                    catch (Exception) { Console.WriteLine("An unexpected error occured. Please try again"); }
                }
                Console.WriteLine();
                if (again == "yes") //Reset board for next game
                {
                    board.EmptyBoard();
                    board.PrintBoard();
                    Console.WriteLine();
                    if (winner == State.Empty) { starter = (rand.Next(0, 2) == 0) ? State.X : State.O; } //random start
                    else { starter = (winner == State.X) ? State.O : State.X; } //loser starts
                }
            }
            //Add number of wins/draws/games played
            Console.WriteLine("Statistics: ");
            Console.WriteLine($"Wins by X: {wins[0]}");
            Console.WriteLine($"Wins by O: {wins[1]}");
            Console.WriteLine($"Draws: {wins[2]} ");
            Console.WriteLine($"Total games played: {wins[0] + wins[1] + wins[2]} \n");

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

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
