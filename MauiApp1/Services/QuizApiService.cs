using System.Net.Http.Json;
using MauiApp1.Modeles;

namespace MauiApp1.Services;

public class QuizApiService
{
    private readonly HttpClient http;

    public QuizApiService()
    {
        http = new HttpClient
        {
            BaseAddress = new Uri("http://172.17.0.62")
        };
    }

    public async Task<List<Quiz>> GetAllQuizzes()
    {
        try
        {
            return await http.GetFromJsonAsync<List<Quiz>>("/api/nohad/quizzes")
                ?? new List<Quiz>();
        }
        catch
        {
            return new List<Quiz>();
        }
    }
    public async Task<List<LeaderboardEntry>> GetLeaderboard()
    {
        try
        {
            return await http.GetFromJsonAsync<List<LeaderboardEntry>>("/api/nohad/leaderboard")
                ?? new List<LeaderboardEntry>();
        }
        catch
        {
            return new List<LeaderboardEntry>();
        }
    }
}

