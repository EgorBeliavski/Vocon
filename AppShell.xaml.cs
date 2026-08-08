using Vocon;
using Vocon.Pages;

namespace Vocon
{
    public partial class AppShell : Shell
    {
        public AppShell(MainPage mainPage, SettingsPage settingsPage)
        {
            InitializeComponent();

            MainShellContent.Content = mainPage;
            SettingsShellContent.Content = settingsPage;
        }
    }
}