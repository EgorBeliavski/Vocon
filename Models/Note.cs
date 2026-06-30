using System;
using System.Collections.Generic;
using System.Text;

namespace Vocon.Models
{
    public class Note
    {
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Transcription { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public string AudioFilePath { get; set; } = string.Empty;

        //public List<string> Tags { get; set; } = new List<string>();
        public string Tag { get; set; } = string.Empty;


    }
}
