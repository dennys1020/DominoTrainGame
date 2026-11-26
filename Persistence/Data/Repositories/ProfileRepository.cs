using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using log4net;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Guards;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

public sealed class ProfileRepository : IProfileRepository
{
    private static readonly ILog _logger;

    private readonly DatabaseExecutor _databaseExecutor;

    static ProfileRepository()
    {
        _logger = LogManager.GetLogger(typeof(ProfileRepository));
    }

    public ProfileRepository(DatabaseExecutor databaseExecutor)
    {
        _databaseExecutor = ArgumentGuard.NotNull(databaseExecutor, nameof(databaseExecutor), _logger);
    }

    public PlayerProfileSummary FindProfileByUsername(string username)
    {
        PlayerProfileSummary profile = _databaseExecutor.Execute(QueryProfileByUsername, username);

        return profile;
    }

    public PlayerProfileSummary FindProfileByPlayerId(int playerId)
    {
        PlayerProfileSummary profile = _databaseExecutor.Execute(QueryProfileByPlayerId, playerId);

        return profile;
    }

    public bool UpdateBiography(int playerId, string biography)
    {
        bool isUpdated = _databaseExecutor.Execute(context => SaveBiography(context, playerId, biography));

        return isUpdated;
    }

    public bool UpdateSocialLinks(int playerId, SocialLinks socialLinks)
    {
        ArgumentGuard.ThrowIfNull(socialLinks, nameof(socialLinks), _logger);
        bool isUpdated = _databaseExecutor.Execute(context => SaveSocialLinks(context, playerId, socialLinks));

        return isUpdated;
    }

    public bool UpdateAvatarFileName(int playerId, string avatarFileName)
    {
        bool isUpdated = _databaseExecutor.Execute(context => SaveAvatarFileName(context, playerId, avatarFileName));

        return isUpdated;
    }

    public ChangeUsernameStatus ChangeUsername(int playerId, string newUsername)
    {
        ArgumentGuard.ThrowIfEmpty(newUsername, nameof(newUsername), _logger);
        ChangeUsernameStatus status = _databaseExecutor.Execute(
            context => SaveUsername(context, playerId, newUsername));

        return status;
    }

    private static PlayerProfileSummary QueryProfileByUsername(DominoGameDBEntities context, string username)
    {
        IQueryable<PlayerProfile> profileQuery = context.PlayerProfiles
            .Where(profile => profile.Player.Username == username);
        PlayerProfileSummary profile = ReadFirstProfile(profileQuery);

        return profile;
    }

    private static PlayerProfileSummary QueryProfileByPlayerId(DominoGameDBEntities context, int playerId)
    {
        IQueryable<PlayerProfile> profileQuery = FilterProfilesByPlayerId(context, playerId);
        PlayerProfileSummary profile = ReadFirstProfile(profileQuery);

        return profile;
    }

    private static PlayerProfileSummary ReadFirstProfile(IQueryable<PlayerProfile> profileQuery)
    {
        List<PlayerProfile> profiles = profileQuery
            .AsNoTracking()
            .Include(profile => profile.Player)
            .ToList();
        PlayerProfileSummary profile = profiles.Any() ? ToProfile(profiles.First()) : PlayerProfileSummary.Empty;

        return profile;
    }

    private static PlayerProfileSummary ToProfile(PlayerProfile storedProfile)
    {
        PlayerIdentity player = new PlayerIdentity(storedProfile.PlayerId, storedProfile.Player.Username);
        ProfileDetails details = new ProfileDetails(
            storedProfile.Biography,
            storedProfile.AvatarFileName,
            storedProfile.CreatedAt);
        SocialLinks socialLinks = new SocialLinks(
            storedProfile.InstagramUsername,
            storedProfile.FacebookUsername,
            storedProfile.TwitterUsername);
        PlayerProfileSummary profile = new PlayerProfileSummary(player, details, socialLinks);

        return profile;
    }

    private static IQueryable<PlayerProfile> FilterProfilesByPlayerId(DominoGameDBEntities context, int playerId)
    {
        IQueryable<PlayerProfile> profileQuery = context.PlayerProfiles
            .Where(profile => profile.PlayerId == playerId);

        return profileQuery;
    }

    private static List<PlayerProfile> QueryTrackedProfiles(DominoGameDBEntities context, int playerId)
    {
        List<PlayerProfile> profiles = FilterProfilesByPlayerId(context, playerId)
            .Include(profile => profile.Player)
            .ToList();

        return profiles;
    }

    private static bool SaveProfileChanges(DominoGameDBEntities context, List<PlayerProfile> profiles)
    {
        context.SaveChanges();
        bool isFound = profiles.Any();

        return isFound;
    }

    private static bool SaveBiography(DominoGameDBEntities context, int playerId, string biography)
    {
        List<PlayerProfile> profiles = QueryTrackedProfiles(context, playerId);

        foreach (PlayerProfile profile in profiles)
        {
            profile.Biography = biography;
        }

        bool isFound = SaveProfileChanges(context, profiles);
        return isFound;
    }

    private static bool SaveSocialLinks(DominoGameDBEntities context, int playerId, SocialLinks socialLinks)
    {
        List<PlayerProfile> profiles = QueryTrackedProfiles(context, playerId);

        foreach (PlayerProfile profile in profiles)
        {
            profile.InstagramUsername = socialLinks.InstagramUsername;
            profile.FacebookUsername = socialLinks.FacebookUsername;
            profile.TwitterUsername = socialLinks.TwitterUsername;
        }

        bool isFound = SaveProfileChanges(context, profiles);
        return isFound;
    }

    private static bool SaveAvatarFileName(DominoGameDBEntities context, int playerId, string avatarFileName)
    {
        List<PlayerProfile> profiles = QueryTrackedProfiles(context, playerId);

        foreach (PlayerProfile profile in profiles)
        {
            profile.AvatarFileName = avatarFileName;
        }

        bool isFound = SaveProfileChanges(context, profiles);
        return isFound;
    }

    private static ChangeUsernameStatus SaveUsername(DominoGameDBEntities context, int playerId, string newUsername)
    {
        List<PlayerProfile> profiles = QueryTrackedProfiles(context, playerId);
        ChangeUsernameStatus status = profiles.Any()
            ? ApplyUsernameChange(context, profiles.First().Player, newUsername)
            : ChangeUsernameStatus.GuestNotAllowed;

        return status;
    }

    private static ChangeUsernameStatus ApplyUsernameChange(
        DominoGameDBEntities context,
        Player player,
        string newUsername)
    {
        bool isTaken = context.Players
            .Where(otherPlayer => otherPlayer.PlayerId != player.PlayerId)
            .Any(otherPlayer => otherPlayer.Username == newUsername);

        if (!isTaken)
        {
            player.Username = newUsername;
            context.SaveChanges();
        }

        ChangeUsernameStatus status = isTaken ? ChangeUsernameStatus.UsernameTaken : ChangeUsernameStatus.Success;
        return status;
    }
}
