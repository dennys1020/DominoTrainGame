using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
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

    private static bool NotifyClient(Action notify)
    {
        try { notify(); return true; }
        catch (Exception exception) when (exception is CommunicationException || exception is TimeoutException
            || exception is ObjectDisposedException || exception is InvalidOperationException)
        {
            Trace.TraceError("The client could not receive an operation notification: {0}", exception);
            return false;
        }
    }

    private static void RunOneWay(Func<Task> operation, Action<Exception> notifyFailure)
    {
        _ = Task.Run(async () =>
        {
            try { await operation().ConfigureAwait(false); }
            catch (Exception exception)
            {
                Trace.TraceError("A one-way service operation failed: {0}", exception);
                NotifyClient(() => notifyFailure(exception));
            }
        });
    }
}
