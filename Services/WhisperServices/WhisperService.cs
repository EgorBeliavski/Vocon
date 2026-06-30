using System;
using System.Collections.Generic;
using System.Text;
using Whisper.net;

namespace Vocon.Services.WhisperService
{
    public partial class WhisperService
    {
        private WhisperFactory? _factory;
        private readonly string _factorypath;

        public WhisperService(){
            _factorypath = Path.Combine(FileSystem.AppDataDirectory, "Models", "ggml-base.bin");
        }
       
        private WhisperFactory GetModel(){
            _factory ??= WhisperFactory.FromPath(_factorypath); return _factory;
        }
        
    }
}
