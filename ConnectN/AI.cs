using System;
using System.Collections.Generic;

namespace ConnectN
{
    public class AI
    {
        Random rand = new Random();
        Position boardSize;
        State player;
        sbyte toWin;


        public AILevel level { get; }
        //rand: random moves, only makes sure columns are empty and places inside board
        //complete: Blocks opponent's n-1 in a row, completes own n-1 in a row, otherwise random, check past gaps
        //extend: blocks/extends shorter lines + forks
        //central: Preference for central moves, does 2 depth for complete
        //minimax: Minimax 2-depth search

        public AI(AILevel level, Board board, State player)
        {
            this.level = level;
            toWin = GameState.ToWin;
            boardSize = new Position(board.numRows, board.numCols);
            this.player = player;
        }

        public Move getMove()
        {
            if (level == AILevel.rand) { return randMove; }
            else if (level == AILevel.complete) { return completeMove; }
            else if (level == AILevel.extend) { return extendMove; }
            else if (level == AILevel.central) { return centralMove; }
            else if (level == AILevel.minimax) { return minimaxMove; }
            else { return delegate { return 0; }; } //anonymous methods :)
        }

        //Initialize valid positions and scoring system
        private (List<Position> positions, Dictionary<sbyte, float> scores) InitScoring(Board board)
        {
            //Get valid columns
            List<sbyte> validCols = new List<sbyte>();
            for (sbyte c = 0; c < board.numCols; c++) { if (board.ValidMove(c)) { validCols.Add(c); } }
            //Get valid positions
            List<Position> validPos = new List<Position>();
            foreach (sbyte col in validCols) { validPos.Add(new Position(col, board)); }
            //Initialize scores at 0
            Dictionary<sbyte, float> scores = new Dictionary<sbyte, float>();
            foreach (sbyte col in validCols) { scores.Add(col, 0f); }
            return (validPos, scores);
        }

        //Given a dictionary from columns to scores, return column with best score (random if tie)
        private sbyte findBest(Dictionary<sbyte, float> scores)
        {
            //Get best score
            float bestScore = float.NegativeInfinity;
            foreach (float score in scores.Values) { if (score > bestScore) { bestScore = score; } }
            //Get all columns with best score
            List<sbyte> bestCols = new List<sbyte>();
            foreach (KeyValuePair<sbyte, float> kv in scores) { if (kv.Value == bestScore) { bestCols.Add(kv.Key); } }
            return bestCols[rand.Next(0, bestCols.Count)]; //returns the key so no array mismatch
        }

        //Scoring priorities: 
        //1. Winning move (infinity)
        //2. Blocking move (10^6)
        //3. Fork move (5*10^4)

        private float ScoreWinning(Board board, Position pos)
        {
            Board localBoard = new Board(board); //use constructor because Board is reference type
            localBoard[pos] = player; //make move
            State winner = GameState.CheckWin(localBoard).winner;
            if (winner == player) { return float.PositiveInfinity; } //positive infinity to make sure it gets picked
            return 0f;
        }

        private float ScoreBlocking(Board board, Position pos)
        {
            State opp = (player == State.X) ? State.O : State.X;
            Board localBoard = new Board(board); //use constructor because Board is reference type
            localBoard[pos] = opp; //make move
            State winner = GameState.CheckWin(localBoard).winner;
            if (winner == opp) { return 1000000; } //High score, but less than winning move (10^6)
            return 0f;
        }

        private float ScoreExtend(Board board, Position pos)
        {
            Board localBoard = new Board(board);
            localBoard[pos] = player;
            Position[] directions = new Position[4]
            { Position.up, Position.right, Position.right + Position.up, Position.right + Position.down };
            localBoard[pos] = player;
            Dictionary<Position, sbyte> inRow = new Dictionary<Position, sbyte>();
            foreach (Position direction in directions)
            {
                sbyte count = 0;
                Position newPos = pos;
                //Count in positive direction
                while (localBoard[newPos] == player)
                {
                    count++;
                    newPos = newPos + direction;
                }
                newPos = pos;
                //Count in negative direction
                while (localBoard[newPos] == player)
                {
                    count++;
                    newPos = newPos + -1 * direction;
                }
                count--; //Remove double counting of pos
                inRow.Add(direction, count);
            }
            float score = 0;
            foreach (KeyValuePair<Position, sbyte> entry in inRow) //Square the count to give higher weight to longer lines
            { score += ((entry.Value) * (entry.Value))/(GameState.ToWin * GameState.ToWin); } //divide by GameState.ToWin to keep const across different board sizes

            return score;
        }

