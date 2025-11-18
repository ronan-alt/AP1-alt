
using AP1.Modeles;
using AP1.Services;
using MauiApp1.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;

namespace AP1.Vues;

public partial class AccueilInfoEleve : ContentPage
{
    private readonly Apis Apis = new Apis();
	
	

    public AccueilInfoEleve()
	{
		InitializeComponent();
		NomLabel.Text =Utilisateur.utilisateur.Nom;
		PrenomLabel.Text =Utilisateur.utilisateur.Prenom;
		EmailLabel.Text =Utilisateur.utilisateur.Email;
    }

	private async void OnRetourClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new AcceuilEleve());
    }
}