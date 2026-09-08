

namespace Vocon.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly IAudioManager _audioManager;
        private readonly WhisperService _service;
        private IAudioRecorder _recorder;
        private string _currentFilePath;
        private IHotKeyService _hotkeyService;
        private readonly TagService _tagService;
        private readonly CommandService _commandService;
        private readonly IMediaControlService _mediaControlService;
        private readonly IBrowserNavigationService _browserNavigationService; 
        private readonly IMicrophoneSettingsService _microphoneSettingsService;
        private readonly IOverlayWindowService _overlayWindowService;

        public ObservableCollection<Note> Notes { get; } = new();

        [ObservableProperty]
        private bool isRecording;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private string statusText = "IDLE";

        [ObservableProperty]
        private bool saveConfirmationVisible;

        [ObservableProperty]
        private string recordButtonText = "Record";
        private readonly INoteRepository _noteRepository;

        public MainPageViewModel(IAudioManager audioManager, WhisperService service,
                          EmbeddingService embeddingService, TagService tagService, IHotKeyService hotkeyService,
                          CommandService commandService, IMediaControlService mediaControlService,
                          IBrowserNavigationService browserNavigationService, 
                          INoteRepository noteRepository, IMicrophoneSettingsService microphoneSettingsService,
                          IOverlayWindowService overlayWindowService)
        {
            _hotkeyService = hotkeyService;
            _audioManager = audioManager;
            _service = service;
            _tagService = tagService;
            _commandService = commandService;
            _mediaControlService = mediaControlService;
            _browserNavigationService = browserNavigationService; 
            _noteRepository = noteRepository;
            _microphoneSettingsService = microphoneSettingsService;
            _overlayWindowService = overlayWindowService;
            _hotkeyService.ChangeState += (newstate) =>
            {
                Task.Run(() => MainThread.BeginInvokeOnMainThread(() => _ = ToggleRecording()));
            };
        }

        bool _isToggling;

        [RelayCommand]
        private async Task ToggleRecording()
        {
            if (_isToggling) return;
            _isToggling = true;
            try
            {
                if (!isRecording)
                    await StartRecording();
                else
                    await StopRecording();
            }
            finally
            {
                _isToggling = false;
            }
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
            _overlayWindowService.Show();
            _overlayWindowService.UpdateState(isRecording: true, isProcessing: false);
            RecordButtonText = "Stop";
        }

        private async Task StopRecording()
        {
            var audioSource = await _recorder.StopAsync();
            isRecording = false;
            IsProcessing = true;
            RecordButtonText = "Record";

            try
            {
                var modelsDir = Path.Combine(FileSystem.AppDataDirectory, "Models");
                Directory.CreateDirectory(modelsDir);

                var fileName = $"recording_{DateTime.UtcNow:yyyyMMdd_HHmmss}.wav";
                _currentFilePath = Path.Combine(modelsDir, fileName);

                using (var sourceStream = audioSource.GetAudioStream())
                using (var fileStream = System.IO.File.Create(_currentFilePath))
                {
                    await sourceStream.CopyToAsync(fileStream);
                }

                var resultText = await _service.TranscribeModel(_currentFilePath);

                var opened = await _browserNavigationService.TryNavigateAsync(resultText);
                if (opened)
                    return;

                var command = _commandService.GetBestTag(resultText);

                if (command != null)
                {
                    switch (command)
                    {
                        case MediaCommand.NextTrack:
                            await _mediaControlService.NextTrack(); break;

                        case MediaCommand.PreviousTrack:
                            await _mediaControlService.PreviousTrack(); break;

                        case MediaCommand.Play:
                            await _mediaControlService.SetPlayState(true); break;

                        case MediaCommand.Pause:
                            await _mediaControlService.SetPlayState(false); break;
                        case MediaCommand.Repeat:
                            await _mediaControlService.Repeat(); break;
                    }
                }
                else
                {
                    var note = new Note
                    {
                        Title = $"{DateTime.Now:dd.MM.yyyy HH:mm}",
                        Transcription = resultText,
                        Date = DateTime.Now,
                        AudioFilePath = _currentFilePath,
                        Tag = _tagService.GetBestTag(resultText)
                    };

                    note.Id = await _noteRepository.SaveNoteAsync(note);
                    MainThread.BeginInvokeOnMainThread(() => Notes.Add(note));

                    _ = ShowSaveConfirmation();
                }
            }
            finally
            {
                IsProcessing = false;
                _overlayWindowService.Hide();
            }
        }

        private async Task ShowSaveConfirmation()
        {
            SaveConfirmationVisible = true;
            await Task.Delay(1800);
            SaveConfirmationVisible = false;
        }

        public async Task LoadNotesAsync()
        {
            var notes = await _noteRepository.GetAllNotesAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Notes.Clear();
                foreach (var note in notes)
                {
                    Notes.Add(note);
                }
            });
        }

        [RelayCommand]
        private async Task DeleteNote(Note note)
        {
            if (note == null) return;

            await _noteRepository.DeleteNoteAsync(note);
            MainThread.BeginInvokeOnMainThread(() => Notes.Remove(note));
        }

        [RelayCommand]
        private async Task EditNote(Note note)
        {
            if (note == null) return;

            note.IsEditing = !note.IsEditing;

            if (!note.IsEditing)
            {
                await _noteRepository.UpdateNoteAsync(note);

                var index = Notes.IndexOf(note);
                if (index >= 0)
                {
                    Notes[index] = note;
                }
            }
        }


    }
}