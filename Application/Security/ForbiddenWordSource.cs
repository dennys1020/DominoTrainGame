using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Security;

public sealed class ForbiddenWordSource : IForbiddenWordSource
{
    private const string ResourceFolder = ".Security.ForbiddenWords.";
    private const int NotFoundIndex = -1;

    private static readonly char[] _lineSeparators;

    static ForbiddenWordSource()
    {
        _lineSeparators = new[] { '\r', '\n' };
    }

    public IReadOnlyList<string> LoadEntries()
    {
        Assembly assembly = typeof(ForbiddenWordSource).Assembly;
        List<string> forbiddenEntries = new List<string>();

        foreach (string resourceName in assembly.GetManifestResourceNames())
        {
            if (resourceName.IndexOf(ResourceFolder, StringComparison.Ordinal) > NotFoundIndex)
            {
                forbiddenEntries.AddRange(ReadEntries(assembly, resourceName));
            }
        }

        return forbiddenEntries;
    }

    private static string[] ReadEntries(Assembly assembly, string resourceName)
    {
        string resourceText;

        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(resourceStream, Encoding.UTF8))
        {
            resourceText = reader.ReadToEnd();
        }

        string[] entries = resourceText.Split(_lineSeparators, StringSplitOptions.RemoveEmptyEntries);
        return entries;
    }
}
