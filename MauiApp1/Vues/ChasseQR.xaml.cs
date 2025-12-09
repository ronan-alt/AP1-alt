using AP1.Modeles;
using AP1.Services;
using MauiApp1.Modeles;
using MauiApp1.Services;
using MauiApp1.Vues;
namespace AP1.Vues;
using ZXing;                       // <--- CELLE-CI DOIT ÊTRE LÀ (pour BarcodeDetectionEventArgs)
using ZXing.Net.Maui;             // <--- CELLE-CI DOIT ÊTRE LÀ (pour BarcodeReaderOptions)
using ZXing.Net.Maui.Controls;    // <--- CELLE-CI DOIT ÊTRE LÀ (pour les Views)

public partial class ChasseQR : ContentPage
{
    private readonly Apis Apis = new Apis();
    private AjoutPoints ajoutspoint;
    private List<LieuQR> _tousLesLieux; // La base de données de tous les QR possibles
    private List<LieuQR> _parcoursActuel; // Les 5 lieux choisis pour ce joueur
    private int _etapeEnCours = 0; // 0 à 4

    private IDispatcherTimer _timer;
    private int _tempsRestantSec;
    private bool _isProcessing = false; // Pour éviter de scanner 50 fois le même code en 1 seconde

    public BarcodeReaderOptions BarcodeOptions { get; set; }

    public ChasseQR()
    {
        InitializeComponent();

        // Configurer la caméra pour ne lire que les QR Codes (plus rapide)
        BarcodeOptions = new BarcodeReaderOptions
        {
            Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
        cameraView.Options = BarcodeOptions;

        ChargerDonneesFake(); // À remplacer par ta BDD plus tard
        DemarrerNouvellePartie();
    }

    private void ChargerDonneesFake()
    {
        // Ici je simule ta base de données.
        // Il faudra qu'il y en ait beaucoup pour que l'aléatoire fonctionne bien !
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
        GameOverOverlay.IsVisible = false;
        cameraView.IsDetecting = true;
        _etapeEnCours = 0;

        // --- MAGIE DE L'ALÉATOIRE ---
        // On mélange la liste et on en prend 5
        var random = new Random();
        _parcoursActuel = _tousLesLieux.OrderBy(x => random.Next()).Take(5).ToList();

        LancerEtape();
    }

    private void LancerEtape()
    {
        var lieu = _parcoursActuel[_etapeEnCours];

        // Mise à jour UI
        LblEtape.Text = $"Étape {_etapeEnCours + 1} / 5";
        LblIndice.Text = lieu.Indice;
        _tempsRestantSec = lieu.DureeSecondes;
        AfficherTemps();

        _isProcessing = false; // On autorise le scan

        // Lancer Timer
        if (_timer != null) _timer.Stop();
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        _tempsRestantSec--;
        AfficherTemps();

        if (_tempsRestantSec <= 0)
        {
            _timer.Stop();
            GestionDefaite();
        }
    }

    private void AfficherTemps()
    {
        TimeSpan t = TimeSpan.FromSeconds(_tempsRestantSec);
        LblTimer.Text = t.ToString(@"mm\:ss");

        // Petit effet visuel : rouge si moins de 10s
        LblTimer.TextColor = _tempsRestantSec <= 10 ? Colors.Red : Colors.Black;
    }

    // --- C'EST ICI QUE LA CAMÉRA PARLE ---
    private void CameraView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return; // On est déjà en train de traiter un scan

        var premierResultat = e.Results.FirstOrDefault();
        if (premierResultat == null) return;

        string codeScanne = premierResultat.Value;
        string codeAttendu = _parcoursActuel[_etapeEnCours].CodeSecret;

        // IMPORTANT : Revenir sur le Thread Principal pour toucher à l'UI
        Dispatcher.Dispatch(async () =>
        {
            if (codeScanne == codeAttendu)
            {
                _isProcessing = true; // Bloque les autres scans
                _timer.Stop();

                // Feedback succès (Vibration + Message)
                try { HapticFeedback.Perform(HapticFeedbackType.LongPress); } catch { }

                await DisplayAlert("Bravo !", "QR Code trouvé !", "Suivant");

                PasserEtapeSuivante();
            }
            else
            {
                //Feedback erreur (optionnel, attention à ne pas spammer)
                 Console.WriteLine($"Mauvais code : {codeScanne}");
            }
        });
    }

    private async void PasserEtapeSuivante()
    {
        _etapeEnCours++;

        if (_etapeEnCours >= 5)
        {
            // VICTOIRE FINALE
            await DisplayAlert("VICTOIRE !", "Tu as terminé le parcours !", "Réclamer mes points");
            // TODO: Envoyer les points à l'API ici
             
            ajoutspoint = new AjoutPoints(Utilisateur.utilisateur.Id, 50); // 100 points pour avoir fini
            bool BB = await Apis.PostOneAsync("api/elies/mobile/addPoints",ajoutspoint);
            if (BB== true)
            {
                await DisplayAlert("", "Vos points ont bien été ajoutés à votre compte", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Erreur", "Une erreur est survenue lors de l'ajout des points", "OK");
                await Navigation.PopAsync();
            }
             // Retour accueil
        }
    }

    private void GestionDefaite()
    {
        cameraView.IsDetecting = false; // On coupe la caméra
        GameOverOverlay.IsVisible = true;
        try { Vibration.Vibrate(); } catch { }
    }

    private void OnRestartClicked(object sender, EventArgs e)
    {
        // On recommence tout depuis le début
        DemarrerNouvellePartie();
    }
}