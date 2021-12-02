using System;
using System.Text;

namespace TheSmartest.Client;

public static class ConsoleView
{
    private static readonly StringBuilder MainScreen = new();
    
    public static string PlayerName { get; set; }
    public static int PlayerId { get; set; }
    public static GameStatus GameStatus { get; set; }
    public static int TotalPlayers { get; set; }
    public static int VotedPlayers { get; set; }
    public static int CurrentQuestionNumber { get; set; }
    public static int TotalQuestions { get; set; }

    public static void Redraw()
    {
        Console.Clear();
        Console.WriteLine(BuildStatusBar());
        Console.WriteLine(MainScreen.ToString());
    }

    private static string BuildStatusBar()
    {
        var sb = new StringBuilder();
        sb.Append($"Player name: {PlayerName}\tPlayer ID: {PlayerId}\tGame status: {GameStatus}");

        switch (GameStatus)
        {
            case GameStatus.WaitingForStart:
                sb.Append($"\tTotal players: {TotalPlayers}\tVoted players: {VotedPlayers}");
                break;
            case GameStatus.Started:
                sb.Append($"\tCurrent question: {CurrentQuestionNumber}\tTotal questions: {TotalQuestions}");
                break;
        }

        sb.AppendLine();
        return sb.ToString();
    }

    public static void AppendLine(string line) => MainScreen.AppendLine(line);

    public static void PrintLine(string line)
    {
        AppendLine(line);
        Redraw();
    }

    public static void Clear()  {
        MainScreen.Clear();
        Console.Clear();
    }

    public static void UpdateStatus(CheckStatusResponse statusResponse)
    {
        GameStatus = statusResponse.Status;
        TotalPlayers = statusResponse.TotalPlayers;
        VotedPlayers = statusResponse.Votes;
        TotalQuestions = statusResponse.TotalQuestions;
        return;
    }
}