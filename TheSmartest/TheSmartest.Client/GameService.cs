using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TheSmartest.Client;

public class GameService
{
    private readonly Game.GameClient _gameClient;
    private int _playerId;
    private CheckStatusResponse _status = new();
    private int _currentQuestionId;
    private bool _isGameEnded = false;

    public GameService(Game.GameClient gameClient)
    {
        _gameClient = gameClient;
    }

    public async Task Start()
    {
        await Register();
#pragma warning disable CS4014
        Task.Factory.StartNew(CheckGameStatus);
#pragma warning restore CS4014
        await StartMainCycle();
    }

    private async Task Register()
    {
        ConsoleView.Clear();
        ConsoleView.PrintLine("Hello! If you want to join game, just enter your name and press enter\n");
        var name = Console.ReadLine();
        var registerRequest = new PlayerRequest { PlayerName = name };
        var registerResult = await _gameClient.RegisterPlayerAsync(registerRequest);

        if (registerResult.Success)
        {
            _playerId = registerResult.PlayerId;
            ConsoleView.PlayerId = _playerId;
            ConsoleView.PlayerName = name;
            ConsoleView.TotalPlayers = registerResult.TotalPlayers;
        }
        else
        {

            while (!registerResult.Success)
            {
                ConsoleView.Clear();
                ConsoleView.PrintLine(
                    $"Oops, error happened:\n\t{registerResult.ErrorMessage}\nWant to try again?[Y/N]");
                var input = (char)Console.Read();
                switch (input)
                {
                    case 'Y' or 'y':
                        registerResult = await _gameClient.RegisterPlayerAsync(registerRequest);
                        if (!registerResult.Success)
                            continue;

                        _playerId = registerResult.PlayerId;
                        ConsoleView.PlayerId = _playerId;
                        ConsoleView.PlayerName = name;
                        ConsoleView.TotalPlayers = registerResult.TotalPlayers;
                        break;
                    case 'N' or 'n':
                        //todo: implement some exit logic
                        break;
                    default:
                        ConsoleView.Clear();
                        ConsoleView.PrintLine("Invalid input, try again");
                        break;
                }
            }
        }

        ConsoleView.Clear();
        ConsoleView.PrintLine("Success!");
    }

    private async Task StartMainCycle()
    {
        while (true)
        {
            if (_status is null)
                continue;
            
            switch (_status.Status)
            {
                case GameStatus.WaitingForStart:
                    await VoteForStart();
                    break;
                case GameStatus.Started:
                    await StartQuizCycle();
                    break;
                case GameStatus.Finished:
                    await DrawFinishScreen();
                    return;
            }
        }
    }

    private async Task VoteForStart()
    {
        ConsoleView.Clear();
        ConsoleView.PrintLine("Enter 'vote' if you want to vote for start");

        var success = false;
        while (!success)
        {
            var input = Console.ReadLine();
            switch (input)
            {
                case "vote":
                    var voteResult = await _gameClient.VoteForStartAsync(new VoteRequest { PlayerId = _playerId });
                    if (!voteResult.Success)
                        ConsoleView.PrintLine($"{voteResult.ErrorMessage}. Try again");
                    else
                    {
                        ConsoleView.PrintLine("Success! Waiting for start");
                        success = true;
                    }
                    break;
                default:
                    ConsoleView.PrintLine("Invalid input. Try again");
                    break;
            }
        }

        while (_status.Status is GameStatus.WaitingForStart)
        {
        }
    }

    private async Task StartQuizCycle()
    {
        while (_status.Status is GameStatus.Continues)
        {
            ConsoleView.Clear();
            
            if (_currentQuestionId == _status.TotalQuestions)
            {
                ConsoleView.PrintLine("Waiting for others to finish");
                await Task.Delay(500);
                continue;
            }
                
            var question = await _gameClient.GetQuestionAsync(new QuestionRequest
            {
                PlayerId = _playerId, 
                PreviousQuestionId = _currentQuestionId
            });

            _currentQuestionId = question.QuestionId;
            ConsoleView.CurrentQuestionNumber = _currentQuestionId;
            ConsoleView.AppendLine($"Question is:\n\t{question.Question}");

            foreach (var questionAnswer in question.Answers)
            {
                ConsoleView.AppendLine($"{questionAnswer.AnswerId} - {questionAnswer.AnswerText}");
            }
            
            ConsoleView.PrintLine("Enter answer's number:");

            var answerId = int.Parse(Console.ReadLine());
            var answerResult = await _gameClient.SendAnswerAsync(new AnswerRequest
                { 
                    AnswerId = answerId, 
                    PlayerId = _playerId,
                    QuestionId = _currentQuestionId 
                });
            
            ConsoleView.AppendLine(answerResult.IsCorrect ? "Correct!" : "Wrong!");
            ConsoleView.PrintLine("Press any key to get next question");

            Console.ReadKey();
        }
    }

    private async Task DrawFinishScreen()
    {
        ConsoleView.Clear();
        ConsoleView.AppendLine("GAME FINISHED\n");

        var winnersResult = await _gameClient.GetWinnerAsync(new WinnerRequest());
        ConsoleView.AppendLine(winnersResult.KindCase is WinnerResponse.KindOneofCase.Winner
            ? $"Winner is player {winnersResult.Winner.Name} with score {winnersResult.Winner.Score}"
            : @$"Winners are players {string.Join(' ', winnersResult.Winners.PlayersList.Select(p => p.Name))} 
                 with score {winnersResult.Winners.PlayersList.First().Score}");

        ConsoleView.AppendLine("The score table is next:");
        foreach (var player in winnersResult.ScoreTable)
        {
            ConsoleView.AppendLine($"\t{player.Name} - {player.Score}");
        }
        
        ConsoleView.PrintLine("Press any key to continue...");
        Console.ReadKey();
    }

    private void CheckGameStatus()
    {
        while (!_isGameEnded)
        {
            var status = _gameClient.CheckGameStatus(new CheckStatusRequest());
            if (!_status.Equals(status))
            {
                _status = status;
                ConsoleView.UpdateStatus(_status);
                ConsoleView.Redraw();
            }

            Thread.Sleep(50);
        }
    }
}