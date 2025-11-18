using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AP1.Modeles;
using AP1.Services;
using Microsoft.Maui.Controls;
using MauiApp1.Vues;

namespace AP1.Vues;

public partial class HomePage : ContentPage
{
    private readonly Apis Apis = new Apis();
    private ObservableCollection<Competition> _competitions = new ObservableCollection<Competition>();

    public HomePage()
    {
        InitializeComponent();
        NewTournamentButton.Clicked += OnNewTournamentClicked;
        ManageTeamsButton.Clicked += OnManageTeamsClicked;
        CompetitionsCollection.ItemsSource = _competitions;

        // Ajoute la compétition créée depuis la page de création
        MessagingCenter.Subscribe<AccueilCreeCompetition, Competition>(this, "CompetitionCreated", (sender, comp) =>
        {
            if (comp is not null)
            {
                _competitions.Insert(0, comp);
                UpdateCounters();
            }
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        SetLoading(true);
        ErrorLabel.IsVisible = false;
        try
        {
            var loaded = await Apis.GetAllAsync<Competition>("api/mobile/GetAllCompetitions");
            MergeLoadedCompetitions(loaded);
        }
        catch (Exception)
        {
            // Ne jamais remplacer la collection pour ne pas perdre les éléments locaux créés
            var offline = new ObservableCollection<Competition>
            {
                new Competition { Id = 1, DateDeb = DateTime.Today.AddDays(-1), DateFin = DateTime.Today.AddDays(1), Nom = "Compétition (offline) 1" },
                new Competition { Id = 2, DateDeb = DateTime.Today.AddDays(-7), DateFin = DateTime.Today.AddDays(-2), Nom = "Compétition (offline) 2" },
            };
            MergeLoadedCompetitions(offline);
            ErrorLabel.Text = "Mode déconnecté : données de test affichées.";
            ErrorLabel.IsVisible = true;
        }

        UpdateCounters();

        SetLoading(false);
    }

    private void MergeLoadedCompetitions(IEnumerable<Competition> loaded)
    {
        if (loaded is null) return;
        var loadedById = loaded.ToDictionary(c => c.Id, c => c);

        // Update existing items present in API payload
        for (int i = 0; i < _competitions.Count; i++)
        {
            var existing = _competitions[i];
            if (loadedById.TryGetValue(existing.Id, out var fromApi))
            {
                existing.Nom = string.IsNullOrWhiteSpace(fromApi.Nom) ? existing.Nom : fromApi.Nom;
                existing.DateDeb = fromApi.DateDeb;
                existing.DateFin = fromApi.DateFin;
            }
        }

        // Add any new items from API that aren't yet in the list
        var existingIds = new HashSet<int>(_competitions.Select(c => c.Id));
        foreach (var c in loaded)
        {
            if (!existingIds.Contains(c.Id))
            {
                _competitions.Add(c);
            }
        }
    }

    private void UpdateCounters()
    {
        var now = DateTime.Now;
        int active = _competitions.Count(c => c.DateDeb <= now && now <= c.DateFin);
        ActiveTournamentsLabel.Text = active.ToString();
        TeamsCountLabel.Text = "0";
        StudentsCountLabel.Text = "0";
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        CompetitionsCollection.IsVisible = !isLoading;
    }

    private async void OnNewTournamentClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AccueilCreeCompetition());
    }

    private async void OnManageTeamsClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Sélection requise", "Sélectionnez un tournoi dans la liste pour le gérer.", "OK");
    }

    private async void OnCompetitionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is Competition selected)
        {
            // Clear selection to allow re-select later
            ((CollectionView)sender!).SelectedItem = null;
            await Navigation.PushAsync(new AccueilGererEquipe(selected));
        }
    }

    private async void OnDeleteCompetitionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Competition competition)
        {
            var confirm = await DisplayAlert("Supprimer", $"Supprimer \"{competition.Nom}\" ?", "Oui", "Non");
            if (confirm)
            {
                // TODO backend : appeler l'API de suppression
                _competitions.Remove(competition);
                UpdateCounters();
            }
        }
    }
}


