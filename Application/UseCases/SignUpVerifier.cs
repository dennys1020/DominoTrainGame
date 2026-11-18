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

    public void SendVerificationCode(PlayerRegistration registration)
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        string verificationCode = _codeGenerator.Generate();

        _emailSender.SendVerificationCode(registration.Email, verificationCode);
        _pendingSignUps.StorePendingSignUp(registration, verificationCode);
    }

    public bool ResendVerificationCode(string email)
    {
        PendingSignUp pendingSignUp = _pendingSignUps.FindPendingSignUpByEmail(email);
        bool isPending = !pendingSignUp.IsEmpty;

        if (isPending)
        {
            SendVerificationCode(pendingSignUp.Registration);
        }

        return isPending;
    }

    public VerifyEmailStatus VerifyPendingSignUpCode(string email, string verificationCode)
    {
        VerifyEmailStatus emailStatus = _pendingSignUps.VerifyPendingSignUpCode(email, verificationCode);
        return emailStatus;
    }

    public PendingSignUp FindPendingSignUpByEmail(string email)
    {
        PendingSignUp pendingSignUp = _pendingSignUps.FindPendingSignUpByEmail(email);
        return pendingSignUp;
    }

    public void DiscardPendingSignUp(string email)
    {
        _pendingSignUps.RemovePendingSignUp(email);
    }
}
