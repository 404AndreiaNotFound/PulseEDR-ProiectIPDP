using PulseEDR.Domain.Common;

namespace PulseEDR.Domain.Entities;

/// <summary>
/// A file recently added to known risky locations (Downloads, Desktop).
/// Risky extensions (.exe, .ps1, .bat, .cmd, .js, .vbs, .scr) are flagged.
/// </summary>
public class RecentFile : Entity
{
    public Guid ScanResultId { get; private set; }
    public string FullPath { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string Extension { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public DateTime LastModified { get; private set; }
    public string? Sha256 { get; private set; }

    private RecentFile() { }

    public RecentFile(string fullPath, string fileName, string extension,
                      long sizeBytes, DateTime lastModified, string? sha256)
    {
        FullPath = fullPath;
        FileName = fileName;
        Extension = extension;
        SizeBytes = sizeBytes;
        LastModified = lastModified;
        Sha256 = sha256;
    }
}