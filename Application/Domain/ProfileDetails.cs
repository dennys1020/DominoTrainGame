using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class ProfileDetails
{
    static ProfileDetails()
    {
        Empty = new ProfileDetails(string.Empty, string.Empty, DateTime.MinValue);
    }

    public ProfileDetails(string biography, string avatarFileName, DateTime memberSince)
    {
        Biography = biography ?? string.Empty;
        AvatarFileName = avatarFileName ?? string.Empty;
        MemberSince = memberSince;
    }

    public static ProfileDetails Empty { get; }

    public string Biography { get; }

    public string AvatarFileName { get; }

    public DateTime MemberSince { get; }

    public bool HasAvatar => !string.IsNullOrEmpty(AvatarFileName);

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
