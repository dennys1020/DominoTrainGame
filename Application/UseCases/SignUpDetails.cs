namespace DominoTrainGame.Server.Application.UseCases;

public sealed class SignUpDetails
{
    public SignUpDetails(string username, string email, string password)
    {
        Username = username ?? string.Empty;
        Email = email ?? string.Empty;
        Password = password ?? string.Empty;
    }

    public string Username { get; }

    public string Email { get; }

    public string Password { get; }
}
