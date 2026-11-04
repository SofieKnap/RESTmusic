using System.ComponentModel.DataAnnotations;

namespace RESTmusic.Models
{
    public class Collection
    {
        // Entity Framework bruger automatisk Id som primærnøgle.
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Artist { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int Duration { get; set; }
        [Range(1, 9999)]
        public int PublicationYear { get; set; } 

        public override string ToString()
        {
            return $"{Title} - {Artist} ({PublicationYear})";
        }


    }
}
