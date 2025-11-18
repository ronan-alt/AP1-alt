using AP1.Modeles;
using AP1.Services;
using MauiApp1.Vues;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System;

namespace AP1.Vues;



    public partial class AcceuilEleve : ContentPage
{
    private readonly Apis Apis = new Apis();
    

    public AcceuilEleve()
    {
        InitializeComponent();
        
    }
    private async void OnInfosClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AccueilInfoEleve());
    }
    public async void OnClassementClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AcceuilClassementEleve());
    }

}


