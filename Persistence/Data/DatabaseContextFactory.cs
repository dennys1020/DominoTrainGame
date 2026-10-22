using System;
using System.Configuration;

namespace DominoTrainGame.Server.Persistence.Data;

internal static class DatabaseContextFactory
{
    private const string ActiveProfileKey = "ActiveDatabase";
    private const string LocalProfileName = "Local";
    private const string LocalConnectionName = "DominoGameDBEntities";
    private const string EntityClientProviderName = "System.Data.EntityClient";
    private const string NamedConnectionPrefix = "name=";

    public static DominoGameDBEntities Create()
    {
        string connectionName = ResolveConnectionName();
        ConnectionStringSettings profile = ConfigurationManager.ConnectionStrings[connectionName];

        if (!IsEntityProfile(profile))
        {
            throw new ConfigurationErrorsException(
                "The selected database profile is missing or invalid in Database.config.");
        }

        DominoGameDBEntities context = new DominoGameDBEntities(NamedConnectionPrefix + connectionName);
        return context;
    }

    private static string ResolveConnectionName()
    {
        ConnectionStringSettings selection = ConfigurationManager.ConnectionStrings[ActiveProfileKey];
        string profileName = selection?.ConnectionString?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(profileName))
        {
            throw new ConfigurationErrorsException(
                "Select a database profile using ActiveDatabase in Database.config.");
        }

        bool isLocalProfile = string.Equals(profileName, LocalProfileName, StringComparison.OrdinalIgnoreCase);
        string connectionName = isLocalProfile ? LocalConnectionName : profileName;
        return connectionName;
    }

    private static bool IsEntityProfile(ConnectionStringSettings profile)
    {
        string connectionString = profile?.ConnectionString ?? string.Empty;
        string providerName = profile?.ProviderName ?? string.Empty;
        bool hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
        bool isEntityProfile = hasConnectionString && string.Equals(
            providerName,
            EntityClientProviderName,
            StringComparison.OrdinalIgnoreCase);
        return isEntityProfile;
    }
}
