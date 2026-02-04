using Microsoft.Extensions.Logging;
using shot_reminder_2.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Indexes;

public sealed class MongoIndexHostedService : IHostedService
{
    private readonly IMongoIndexInitializer _initializer;
    private readonly ILogger<MongoIndexHostedService> _logger;

    public MongoIndexHostedService(IMongoIndexInitializer initializer, ILogger<MongoIndexHostedService> logger)
    {
        _initializer = initializer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing Mongo indexes...");
        await _initializer.EnsureIndexesAsync(cancellationToken);
        _logger.LogInformation("Mongo indexes initialized.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
