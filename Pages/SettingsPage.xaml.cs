using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Audio;
using Vocon.ViewModels;


namespace Vocon.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

    }
}