using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application.UseCases;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple,
    UseSynchronizationContext = false)]
public sealed partial class GameService
{
    private readonly GameUseCases _application;

    public GameService(GameUseCases application) =>
        _application = application ?? throw new ArgumentNullException(nameof(application));

    private static string NormalizeLanguage(string language) =>
        string.Equals(language, "es", StringComparison.OrdinalIgnoreCase) ? "es" : "en";
}
