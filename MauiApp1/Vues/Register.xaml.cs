using AP1.Modeles;
using AP1.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;

namespace AP1.Vues;

public partial class Register : ContentPage
{
    private readonly Apis Apis = new Apis();
    public Register()
    {
        InitializeComponent();

    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text;
        var prenom = PrenomEntry.Text;
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(name) ||string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword) || string.IsNullOrWhiteSpace(prenom))
        {
            await DisplayAlert("Erreur", "Tous les champs sont obligatoires.", "OK");
            return;
        }
        if (password.Length <8)
        {            
            await DisplayAlert("Erreur", "Le mot de passe doit contenir au moins 8 caractères.", "OK");
            return ;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Erreur", "Les mots de passe ne correspondent pas.", "OK");
            return ;
        }
        User u1 = new User(email, password, name, prenom);
        bool BB = await Apis.PostOneAsync("api/mobile/register", u1);
        if (BB == true)
        {
            await DisplayAlert("", "votre compte a bien été creer", "OK");
            await Navigation.PushAsync(new AjouterEquipe());
        }
        else
        {
            await DisplayAlert("Erreur", "Une erreur est survenue", "OK");
            await Navigation.PushAsync(new Register());
        }
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }


}

