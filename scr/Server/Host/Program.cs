using System;
using System.Threading;
using DominoTrainGame.Server.Service;

namespace DominoTrainGame.Server.Host;

internal static class Program
{
    private static int Main()
    {
        try
        {
            using (GameServiceRuntime service = new GameServiceRuntime())
            using (ManualResetEventSlim stop = new ManualResetEventSlim())
            {
                Console.CancelKeyPress += (sender, args) => { args.Cancel = true; stop.Set(); };
                service.Start();
                Console.WriteLine("Domino Train server is running with the configured endpoints.");
                Console.WriteLine("Press Ctrl+C to stop the server.");
                stop.Wait();
            }
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("The server could not start: " + exception.Message);
            return 1;
        }
    }
}
