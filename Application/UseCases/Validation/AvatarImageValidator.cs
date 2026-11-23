using System;
using System.Linq;
using log4net.Ext.Trace;

namespace DominoTrainGame.Server.Application.UseCases.Validation;

public static class AvatarImageValidator
{
    public const int MaximumAvatarImageBytes = 1048576;

    private const string PngFileExtension = ".png";
    private const string JpgFileExtension = ".jpg";

    private static readonly ITraceLog _logger;
    private static readonly byte[] _pngFileSignature;
    private static readonly byte[] _jpgFileSignature;

    static AvatarImageValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(AvatarImageValidator));
        _pngFileSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        _jpgFileSignature = new byte[] { 0xFF, 0xD8, 0xFF };
    }

    public static string ResolveAvatarFileExtension(byte[] imageBytes)
    {
        ValidateAvatarImageSize(imageBytes);
        bool isPng = StartsWithSignature(imageBytes, _pngFileSignature);
        bool isJpg = StartsWithSignature(imageBytes, _jpgFileSignature);

        if (!isPng && !isJpg)
        {
            _logger.Warn("An avatar that is not a PNG or JPG file was rejected.");
            throw new ArgumentException("The avatar image must be a PNG or JPG file.", nameof(imageBytes));
        }

        string fileExtension = isPng ? PngFileExtension : JpgFileExtension;

        return fileExtension;
    }

    private static void ValidateAvatarImageSize(byte[] imageBytes)
    {
        bool hasContent = imageBytes != null && imageBytes.Any();

        if (!hasContent || imageBytes.Length > MaximumAvatarImageBytes)
        {
            _logger.Warn("An avatar that is empty or larger than the maximum size was rejected.");
            throw new ArgumentException(
                "The avatar image is empty or exceeds the maximum allowed size.",
                nameof(imageBytes));
        }
    }

    private static bool StartsWithSignature(byte[] imageBytes, byte[] fileSignature)
    {
        bool matchesSignature = imageBytes.Take(fileSignature.Length).SequenceEqual(fileSignature);
        return matchesSignature;
    }
}
