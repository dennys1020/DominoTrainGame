using System;
using System.Diagnostics;
using System.ServiceModel;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService
{
    private static void NotifyClient(Action notification)
    {
        try { notification(); }
        catch (CommunicationException exception) { Trace.TraceError(exception.ToString()); }
        catch (TimeoutException exception) { Trace.TraceError(exception.ToString()); }
        catch (ObjectDisposedException exception) { Trace.TraceError(exception.ToString()); }
    }
}
