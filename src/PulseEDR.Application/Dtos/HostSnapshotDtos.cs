namespace PulseEDR.Application.Dtos;

/// <summary>
/// Process snapshot returned to the UI.
/// </summary>
public sealed record ProcessInfoDto(
    int Pid,
    string Name,
    string? ExecutablePath,
    string? CommandLine,
    int? ParentPid,
    DateTime StartTime);

/// <summary>
/// Network connection snapshot returned to the UI.
/// </summary>
public sealed record NetworkConnectionDto(
    string LocalAddress,
    int LocalPort,
    string RemoteAddress,
    int RemotePort,
    string Protocol,
    string State,
    int? OwnerPid);

/// <summary>
/// Installed software entry returned to the UI.
/// </summary>
public sealed record InstalledSoftwareDto(
    string Name,
    string Version,
    string? Publisher,
    string? InstallLocation,
    DateTime? InstallDate);

/// <summary>
/// Recent file entry returned to the UI.
/// </summary>
public sealed record RecentFileDto(
    string FullPath,
    string FileName,
    string Extension,
    long SizeBytes,
    DateTime LastModified,
    string? Sha256);