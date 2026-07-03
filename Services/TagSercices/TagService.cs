using Vocon.Services.EmbeddingServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vocon.TagSercices
{
    public partial class TagService
    {
        private readonly EmbeddingService _embeddingService;
        private readonly List<string> _tags;
        private List<float[]> _tagEmbeddings = new();

        public TagService(EmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
            _tags = new List<string>
        {
            "music", "work", "health", "travel", "food","hobby","finance","home","family","shopping","ideas",
            "goals","learning","social","other"
        };
        }
        public void Initialize()
        {
            _tagEmbeddings.Clear();
            foreach (var tag in _tags)
            {
                _tagEmbeddings.Add(_embeddingService.GetEmbeddings(tag));
            }
        }
    }
}
