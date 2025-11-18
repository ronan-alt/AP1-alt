using AP1.Modeles;
using AP1.Services;
using Intuit.Ipp.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;


namespace AP1.Vues;




public partial class AjouterEquipe : ContentPage
{
    private readonly Apis Apis = new Apis(); // ton service API
    private ObservableCollection<Equipe> teams = new();

    public AjouterEquipe()
    {
        InitializeComponent();
    }

    // OnAppearing doit être "override async" et non "async void" tout seul
    protected override async void OnAppearing()
    {
        foreach ( var equipe in teams)
        {

        }
    }

    private async void ConfirmButtonClick(object sender, EventArgs e)
    {
        if (TeamsPicker.SelectedItem is Equipe selectedTeam)
        {
            await DisplayAlert("Confirmation", $"Équipe sélectionnée : {selectedTeam.NomEquipe}", "OK");
            //  ici tu peux ajouter la logique (navigation, enregistrement, etc.)
        }
        else
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner une équipe.", "OK");
        }
    }

    private async void CreateEquipeClick(object sender, EventArgs e)
    {
        string newTeamName = NewTeamEntry.Text?.Trim();

        if (string.IsNullOrEmpty(newTeamName))
        {
            await DisplayAlert("Erreur", "Veuillez entrer un nom pour la nouvelle équipe.", "OK");
            return;
        }

        if (teams.Any(t => t.NomEquipe.Equals(newTeamName, StringComparison.OrdinalIgnoreCase)))
        {
            await DisplayAlert("Erreur", "Cette équipe existe déjà.", "OK");
            return;
        }

        //  Création locale pour l’instant
        var newTeam = new Equipe { NomEquipe = newTeamName };
        teams.Add(newTeam);

        // Rafraîchir la source du Picker
        TeamsPicker.ItemsSource = null;
        TeamsPicker.ItemsSource = teams;
        TeamsPicker.SelectedItem = newTeam;

        await DisplayAlert("Succès", $"Nouvelle équipe '{newTeamName}' créée !", "OK");
        NewTeamEntry.Text = string.Empty;
    }
}

    //public async void OnLoadEquipe(object sender, EventArgs e)
    //{
    //	teams = await Apis.GetAllAsync<Equipe>("api/mobile/getLesEquipes");

    //   }
    //   public async void OnConfirmButtonClick(object sender, EventArgs e)
    //   {
    //       if (TeamsPicker.SelectedItem is Equipe team)
    //       {
    //           await DisplayAlert("Confirmation", $"Équipe sélectionnée : {team.NomEquipe}", "OK");

    //       }

    //   }


    //   public async void OnCreateEquipeClick(object sender, EventArgs e)
    //{
    //       if (TeamsPicker.SelectedItem is Equipe team)
    //       {
    //           await DisplayAlert("Confirmation", $"Équipe sélectionnée : {team.NomEquipe}", "OK");
    //           await Navigation.PushAsync(new LoginPage());
    //       }
    //       else
    //       {
    //           var nomEquipe = NewTeamEntry.Text;

    //       }

    //   }
