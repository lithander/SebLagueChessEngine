using Chess;
using MinimalChess;
using System;
using System.Threading.Tasks;
using static Chess.Move;

namespace MinimalChessEngine
{
    public static class Program
    {
        static Engine _engine = new Engine();

        static async Task Main(string[] args)
        {
            Console.WriteLine("SebLague Chess-AI (UCI capable)");
            _engine.Start();
            while (_engine.Running)
            {
                string input = await Task.Run(Console.ReadLine);
                ParseUciCommand(input);
            }
        }

        private static void ParseUciCommand(string input)
        {
            //remove leading & trailing whitecases, convert to lower case characters and split using ' ' as delimiter
            string[] tokens = input.Trim().Split();
            switch (tokens[0])
            {
                case "uci":
                    Console.WriteLine("id name SebLague Chess-AI");
                    Console.WriteLine("id author Sebastian Lague");
                    Console.WriteLine("uciok");
                    break;
                case "isready":
                    Console.WriteLine("readyok");
                    break;
                case "position":
                    UciPosition(tokens);
                    break;
                case "go":
                    UciGo(tokens);
                    break;
                case "ucinewgame":
                    break;
                case "stop":
                    _engine.Stop();
                    break;
                case "quit":
                    _engine.Quit();
                    break;
                default:
                    Console.WriteLine("UNKNOWN INPUT " + input);
                    return;
            }
        }

        private static void UciPosition(string[] tokens)
        {
            //position [fen <fenstring> | startpos ]  moves <move1> .... <movei>
            if (tokens[1] == "startpos")
                _engine.SetupPosition();
            else if (tokens[1] == "fen") //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
            {
                string fen = string.Join(' ', tokens, 2, tokens.Length - 2);
                _engine.SetupPosition(fen);
            }
            else
            {
                Uci.Log("'position' parameters missing or not understood. Assuming 'startpos'.");
                _engine.SetupPosition();
            }

            int firstMove = Array.IndexOf(tokens, "moves") + 1;
            if (firstMove == 0)
                return;

            for (int i = firstMove; i < tokens.Length; i++)
            {
                Move move = MoveFromUciNotation(tokens[i]);
                _engine.Play(move);
            }
        }

        public static Move MoveFromUciNotation(string uciMoveNotation)
        {
            if (uciMoveNotation.Length < 4)
                throw new ArgumentException($"Long algebraic notation expected. '{uciMoveNotation}' is too short!");
            if (uciMoveNotation.Length > 5)
                throw new ArgumentException($"Long algebraic notation expected. '{uciMoveNotation}' is too long!");

            //expected format is the long algebraic notation without piece names
            https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
            //Examples: e2e4, e7e5, e1g1(white short castling), e7e8q(for promotion)
            string fromSquare = uciMoveNotation.Substring(0, 2);
            string toSquare = uciMoveNotation.Substring(2, 2);
            int fromIndex = Notation.ToSquareIndex(fromSquare);
            int toIndex = Notation.ToSquareIndex(toSquare);
                        
            
            int flags = 0;
            //the presence of a 5th character should mean promotion
            if(uciMoveNotation.Length == 5)
            {
                //promotion flags
                char promotion = uciMoveNotation[4];
                flags = promotion switch
                {
                    'N' => Flag.PromoteToKnight,
                    'B' => Flag.PromoteToBishop,
                    'R' => Flag.PromoteToRook,
                    'Q' => Flag.PromoteToQueen,
                    'n' => Flag.PromoteToKnight,
                    'b' => Flag.PromoteToBishop,
                    'r' => Flag.PromoteToRook,
                    'q' => Flag.PromoteToQueen,
                    _ => throw new NotImplementedException()
                };
            }

            return new Move(fromIndex, toIndex, flags);
        }

        private static void UciGo(string[] tokens)
        {
            if (TryParse(tokens, "movetime", out int timePerMove))
            {
                //Fixed move time e.g. 5 Minutes per Move = go movetime 300000
                TryParse(tokens, "depth", out int searchDepth, int.MaxValue);
                _engine.Go(timePerMove, searchDepth);
            }
            else if (TryParse(tokens, "btime", out int blackTime) && TryParse(tokens, "wtime", out int whiteTime))
            {
                //Searching on a budget that may increase at certain intervals
                //40 Moves in 5 Minutes = go wtime 300000 btime 300000 movestogo 40
                //40 Moves in 5 Minutes, 1 second increment per Move =  go wtime 300000 btime 300000 movestogo 40 winc 1000 binc 1000 movestogo 40
                //5 Minutes total, no increment (sudden death) = go wtime 300000 btime 300000
                TryParse(tokens, "binc", out int blackIncrement);
                TryParse(tokens, "winc", out int whiteIncrement);
                TryParse(tokens, "movestogo", out int movesToGo, 40); //assuming 30 e.g. spend 1/30th of total budget on the move
                TryParse(tokens, "depth", out int searchDepth, int.MaxValue);
                _engine.Go(blackTime, whiteTime, blackIncrement, whiteIncrement, movesToGo, searchDepth);
            }
            else if (TryParse(tokens, "depth", out int searchDepth))
            {
                _engine.Go(searchDepth);
            }
            else if (IsDefined(tokens, "infinite"))
            {
                //Infinite = go infinite
                _engine.Go();
            }
            else
            {
                Uci.Log("'go' parameters missing or not understood. Stop the search using 'stop'.");
                _engine.Go();
            }
        }

        private static bool IsDefined(string[] tokens, string name)
        {
            return Array.IndexOf(tokens, name) >= 0;
        }

        private static bool TryParse(string[] tokens, string name, out int value, int defaultValue = 0)
        {
            value = defaultValue;
            int iParam = Array.IndexOf(tokens, name);
            if (iParam < 0)
                return false;
            int iValue = iParam + 1;
            if (iValue >= tokens.Length)
                return false;

            return int.TryParse(tokens[iValue], out value);
        }
    }
}
