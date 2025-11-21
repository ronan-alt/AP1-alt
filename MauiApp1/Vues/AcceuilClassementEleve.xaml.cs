using AP1.Modeles;
using AP1.Services;
using AP1.Vues;
using MauiApp1.Vues;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;

namespace AP1.Vues;

public partial class AcceuilClassementEleve : ContentPage
{   private ObservableCollection<Equipe> teams;
    private readonly Apis Apis = new Apis();
    public AcceuilClassementEleve()
	{
        InitializeComponent();
        OnAppearing();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var result = await Apis.GetAllAsync<Equipe>("api/elies/mobile/EquipeScore");
        var sortedTeams = result
        .OrderByDescending(t => t.Score)
        .ToList();
        teams = new ObservableCollection<Equipe>(sortedTeams);
        ClassementCollectionView.ItemsSource = teams;
    }
    private async void OnRetourClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AcceuilEleve());
    }
}