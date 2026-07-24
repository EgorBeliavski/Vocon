using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using Vocon.Models;
using Vocon.Services;
using Vocon.Services.CommandService;
using Vocon.Services.EmbeddingServices;
using Vocon.Services.HotKeyService;
using Vocon.Services.WhisperService;
using Vocon.TagSercices;

namespace Vocon.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly IAudioManager _audioManager;
        private readonly WhisperService _service;
        private IAudioRecorder _recorder;
        private string _currentFilePath;
        private HotKeyService _hotkeyService;
        private readonly TagService _tagService;
        private readonly CommandService _commandService;
        private readonly IMediaControlService _mediaControlService;
        public ObservableCollection<Note> Notes { get; } = new();

        [ObservableProperty]
        private bool isRecording;

        [ObservableProperty]
        private string recordButtonText = "Record";
        private readonly INoteRepository _noteRepository;
        public MainPageViewModel(IAudioManager audioManager, WhisperService service,
                          EmbeddingService embeddingService, TagService tagService, HotKeyService hotkeyService,
                          CommandService commandService,IMediaControlService mediaControlService,INoteRepository noteRepository)
        {
            _hotkeyService = hotkeyService;
            _audioManager = audioManager;
            _service = service;
            _tagService = tagService;
            _commandService = commandService;
            _mediaControlService = mediaControlService;
            _noteRepository = noteRepository;
            _hotkeyService.ChangeState += (newstate) =>{
                MainThread.BeginInvokeOnMainThread(() => ToggleRecording());
            };

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
            var command = _commandService.GetBestTag(resultText);

            if (command!=null)
            {
                switch(command)
                {
                    case MediaCommand.NextTrack:
                        await _mediaControlService.NextTrack(); break;

                    case MediaCommand.PreviousTrack:
                        await _mediaControlService.PreviousTrack(); break;

                    case MediaCommand.Play:
                        await _mediaControlService.SetPlayState(true);break;

                    case MediaCommand.Pause:
                        await _mediaControlService.SetPlayState(false); break;
                    case MediaCommand.Repeat:
                        await _mediaControlService.Repeat(); break;
                }
                     
            }
            else{   
                var note = new Note
                {
                    Title = $"{DateTime.Now:dd.MM.yyyy HH:mm}",
                    Transcription = resultText,
                    Date = DateTime.Now,
                    AudioFilePath = _currentFilePath,
                    Tag = _tagService.GetBestTag(resultText)
                };

                note.Id = await _noteRepository.SaveNoteAsync(note);
                Notes.Add(note);
            }



           
        }
        public async Task LoadNotesAsync()
        {
            var notes = await _noteRepository.GetAllNotesAsync();
            Notes.Clear();
            foreach (var note in notes)
            {
                Notes.Add(note);
            }
        }

        [RelayCommand]
        private async Task DeleteNote(Note note)
        {
            if (note == null) return;

            await _noteRepository.DeleteNoteAsync(note);
            Notes.Remove(note);
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