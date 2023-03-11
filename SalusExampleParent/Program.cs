using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Salus;
using Salus.Configuration.Retry;
using SalusExampleParent.Messaging;

namespace SalusExampleParent;

internal static class Program
{
    [STAThread]
    static async Task Main()
    {

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

                services.AddScoped<MainForm>();
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            // Ensure the database is created
            var context = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
            context.Database.OpenConnection();
            context.Database.Migrate();

            // IMPORTANT - this starts the queue processor. If you miss out this line,
            // failed messages will not get re-sent!
            await host.StartAsync();

            // Now start the application
            ApplicationConfiguration.Initialize();
            Application.Run(scope.ServiceProvider.GetRequiredService<MainForm>());

            // Clean up gracefully
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.StopApplication();
            await host.WaitForShutdownAsync();
        }
    }
}