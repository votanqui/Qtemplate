namespace Qtemplate.Application.DTOs.Stats;

public class MediaStatsDto
{
    public int TotalFiles { get; set; }
    public long TotalSize { get; set; }          // Bytes
    public string TotalSizeFormatted { get; set; } = string.Empty; // "12.4 MB"
    public List<MediaByFolderDto> ByStorage { get; set; } = new();
    public List<MediaByTypeDto> ByType { get; set; } = new();
}

public class MediaByFolderDto
{
    public string StorageType { get; set; } = string.Empty;  // ← đổi tên field
    public int Count { get; set; }
    public long TotalSize { get; set; }
    public string TotalSizeFormatted { get; set; } = string.Empty;
}
public class MediaByTypeDto
{
    public string MimeType { get; set; } = string.Empty;
    public int Count { get; set; }
    public long TotalSize { get; set; }
}