
namespace Vocon.ViewModels
{
    public partial class OverlayViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isRecording;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private string statusText = "";
    }
}