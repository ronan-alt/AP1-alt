using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AP1.Modeles;
using AP1.Services;
using Microsoft.Maui.Controls;
using MauiApp1.Vues;

namespace AP1.Vues;

public partial class AccueilProfesseur : ContentPage
{
    private readonly Apis Apis = new Apis();
    private ObservableCollection<Competition> _competitions = new ObservableCollection<Competition>();
    private Competition? _selectedCompetition;
    private CompetitionListResponse? _lastSnapshot;
    
     public static Dictionary<int, string> CompetitionNames { get; } = new();

    public AccueilProfesseur()
    {
        InitializeComponent();
        NewTournamentButton.Clicked += OnNewTournamentClicked;
        ManageTeamsButton.Clicked += OnManageTeamsClicked;
        CompetitionsCollection.ItemsSource = _competitions;

        MessagingCenter.Subscribe<AccueilCreeCompetition, Competition>(this, "CompetitionCreated", async (sender, comp) =>
        {
            if (comp != null && !string.IsNullOrWhiteSpace(comp.Nom))
            {
                CompetitionNames[comp.Id] = comp.Nom;
            }
            
            await LoadDataAsync();
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
            var response = await Apis.GetSingleAsync<CompetitionListResponse>("api/mobile/competitions");
            
            if (response?.Competitions != null)
            {
                foreach (var c in response.Competitions)
                {
                    c.FixNameFromExtraData();
                    
                    if (CompetitionNames.ContainsKey(c.Id))
                    {
                        c.Nom = CompetitionNames[c.Id];
                    }
                }
            }

            _lastSnapshot = response;
            MergeLoadedCompetitions(response?.Competitions);
            ApplyCounters(response);
        }
        catch (Exception)
        {
            var offline = new List<Competition>
            {
                new() { Id = 1, Nom = "Compétition (offline) 1", DateDeb = DateTime.Today, DateFin = DateTime.Today.AddDays(1) },
                new() { Id = 2, Nom = "Compétition (offline) 2", DateDeb = DateTime.Today.AddDays(2), DateFin = DateTime.Today.AddDays(4) }
            };
            MergeLoadedCompetitions(offline);
            ApplyCounters();
            ErrorLabel.Text = "Mode déconnecté : données de test affichées.";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void MergeLoadedCompetitions(IEnumerable<Competition>? loaded)
    {
        if (loaded is null) return;
        var loadedById = loaded.ToDictionary(c => c.Id, c => c);

        for (int i = 0; i < _competitions.Count; i++)
        {
            var existing = _competitions[i];
            if (loadedById.TryGetValue(existing.Id, out var fromApi))
            {
               
                _competitions[i] = fromApi;
            }
        }

        var existingIds = new HashSet<int>(_competitions.Select(c => c.Id));
        foreach (var comp in loaded)
        {
            if (!existingIds.Contains(comp.Id))
            {
                
                if (string.IsNullOrWhiteSpace(comp.Nom) || comp.Nom.StartsWith("Compétition #"))
                {
                    Task.Run(async () => await FetchCompetitionDetails(comp));
                }
                _competitions.Add(comp);
            }
        }
    }

    private async Task FetchCompetitionDetails(Competition comp)
    {
        try
        {
           
            var details = await Apis.GetSingleAsync<CompetitionTeamsResponse>($"api/mobile/competitions/{comp.Id}/teams");
            
            string? newName = null;

            if (details?.Competition != null)
            {
        
                details.Competition.FixNameFromExtraData();
                newName = details.Competition.Nom;
            }

            
            if (string.IsNullOrWhiteSpace(newName) || newName.Contains("(Nom manquant)"))
            {
                newName = $"Compétition #{comp.Id} (Non chargée)";
            }

            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                comp.Nom = newName;
                
                var index = _competitions.IndexOf(comp);
                if (index >= 0) _competitions[index] = comp;
            });
        }
        catch
        {
            
             MainThread.BeginInvokeOnMainThread(() =>
            {
                comp.Nom = $"Compétition #{comp.Id} (Err. Réseau)";
                var index = _competitions.IndexOf(comp);
                if (index >= 0) _competitions[index] = comp;
            });
        }
    }

    private void ApplyCounters(CompetitionListResponse? snapshot = null)
    {
        if (snapshot is not null)
        {
            TotalCompetitionsLabel.Text = snapshot.TotalCount.ToString();
            InProgressCompetitionsLabel.Text = snapshot.InProgressCount.ToString();
            UpcomingCompetitionsLabel.Text = snapshot.UpcomingCount.ToString();
            return;
        }

        var now = DateTime.Now;
        TotalCompetitionsLabel.Text = _competitions.Count.ToString();
        InProgressCompetitionsLabel.Text = _competitions.Count(c => c.DateDeb <= now && now <= c.DateFin).ToString();
        UpcomingCompetitionsLabel.Text = _competitions.Count(c => c.DateDeb > now).ToString();
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
        if (_selectedCompetition is null)
        {
            await DisplayAlert("Sélection requise", "Sélectionnez un tournoi dans la liste pour le gérer.", "OK");
            return;
        }

        await Navigation.PushAsync(new AccueilGererEquipe(_selectedCompetition));
    }

    private async void OnCompetitionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is Competition selected)
        {
            _selectedCompetition = selected;
            ((CollectionView)sender!).SelectedItem = null;
            await Navigation.PushAsync(new AccueilGererEquipe(selected));
        }
    }

    private async void OnDeleteCompetitionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Competition competition)
        {
            var confirm = await DisplayAlert("Supprimer", $"Supprimer \"{competition.Nom}\" ?", "Oui", "Non");
            if (!confirm) return;

            try
            {
                await Apis.DeleteAsync($"api/mobile/competitions/{competition.Id}");
                _competitions.Remove(competition);
                ApplyCounters();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (message.Contains("Integrity constraint violation") || message.Contains("foreign key constraint fails"))
                {
                    message = "Impossible de supprimer cette compétition car elle contient des données liées (épreuves, équipes, etc.). Veuillez supprimer ces éléments d'abord.";
                }
                else if (message.Contains("500"))
                {
                    message = "Erreur serveur lors de la suppression. Vérifiez que la compétition ne contient pas d'éléments liés.";
                }

                await DisplayAlert("Erreur", message, "OK");
            }
        }
    }
}


