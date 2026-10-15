using System;
using System.ServiceModel;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : IVerificationService
{
    public void BeginVerification(BeginVerificationRequest request)
    {
        IVerificationCallback callback = OperationContext.Current.GetCallbackChannel<IVerificationCallback>();
        Guid requestId = request?.RequestId ?? Guid.Empty;
        RunOneWay(async () =>
        {
            if (!NotifyVerification(callback, requestId, OperationState.Started, "Sending the verification email."))
            {
                return;
            }
            Guid sessionId = request is null ? Guid.Empty
                : await _application.BeginVerificationAsync(request.Email, NormalizeLanguage(request.Language)).ConfigureAwait(false);
            bool wasSent = sessionId != Guid.Empty;
            NotifyVerification(callback, requestId, wasSent ? OperationState.Completed : OperationState.Failed,
                wasSent ? "The verification email was sent." : "The verification email could not be sent.",
                new VerificationSessionDto { SessionId = sessionId, WasSent = wasSent });
        }, _ => NotifyVerification(callback, requestId, OperationState.Failed,
            "The server could not send the verification email."));
    }

    public void ResendVerification(ResendVerificationRequest request)
    {
        IVerificationCallback callback = OperationContext.Current.GetCallbackChannel<IVerificationCallback>();
        Guid requestId = request?.RequestId ?? Guid.Empty;
        RunOneWay(async () =>
        {
            if (!NotifyVerification(callback, requestId, OperationState.Started, "Resending the verification email."))
            {
                return;
            }
            bool wasSent = request != null
                && await _application.ResendVerificationAsync(request.SessionId).ConfigureAwait(false);
            NotifyVerification(callback, requestId, wasSent ? OperationState.Completed : OperationState.Failed,
                wasSent ? "The verification email was resent." : "The verification email could not be resent.",
                new VerificationSessionDto { SessionId = request?.SessionId ?? Guid.Empty, WasSent = wasSent });
        }, _ => NotifyVerification(callback, requestId, OperationState.Failed,
            "The server could not resend the verification email."));
    }

    public VerificationCodeResponse VerifyCode(VerifyCodeRequest request) => new VerificationCodeResponse
    {
        Status = request is null ? VerificationCodeStatus.Invalid
            : DtoMapper.Map(_application.VerifyCode(request.SessionId, request.Code))
    };

    private static bool NotifyVerification(IVerificationCallback callback, Guid requestId, OperationState state,
        string message, VerificationSessionDto result = null) => NotifyClient(() =>
            callback.OnVerificationChanged(new VerificationNotification
            {
                RequestId = requestId, State = state, Message = message, Result = result
            }));
}
