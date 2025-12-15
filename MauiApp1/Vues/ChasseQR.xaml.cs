using AP1.Modeles;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using ZXing;
using MauiApp1.Modeles;

namespace AP1.Vues;

public partial class ChasseQR : ContentPage
{
    private readonly Services.Apis Apis = new Services.Apis(); // Vérifie tes namespaces ici

    private List<LieuQR> _tousLesLieux;
    private List<LieuQR> _parcoursActuel;
    private int _etapeEnCours = 0;

    // Timer système unique
    private IDispatcherTimer _timer;
    private int _tempsRestantSec;
    private bool _isProcessing = false;

    public BarcodeReaderOptions BarcodeOptions { get; set; }

    public ChasseQR()
    {
        InitializeComponent();

        // 1. Initialisation de la caméra
        BarcodeOptions = new BarcodeReaderOptions
        {
            Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
        cameraView.Options = BarcodeOptions;

        // 2. Initialisation UNIQUE du Timer (C'est la correction majeure !)
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;

        ChargerDonneesFake();
        DemarrerNouvellePartie();
    }

    private void ChargerDonneesFake()
    {
        _tousLesLieux = new List<LieuQR>
        {
            new LieuQR { CodeSecret = "CDI_REF_01", Indice = "Le silence est d'or. Cherche près des dictionnaires.", DureeSecondes = 60 },
            new LieuQR { CodeSecret = "VIE_SCO_02", Indice = "Là où on régularise les absences.", DureeSecondes = 45 },
            new LieuQR { CodeSecret = "CAFE_PROF", Indice = "Interdit aux élèves, mais le code est sur la porte.", DureeSecondes = 90 },
            new LieuQR { CodeSecret = "GYMNASE_PANIER", Indice = "Marque un panier à 3 points pour le voir.", DureeSecondes = 120 },
            new LieuQR { CodeSecret = "LABO_SVT", Indice = "Entre les microscopes et les squelettes.", DureeSecondes = 60 },
            new LieuQR { CodeSecret = "CANTINE_EAU", Indice = "J'ai soif ! Trouve la fontaine.", DureeSecondes = 50 },
            new LieuQR { CodeSecret = "PORTAIL", Indice = "Bienvenue au lycée !", DureeSecondes = 30 }
        };
    }

    private void DemarrerNouvellePartie()
    {
        try
        {
            GameOverOverlay.IsVisible = false;
            cameraView.IsDetecting = true;
            _etapeEnCours = 0;

            var random = new Random();
            _parcoursActuel = _tousLesLieux.OrderBy(x => random.Next()).Take(5).ToList();

            LancerEtape();
        }
        catch (Exception ex)
        {
            DisplayAlert("Erreur", "Impossible de démarrer la partie : " + ex.Message, "OK");
        }
    }

    private void LancerEtape()
    {
        try
        {
            // Sécurité : Vérifier qu'on ne sort pas de la liste
            if (_etapeEnCours >= _parcoursActuel.Count) return;

            var lieu = _parcoursActuel[_etapeEnCours];

            // Mise à jour UI
            LblEtape.Text = $"Étape {_etapeEnCours + 1} / 5";
            LblIndice.Text = lieu.Indice;

            // Réinitialiser le temps
            _tempsRestantSec = lieu.DureeSecondes;
            AfficherTemps();

            // On débloque le scan
            _isProcessing = false;
            cameraView.IsDetecting = true;

            // Gestion du Timer (On ne le recrée pas, on le redémarre juste)
            if (_timer.IsRunning) _timer.Stop();
            _timer.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur LancerEtape : {ex.Message}");
            // Si ça plante ici, on débloque quand même le procesing pour ne pas freezer
            _isProcessing = false;
        }
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        _tempsRestantSec--;
        AfficherTemps();

        if (_tempsRestantSec <= 0)
        {
            _timer.Stop(); // On arrête le timer proprement
            GestionDefaite();
        }
    }

    private void AfficherTemps()
    {
        TimeSpan t = TimeSpan.FromSeconds(_tempsRestantSec);
        LblTimer.Text = t.ToString(@"mm\:ss");
        LblTimer.TextColor = _tempsRestantSec <= 10 ? Colors.Red : Colors.Black;
    }

    private void CameraView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;

        var premierResultat = e.Results.FirstOrDefault();
        if (premierResultat == null) return;

        string codeScanne = premierResultat.Value;

        // Sécurité anti-crash
        if (_parcoursActuel == null || _etapeEnCours >= _parcoursActuel.Count) return;
        string codeAttendu = _parcoursActuel[_etapeEnCours].CodeSecret;

        Dispatcher.Dispatch(async () =>
        {
            // On vérifie une 2ème fois ici car le Dispatcher peut avoir du délai
            if (_isProcessing) return;

            if (codeScanne == codeAttendu)
            {
                _isProcessing = true; // On bloque tout de suite
                _timer.Stop();       // On arrête le timer

                try { HapticFeedback.Perform(HapticFeedbackType.LongPress); } catch { }

                await DisplayAlert("Bravo !", "QR Code trouvé !", "Suivant");

                PasserEtapeSuivante();
            }
        });
    }

    private async void PasserEtapeSuivante()
    {
        _etapeEnCours++;

        if (_etapeEnCours >= 5)
        {
            // --- VICTOIRE ---
            bool choix = await DisplayAlert("VICTOIRE !", "Tu as terminé le parcours !", "Réclamer mes points", "Annuler");
            if (!choix) return;

            try
            {
                // Vérifie que la classe Utilisateur existe et est connectée
                // int userId = Utilisateur.utilisateur != null ? Utilisateur.utilisateur.Id : 1; 
                int userId = 1; // ID temporaire pour tester si tu n'as pas de user connecté

                var ajout = new AjoutPoints(userId, 50);

                // Attention à bien retirer "/elies" si ton API ne l'attend pas
                bool succes = await Apis.PostOneAsync("api/elies/mobile/addPoints", ajout);

                if (succes)
                {
                    await DisplayAlert("Succès", "Points ajoutés !", "OK");
                    await Navigation.PushAsync(new AcceuilEleve());
                }
                else
                {
                    await DisplayAlert("Erreur", "Erreur serveur lors de l'ajout.", "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", "Bug technique: " + ex.Message, "OK");
            }
        }
        else
        {
            // --- ETAPE SUIVANTE ---
            LancerEtape();
        }
    }

    private void GestionDefaite()
    {
        cameraView.IsDetecting = false;
        GameOverOverlay.IsVisible = true;
        try { Vibration.Vibrate(); } catch { }
    }

    private void OnRestartClicked(object sender, EventArgs e)
    {
        DemarrerNouvellePartie();
    }
}