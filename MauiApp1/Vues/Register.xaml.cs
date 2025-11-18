using AP1.Modeles;
using AP1.Services;
using Intuit.Ipp.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;

namespace AP1.Vues;

public partial class Register : ContentPage
{
    private readonly Apis Apis = new Apis();
    private ObservableCollection<Equipe> teams = new();
    public Register()
    {
        InitializeComponent();

    }
    protected override async void OnAppearing()

    {
        teams = await Apis.GetAllAsync<Equipe>("api/mobile/getJusteLesEquipes");
        TeamsPicker.ItemsSource = teams;
        TeamsPicker.ItemDisplayBinding = new Binding("NomEquipe");
        // User u1 = new User()
        //foreach ( var equipe in teams)
        //{
        //    foreach (var nomEquipe in equipe.NomEquipe)
        //    {
        //        TeamsPicker.Items.Add(nomEquipe.ToString());
        //    }
        //}
    }
    private async void OnRegisterClicked(object sender, EventArgs e)
    {

        var name = NameEntry.Text;
        var prenom = PrenomEntry.Text;
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;
        var selectedTeam = TeamsPicker.SelectedItem as Equipe;

        if (selectedTeam == null)
        {
            await DisplayAlert("Erreur", "Tu dois choisir une équipe.", "OK");
            return;
        }
        int idEquipe = selectedTeam.Id;
        string nomEquipe = selectedTeam.NomEquipe;
        Equipe equipeChoisie = new Equipe(idEquipe);
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword) || string.IsNullOrWhiteSpace(prenom))
        {
            await DisplayAlert("Erreur", "Tous les champs sont obligatoires.", "OK");
            return;
        }
        if (password.Length < 8)
        {
            await DisplayAlert("Erreur", "Le mot de passe doit contenir au moins 8 caractères.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Erreur", "Les mots de passe ne correspondent pas.", "OK");
            return;
        }
        AP1.Modeles.User u1 = new AP1.Modeles.User(email, password, name, prenom, equipeChoisie);
        bool BB = await Apis.PostOneAsync("api/mobile/registerAvecEquipe", u1);
        if (BB == true)
        {
            await DisplayAlert("", "votre compte a bien été creer", "OK");
            await Navigation.PushAsync(new LoginPage());
        }
        else
        {
            await DisplayAlert("Erreur", "Une erreur est survenue", "OK");
            await Navigation.PushAsync(new Register());
        }

        // Utilisateur.utilisateur = u1(BB.email,BB.,null);
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }


}

