using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Grpc.Core;

namespace TheSmartest.Server
{
    public class GameServicesImplementation : Game.GameBase
    {
        private readonly ConcurrentDictionary<int, Player> _players;
        private int _lastPlayerId;
        private int _votesCount;
        private GameStatus _gameStatus;

        public GameServicesImplementation()
        {
            _players = new ConcurrentDictionary<int, Player>();
            _gameStatus = GameStatus.WaitingForStart;
        }

        public override Task<PlayerAnswer> RegisterPlayer(PlayerRequest request, ServerCallContext context)
        {
            var id = Interlocked.Increment(ref _lastPlayerId);
            var player = new Player() { Name = request.PlayerName };
            
            if (!_players.TryAdd(id, player))
            {
                var errorAnswer = new PlayerAnswer()
                {
                    Success = false, 
                    ErrorMessage = "Error happened while trying to register new player"
                };
                
                return Task.FromResult(errorAnswer);
            }

            var response = new PlayerAnswer() { Success = true, PlayerId = id };
            return Task.FromResult(response);
        }

        public override Task<CheckStatusResponse> CheckGameStatus(CheckStatusRequest request, ServerCallContext context)
        {
            var response = new CheckStatusResponse()
            {
                Status = _gameStatus,
                TotalPlayers = _players.Count,
                Votes = _votesCount,
                TotalQuestions = 0 //TODO: get real question count 
            };
            return Task.FromResult(response);
        }

        public override Task<VoteResponse> VoteForStart(VoteRequest request, ServerCallContext context)
        {
            if (!_players.ContainsKey(request.PlayerId))
            {
                var errorAnswer = new VoteResponse()
                {
                    Success = false,
                    ErrorMessage = $"Player with id = {request.PlayerId} is not registered"
                };
                return Task.FromResult<VoteResponse>(errorAnswer);
            }

            _votesCount++;
            
            return Task.FromResult<VoteResponse>(new VoteResponse() {Success = true});
        }

        public override Task<QuestionResponse> GetQuestion(QuestionRequest request, ServerCallContext context)
        {
            return base.GetQuestion(request, context);
        }

        public override Task<AnswerResponse> SendAnswer(AnswerRequest request, ServerCallContext context)
        {
            return base.SendAnswer(request, context);
        }

        public override Task<WinnerResponse> GetWinner(WinnerRequest request, ServerCallContext context)
        {
            var maxScore = _players.Select(p => p.Value.Score).Max();
            var winners = _players
                .Where(p => p.Value.Score.Equals(maxScore))
                .Select(p => p.Value);
            
            var orderedPlayers = _players
                .Select(p => p.Value)
                .OrderByDescending(p => p.Score);

            var response = new WinnerResponse();
            if (winners.Count() > 1)
            {
                response.Winners = new Players();
                response.Winners.PlayersList.AddRange(winners);
            }
            else
            {
                response.Winner = winners.First();
            }

            response.ScoreTable.AddRange(orderedPlayers);
            return Task.FromResult(response);
        }
    }
}