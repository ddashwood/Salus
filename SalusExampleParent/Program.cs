using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Salus;
using Salus.Configuration.Retry;
using Salus.Messaging;
using SalusExampleParent;
using SalusExampleParent.Messaging;

using IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSalus<ExampleDbContext>(new RabbitMqMessageSender(), salusOptions =>
        //services.AddSalus<ExampleDbContext>(new ConsoleMessageSender(), salusOptions =>
        //services.AddSalus<ExampleDbContext>(new ExceptionMessageSender(), salusOptions =>
        {
            salusOptions
                .SetRetryQueueProcessInterval(100)
                .SetRetryStrategy(new ExponentialBackoffRetry(500, 1.1, 30000))
                .SetPurgeInterval(100)
                .SetPurgeSeconds(10);
        },
        contextOptions =>
        {
            contextOptions.UseSqlite("Data Source=Application.db");
        });

        services.AddScoped<ExampleParent>();        
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    // Ensure the database is created
    var context = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
    context.Database.EnsureDeleted();
    context.Database.OpenConnection();
    context.Database.Migrate();

    // IMPORTANT - this starts the queue processor. If you miss out this line,
    // failed messages will not get re-sent!
    await host.StartAsync();

    // Now start the application
    var app = scope.ServiceProvider.GetRequiredService<ExampleParent>();
    await app.Run();

    // Clean up gracefully
    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.StopApplication();
    await host.WaitForShutdownAsync();
}