using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Verification;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : IVerificationService
{
    private readonly VerificationUseCases _verification;

    public async Task BeginVerificationAsync(Guid requestId, BeginVerificationRequest request)
    {
        IVerificationCallback callback = OperationContext.Current.GetCallbackChannel<IVerificationCallback>();
        Guid sessionId = Guid.Empty;
        if (request != null)
        {
            sessionId = await _verification.BeginAsync(request.Email,
                NormalizeLanguage(request.Language)).ConfigureAwait(false);
        }

        VerificationSessionDto session = new VerificationSessionDto
        {
            SessionId = sessionId,
            WasSent = sessionId != Guid.Empty
        };

        NotifyClient(() => callback.OnVerificationStarted(requestId, session));
    }

    public async Task ResendVerificationAsync(Guid requestId, Guid sessionId)
    {
        IVerificationCallback callback = OperationContext.Current.GetCallbackChannel<IVerificationCallback>();
        bool wasSent = await _verification.ResendAsync(sessionId).ConfigureAwait(false);
        NotifyClient(() => callback.OnVerificationResent(requestId, wasSent));
    }

    public void VerifyCode(Guid requestId, Guid sessionId, string code)
    {
        IVerificationCallback callback = OperationContext.Current.GetCallbackChannel<IVerificationCallback>();
        VerificationStatus status = ValidateCode(sessionId, code);
        NotifyClient(() => callback.OnCodeVerified(requestId, status));
    }

    private VerificationStatus ValidateCode(Guid sessionId, string code)
    {
        VerificationCodeStatus result = _verification.Verify(sessionId, code);
        VerificationStatus status;
        switch (result)
        {
            case VerificationCodeStatus.Valid:
                status = VerificationStatus.Valid;
                break;
            case VerificationCodeStatus.Invalid:
                status = VerificationStatus.Invalid;
                break;
            case VerificationCodeStatus.Expired:
                status = VerificationStatus.Expired;
                break;
            case VerificationCodeStatus.TooManyAttempts:
                status = VerificationStatus.TooManyAttempts;
                break;
            default:
                status = VerificationStatus.SystemError;
                break;
        }

        return status;
    }
}
