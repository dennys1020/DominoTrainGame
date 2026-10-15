namespace DominoTrainGame.Server.Application.Results;

public sealed class RecoveryAccountResult
{
    public RecoverPasswordValidationStatus Status { get; set; }
    public int PlayerId { get; set; }
    public string Email { get; set; } = string.Empty;
}
