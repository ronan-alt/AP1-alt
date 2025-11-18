using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;
using AP1.Modeles;

namespace MauiApp1.Vues;

public partial class AccueilCreeCompetition : ContentPage
{
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
       
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = string.Empty;
        try
        {
            await Task.Delay(300); 
            var comp = new Competition
            {
                Id = new Random().Next(1000, 9999),
                Nom = string.IsNullOrWhiteSpace(CompetitionNameEntry.Text) ? "Nouvelle compétition" : CompetitionNameEntry.Text.Trim(),
                DateDeb = StartDatePicker.Date,
                DateFin = EndDatePicker.Date
            };
            MessagingCenter.Send(this, "CompetitionCreated", comp);

            
            await DisplayAlert("Créée", $"\"{comp.Nom}\" a été créée.", "OK");

           
            await Navigation.PopToRootAsync();
        }
        catch (Exception ex)
        {
          
            ErrorLabel.Text = $"Erreur lors de la création: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
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
}