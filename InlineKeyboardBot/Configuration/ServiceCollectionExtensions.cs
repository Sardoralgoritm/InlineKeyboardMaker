using InlineKeyboardBot.Data;
using InlineKeyboardBot.Data.Repositories;
using InlineKeyboardBot.Data.Repositories.Interfaces;
using InlineKeyboardBot.Handlers;
using InlineKeyboardBot.Handlers.Interfaces;
using InlineKeyboardBot.Services;
using InlineKeyboardBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Telegram.Bot;

namespace InlineKeyboardBot.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Database Health Check
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Simple database connectivity check
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy("Cannot connect to database");
                }

                // Optional: Check if we can query data
                var userCount = await _context.Users.CountAsync(cancellationToken);
                var channelCount = await _context.Channels.CountAsync(cancellationToken);

                var data = new Dictionary<string, object>
            {
                { "users_count", userCount },
                { "channels_count", channelCount },
                { "database_provider", _context.Database.ProviderName ?? "Unknown" }
            };

                return HealthCheckResult.Healthy("Database is healthy", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
            }
        }
    }

    /// <summary>
    /// Bot konfiguratsiyasini qo'shish
    /// </summary>
    public static IServiceCollection AddBotConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Bot Configuration
        var botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
        if (botConfig == null)
        {
            throw new InvalidOperationException("BotConfiguration section is missing in appsettings.json");
        }

        services.Configure<BotConfiguration>(configuration.GetSection("BotConfiguration"));
        services.AddSingleton(botConfig);

        // Database Configuration
        var dbConfig = configuration.GetSection("DatabaseConfiguration").Get<DatabaseConfiguration>();
        if (dbConfig == null)
        {
            dbConfig = new DatabaseConfiguration
            {
                ConnectionString = configuration.GetConnectionString("DefaultConnection") ??
                                  throw new InvalidOperationException("Database connection string is not configured")
            };
        }

        dbConfig.Validate();
        services.Configure<DatabaseConfiguration>(configuration.GetSection("DatabaseConfiguration"));
        services.AddSingleton(dbConfig);

        return services;
    }

    /// <summary>
    /// Database konfiguratsiyasini qo'shish
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, DatabaseConfiguration dbConfig)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(dbConfig.GetConnectionString(), npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: dbConfig.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(dbConfig.MaxRetryDelay),
                    errorCodesToAdd: null);
            });

            // Development settings
            if (dbConfig.EnableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            if (dbConfig.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            // Performance settings
            if (dbConfig.EnableServiceProviderCaching)
            {
                options.EnableServiceProviderCaching();
            }
        });

        return services;
    }

    /// <summary>
    /// Telegram Bot Client ni qo'shish
    /// </summary>
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, BotConfiguration botConfig)
    {
        if (string.IsNullOrWhiteSpace(botConfig.BotToken))
        {
            throw new ArgumentException("Bot token is required");
        }

        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                return new TelegramBotClient(botConfig.BotToken, httpClient);
            });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();

        return services;
    }

    public static IServiceCollection AddBotServices(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<IBotService, BotService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IChannelService, ChannelService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IUserSessionService, UserSessionService>();

        // Additional services (keyinroq qo'shamiz)
        // services.AddScoped<IPostService, PostService>();
        // services.AddScoped<IStatisticsService, StatisticsService>();

        return services;
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<IUpdateHandler, UpdateDistributor>();
        services.AddScoped<CommandHandler>();
        services.AddScoped<CallbackHandler>();
        services.AddScoped<MessageHandler>();

        return services;
    }

    public static IServiceCollection AddBotLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();

            // File logging (agar kerak bo'lsa)
            // builder.AddFile("Logs/bot-{Date}.txt");
        });

        return services;
    }

    public static IServiceCollection AddBotHealthChecks(this IServiceCollection services, DatabaseConfiguration dbConfig, BotConfiguration botConfig)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<TelegramBotHealthCheck>("telegram_bot");

        return services;
    }

    public static IServiceCollection AddInlineKeyboardBot(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.AddBotConfiguration(configuration);

        // Get configurations
        var botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>()!;
        var dbConfig = configuration.GetSection("DatabaseConfiguration").Get<DatabaseConfiguration>()
                      ?? new DatabaseConfiguration
                      {
                          ConnectionString = configuration.GetConnectionString("DefaultConnection")!
                      };

        // Add services
        services.AddDatabase(dbConfig);
        services.AddTelegramBot(botConfig);
        services.AddRepositories();
        services.AddBotServices();
        services.AddHandlers();
        services.AddBotLogging(configuration);
        services.AddBotHealthChecks(dbConfig, botConfig);

        // Controllers
        services.AddControllers();

        return services;
    }
}

public class TelegramBotHealthCheck : IHealthCheck
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotHealthCheck> _logger;

    public TelegramBotHealthCheck(ITelegramBotClient botClient, ILogger<TelegramBotHealthCheck> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var me = await _botClient.GetMeAsync(cancellationToken);

            if (me == null)
            {
                return HealthCheckResult.Unhealthy("Bot information is null");
            }

            var data = new Dictionary<string, object>
            {
                { "bot_id", me.Id },
                { "bot_username", me.Username ?? "N/A" },
                { "bot_name", me.FirstName },
                { "can_join_groups", me.CanJoinGroups },
                { "can_read_all_group_messages", me.CanReadAllGroupMessages },
                { "supports_inline_queries", me.SupportsInlineQueries }
            };

            return HealthCheckResult.Healthy("Telegram Bot is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telegram Bot health check failed");
            return HealthCheckResult.Unhealthy("Telegram Bot is unhealthy", ex);
        }
    }
}

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseBotMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        // Health checks
        app.UseHealthChecks("/health");

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health/ready");
        });

        return app;
    }

    public static async Task<IApplicationBuilder> InitializeBotAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            // Database migration
            var context = services.GetRequiredService<ApplicationDbContext>();
            var dbConfig = services.GetRequiredService<DatabaseConfiguration>();

            if (dbConfig.AutoMigrate)
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed");
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured created");
            }

            // Webhook setup
            var botService = services.GetRequiredService<IBotService>();
            await botService.SetWebhookAsync(CancellationToken.None);

            logger.LogInformation("Bot initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bot initialization failed");
            throw;
        }

        return app;
    }
}