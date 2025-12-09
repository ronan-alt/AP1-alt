using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AP1.Modeles;
using AP1.Services;
using AP1.Vues;

namespace MauiApp1.Vues;

public partial class AccueilGererEquipe : ContentPage
{
    private readonly Apis Apis = new Apis();
    private readonly ObservableCollection<TeamItem> _teams = new();
    private Competition? _competition;

    public AccueilGererEquipe()
    {
        InitializeComponent();
        TeamsCollection.ItemsSource = _teams;
    }

    public AccueilGererEquipe(Competition competition) : this()
    {
        if (competition != null && competition.Id > 0)
        {
            _competition = competition;
            
           
            if (AP1.Vues.AccueilProfesseur.CompetitionNames.ContainsKey(competition.Id))
            {
                _competition.Nom = AP1.Vues.AccueilProfesseur.CompetitionNames[competition.Id];
            }
            
            LoadCompetitionIntoForm(_competition);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_competition is not null)
        {
            await RefreshCompetitionAsync(_competition.Id);
        }
    }

    private async Task RefreshCompetitionAsync(int competitionId)
    {
        try
        {
            SetLoading(true);
            ErrorLabel.IsVisible = false;
            var response = await Apis.GetSingleAsync<CompetitionTeamsResponse>($"api/mobile/competitions/{competitionId}/teams");
            if (response?.Competition is not null)
            {
                response.Competition.FixNameFromExtraData();
                
                
                if (AP1.Vues.AccueilProfesseur.CompetitionNames.ContainsKey(response.Competition.Id))
                {
                    response.Competition.Nom = AP1.Vues.AccueilProfesseur.CompetitionNames[response.Competition.Id];
                }
                
                _competition = response.Competition;
                
                 if (_competition.Id <= 0)
                {
                    _competition.Id = competitionId;
                }

                LoadCompetitionIntoForm(_competition);
            }

            _teams.Clear();
            foreach (var team in response?.Teams ?? Enumerable.Empty<Equipe>())
            {
                _teams.Add(new TeamItem
                {
                    Id = team.Id,
                    Nom = string.IsNullOrWhiteSpace(team.NomEquipe) ? $"Équipe #{team.Id}" : team.NomEquipe,
                    Description = $"{team.Nbplaces} places – {team.LesUsers?.Count ?? 0} membres"
                });
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Chargement impossible: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void LoadCompetitionIntoForm(Competition competition)
    {
    
        string displayName = competition.Nom;
        if (string.IsNullOrWhiteSpace(displayName) && AP1.Vues.AccueilProfesseur.CompetitionNames.ContainsKey(competition.Id))
        {
            displayName = AP1.Vues.AccueilProfesseur.CompetitionNames[competition.Id];
        }
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = $"Compétition #{competition.Id}";
        }

        CompetitionNameEntry.Text = displayName;
        StartDatePicker.Date = competition.DateDeb == default ? DateTime.Today : competition.DateDeb;
        EndDatePicker.Date = competition.DateFin == default ? DateTime.Today.AddDays(1) : competition.DateFin;
        StatusPicker.SelectedIndex = 0;
        RegistrationSwitch.IsToggled = false;
    }

    private async void OnRetourClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnGoHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_competition is null || _competition.Id <= 0)
        {
            await DisplayAlert("Erreur", "Aucune compétition valide chargée. L'ID est manquant ou invalide.", "OK");
            return;
        }

        if (EndDatePicker.Date <= StartDatePicker.Date)
        {
            await DisplayAlert("Validation", "La date de fin doit être postérieure au début.", "OK");
            return;
        }

        var payload = new CompetitionUpsertRequest
        {
            Nom = string.IsNullOrWhiteSpace(CompetitionNameEntry.Text) ? $"Compétition #{_competition.Id}" : CompetitionNameEntry.Text.Trim(),
            DateDeb = StartDatePicker.Date,
            DateFin = EndDatePicker.Date
        };

        try
        {
            SetLoading(true);
            var updated = await Apis.PutAsync<CompetitionUpsertRequest, Competition>($"api/mobile/competitions/{_competition.Id}", payload)
                          ?? throw new InvalidOperationException("Réponse vide lors de la mise à jour.");
            _competition = updated;
            
            // On stocke le nom localement car l'API ne le renvoie pas
            AP1.Vues.AccueilProfesseur.CompetitionNames[_competition.Id] = payload.Nom;
            updated.Nom = payload.Nom; // Force l'affichage
            
            LoadCompetitionIntoForm(updated);
            await DisplayAlert("Enregistré", "La compétition a été mise à jour.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible d'enregistrer : {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Annuler", "Annuler les modifications ?", "Oui", "Non");
        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnAddTeamClicked(object sender, EventArgs e)
    {
        if (_competition is null)
        {
            await DisplayAlert("Sélection requise", "Aucune compétition chargée.", "OK");
            return;
        }

        var teamIdText = await DisplayPromptAsync("Associer une équipe", "Identifiant de l'équipe :");
        if (!int.TryParse(teamIdText, out var teamId) || teamId <= 0)
        {
            await DisplayAlert("Validation", "Merci de saisir un identifiant numérique.", "OK");
            return;
        }

        try
        {
            SetLoading(true);
            await Apis.PostAsync<TeamLinkRequest, CompetitionTeamsResponse>(
                $"api/mobile/competitions/{_competition.Id}/teams",
                new TeamLinkRequest { TeamId = teamId });
            await RefreshCompetitionAsync(_competition.Id);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Association impossible : {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnRemoveTeamClicked(object sender, EventArgs e)
    {
        if (_competition is null) return;

        if (sender is Button button && button.BindingContext is TeamItem team)
        {
            var confirm = await DisplayAlert("Retirer l'équipe", $"Retirer '{team.Nom}' ?", "Oui", "Non");
            if (!confirm) return;

            try
            {
                SetLoading(true);
                await Apis.DeleteAsync($"api/mobile/competitions/{_competition.Id}/teams/{team.Id}");
                _teams.Remove(team);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Suppression impossible : {ex.Message}", "OK");
            }
            finally
            {
                SetLoading(false);
            }
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SaveButton.IsEnabled = !isLoading;
        CancelButton.IsEnabled = !isLoading;
    }

    private sealed class TeamItem
    {
        public int Id { get; init; }
        public string Nom { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}
