using AP1.Modeles;
using AP1.Services;
using MauiApp1.Services;
using MauiApp1.Vues;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AP1.Vues;

public partial class AccueilJeux : ContentPage
{
    private AP1.Modeles.Jeu _jeuEnCours;
    private readonly Apis Apis = new Apis();
    private Jeu prochainJeu = new Jeu();
    // Timer système pour le compte à rebours
    private IDispatcherTimer _timer;
    private TimeSpan _tempsRestant;

    public AccueilJeux()
    {
        InitializeComponent();
        InitialiserTimerDepuisBDD();
    }

    
    private async Task InitialiserTimerDepuisBDD()
    {
        try
        {
            // 1. Récupération de la liste
            var result = await Apis.GetOneAsync<AP1.Modeles.Jeu>("api/mobile/nextEpreuve", prochainJeu);

            // --- SECURITE 1 : On vérifie que la liste n'est pas vide avant de prendre le [0] ---
            if (result == null)
            {
                LblTimer.Text = "Pas de jeu prévu.";
                return; // On arrête tout pour ne pas planter
            }

            // On prend le premier élément en toute sécurité
            _jeuEnCours = result;
            DateTime dateProchainJeu = _jeuEnCours.DateDebut;
            DateTime dateActuelle = DateTime.Now.AddHours(+1);

            // 2. Calcul direct (pas besoin de convertir en double et revenir en TimeSpan)
            _tempsRestant = dateProchainJeu - dateActuelle;

            // --- SECURITE 2 : Si le jeu a déjà commencé (temps négatif) ---
            if (_tempsRestant.TotalSeconds <= 0)
            {
                _tempsRestant = TimeSpan.Zero;
                MettreAJourAffichageTimer();
                ActiverBoutonJeu(); // On active direct le bouton !
                return; // Pas besoin de lancer le timer
            }

            // 3. Tout est bon, on lance le compte à rebours
            MettreAJourAffichageTimer();

            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }
        catch (Exception ex)
        {
            // On affiche l'erreur technique pour comprendre
            LblTimer.Text = ex.Message;
            LblTimer.FontSize = 12; // On réduit la police pour que ça rentre
        }
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        if (_tempsRestant.TotalSeconds > 0)
        {
            _tempsRestant = _tempsRestant.Subtract(TimeSpan.FromSeconds(1));
            MettreAJourAffichageTimer();
        }
        else
        {
            // Le temps est écoulé !
            _timer.Stop();
            ActiverBoutonJeu();
        }
    }

    private void MettreAJourAffichageTimer()
    {
        if (_tempsRestant.Days > 0)
        {
            // Affiche : 1j 04:15:30
            LblTimer.Text = _tempsRestant.ToString(@"d\j\ hh\:mm\:ss");
        }
        else
        {
            // Affiche : 04:15:30
            LblTimer.Text = _tempsRestant.ToString(@"hh\:mm\:ss");
        }
    }

    private void ActiverBoutonJeu()
    {
        // On active le bouton et on le rend totalement opaque
        BtnAccederJeu.IsEnabled = true;
        BtnAccederJeu.Opacity = 1;
    }
    // --- ION DES CLICS ---

    private async void OnAccederJeuClicked(object sender, EventArgs e)
    {
        // Sécurité : Si le jeu n'est pas chargé
        if (_jeuEnCours == null)
        {
            await DisplayAlert("Erreur", "Aucune information sur le jeu.", "OK");
            return;
        }

        // On regarde l'ID du jeu reçu de la base de données
        switch (_jeuEnCours.Id)
        {
            case 1:
                // Si l'ID est 1, on va vers la page du 1, 2, 3 Soleil
                // On peut passer l'objet _jeuEnCours en paramètre si besoin
                //await Navigation.PushAsync(new AP1.Vues.JEU1());
                break;

            case 2:
                // Exemple pour un futur jeu
                // await Navigation.PushAsync(new AP1.Vues.PageJeu2());
               // await DisplayAlert("Bientôt", "Ce jeu n'est pas encore codé !", "OK");
                break;

            default:
                // Si l'ID est inconnu
                //await DisplayAlert("Oups", $"Le jeu ID {_jeuEnCours.Id} n'est pas reconnu.", "OK");
                break;
        }
    }
    

    private async void OnClassementClicked(object sender, EventArgs e)
    {
        // Navigation vers le classement
        await Navigation.PushAsync(new AcceuilClassementEleve());

    }

    private async void OnRetourClicked(object sender, EventArgs e)
    {
        // Retour à la page précédente
        await Navigation.PopAsync();
    }
}