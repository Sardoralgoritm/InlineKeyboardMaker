namespace InlineKeyboardBot.Configuration;

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = default!;
    public bool EnableDetailedErrors { get; set; } = false;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelay { get; set; } = 30;
    public bool EnableServiceProviderCaching { get; set; } = true;
    public bool EnableQuerySplittingBehavior { get; set; } = false;

    // Migration settings
    public bool AutoMigrate { get; set; } = false;
    public bool ResetDatabaseOnStartup { get; set; } = false;

    // Connection pool settings
    public int MinPoolSize { get; set; } = 1;
    public int MaxPoolSize { get; set; } = 100;
    public int ConnectionLifetime { get; set; } = 0;
    public int ConnectionTimeout { get; set; } = 15;

    // Performance settings
    public bool EnableQueryCaching { get; set; } = true;
    public int QueryCacheSize { get; set; } = 1024;

    public string GetConnectionString()
    {
        if (string.IsNullOrEmpty(ConnectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured");
        }

        return ConnectionString;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new ArgumentException("ConnectionString cannot be null or empty");
        }

        if (CommandTimeout < 0)
        {
            throw new ArgumentException("CommandTimeout must be non-negative");
        }

        if (MaxRetryCount < 0)
        {
            throw new ArgumentException("MaxRetryCount must be non-negative");
        }

        if (MaxRetryDelay < 0)
        {
            throw new ArgumentException("MaxRetryDelay must be non-negative");
        }

        if (MinPoolSize < 1)
        {
            throw new ArgumentException("MinPoolSize must be at least 1");
        }

        if (MaxPoolSize < MinPoolSize)
        {
            throw new ArgumentException("MaxPoolSize must be greater than or equal to MinPoolSize");
        }
    }
}