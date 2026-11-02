namespace RESTmusic.Models
{
    public class Collection
    {
       
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int PublicationYear { get; set; } 

        public override string ToString()
        {
            return $"{Title} - {Artist} ({PublicationYear})";
        }


    }
}
