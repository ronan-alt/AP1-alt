using AP1.Services;
using AP1.Vues;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using AP1.Modeles;

namespace MauiApp1.Vues;

public partial class AccueilGererEquipe : ContentPage
    {
        private readonly Apis Apis = new Apis();
        private readonly ObservableCollection<TeamItem> _teams = new ObservableCollection<TeamItem>();
        private Competition? _competition;

        public AccueilGererEquipe()
        {
            InitializeComponent();
            TeamsCollection.ItemsSource = _teams;
        }

        public AccueilGererEquipe(Competition competition) : this()
        {
            _competition = competition;
            LoadCompetitionIntoForm(competition);
        }

        private void LoadCompetitionIntoForm(Competition c)
        {
           
            CompetitionNameEntry.Text = $"Compétition #{c.Id}";
            StartDatePicker.Date = c.DateDeb == default ? DateTime.Today : c.DateDeb;
            EndDatePicker.Date = c.DateFin == default ? DateTime.Today.AddDays(1) : c.DateFin;
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
            
            await DisplayAlert("Enregistré", "La compétition a été enregistrée.", "OK");
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert("Annuler", "Annuler les modifications en cours ?", "Oui", "Non");
            if (confirm)
            {
                await Navigation.PopAsync();
            }
        }

        private async void OnAddTeamClicked(object sender, EventArgs e)
        {
           
            var name = await DisplayPromptAsync("Nouvelle équipe", "Nom de l'équipe:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                _teams.Add(new TeamItem { Nom = name, Description = "Équipe ajoutée manuellement" });
            }
        }

        private async void OnRemoveTeamClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TeamItem team)
            {
                var confirm = await DisplayAlert("Retirer l'équipe", $"Retirer '{team.Nom}' ?", "Oui", "Non");
                if (confirm)
                {
                    _teams.Remove(team);
                }
            }
        }

        private sealed class TeamItem
        {
            public string Nom { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }