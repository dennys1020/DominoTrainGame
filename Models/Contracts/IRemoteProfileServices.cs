using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteProfileServices
{
    string CurrentUsername { get; }

    Task<PlayerProfileResult> GetProfileAsync(string username);

    Task<ProfileActionResult> UpdateBiographyAsync(string biography);

    Task<ProfileActionResult> ChangeUsernameAsync(string newUsername);

    Task<ProfileActionResult> UpdateSocialLinksAsync(ProfileSocialLinks socialLinks);

    Task<ProfileActionResult> UpdateAvatarAsync(byte[] imageBytes);
}
