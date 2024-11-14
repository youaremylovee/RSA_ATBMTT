namespace Website.Utils
{
    public class FileUtils
    {
        public static string GetFileType(string FileName)
        {
            var ext =  FileName.Substring(FileName.LastIndexOf('.') + 1);
            return ext.Trim().ToLower() switch
            {
                "pdf" => "pdf",
                "zip" => "archive",
                "rar" => "archive",
                "jpg" => "image",
                "jpeg" => "image",
                "png" => "image",
                "docx" => "word",
                "doc" => "word",
                "txt" => "word",
                "mkv" => "video",
                "mp4" => "video",
                _ => "o"
            };
        }
        public static string GetFileSize(long bytes)
        {
            string fileSize = string.Empty;
            int index = 0;
            string[] strings = { "B", "KB", "MB", "GB", "TB" };
            while(bytes > 1024)
            {
                bytes /= 1024;
                index++;
            }
            fileSize = bytes.ToString() + " " + strings[index];
            return fileSize;
        }
    }
}
