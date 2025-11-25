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
            var result = await Apis.GetAllAsync<AP1.Modeles.Jeu>("api/mobile/nextEpreuve");

            // --- SECURITE 1 : On vérifie que la liste n'est pas vide avant de prendre le [0] ---
            if (result == null || result.Count == 0)
            {
                LblTimer.Text = "Pas de jeu prévu.";
                return; // On arrête tout pour ne pas planter
            }

            // On prend le premier élément en toute sécurité
            prochainJeu = result[0];

            DateTime dateProchainJeu = prochainJeu.DateDebut;
            DateTime dateActuelle = DateTime.Now;

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
        // Format hh:mm:ss
        LblTimer.Text = _tempsRestant.ToString(@"hh\:mm\:ss");
    }

    private void ActiverBoutonJeu()
    {
        // On active le bouton et on le rend totalement opaque
        BtnAccederJeu.IsEnabled = true;
        BtnAccederJeu.Opacity = 1;
        BtnAccederJeu.Text = "?? C'est parti !"; // Petit effet sympa
    }

    // --- GESTION DES CLICS ---

    private async void OnAccederJeuClicked(object sender, EventArgs e)
    {
        // Navigation vers la page du jeu (à créer plus tard)
        // await Navigation.PushAsync(new PageDuJeu()); 
        await DisplayAlert("Navigation", "On va vers le jeu !", "OK");
    }

    private async void OnClassementClicked(object sender, EventArgs e)
    {
        // Navigation vers le classement
        await DisplayAlert("Classement", "Affichage du classement...", "OK");
        await Navigation.PushAsync(new AcceuilClassementEleve());

    }

    private async void OnRetourClicked(object sender, EventArgs e)
    {
        // Retour à la page précédente
        await Navigation.PopAsync();
    }
}