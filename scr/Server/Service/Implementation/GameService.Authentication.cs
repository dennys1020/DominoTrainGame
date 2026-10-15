using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;
using LoginValidationStatus = DominoTrainGame.Server.Service.DTO.LoginValidationStatus;
using SignUpValidationStatus = DominoTrainGame.Server.Service.DTO.SignUpValidationStatus;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : IAuthenticationService
{
    public LoginResponse LogIn(LoginRequest request) => new LoginResponse
    {
        Status = request is null ? LoginValidationStatus.EmptyFields
            : DtoMapper.Map(_application.LogInUser(request.Identifier, request.Password))
    };

    public SignUpResponse CheckAvailability(RegistrationRequest request) => new SignUpResponse
    {
        Status = request is null ? SignUpValidationStatus.EmptyFields
            : DtoMapper.Map(_application.CheckAvailability(request.Username, request.Email, request.Password))
    };

    public SignUpResponse RegisterUser(RegistrationRequest request) => new SignUpResponse
    {
        Status = request is null ? SignUpValidationStatus.EmptyFields
            : DtoMapper.Map(_application.RegisterUser(request.Username, request.Email, request.Password,
                NormalizeLanguage(request.Language), request.VerificationId))
    };
}
