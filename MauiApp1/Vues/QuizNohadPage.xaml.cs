using MauiApp1.Modeles;
using MauiApp1.Services;

namespace MauiApp1.Vues;

public partial class QuizNohadPage : ContentPage
{
    private readonly QuizApiService api = new QuizApiService();
    private Quiz quiz;
    private int questionIndex = 0;
    private int score = 0;

    private DateTime startTime;
    private int tempsRestant;
    private bool timerRunning = false;

    public QuizNohadPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var quizzes = await api.GetAllQuizzes();
        if (quizzes == null || quizzes.Count == 0)
        {
            await DisplayAlert("Erreur", "Aucun quiz trouvé dans l’API.", "OK");
            await Navigation.PopAsync();
            return;
        }

        quiz = quizzes[0]; // prendre le premier quiz
        questionIndex = 0;
        score = 0;

        AfficherQuestion();
    }

    private void AfficherQuestion()
    {
        var q = quiz.Questions[questionIndex];

        LblQuestion.Text = q.Enonce;
        LblScore.Text = $"Score : {score}";

        // Masquer tous les boutons
        BtnA.IsVisible = BtnB.IsVisible = BtnC.IsVisible = BtnD.IsVisible = false;

        // Liste des boutons
        var btns = new[] { BtnA, BtnB, BtnC, BtnD };

        // Afficher les choix dynamiquement
        for (int i = 0; i < q.Choices.Count && i < btns.Length; i++)
        {
            btns[i].IsVisible = true;
            btns[i].Text = q.Choices[i].Texte;
            btns[i].CommandParameter = q.Choices[i].Id;
        }

        // Timer pour cette question
        tempsRestant = q.DureeSecondes;
        LblTimer.Text = $"{tempsRestant}s";

        startTime = DateTime.Now;
        StartTimer();
    }

    private void StartTimer()
    {
        timerRunning = true;

        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!timerRunning)
                return false;

            tempsRestant--;
            LblTimer.Text = $"{tempsRestant}s";

            if (tempsRestant <= 0)
            {
                timerRunning = false;
                PasserQuestion();
                return false;
            }

            return true;
        });
    }

    private async void OnChoiceClicked(object sender, EventArgs e)
    {
        timerRunning = false;

        var btn = (Button)sender;
        int choiceId = Convert.ToInt32(btn.CommandParameter);

        var q = quiz.Questions[questionIndex];
        var choix = q.Choices.First(c => c.Id == choiceId);

        // Score local (affiché seulement pour l'élève)
        if (choix.IsCorrect)
        {
            score += 5;
            LblScore.Text = $"Score : {score}";
        }

        // (plus tard, ici on enverra la réponse userId-choice à l'API)

        PasserQuestion();
    }

    private async void PasserQuestion()
    {
        questionIndex++;

        // 👉 Fin du quiz ?
        if (questionIndex >= quiz.Questions.Count)
        {
            await DisplayAlert("Quiz terminé", $"Score final : {score}", "OK");

            // 👉 Récupérer le classement depuis l'API
            var leaderboard = await api.GetLeaderboard();

            if (leaderboard.Count > 0)
            {
                // 👉 Gagnant = celui qui a le plus de points
                var gagnant = leaderboard.OrderByDescending(e => e.points).First();

                // 👉 Ouvrir la page WinnerPage AVEC le gagnant
                await Navigation.PushAsync(new WinnerPage(gagnant));
            }
            else
            {
                await DisplayAlert("Erreur", "Aucun gagnant trouvé.", "OK");
            }

            return;
        }

        // Sinon passer à la prochaine question
        AfficherQuestion();
    }

}


