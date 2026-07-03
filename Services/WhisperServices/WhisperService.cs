using System;
using System.Collections.Generic;
using System.Text;
using Vocon.Services.SettingLanguageService;
using Whisper.net;

namespace Vocon.Services.WhisperService
{
    public partial class WhisperService
    {
        private WhisperFactory? _factory;
        private readonly string _factorypath;
        private readonly ISettingLanguageService _settingsService;
        public WhisperService(ISettingLanguageService settingsService)
        {
            _factorypath = Path.Combine(FileSystem.AppDataDirectory, "Models", "ggml-base.bin");
            _settingsService = settingsService;
        }
       
        private WhisperFactory GetModel(){
            if (!File.Exists(_factorypath))
                throw new FileNotFoundException($"Model not found at: {_factorypath}");
            _factory ??= WhisperFactory.FromPath(_factorypath); return _factory;
        }
        
    }
}
