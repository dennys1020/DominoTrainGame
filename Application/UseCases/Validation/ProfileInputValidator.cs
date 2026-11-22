using System;
using System.Text.RegularExpressions;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases.Validation;

public static class ProfileInputValidator
{
    public const int MaximumBiographyLength = 255;
    public const int MaximumUsernameLength = 45;
    public const int MaximumInstagramUsernameLength = 30;
    public const int MaximumFacebookUsernameLength = 50;
    public const int MaximumTwitterUsernameLength = 15;

    private const int RegexTimeoutMilliseconds = 100;
    private const string UsernamePattern = "^[A-Za-z0-9_]{3,45}$";
    private const string InstagramUsernamePattern = "^[A-Za-z0-9._]{1,30}$";
    private const string FacebookUsernamePattern = "^[A-Za-z0-9.]{5,50}$";
    private const string TwitterUsernamePattern = "^[A-Za-z0-9_]{1,15}$";
    private const string SocialUsernamePrefix = "@";

    private static readonly Regex _usernameRegex;
    private static readonly Regex _instagramUsernameRegex;
    private static readonly Regex _facebookUsernameRegex;
    private static readonly Regex _twitterUsernameRegex;

    static ProfileInputValidator()
    {
        _usernameRegex = CreateRegex(UsernamePattern);
        _instagramUsernameRegex = CreateRegex(InstagramUsernamePattern);
        _facebookUsernameRegex = CreateRegex(FacebookUsernamePattern);
        _twitterUsernameRegex = CreateRegex(TwitterUsernamePattern);
    }

    public static string NormalizeBiography(string biography)
    {
        string normalizedBiography = biography?.Trim() ?? string.Empty;

        if (normalizedBiography.Length > MaximumBiographyLength)
        {
            throw new ArgumentException("The biography exceeds the maximum allowed length.", nameof(biography));
        }

        return normalizedBiography;
    }

    public static string NormalizeNewUsername(string newUsername)
    {
        string normalizedUsername = newUsername?.Trim() ?? string.Empty;
        bool isUsernameValid = IsFormatValid(normalizedUsername, _usernameRegex, MaximumUsernameLength);

        if (!isUsernameValid)
        {
            throw new ArgumentException(
                "The username must have between 3 and 45 letters, digits or underscores.",
                nameof(newUsername));
        }

        return normalizedUsername;
    }

    public static SocialLinks NormalizeSocialLinks(SocialLinks socialLinks)
    {
        if (socialLinks == null)
        {
            throw new ArgumentNullException(nameof(socialLinks));
        }

        SocialLinks normalizedSocialLinks = new SocialLinks(
            NormalizeSocialUsername(
                socialLinks.InstagramUsername,
                _instagramUsernameRegex,
                MaximumInstagramUsernameLength),
            NormalizeSocialUsername(
                socialLinks.FacebookUsername,
                _facebookUsernameRegex,
                MaximumFacebookUsernameLength),
            NormalizeSocialUsername(
                socialLinks.TwitterUsername,
                _twitterUsernameRegex,
                MaximumTwitterUsernameLength));

        return normalizedSocialLinks;
    }

    private static Regex CreateRegex(string pattern)
    {
        Regex regex = new Regex(
            pattern,
            RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));
        return regex;
    }

    private static string NormalizeSocialUsername(
        string socialUsername,
        Regex socialUsernameRegex,
        int maximumLength)
    {
        string trimmedUsername = socialUsername?.Trim() ?? string.Empty;
        bool hasPrefix = trimmedUsername.StartsWith(SocialUsernamePrefix, StringComparison.Ordinal);
        string normalizedUsername = hasPrefix
            ? trimmedUsername.Substring(SocialUsernamePrefix.Length)
            : trimmedUsername;
        bool isRemoved = normalizedUsername.Length == 0;

        if (!isRemoved && !IsFormatValid(normalizedUsername, socialUsernameRegex, maximumLength))
        {
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
