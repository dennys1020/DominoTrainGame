using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

/// <summary>
/// Shows and edits the profiles of registered players. Every request carries the session token of the caller.
/// </summary>
[ServiceContract]
public interface IProfileService
{
    /// <summary>
    /// Gets the profile, the avatar image and the statistics of a registered player.
    /// </summary>
    /// <param name="request">The session token of the caller and the username of the profile owner.</param>
    /// <returns>The profile of the player.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <c>SessionNotFound</c>, <c>GuestNotAllowed</c>, <c>PlayerNotFound</c>, <c>InvalidInput</c>
    /// or <c>DatabaseUnavailable</c>.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    ProfileResponse GetProfile(ProfileRequest request);

    /// <summary>
    /// Gets the profile, the avatar image and the statistics of the caller.
    /// </summary>
    /// <param name="request">The session token of the caller.</param>
    /// <returns>The profile of the caller.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <c>SessionNotFound</c>, <c>GuestNotAllowed</c>, <c>PlayerNotFound</c>
    /// or <c>DatabaseUnavailable</c>.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    ProfileResponse GetOwnProfile(SessionTokenRequest request);

    /// <summary>
    /// Replaces the biography of the caller.
    /// </summary>
    /// <param name="request">The session token of the caller and the new biography.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <c>SessionNotFound</c>, <c>GuestNotAllowed</c>, <c>InvalidInput</c>
    /// or <c>DatabaseUnavailable</c>.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void UpdateBiography(BiographyUpdateRequest request);

    /// <summary>
    /// Changes the username of the caller and renews the session with the new name.
    /// </summary>
    /// <param name="request">The session token of the caller and the new username.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <c>SessionNotFound</c>, <c>GuestNotAllowed</c>, <c>UsernameAlreadyTaken</c>,
    /// <c>InvalidInput</c> or <c>DatabaseUnavailable</c>.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void ChangeUsername(UsernameChangeRequest request);

    /// <summary>
    /// Replaces the social network usernames of the caller.
    /// </summary>
    /// <param name="request">The session token of the caller and the three social network usernames.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <c>SessionNotFound</c>, <c>GuestNotAllowed</c>, <c>InvalidInput</c>
    /// or <c>DatabaseUnavailable</c>.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void UpdateSocialLinks(SocialLinksUpdateRequest request);

    /// <summary>
    /// Replaces the avatar image of the caller with a PNG or JPG image of at most 1 MB.
    /// </summary>
    /// <param name="request">The session token of the caller and the content of the image.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <c>SessionNotFound</c>, <c>GuestNotAllowed</c>, <c>InvalidInput</c>,
    /// <c>DatabaseUnavailable</c> or <c>AvatarStorageUnavailable</c>.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void UpdateAvatar(AvatarUpdateRequest request);
}
