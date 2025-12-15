using System;
using AP1.Modeles;
using AP1.Services;

namespace MauiApp1.Vues;

public partial class AccueilCreeCompetition : ContentPage
{
    private readonly Apis _apis = new();

    public AccueilCreeCompetition()
    {
        InitializeComponent();
        StartDatePicker.Date = DateTime.Today;
        EndDatePicker.Date = DateTime.Today.AddDays(7);
        StatusPicker.SelectedIndex = 0;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnGoHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        if (!ValidatePayload(out var message))
        {
            await DisplayAlert("Formulaire incomplet", message, "OK");
            return;
        }

        var payload = new CompetitionUpsertRequest
        {
            Nom = CompetitionNameEntry.Text!.Trim(),
            DateDeb = StartDatePicker.Date.ToString("yyyy-MM-dd"),
            DateFin = EndDatePicker.Date.ToString("yyyy-MM-dd")
        };

        SetLoading(true);

        try
        {
            // DEBUG : ON VERIFIE ENCORE CE QU'ON ENVOIE
            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.Indented);
            await DisplayAlert("VERIFICATION JSON", jsonPayload, "ENVOYER");

            var response = await _apis.PostAsync<CompetitionUpsertRequest, CompetitionCreationResponse>("api/mobile/competitions", payload);
            
            if (response is null || !response.Success || response.Competition is null)
            {
                throw new InvalidOperationException("Échec de la création (réponse invalide).");
            }

            var created = response.Competition;

            if (string.IsNullOrWhiteSpace(created.Nom))
            {
                 created.Nom = CompetitionNameEntry.Text!.Trim();
            }

            MessagingCenter.Send(this, "CompetitionCreated", created);
            
            await DisplayAlert("Succès", $"La compétition \"{created.Nom}\" a été créée.", "OK");
            await Navigation.PopToRootAsync();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur lors de la création: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Annuler", "Annuler la création ?", "Oui", "Non");
        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }

    private bool ValidatePayload(out string message)
    {
        var name = CompetitionNameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            message = "Veuillez saisir un nom de compétition.";
            return false;
        }

        if (EndDatePicker.Date <= StartDatePicker.Date)
        {
            message = "La date de fin doit être postérieure à la date de début.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        CreateButton.IsEnabled = !isLoading;
        CancelButton.IsEnabled = !isLoading;
        if (isLoading)
        {
            ErrorLabel.IsVisible = false;
            ErrorLabel.Text = string.Empty;
        }
    }
}
