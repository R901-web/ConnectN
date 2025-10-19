using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace ConnectN_v2
{
    enum State { Empty, X, O }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connect N \n");
            State winner = State.Empty;
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

            //Game loop
            byte column = 0;
            State nowPlayer = State.X;
            while (winner == State.Empty)
            {
                Console.WriteLine($"Player {nowPlayer}'s turn");
                //Get column and check if valid move
                Console.WriteLine($"Enter the column (1-{board.numCols})");
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
                //Set to 0 for while loop for next turn
                column = 0;
                board.PrintBoard();
                Console.WriteLine("");
                //Check if win
                winner = GameState.CheckWin(board);
                if (winner == State.Empty && GameState.AllFull(board)) { break; } //Draw

                //Switch player
                if (nowPlayer == State.X) { nowPlayer = State.O; }
                else if (nowPlayer == State.O) { nowPlayer = State.X; }
            }
            //Winner/Draw logic
            if (winner == State.Empty) { Console.WriteLine("The game ended in a draw"); }
            else
            {
                if (winner == State.X) { Console.ForegroundColor = ConsoleColor.Red; }
                else if (winner == State.O) { Console.ForegroundColor = ConsoleColor.Blue; }
                Console.WriteLine($"Player {winner} wins!");
                Console.ResetColor();
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
