using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Contracts;
using DominoTrainGame.Models;
using DominoTrainGame.Utils;
using DominoTrainGame.Validator;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application;

public sealed class GameServices : IGameServices
{
    private readonly LoginValidator _login;
    private readonly SignUpValidator _signUp;
    private readonly RecoverPasswordValidator _recovery;
    private readonly IEmailSender _email;
    private readonly ILeaderboardSource _leaderboard;

    public GameServices(IAuthenticationStore accounts, IEmailSender email, ILeaderboardSource leaderboard)
    {
        _login = new LoginValidator(accounts);
        _signUp = new SignUpValidator(accounts);
        _recovery = new RecoverPasswordValidator(accounts);
        _email = email;
        _leaderboard = leaderboard;
    }

    public LoginValidationStatus LogInUser(string identifier, string password) => _login.LogInUser(identifier, password);
    public SignUpValidationStatus CheckAvailability(string username, string email, string password) =>
        _signUp.CheckAvailability(username, email, password);
    public SignUpValidationStatus RegisterUser(string username, string email, string password) =>
        _signUp.RegisterUser(username, email, password);
    public RecoverPasswordValidationStatus FindAccount(string identifier, out int playerId, out string email) =>
        _recovery.FindAccount(identifier, out playerId, out email);
    public RecoverPasswordValidationStatus UpdatePassword(int playerId, string password, string confirmation) =>
        _recovery.UpdatePassword(playerId, password, confirmation);
    public Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken) =>
        _leaderboard.GetEntriesAsync(cancellationToken);
    public IVerificationSession CreateVerificationSession() => new VerificationCodeService();
    public Task<bool> SendVerificationCodeAsync(string email, string code) => _email.SendVerificationCodeAsync(email, code);
}
