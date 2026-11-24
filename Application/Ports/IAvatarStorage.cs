namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Saves, loads and deletes the avatar image files of the players.
/// </summary>
public interface IAvatarStorage
{
    /// <summary>
    /// Saves an avatar image under a new generated file name.
    /// </summary>
    /// <param name="imageBytes">The content of the image.</param>
    /// <param name="fileExtension">The extension that matches the image format, such as <c>.png</c>.</param>
    /// <returns>The generated name of the saved file.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="imageBytes"/> is empty.
    /// </exception>
    /// <exception cref="Errors.AvatarStorageUnavailableException">
    /// Thrown when the file could not be written.
    /// </exception>
    string SaveAvatarImage(byte[] imageBytes, string fileExtension);

    /// <summary>
    /// Loads the content of a saved avatar image.
    /// </summary>
    /// <param name="avatarFileName">The name returned when the image was saved.</param>
    /// <returns>The content of the image, or an empty array when the file does not exist.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="avatarFileName"/> does not have the generated format.
    /// </exception>
    /// <exception cref="Errors.AvatarStorageUnavailableException">
    /// Thrown when the file could not be read.
    /// </exception>
    byte[] LoadAvatarImage(string avatarFileName);

    /// <summary>
    /// Deletes a saved avatar image. Nothing happens when the file does not exist.
    /// </summary>
    /// <param name="avatarFileName">The name returned when the image was saved.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="avatarFileName"/> does not have the generated format.
    /// </exception>
    /// <exception cref="Errors.AvatarStorageUnavailableException">
    /// Thrown when the file could not be deleted.
    /// </exception>
    void DeleteAvatarImage(string avatarFileName);
}
