

namespace Vocon.Services.WhisperService
{
    public partial class WhisperService
    {

        
        public async Task<string> TranscribeModel(string audiofile)
        {
            var language = _settingsService.SelectedLanguageCode;

            
            if (!System.IO.File.Exists(audiofile))
            {
                throw new Exception("File not exist");
            }
            var factory = GetModel();
            using var processor = factory.CreateBuilder()
                .WithLanguage(language)
                .Build();
            var resultstring = new StringBuilder();
            using var fileStream = System.IO.File.OpenRead(audiofile);
            await foreach (var frame in processor.ProcessAsync(fileStream))
            {

                resultstring.Append(frame.Text);
            }

            return resultstring.ToString().Trim();
        }
    }
}
