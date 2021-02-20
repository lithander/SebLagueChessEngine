using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Chess;


namespace MinimalChessEngine
{
    public static class Uci
    {
        static public void BestMove(Move move)
        {
            string moveStr = BoardRepresentation.SquareNameFromIndex(move.StartSquare);
            moveStr += BoardRepresentation.SquareNameFromIndex(move.TargetSquare);
            // add promotion piece
            if (move.IsPromotion)
            {
                int promotionPieceType = move.PromotionPieceType;
                moveStr += PGNCreator.GetSymbolFromPieceType(promotionPieceType);
            }

            Console.WriteLine($"bestmove {moveStr}");
        }

        static internal void Info(int depth, int score, long nodes, int timeMs, Move[] pv)
        {
            double tS = Math.Max(1, timeMs) / 1000.0;
            int nps = (int)(nodes / tS);
            Console.WriteLine($"info depth {depth} score cp {score} nodes {nodes} nps {nps} time {timeMs} pv {string.Join(' ', pv)}");
        }

        static public void Log(string message)
        {
            Console.WriteLine($"info string {message}");
        }
    }

    class Engine
    {
        const int MOVE_TIME_MARGIN = 10;

        MoveGenerator _moveGen = new MoveGenerator();
        Search _search = null;
        Thread _searching = null;
        Move _best = default;
        long _t0 = -1;
        long _tN = -1;
        int _timeBudget = 0;
        int _searchDepth = 0;
        Board _board = new Board();

        public bool Running { get; private set; }

        public Engine()
        {
        }

        public void Start()
        {
            Stop();
            Running = true;
        }

        internal void Quit()
        {
            Stop();
            Running = false;
        }

        //*************
        //*** SETUP ***
        //*************

        internal void SetupPosition()
        {
            SetupPosition(FenUtility.startFen);
        }

        internal void SetupPosition(string fen)
        {
            Stop();
            _board.LoadPosition(fen);
        }

        internal void Play(Move move)
        {
            Stop();
            _board.MakeMove(WithFlags(move));
        }


        //TJ: setting the right flags after parsing the move from a string without having a board instance as context isn't possible and so we add the flags here
        public Move WithFlags(Move move)
        {
            foreach (var candidate in _moveGen.GenerateMoves(_board, true))
                if (candidate.StartSquare == move.StartSquare && candidate.TargetSquare == move.TargetSquare && candidate.PromotionPieceType == move.PromotionPieceType)
                    return candidate;

            //not found? well... that's... kinda bad
            throw new Exception(move.Name + " not valid in the given position!");
        }

        //**************
        //*** Search ***
        //**************

        internal void Go()
        {
            Stop();
            _searchDepth = int.MaxValue;
            _timeBudget = int.MaxValue;
            StartSearch();
        }

        internal void Go(int maxSearchDepth)
        {
            Stop();
            _searchDepth = maxSearchDepth;
            _timeBudget = int.MaxValue;
            StartSearch();
        }

        internal void Go(int timePerMove, int maxSearchDepth)
        {
            Stop();
            _searchDepth = maxSearchDepth;
            _timeBudget = timePerMove - MOVE_TIME_MARGIN;
            StartSearch();
        }

        internal void Go(int blackTime, int whiteTime, int blackIncrement, int whiteIncrement, int movesToGo, int maxSearchDepth)
        {
            Stop();
            _searchDepth = maxSearchDepth;
            int myTime = _board.WhiteToMove ? whiteTime : blackTime;
            int myIncrement = _board.WhiteToMove ? whiteIncrement : blackIncrement;
            int totalTime = myTime + myIncrement * (movesToGo - 1) - MOVE_TIME_MARGIN;
            _timeBudget = totalTime / movesToGo;
            Uci.Log($"Search budget set to {_timeBudget}ms!");
            StartSearch();
        }

        public void Stop()
        {
            if (_searching != null)
            {
                _timeBudget = 0; //this will cause the thread to terminate
                _searching.Join();
                _searching = null;
            }
        }

        //*****************
        //*** INTERNALS ***
        //*****************

        private void Search()
        {
            _tN = Now;
            _search.StartSearch(() => RemainingTimeBudget < 0);
            _best = _search.bestMove;
            Uci.BestMove(_best);
            _search = null;
            return;
        }

        private void StartSearch()
        {
            _t0 = Now;
            _search = new Search(_board, new AISettings());
            _searching = new Thread(Search);
            _searching.Priority = ThreadPriority.Highest;
            _searching.Start();
        }

        private long Now => Stopwatch.GetTimestamp();

        private int MilliSeconds(long ticks)
        {
            double dt = ticks / (double)Stopwatch.Frequency;
            return (int)(1000 * dt);
        }

        private int ElapsedMilliseconds => MilliSeconds(Now - _t0);

        private int RemainingTimeBudget => _timeBudget - ElapsedMilliseconds;
    }
}
