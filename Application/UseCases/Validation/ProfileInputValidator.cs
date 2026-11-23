using System;
using System.Text.RegularExpressions;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases.Validation;

public static class ProfileInputValidator
{
    public const int MaximumBiographyLength = 255;
    public const int MinimumUsernameLength = 3;
    public const int MinimumSocialUsernameLength = 1;
    public const int MaximumInstagramUsernameLength = 30;
    public const int MinimumFacebookUsernameLength = 5;
    public const int MaximumFacebookUsernameLength = 50;
    public const int MaximumTwitterUsernameLength = 15;

    private const int RegexTimeoutMilliseconds = 100;
    private const string UsernamePattern = "^[A-Za-z0-9_]+$";
    private const string InstagramUsernamePattern = "^[A-Za-z0-9._]+$";
    private const string FacebookUsernamePattern = "^[A-Za-z0-9.]+$";
    private const string TwitterUsernamePattern = "^[A-Za-z0-9_]+$";
    private const string SocialUsernamePrefix = "@";

    private static readonly Regex _usernameRegex;
    private static readonly SocialUsernameRule _instagramUsernameRule;
    private static readonly SocialUsernameRule _facebookUsernameRule;
    private static readonly SocialUsernameRule _twitterUsernameRule;

    static ProfileInputValidator()
    {
        _usernameRegex = CreateRegex(UsernamePattern);
        _instagramUsernameRule = new SocialUsernameRule(
            CreateRegex(InstagramUsernamePattern),
            MinimumSocialUsernameLength,
            MaximumInstagramUsernameLength);
        _facebookUsernameRule = new SocialUsernameRule(
            CreateRegex(FacebookUsernamePattern),
            MinimumFacebookUsernameLength,
            MaximumFacebookUsernameLength);
        _twitterUsernameRule = new SocialUsernameRule(
            CreateRegex(TwitterUsernamePattern),
            MinimumSocialUsernameLength,
            MaximumTwitterUsernameLength);
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
        bool isLengthValid = IsLengthWithinRange(
            normalizedUsername,
            MinimumUsernameLength,
            PlayerInputValidator.MaximumUsernameLength);
        bool isUsernameValid = isLengthValid && MatchesFormatWithinTimeout(normalizedUsername, _usernameRegex);

        if (!isUsernameValid)
        {
            throw new ArgumentException("The username format is not valid.", nameof(newUsername));
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
            NormalizeSocialUsername(socialLinks.InstagramUsername, _instagramUsernameRule),
            NormalizeSocialUsername(socialLinks.FacebookUsername, _facebookUsernameRule),
            NormalizeSocialUsername(socialLinks.TwitterUsername, _twitterUsernameRule));

        return normalizedSocialLinks;
    }

    private static Regex CreateRegex(string pattern)
    {
        TimeSpan matchTimeout = TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds);
        Regex regex = new Regex(pattern, RegexOptions.CultureInvariant, matchTimeout);
        return regex;
    }

    private static string NormalizeSocialUsername(string socialUsername, SocialUsernameRule usernameRule)
    {
        string trimmedUsername = socialUsername?.Trim() ?? string.Empty;
        bool hasPrefix = trimmedUsername.StartsWith(SocialUsernamePrefix, StringComparison.Ordinal);
        string normalizedUsername = hasPrefix
            ? trimmedUsername.Substring(SocialUsernamePrefix.Length)
            : trimmedUsername;
        bool isRemoved = string.IsNullOrEmpty(normalizedUsername);

        if (!isRemoved && !IsSocialUsernameValid(normalizedUsername, usernameRule))
        {
            throw new ArgumentException(
                "A social network username does not have a valid format.",
                nameof(socialUsername));
        }

        return normalizedUsername;
    }

    private static bool IsSocialUsernameValid(string socialUsername, SocialUsernameRule usernameRule)
    {
        bool isLengthValid = IsLengthWithinRange(
            socialUsername,
            usernameRule.MinimumLength,
            usernameRule.MaximumLength);
        bool isUsernameValid = isLengthValid && MatchesFormatWithinTimeout(socialUsername, usernameRule.FormatRegex);

        return isUsernameValid;
    }

    private static bool IsLengthWithinRange(string value, int minimumLength, int maximumLength)
    {
        bool isLongEnough = value.Length >= minimumLength;
        bool isShortEnough = value.Length <= maximumLength;
        bool isWithinRange = isLongEnough && isShortEnough;

        return isWithinRange;
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

    private sealed class SocialUsernameRule
    {
        public SocialUsernameRule(Regex formatRegex, int minimumLength, int maximumLength)
        {
            FormatRegex = formatRegex;
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;
        }

        public Regex FormatRegex { get; }

        public int MinimumLength { get; }

        public int MaximumLength { get; }
    }
}
