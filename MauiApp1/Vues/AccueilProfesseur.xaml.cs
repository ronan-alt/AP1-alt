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
    private bool _isLoading = false;
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
        if (_isLoading) return;
        _isLoading = true;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            // 1. Récupération "Magique" (dynamic) pour contourner les problèmes de type
            dynamic result = await Apis.GetSingleAsync<CompetitionListResponse>("api/mobile/competitions");
            
            // 2. On essaie de récupérer la liste, peu importe la structure retournée
            List<Competition> loadedCompetitions = new List<Competition>();

            try 
            {
                // Cas 1 : C'est directement l'objet
                if (result != null && result.Competitions != null)
                {
                    loadedCompetitions = result.Competitions;
                }
            }
            catch
            {
                // Cas 2 : C'est une liste (fallback)
                try
                {
                     var list = (IEnumerable<dynamic>)result;
                     var first = list?.FirstOrDefault();
                     if (first != null && first.Competitions != null)
                     {
                         loadedCompetitions = first.Competitions;
                     }
                }
                catch { /* Perdu */ }
            }
            
            // 3. Mise à jour de l'interface (On vide et on remplit)
            _competitions.Clear();
            
            foreach (var comp in loadedCompetitions)
            {
                // Petit fix pour le nom si manquant
                comp.FixNameFromExtraData();

                // Si on a le nom en mémoire (créé récemment), on l'utilise
                if ((string.IsNullOrEmpty(comp.Nom) || comp.Nom.StartsWith("Compétition #")) 
                    && CompetitionNames.ContainsKey(comp.Id))
                {
                    comp.Nom = CompetitionNames[comp.Id];
                }

                _competitions.Add(comp);
            }

            // Gestion de l'affichage vide/plein via le CollectionView automatique
            CompetitionsCollection.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", "Impossible de charger les compétitions : " + ex.Message, "OK");
        }
        finally
        {
            _isLoading = false;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
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


