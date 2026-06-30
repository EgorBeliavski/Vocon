# Vocon
<<<<<<< HEAD

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
- Language selection for transcription is not yet exposed in the UI — English is used by default for now.

### In Progress
- Voice-controlled media playback (play/pause/skip via simulated OS-level media keys — works with any media player)
- Voice command pipeline: hold a global hotkey to record, release to transcribe, then classify the result as a command or a note
- Automatic semantic tagging using multilingual sentence embeddings (cosine similarity against predefined tag categories)
- Multilingual support for both transcription and tagging out of the box
- Two-panel UI: note list + recording controls

### Planned
- Wake word detection (always-on listening via Porcupine) as an alternative to the hotkey
- Terminal-style slash-command interface
- Expanded system control beyond media

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

## Architecture Overview

- `EmbeddingService` — tokenizes text, runs ONNX inference, mean-pools and L2-normalizes output into a sentence embedding
- `TagService` — pre-computes embeddings for tag categories at startup, assigns the closest tag to each note via cosine similarity
- `WhisperService` — wraps Whisper.net for local audio transcription
- Services are registered as singletons via DI and eagerly initialized at app startup

## Getting Started

### Prerequisites
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 with the .NET MAUI workload

### Setup

1. Clone the repo:
   ```bash
   git clone https://github.com/<your-username>/Vocon.git
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
>>>>>>> e4c3ffdab5a3cf85ee8140841d41773532cfa9d0
