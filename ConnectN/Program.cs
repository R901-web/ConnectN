using System;

namespace ConnectN
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connect N v1.2.1\n"); 
            Random rand = new Random();
            Logger logger = new Logger();

            //Create board
            Position? size = null;
            Console.Write("Enter the dimensions of the board (rows, columns): ");
            logger.Log("Board Dimensions");
            while (size == null)
            {
                try 
                {
                    string input = Console.ReadLine();
                    logger.LogInput(input);
                    string[] sizeString = input.Split(',');
                    if (sizeString.Length != 2) { throw new FormatException(); }
                    sbyte[] sizesbyte = new sbyte[2] { Convert.ToSByte(sizeString[0]), Convert.ToSByte(sizeString[1]) };
                    size = new Position(sizesbyte[0], sizesbyte[1]);
                    if (size?.row < 2 || size?.col < 2) { throw new OverflowException(); } //already assinged -> cannot be null
                    if (size?.row > 30 || size?.col > 30) { throw new OverflowException(); }
                }
                catch (OverflowException e) { Console.Write("Enter numbers in range (2-30): "); logger.LogError(e); size = null; }
                catch (FormatException e) { Console.Write("Enter two integers separated by a comma: "); logger.LogError(e); }
                catch (Exception e) { Console.Write("An unexpected error occurred. Please try again: "); logger.LogError(e); }
            }
            Console.WriteLine();
            Board board = new Board(size ?? new Position(6,7)); //defaults to Connect 4 board if somehow get past error handling

            //Set number to connect
            sbyte? toWin = null;
            Console.Write("Enter the number in a row to win: ");
            logger.Log("Pieces to connect");
            while (toWin == null)
            {
                try
                {
                    string input = Console.ReadLine();
                    logger.LogInput(input);
                    toWin = Convert.ToSByte(input);
                    if (toWin < 2) { throw new OverflowException(); } //3x9 board can have 9 in a row 
                    if (toWin > board.numCols && toWin > board.numRows) { throw new OverflowException(); }
                }
                catch (OverflowException e) 
                { Console.Write($"Enter a number in range (2-{Math.Max(board.numRows, board.numCols)}): "); logger.LogError(e); toWin = null; }
                catch (FormatException e) { Console.Write("Enter an integer: "); logger.LogError(e); }
                catch (Exception e) { Console.Write("An unexpected error occurred. Please try again: "); logger.LogError(e); }
            }
            Console.WriteLine();
            GameState.ToWin = toWin ?? Math.Min(board.numRows, board.numCols);

            //Decide AI level
            AI aiX = null; AI aiO = null;
            for (int i = 0; i < 2; i++)
            {
                AILevel? level = null;
                State player = (i == 0) ? State.X : State.O;
                Console.Write($"Enter the AI level for {player} (0 for off, otherwise 1-5): ");
                logger.Log($"AI Level for {player}");
                while (level == null)
                {
                    try
                    {
                        string input = Console.ReadLine();
                        logger.LogInput(input);
                        sbyte levelNum = Convert.ToSByte(input);
                        if (levelNum > 5 || levelNum < 0) { throw new OverflowException(); }
                        level = (AILevel)levelNum;
                    }
                    catch (OverflowException e) { Console.Write("Enter a number in range (0-5): "); logger.LogError(e); }
                    catch (FormatException e) { Console.Write("Enter an integer: "); logger.LogError(e); }
                    catch (Exception e) { Console.Write("An unexpected error occurred. Please try again: "); logger.LogError(e); }
                }
                Console.WriteLine();
                if (player == State.X) { aiX = new AI(level ?? AILevel.off, board, player); } //defaults to off -> 2 player
                else { aiO = new AI(level ?? AILevel.off, board, player); }
            }

            //Announce stuff to user
            Console.WriteLine("");
            board.printBoard(Position.Sentinel, new Position[0]);
            Console.WriteLine("");
            Console.WriteLine($"The board is {board.numRows} rows tall and {board.numCols} columns wide");
            Console.WriteLine($"Get {GameState.ToWin} to win \n"); //Use class values instead of local variables
            logger.Log(new string[]
            {
                $"Board size = {board.numRows} x {board.numCols}",
                $"Pieces to connect = {GameState.ToWin}",
                $"Print style = {(board.compact? "Compact" : "Normal")}",
                $"AI Level for X = {aiX.level}",
                $"AI level for O = {aiO.level}"
            });

            //Tournament loop
            bool? again = null;
            int[] wins = new int[3] { 0, 0, 0 }; //xWins, oWins, draws
            int numGames = 0;
            State starter = State.X;

            Move humanMove = delegate
            {
                Console.Write($"Enter the column (1-{board.numCols}): ");
                sbyte column = 0;
                while (column < 1)
                {
                    try
                    {
                        string input = Console.ReadLine();
                        logger.LogInput(input);
                        column = Convert.ToSByte(input);
                        if (column == 0) { throw new OverflowException(); }
                        if (column > board.numCols) { column = 0; throw new OverflowException(); }
                        if (!board.ValidMove((sbyte)(column - 1))) { column = 0; throw new FullColumnException(); }
                    }
                    catch (OverflowException e) { Console.Write($"Enter a number in range (1-{board.numCols}): "); logger.LogError(e); }
                    catch (FormatException e) { Console.Write("Enter an integer: "); logger.LogError(e); }
                    catch (FullColumnException e) { Console.Write("Choose a column with empty cells: "); logger.LogError(e); }
                    catch (Exception e) { Console.Write("An unexpected error occured. Please try again: "); logger.LogError(e); }
                }
                return (sbyte)(column - 1); //convert to 0-based
            };

            Move xMove = (aiX.level == AILevel.off) ? humanMove : aiX.getMove();
            Move oMove = (aiO.level == AILevel.off) ? humanMove : aiO.getMove();

            while (again == true || again == null) 
            {
                //Game loop -> 1 game only
                numGames++;
                State winner = State.Empty;
                Position[] winPos = new Position[0];
                int numMoves = 0;
                State nowPlayer = starter;
                logger.LogGameStart(numGames, starter);
                while (winner == State.Empty)
                {
                    numMoves++;
                    bool isAI = (nowPlayer == State.X) ? (aiX.level != AILevel.off) : (aiO.level != AILevel.off);

                    //Get move and place counter
                    Console.WriteLine($"Player {nowPlayer}'s turn");
                    Move chooseMove = (nowPlayer == State.X) ? xMove : oMove;
                    if (isAI) { Console.Write($"Enter the column (1-{board.numCols}): "); }
                    sbyte c = chooseMove(board);
                    if (isAI) { Console.WriteLine(c + 1); }
                    Console.WriteLine();
                    Position move = new Position(c, board); 
                    logger.LogMove(nowPlayer, move);
                    board[move] = nowPlayer; //Set square 

                    //Find winner/winning positions
                    (winner, winPos) = GameState.CheckWin(board);
                    board.printBoard(move, winPos); //print board
                    Console.WriteLine();
                    //Switch player
                    nowPlayer = (nowPlayer == State.X) ? State.O : State.X;
                    //Exit if win/draw -> win handled by while loop
                    if (winner == State.Empty && GameState.AllFull(board)) { break; } //Draw
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
                logger.LogGameEnd(winner, numMoves, winPos);
                //Play again
                again = null; //reset for loop
                Console.Write("Play again? (yes/no): ");
                while (again == null)
                {
                    try
                    {
                        string input = Console.ReadLine();
                        logger.LogInput(input);
                        input = input.ToUpper();
                        if (input != "YES" && input != "NO" && input != "Y" && input != "N") { throw new FormatException(); }
                        else
                        {
                            if (input == "YES" || input == "Y") { again = true; }
                            else if (input == "NO" || input == "N") { again = false; }
                        }
                    }
                    catch (FormatException e) { Console.Write("Enter yes or no: "); logger.LogError(e); }
                    catch (Exception e) { Console.Write("An unexpected error occured. Please try again: "); logger.LogError(e); }
                }
                Console.WriteLine();
                if (again == true) //Reset board for next game
                {
                    board.EmptyBoard(); board.printBoard(Position.Sentinel, new Position[0]); Console.WriteLine(); //print empty board
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

            logger.LogEnd(wins, numGames);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
