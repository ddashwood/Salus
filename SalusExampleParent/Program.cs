using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Salus;
using SalusExampleParent;

using IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSalus();
        services.AddSingleton<ExampleParent>();
    })
    .Build();

var app = host.Services.GetRequiredService<ExampleParent>();
app.Run();