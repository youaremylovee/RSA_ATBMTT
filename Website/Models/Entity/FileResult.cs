namespace Website.Models.Entity
{
    public class FileResult
    {
        public string FileName { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string SignType { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        public string ActionTask { get; set; } = string.Empty;
    }
}