        //Find 2 from n-2 to n-1 -> write new CheckWin for n-1
        private float ScoreFork(Board board, Position pos)
        {
            Board localBoard = new Board(board); //use constructor because Board is reference type
            localBoard[pos] = player; //make move
            //Find all n-1 in a row
            int forkCount = 0;
            List<Position> newPositions = InitScoring(localBoard).positions;
            foreach(Position newPos in newPositions)
            {
                if (ScoreWinning(localBoard, newPos) == float.PositiveInfinity)
                { forkCount++; }
            }
            if (forkCount >= 2) { return 50000; } //High score, but less than blocking move (5*10^4)
            return 0f;
        }

        private float ScoreCentral(Board board, Position pos)
        {
            float center = (board.numCols - 1) / 2f;
            return -10 * (Math.Abs(pos.col - center) + center); //closer to centre is higher score (+ center so no negative)
        }

        //Makes sure doesnt allow opponent to win on next move (e.g. horizontal row, then the move fills gap)
        private float Score2DepthWin(Board board, Position pos)
        {
            Board localBoard = new Board(board); //use constructor because Board is reference type
            localBoard[pos] = player; //make move
            List<Position> newPositions = InitScoring(localBoard).positions;
            foreach (Position newPos in newPositions)
            {
                if (ScoreBlocking(localBoard, newPos) == 1000000) //opponent can win next move
                { return -100000; } //negative score to avoid this move
            }
            return 0f;
        }

        private float ScoreMinimax(Board board, Position pos)
        {
            float ScoreAll(Board b, Position p)
            {
                float sc = 0f;
                sc += ScoreWinning(b, p);
                sc += ScoreBlocking(b, p);
                sc += ScoreExtend(b, p);
                sc += ScoreFork(b, p);
                sc += ScoreCentral(b, p);
                sc += Score2DepthWin(b, p);
                return sc;
            }

            Board localBoard = new Board(board); //use constructor because Board is reference type
            State opp = (player == State.X) ? State.O : State.X;

            localBoard[pos] = player; //make move
            float score = 0f;
            score += ScoreAll(localBoard, pos);

            List<Position> newPositions = InitScoring(localBoard).positions;
            foreach (Position newPos in newPositions)
            {
                Board opponentBoard = new Board(localBoard);
                opponentBoard[newPos] = opp; //opponent makes move
                score -= ScoreAll(opponentBoard, newPos) * 0.2f; //opp score 0.2 weightage bcs subtracted multiple times
            }

            return score;
        }

        //For each move method ->
        //find valid positions with getPos -> use heuristics to score each position -> return best position with findBest

        private sbyte randMove(Board board)
        {
            var (validPos, scores) = InitScoring(board);
            if (validPos.Count == 0) { return 127; } //if board full
            return findBest(scores); //all scores are 0, so random valid move (findBest handles random selection)
        }

        private sbyte completeMove(Board board)
        {
            var (validPos, scores) = InitScoring(board);
            if (validPos.Count == 0) { return 127; } //if board full
            //Score each position
            foreach (Position pos in validPos)
            {
                scores[pos.col] += ScoreWinning(board, pos);
                scores[pos.col] += ScoreBlocking(board, pos);
            }
            return findBest(scores);
        }

        private sbyte extendMove(Board board)
        {
            var (validPos, scores) = InitScoring(board);
            if (validPos.Count == 0) { return 127; } //if board full
            //Score each position
            foreach (Position pos in validPos)
            {
                scores[pos.col] += ScoreWinning(board, pos);
                scores[pos.col] += ScoreBlocking(board, pos);
                scores[pos.col] += ScoreExtend(board, pos);
                scores[pos.col] += ScoreFork(board, pos);
            }
            return findBest(scores);
        }

        private sbyte centralMove(Board board)
        {
            var (validPos, scores) = InitScoring(board);
            if (validPos.Count == 0) { return 127; }
            //Score each position
            foreach (Position pos in validPos)
            {
                scores[pos.col] += ScoreWinning(board, pos);
                scores[pos.col] += ScoreBlocking(board, pos);
                scores[pos.col] += ScoreExtend(board, pos);
                scores[pos.col] += ScoreFork(board, pos);
                scores[pos.col] += ScoreCentral(board, pos);
                scores[pos.col] += Score2DepthWin(board, pos);
            }
            return findBest(scores);
        }

        private sbyte minimaxMove(Board board)
        {
            var (validPos, scores) = InitScoring(board);
            if (validPos.Count == 0) { return 127; }
            State opponent = (player == State.X) ? State.O : State.X;
            //Score each position
            foreach (Position pos in validPos)
            {
                scores[pos.col] += ScoreMinimax(board, pos);
            }
            return findBest(scores);
        }
    }
}
