using System;
using log4net;

namespace DominoTrainGame.Server.Application.Guards;

public static class ArgumentGuard
{
    private const string MissingFormat = "A required argument is missing. Parameter: {0}";
    private const string EmptyFormat = "A required text argument is empty. Parameter: {0}";
    private const string EmptyMessage = "The argument cannot be empty";

    public static T NotNull<T>(T argument, string parameterName, ILog logger) where T : class
    {
        ThrowIfFull(argument, parameterName, logger);
        return argument;
    }

    public static void ThrowIfFull(object argument, string parameterName, ILog logger)
    {
        if (argument == null)
        {
            logger.WarnFormat(MissingFormat, parameterName);
            throw new ArgumentNullException(parameterName);
        }
    }

    public static void ThrowIfEmpty(string argument, string parameterName, ILog logger)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            logger.WarnFormat(EmptyFormat, parameterName);
            throw new ArgumentException(EmptyMessage, parameterName);
        }
    }
}
