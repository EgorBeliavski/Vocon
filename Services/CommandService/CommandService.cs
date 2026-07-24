using System;
using System.Collections.Generic;
using System.Text;
using Vocon.Services.EmbeddingServices;

namespace Vocon.Services.CommandService
{
    public enum MediaCommand{
        NextTrack, 
        PreviousTrack, 
        Play,
        Pause,
        Repeat
    }
    public class CommandService
    {
        private readonly EmbeddingService _embeddingService;
        private readonly List<(string Phrase, MediaCommand Command)> _commands = new List<(string Phrase, MediaCommand Command)>
        {
            // NextTrack
            ("next track", MediaCommand.NextTrack),
            ("next song", MediaCommand.NextTrack),
            ("skip", MediaCommand.NextTrack),
            ("lets go", MediaCommand.NextTrack),
           

            // PreviousTrack
            ("previous track", MediaCommand.PreviousTrack),
            ("previous song", MediaCommand.PreviousTrack),
            ("go back", MediaCommand.PreviousTrack),
            

            // Play
            ("play", MediaCommand.Play),
            ("play music", MediaCommand.Play),
            

            // Pause
            ("pause", MediaCommand.Pause),
            ("stop", MediaCommand.Pause),

            //Repeat
            ("repeat", MediaCommand.Repeat),
            ("repeat song", MediaCommand.Repeat),
            ("repeat track", MediaCommand.Repeat),
            ("loop", MediaCommand.Repeat)

        };
        private List<float[]> _tagEmbeddings = new();


        public CommandService(EmbeddingService embeddingService){
            _embeddingService = embeddingService;
        }
        public MediaCommand? GetBestTag(string text)
        {
            if (_tagEmbeddings.Count == 0)
                throw new InvalidOperationException("CommandService not initialized");
            float[] textEmbedding = _embeddingService.GetEmbeddings(text);

            float bestScore = float.MinValue;
            int bestIndex = 0;

            for (int i = 0; i < _tagEmbeddings.Count; i++)
            {
                float score = CosineSimilarity(textEmbedding, _tagEmbeddings[i]);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }
            const float threshold = 0.6f;
            if (bestScore < threshold)
                return null;
            return _commands[bestIndex].Command;
        }

        private float CosineSimilarity(float[] a, float[] b)
        {
            float sum = 0f;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }
        public void Initialize()
        {
            _tagEmbeddings.Clear();
            foreach (var entry in _commands)
            {
                _tagEmbeddings.Add(_embeddingService.GetEmbeddings(entry.Phrase));
            }
        }
    }
}
