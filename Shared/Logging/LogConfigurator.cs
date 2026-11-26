using System;
using System.IO;
using log4net.Config;

namespace DominoTrainGame.Shared.Logging;

internal static class LogConfigurator
{
    private const string ConfigurationFileName = "log4net.config";

    public static void Configure()
    {
        string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationFileName);
        XmlConfigurator.Configure(new FileInfo(configurationPath));
    }
}
