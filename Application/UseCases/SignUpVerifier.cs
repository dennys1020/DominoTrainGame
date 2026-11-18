using System;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class SignUpVerifier
{
    private readonly PendingSignUpRegistry _pendingSignUps;
    private readonly IVerificationCodeGenerator _codeGenerator;
    private readonly IEmailSender _emailSender;

    public SignUpVerifier(
        PendingSignUpRegistry pendingSignUps,
        IVerificationCodeGenerator codeGenerator,
        IEmailSender emailSender)
    {
        _pendingSignUps = pendingSignUps ?? throw new ArgumentNullException(nameof(pendingSignUps));
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public void SendCode(PlayerRegistration registration)
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        string verificationCode = _codeGenerator.Generate();

        _emailSender.SendVerificationCode(registration.Email, verificationCode);
        _pendingSignUps.Store(registration, verificationCode);
    }

    public bool ResendCode(string email)
    {
        bool isPending = _pendingSignUps.TryFind(email, out PendingSignUp pendingSignUp);

        if (isPending)
        {
            SendCode(pendingSignUp.Registration);
        }

        return isPending;
    }

    public VerifyEmailStatus VerifyAccount(string email, string verificationCode)
    {
        VerifyEmailStatus status = _pendingSignUps.Verify(email, verificationCode);
        return status;
    }

    public bool TryFindPendingSignUp(string email, out PendingSignUp pendingSignUp)
    {
        bool isFound = _pendingSignUps.TryFind(email, out pendingSignUp);
        return isFound;
    }

    public void DiscardPendingSignUp(string email)
    {
        _pendingSignUps.Remove(email);
    }
}
