using System;
using System.Text.RegularExpressions;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Guards;
using DominoTrainGame.Shared.Common;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Server.Application.UseCases.Validation;

public static class ProfileInputValidator
{
    public const int MaximumBiographyLength = 255;
    public const int MinimumUsernameLength = 3;
    public const int MaximumInstagramUsernameLength = 30;
    public const int MinimumFacebookUsernameLength = 5;
    public const int MaximumFacebookUsernameLength = 50;
    public const int MaximumTwitterUsernameLength = 15;

    private const int RegexTimeoutMilliseconds = 100;
    private const string LettersDigitsUnderscorePattern = "^[A-Za-z0-9_]+$";
    private const string InstagramUsernamePattern = "^[A-Za-z0-9._]+$";
    private const string FacebookUsernamePattern = "^[A-Za-z0-9.]+$";
    private const string SocialUsernamePrefix = "@";

    private static readonly ITraceLog _logger;
    private static readonly Regex _lettersDigitsUnderscoreRegex;
    private static readonly Regex _instagramUsernameRegex;
    private static readonly Regex _facebookUsernameRegex;

    static ProfileInputValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(ProfileInputValidator));
        _lettersDigitsUnderscoreRegex = CreateRegex(LettersDigitsUnderscorePattern);
        _instagramUsernameRegex = CreateRegex(InstagramUsernamePattern);
        _facebookUsernameRegex = CreateRegex(FacebookUsernamePattern);
    }

    public static string NormalizeBiography(string biography)
    {
        string normalizedBiography = InputText.Normalize(biography);

        if (normalizedBiography.Length > MaximumBiographyLength)
        {
            _logger.Warn("A biography longer than the maximum length was rejected.");
            throw new ArgumentException("The biography exceeds the maximum allowed length.", nameof(biography));
        }

        return normalizedBiography;
    }

    public static string NormalizeNewUsername(string newUsername)
    {
        string normalizedUsername = InputText.Normalize(newUsername);
        bool isLongEnough = normalizedUsername.Length >= MinimumUsernameLength;
        bool isFormatValid = IsFormatValid(
            normalizedUsername,
            _lettersDigitsUnderscoreRegex,
            GameRules.MaximumUsernameLength);

        if (!isLongEnough || !isFormatValid)
        {
            _logger.Warn("A username with an invalid length or format was rejected.");
            throw new ArgumentException("The username format is not valid.", nameof(newUsername));
        }

        return normalizedUsername;
    }

    public static SocialLinks NormalizeSocialLinks(SocialLinks socialLinks)
    {
        ArgumentGuard.ThrowIfNull(socialLinks, nameof(socialLinks), _logger);
        SocialLinks normalizedSocialLinks = new SocialLinks(
            NormalizeSocialUsername(
                socialLinks.InstagramUsername,
                _instagramUsernameRegex,
                MaximumInstagramUsernameLength),
            NormalizeFacebookUsername(socialLinks.FacebookUsername),
            NormalizeSocialUsername(
                socialLinks.TwitterUsername,
                _lettersDigitsUnderscoreRegex,
                MaximumTwitterUsernameLength));

        return normalizedSocialLinks;
    }

    private static Regex CreateRegex(string pattern)
    {
        TimeSpan matchTimeout = TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds);
        Regex regex = new Regex(pattern, RegexOptions.CultureInvariant, matchTimeout);
        return regex;
    }

    private static string NormalizeFacebookUsername(string facebookUsername)
    {
        string normalizedUsername = NormalizeSocialUsername(
            facebookUsername,
            _facebookUsernameRegex,
            MaximumFacebookUsernameLength);
        bool isRemoved = normalizedUsername.Length == InputText.EmptyLength;
        bool isTooShort = normalizedUsername.Length < MinimumFacebookUsernameLength;

        if (!isRemoved && isTooShort)
        {
            _logger.Warn("A Facebook username shorter than the minimum length was rejected.");
            throw new ArgumentException(
                "A social network username does not have a valid format.",
                nameof(facebookUsername));
        }

        return normalizedUsername;
    }

    private static string NormalizeSocialUsername(string socialUsername, Regex usernameRegex, int maximumLength)
    {
        string trimmedUsername = InputText.Normalize(socialUsername);
        bool hasPrefix = trimmedUsername.StartsWith(SocialUsernamePrefix, StringComparison.Ordinal);
        string normalizedUsername = hasPrefix
            ? trimmedUsername.Substring(SocialUsernamePrefix.Length)
            : trimmedUsername;
        bool isRemoved = normalizedUsername.Length == InputText.EmptyLength;

        if (!isRemoved && !IsFormatValid(normalizedUsername, usernameRegex, maximumLength))
        {
            _logger.Warn("A social network username with an invalid length or format was rejected.");
            throw new ArgumentException(
                "A social network username does not have a valid format.",
                nameof(socialUsername));
        }

        return normalizedUsername;
    }

    private static bool IsFormatValid(string value, Regex formatRegex, int maximumLength)
    {
        bool isWithinMaximumLength = value.Length <= maximumLength;
        bool isFormatValid = isWithinMaximumLength && MatchesFormatWithinTimeout(value, formatRegex);

        return isFormatValid;
    }

    private static bool MatchesFormatWithinTimeout(string value, Regex formatRegex)
    {
        bool isMatch;

        try
        {
            isMatch = formatRegex.IsMatch(value);
        }
        catch (RegexMatchTimeoutException)
        {
            isMatch = false;
        }

        return isMatch;
    }
}
