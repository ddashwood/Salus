using Microsoft.Extensions.Hosting;
using SalusExampleChild;
using Salus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSalus<ChildContext>(
            salusOptions => { },
            contextOptions =>
            {
                contextOptions.UseSqlite("Data Source=Child.db");
            }
        );

        services.AddScoped<ExampleChild>();
    })
    .Build();


using (var scope = host.Services.CreateScope())
{
    // Ensure the database is created
    var context = scope.ServiceProvider.GetRequiredService<ChildContext>();
    context.Database.OpenConnection();
    context.Database.Migrate();

    // Now start the application
    var program = scope.ServiceProvider.GetRequiredService<ExampleChild>();
    await program.Run();
}
