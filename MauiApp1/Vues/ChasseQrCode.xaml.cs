using Microsoft.Maui.Controls;
using System.Net.NetworkInformation;
using ZXing.Net.Maui;
using MauiApp1.Modeles;

namespace MauiApp1.Vues;

public partial class ChasseQrCode : ContentPage
{
    private int _step = 0;
    private int _score = 0;
    private readonly string[] _blurredImages = new[]
    {
        "blur1.png",
        "blur2.png",
        "blur3.png",
        "blur4.png"
    };


    public ChasseQrCode()
    {
        InitializeComponent();
        LoadStep();
    }


    private void LoadStep()
    {
        if (_step < _blurredImages.Length)
        {
            Modeles.Image.Source = _blurredImages[_step];
            ScoreLabel.Text = $"Score: {_score}";
        }
        else
        {
            ScoreLabel.Text = $"Score final: {_score}";
            BlurredImage.Source = null;
        }
    }


    private async void OnScanClicked(object sender, EventArgs e)
    {
        var scanner = new ZXing.Net.Maui.Controls.CameraBarcodeReaderModal();
        var result = await scanner.Read();


        if (result != null)
        {
            // Check QR validity
            if (result.Value == $"QR{_step + 1}")
            {
                _step++;


                if (_step == 4)
                {
                    _score += 1000;
                }


                LoadStep();
            }
        }
    }
}