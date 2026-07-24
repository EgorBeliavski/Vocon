using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;

namespace Vocon.Models
{
    public partial class Note : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ObservableProperty]
        private string title = string.Empty;

        public string Transcription { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public string AudioFilePath { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;

        [property: Ignore]
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotEditing))]
        private bool isEditing;

        [Ignore]
        public bool IsNotEditing => !IsEditing;
    }
}