using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Salus;
using Salus.Configuration.Retry;
using SalusExampleParent;

using IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSalus<ExampleDbContext>(options =>
        {
            options
                //.SetMessageSender(message => throw new Exception("*** Example Failure ***"))
                .SetMessageSender(message => Console.WriteLine("Sending: " + JsonConvert.SerializeObject(message)))
                .SetRetryQueueProcessInterval(100)
                .SetRetryStrategy(new ExponentialBackoffRetry(500, 1.1, 30000));
        });
        services.AddScoped<ExampleParent>();
        services.AddDbContext<ExampleDbContext>(options =>
        {
            options.UseSqlite("Data Source=Application.db");
        });
        
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