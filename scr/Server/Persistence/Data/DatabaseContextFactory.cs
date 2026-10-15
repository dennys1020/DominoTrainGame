using System;
using System.Configuration;
using System.Data.Entity.Core;

namespace DominoTrainGame;

public static class DatabaseContextFactory
{
    public static DominoGameDBEntities Create()
    {
        try
        {
            ConnectionStringSettings selection =
                ConfigurationManager.ConnectionStrings["ActiveDatabase"];
            string profileName = selection?.ConnectionString.Trim();

            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ConfigurationErrorsException(
                    "Select a database profile using ActiveDatabase in Database.config.");
            }

            string connectionName = string.Equals(profileName, "Local", StringComparison.OrdinalIgnoreCase)
                ? "DominoGameDBEntities"
                : profileName;
            ConnectionStringSettings profile = ConfigurationManager.ConnectionStrings[connectionName];

            if (profile is null || string.IsNullOrWhiteSpace(profile.ConnectionString)
                || !string.Equals(profile.ProviderName, "System.Data.EntityClient",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfigurationErrorsException(
                    "The selected database profile is missing or invalid in Database.config.");
            }

            return new DominoGameDBEntities("name=" + connectionName);
        }
        catch (ConfigurationErrorsException exception)
        {
            throw new EntityException("The database configuration could not be loaded.", exception);
        }
    }
}
