using AP1.Modeles;
using AP1.Services;
using Intuit.Ipp.Data;
using MauiApp1.Services;
using MauiApp1.Vues;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;

namespace AP1.Vues;



    public partial class AcceuilEleve : ContentPage
{
    private readonly Apis Apis = new Apis();
    private ObservableCollection<AP1.Modeles.User> team;

    public AcceuilEleve()
    {
        InitializeComponent();
        Onload();
    }
    private async void Onload()
    {
        var result = await Apis.GetAllAsync<AP1.Modeles.User>("api/mobile/listeUsers");
        result.ToList();
        team = new ObservableCollection<Modeles.User>(result);
        PlayersCollection.ItemsSource = team;
    }
    private async void OnInfosClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AccueilInfoEleve());
    }
    public async void OnClassementClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AcceuilClassementEleve());
    }
    private async void OnJouerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AccueilJeux());

    }

}


