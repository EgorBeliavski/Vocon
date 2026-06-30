using System;
using System.Collections.Generic;
using System.Text;

namespace Vocon.Services.WhisperService
{
    public partial class WhisperService
    {
        public async Task<string> TranscribeModel(string audiofile, string language = "ru")
        {
            if (!File.Exists(audiofile))
            {
                throw new Exception("File not exist");
            }
            var factory = GetModel();

            var processor = factory.CreateBuilder().WithLanguage(language).Build();
            var resultstring = new StringBuilder();
            using var fileStream = File.OpenRead(audiofile);
            await foreach (var frame in processor.ProcessAsync(fileStream))
            {

                resultstring.Append(frame.Text);
            }

            return resultstring.ToString().Trim();
        }
    }
}
