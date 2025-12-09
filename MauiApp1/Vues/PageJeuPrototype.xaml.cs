using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AP1.Services;
using ZXing.Net.Maui;

namespace AP1.Vues;

public partial class PageJeuPrototype : ContentPage
{
    private readonly Apis Apis = new Apis();

    // DTOs pour la validation
    public class ValidationRequest
    {
        public int Lieu_id { get; set; }
        public string Code_qr { get; set; }
        public int Score { get; set; }
    }

    public class ValidationResponse
    {
        public bool Valide { get; set; }
        public int Points { get; set; }
        public string Message { get; set; }
    }

    // Modèles pour l'API
    public class ApiReponse
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public bool Est_correcte { get; set; }
    }

    public class ApiQuestion
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public List<ApiReponse> Reponses { get; set; }
        public int Lieu_id { get; set; }
    }

    public class ApiLieu
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Image_url { get; set; }
    }

    // Notre classe interne adaptée
    private class Question
    {
        public string Text { get; set; }
        public List<ApiReponse> Answers { get; set; } // On garde l'objet réponse complet
    }

    private List<Question> _questions = new();
    private int _currentIndex = 0;
    private int _score = 0;
    private int _lieuId = 0; // Pour savoir quel lieu on cherche

    public PageJeuPrototype()
    {
        InitializeComponent();
        // On lance le chargement async
        Task.Run(async () => await LoadGameDataAsync());
    }

    private async Task LoadGameDataAsync()
    {
        try
        {
            // 1. Charger les questions depuis l'API
            var apiQuestions = await Apis.GetAllAsync<ApiQuestion>("api/mobile/jeu/questions");
            
            if (apiQuestions != null && apiQuestions.Count > 0)
            {
                _questions.Clear();
                foreach (var q in apiQuestions)
                {
                    // On mélange les réponses pour ne pas que la bonne soit toujours au même endroit
                    var shuffledAnswers = q.Reponses.OrderBy(x => Guid.NewGuid()).ToList();
                    
                    _questions.Add(new Question 
                    { 
                        Text = q.Libelle, 
                        Answers = shuffledAnswers
                    });

                    // On récupère l'ID du lieu de la première question (supposons qu'elles portent sur le même lieu ou un lieu au hasard)
                    if (_lieuId == 0) _lieuId = q.Lieu_id;
                }
            }
            else
            {
                // API a répondu mais liste vide : on charge le fallback
                LoadFallbackQuestions();
            }

            // 2. Charger l'image du lieu si on a un ID
            if (_lieuId > 0)
            {
                var lieu = await Apis.GetSingleAsync<ApiLieu>($"api/mobile/jeu/lieu/{_lieuId}");
                if (lieu != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        TargetImage.Source = lieu.Image_url;
                        MysteryLabel.Text = "Lieu Mystère"; // On cache le nom pour l'instant
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur API Jeu : {ex.Message}");
            // Si erreur, on charge les questions de secours (en dur)
            LoadFallbackQuestions();
        }

        // Si l'API n'a rien renvoyé, on met le fallback
        if (_questions.Count == 0) LoadFallbackQuestions();

        // On lance l'affichage sur le thread principal
        MainThread.BeginInvokeOnMainThread(ShowCurrentQuestion);
    }

    private void LoadFallbackQuestions()
    {
        // Banque de secours si l'API ne marche pas
        _questions = new List<Question>
        {
            new Question { Text = "Capitale de la France ? (Mode Hors Ligne)", Answers = new List<ApiReponse> { 
                new ApiReponse { Libelle = "Paris", Est_correcte = true },
                new ApiReponse { Libelle = "Lyon", Est_correcte = false },
                new ApiReponse { Libelle = "Marseille", Est_correcte = false },
                new ApiReponse { Libelle = "Bordeaux", Est_correcte = false }
            }},
             new Question { Text = "Combien font 2+2 ?", Answers = new List<ApiReponse> { 
                new ApiReponse { Libelle = "4", Est_correcte = true },
                new ApiReponse { Libelle = "5", Est_correcte = false },
                new ApiReponse { Libelle = "3", Est_correcte = false },
                new ApiReponse { Libelle = "0", Est_correcte = false }
            }},
             new Question { Text = "Couleur du ciel ?", Answers = new List<ApiReponse> { 
                new ApiReponse { Libelle = "Bleu", Est_correcte = true },
                new ApiReponse { Libelle = "Vert", Est_correcte = false },
                new ApiReponse { Libelle = "Rouge", Est_correcte = false },
                new ApiReponse { Libelle = "Jaune", Est_correcte = false }
            }},
             new Question { Text = "Animal qui aboie ?", Answers = new List<ApiReponse> { 
                new ApiReponse { Libelle = "Chien", Est_correcte = true },
                new ApiReponse { Libelle = "Chat", Est_correcte = false },
                new ApiReponse { Libelle = "Oiseau", Est_correcte = false },
                new ApiReponse { Libelle = "Poisson", Est_correcte = false }
            }},
             new Question { Text = "Liquide vital ?", Answers = new List<ApiReponse> { 
                new ApiReponse { Libelle = "Eau", Est_correcte = true },
                new ApiReponse { Libelle = "Coca", Est_correcte = false },
                new ApiReponse { Libelle = "Huile", Est_correcte = false },
                new ApiReponse { Libelle = "Sable", Est_correcte = false }
            }}
        };
    }

    private void ShowCurrentQuestion()
    {
        if (_currentIndex >= _questions.Count)
        {
            EndGame();
            return;
        }

        var q = _questions[_currentIndex];
        ProgressLabel.Text = $"Question {_currentIndex + 1}/{_questions.Count}";
        QuestionLabel.Text = q.Text;

        // On met à jour les boutons (attention, l'ordre dépend du XAML)
        var stack = AnswersStack;
        
        // On récupère les boutons (les enfants qui sont des Button)
        var buttons = new List<Button>();
        foreach(var child in stack.Children)
        {
            if (child is Button btn) buttons.Add(btn);
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            if (i < q.Answers.Count)
            {
                buttons[i].Text = q.Answers[i].Libelle;
                buttons[i].IsEnabled = true;
                buttons[i].BackgroundColor = Color.FromArgb("#34495E"); // Reset couleur
                
                // On stocke l'objet réponse dans le bouton pour le retrouver au clic
                buttons[i].BindingContext = q.Answers[i];
            }
            else
            {
                // Si moins de 4 réponses, on cache les boutons en trop
                buttons[i].Text = "";
                buttons[i].IsEnabled = false;
            }
        }
    }

    private async void OnAnswerClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var reponse = (ApiReponse)button.BindingContext;

        if (reponse == null) return;

        bool isCorrect = reponse.Est_correcte;

        if (isCorrect)
        {
            button.BackgroundColor = Colors.Green;
            _score++;
        }
        else
        {
            button.BackgroundColor = Colors.Red;
        }

        // Petite pause pour voir la couleur
        await Task.Delay(500);

        _currentIndex++;
        ShowCurrentQuestion();
    }

    private void EndGame()
    {
        // On cache les questions
        AnswersStack.IsVisible = false;

        // On affiche la section finale (Bouton Scan)
        FinalSection.IsVisible = true;
        SuccessLabel.IsVisible = false; // On cache le bravo pour l'instant
        
        // On calcule la netteté finale en fonction du score
        double maxBlur = 0.95;
        double clarity = (double)_score / _questions.Count;
        BlurOverlay.Opacity = maxBlur * (1 - clarity);
        
        if (_score == _questions.Count)
        {
            MysteryLabel.IsVisible = false;
        }
        else
        {
            MysteryLabel.Text = $"Score : {_score}/{_questions.Count}";
        }
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
         // On affiche l'overlay caméra
         // CameraOverlay.IsVisible = true;
         // BarcodeReader.IsDetecting = true;
         
         // MODE SIMULATION (TEST PC)
         bool confirm = await DisplayAlert("Scanner", "Simuler le scan du QR Code ?", "Oui", "Annuler");
        
         if (confirm)
         {
             SuccessLabel.IsVisible = true;
             SuccessLabel.Text = "Bravo ! Lieu validé (Simulation).\nVous gagnez points !"; 
             ((Button)sender).IsEnabled = false;
         }
    }

    private void OnCancelScanClicked(object sender, EventArgs e)
    {
        // CameraOverlay.IsVisible = false;
        // BarcodeReader.IsDetecting = false;
    }

    private bool _isProcessingScan = false;

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessingScan) return;

        var first = e.Results?.FirstOrDefault();
        if (first is null) return;

        string codeScanned = first.Value;
        if (string.IsNullOrWhiteSpace(codeScanned)) return;

        _isProcessingScan = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // On arrête le scan
            // BarcodeReader.IsDetecting = false;
            // CameraOverlay.IsVisible = false;

            // Appel API pour valider
            var request = new ValidationRequest
            {
                Lieu_id = _lieuId,
                Code_qr = codeScanned,
                Score = _score
            };

            try
            {
                var result = await Apis.PostAsync<ValidationRequest, ValidationResponse>("api/mobile/jeu/valider", request);
                
                if (result != null && result.Valide)
                {
                    // Succès !
                    SuccessLabel.IsVisible = true;
                    SuccessLabel.Text = result.Message + $"\n(+{result.Points} pts)";
                    
                    // On désactive le bouton Scan
                    ((Button)FinalSection.Children.Last()).IsEnabled = false;
                    ((Button)FinalSection.Children.Last()).Text = "QR CODE VALIDÉ";
                    ((Button)FinalSection.Children.Last()).BackgroundColor = Colors.Gray;
                }
                else
                {
                    // Échec
                    await DisplayAlert("Scan", result?.Message ?? "Code invalide.", "Réessayer");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", "Erreur de validation : " + ex.Message, "OK");
            }
            finally
            {
                _isProcessingScan = false;
            }
        });
    }
}
