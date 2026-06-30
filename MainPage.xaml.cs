
using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Audio;
using Vocon.Services.EmbeddingServices;
using Vocon.ViewModels;
using Whisper.net;


namespace Vocon
{

    public partial class MainPage : ContentPage
    {
        

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
         
        }
        
    }
}
