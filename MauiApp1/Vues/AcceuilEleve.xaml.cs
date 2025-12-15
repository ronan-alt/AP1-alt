using AP1.Modeles;
using AP1.Services;
// using Intuit.Ipp.Data; <--- ON ENLEVE CA POUR EVITER LES ERREURS
using MauiApp1.Services;
using MauiApp1.Vues;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq; // Nécessaire si tu manipules des listes

namespace AP1.Vues;

public partial class AcceuilEleve : ContentPage
{
    private readonly Apis Apis = new Apis();
    // J'utilise le chemin complet pour User pour éviter les confusions
    private ObservableCollection<AP1.Modeles.User> team;

    public AcceuilEleve()
    {
        InitializeComponent();
        // On n'appelle plus Onload ici, on laisse OnAppearing faire le travail
    }

    // Cette méthode est magique : elle se lance à chaque fois que la page s'affiche
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ChargerUtilisateurs();
    }

    private async System.Threading.Tasks.Task ChargerUtilisateurs()
    {
        try
        {
            var result = await Apis.GetAllAsync<AP1.Modeles.User>("api/mobile/listeUsers");

            // Sécurité : On vérifie si l'API a renvoyé quelque chose
            if (result != null)
            {
               
                team = new ObservableCollection<AP1.Modeles.User>(result);
                PlayersCollection.ItemsSource = team;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Erreur chargement users : " + ex.Message);
            await DisplayAlert("Erreur", "Impossible de charger la liste des élèves.", "OK");
        }
    }

    private async void OnInfosClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AccueilInfoEleve());
    }

    public async void OnClassementClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AcceuilClassementEleve());
    }

    private async void OnJouerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AccueilJeux());
    }
}