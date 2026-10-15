using System;
using System.Threading.Tasks;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : IVerificationService
{
    public async Task<VerificationSessionDto> BeginVerificationAsync(BeginVerificationRequest request)
    {
        Guid sessionId = request is null ? Guid.Empty
            : await _application.BeginVerificationAsync(request.Email, NormalizeLanguage(request.Language)).ConfigureAwait(false);
        return new VerificationSessionDto { SessionId = sessionId, WasSent = sessionId != Guid.Empty };
    }

    public async Task<VerificationSessionDto> ResendVerificationAsync(ResendVerificationRequest request) =>
        new VerificationSessionDto
        {
            SessionId = request?.SessionId ?? Guid.Empty,
            WasSent = request != null && await _application.ResendVerificationAsync(request.SessionId).ConfigureAwait(false)
        };

    public VerificationCodeResponse VerifyCode(VerifyCodeRequest request) => new VerificationCodeResponse
    {
        Status = request is null ? VerificationCodeStatus.Invalid
            : DtoMapper.Map(_application.VerifyCode(request.SessionId, request.Code))
    };
}
