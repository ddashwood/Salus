using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Salus;
using SalusExampleParent;

using IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSalus();
        services.AddSingleton<ExampleParent>();
        services.AddDbContext<ExampleDbContext>(options =>
        {
            options.UseSqlite("Filename=:memory:");
        });
    })
    .Build();

using var scope = host.Services.CreateScope();
var app = scope.ServiceProvider.GetRequiredService<ExampleParent>();
app.Run();