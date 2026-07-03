using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using Vocon.Models;
using Vocon.Services.EmbeddingServices;
using Vocon.Services.WhisperService;
using Vocon.TagSercices;
using System.Collections.ObjectModel;

namespace Vocon.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly IAudioManager _audioManager;
        private readonly WhisperService _service;
        private IAudioRecorder _recorder;
        private string _currentFilePath;

        private readonly EmbeddingService _embeddingService;
        private readonly TagService _tagService;
        public ObservableCollection<Note> Notes { get; } = new();

        [ObservableProperty]
        private bool isRecording;

        [ObservableProperty]
        private string recordButtonText = "Record";

        public MainPageViewModel(IAudioManager audioManager, WhisperService service,
                          EmbeddingService embeddingService, TagService tagService)
        {
            _audioManager = audioManager;
            _service = service;
            _embeddingService = embeddingService;
            _tagService = tagService;
        }

        [RelayCommand]
        private async Task ToggleRecording()
        {
            if (!isRecording)
                await StartRecording();
            else
                await StopRecording();
        }

        private async Task StartRecording()
        {
            _recorder = _audioManager.CreateRecorder();

            await _recorder.StartAsync(new AudioRecorderOptions
            {
                SampleRate = 16000,
                Channels = ChannelType.Mono,
                BitDepth = BitDepth.Pcm16bit
            });

            isRecording = true;
            RecordButtonText = "Stop";
        }

        private async Task StopRecording()
        {
            var audioSource = await _recorder.StopAsync();
            isRecording = false;
            RecordButtonText = "Record";

            var modelsDir = Path.Combine(FileSystem.AppDataDirectory, "Models");
            Directory.CreateDirectory(modelsDir);

            var fileName = $"recording_{DateTime.UtcNow:yyyyMMdd_HHmmss}.wav";
            _currentFilePath = Path.Combine(modelsDir, fileName);

            using (var sourceStream = audioSource.GetAudioStream())
            using (var fileStream = File.Create(_currentFilePath))
            {
                await sourceStream.CopyToAsync(fileStream);
            }

            var resultText = await _service.TranscribeModel(_currentFilePath);

            Notes.Add(new Note
            {
                Title = $"{DateTime.Now:dd.MM.yyyy HH:mm}",
                Transcription = resultText,
                Date = DateTime.Now,
                AudioFilePath = _currentFilePath,
                Tag = _tagService.GetBestTag(resultText)
            });
        }

        
    }
}