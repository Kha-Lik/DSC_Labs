using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace TheSmartest.Server
{
    public class GameServicesImplementation : Game.GameBase
    {
        private readonly ConcurrentDictionary<int, Player> _players;
        private readonly ConcurrentDictionary<int, int> _questionsCountByPlayer;
        private readonly ConcurrentQueue<int> _votedPlayers;
        private readonly IQuestionService _questionService;
        private int _votesCount;
        private int _lastPlayerId;
        private GameStatus _gameStatus;

        public GameServicesImplementation(IQuestionService questionService)
        {
            _players = new ConcurrentDictionary<int, Player>();
            _questionsCountByPlayer = new ConcurrentDictionary<int, int>();
            _votedPlayers = new ConcurrentQueue<int>();
            _questionService = questionService;
            _gameStatus = GameStatus.WaitingForStart;

            Task.Factory.StartNew(CheckEndGameConditions);
        }

        public override Task<PlayerAnswer> RegisterPlayer(PlayerRequest request, ServerCallContext context)
        {
            var id = Interlocked.Increment(ref _lastPlayerId);
            var player = new Player { Name = request.PlayerName };

            if (!_players.TryAdd(id, player))
            {
                var errorAnswer = new PlayerAnswer
                {
                    Success = false,
                    ErrorMessage = "Error happened while trying to register new player"
                };

                return Task.FromResult(errorAnswer);
            }

            var response = new PlayerAnswer { Success = true, PlayerId = id };
            return Task.FromResult(response);
        }

        public override Task<CheckStatusResponse> CheckGameStatus(CheckStatusRequest request, ServerCallContext context)
        {
            var response = new CheckStatusResponse
            {
                Status = _gameStatus,
                TotalPlayers = _players.Count,
                Votes = _votesCount,
                TotalQuestions = _questionService.GetQuestionsCount()
            };
            return Task.FromResult(response);
        }

        public override Task<VoteResponse> VoteForStart(VoteRequest request, ServerCallContext context)
        {
            if (!_players.ContainsKey(request.PlayerId))
            {
                var errorAnswer = new VoteResponse
                {
                    Success = false,
                    ErrorMessage = $"Player with id = {request.PlayerId} is not registered"
                };

                return Task.FromResult(errorAnswer);
            }

            if (_votedPlayers.Contains(request.PlayerId))
            {
                var errorAnswer = new VoteResponse
                {
                    Success = false,
                    ErrorMessage = "You already voted"
                };

                return Task.FromResult(errorAnswer);
            }

            _votedPlayers.Enqueue(request.PlayerId);
            if (Interlocked.Increment(ref _votesCount) == _players.Count)
                _gameStatus = GameStatus.Started;

            return Task.FromResult(new VoteResponse { Success = true });
        }

        public override Task<QuestionResponse> GetQuestion(QuestionRequest request, ServerCallContext context)
        {
            if (!_players.ContainsKey(request.PlayerId) || _gameStatus is not GameStatus.Started)
                return Task.FromResult(new QuestionResponse());

            var question = _questionService.GetNextQuestion(request.PreviousQuestionId);
            var response = new QuestionResponse
            {
                Question = question.Text,
                QuestionId = question.Id
            };

            return Task.FromResult(response);
        }

        public override Task<AnswerResponse> SendAnswer(AnswerRequest request, ServerCallContext context)
        {
            if (!_players.ContainsKey(request.PlayerId) || _gameStatus is not GameStatus.Started)
                return Task.FromResult(new AnswerResponse());

            var result = _questionService.CheckAnswer(request.QuestionId, request.AnswerId);
            _questionsCountByPlayer[request.PlayerId]++;

            return Task.FromResult(new AnswerResponse { IsCorrect = result });
        }

        public override Task<WinnerResponse> GetWinner(WinnerRequest request, ServerCallContext context)
        {
            if (_gameStatus is not GameStatus.Finished)
                return Task.FromResult(new WinnerResponse());

            var maxScore = _players.Select(p => p.Value.Score).Max();
            var winners = _players
                .Where(p => p.Value.Score.Equals(maxScore))
                .Select(p => p.Value).ToList();

            var orderedPlayers = _players
                .Select(p => p.Value)
                .OrderByDescending(p => p.Score);

            var response = new WinnerResponse();
            if (winners.Count > 1)
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

        public override Task<RestartResponse> Restart(RestartRequest request, ServerCallContext context)
        {
            if (!_players.ContainsKey(request.PlayerId) || _players.First().Key != request.PlayerId)
                return Task.FromResult(new RestartResponse
                {
                    Success = false,
                    ErrorMessage = "You aren't registered or you aren't the first registered player"
                });

            if (_gameStatus is not GameStatus.Finished)
                return Task.FromResult(new RestartResponse
                {
                    Success = false,
                    ErrorMessage = "You can't restart game that is not finished yes"
                });

            _votedPlayers.Clear();
            _questionsCountByPlayer.Clear();
            _gameStatus = GameStatus.WaitingForStart;

            return Task.FromResult(new RestartResponse { Success = true });
        }

        public override Task<UnregisterResponse> UnregisterPlayer(UnregisterRequest request, ServerCallContext context)
        {
            return !_players.ContainsKey(request.PlayerId) 
                ? Task.FromResult(new UnregisterResponse {Result = false}) 
                : Task.FromResult(!_players.TryRemove(request.PlayerId, out _) 
                    ? new UnregisterResponse {Result = false} 
                    : new UnregisterResponse {Result = true});
        }   

        private async void CheckEndGameConditions()
        {
            while (true)
            {
                await Task.Delay(50);

                if (_gameStatus is not GameStatus.Started)
                    continue;

                if (_questionsCountByPlayer.Any(kvp => kvp.Value != _questionService.GetQuestionsCount()))
                    continue;

                _gameStatus = GameStatus.Finished;
                return;
            }
        }
    }
}