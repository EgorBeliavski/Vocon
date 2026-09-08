
namespace Vocon.Pages
{
    public partial class OverlayPage : ContentPage
    {
        public OverlayPage(OverlayViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}