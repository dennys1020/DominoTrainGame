using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

/// <summary>
/// Creates accounts and opens the sessions of the players.
/// </summary>
/// <remarks>
/// A successful login returns the session token that the other services ask for.
/// </remarks>
[ServiceContract]
public interface IAuthenticationService
{
    /// <summary>
    /// Logs in a registered player with a username or email and a password.
    /// </summary>
    /// <param name="request">The username or email and the password.</param>
    /// <returns>
    /// The player identifier and a new session token, or a response with <c>IsAuthenticated</c> set to <c>false</c>
    /// when the credentials are not valid.
    /// </returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when the identifier or the password is empty or too
    /// long, <see cref="GameServiceFaultCode.AlreadyConnected"/> when the player already has an open session, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    LoginResponse LogIn(LoginRequest request);

    /// <summary>
    /// Starts the registration of a new account and sends a verification code to its email.
    /// </summary>
    /// <param name="request">The username, email, password and language of the new account.</param>
    /// <returns><c>true</c> when the code was sent; <c>false</c> when the username or the email is in use.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when a field is empty, too long, too short or
    /// malformed, <see cref="GameServiceFaultCode.EmailUnavailable"/> when the email cannot be sent, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    bool SignUp(SignUpRequest request);

    /// <summary>
    /// Logs in a new guest player with a generated username.
    /// </summary>
    /// <returns>
    /// The guest identifier and a new session token, or a response with <c>IsAuthenticated</c> set to <c>false</c> when
    /// no free guest username was found.
    /// </returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    LoginResponse LogInAsGuest();

    /// <summary>
    /// Checks the verification code of a pending registration and creates the account.
    /// </summary>
    /// <param name="request">The email of the registration and the code entered by the player.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.VerificationNotFound"/> when the email has no pending registration,
    /// <see cref="GameServiceFaultCode.InvalidVerificationCode"/> when the code is wrong,
    /// <see cref="GameServiceFaultCode.VerificationCodeExpired"/> when the code expired,
    /// <see cref="GameServiceFaultCode.TooManyVerificationAttempts"/> when the code was entered wrong too many times,
    /// <see cref="GameServiceFaultCode.AccountAlreadyExists"/> when another player took the username or the email
    /// first, <see cref="GameServiceFaultCode.InvalidInput"/> when the account data violates a database rule, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void VerifyEmail(VerifyEmailRequest request);

    /// <summary>
    /// Sends a new verification code to the email of a pending registration.
    /// </summary>
    /// <param name="request">The email of the registration and the language of the client.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.VerificationNotFound"/> when the email has no pending registration,
    /// <see cref="GameServiceFaultCode.ResendTooSoon"/> when the previous code was sent too recently, or
    /// <see cref="GameServiceFaultCode.EmailUnavailable"/> when the email cannot be sent.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void ResendVerificationCode(ResendVerificationCodeRequest request);
}
