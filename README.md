# Vocon


**Vocon** is a Windows desktop application for hands-free voice note-taking and PC control, built with .NET MAUI and fully local speech-to-text inference (Whisper.net). Voice input is automatically organized by semantic category using on-device ML embeddings, and voice commands can control system functions like media playback — no cloud APIs, no subscriptions.

> This project is under active development. Expect breaking changes and incomplete features.

## Why Vocon?

Most voice assistants require cloud connectivity, subscriptions, or are locked to a single ecosystem. Vocon runs entirely on-device:
- **Local transcription** via Whisper.net — no audio leaves your machine
- **Semantic tagging** via ONNX embeddings — notes are categorized by meaning, not keyword matching, and it works across languages out of the box
- **System-level voice control** — control media playback (and eventually more) on any application, simulated at the OS level via `user32.dll`

## Features

### Working
- Voice recording and local transcription via Whisper.net (fully on-device, no internet required)
- Automatic semantic tagging using multilingual sentence embeddings — notes are categorized by meaning, not keywords
- MVVM architecture with full dependency injection — services are cleanly separated and lazily resolved
- Vocon supports multilingual voice input. Currently available in 4 languages:Russian,English,French,German 
- Global hotkey toggle (Alt+Space) to start/stop recording — works system-wide, even when the app isn't focused
- System-level media control — reliable Play/Pause, Next Track, and Previous Track simulation via `SendInput` (with cross-architecture x86/x64 memory alignment).
- Autostart — implemented via a shortcut in the Windows Startup folder (not the registry), so the mechanism stays visible and removable by the user
- Hotkey - finally complete.Finally you can change recorder hotkey.

### In Progress
- Save confirmation feedback
- Recording/processing state indicator
- First-run onboarding

### Planned
- Tray icon / background operation
- Handle missing/disabled microphone
- Distributable packaging
- Model download / setup script

## Tech Stack

| Layer | Technology |
|---|---|
| App framework | .NET MAUI (Windows target) |
| Architecture | MVVM (CommunityToolkit.Mvvm) + Dependency Injection |
| Speech-to-text | [Whisper.net](https://github.com/sandrohanea/whisper.net) (local inference) |
| Semantic embeddings | ONNX Runtime + `paraphrase-multilingual-MiniLM-L12-v2` (quantized) |
| Tokenization | BlingFire (XLM-RoBERTa) — restored automatically via NuGet |
| System control | `user32.dll` (P/Invoke) for media key simulation |
| Audio recording | Plugin.Maui.Audio |
| Global hotkeys | `user32.dll` (P/Invoke) `RegisterHotKey` + `comctl32.dll` window subclassing |
| **System control** | Robust `user32.dll` P/Invoke (`SendInput`, `RegisterHotKey`) with strict x86/x64 memory alignment |
| **OS Media Integration** | Windows `GlobalSystemMediaTransportControlsSessionManager` API |
| Local storage | SQLite via sqlite-net-pcl |

## Architecture Overview

- `EmbeddingService` — tokenizes text, runs ONNX inference, mean-pools and L2-normalizes output into a sentence embedding
- `TagService` — pre-computes embeddings for tag categories at startup, assigns the closest tag to each note via cosine similarity
- `WhisperService` — wraps Whisper.net for local audio transcription
- `HotKeyService` — registers a system-wide hotkey via `RegisterHotKey`, tracks toggle state, and raises an event when triggered
- `MessageHotkeyService` — intercepts window messages via `SetWindowSubclass` to route `WM_HOTKEY` notifications into `HotKeyService`
- `MediaControlService` — handles OS-level media key simulation (Play/Pause, Next/Prev) via `SendInput`
- `CommandService` — classifies transcribed voice commands via cosine similarity against known command phrases
- `NoteRepository` — SQLite-backed persistence layer for notes, with inline editing support
- `AutoStartService` — manages Windows autostart via a shortcut in the Startup folder (not registry, for user transparency)
- `MicroDeviceService` — enumerates and manages available microphone input devices

## Getting Started

### Prerequisites
- Windows 10/11
- .NET 10 SDK
- Visual Studio 2022 with the .NET MAUI workload

### Setup

1. Clone the repo:
   ```bash
   git clone https://github.com/EgorBeliavski/Vocon.git
   ```

2. Restore NuGet packages (BlingFire tokenizer files are included automatically):
   ```bash
   dotnet restore
   ```

3. **Download the models manually** (not included in the repo due to file size):

   | File | Source | Destination |
   |---|---|---|
   | `ggml-base.bin` | [Whisper.net releases](https://huggingface.co/sandrohanea/whisper.net) | `Resources/Models/` |
   | `model_quantized.onnx` | [paraphrase-multilingual-MiniLM-L12-v2 on HuggingFace](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2) | `Resources/Models/` |

4. Open `Vocon.sln` in Visual Studio and build.

## License

TBD
=======
Voice-controlled note-taking and PC management app for Windows. Local speech-to-text via Whisper.net, semantic tagging via ONNX embeddings — no cloud, no subscriptions.

