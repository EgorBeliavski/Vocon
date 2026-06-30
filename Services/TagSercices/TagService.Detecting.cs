using Vocon.Services.EmbeddingServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vocon.TagSercices
{
    public partial class TagService
    {
        

        public string GetBestTag(string text)
        {
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

            return _tags[bestIndex];
        }

        private float CosineSimilarity(float[] a, float[] b)
        {
            float sum = 0f;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }
    }
}
