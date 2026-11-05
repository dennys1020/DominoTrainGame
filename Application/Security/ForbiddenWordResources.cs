using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DominoTrainGame.Server.Application.Security;

public static class ForbiddenWordResources
{
    private const string ResourceFolder = ".Security.ForbiddenWords.";
    private const int NotFoundIndex = -1;

    private static readonly char[] LineSeparators = { '\r', '\n' };

    public static List<string> Load()
    {
        Assembly assembly = typeof(ForbiddenWordResources).Assembly;
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

        string[] entries = resourceText.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries);
        return entries;
    }
}
