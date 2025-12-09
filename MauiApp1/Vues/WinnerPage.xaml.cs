
using MauiApp1.Modeles;
using MauiApp1.Services;

using MauiApp1.Modeles;

namespace MauiApp1.Vues;

public partial class WinnerPage : ContentPage
{
    private LeaderboardEntry Winner;

    public WinnerPage(LeaderboardEntry gagnant)
    {
        InitializeComponent();
        Winner = gagnant;

        // afficher les infos
        LblWinner.Text = $"Joueur {Winner.userId}";
        LblScore.Text = $"Score : {Winner.points}";
    }

    private async void OnReturnClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
    private async void OnReplayClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new QuizNohadPage());
    }
}
