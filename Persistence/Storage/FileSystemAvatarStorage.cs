using System;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Guards;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Storage;

public sealed class FileSystemAvatarStorage : IAvatarStorage
{
    private const string AvatarFileNamePattern = @"^[0-9a-f]+\.(png|jpg)$";
    private const string GuidFileNameFormat = "N";
    private const int RegexTimeoutMilliseconds = 100;
    private const int AvatarFileNameLength = 36;

    private static readonly ILog _logger;
    private static readonly Regex _avatarFileNameRegex;

    private readonly string _avatarFolderPath;

    static FileSystemAvatarStorage()
    {
        _logger = LogManager.GetLogger(typeof(FileSystemAvatarStorage));
        TimeSpan matchTimeout = TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds);
        _avatarFileNameRegex = new Regex(AvatarFileNamePattern, RegexOptions.CultureInvariant, matchTimeout);
    }

    public FileSystemAvatarStorage(string avatarFolderPath)
    {
        ArgumentGuard.ThrowIfEmpty(avatarFolderPath, nameof(avatarFolderPath), _logger);
        _avatarFolderPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, avatarFolderPath));
    }

    public string SaveAvatarImage(byte[] imageBytes, string fileExtension)
    {
        ArgumentGuard.ThrowIfNull(imageBytes, nameof(imageBytes), _logger);
        string newFileName = Guid.NewGuid().ToString(GuidFileNameFormat) + fileExtension;
        string avatarFilePath = BuildAvatarFilePath(newFileName);
        string avatarFileName = RunFileOperation(
            () => WriteAvatarFile(avatarFilePath, imageBytes),
            "The avatar image could not be saved.");

        return avatarFileName;
    }

    public byte[] LoadAvatarImage(string avatarFileName)
    {
        string avatarFilePath = BuildAvatarFilePath(avatarFileName);
        byte[] avatarImage = RunFileOperation(
            () => ReadAvatarFile(avatarFilePath),
            "The avatar image could not be loaded.");

        return avatarImage;
    }

    public void DeleteAvatarImage(string avatarFileName)
    {
        string avatarFilePath = BuildAvatarFilePath(avatarFileName);
        bool wasDeleted = RunFileOperation(
            () => RemoveAvatarFile(avatarFilePath),
            "The avatar image could not be deleted.");

        if (!wasDeleted)
        {
            _logger.Warn("An avatar image could not be deleted because the file no longer exists.");
        }
    }

    private static TResult RunFileOperation<TResult>(Func<TResult> fileOperation, string failureMessage)
    {
        TResult result;

        try
        {
            result = fileOperation();
        }
        catch (IOException exception)
        {
            throw new AvatarStorageUnavailableException(failureMessage, exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new AvatarStorageUnavailableException(failureMessage, exception);
        }

        return result;
    }

    private static byte[] ReadAvatarFile(string avatarFilePath)
    {
        bool fileExists = File.Exists(avatarFilePath);
        byte[] avatarImage = fileExists ? File.ReadAllBytes(avatarFilePath) : Array.Empty<byte>();

        return avatarImage;
    }

    private static bool RemoveAvatarFile(string avatarFilePath)
    {
        bool fileExists = File.Exists(avatarFilePath);

        if (fileExists)
        {
            File.Delete(avatarFilePath);
        }

        return fileExists;
    }

    private static bool MatchesFileNameWithinTimeout(string fileName)
    {
        bool isMatch;

        try
        {
            isMatch = _avatarFileNameRegex.IsMatch(fileName);
        }
        catch (RegexMatchTimeoutException)
        {
            isMatch = false;
        }

        return isMatch;
    }

    private string WriteAvatarFile(string avatarFilePath, byte[] imageBytes)
    {
        Directory.CreateDirectory(_avatarFolderPath);
        File.WriteAllBytes(avatarFilePath, imageBytes);
        string avatarFileName = Path.GetFileName(avatarFilePath);

        return avatarFileName;
    }

    private string BuildAvatarFilePath(string avatarFileName)
    {
        string fileName = avatarFileName ?? string.Empty;
        bool hasExpectedLength = fileName.Length == AvatarFileNameLength;
        bool isFileNameValid = hasExpectedLength && MatchesFileNameWithinTimeout(fileName);

        if (!isFileNameValid)
        {
            _logger.Warn("An avatar file name without the generated format was rejected.");
            throw new ArgumentException(
                "The avatar file name does not have the expected format.",
                nameof(avatarFileName));
        }

        string avatarFilePath = Path.Combine(_avatarFolderPath, fileName);
        return avatarFilePath;
    }
}
